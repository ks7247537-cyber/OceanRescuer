using UnityEngine;

/// <summary>
/// PlayerPrefs based save system for Ocean Rescuers.
/// Stores: level stars, unlock states, coins, gems.
/// Static class — no MonoBehaviour needed.
/// </summary>
public static class SaveManager
{
    private const string KEY_COINS       = "Coins";
    private const string KEY_GEMS        = "Gems";
    private const string KEY_INITIALIZED = "SaveInitialized";

    public static void InitializeDefaultData()
    {
        if (PlayerPrefs.GetInt(KEY_INITIALIZED, 0) == 1) return;
        PlayerPrefs.SetInt(LevelUnlockKey(0, 0), 1);
        PlayerPrefs.SetInt(KEY_COINS, 0);
        PlayerPrefs.SetInt(KEY_GEMS,  0);
        PlayerPrefs.SetInt(KEY_INITIALIZED, 1);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Default data initialized.");
    }

    public static void SaveLevelStars(int worldIndex, int levelIndex, int stars)
    {
        string key = LevelStarsKey(worldIndex, levelIndex);
        int existing = PlayerPrefs.GetInt(key, 0);
        if (stars > existing) { PlayerPrefs.SetInt(key, stars); PlayerPrefs.Save(); }
    }

    public static int GetLevelStars(int worldIndex, int levelIndex)
        => PlayerPrefs.GetInt(LevelStarsKey(worldIndex, levelIndex), 0);

    public static bool IsLevelUnlocked(int worldIndex, int levelIndex)
    {
        if (worldIndex == 0 && levelIndex == 0) return true;
        return PlayerPrefs.GetInt(LevelUnlockKey(worldIndex, levelIndex), 0) == 1;
    }

    public static void UnlockLevel(int worldIndex, int levelIndex)
    {
        PlayerPrefs.SetInt(LevelUnlockKey(worldIndex, levelIndex), 1);
        PlayerPrefs.Save();
    }

    public static bool IsWorldUnlocked(int worldIndex)
    {
        if (worldIndex == 0) return true;
        return PlayerPrefs.GetInt($"World_Unlocked_{worldIndex}", 0) == 1;
    }

    public static void UnlockWorld(int worldIndex)
    {
        PlayerPrefs.SetInt($"World_Unlocked_{worldIndex}", 1);
        PlayerPrefs.Save();
    }

    public static void SaveCoins(int coins) { PlayerPrefs.SetInt(KEY_COINS, Mathf.Max(0, coins)); PlayerPrefs.Save(); }
    public static int  GetCoins()    => PlayerPrefs.GetInt(KEY_COINS, 0);
    public static void AddCoins(int amount) => SaveCoins(GetCoins() + amount);
    public static bool SpendCoins(int amount)
    {
        int c = GetCoins(); if (c < amount) return false;
        SaveCoins(c - amount); return true;
    }

    public static void SaveGems(int gems) { PlayerPrefs.SetInt(KEY_GEMS, Mathf.Max(0, gems)); PlayerPrefs.Save(); }
    public static int  GetGems()     => PlayerPrefs.GetInt(KEY_GEMS, 0);

    public static void SetSelectedWorld(int w) => PlayerPrefs.SetInt("SelectedWorld", w);
    public static int  GetSelectedWorld()       => PlayerPrefs.GetInt("SelectedWorld", 0);
    public static void SetSelectedLevel(int l)  => PlayerPrefs.SetInt("SelectedLevel", l);
    public static int  GetSelectedLevel()       => PlayerPrefs.GetInt("SelectedLevel", 0);

    private static string LevelStarsKey(int w, int l)  => $"Stars_W{w}_L{l}";
    private static string LevelUnlockKey(int w, int l) => $"Unlocked_W{w}_L{l}";
}
