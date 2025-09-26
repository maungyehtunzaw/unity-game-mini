using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using MiniGameHub.Contracts;

namespace MiniGameHub.Games
{
    /// <summary>
    /// Complete Tic-Tac-Toe mini-game implementation
    /// </summary>
    public class TicTacToeGame : MonoBehaviour, IMinigame
    {
        [Header("Game Board")]
        [SerializeField] private Button[] boardButtons = new Button[9];
        [SerializeField] private TextMeshProUGUI[] boardTexts = new TextMeshProUGUI[9];
        
        [Header("Game UI")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI gameOverText;
        [SerializeField] private Button playAgainButton;
        
        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem winEffect;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip moveSound;
        [SerializeField] private AudioClip winSound;
        [SerializeField] private AudioClip loseSound;
        [SerializeField] private AudioClip drawSound;
        
        [Header("Game Settings")]
        [SerializeField] private bool playerStartsFirst = true;
        [SerializeField] private float aiMoveDelay = 1f;
        [SerializeField] private Color playerColor = Color.blue;
        [SerializeField] private Color aiColor = Color.red;
        [SerializeField] private Color defaultColor = Color.white;

        // Game state
        private GameState gameState;
        private Player currentPlayer;
        private int[] board = new int[9]; // 0 = empty, 1 = player, 2 = AI
        private bool gameActive;
        private int playerScore;
        private int aiScore;
        private int drawCount;
        private IMinigameContext context;
        private float gameStartTime;
        private int totalMoves;

        // Constants
        private const int EMPTY = 0;
        private const int PLAYER = 1;
        private const int AI = 2;

        public enum Player
        {
            Human,
            AI
        }

        public enum GameState
        {
            Playing,
            PlayerWon,
            AIWon,
            Draw
        }

        public void Initialize(IMinigameContext gameContext)
        {
            context = gameContext;
            SetupGame();
            StartNewGame();
        }

        private void SetupGame()
        {
            // Setup board buttons
            for (int i = 0; i < boardButtons.Length; i++)
            {
                int index = i; // Capture for closure
                boardButtons[i].onClick.RemoveAllListeners();
                boardButtons[i].onClick.AddListener(() => OnCellClicked(index));
            }
            
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
            
            UpdateScoreDisplay();
        }

        public void StartNewGame()
        {
            // Reset game state
            gameState = GameState.Playing;
            gameActive = true;
            totalMoves = 0;
            gameStartTime = Time.time;
            
            // Clear board
            for (int i = 0; i < board.Length; i++)
            {
                board[i] = EMPTY;
                if (boardTexts[i] != null)
                {
                    boardTexts[i].text = "";
                    boardTexts[i].color = defaultColor;
                }
                if (boardButtons[i] != null)
                {
                    boardButtons[i].interactable = true;
                }
            }
            
            // Set starting player
            currentPlayer = playerStartsFirst ? Player.Human : Player.AI;
            
            // Hide game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            UpdateStatusText();
            
            // If AI starts, make AI move
            if (currentPlayer == Player.AI)
            {
                StartCoroutine(AIMove());
            }
        }

        private void OnCellClicked(int index)
        {
            if (!gameActive || currentPlayer != Player.Human || board[index] != EMPTY)
                return;
            
            MakeMove(index, PLAYER);
            
            if (gameActive && currentPlayer == Player.AI)
            {
                StartCoroutine(AIMove());
            }
        }

        private void MakeMove(int index, int player)
        {
            board[index] = player;
            totalMoves++;
            
            // Update visual
            if (boardTexts[index] != null)
            {
                boardTexts[index].text = player == PLAYER ? "X" : "O";
                boardTexts[index].color = player == PLAYER ? playerColor : aiColor;
            }
            
            // Play sound
            PlaySound(moveSound);
            
            // Check for win or draw
            CheckGameEnd();
            
            if (gameActive)
            {
                // Switch player
                currentPlayer = currentPlayer == Player.Human ? Player.AI : Player.Human;
                UpdateStatusText();
            }
        }

        private IEnumerator AIMove()
        {
            yield return new WaitForSeconds(aiMoveDelay);
            
            if (!gameActive) yield break;
            
            int bestMove = GetBestMove();
            if (bestMove != -1)
            {
                MakeMove(bestMove, AI);
            }
        }

        private int GetBestMove()
        {
            // Use minimax algorithm for AI
            int bestScore = int.MinValue;
            int bestMove = -1;
            
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == EMPTY)
                {
                    board[i] = AI;
                    int score = Minimax(board, 0, false);
                    board[i] = EMPTY;
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = i;
                    }
                }
            }
            
