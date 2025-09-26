using UnityEngine;

[System.Serializable]
public class SaveData 
{
    public int coins = 100; // Start with some coins
    public int totalGamesPlayed = 0;
    public int highestScore = 0;
    public string[] unlockedCategories = {"Basic"};
    public bool[] achievementsUnlocked = new bool[50]; // 50 achievements
    public int playerLevel = 1;
    public float totalPlayTime = 0f;
    public string lastPlayedGame = "";
    public bool removeAds = false;
    public bool premiumAccess = false;
    public int[] gameScores = new int[10000]; // Track high scores for all games
    public bool[] gameCompleted = new bool[10000]; // Track completion status
}

public class SaveService 
{
    const string KEY = "mega_hub_save_v2"; // Updated version
    SaveData _data;

    public SaveService() 
    { 
        Load(); 
    }

    public SaveData Data => _data;

    public void Load()
    {
        if(PlayerPrefs.HasKey(KEY))
        {
            try 
            {
                var json = PlayerPrefs.GetString(KEY);
                _data = JsonUtility.FromJson<SaveData>(json);
                Debug.Log($"[SaveService] Loaded save data - Coins: {_data.coins}, Games Played: {_data.totalGamesPlayed}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveService] Failed to load save data: {e.Message}");
                _data = new SaveData();
                Save();
            }
        } 
        else 
        {
            _data = new SaveData();
            Save();
            Debug.Log("[SaveService] Created new save data");
        }
    }

    public void Save()
    {
        try 
        {
            var json = JsonUtility.ToJson(_data);
            PlayerPrefs.SetString(KEY, json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveService] Failed to save data: {e.Message}");
        }
    }

    public void BackupSave()
    {
        // Create backup with timestamp
        var backupKey = $"{KEY}_backup_{System.DateTime.Now.Ticks}";
        var json = JsonUtility.ToJson(_data);
        PlayerPrefs.SetString(backupKey, json);
    }

    public void ResetSave()
    {
        BackupSave();
        _data = new SaveData();
        Save();
        Debug.Log("[SaveService] Save data reset");
    }
}