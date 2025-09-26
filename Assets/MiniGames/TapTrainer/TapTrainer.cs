using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Tap Trainer - A timing-based mini-game where players tap when a moving marker hits the target
/// This serves as a base template that can be varied in thousands of ways
/// </summary>
public class TapTrainer : MonoBehaviour, IMinigame, IConfigurableMinigame
{
    [Header("Game Objects")]
    public Text scoreText;
    public Text instructionsText;
    public Image needle;
    public RectTransform targetArc;
    public Button tapButton;
    public ParticleSystem hitEffect;
    public ParticleSystem missEffect;
    
    [Header("Game Settings")]
    public float baseSpeed = 180f; // degrees per second
    public float allowedWindow = 20f; // +/- degrees for success
    public int totalRounds = 10;
    public bool randomizeDirection = true;
    
    [Header("Difficulty Scaling")]
    public float speedMultiplier = 1f;
    public float windowMultiplier = 1f;
    public int bonusRounds = 0;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip missSound;
    public AudioClip gameOverSound;

    // Private game state
    private IMinigameContext _context;
    private int _score = 0;
    private int _currentRound = 0;
    private float _angle = 0f;
    private bool _running = false;
    private bool _clockwise = true;
    private float _gameStartTime;
    private int _perfectHits = 0;
    private int _goodHits = 0;
    private int _totalTaps = 0;
    
    // Configurable properties
    private GameDifficulty _difficulty = GameDifficulty.Medium;
    private GameMode _gameMode = GameMode.Normal;
    private GameSettings _settings;

    public void Init(IMinigameContext ctx)
    {
        _context = ctx;
        _settings = ctx.GetPlayerSettings();
        
        SetupAudio();
        SetupUI();
        
        Debug.Log("[TapTrainer] Initialized");
    }

    public void StartGame()
    {
        _running = true;
        _score = 0;
        _currentRound = 0;
        _totalTaps = 0;
        _perfectHits = 0;
        _goodHits = 0;
        _gameStartTime = Time.time;
        
        // Apply difficulty settings
        ApplyDifficultySettings();
        
        // Setup UI
        if (instructionsText != null)
            instructionsText.text = "Tap when the needle hits the target!";
        
        if (tapButton != null)
            tapButton.onClick.AddListener(HandleTap);
        
        UpdateUI();
        NextRound();
        
        Debug.Log($"[TapTrainer] Game started - Difficulty: {_difficulty}, Mode: {_gameMode}");
    }

    public void StopGame()
    {
        _running = false;
        
        if (tapButton != null)
            tapButton.onClick.RemoveAllListeners();
        
        Debug.Log("[TapTrainer] Game stopped");
    }

    public void CleanupGame()
    {
        StopGame();
        
        // Clean up any spawned objects, stop coroutines, etc.
        StopAllCoroutines();
        
        Debug.Log("[TapTrainer] Game cleaned up");
    }

    public string GetGameState()
    {
        var state = new TapTrainerState
        {
            score = _score,
            currentRound = _currentRound,
            totalRounds = totalRounds,
            difficulty = _difficulty,
            gameMode = _gameMode
        };
        
        return JsonUtility.ToJson(state);
    }

