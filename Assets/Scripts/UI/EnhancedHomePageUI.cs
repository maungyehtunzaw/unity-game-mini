using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using MiniGameHub.Core;

namespace MiniGameHub.UI
{
    /// <summary>
    /// Enhanced Home Page UI with status bar and game listing
    /// </summary>
    public class EnhancedHomePageUI : MonoBehaviour
    {
        [Header("Status Bar Elements")]
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Image profileIcon;
        
        [Header("Game Listing Elements")]
        [SerializeField] private Transform gameListParent;
        [SerializeField] private GameObject gameItemPrefab;
        [SerializeField] private ScrollRect gameScrollView;
        
        [Header("Category Filter")]
        [SerializeField] private Transform categoryTabsParent;
        [SerializeField] private GameObject categoryTabPrefab;
        [SerializeField] private Button allGamesTab;
        
        [Header("Featured Games Section")]
        [SerializeField] private Transform featuredGamesParent;
        [SerializeField] private GameObject featuredGamePrefab;
        [SerializeField] private GameObject featuredGamesSection;
        
        [Header("Search and Filter")]
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private Button searchButton;
        [SerializeField] private Button clearSearchButton;
        
        // Private variables
        private string currentCategory = GameListConstants.CATEGORY_ALL;
        private string currentSearchQuery = "";
        private List<GameObject> gameItemObjects = new List<GameObject>();
        private List<GameObject> categoryTabObjects = new List<GameObject>();
        private EconomyService economyService;
        
        // Events
        public System.Action<string> OnGameSelected;
        public System.Action OnProfileClicked;
        public System.Action OnSettingsClicked;

        private void Start()
        {
            InitializeServices();
            SetupStatusBar();
            SetupCategories();
            SetupFeaturedGames();
            SetupGameListing();
            SetupSearch();
            
            // Refresh UI
            RefreshCoinsDisplay();
            RefreshGameList();
        }

        private void InitializeServices()
        {
            economyService = ServiceLocator.Instance.GetService<EconomyService>();
            
            // Subscribe to economy events
            if (economyService != null)
            {
                economyService.OnCoinsChanged += RefreshCoinsDisplay;
            }
        }

        private void SetupStatusBar()
        {
            // Setup profile button
            if (profileButton != null)
            {
                profileButton.onClick.RemoveAllListeners();
                profileButton.onClick.AddListener(() => OnProfileClicked?.Invoke());
            }
            
            // Setup settings button
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveAllListeners();
                settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
            }
            
            // Load profile icon from player data
            LoadProfileIcon();
        }

        private void SetupCategories()
        {
            if (categoryTabsParent == null || categoryTabPrefab == null) return;
            
            // Clear existing tabs
            foreach (GameObject tab in categoryTabObjects)
            {
                if (tab != null) DestroyImmediate(tab);
            }
            categoryTabObjects.Clear();
            
            // Create category tabs
            var availableCategories = GameListConstants.GetAvailableCategories();
            
            foreach (string category in availableCategories)
            {
                GameObject tabObj = Instantiate(categoryTabPrefab, categoryTabsParent);
                CategoryTab tabComponent = tabObj.GetComponent<CategoryTab>();
                
                if (tabComponent != null)
                {
                    tabComponent.Setup(category, category == currentCategory);
                    tabComponent.OnTabSelected += SelectCategory;
                }
                
                categoryTabObjects.Add(tabObj);
            }
        }

        private void SetupFeaturedGames()
        {
            if (featuredGamesParent == null || featuredGamePrefab == null) return;
            
            // Clear existing featured games
            for (int i = featuredGamesParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(featuredGamesParent.GetChild(i).gameObject);
            }
            
            // Create featured games
            foreach (string gameId in GameListConstants.FeaturedGames)
            {
                if (GameListConstants.IsGameEnabled(gameId))
                {
                    GameObject featuredGameObj = Instantiate(featuredGamePrefab, featuredGamesParent);
                    FeaturedGameItem featuredItem = featuredGameObj.GetComponent<FeaturedGameItem>();
                    
                    if (featuredItem != null)
                    {
                        featuredItem.Setup(gameId);
                        featuredItem.OnGameSelected += (id) => OnGameSelected?.Invoke(id);
                    }
                }
            }
            
            // Show/hide featured section based on content
            if (featuredGamesSection != null)
            {
                featuredGamesSection.SetActive(featuredGamesParent.childCount > 0);
            }
        }

        private void SetupGameListing()
        {
            if (gameListParent == null || gameItemPrefab == null) return;
            
            // This will be populated by RefreshGameList()
        }

