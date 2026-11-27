using UnityEngine;

/// <summary>
/// ScriptableObject defining independent configuration for each World in Ocean Rescuers.
/// Isolates world identity, level data resource path, background, atmosphere,
/// default companion, unlock requirements, and completion rewards.
/// </summary>
[CreateAssetMenu(fileName = "WorldConfig", menuName = "OceanRescuer/World Config")]
public class WorldConfig : ScriptableObject
{
    [Header("World Identity")]
    [Tooltip("Zero-based world index (0=Coral Reef, 1=Deep Ocean, 2=Lost City).")]
    public int worldIndex = 0;

    [Tooltip("Unique string identifier for this world.")]
    public string worldId = "coral_reef";

    [Tooltip("User-facing world display name.")]
    public string displayName = "Coral Reef";

    [TextArea(2, 4)]
    public string description = "Vibrant sunlit shallow waters with gentle currents and colorful coral reefs.";

    [Header("Level Data Storage")]
    [Tooltip("Resources path where this world's LevelData assets reside (e.g. 'Levels/' or 'Levels/DeepOcean/').")]
    public string levelResourcePath = "Levels/";

    [Tooltip("Total number of levels in this world.")]
    public int totalLevels = 12;

    [Header("Visual Theme & Background")]
    [Tooltip("Background sprite displayed in Gameplay and Level Selection for this world.")]
    public Sprite backgroundSprite;

    [Tooltip("Wide banner sprite used in headers and world selection cards.")]
    public Sprite bannerSprite;

    [Tooltip("Card artwork used in World Selection.")]
    public Sprite worldCardSprite;

    [Tooltip("Primary theme accent color for UI highlights.")]
    public Color themeColor = new Color(0.20f, 0.75f, 0.95f, 1f);

    [Header("Companion & Mechanics")]
    [Tooltip("Default companion creature assigned in this world if not overridden by LevelData.")]
    public CreatureType defaultCreature = CreatureType.Clownfish;

    [Tooltip("Whether interactive companion abilities (Turtle, Octopus, Jellyfish) are active in this world.")]
    public bool allowsCreatureAbilities = false;

    [Tooltip("Whether deep ocean atmosphere effects (caustics, ambient light rays) are enabled.")]
    public bool hasAtmosphereEffects = false;

    [Header("Unlock Requirements")]
    [Tooltip("World index required to be completed before this world unlocks (-1 = always unlocked).")]
    public int unlockRequiredWorld = -1;

    [Tooltip("Minimum stars required across preceding worlds to unlock (0 = no star requirement).")]
    public int unlockRequiredStars = 0;

    [Header("World Completion Rewards")]
    public int worldClearCoins = 1000;
    public int worldClearGems = 50;

    // Cache to prevent repetitive Resources.Load calls
    private static WorldConfig[] _cachedConfigs = new WorldConfig[3];

    public const int TOTAL_WORLDS = 3;

    /// <summary>
    /// Safe accessor that returns the WorldConfig for the specified worldIndex.
    /// Never returns null (falls back to a default programmatic configuration if asset is missing).
    /// </summary>
    public static WorldConfig GetConfig(int worldIndex)
    {
        worldIndex = Mathf.Clamp(worldIndex, 0, TOTAL_WORLDS - 1);

        if (_cachedConfigs[worldIndex] != null)
        {
            return _cachedConfigs[worldIndex];
        }

        // Try loading from Resources (matching both naming schemes: World_00_CoralReef or World_00)
        var loaded = Resources.Load<WorldConfig>($"Worlds/World_{worldIndex:D2}_{GetWorldNameSuffix(worldIndex)}")
                  ?? Resources.Load<WorldConfig>($"Worlds/World_{worldIndex}_{GetWorldNameSuffix(worldIndex)}")
                  ?? Resources.Load<WorldConfig>($"Worlds/World_{worldIndex:D2}")
                  ?? Resources.Load<WorldConfig>($"Worlds/World_{worldIndex}")
                  ?? Resources.Load<WorldConfig>($"Worlds/{GetDefaultWorldId(worldIndex)}");

        if (loaded != null)
        {
            if (loaded.backgroundSprite == null)
            {
                loaded.backgroundSprite = Resources.Load<Sprite>(GetDefaultBackgroundPath(worldIndex));
            }
            _cachedConfigs[worldIndex] = loaded;
            return loaded;
        }

        // Programmatic fallback
        var fallback = CreateInstance<WorldConfig>();
        fallback.worldIndex = worldIndex;
        fallback.worldId = GetDefaultWorldId(worldIndex);
        fallback.displayName = GetDefaultDisplayName(worldIndex);
        fallback.levelResourcePath = GetDefaultResourcePath(worldIndex);
        fallback.totalLevels = 12;
        fallback.backgroundSprite = Resources.Load<Sprite>(GetDefaultBackgroundPath(worldIndex));
        fallback.defaultCreature = (worldIndex == 0) ? CreatureType.Clownfish : CreatureType.Turtle;
        fallback.allowsCreatureAbilities = (worldIndex >= 1);
        fallback.hasAtmosphereEffects = (worldIndex == 1);
        fallback.unlockRequiredWorld = worldIndex - 1;

        _cachedConfigs[worldIndex] = fallback;
        return fallback;
    }

    public static string GetWorldNameSuffix(int worldIndex)
    {
        switch (worldIndex)
        {
            case 0:  return "CoralReef";
            case 1:  return "DeepOcean";
            case 2:  return "LostCity";
            default: return "CoralReef";
        }
    }

    public static string GetDefaultBackgroundPath(int worldIndex)
    {
        switch (worldIndex)
        {
            case 0:  return "Backgrounds/CoralReefBackground";
            case 1:  return "Backgrounds/DeepSeaBackground";
            case 2:  return "Backgrounds/LostCityBackground";
            default: return "Backgrounds/CoralReefBackground";
        }
    }

    public static string GetDefaultWorldId(int worldIndex)
    {
        switch (worldIndex)
        {
            case 0:  return "coral_reef";
            case 1:  return "deep_ocean";
            case 2:  return "lost_city";
            default: return $"world_{worldIndex}";
        }
    }

    public static string GetDefaultDisplayName(int worldIndex)
    {
        switch (worldIndex)
        {
            case 0:  return "Coral Reef";
            case 1:  return "Deep Ocean";
            case 2:  return "Lost City";
            default: return $"World {worldIndex + 1}";
        }
    }

    public static string GetDefaultResourcePath(int worldIndex)
    {
        switch (worldIndex)
        {
            case 0:  return "Levels/";
            case 1:  return "Levels/DeepOcean/";
            case 2:  return "Levels/LostCity/";
            default: return $"Levels/World_{worldIndex}/";
        }
    }
}
