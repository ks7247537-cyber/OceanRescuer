using UnityEngine;

/// <summary>
/// PlayerPrefs based save system for Ocean Rescuers.
/// Stores: level stars, unlock states, coins, gems.
/// Static class — no MonoBehaviour needed.
/// 
/// Indexing convention:
///   worldIndex: 0-based (0 = Coral Reef, 1 = Deep Ocean, 2 = Lost City)
///   levelIndex: 0-based (0 = Level 1, 1 = Level 2, ... 11 = Level 12)
/// </summary>
public static class SaveManager
{
    private const string KEY_COINS       = "Coins";
    private const string KEY_GEMS        = "Gems";
    private const string KEY_INITIALIZED = "SaveInitialized";
    private const string KEY_SAVE_VERSION = "SaveVersion_v3_ProgressionClean";

    // ── Energy Keys ─────────────────────────────────────────────────────────
    private const string KEY_ENERGY_CURRENT       = "Energy_Current";
    private const string KEY_ENERGY_LAST_REGEN    = "Energy_LastRegenTimestamp";
    private const string KEY_ENERGY_UNLIMITED_END = "Energy_UnlimitedEndTime";
    public const int     DEFAULT_MAX_ENERGY       = 30;

    static SaveManager()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// Guarantees that progress data is initialized cleanly on start.
    /// Wipes legacy dirty test keys once so only Level 1 is unlocked initially.
    /// </summary>
    public static void EnsureInitialized()
    {
        if (PlayerPrefs.GetInt(KEY_SAVE_VERSION, 0) == 0)
        {
            ResetLevelProgress(false);
            PlayerPrefs.SetInt(KEY_SAVE_VERSION, 1);
            PlayerPrefs.Save();
            Debug.Log("[SaveManager] Progression initialized fresh: Old test PlayerPrefs wiped. Only World 1, Level 1 unlocked.");
        }
    }

    public static void InitializeDefaultData()
    {
        EnsureInitialized();
    }