        private void SetupSearch()
        {
            if (searchField != null)
            {
                searchField.onValueChanged.RemoveAllListeners();
                searchField.onValueChanged.AddListener(OnSearchTextChanged);
                searchField.onEndEdit.RemoveAllListeners();
                searchField.onEndEdit.AddListener(OnSearchSubmitted);
            }
            
            if (searchButton != null)
            {
                searchButton.onClick.RemoveAllListeners();
                searchButton.onClick.AddListener(() => OnSearchSubmitted(searchField.text));
            }
            
            if (clearSearchButton != null)
            {
                clearSearchButton.onClick.RemoveAllListeners();
                clearSearchButton.onClick.AddListener(ClearSearch);
            }
        }

        private void RefreshCoinsDisplay()
        {
            if (coinsText != null && economyService != null)
            {
                int coins = economyService.GetCoins();
                coinsText.text = FormatCoinAmount(coins);
            }
        }

        private string FormatCoinAmount(int coins)
        {
            if (coins >= 1000000)
                return $"{coins / 1000000.0f:F1}M";
            else if (coins >= 1000)
                return $"{coins / 1000.0f:F1}K";
            else
                return coins.ToString();
        }

        private void LoadProfileIcon()
        {
            // Load profile icon from saved data or use default
            // This can be enhanced to load custom avatars
            if (profileIcon != null)
            {
                // For now, use a default icon
                // In a full implementation, you'd load from PlayerPrefs or a save file
            }
        }

        public void SelectCategory(string category)
        {
            if (currentCategory == category) return;
            
            currentCategory = category;
            
            // Update tab visual states
            foreach (GameObject tabObj in categoryTabObjects)
            {
                CategoryTab tab = tabObj.GetComponent<CategoryTab>();
                if (tab != null)
                {
                    tab.SetSelected(tab.CategoryName == category);
                }
            }
            
            RefreshGameList();
        }

        private void RefreshGameList()
        {
            if (gameListParent == null || gameItemPrefab == null) return;
            
            // Clear existing game items
            foreach (GameObject item in gameItemObjects)
            {
                if (item != null) DestroyImmediate(item);
            }
            gameItemObjects.Clear();
            
            // Get games for current category and search
            List<string> gamesToShow = GetFilteredGames();
            
            // Create game items
            foreach (string gameId in gamesToShow)
            {
                GameObject gameItemObj = Instantiate(gameItemPrefab, gameListParent);
                GameListItem gameItem = gameItemObj.GetComponent<GameListItem>();
                
                if (gameItem != null)
                {
                    gameItem.Setup(gameId);
                    gameItem.OnGameSelected += (id) => OnGameSelected?.Invoke(id);
                }
                
                gameItemObjects.Add(gameItemObj);
            }
            
            // Scroll to top after refresh
            if (gameScrollView != null)
            {
                Canvas.ForceUpdateCanvases();
                gameScrollView.verticalNormalizedPosition = 1f;
            }
        }

        private List<string> GetFilteredGames()
        {
            List<string> games = GameListConstants.GetEnabledGamesInCategory(currentCategory);
            
            // Apply search filter
            if (!string.IsNullOrEmpty(currentSearchQuery))
            {
                games = games.FindAll(gameId =>
                {
                    string displayName = GameListConstants.GetGameDisplayName(gameId);
                    string description = GameListConstants.GetGameDescription(gameId);
                    
                    return displayName.ToLower().Contains(currentSearchQuery.ToLower()) ||
                           description.ToLower().Contains(currentSearchQuery.ToLower());
                });
            }
            
            return games;
        }

        private void OnSearchTextChanged(string text)
        {
            // Optional: implement real-time search
            // For performance, you might want to add a small delay
        }

        private void OnSearchSubmitted(string text)
        {
            currentSearchQuery = text.Trim();
            RefreshGameList();
            
            // Update clear button visibility
            if (clearSearchButton != null)
            {
                clearSearchButton.gameObject.SetActive(!string.IsNullOrEmpty(currentSearchQuery));
            }
        }

        private void ClearSearch()
        {
            if (searchField != null)
            {
                searchField.text = "";
            }
            
            currentSearchQuery = "";
            RefreshGameList();
            
            if (clearSearchButton != null)
            {
                clearSearchButton.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (economyService != null)
            {
                economyService.OnCoinsChanged -= RefreshCoinsDisplay;
            }
        }

        #region Public API
        /// <summary>
        /// Manually refresh the coins display
        /// </summary>
        public void UpdateCoinsDisplay()
        {
            RefreshCoinsDisplay();
        }

        /// <summary>
        /// Set the current category filter
        /// </summary>
        public void SetCategory(string category)
        {
            SelectCategory(category);
        }

        /// <summary>
        /// Perform a search for games
        /// </summary>
        public void SearchGames(string query)
        {
            if (searchField != null)
            {
                searchField.text = query;
            }
            OnSearchSubmitted(query);
        }

        /// <summary>
        /// Refresh the entire UI
        /// </summary>
        public void RefreshUI()
        {
            RefreshCoinsDisplay();
            SetupCategories();
            SetupFeaturedGames();
            RefreshGameList();
        }
        #endregion
    }
}