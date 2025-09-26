using System;
using System.Collections.Generic;
using UnityEngine;

public class EventBus 
{
    // Mini-game events
    public event Action<GameResult> OnMiniGameStarted;
    public event Action<GameResult> OnMiniGameFinished;
    public event Action<string> OnGameUnlocked;
    
    // Economy events
    public event Action<int> OnCoinsEarned;
    public event Action<int> OnCoinsSpent;
    public event Action<int> OnLevelUp;
    
    // Achievement events
    public event Action<int> OnAchievementUnlocked;
    
    // Performance events
    public event Action<string> OnPerformanceWarning;
    
    // UI events
    public event Action OnReturnToHub;
    public event Action<string> OnCategorySelected;

    private Dictionary<Type, List<Delegate>> _eventCallbacks = new Dictionary<Type, List<Delegate>>();

    public void PublishMiniGameStarted(GameResult result) 
    {
        OnMiniGameStarted?.Invoke(result);
        Debug.Log($"[EventBus] Game started: {result.gameId}");
    }
    
    public void PublishMiniGameFinished(GameResult result) 
    {
        OnMiniGameFinished?.Invoke(result);
        Debug.Log($"[EventBus] Game finished: {result.gameId}, Score: {result.score}");
    }

    public void PublishGameUnlocked(string gameId)
    {
        OnGameUnlocked?.Invoke(gameId);
        Debug.Log($"[EventBus] Game unlocked: {gameId}");
    }

    public void PublishCoinsEarned(int amount)
    {
        OnCoinsEarned?.Invoke(amount);
    }

    public void PublishCoinsSpent(int amount)
    {
        OnCoinsSpent?.Invoke(amount);
    }

    public void PublishLevelUp(int newLevel)
    {
        OnLevelUp?.Invoke(newLevel);
        Debug.Log($"[EventBus] Level up! New level: {newLevel}");
    }

    public void PublishAchievementUnlocked(int achievementId)
    {
        OnAchievementUnlocked?.Invoke(achievementId);
        Debug.Log($"[EventBus] Achievement unlocked: {achievementId}");
    }

    public void PublishPerformanceWarning(string message)
    {
        OnPerformanceWarning?.Invoke(message);
        Debug.LogWarning($"[EventBus] Performance Warning: {message}");
    }

    public void PublishReturnToHub()
    {
        OnReturnToHub?.Invoke();
    }

    public void PublishCategorySelected(string category)
    {
        OnCategorySelected?.Invoke(category);
    }

    // Generic event system for extensibility
    public void Subscribe<T>(Action<T> callback) where T : class
    {
        var eventType = typeof(T);
        if (!_eventCallbacks.ContainsKey(eventType))
        {
            _eventCallbacks[eventType] = new List<Delegate>();
        }
        _eventCallbacks[eventType].Add(callback);
    }

    public void Unsubscribe<T>(Action<T> callback) where T : class
    {
        var eventType = typeof(T);
        if (_eventCallbacks.ContainsKey(eventType))
        {
            _eventCallbacks[eventType].Remove(callback);
        }
    }

    public void Publish<T>(T eventData) where T : class
    {
        var eventType = typeof(T);
        if (_eventCallbacks.ContainsKey(eventType))
        {
            foreach (var callback in _eventCallbacks[eventType])
            {
                if (callback is Action<T> typedCallback)
                {
                    typedCallback.Invoke(eventData);
                }
            }
        }
    }

    public void Cleanup()
    {
        OnMiniGameStarted = null;
        OnMiniGameFinished = null;
        OnGameUnlocked = null;
        OnCoinsEarned = null;
        OnCoinsSpent = null;
        OnLevelUp = null;
        OnAchievementUnlocked = null;
        OnPerformanceWarning = null;
        OnReturnToHub = null;
        OnCategorySelected = null;
        _eventCallbacks.Clear();
    }
}