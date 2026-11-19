using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Level Selection screen.
/// Reads the selected world from PlayerPrefs, populates LevelButtons, and
/// handles level selection and navigation.
/// </summary>
public class LevelSelectionManager : MonoBehaviour
{
    private const string KEY_SELECTED_WORLD = "SelectedWorld";
    private const string KEY_SELECTED_LEVEL = "SelectedLevel";

    [Header("Configuration")]
    [SerializeField] private int worldIndex = 0;

    [Header("Level Buttons")]
    [SerializeField] private LevelButton[] levelButtons;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI worldTitleText;

    [Header("Navigation")]
    [SerializeField] private UnityEngine.UI.Button backButton;

    private static readonly string[] WORLD_NAMES = { "Coral Reef", "Deep Ocean", "Lost City" };

    private void Start()
    {
        worldIndex = PlayerPrefs.GetInt(KEY_SELECTED_WORLD, worldIndex);

        if (worldTitleText != null && worldIndex < WORLD_NAMES.Length)
            worldTitleText.text = WORLD_NAMES[worldIndex];

        if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

        PopulateLevelButtons();
        AudioManager.Instance?.PlayMenuMusic();
    }

    public void OnLevelSelected(int levelIndex)
    {
        if (!SaveManager.IsLevelUnlocked(worldIndex, levelIndex))
        {
            Debug.Log($"[LevelSelectionManager] Level {levelIndex} of world {worldIndex} is locked.");
            return;
        }
        PlayerPrefs.SetInt(KEY_SELECTED_LEVEL, levelIndex);
        PlayerPrefs.Save();
        SceneLoader.LoadScene(SceneLoader.GAMEPLAY);
    }

    public void OnBackClicked() => SceneLoader.LoadScene(SceneLoader.WORLD_SELECT);

    private void PopulateLevelButtons()
    {
        if (levelButtons == null) return;
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;
            int   levelNum      = i + 1;
            int   stars         = SaveManager.GetLevelStars(worldIndex, i);
            bool  unlocked      = SaveManager.IsLevelUnlocked(worldIndex, i);
            int   capturedIndex = i;
            levelButtons[i].Setup(levelNum, stars, unlocked);
            levelButtons[i].SetClickCallback(() => OnLevelSelected(capturedIndex));
        }
    }
}
