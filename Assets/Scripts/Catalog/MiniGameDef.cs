using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName="MegaHub/MiniGameDef")]
public class MiniGameDef : ScriptableObject 
{
    [Header("Basic Info")]
    public string gameId;
    public string displayName;
    public string description;
    public Sprite icon;
    
    [Header("Loading")]
    [Tooltip("Addressables key or Resources path to prefab implementing IMinigame")]
    public string prefabKey;
    public bool useAddressables = true;
    
    [Header("Game Properties")]
    public GameCategory category = GameCategory.Casual;
    public GameDifficulty baseDifficulty = GameDifficulty.Easy;
    [Range(0, 1000)] public int baseCoinReward = 10;
    [Range(1, 60)] public float estimatedPlayTime = 3f; // minutes
    
    [Header("Unlock Requirements")]
    public bool unlockedByDefault = true;
    public int requiredLevel = 1;
    public int requiredCoins = 0;
    public string[] prerequisiteGames;
    
    [Header("Variations")]
    public bool hasVariations = false;
    public GameVariation[] variations;
    
    [Header("Tags and Metadata")]
    public string[] tags;
    public bool featured = false;
    public bool newGame = false;
    public int priority = 0; // For sorting
    
    [Header("Multiplayer Support")]
    public bool supportsMultiplayer = false;
    [Range(1, 8)] public int maxPlayers = 1;
    
    [Header("Accessibility")]
    public bool colorBlindFriendly = false;
    public bool supportsVoiceOver = false;
    public bool oneHandedPlayable = false;

    public bool IsUnlocked()
    {
        if (unlockedByDefault) return true;
        
        var economy = ServiceLocator.Economy;
        if (economy.PlayerLevel < requiredLevel) return false;
        if (economy.Coins < requiredCoins) return false;
        
        // Check prerequisite games
        if (prerequisiteGames != null && prerequisiteGames.Length > 0)
        {
            var save = ServiceLocator.Save;
            foreach (string prereq in prerequisiteGames)
            {
                // Check if prerequisite game is completed
                // This would need to be implemented based on your save system
            }
        }
        
        return true;
    }
    
    public int GetCoinReward(int score, GameDifficulty difficulty)
    {
        float multiplier = 1f;
        
        // Difficulty multiplier
        switch (difficulty)
        {
            case GameDifficulty.VeryEasy: multiplier *= 0.5f; break;
            case GameDifficulty.Easy: multiplier *= 1f; break;
            case GameDifficulty.Medium: multiplier *= 1.5f; break;
            case GameDifficulty.Hard: multiplier *= 2f; break;
            case GameDifficulty.VeryHard: multiplier *= 2.5f; break;
            case GameDifficulty.Expert: multiplier *= 3f; break;
        }
        
        // Score-based bonus
        float scoreBonus = Mathf.Log10(score + 1) * 0.1f;
        
        return Mathf.RoundToInt((baseCoinReward + scoreBonus) * multiplier);
    }
}

[System.Serializable]
public class GameVariation
{
    public string variationId;
    public string displayName;
    public string description;
    public Sprite variationIcon;
    public GameDifficulty difficulty;
    public float rewardMultiplier = 1f;
    public string[] modifiers; // Custom game modifiers
}