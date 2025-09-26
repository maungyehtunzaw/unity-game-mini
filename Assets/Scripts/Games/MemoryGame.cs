using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniGameHub.Contracts;

namespace MiniGameHub.Games
{
    /// <summary>
    /// Memory Game - Match pairs of cards
    /// </summary>
    public class MemoryGame : MonoBehaviour, IMinigame
    {
        [Header("Game Board")]
        [SerializeField] private Transform cardGrid;
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private int gridSize = 4; // 4x4 grid = 16 cards (8 pairs)
        
        [Header("Game UI")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI matchesText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private Button playAgainButton;
        
        [Header("Difficulty Settings")]
        [SerializeField] private float cardFlipSpeed = 0.3f;
        [SerializeField] private float revealTime = 1.5f;
        [SerializeField] private int maxMistakes = 10;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem matchEffect;
        [SerializeField] private ParticleSystem gameCompleteEffect;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip cardFlipSound;
        [SerializeField] private AudioClip matchSound;
        [SerializeField] private AudioClip mismatchSound;
        [SerializeField] private AudioClip gameWinSound;
        
        [Header("Card Symbols")]
        [SerializeField] private Sprite[] cardSymbols;
        [SerializeField] private Sprite cardBackSprite;
        [SerializeField] private Color[] cardColors;

        // Game state
        private List<MemoryCard> cards = new List<MemoryCard>();
        private List<MemoryCard> selectedCards = new List<MemoryCard>();
        private int matchesFound;
        private int totalPairs;
        private int moves;
        private int mistakes;
        private float gameStartTime;
        private bool gameActive;
        private bool processingCards;
        private IMinigameContext context;

        // Game stats
        private int currentScore;
        private float gameTime;

        public void Initialize(IMinigameContext gameContext)
        {
            context = gameContext;
            SetupGame();
            StartNewGame();
        }

        private void SetupGame()
        {
            // Setup UI buttons
            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(StartNewGame);
            }
            
            if (exitButton != null)
            {
                exitButton.onClick.RemoveAllListeners();
                exitButton.onClick.AddListener(ExitGame);
            }
            
            if (playAgainButton != null)
            {
                playAgainButton.onClick.RemoveAllListeners();
                playAgainButton.onClick.AddListener(StartNewGame);
            }
            
            // Hide game over panel initially
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            // Calculate total pairs needed
            totalPairs = (gridSize * gridSize) / 2;
        }

        public void StartNewGame()
        {
            // Reset game state
            gameActive = true;
            processingCards = false;
            matchesFound = 0;
            moves = 0;
            mistakes = 0;
            currentScore = 0;
            gameStartTime = Time.time;
            selectedCards.Clear();
            
            // Hide game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            // Create the card grid
            CreateCardGrid();
            
            // Update UI
            UpdateUI();
            
            // Show all cards briefly at start
            StartCoroutine(ShowAllCardsAtStart());
        }

        private void CreateCardGrid()
        {
            // Clear existing cards
            foreach (MemoryCard card in cards)
            {
                if (card != null && card.gameObject != null)
                    DestroyImmediate(card.gameObject);
            }
            cards.Clear();
            
            // Create symbol pairs
            List<int> symbolIndices = new List<int>();
            for (int i = 0; i < totalPairs; i++)
            {
                symbolIndices.Add(i % cardSymbols.Length); // Use available symbols
                symbolIndices.Add(i % cardSymbols.Length); // Add pair
            }
            
            // Shuffle the symbols
            ShuffleList(symbolIndices);
            
            // Create cards
            for (int i = 0; i < gridSize * gridSize; i++)
            {
                GameObject cardObj = Instantiate(cardPrefab, cardGrid);
                MemoryCard card = cardObj.GetComponent<MemoryCard>();
                
                if (card == null)
                {
                    card = cardObj.AddComponent<MemoryCard>();
                }
                
                // Setup card
                int symbolIndex = symbolIndices[i];
                card.Initialize(i, symbolIndex, cardSymbols[symbolIndex], cardBackSprite, cardColors[symbolIndex % cardColors.Length]);
                card.OnCardClicked += OnCardClicked;
                
                cards.Add(card);
            }
            
            // Setup grid layout
            GridLayoutGroup gridLayout = cardGrid.GetComponent<GridLayoutGroup>();
            if (gridLayout != null)
            {
                gridLayout.constraintCount = gridSize;
            }
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                T temp = list[i];
                int randomIndex = Random.Range(i, list.Count);
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }

        private IEnumerator ShowAllCardsAtStart()
        {
            // Flip all cards to show symbols
            foreach (MemoryCard card in cards)
            {
                card.FlipToFront(false);
            }
            
            yield return new WaitForSeconds(2f);
            
            // Flip all cards back to hidden
            foreach (MemoryCard card in cards)
            {
                card.FlipToBack(false);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // Enable interactions
            foreach (MemoryCard card in cards)
            {
                card.SetInteractable(true);
            }
        }

        private void OnCardClicked(MemoryCard card)
        {
            if (!gameActive || processingCards || selectedCards.Contains(card) || card.IsMatched)
                return;
            
            // Flip card
            card.FlipToFront(true);
            selectedCards.Add(card);
            PlaySound(cardFlipSound);
            
            // Check if we have two cards selected
            if (selectedCards.Count == 2)
            {
                moves++;
                processingCards = true;
                StartCoroutine(CheckMatch());
            }
            
            UpdateUI();
        }

        private IEnumerator CheckMatch()
        {
            yield return new WaitForSeconds(revealTime);
            
            MemoryCard card1 = selectedCards[0];
            MemoryCard card2 = selectedCards[1];
            
            if (card1.SymbolIndex == card2.SymbolIndex)
            {
                // Match found!
                card1.SetMatched(true);
                card2.SetMatched(true);
                matchesFound++;
                
                PlaySound(matchSound);
                
                // Show match effect
                if (matchEffect != null)
                {
                    matchEffect.transform.position = (card1.transform.position + card2.transform.position) / 2;
                    matchEffect.Play();
                }
                
                // Check if game is complete
                if (matchesFound >= totalPairs)
                {
                    EndGame(true);
                }
            }
            else
            {
                // No match - flip cards back
                card1.FlipToBack(true);
                card2.FlipToBack(true);
                mistakes++;
                
                PlaySound(mismatchSound);
                
                // Check if too many mistakes
                if (mistakes >= maxMistakes)
                {
                    EndGame(false);
                }
            }
            
            selectedCards.Clear();
            processingCards = false;
            UpdateUI();
        }

        private void EndGame(bool success)
        {
            gameActive = false;
            gameTime = Time.time - gameStartTime;
            
            // Calculate final score
            CalculateFinalScore(success);
            
            // Show game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            
            if (gameOverText != null)
            {
                if (success)
                {
                    gameOverText.text = "Congratulations!\nAll pairs matched!";
                    PlaySound(gameWinSound);
                    
                    if (gameCompleteEffect != null)
                    {
                        gameCompleteEffect.Play();
                    }
                }
                else
                {
                    gameOverText.text = "Game Over!\nToo many mistakes.";
                }
            }
            
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {currentScore}";
            }
            
            // Disable all card interactions
            foreach (MemoryCard card in cards)
            {
                card.SetInteractable(false);
            }
            
            // Report result to context
            ReportGameResult(success);
        }

        private void CalculateFinalScore(bool success)
        {
            if (!success)
            {
                currentScore = 0;
                return;
            }
            
            int baseScore = 1000;
            int timeBonus = Mathf.Max(0, 500 - Mathf.RoundToInt(gameTime * 5));
            int movesBonus = Mathf.Max(0, (totalPairs * 3 - moves) * 50);
            int mistakesPenalty = mistakes * 100;
            
            currentScore = Mathf.Max(100, baseScore + timeBonus + movesBonus - mistakesPenalty);
        }

        private void ReportGameResult(bool success)
        {
            if (context == null) return;
            
            var result = new GameResult
            {
                completed = success,
                success = success,
                score = currentScore,
                timeElapsed = gameTime,
                gameSpecificData = new Dictionary<string, object>
                {
                    ["matches"] = matchesFound,
                    ["totalPairs"] = totalPairs,
                    ["moves"] = moves,
                    ["mistakes"] = mistakes,
                    ["difficulty"] = $"{gridSize}x{gridSize}",
                    ["efficiency"] = moves > 0 ? (float)matchesFound / moves : 0f
                }
            };
            
            context.ReportResult(result);
        }

        private void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore}";
            }
            
            if (movesText != null)
            {
                movesText.text = $"Moves: {moves}";
            }
            
            if (matchesText != null)
            {
                matchesText.text = $"Matches: {matchesFound}/{totalPairs}";
            }
            
            if (timerText != null && gameActive)
            {
                float currentTime = Time.time - gameStartTime;
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                timerText.text = $"Time: {minutes:00}:{seconds:00}";
            }
        }

        private void Update()
        {
            if (gameActive)
            {
                UpdateUI();
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void ExitGame()
        {
            if (context != null)
            {
                var result = new GameResult
                {
                    completed = false,
                    success = false,
                    score = 0,
                    timeElapsed = Time.time - gameStartTime,
                    gameSpecificData = new Dictionary<string, object>
                    {
                        ["exitedEarly"] = true,
                        ["matchesFound"] = matchesFound,
                        ["moves"] = moves
                    }
                };
                
                context.ReportResult(result);
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        #region Public API
        /// <summary>
        /// Set the grid size for difficulty adjustment
        /// </summary>
        public void SetGridSize(int size)
        {
            gridSize = Mathf.Clamp(size, 2, 6);
            totalPairs = (gridSize * gridSize) / 2;
        }

        /// <summary>
        /// Set the maximum number of mistakes allowed
        /// </summary>
        public void SetMaxMistakes(int max)
        {
            maxMistakes = Mathf.Max(1, max);
        }

        /// <summary>
        /// Get current game statistics
        /// </summary>
        public Dictionary<string, object> GetGameStats()
        {
            return new Dictionary<string, object>
            {
                ["matches"] = matchesFound,
                ["moves"] = moves,
                ["mistakes"] = mistakes,
                ["timeElapsed"] = Time.time - gameStartTime,
                ["gameActive"] = gameActive
            };
        }
        #endregion
    }
}