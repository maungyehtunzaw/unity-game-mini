using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName="MegaHub/GameRegistry")]
public class GameRegistry : ScriptableObject 
{
    [Header("All Games")]
    public List<MiniGameDef> games = new List<MiniGameDef>();
    
    [Header("Featured Games")]
    public List<MiniGameDef> featuredGames = new List<MiniGameDef>();
    
    [Header("Categories")]
    public List<CategoryInfo> categories = new List<CategoryInfo>();

    // Cache for faster lookups
    private Dictionary<string, MiniGameDef> _gameCache;
    private Dictionary<GameCategory, List<MiniGameDef>> _categoryCache;

    void OnEnable()
    {
        RefreshCache();
    }

    public void RefreshCache()
    {
        _gameCache = new Dictionary<string, MiniGameDef>();
        _categoryCache = new Dictionary<GameCategory, List<MiniGameDef>>();

        foreach (var game in games)
        {
            if (game != null && !string.IsNullOrEmpty(game.gameId))
            {
                _gameCache[game.gameId] = game;
                
                if (!_categoryCache.ContainsKey(game.category))
                {
                    _categoryCache[game.category] = new List<MiniGameDef>();
                }
                _categoryCache[game.category].Add(game);
            }
        }
    }

    public MiniGameDef GetById(string id) 
    {
        if (_gameCache == null) RefreshCache();
        return _gameCache.ContainsKey(id) ? _gameCache[id] : null;
    }

    public List<MiniGameDef> GetByCategory(GameCategory category)
    {
        if (_categoryCache == null) RefreshCache();
        return _categoryCache.ContainsKey(category) ? _categoryCache[category] : new List<MiniGameDef>();
    }

    public List<MiniGameDef> GetUnlockedGames()
    {
        return games.Where(game => game != null && game.IsUnlocked()).ToList();
    }

    public List<MiniGameDef> GetNewGames()
    {
        return games.Where(game => game != null && game.newGame).ToList();
    }

    public List<MiniGameDef> GetFeaturedGames()
    {
        if (featuredGames.Count > 0)
            return featuredGames.Where(game => game != null && game.IsUnlocked()).ToList();
        
        // If no manually set featured games, return top-rated or random selection
        return games.Where(game => game != null && game.featured && game.IsUnlocked())
                   .OrderBy(game => game.priority)
                   .Take(6)
                   .ToList();
    }

    public List<MiniGameDef> SearchGames(string searchTerm, GameCategory? category = null)
    {
        if (string.IsNullOrEmpty(searchTerm)) 
            return category.HasValue ? GetByCategory(category.Value) : GetUnlockedGames();

        var results = games.Where(game => game != null && game.IsUnlocked()).ToList();

        // Filter by category if specified
        if (category.HasValue)
        {
            results = results.Where(game => game.category == category.Value).ToList();
        }

        // Search in name, description, and tags
        results = results.Where(game => 
            game.displayName.ToLower().Contains(searchTerm.ToLower()) ||
            game.description.ToLower().Contains(searchTerm.ToLower()) ||
            (game.tags != null && game.tags.Any(tag => tag.ToLower().Contains(searchTerm.ToLower())))
        ).ToList();

        return results.OrderBy(game => game.priority).ToList();
    }

    public List<MiniGameDef> GetRandomGames(int count, GameCategory? category = null)
    {
        var availableGames = category.HasValue ? GetByCategory(category.Value) : GetUnlockedGames();
        return availableGames.Where(game => game.IsUnlocked())
                           .OrderBy(x => Random.value)
                           .Take(count)
                           .ToList();
    }

    public List<MiniGameDef> GetRecommendedGames(string lastPlayedGameId, int count = 5)
    {
        var lastGame = GetById(lastPlayedGameId);
        if (lastGame == null)
            return GetRandomGames(count);

        // Recommend games from same category first
        var sameCategory = GetByCategory(lastGame.category)
                          .Where(game => game.gameId != lastPlayedGameId && game.IsUnlocked())
                          .Take(count / 2)
                          .ToList();

        // Fill remaining with random games from other categories
        var otherGames = GetRandomGames(count - sameCategory.Count);
        
        sameCategory.AddRange(otherGames);
        return sameCategory.Take(count).ToList();
    }

    public int GetTotalGameCount() => games.Count;
    public int GetUnlockedGameCount() => games.Count(game => game != null && game.IsUnlocked());
    
    public float GetProgressPercentage()
    {
        if (games.Count == 0) return 0f;
        return (float)GetUnlockedGameCount() / games.Count * 100f;
    }

    // Editor utility methods
    #if UNITY_EDITOR
    [ContextMenu("Auto-populate from project")]
    public void AutoPopulateGames()
    {
        games.Clear();
        
        // Find all MiniGameDef assets in the project
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:MiniGameDef");
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            MiniGameDef game = UnityEditor.AssetDatabase.LoadAssetAtPath<MiniGameDef>(path);
            
            if (game != null && !games.Contains(game))
            {
                games.Add(game);
            }
        }
        
        RefreshCache();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Auto-populated {games.Count} games");
    }

    [ContextMenu("Sort games by priority")]
    public void SortGamesByPriority()
    {
        games = games.OrderBy(game => game.priority).ThenBy(game => game.displayName).ToList();
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Validate all games")]
    public void ValidateAllGames()
    {
        int invalidCount = 0;
        
        foreach (var game in games)
        {
            if (game == null)
            {
                Debug.LogError("Null game found in registry");
                invalidCount++;
                continue;
            }

            if (string.IsNullOrEmpty(game.gameId))
            {
                Debug.LogError($"Game {game.name} has empty gameId");
                invalidCount++;
            }

            if (string.IsNullOrEmpty(game.prefabKey))
            {
                Debug.LogError($"Game {game.gameId} has empty prefabKey");
                invalidCount++;
            }
        }
        
        Debug.Log($"Validation complete. Found {invalidCount} issues out of {games.Count} games");
    }
    #endif
}

[System.Serializable]
public class CategoryInfo
{
    public GameCategory category;
    public string displayName;
    public string description;
    public Sprite icon;
    public Color themeColor = Color.white;
    public bool unlocked = true;
}