#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Safe Editor-only progression test tool for Ocean Rescuers.
/// Allows resetting level progress or unlocking levels for QA testing without code changes.
/// </summary>
public static class ProgressionTestEditor
{
    [MenuItem("OceanRescuer/Progression/Reset Level Progress (Only Level 1 Unlocked)")]
    public static void ResetProgress()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Reset Level Progress",
            "This will reset all level unlocks and stars back to default.\n\nOnly World 1, Level 1 will remain unlocked.\nCoins and Gems will be preserved.\n\nProceed with reset?",
            "Reset Progress",
            "Cancel"
        );

        if (confirmed)
        {
            SaveManager.ResetLevelProgress(false);
            if (Application.isPlaying)
            {
                Object.FindFirstObjectByType<LevelSelectionManager>()?.RefreshLevelButtons();
            }
            EditorUtility.DisplayDialog(
                "Progress Reset",
                "Level progression has been reset!\n\nOnly Level 1 is now unlocked. Levels 2–12 are locked.",
                "OK"
            );
        }
    }

    [MenuItem("OceanRescuer/Progression/Unlock Level 2 (Direct Test)")]
    public static void UnlockLevel2()
    {
        int world = SaveManager.GetSelectedWorld();
        SaveManager.SaveLevelStars(world, 0, 3);
        SaveManager.UnlockLevel(world, 1);
        if (Application.isPlaying)
        {
            Object.FindFirstObjectByType<LevelSelectionManager>()?.RefreshLevelButtons();
        }
        EditorUtility.DisplayDialog(
            "Level 2 Unlocked",
            $"World {world + 1}: Level 1 awarded 3 stars.\nLevel 2 is now unlocked for testing.",
            "OK"
        );
    }

    [MenuItem("OceanRescuer/Progression/Unlock All Levels (1-12)")]
    public static void UnlockAllLevels()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Unlock All Levels",
            "Unlock all 12 levels across all worlds for testing?",
            "Unlock All",
            "Cancel"
        );

        if (confirmed)
        {
            SaveManager.UnlockAllLevels();
            if (Application.isPlaying)
            {
                Object.FindFirstObjectByType<LevelSelectionManager>()?.RefreshLevelButtons();
            }
            EditorUtility.DisplayDialog(
                "All Levels Unlocked",
                "All 12 levels in all worlds have been unlocked for testing.",
                "OK"
            );
        }
    }

    [MenuItem("OceanRescuer/Progression/Print Progress to Console")]
    public static void PrintProgress()
    {
        int world = SaveManager.GetSelectedWorld();
        int level = SaveManager.GetSelectedLevel();
        Debug.Log($"=== Ocean Rescuers Progression Status ===\nSelected World: {world + 1}, Selected Level: {level + 1}");
        for (int i = 0; i < 12; i++)
        {
            bool unlocked = SaveManager.IsLevelUnlocked(world, i);
            int stars = SaveManager.GetLevelStars(world, i);
            Debug.Log($"  Level {i + 1}: Unlocked={unlocked}, Stars={stars}");
        }
    }
}
#endif
