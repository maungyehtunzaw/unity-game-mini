using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages achievements and progression system for the game hub
/// </summary>
public class AchievementManager : MonoBehaviour
{
    [Header("Achievement Configuration")]
    public bool enableAchievements = true;
    public int maxAchievements = 100;
    
    [Header("Rewards")]
    public int achievementCoinReward = 25;
    public int rareAchievementCoinReward = 100;
    public int legendaryAchievementCoinReward = 250;

    public static AchievementManager Instance { get; private set; }

    private Dictionary<int, Achievement> _achievements = new Dictionary<int, Achievement>();
    private HashSet<int> _unlockedAchievements = new HashSet<int>();
    private Dictionary<string, int> _progressTrackers = new Dictionary<string, int>();

    // Events
    public System.Action<Achievement> OnAchievementUnlocked;
    public System.Action<Achievement, int> OnAchievementProgress;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetupEventListeners();
        LoadAchievementProgress();
    }

    void InitializeAchievements()
    {
        if (!enableAchievements) return;

        CreateAchievements();
        Debug.Log($"[AchievementManager] Initialized {_achievements.Count} achievements");
    }

    void CreateAchievements()
    {
        int id = 0;

        // Basic achievements
        AddAchievement(id++, "First Steps", "Play your first mini-game", AchievementRarity.Common, "games_played", 1);
        AddAchievement(id++, "Game Enthusiast", "Play 10 different mini-games", AchievementRarity.Common, "unique_games_played", 10);
        AddAchievement(id++, "Game Master", "Play 50 different mini-games", AchievementRarity.Rare, "unique_games_played", 50);
        AddAchievement(id++, "Legend", "Play 100 different mini-games", AchievementRarity.Legendary, "unique_games_played", 100);

        // Score achievements
        AddAchievement(id++, "High Scorer", "Score 1,000 points in any game", AchievementRarity.Common, "max_score", 1000);
        AddAchievement(id++, "Score King", "Score 10,000 points in any game", AchievementRarity.Rare, "max_score", 10000);
        AddAchievement(id++, "Score God", "Score 50,000 points in any game", AchievementRarity.Legendary, "max_score", 50000);

        // Coin achievements
        AddAchievement(id++, "Coin Collector", "Collect 1,000 coins", AchievementRarity.Common, "total_coins_earned", 1000);
        AddAchievement(id++, "Wealthy", "Collect 10,000 coins", AchievementRarity.Rare, "total_coins_earned", 10000);
        AddAchievement(id++, "Millionaire", "Collect 100,000 coins", AchievementRarity.Legendary, "total_coins_earned", 100000);

        // Streak achievements
        AddAchievement(id++, "Winning Streak", "Win 5 games in a row", AchievementRarity.Rare, "win_streak", 5);
        AddAchievement(id++, "Unstoppable", "Win 10 games in a row", AchievementRarity.Legendary, "win_streak", 10);

        // Time-based achievements
        AddAchievement(id++, "Marathon Player", "Play for 60 minutes total", AchievementRarity.Common, "total_play_time", 3600); // 60 minutes in seconds
        AddAchievement(id++, "Dedicated", "Play for 10 hours total", AchievementRarity.Rare, "total_play_time", 36000); // 10 hours
        AddAchievement(id++, "Addicted", "Play for 100 hours total", AchievementRarity.Legendary, "total_play_time", 360000); // 100 hours

        // Category-specific achievements
        foreach (var category in System.Enum.GetValues(typeof(GameCategory)).Cast<GameCategory>())
        {
            AddAchievement(id++, $"{category} Fan", $"Play 10 {category} games", AchievementRarity.Common, $"category_{category}_played", 10);
            AddAchievement(id++, $"{category} Expert", $"Master 5 {category} games", AchievementRarity.Rare, $"category_{category}_mastered", 5);
        }

        // Difficulty achievements
        AddAchievement(id++, "Easy Rider", "Complete 10 easy games", AchievementRarity.Common, "easy_games_completed", 10);
        AddAchievement(id++, "Challenge Accepted", "Complete 10 hard games", AchievementRarity.Rare, "hard_games_completed", 10);
        AddAchievement(id++, "Expert Level", "Complete 5 expert games", AchievementRarity.Legendary, "expert_games_completed", 5);

        // Perfect performance achievements
        AddAchievement(id++, "Perfectionist", "Get 3 stars in 10 games", AchievementRarity.Rare, "three_star_games", 10);
        AddAchievement(id++, "Flawless", "Get 3 stars in 50 games", AchievementRarity.Legendary, "three_star_games", 50);

        // Social achievements (if multiplayer is added)
        AddAchievement(id++, "Social Player", "Play 5 multiplayer games", AchievementRarity.Common, "multiplayer_games", 5);

        // Special achievements
        AddAchievement(id++, "Speed Demon", "Complete a game in under 30 seconds", AchievementRarity.Rare, "fastest_completion", 30);
        AddAchievement(id++, "Lucky Seven", "Score exactly 777 points", AchievementRarity.Rare, "lucky_seven", 1);
        AddAchievement(id++, "Night Owl", "Play between 12 AM and 6 AM", AchievementRarity.Common, "night_games", 1);
        AddAchievement(id++, "Early Bird", "Play between 6 AM and 9 AM", AchievementRarity.Common, "morning_games", 1);

        // Daily achievements
        AddAchievement(id++, "Daily Player", "Play every day for 7 days", AchievementRarity.Rare, "daily_streak", 7);
        AddAchievement(id++, "Consistent", "Play every day for 30 days", AchievementRarity.Legendary, "daily_streak", 30);

        // Meta achievements
        AddAchievement(id++, "Achievement Hunter", "Unlock 25 achievements", AchievementRarity.Rare, "achievements_unlocked", 25);
        AddAchievement(id++, "Completionist", "Unlock 50 achievements", AchievementRarity.Legendary, "achievements_unlocked", 50);
    }

    void AddAchievement(int id, string title, string description, AchievementRarity rarity, string progressKey, int targetValue)
    {
        var achievement = new Achievement
        {
            id = id,
            title = title,
            description = description,
            rarity = rarity,
            progressKey = progressKey,
            targetValue = targetValue,
            coinReward = GetCoinReward(rarity)
        };

        _achievements[id] = achievement;
    }

    int GetCoinReward(AchievementRarity rarity)
    {
        switch (rarity)
        {
            case AchievementRarity.Common: return achievementCoinReward;
            case AchievementRarity.Rare: return rareAchievementCoinReward;
            case AchievementRarity.Legendary: return legendaryAchievementCoinReward;
            default: return achievementCoinReward;
        }
    }

    void SetupEventListeners()
    {
        if (ServiceLocator.Bus != null)
        {
            ServiceLocator.Bus.OnMiniGameFinished += OnGameFinished;
            ServiceLocator.Bus.OnCoinsEarned += OnCoinsEarned;
        }

        if (ServiceLocator.Economy != null)
        {
            ServiceLocator.Economy.OnCoinsChanged += OnCoinsChanged;
        }
    }

    void LoadAchievementProgress()
    {
        var saveData = ServiceLocator.Save?.Data;
        if (saveData != null)
        {
            // Load unlocked achievements
            for (int i = 0; i < saveData.achievementsUnlocked.Length && i < maxAchievements; i++)
            {
                if (saveData.achievementsUnlocked[i])
                {
                    _unlockedAchievements.Add(i);
                }
            }
        }

        Debug.Log($"[AchievementManager] Loaded {_unlockedAchievements.Count} unlocked achievements");
    }

    void SaveAchievementProgress()
    {
        var saveData = ServiceLocator.Save?.Data;
        if (saveData != null)
        {
            // Ensure array is large enough
            if (saveData.achievementsUnlocked.Length < maxAchievements)
            {
                var newArray = new bool[maxAchievements];
                System.Array.Copy(saveData.achievementsUnlocked, newArray, saveData.achievementsUnlocked.Length);
                saveData.achievementsUnlocked = newArray;
            }

            // Save unlocked achievements
            foreach (int achievementId in _unlockedAchievements)
            {
                if (achievementId < saveData.achievementsUnlocked.Length)
                {
                    saveData.achievementsUnlocked[achievementId] = true;
                }
            }

            ServiceLocator.Save.Save();
        }
    }

    public void UpdateProgress(string progressKey, int value, bool isIncrement = true)
    {
        int newValue = value;
        
        if (isIncrement)
        {
            _progressTrackers.TryGetValue(progressKey, out int currentValue);
            newValue = currentValue + value;
        }
        
        _progressTrackers[progressKey] = newValue;

        // Check for achievement unlocks
        CheckAchievements(progressKey, newValue);
    }

    void CheckAchievements(string progressKey, int currentValue)
    {
        foreach (var achievement in _achievements.Values)
        {
            if (achievement.progressKey == progressKey && 
                !_unlockedAchievements.Contains(achievement.id) &&
                currentValue >= achievement.targetValue)
            {
                UnlockAchievement(achievement);
            }
            else if (achievement.progressKey == progressKey)
            {
                // Update progress event
                OnAchievementProgress?.Invoke(achievement, currentValue);
            }
        }
    }

    void UnlockAchievement(Achievement achievement)
    {
        _unlockedAchievements.Add(achievement.id);
        
        // Award coins
        ServiceLocator.Economy?.AddCoins(achievement.coinReward);
        
        // Fire events
        OnAchievementUnlocked?.Invoke(achievement);
        ServiceLocator.Bus?.PublishAchievementUnlocked(achievement.id);
        
        // Save progress
        SaveAchievementProgress();
        
        Debug.Log($"[AchievementManager] Achievement unlocked: {achievement.title} (+{achievement.coinReward} coins)");
    }

    // Event handlers
    void OnGameFinished(GameResult result)
    {
        // Track games played
        UpdateProgress("games_played", 1);
        UpdateProgress($"games_played_{result.gameId}", 1);
        
        // Track unique games
        if (GetProgress($"games_played_{result.gameId}") == 1)
        {
            UpdateProgress("unique_games_played", 1);
        }

        // Track high scores
        if (result.score > GetProgress("max_score"))
        {
            UpdateProgress("max_score", result.score, false);
        }

        // Track difficulty completions
        if (result.completed)
        {
            switch (result.difficulty)
            {
                case GameDifficulty.Easy:
                case GameDifficulty.VeryEasy:
                    UpdateProgress("easy_games_completed", 1);
                    break;
                case GameDifficulty.Hard:
                case GameDifficulty.VeryHard:
                    UpdateProgress("hard_games_completed", 1);
                    break;
                case GameDifficulty.Expert:
                    UpdateProgress("expert_games_completed", 1);
                    break;
            }
        }

        // Track star ratings
        if (result.starsEarned == 3)
        {
            UpdateProgress("three_star_games", 1);
        }

        // Track completion time
        if (result.duration < GetProgress("fastest_completion") || GetProgress("fastest_completion") == 0)
        {
            UpdateProgress("fastest_completion", Mathf.RoundToInt(result.duration), false);
        }

        // Check for lucky seven
        if (result.score == 777)
        {
            UpdateProgress("lucky_seven", 1);
        }

        // Track time of day
        var hour = System.DateTime.Now.Hour;
        if (hour >= 0 && hour < 6)
        {
            UpdateProgress("night_games", 1);
        }
        else if (hour >= 6 && hour < 9)
        {
            UpdateProgress("morning_games", 1);
        }

        // Track play time
        UpdateProgress("total_play_time", Mathf.RoundToInt(result.duration));
    }

    void OnCoinsEarned(int amount)
    {
        UpdateProgress("total_coins_earned", amount);
    }

    void OnCoinsChanged(int newTotal)
    {
        // Could track total coins owned if needed
    }

    // Public methods
    public List<Achievement> GetAllAchievements()
    {
        return _achievements.Values.ToList();
    }

    public List<Achievement> GetUnlockedAchievements()
    {
        return _achievements.Values.Where(a => _unlockedAchievements.Contains(a.id)).ToList();
    }

    public List<Achievement> GetLockedAchievements()
    {
        return _achievements.Values.Where(a => !_unlockedAchievements.Contains(a.id)).ToList();
    }

    public bool IsAchievementUnlocked(int achievementId)
    {
        return _unlockedAchievements.Contains(achievementId);
    }

    public int GetProgress(string progressKey)
    {
        return _progressTrackers.ContainsKey(progressKey) ? _progressTrackers[progressKey] : 0;
    }

    public float GetAchievementProgress(int achievementId)
    {
        if (_achievements.ContainsKey(achievementId))
        {
            var achievement = _achievements[achievementId];
            int currentProgress = GetProgress(achievement.progressKey);
            return Mathf.Clamp01((float)currentProgress / achievement.targetValue);
        }
        return 0f;
    }

    public int GetUnlockedCount()
    {
        return _unlockedAchievements.Count;
    }

    public int GetTotalCount()
    {
        return _achievements.Count;
    }

    void OnDestroy()
    {
        SaveAchievementProgress();
    }
}

[System.Serializable]
public class Achievement
{
    public int id;
    public string title;
    public string description;
    public AchievementRarity rarity;
    public string progressKey;
    public int targetValue;
    public int coinReward;
    public string iconName; // For UI
    
    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case AchievementRarity.Common: return Color.white;
            case AchievementRarity.Rare: return Color.blue;
            case AchievementRarity.Legendary: return Color.yellow;
            default: return Color.white;
        }
    }
}

public enum AchievementRarity
{
    Common,
    Rare,
    Legendary
}