    public static void SaveLevelStars(int worldIndex, int levelIndex, int stars)
    {
        string key = LevelStarsKey(worldIndex, levelIndex);
        int existing = PlayerPrefs.GetInt(key, 0);
        if (stars > existing)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
            Debug.Log($"[SaveManager] Saved stars for W{worldIndex + 1}_L{levelIndex + 1}: {stars}");
        }
    }

    public static int GetLevelStars(int worldIndex, int levelIndex)
        => PlayerPrefs.GetInt(LevelStarsKey(worldIndex, levelIndex), 0);

    public static bool IsLevelUnlocked(int worldIndex, int levelIndex)
    {
        // World 1, Level 1 (0, 0) is always unlocked
        if (worldIndex == 0 && levelIndex == 0) return true;
        return PlayerPrefs.GetInt(LevelUnlockKey(worldIndex, levelIndex), 0) == 1;
    }

    public static void UnlockLevel(int worldIndex, int levelIndex)
    {
        // Completing level 12 (index 11) unlocks next world and its level 1
        if (levelIndex >= 12)
        {
            int nextWorld = worldIndex + 1;
            UnlockWorld(nextWorld);
            PlayerPrefs.SetInt(LevelUnlockKey(nextWorld, 0), 1);
            PlayerPrefs.Save();
            Debug.Log($"[SaveManager] World {nextWorld + 1} and Level 1 unlocked!");
            return;
        }

        PlayerPrefs.SetInt(LevelUnlockKey(worldIndex, levelIndex), 1);
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] Level unlocked: World {worldIndex + 1}, Level {levelIndex + 1}");
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
        Debug.Log($"[SaveManager] World unlocked: World {worldIndex + 1}");
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

    // ── Power-Ups Inventory ─────────────────────────────────────────────────
    public static int GetPowerUpCount(string powerUpType) => PlayerPrefs.GetInt("PowerUp_" + powerUpType, 0);
    public static void AddPowerUp(string powerUpType, int count = 1)
    {
        int current = GetPowerUpCount(powerUpType);
        PlayerPrefs.SetInt("PowerUp_" + powerUpType, Mathf.Max(0, current + count));
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] PowerUp {powerUpType} updated to {GetPowerUpCount(powerUpType)}");
    }

    // ── Energy System Storage ───────────────────────────────────────────────
    public static int GetEnergy() => PlayerPrefs.GetInt(KEY_ENERGY_CURRENT, DEFAULT_MAX_ENERGY);
    public static void SaveEnergy(int energy)
    {
        PlayerPrefs.SetInt(KEY_ENERGY_CURRENT, Mathf.Max(0, energy));
        PlayerPrefs.Save();
    }

    public static long GetLastRegenTimestamp()
    {
        string s = PlayerPrefs.GetString(KEY_ENERGY_LAST_REGEN, "");
        if (long.TryParse(s, out long val)) return val;
        long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString(KEY_ENERGY_LAST_REGEN, now.ToString());
        PlayerPrefs.Save();
        return now;
    }

    public static void SaveLastRegenTimestamp(long unixSeconds)
    {
        PlayerPrefs.SetString(KEY_ENERGY_LAST_REGEN, unixSeconds.ToString());
        PlayerPrefs.Save();
    }

    public static long GetUnlimitedEnergyEndTime()
    {
        string s = PlayerPrefs.GetString(KEY_ENERGY_UNLIMITED_END, "0");
        return long.TryParse(s, out long val) ? val : 0;
    }

    public static void SaveUnlimitedEnergyEndTime(long unixSeconds)
    {
        PlayerPrefs.SetString(KEY_ENERGY_UNLIMITED_END, unixSeconds.ToString());
        PlayerPrefs.Save();
    }

    // ── Reward Claim Tracking ───────────────────────────────────────────────
    public static string LevelRewardClaimedKey(int w, int l) => $"RewardClaimed_W{w}_L{l}";
    public static string Level3StarRewardKey(int w, int l)   => $"3StarRewardClaimed_W{w}_L{l}";
    public static string WorldRewardClaimedKey(int w)        => $"WorldRewardClaimed_W{w}";

    public static bool HasClaimedLevelReward(int w, int l)
        => PlayerPrefs.GetInt(LevelRewardClaimedKey(w, l), 0) == 1;

    public static void SetLevelRewardClaimed(int w, int l, bool claimed)
    {
        PlayerPrefs.SetInt(LevelRewardClaimedKey(w, l), claimed ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool HasClaimed3StarReward(int w, int l)
        => PlayerPrefs.GetInt(Level3StarRewardKey(w, l), 0) == 1;

    public static void Set3StarRewardClaimed(int w, int l, bool claimed)
    {
        PlayerPrefs.SetInt(Level3StarRewardKey(w, l), claimed ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool HasClaimedWorldReward(int w)
        => PlayerPrefs.GetInt(WorldRewardClaimedKey(w), 0) == 1;

    public static void SetWorldRewardClaimed(int w, bool claimed)
    {
        PlayerPrefs.SetInt(WorldRewardClaimedKey(w), claimed ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static void SetSelectedWorld(int w) { PlayerPrefs.SetInt("SelectedWorld", Mathf.Max(0, w)); PlayerPrefs.Save(); }
    public static int  GetSelectedWorld()       => PlayerPrefs.GetInt("SelectedWorld", 0);
    public static void SetSelectedLevel(int l)  { PlayerPrefs.SetInt("SelectedLevel", Mathf.Max(0, l)); PlayerPrefs.Save(); }
    public static int  GetSelectedLevel()       => PlayerPrefs.GetInt("SelectedLevel", 0);

    public static string LevelStarsKey(int w, int l)  => $"Stars_W{w}_L{l}";
    public static string LevelUnlockKey(int w, int l) => $"Unlocked_W{w}_L{l}";

    /// <summary>
    /// Resets all level unlocks and star progression back to default (only Level 1 unlocked).
    /// Safe for testing. Does not wipe coins/gems unless resetCurrency is true.
    /// </summary>
    public static void ResetLevelProgress(bool resetCurrency = false, int worldCount = 3, int levelsPerWorld = 12)
    {
        for (int w = 0; w < worldCount; w++)
        {
            for (int l = 0; l < levelsPerWorld; l++)
            {
                PlayerPrefs.DeleteKey(LevelStarsKey(w, l));
                PlayerPrefs.DeleteKey(LevelUnlockKey(w, l));
                PlayerPrefs.DeleteKey(LevelRewardClaimedKey(w, l));
                PlayerPrefs.DeleteKey(Level3StarRewardKey(w, l));
                // Clean any alternate legacy key naming
                PlayerPrefs.DeleteKey($"Level_{l + 1}_Unlocked");
                PlayerPrefs.DeleteKey($"Level_{l}_Unlocked");
            }
            if (w > 0) PlayerPrefs.DeleteKey($"World_Unlocked_{w}");
            PlayerPrefs.DeleteKey(WorldRewardClaimedKey(w));
        }

        if (resetCurrency)
        {
            PlayerPrefs.SetInt(KEY_COINS, 0);
            PlayerPrefs.SetInt(KEY_GEMS,  0);
        }

        // Only World 0, Level 0 is unlocked
        PlayerPrefs.SetInt(LevelUnlockKey(0, 0), 1);
        PlayerPrefs.SetInt("World_Unlocked_0", 1);
        PlayerPrefs.SetInt("SelectedWorld", 0);
        PlayerPrefs.SetInt("SelectedLevel", 0);
        PlayerPrefs.SetInt(KEY_INITIALIZED, 1);
        PlayerPrefs.SetInt(KEY_SAVE_VERSION, 1);
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] Level progress reset: only Level 1 is unlocked.");
    }

    /// <summary>
    /// Unlocks all levels in all worlds for testing.
    /// </summary>
    public static void UnlockAllLevels(int worldCount = 3, int levelsPerWorld = 12)
    {
        for (int w = 0; w < worldCount; w++)
        {
            PlayerPrefs.SetInt($"World_Unlocked_{w}", 1);
            for (int l = 0; l < levelsPerWorld; l++)
            {
                PlayerPrefs.SetInt(LevelUnlockKey(w, l), 1);
            }
        }
        PlayerPrefs.Save();
        Debug.Log("[SaveManager] All levels unlocked for testing.");
    }
}
