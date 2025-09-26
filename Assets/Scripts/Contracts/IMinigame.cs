using UnityEngine;

/// <summary>
/// Interface that all mini-games must implement to integrate with the hub system
/// </summary>
public interface IMinigame 
{
    /// <summary>
    /// Initialize the mini-game with context
    /// </summary>
    void Init(IMinigameContext ctx);
    
    /// <summary>
    /// Start the game session
    /// </summary>
    void StartGame();
    
    /// <summary>
    /// Stop/pause the game
    /// </summary>
    void StopGame();
    
    /// <summary>
    /// Cleanup resources when game is unloaded
    /// </summary>
    void CleanupGame();
    
    /// <summary>
    /// Get current game state for saving/resuming
    /// </summary>
    string GetGameState();
    
    /// <summary>
    /// Load saved game state
    /// </summary>
    void LoadGameState(string state);
    
    /// <summary>
    /// Get game information
    /// </summary>
    GameInfo GetGameInfo();
}

/// <summary>
/// Extended interface for games that support different difficulty levels
/// </summary>
public interface IConfigurableMinigame : IMinigame
{
    void SetDifficulty(GameDifficulty difficulty);
    void SetGameMode(GameMode mode);
    void ApplyCustomSettings(GameSettings settings);
}

/// <summary>
/// Interface for games that support multiplayer
/// </summary>
public interface IMultiplayerMinigame : IMinigame
{
    void SetupMultiplayer(int playerCount);
    void OnPlayerAction(int playerId, object action);
    bool IsMultiplayerSupported { get; }
}