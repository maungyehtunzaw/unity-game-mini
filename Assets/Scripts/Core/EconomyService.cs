using System;
using UnityEngine;

public class EconomyService 
{
    readonly SaveService _save;
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnLevelUp;

    public EconomyService(SaveService save)
    { 
        _save = save; 
    }

    public int Coins => _save.Data.coins;
    public int PlayerLevel => _save.Data.playerLevel;
    public int TotalGamesPlayed => _save.Data.totalGamesPlayed;

    public void AddCoins(int amount)
    {
        if(amount <= 0) return;
        
        int previousLevel = _save.Data.playerLevel;
        _save.Data.coins += amount;
        
        // Level up system based on coins earned
        int newLevel = Mathf.FloorToInt(_save.Data.coins / 1000f) + 1;
        if(newLevel > _save.Data.playerLevel)
        {
            _save.Data.playerLevel = newLevel;
            OnLevelUp?.Invoke(newLevel);
            Debug.Log($"[EconomyService] Player leveled up to {newLevel}!");
        }
        
        _save.Save();
        OnCoinsChanged?.Invoke(_save.Data.coins);
        Debug.Log($"[EconomyService] Added {amount} coins. Total: {_save.Data.coins}");
    }

    public bool SpendCoins(int amount)
    {
        if(amount <= 0) return true;
        if(_save.Data.coins < amount) 
        {
            Debug.Log($"[EconomyService] Not enough coins. Need: {amount}, Have: {_save.Data.coins}");
            return false;
        }
        
        _save.Data.coins -= amount;
        _save.Save();
        OnCoinsChanged?.Invoke(_save.Data.coins);
        Debug.Log($"[EconomyService] Spent {amount} coins. Remaining: {_save.Data.coins}");
        return true;
    }

    public void RecordGamePlayed(string gameId, int score)
    {
        _save.Data.totalGamesPlayed++;
        
        if(score > _save.Data.highestScore)
        {
            _save.Data.highestScore = score;
        }
        
        _save.Data.lastPlayedGame = gameId;
        _save.Save();
    }

    public void RecordPlayTime(float sessionTime)
    {
        _save.Data.totalPlayTime += sessionTime;
        _save.Save();
    }

    public bool HasRemoveAds() => _save.Data.removeAds;
    
    public void SetRemoveAds(bool value)
    {
        _save.Data.removeAds = value;
        _save.Save();
    }

    public bool HasPremiumAccess() => _save.Data.premiumAccess;
    
    public void SetPremiumAccess(bool value)
    {
        _save.Data.premiumAccess = value;
        _save.Save();
    }

    public int GetCoins() => _save.Data.coins;

    public void Cleanup()
    {
        // Any cleanup needed
    }
}