    public void LoadGameState(string state)
    {
        try
        {
            var gameState = JsonUtility.FromJson<TapTrainerState>(state);
            _score = gameState.score;
            _currentRound = gameState.currentRound;
            totalRounds = gameState.totalRounds;
            _difficulty = gameState.difficulty;
            _gameMode = gameState.gameMode;
            
            UpdateUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TapTrainer] Failed to load game state: {e.Message}");
        }
    }

    public GameInfo GetGameInfo()
    {
        return new GameInfo("tap_trainer", "Tap Trainer", GameCategory.Reflex)
        {
            description = "Test your timing by tapping when the needle hits the target!",
            difficulty = _difficulty,
            estimatedPlayTime = 2f,
            tags = new string[] { "timing", "reflex", "casual" }
        };
    }

    // IConfigurableMinigame implementation
    public void SetDifficulty(GameDifficulty difficulty)
    {
        _difficulty = difficulty;
        if (_running)
            ApplyDifficultySettings();
    }

    public void SetGameMode(GameMode mode)
    {
        _gameMode = mode;
        ApplyGameModeSettings();
    }

    public void ApplyCustomSettings(GameSettings settings)
    {
        _settings = settings;
        SetupAudio();
    }

    void Update()
    {
        if (!_running) return;
        
        // Update needle rotation
        float currentSpeed = baseSpeed * speedMultiplier * (_clockwise ? 1f : -1f);
        _angle = (_angle + currentSpeed * Time.deltaTime) % 360f;
        
        if (needle != null)
            needle.rectTransform.localEulerAngles = new Vector3(0, 0, -_angle);
        
        // Handle continuous tap mode
        if (_gameMode == GameMode.Endless && Input.GetMouseButtonDown(0))
        {
            HandleTap();
        }
    }

    void HandleTap()
    {
        if (!_running) return;
        
        _totalTaps++;
        
        float targetAngle = targetArc != null ? targetArc.localEulerAngles.z : 0f;
        float delta = Mathf.DeltaAngle(_angle, targetAngle);
        float absError = Mathf.Abs(delta);
        
        bool hit = absError <= (allowedWindow * windowMultiplier);
        
        if (hit)
        {
            int points = CalculatePoints(absError);
            _score += points;
            
            // Track hit quality
            if (absError <= (allowedWindow * 0.3f))
                _perfectHits++;
            else if (absError <= (allowedWindow * 0.7f))
                _goodHits++;
            
            // Visual feedback
            PlayHitEffect(true);
            PlaySound(hitSound);
            
            // Haptic feedback
            _context?.PlayHaptic(HapticType.Light);
            
            Debug.Log($"[TapTrainer] HIT! Error: {absError:F1}°, Points: {points}");
        }
        else
        {
            // Miss penalty
            _score = Mathf.Max(0, _score - 5);
            
            PlayHitEffect(false);
            PlaySound(missSound);
            
            _context?.PlayHaptic(HapticType.Warning);
            
            Debug.Log($"[TapTrainer] MISS! Error: {absError:F1}°");
        }
        
        UpdateUI();
        
        // Progress to next round
        if (_gameMode != GameMode.Endless)
        {
            _currentRound++;
            if (_currentRound >= totalRounds)
            {
                FinishGame();
            }
            else
            {
                NextRound();
            }
        }
        
        // Update progress
        float progress = _gameMode == GameMode.Endless ? 
            Mathf.Min(1f, _totalTaps / 50f) : 
            (float)_currentRound / totalRounds;
        _context?.ReportProgress(progress);
    }

    int CalculatePoints(float error)
    {
        float errorRatio = error / (allowedWindow * windowMultiplier);
        
        if (errorRatio <= 0.3f) return 100; // Perfect
        if (errorRatio <= 0.7f) return 50;  // Good
        return 25; // Just hit
    }

    void NextRound()
    {
        // Randomize direction
        if (randomizeDirection)
            _clockwise = Random.value > 0.5f;
        
        // Increase difficulty over time
        if (_gameMode == GameMode.Challenge)
        {
            speedMultiplier = 1f + (_currentRound * 0.1f);
            windowMultiplier = Mathf.Max(0.5f, 1f - (_currentRound * 0.05f));
        }
        
        // Reset angle randomly
        _angle = Random.Range(0f, 360f);
    }

    void FinishGame()
    {
        _running = false;
        
        float duration = Time.time - _gameStartTime;
        
        // Calculate final score bonuses
        int finalScore = _score;
        
        // Perfect hit bonus
        if (_perfectHits > totalRounds * 0.8f)
            finalScore += 500; // Perfect game bonus
        
        // Speed bonus for quick completion
        if (duration < totalRounds * 2f)
            finalScore += Mathf.RoundToInt(100f * (totalRounds * 2f - duration));
        
        // Create result
        var result = new GameResult("tap_trainer", finalScore, 0, duration, true)
        {
            difficulty = _difficulty
        };
        
        // Calculate stars based on performance
        float accuracy = (float)(_perfectHits + _goodHits) / _totalTaps;
        if (accuracy >= 0.9f) result.SetCustomStarRating(3);
        else if (accuracy >= 0.7f) result.SetCustomStarRating(2);
        else if (accuracy >= 0.5f) result.SetCustomStarRating(1);
        else result.SetCustomStarRating(0);
        
        PlaySound(gameOverSound);
        _context?.ReportResult(result);
        
        Debug.Log($"[TapTrainer] Game finished! Score: {finalScore}, Accuracy: {accuracy:P1}");
    }

    void ApplyDifficultySettings()
    {
        switch (_difficulty)
        {
            case GameDifficulty.VeryEasy:
                speedMultiplier = 0.5f;
                windowMultiplier = 1.5f;
                break;
            case GameDifficulty.Easy:
                speedMultiplier = 0.7f;
                windowMultiplier = 1.2f;
                break;
            case GameDifficulty.Medium:
                speedMultiplier = 1f;
                windowMultiplier = 1f;
                break;
            case GameDifficulty.Hard:
                speedMultiplier = 1.3f;
                windowMultiplier = 0.8f;
                break;
            case GameDifficulty.VeryHard:
                speedMultiplier = 1.6f;
                windowMultiplier = 0.6f;
                break;
            case GameDifficulty.Expert:
                speedMultiplier = 2f;
                windowMultiplier = 0.4f;
                break;
        }
    }

    void ApplyGameModeSettings()
    {
        switch (_gameMode)
        {
            case GameMode.TimeAttack:
                totalRounds = 20;
                // Could add time pressure effects
                break;
            case GameMode.Survival:
                totalRounds = 100;
                // Miss = game over
                break;
            case GameMode.Endless:
                totalRounds = int.MaxValue;
                break;
            case GameMode.Zen:
                speedMultiplier = 0.5f;
                windowMultiplier = 2f;
                break;
        }
    }

    void SetupAudio()
    {
        if (audioSource != null && _settings != null)
        {
            audioSource.volume = _settings.soundVolume;
        }
    }

    void SetupUI()
    {
        if (tapButton != null)
        {
            // Make tap button cover the whole screen for easy tapping
            var rectTransform = tapButton.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {_score:N0}";
        
        if (instructionsText != null && _gameMode != GameMode.Endless)
            instructionsText.text = $"Round {_currentRound + 1}/{totalRounds}";
    }

    void PlayHitEffect(bool hit)
    {
        var effect = hit ? hitEffect : missEffect;
        if (effect != null)
            effect.Play();
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null && _settings != null && _settings.soundVolume > 0)
        {
            audioSource.PlayOneShot(clip, _settings.soundVolume);
        }
    }
}

[System.Serializable]
public class TapTrainerState
{
    public int score;
    public int currentRound;
    public int totalRounds;
    public GameDifficulty difficulty;
    public GameMode gameMode;
}