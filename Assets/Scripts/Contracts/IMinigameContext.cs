/// <summary>
/// Context interface that provides mini-games with access to hub services
/// </summary>
public interface IMinigameContext 
{
    /// <summary>
    /// Report game results to the hub
    /// </summary>
    void ReportResult(GameResult result);
    
    /// <summary>
    /// Report progress updates during gameplay
    /// </summary>
    void ReportProgress(float progress, string status = "");
    
    /// <summary>
    /// Request to pause the game
    /// </summary>
    void RequestPause();
    
    /// <summary>
    /// Request to resume the game
    /// </summary>
    void RequestResume();
    
    /// <summary>
    /// Request to quit the game and return to hub
    /// </summary>
    void RequestQuit();
    
    /// <summary>
    /// Show achievement notification
    /// </summary>
    void ShowAchievement(string achievementId, string title, string description);
    
    /// <summary>
    /// Request to show rewarded ad
    /// </summary>
    void ShowRewardedAd(System.Action<bool> onComplete);
    
    /// <summary>
    /// Play haptic feedback
    /// </summary>
    void PlayHaptic(HapticType type);
    
    /// <summary>
    /// Get player preferences/settings
    /// </summary>
    GameSettings GetPlayerSettings();
    
    /// <summary>
    /// Get current player stats
    /// </summary>
    PlayerStats GetPlayerStats();
    
    /// <summary>
    /// Check if game is unlocked
    /// </summary>
    bool IsGameUnlocked(string gameId);
    
    /// <summary>
    /// Get high score for this game
    /// </summary>
    int GetHighScore(string gameId);
    
    /// <summary>
    /// Save custom game data
    /// </summary>
    void SaveGameData(string key, string data);
    
    /// <summary>
    /// Load custom game data
    /// </summary>
    string LoadGameData(string key);
}

[System.Serializable]
public class PlayerStats
{
    public int totalGamesPlayed;
    public int totalScore;
    public float totalPlayTime;
    public int currentLevel;
    public int totalCoins;
    public int totalAchievements;
    public string favoriteCategory;
}

public enum HapticType
{
    Light,
    Medium,
    Heavy,
    Success,
    Warning,
    Error
}