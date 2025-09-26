using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class HomeController : MonoBehaviour 
{
    [Header("References")]
    public GameRegistry registry;
    public Transform gridRoot; // parent for game buttons
    public Button buttonPrefab; // simple UI button prefab
    public Text coinText;
    public Text levelText;
    
    [Header("Category Navigation")]
    public Transform categoryTabs;
    public Button categoryButtonPrefab;
    
    [Header("Search and Filter")]
    public InputField searchField;
    public Dropdown categoryFilter;
    public Button randomGameButton;
    
    [Header("Featured Section")]
    public Transform featuredGamesRoot;
    public Button featuredGamePrefab;
    
    [Header("Player Stats")]
    public Slider progressSlider;
    public Text progressText;
    public Button achievementsButton;
    public Button shopButton;
    public Button settingsButton;

    private GameCategory _currentCategory = GameCategory.Arcade;
    private List<MiniGameDef> _currentGames = new List<MiniGameDef>();
    private Dictionary<GameCategory, Button> _categoryButtons = new Dictionary<GameCategory, Button>();

    void Start()
    {
        if (registry == null)
        {
            Debug.LogError("[HomeController] GameRegistry not assigned!");
            return;
        }

        InitializeUI();
        SetupEventListeners();
        RefreshAllData();
        
        Debug.Log("[HomeController] Home scene initialized");
    }

    void InitializeUI()
    {
        // Setup category tabs
        CreateCategoryTabs();
        
        // Setup filter dropdown
        SetupCategoryFilter();
        
        // Setup search
        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(OnSearchChanged);
        }
        
        // Setup buttons
        if (randomGameButton != null)
            randomGameButton.onClick.AddListener(PlayRandomGame);
        
        if (achievementsButton != null)
            achievementsButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Achievements));
        
        if (shopButton != null)
            shopButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Shop));
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => SceneManager.LoadScene(SceneNames.Settings));
    }

    void CreateCategoryTabs()
    {
        if (categoryTabs == null || categoryButtonPrefab == null) return;

        var categories = System.Enum.GetValues(typeof(GameCategory)).Cast<GameCategory>();
        
        foreach (var category in categories)
        {
            var categoryButton = Instantiate(categoryButtonPrefab, categoryTabs);
            categoryButton.GetComponentInChildren<Text>().text = category.ToString();
            
            var cat = category; // Capture for closure
            categoryButton.onClick.AddListener(() => SelectCategory(cat));
            
            _categoryButtons[category] = categoryButton;
        }
        
        // Select first category
        SelectCategory(_currentCategory);
    }

    void SetupCategoryFilter()
    {
        if (categoryFilter == null) return;
        
        categoryFilter.ClearOptions();
        var options = new List<string> { "All Categories" };
        options.AddRange(System.Enum.GetNames(typeof(GameCategory)));
        categoryFilter.AddOptions(options);
        
        categoryFilter.onValueChanged.AddListener(OnCategoryFilterChanged);
    }

    void SetupEventListeners()
    {
        // Listen to economy changes
        ServiceLocator.Economy.OnCoinsChanged += RefreshCoinDisplay;
        ServiceLocator.Economy.OnLevelUp += OnPlayerLevelUp;
        
        // Listen to game events
        ServiceLocator.Bus.OnReturnToHub += OnReturnFromGame;
        ServiceLocator.Bus.OnGameUnlocked += OnGameUnlocked;
    }

    void RefreshAllData()
    {
        RefreshCoinDisplay();
        RefreshLevelDisplay();
        RefreshProgressDisplay();
        RefreshFeaturedGames();
        RefreshGameGrid();
    }

    void RefreshCoinDisplay()
    {
        if (coinText != null)
            coinText.text = $"Coins: {ServiceLocator.Economy.Coins:N0}";
    }

    void RefreshLevelDisplay()
    {
        if (levelText != null)
            levelText.text = $"Level {ServiceLocator.Economy.PlayerLevel}";
    }

    void RefreshProgressDisplay()
    {
        if (progressSlider != null && progressText != null)
        {
            float progress = registry.GetProgressPercentage();
            progressSlider.value = progress / 100f;
            progressText.text = $"{registry.GetUnlockedGameCount()}/{registry.GetTotalGameCount()} Games";
        }
    }

    void RefreshFeaturedGames()
    {
        if (featuredGamesRoot == null || featuredGamePrefab == null) return;
        
        // Clear existing featured games
        foreach (Transform child in featuredGamesRoot)
        {
            if (child != featuredGamePrefab.transform)
                Destroy(child.gameObject);
        }
        
        var featuredGames = registry.GetFeaturedGames();
        foreach (var game in featuredGames.Take(6)) // Show max 6 featured games
        {
            CreateFeaturedGameButton(game);
        }
    }

    void CreateFeaturedGameButton(MiniGameDef game)
    {
        var button = Instantiate(featuredGamePrefab, featuredGamesRoot);
        button.gameObject.SetActive(true);
        
        // Setup button appearance
        var buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
            buttonText.text = game.displayName;
        
        var iconImage = button.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null && game.icon != null)
            iconImage.sprite = game.icon;
        
        // Add click listener
        button.onClick.AddListener(() => StartMiniGame(game.gameId));
    }

    void RefreshGameGrid()
    {
        if (gridRoot == null || buttonPrefab == null) return;
        
        // Clear existing buttons
        foreach (Transform child in gridRoot)
        {
            if (child != buttonPrefab.transform)
                Destroy(child.gameObject);
        }
        
        // Get games to display
        _currentGames = GetGamesToDisplay();
        
        // Create buttons for games
        foreach (var game in _currentGames)
        {
            CreateGameButton(game);
        }
        
        Debug.Log($"[HomeController] Showing {_currentGames.Count} games in category {_currentCategory}");
    }

    List<MiniGameDef> GetGamesToDisplay()
    {
        List<MiniGameDef> games;
        
        // Apply search filter
        string searchTerm = searchField?.text ?? "";
        if (!string.IsNullOrEmpty(searchTerm))
        {
            games = registry.SearchGames(searchTerm, _currentCategory);
        }
        else
        {
            games = registry.GetByCategory(_currentCategory);
        }
        
        // Filter by unlocked status
        games = games.Where(g => g.IsUnlocked()).ToList();
        
        // Sort by priority
        games = games.OrderBy(g => g.priority).ThenBy(g => g.displayName).ToList();
        
        return games;
    }

    void CreateGameButton(MiniGameDef game)
    {
        var button = Instantiate(buttonPrefab, gridRoot);
        button.gameObject.SetActive(true);
        
        // Setup button text
        var buttonText = button.GetComponentInChildren<Text>();
        if (buttonText != null)
        {
            buttonText.text = game.displayName;
            
            // Add difficulty indicator
            string difficultyIcon = GetDifficultyIcon(game.baseDifficulty);
            buttonText.text += $" {difficultyIcon}";
        }
        
        // Setup icon if available
        var iconImage = button.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage != null && game.icon != null)
            iconImage.sprite = game.icon;
        
        // Add new game indicator
        if (game.newGame)
        {
            var newBadge = button.transform.Find("NewBadge");
            if (newBadge != null)
                newBadge.gameObject.SetActive(true);
        }
        
        // Add click listener
        button.onClick.AddListener(() => StartMiniGame(game.gameId));
        
        // Add hover effects (for PC testing)
        #if UNITY_EDITOR || UNITY_STANDALONE
        var buttonComponent = button.GetComponent<Button>();
        var colors = buttonComponent.colors;
        colors.highlightedColor = Color.yellow;
        buttonComponent.colors = colors;
        #endif
    }

    string GetDifficultyIcon(GameDifficulty difficulty)
    {
        switch (difficulty)
        {
            case GameDifficulty.VeryEasy: return "⭐";
            case GameDifficulty.Easy: return "⭐⭐";
            case GameDifficulty.Medium: return "⭐⭐⭐";
            case GameDifficulty.Hard: return "⭐⭐⭐⭐";
            case GameDifficulty.VeryHard: return "⭐⭐⭐⭐⭐";
            case GameDifficulty.Expert: return "💀";
            default: return "";
        }
    }

    public void SelectCategory(GameCategory category)
    {
        _currentCategory = category;
        
        // Update button states
        foreach (var kvp in _categoryButtons)
        {
            var buttonComponent = kvp.Value.GetComponent<Button>();
            var colors = buttonComponent.colors;
            colors.normalColor = kvp.Key == category ? Color.green : Color.white;
            buttonComponent.colors = colors;
        }
        
        RefreshGameGrid();
        ServiceLocator.Bus.PublishCategorySelected(category.ToString());
    }

    public void StartMiniGame(string gameId)
    {
        var gameDef = registry.GetById(gameId);
        if (gameDef == null)
        {
            Debug.LogError($"[HomeController] Game not found: {gameId}");
            return;
        }
        
        if (!gameDef.IsUnlocked())
        {
            Debug.Log($"[HomeController] Game not unlocked: {gameId}");
            // Could show unlock requirements dialog here
            return;
        }
        
        // Store the game to load
        PlayerPrefs.SetString("_pending_game", gameId);
        
        // Record game start
        ServiceLocator.Economy.RecordGamePlayed(gameId, 0);
        
        Debug.Log($"[HomeController] Starting game: {gameId}");
        SceneManager.LoadScene(SceneNames.MiniGame);
    }

    public void PlayRandomGame()
    {
        var randomGames = registry.GetRandomGames(1, _currentCategory);
        if (randomGames.Count > 0)
        {
            StartMiniGame(randomGames[0].gameId);
        }
    }

    // Event handlers
    void OnSearchChanged(string searchTerm)
    {
        RefreshGameGrid();
    }

    void OnCategoryFilterChanged(int index)
    {
        if (index == 0) // "All Categories"
        {
            // Could implement all categories view
        }
        else
        {
            var categories = System.Enum.GetValues(typeof(GameCategory)).Cast<GameCategory>().ToArray();
            if (index - 1 < categories.Length)
            {
                SelectCategory(categories[index - 1]);
            }
        }
    }

    void OnPlayerLevelUp(int newLevel)
    {
        RefreshLevelDisplay();
        RefreshGameGrid(); // Some games might have been unlocked
        
        // Could show level up celebration here
        Debug.Log($"[HomeController] Player reached level {newLevel}!");
    }

    void OnReturnFromGame()
    {
        RefreshAllData();
    }

    void OnGameUnlocked(string gameId)
    {
        RefreshGameGrid();
        Debug.Log($"[HomeController] Game unlocked: {gameId}");
    }

    void OnDestroy()
    {
        // Cleanup event listeners
        if (ServiceLocator.Economy != null)
        {
            ServiceLocator.Economy.OnCoinsChanged -= RefreshCoinDisplay;
            ServiceLocator.Economy.OnLevelUp -= OnPlayerLevelUp;
        }
        
        if (ServiceLocator.Bus != null)
        {
            ServiceLocator.Bus.OnReturnToHub -= OnReturnFromGame;
            ServiceLocator.Bus.OnGameUnlocked -= OnGameUnlocked;
        }
    }
}