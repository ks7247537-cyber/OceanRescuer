using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// World Selection screen controller.
/// Worlds: 0=Coral Reef, 1=Deep Ocean, 2=Lost City.
/// World 0 always unlocked; others unlock via SaveManager.IsWorldUnlocked().
/// </summary>
public class WorldSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class WorldCardData
    {
        public Button button;
        public GameObject lockedOverlay;
        public TextMeshProUGUI worldNameText;
        public Image worldBannerImage;
    }

    [Header("World Cards (index 0=Coral Reef, 1=Deep Ocean, 2=Lost City)")]
    [SerializeField] private WorldCardData[] worldCards;

    [Header("Navigation")]
    [SerializeField] private Button backButton;

    private static readonly string[] WORLD_NAMES = { "Coral Reef", "Deep Ocean", "Lost City" };

    private void Start()
    {
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        UpdateWorldCards();
        UIButtonFeedback.AttachRecursively(transform);
        AudioManager.Instance?.PlayMenuMusic();
    }

    private void UpdateWorldCards()
    {
        if (worldCards == null) return;
        for (int i = 0; i < worldCards.Length; i++)
        {
            bool unlocked = SaveManager.IsWorldUnlocked(i);
            var card = worldCards[i];
            if (card == null) continue;

            WorldConfig config = WorldConfig.GetConfig(i);

            if (card.lockedOverlay != null)
            {
                card.lockedOverlay.SetActive(!unlocked);
                Transform parent = card.lockedOverlay.transform.parent;
                if (parent != null && (parent.name.Equals("overlay", System.StringComparison.OrdinalIgnoreCase) || 
                                       parent.name.Equals("0verlay", System.StringComparison.OrdinalIgnoreCase)))
                {
                    parent.gameObject.SetActive(!unlocked);
                }
            }
            if (card.button        != null) card.button.interactable = unlocked;
            if (card.worldNameText != null)
                card.worldNameText.text = config.displayName;
            if (card.worldBannerImage != null && config.worldCardSprite != null)
                card.worldBannerImage.sprite = config.worldCardSprite;

            int capturedIndex = i;
            if (card.button != null)
            {
                card.button.onClick.RemoveAllListeners();
                card.button.onClick.AddListener(() => OnWorldSelected(capturedIndex));
            }
        }
    }

    public void OnWorldSelected(int worldIndex)
    {
        if (!SaveManager.IsWorldUnlocked(worldIndex))
        {
            Debug.Log($"[WorldSelectionManager] World {worldIndex} locked.");
            return;
        }
        AudioManager.Instance?.PlayButtonClick();
        SaveManager.SetSelectedWorld(worldIndex);
        SceneLoader.LoadScene(SceneLoader.LEVEL_SELECT);
    }

    public void OnBackClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneLoader.LoadScene(SceneLoader.MAIN_MENU);
    }
}