            return bestMove;
        }

        private int Minimax(int[] board, int depth, bool isMaximizing)
        {
            int result = CheckWinner();
            
            if (result == AI) return 10 - depth;
            if (result == PLAYER) return depth - 10;
            if (IsBoardFull()) return 0;
            
            if (isMaximizing)
            {
                int bestScore = int.MinValue;
                for (int i = 0; i < board.Length; i++)
                {
                    if (board[i] == EMPTY)
                    {
                        board[i] = AI;
                        int score = Minimax(board, depth + 1, false);
                        board[i] = EMPTY;
                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
                return bestScore;
            }
            else
            {
                int bestScore = int.MaxValue;
                for (int i = 0; i < board.Length; i++)
                {
                    if (board[i] == EMPTY)
                    {
                        board[i] = PLAYER;
                        int score = Minimax(board, depth + 1, true);
                        board[i] = EMPTY;
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
                return bestScore;
            }
        }

        private void CheckGameEnd()
        {
            int winner = CheckWinner();
            
            if (winner == PLAYER)
            {
                gameState = GameState.PlayerWon;
                playerScore++;
                EndGame("You Win!", winSound);
            }
            else if (winner == AI)
            {
                gameState = GameState.AIWon;
                aiScore++;
                EndGame("AI Wins!", loseSound);
            }
            else if (IsBoardFull())
            {
                gameState = GameState.Draw;
                drawCount++;
                EndGame("It's a Draw!", drawSound);
            }
        }

        private int CheckWinner()
        {
            // Check rows
            for (int row = 0; row < 3; row++)
            {
                if (board[row * 3] != EMPTY && 
                    board[row * 3] == board[row * 3 + 1] && 
                    board[row * 3 + 1] == board[row * 3 + 2])
                {
                    return board[row * 3];
                }
            }
            
            // Check columns
            for (int col = 0; col < 3; col++)
            {
                if (board[col] != EMPTY && 
                    board[col] == board[col + 3] && 
                    board[col + 3] == board[col + 6])
                {
                    return board[col];
                }
            }
            
            // Check diagonals
            if (board[0] != EMPTY && board[0] == board[4] && board[4] == board[8])
            {
                return board[0];
            }
            
            if (board[2] != EMPTY && board[2] == board[4] && board[4] == board[6])
            {
                return board[2];
            }
            
            return EMPTY;
        }

        private bool IsBoardFull()
        {
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] == EMPTY) return false;
            }
            return true;
        }

        private void EndGame(string message, AudioClip sound)
        {
            gameActive = false;
            
            // Disable all buttons
            for (int i = 0; i < boardButtons.Length; i++)
            {
                boardButtons[i].interactable = false;
            }
            
            // Show game over panel
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            
            if (gameOverText != null)
            {
                gameOverText.text = message;
            }
            
            // Play sound and effects
            PlaySound(sound);
            if (gameState == GameState.PlayerWon && winEffect != null)
            {
                winEffect.Play();
            }
            
            UpdateStatusText();
            UpdateScoreDisplay();
            
            // Report result to context
            ReportGameResult();
        }

        private void ReportGameResult()
        {
            if (context == null) return;
            
            float gameTime = Time.time - gameStartTime;
            int baseScore = 100;
            int timeBonus = Mathf.Max(0, 300 - Mathf.RoundToInt(gameTime * 10)); // Bonus for quick games
            int moveBonus = Mathf.Max(0, (9 - totalMoves) * 20); // Bonus for fewer moves
            
            var result = new GameResult
            {
                completed = true,
                success = gameState == GameState.PlayerWon,
                score = gameState switch
                {
                    GameState.PlayerWon => baseScore + timeBonus + moveBonus,
                    GameState.Draw => 50,
                    GameState.AIWon => 25,
                    _ => 0
                },
                timeElapsed = gameTime,
                gameSpecificData = new Dictionary<string, object>
                {
                    ["moves"] = totalMoves,
                    ["gameState"] = gameState.ToString(),
                    ["playerScore"] = playerScore,
                    ["aiScore"] = aiScore,
                    ["draws"] = drawCount
                }
            };
            
            context.ReportResult(result);
        }

        private void UpdateStatusText()
        {
            if (statusText == null) return;
            
            if (!gameActive)
            {
                statusText.text = gameState switch
                {
                    GameState.PlayerWon => "You Won!",
                    GameState.AIWon => "AI Won!",
                    GameState.Draw => "Draw!",
                    _ => "Game Over"
                };
            }
            else
            {
                statusText.text = currentPlayer == Player.Human ? "Your Turn" : "AI Turn";
            }
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = $"You: {playerScore} | AI: {aiScore} | Draws: {drawCount}";
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
                        ["moves"] = totalMoves
                    }
                };
                
                context.ReportResult(result);
            }
        }

        public void OnDestroy()
        {
            // Clean up any coroutines
            StopAllCoroutines();
        }
    }
}