using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;

/// <summary>
/// Handles loading and running individual mini-games
/// Implements IMinigameContext to provide services to games
/// </summary>
public class MiniGameLoader : MonoBehaviour, IMinigameContext 
{
    [Header("References")]
    public GameRegistry registry;
    public Transform gameRoot;
    public Button quitButton;
    public Button pauseButton;
    
    [Header("UI Elements")]
    public Text gameTitle;
    public Text coinDisplay;
    public Text scoreDisplay;
    public Slider progressBar;
    public Text statusText;
    
    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public Button resumeButton;
    public Button restartButton;
    public Button homeButton;
    
    [Header("Results Screen")]
    public GameObject resultsScreen;
    public Text finalScoreText;
    public Text coinsEarnedText;
    public Text newRecordText;
    public Button playAgainButton;
    public Button nextGameButton;
    public Button backToHubButton;

    private IMinigame _current;
    private string _gameId;
    private MiniGameDef _gameDef;
    private bool _gameActive = false;
    private bool _gamePaused = false;
    private float _gameStartTime;
    private AsyncOperationHandle<GameObject> _loadHandle;

    void Start()
    {
        _gameId = PlayerPrefs.GetString("_pending_game", string.Empty);
        _gameDef = registry.GetById(_gameId);
        
        if (_gameDef == null)
        {
            Debug.LogError($"[MiniGameLoader] Game not found: {_gameId}");
            ReturnToHub();
            return;
        }

        StartCoroutine(LoadAndStartGame());
        SetupUI();
    }

    void SetupUI()
    {
        // Setup basic UI
        if (gameTitle != null)
            gameTitle.text = _gameDef.displayName;
        
        if (coinDisplay != null)
            RefreshCoinDisplay();
        
        // Setup buttons
        if (quitButton != null)
            quitButton.onClick.AddListener(RequestQuit);
        
        if (pauseButton != null)
            pauseButton.onClick.AddListener(RequestPause);
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(RequestResume);
        
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        
        if (homeButton != null)
            homeButton.onClick.AddListener(RequestQuit);
        
        // Results screen buttons
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(RestartGame);
        
        if (nextGameButton != null)
            nextGameButton.onClick.AddListener(PlayNextGame);
        
        if (backToHubButton != null)
            backToHubButton.onClick.AddListener(RequestQuit);
        
        // Hide menus initially
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        
        if (resultsScreen != null)
            resultsScreen.SetActive(false);
    }

    IEnumerator LoadAndStartGame()
    {
        if (statusText != null)
            statusText.text = "Loading game...";
        
        GameObject gamePrefab = null;
        
        // Try loading via Addressables first
        if (_gameDef.useAddressables)
        {
            yield return StartCoroutine(LoadViaAddressables());
            if (_loadHandle.IsValid() && _loadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                gamePrefab = _loadHandle.Result;
            }
        }
        
        // Fallback to Resources if Addressables failed
        if (gamePrefab == null)
        {
            yield return StartCoroutine(LoadViaResources());
        }
        
        if (gamePrefab == null)
        {
            Debug.LogError($"[MiniGameLoader] Failed to load game prefab: {_gameDef.prefabKey}");
            if (statusText != null)
                statusText.text = "Failed to load game!";
            yield return new WaitForSeconds(2f);
            ReturnToHub();
            yield break;
        }
        
        // Instantiate and initialize the game
        var gameObject = Instantiate(gamePrefab, gameRoot);
        _current = gameObject.GetComponent<IMinigame>();
        
        if (_current == null)
        {
            Debug.LogError($"[MiniGameLoader] Game prefab doesn't implement IMinigame: {_gameDef.gameId}");
            if (statusText != null)
                statusText.text = "Game not compatible!";
            yield return new WaitForSeconds(2f);
            ReturnToHub();
            yield break;
        }
        
        // Initialize and start the game
        _current.Init(this);
        _gameStartTime = Time.time;
        _gameActive = true;
        _current.StartGame();
        
        if (statusText != null)
            statusText.text = "Game ready!";
        
        // Report game started
        var startResult = new GameResult(_gameId, 0, 0, 0f, false);
        ServiceLocator.Bus.PublishMiniGameStarted(startResult);
        
        Debug.Log($"[MiniGameLoader] Game started: {_gameId}");
    }

    IEnumerator LoadViaAddressables()
    {
        try
        {
            _loadHandle = Addressables.LoadAssetAsync<GameObject>(_gameDef.prefabKey);
            yield return _loadHandle;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[MiniGameLoader] Addressables load failed: {e.Message}");
        }
    }

