using System;
using UnityEngine;

[System.Serializable]
public class GameResult 
{
    public string gameId;
    public int score;
    public int coinsAwarded;
    public float duration;
    public bool completed;
    public DateTime timestamp;
    public GameDifficulty difficulty;
    public int starsEarned; // 1-3 star rating
    public bool newRecord;
    public string[] achievementsUnlocked;
    public int experienceGained;
    
    public GameResult()
    {
        timestamp = DateTime.Now;
        achievementsUnlocked = new string[0];
    }
    
    public GameResult(string id, int finalScore, int coins, float time, bool isCompleted = true)
    {
        gameId = id;
        score = finalScore;
        coinsAwarded = coins;
        duration = time;
        completed = isCompleted;
        timestamp = DateTime.Now;
        achievementsUnlocked = new string[0];
        
        // Calculate stars based on score (can be overridden by individual games)
        CalculateStars();
    }
    
    private void CalculateStars()
    {
        if (!completed)
        {
            starsEarned = 0;
            return;
        }
        
        // Basic star calculation - games can override this
        if (score >= 1000) starsEarned = 3;
        else if (score >= 500) starsEarned = 2;
        else if (score > 0) starsEarned = 1;
        else starsEarned = 0;
    }
    
    public void SetCustomStarRating(int stars)
    {
        starsEarned = Mathf.Clamp(stars, 0, 3);
    }
}

[System.Serializable]
public class GameInfo
{
    public string gameId;
    public string displayName;
    public string description;
    public GameCategory category;
    public GameDifficulty difficulty;
    public float estimatedPlayTime; // in minutes
    public bool isUnlocked;
    public bool supportsMultiplayer;
    public int maxPlayers;
    public string[] tags;
    
    public GameInfo(string id, string name, GameCategory cat = GameCategory.Casual)
    {
        gameId = id;
        displayName = name;
        category = cat;
        difficulty = GameDifficulty.Easy;
        estimatedPlayTime = 2f;
        isUnlocked = true;
        supportsMultiplayer = false;
        maxPlayers = 1;
        tags = new string[0];
    }
}

public enum GameCategory
{
    Puzzle,
    Arcade,
    Casual,
    Action,
    Strategy,
    Educational,
    Memory,
    Reflex,
    Sports,
    Racing
}

public enum GameDifficulty
{
    VeryEasy,
    Easy,
    Medium,
    Hard,
    VeryHard,
    Expert
}

public enum GameMode
{
    Normal,
    TimeAttack,
    Survival,
    Endless,
    Challenge,
    Zen
}

[System.Serializable]
public class GameSettings
{
    public float soundVolume = 1f;
    public float musicVolume = 1f;
    public bool vibrationEnabled = true;
    public bool tutorialEnabled = true;
    public int controlScheme = 0; // Different input methods
    public bool accessibilityMode = false;
}