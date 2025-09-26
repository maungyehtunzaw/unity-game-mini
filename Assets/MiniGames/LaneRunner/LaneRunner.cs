using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Lane Runner - An endless runner where players swipe left/right to avoid obstacles
/// Another base template that can be varied thousands of ways
/// </summary>
public class LaneRunner : MonoBehaviour, IMinigame, IConfigurableMinigame
{
    [Header("Game Objects")]
    public Transform player;
    public Transform[] lanes; // Lane positions
    public GameObject obstaclePrefab;
    public GameObject coinPrefab;
    public GameObject powerUpPrefab;
    
    [Header("UI")]
    public Text scoreText;
    public Text distanceText;
    public Text instructionsText;
    public Slider healthBar;
    
    [Header("Game Settings")]
    public float forwardSpeed = 6f;
    public float laneChangeSpeed = 5f;
    public float obstacleSpawnDistance = 10f;
    public int maxHealth = 3;
    
    [Header("Difficulty Progression")]
    public float speedIncreaseRate = 0.1f;
    public float spawnRateIncrease = 0.05f;
    
    [Header("Effects")]
    public ParticleSystem crashEffect;
    public ParticleSystem coinCollectEffect;
    public AudioSource audioSource;
    public AudioClip coinSound;
    public AudioClip crashSound;
    public AudioClip powerUpSound;

    // Private game state
    private IMinigameContext _context;
    private int _currentLane = 1; // Start in middle lane
    private float _distance = 0f;
    private int _score = 0;
    private int _health;
    private bool _running = false;
    private float _gameStartTime;
    private float _nextSpawnZ;
    private float _currentSpeed;
    private Queue<GameObject> _activeObstacles = new Queue<GameObject>();
    private Queue<GameObject> _activeCoins = new Queue<GameObject>();
    
    // Input handling
    private Vector2 _startTouchPos;
    private bool _touchStarted = false;
    
    // Configurable properties
    private GameDifficulty _difficulty = GameDifficulty.Medium;
    private GameMode _gameMode = GameMode.Normal;
    private GameSettings _settings;

    public void Init(IMinigameContext ctx)
    {
        _context = ctx;
        _settings = ctx.GetPlayerSettings();
        
        SetupLanes();
        SetupAudio();
        
        Debug.Log("[LaneRunner] Initialized");
    }

    public void StartGame()
    {
        _running = true;
        _distance = 0f;
        _score = 0;
        _health = maxHealth;
        _currentLane = lanes.Length / 2; // Start in middle
        _gameStartTime = Time.time;
        _nextSpawnZ = obstacleSpawnDistance;
        _currentSpeed = forwardSpeed;
        
        // Apply difficulty settings
        ApplyDifficultySettings();
        
        // Position player
        if (player != null && lanes.Length > 0)
        {
            Vector3 pos = player.position;
            pos.x = lanes[_currentLane].position.x;
            player.position = pos;
        }
        
        // Setup UI
        if (instructionsText != null)
            instructionsText.text = "Swipe left/right to change lanes!";
        
        UpdateUI();
        
        Debug.Log($"[LaneRunner] Game started - Difficulty: {_difficulty}, Mode: {_gameMode}");
    }

    public void StopGame()
    {
        _running = false;
        Debug.Log("[LaneRunner] Game stopped");
    }

    public void CleanupGame()
    {
        StopGame();
        
        // Clean up spawned objects
        CleanupObjects();
        
        Debug.Log("[LaneRunner] Game cleaned up");
    }

    public string GetGameState()
    {
        var state = new LaneRunnerState
        {
            score = _score,
            distance = _distance,
            health = _health,
            currentLane = _currentLane,
            difficulty = _difficulty,
            gameMode = _gameMode
        };
        
        return JsonUtility.ToJson(state);
    }