    IEnumerator LoadViaResources()
    {
        try
        {
            var prefab = Resources.Load<GameObject>(_gameDef.prefabKey);
            if (prefab != null)
            {
                // Create a fake successful handle for consistency
                Debug.Log($"[MiniGameLoader] Loaded via Resources: {_gameDef.prefabKey}");
            }
            yield return null; // Simulate async behavior
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MiniGameLoader] Resources load failed: {e.Message}");
        }
    }

    void RefreshCoinDisplay()
    {
        if (coinDisplay != null)
            coinDisplay.text = $"Coins: {ServiceLocator.Economy.Coins:N0}";
    }

    void RestartGame()
    {
        if (_current != null)
        {
            _current.StopGame();
            _current.CleanupGame();
        }
        
        // Hide menus
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (resultsScreen != null)
            resultsScreen.SetActive(false);
        
        // Restart
        StartCoroutine(LoadAndStartGame());
    }

    void PlayNextGame()
    {
        var recommendations = registry.GetRecommendedGames(_gameId, 1);
        if (recommendations.Count > 0)
        {
            PlayerPrefs.SetString("_pending_game", recommendations[0].gameId);
            SceneManager.LoadScene(SceneNames.MiniGame);
        }
        else
        {
            ReturnToHub();
        }
    }

    void ReturnToHub()
    {
        ServiceLocator.Bus.PublishReturnToHub();
        SceneManager.LoadScene(SceneNames.Home);
    }

    // IMinigameContext implementation
    public void ReportResult(GameResult result)
    {
        if (!_gameActive) return;
        
        _gameActive = false;
        float duration = Time.time - _gameStartTime;
        result.duration = duration;
        
        // Calculate coin reward
        int coinReward = _gameDef.GetCoinReward(result.score, result.difficulty);
        result.coinsAwarded = coinReward;
        
        // Check for new record
        int previousHighScore = ServiceLocator.Save.Data.gameScores[GetGameIndex()];
        if (result.score > previousHighScore)
        {
            result.newRecord = true;
            ServiceLocator.Save.Data.gameScores[GetGameIndex()] = result.score;
        }
        
        // Award coins and record stats
        ServiceLocator.Economy.AddCoins(coinReward);
        ServiceLocator.Economy.RecordGamePlayed(_gameId, result.score);
        ServiceLocator.Economy.RecordPlayTime(duration);
        
        // Mark as completed if applicable
        if (result.completed)
        {
            ServiceLocator.Save.Data.gameCompleted[GetGameIndex()] = true;
        }
        
        ServiceLocator.Save.Save();
        
        // Publish result
        ServiceLocator.Bus.PublishMiniGameFinished(result);
        
        // Show results screen
        ShowResultsScreen(result);
        
        Debug.Log($"[MiniGameLoader] Game finished: {result.gameId}, Score: {result.score}, Coins: {coinReward}");
    }

    void ShowResultsScreen(GameResult result)
    {
        if (resultsScreen == null) return;
        
        resultsScreen.SetActive(true);
        
        if (finalScoreText != null)
            finalScoreText.text = $"Score: {result.score:N0}";
        
        if (coinsEarnedText != null)
            coinsEarnedText.text = $"Coins Earned: +{result.coinsAwarded}";
        
        if (newRecordText != null)
        {
            newRecordText.gameObject.SetActive(result.newRecord);
            if (result.newRecord)
                newRecordText.text = "NEW RECORD!";
        }
        
        RefreshCoinDisplay();
    }

    int GetGameIndex()
    {
        // Simple hash of game ID to get consistent index
        return Mathf.Abs(_gameId.GetHashCode()) % 10000;
    }

    public void ReportProgress(float progress, string status = "")
    {
        if (progressBar != null)
            progressBar.value = progress;
        
        if (statusText != null && !string.IsNullOrEmpty(status))
            statusText.text = status;
    }

    public void RequestPause()
    {
        if (!_gameActive || _gamePaused) return;
        
        _gamePaused = true;
        _current?.StopGame();
        
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
        
        Time.timeScale = 0f; // Pause game time
    }

    public void RequestResume()
    {
        if (!_gamePaused) return;
        
        _gamePaused = false;
        Time.timeScale = 1f; // Resume game time
        
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        
        _current?.StartGame();
    }

    public void RequestQuit()
    {
        if (_current != null)
        {
            _current.StopGame();
            _current.CleanupGame();
        }
        
        Time.timeScale = 1f; // Ensure time scale is reset
        ReturnToHub();
    }

    public void ShowAchievement(string achievementId, string title, string description)
    {
        // Could implement achievement notification popup here
        Debug.Log($"[MiniGameLoader] Achievement unlocked: {title} - {description}");
    }

    public void ShowRewardedAd(System.Action<bool> onComplete)
    {
        // Implement rewarded ad logic here
        Debug.Log("[MiniGameLoader] Showing rewarded ad...");
        onComplete?.Invoke(true); // Simulate success for now
    }

    public void PlayHaptic(HapticType type)
    {
        // Implement haptic feedback for mobile devices
        #if UNITY_ANDROID || UNITY_IOS
        switch (type)
        {
            case HapticType.Light:
                Handheld.Vibrate();
                break;
            case HapticType.Medium:
            case HapticType.Heavy:
                Handheld.Vibrate();
                break;
        }
        #endif
    }

    public GameSettings GetPlayerSettings()
    {
        // Return player settings - could be expanded
        return new GameSettings();
    }

    public PlayerStats GetPlayerStats()
    {
        var save = ServiceLocator.Save.Data;
        return new PlayerStats
        {
            totalGamesPlayed = save.totalGamesPlayed,
            totalPlayTime = save.totalPlayTime,
            currentLevel = ServiceLocator.Economy.PlayerLevel,
            totalCoins = ServiceLocator.Economy.Coins
        };
    }

    public bool IsGameUnlocked(string gameId)
    {
        var game = registry.GetById(gameId);
        return game?.IsUnlocked() ?? false;
    }

    public int GetHighScore(string gameId)
    {
        int index = Mathf.Abs(gameId.GetHashCode()) % 10000;
        return ServiceLocator.Save.Data.gameScores[index];
    }

    public void SaveGameData(string key, string data)
    {
        PlayerPrefs.SetString($"game_{_gameId}_{key}", data);
    }

    public string LoadGameData(string key)
    {
        return PlayerPrefs.GetString($"game_{_gameId}_{key}", "");
    }

    void OnDestroy()
    {
        // Cleanup
        if (_current != null)
        {
            _current.StopGame();
            _current.CleanupGame();
        }
        
        // Release Addressables handle
        if (_loadHandle.IsValid())
        {
            Addressables.Release(_loadHandle);
        }
        
        Time.timeScale = 1f; // Ensure time scale is reset
    }

    void Update()
    {
        // Handle Android back button
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_gamePaused)
                RequestResume();
            else
                RequestPause();
        }
    }
}