    public void LoadGameState(string state)
    {
        try
        {
            var gameState = JsonUtility.FromJson<LaneRunnerState>(state);
            _score = gameState.score;
            _distance = gameState.distance;
            _health = gameState.health;
            _currentLane = gameState.currentLane;
            _difficulty = gameState.difficulty;
            _gameMode = gameState.gameMode;
            
            UpdateUI();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LaneRunner] Failed to load game state: {e.Message}");
        }
    }

    public GameInfo GetGameInfo()
    {
        return new GameInfo("lane_runner", "Lane Runner", GameCategory.Arcade)
        {
            description = "Swipe to change lanes and avoid obstacles in this endless runner!",
            difficulty = _difficulty,
            estimatedPlayTime = 5f,
            tags = new string[] { "endless", "runner", "arcade", "swipe" }
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
        
        HandleInput();
        UpdateMovement();
        UpdateSpawning();
        UpdateGameLogic();
        CleanupOffScreenObjects();
    }

    void HandleInput()
    {
        // Mouse/touch input handling
        if (Input.GetMouseButtonDown(0))
        {
            _startTouchPos = Input.mousePosition;
            _touchStarted = true;
        }
        else if (Input.GetMouseButtonUp(0) && _touchStarted)
        {
            Vector2 endTouchPos = Input.mousePosition;
            Vector2 swipeDelta = endTouchPos - _startTouchPos;
            
            // Minimum swipe distance to register
            if (swipeDelta.magnitude > 50f)
            {
                if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                {
                    // Horizontal swipe
                    if (swipeDelta.x > 0)
                        ChangeLane(1); // Swipe right
                    else
                        ChangeLane(-1); // Swipe left
                }
            }
            
            _touchStarted = false;
        }
        
        // Keyboard input (for testing)
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            ChangeLane(-1);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            ChangeLane(1);
    }

    void ChangeLane(int direction)
    {
        int newLane = Mathf.Clamp(_currentLane + direction, 0, lanes.Length - 1);
        
        if (newLane != _currentLane)
        {
            _currentLane = newLane;
            
            // Smooth movement to new lane
            if (player != null)
            {
                StartCoroutine(MovePlayerToLane());
            }
            
            // Haptic feedback
            _context?.PlayHaptic(HapticType.Light);
        }
    }

    IEnumerator MovePlayerToLane()
    {
        if (player == null || lanes.Length == 0) yield break;
        
        Vector3 startPos = player.position;
        Vector3 targetPos = startPos;
        targetPos.x = lanes[_currentLane].position.x;
        
        float elapsed = 0f;
        float duration = 1f / laneChangeSpeed;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            
            player.position = Vector3.Lerp(startPos, targetPos, progress);
            yield return null;
        }
        
        player.position = targetPos;
    }

    void UpdateMovement()
    {
        // Move player forward
        _distance += _currentSpeed * Time.deltaTime;
        
        if (player != null)
        {
            Vector3 pos = player.position;
            pos.z += _currentSpeed * Time.deltaTime;
            player.position = pos;
        }
        
        // Increase speed over time
        _currentSpeed += speedIncreaseRate * Time.deltaTime;
    }

    void UpdateSpawning()
    {
        if (_distance >= _nextSpawnZ)
        {
            SpawnObstacle();
            
            // Occasionally spawn coins and power-ups
            if (Random.value < 0.3f)
                SpawnCoin();
            
            if (Random.value < 0.1f)
                SpawnPowerUp();
            
            // Calculate next spawn distance
            float baseDistance = obstacleSpawnDistance;
            float speedFactor = _currentSpeed / forwardSpeed;
            _nextSpawnZ = _distance + (baseDistance / speedFactor);
        }
    }

    void SpawnObstacle()
    {
        if (obstaclePrefab == null || lanes.Length == 0) return;
        
        // Choose random lane for obstacle
        int obstacleLane = Random.Range(0, lanes.Length);
        
        Vector3 spawnPos = lanes[obstacleLane].position;
        spawnPos.z = player.position.z + 20f; // Spawn ahead of player
        
        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        obstacle.GetComponent<ObstacleController>()?.Initialize(this);
        
        _activeObstacles.Enqueue(obstacle);
    }

    void SpawnCoin()
    {
        if (coinPrefab == null || lanes.Length == 0) return;
        
        // Find a lane without obstacles nearby
        int coinLane = Random.Range(0, lanes.Length);
        
        Vector3 spawnPos = lanes[coinLane].position;
        spawnPos.z = player.position.z + 15f;
        spawnPos.y += 0.5f; // Slightly elevated
        
        GameObject coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        coin.GetComponent<CoinController>()?.Initialize(this);
        
        _activeCoins.Enqueue(coin);
    }

    void SpawnPowerUp()
    {
        if (powerUpPrefab == null || lanes.Length == 0) return;
        
        int powerUpLane = Random.Range(0, lanes.Length);
        
        Vector3 spawnPos = lanes[powerUpLane].position;
        spawnPos.z = player.position.z + 18f;
        spawnPos.y += 1f;
        
        GameObject powerUp = Instantiate(powerUpPrefab, spawnPos, Quaternion.identity);
        powerUp.GetComponent<PowerUpController>()?.Initialize(this);
    }

    void UpdateGameLogic()
    {
        // Update score based on distance
        int distanceScore = Mathf.FloorToInt(_distance / 10f) * 10;
        if (distanceScore > _score)
        {
            _score = distanceScore;
            UpdateUI();
        }
        
        // Report progress
        float progress = Mathf.Min(1f, _distance / 1000f); // Progress up to 1000m
        _context?.ReportProgress(progress, $"Distance: {_distance:F0}m");
    }

    void CleanupOffScreenObjects()
    {
        // Clean up obstacles that are behind the player
        while (_activeObstacles.Count > 0)
        {
            var obstacle = _activeObstacles.Peek();
            if (obstacle == null || obstacle.transform.position.z < player.position.z - 10f)
            {
                _activeObstacles.Dequeue();
                if (obstacle != null)
                    Destroy(obstacle);
            }
            else
            {
                break;
            }
        }
        
        // Clean up coins
        while (_activeCoins.Count > 0)
        {
            var coin = _activeCoins.Peek();
            if (coin == null || coin.transform.position.z < player.position.z - 10f)
            {
                _activeCoins.Dequeue();
                if (coin != null)
                    Destroy(coin);
            }
            else
            {
                break;
            }
        }
    }

    void CleanupObjects()
    {
        // Destroy all active objects
        while (_activeObstacles.Count > 0)
        {
            var obj = _activeObstacles.Dequeue();
            if (obj != null) Destroy(obj);
        }
        
        while (_activeCoins.Count > 0)
        {
            var obj = _activeCoins.Dequeue();
            if (obj != null) Destroy(obj);
        }
    }

    // Collision handling
    public void OnObstacleHit()
    {
        _health--;
        
        if (crashEffect != null)
            crashEffect.Play();
        
        PlaySound(crashSound);
        _context?.PlayHaptic(HapticType.Heavy);
        
        UpdateUI();
        
        if (_health <= 0)
        {
            GameOver();
        }
    }

    public void OnCoinCollected()
    {
        _score += 50;
        
        if (coinCollectEffect != null)
            coinCollectEffect.Play();
        
        PlaySound(coinSound);
        _context?.PlayHaptic(HapticType.Light);
        
        UpdateUI();
    }

    public void OnPowerUpCollected(PowerUpType type)
    {
        PlaySound(powerUpSound);
        _context?.PlayHaptic(HapticType.Medium);
        
        switch (type)
        {
            case PowerUpType.Health:
                _health = Mathf.Min(maxHealth, _health + 1);
                break;
            case PowerUpType.ScoreMultiplier:
                _score += 100;
                break;
            case PowerUpType.Shield:
                // Could implement temporary invincibility
                break;
        }
        
        UpdateUI();
    }

    void GameOver()
    {
        _running = false;
        
        float duration = Time.time - _gameStartTime;
        
        // Create result
        var result = new GameResult("lane_runner", _score, 0, duration, _distance >= 500f)
        {
            difficulty = _difficulty
        };
        
        // Calculate stars based on distance
        if (_distance >= 1000f) result.SetCustomStarRating(3);
        else if (_distance >= 500f) result.SetCustomStarRating(2);
        else if (_distance >= 200f) result.SetCustomStarRating(1);
        else result.SetCustomStarRating(0);
        
        _context?.ReportResult(result);
        
        Debug.Log($"[LaneRunner] Game Over! Distance: {_distance:F0}m, Score: {_score}");
    }

    void ApplyDifficultySettings()
    {
        switch (_difficulty)
        {
            case GameDifficulty.VeryEasy:
                forwardSpeed = 4f;
                obstacleSpawnDistance = 15f;
                maxHealth = 5;
                break;
            case GameDifficulty.Easy:
                forwardSpeed = 5f;
                obstacleSpawnDistance = 12f;
                maxHealth = 4;
                break;
            case GameDifficulty.Medium:
                forwardSpeed = 6f;
                obstacleSpawnDistance = 10f;
                maxHealth = 3;
                break;
            case GameDifficulty.Hard:
                forwardSpeed = 8f;
                obstacleSpawnDistance = 8f;
                maxHealth = 2;
                break;
            case GameDifficulty.VeryHard:
                forwardSpeed = 10f;
                obstacleSpawnDistance = 6f;
                maxHealth = 2;
                break;
            case GameDifficulty.Expert:
                forwardSpeed = 12f;
                obstacleSpawnDistance = 5f;
                maxHealth = 1;
                break;
        }
        
        _currentSpeed = forwardSpeed;
        _health = maxHealth;
    }

    void ApplyGameModeSettings()
    {
        switch (_gameMode)
        {
            case GameMode.TimeAttack:
                // Could add time limit
                break;
            case GameMode.Survival:
                maxHealth = 1; // One hit and you're out
                break;
            case GameMode.Zen:
                forwardSpeed *= 0.7f;
                obstacleSpawnDistance *= 1.5f;
                break;
        }
    }

    void SetupLanes()
    {
        if (lanes.Length == 0)
        {
            Debug.LogWarning("[LaneRunner] No lanes configured!");
        }
    }

    void SetupAudio()
    {
        if (audioSource != null && _settings != null)
        {
            audioSource.volume = _settings.soundVolume;
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {_score:N0}";
        
        if (distanceText != null)
            distanceText.text = $"Distance: {_distance:F0}m";
        
        if (healthBar != null)
            healthBar.value = (float)_health / maxHealth;
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null && _settings != null && _settings.soundVolume > 0)
        {
            audioSource.PlayOneShot(clip, _settings.soundVolume);
        }
    }
}

// Supporting classes for lane runner objects
public class ObstacleController : MonoBehaviour
{
    private LaneRunner _gameController;
    
    public void Initialize(LaneRunner controller)
    {
        _gameController = controller;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _gameController?.OnObstacleHit();
            Destroy(gameObject);
        }
    }
}

public class CoinController : MonoBehaviour
{
    private LaneRunner _gameController;
    
    public void Initialize(LaneRunner controller)
    {
        _gameController = controller;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _gameController?.OnCoinCollected();
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        // Rotate coin for visual appeal
        transform.Rotate(0, 90f * Time.deltaTime, 0);
    }
}

public class PowerUpController : MonoBehaviour
{
    public PowerUpType powerUpType = PowerUpType.Health;
    private LaneRunner _gameController;
    
    public void Initialize(LaneRunner controller)
    {
        _gameController = controller;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _gameController?.OnPowerUpCollected(powerUpType);
            Destroy(gameObject);
        }
    }
}

public enum PowerUpType
{
    Health,
    ScoreMultiplier,
    Shield
}

[System.Serializable]
public class LaneRunnerState
{
    public int score;
    public float distance;
    public int health;
    public int currentLane;
    public GameDifficulty difficulty;
    public GameMode gameMode;
}