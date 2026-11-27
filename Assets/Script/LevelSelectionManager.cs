using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the Level Selection screen.
/// Reads the selected world from SaveManager, populates LevelButtons, and
/// handles level selection and navigation.
/// </summary>
public class LevelSelectionManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private int worldIndex = 0;

    [Header("Level Buttons")]
    [SerializeField] private LevelButton[] levelButtons;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI worldTitleText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI energyTimerText;

    [Header("Navigation")]
    [SerializeField] private UnityEngine.UI.Button backButton;

    [Header("World Header & Switch (Manual Control)")]
    [Tooltip("If TRUE, LevelSelectionManager will NOT overwrite your WorldNavigationHeader text, font, color, or position! You have 100% manual control in the Scene and Inspector.")]
    [SerializeField] private bool manualHeaderControl = true;

    [Tooltip("If TRUE, LevelSelectionManager will NOT overwrite your WorldSwitchBtn text, sprite, or color! You have 100% manual control in the Scene and Inspector.")]
    [SerializeField] private bool manualSwitchButtonControl = true;

    [Tooltip("Check if you want to completely hide the yellow header text.")]
    [SerializeField] private bool hideWorldHeader = false;

    [Tooltip("Check if you want to completely hide the top-right world switch button.")]
    [SerializeField] private bool hideWorldSwitchButton = false;

    [Header("World Header & Switch UI References")]
    [SerializeField] private GameObject worldNavigationHeader;
    [SerializeField] private Image worldBannerImage;
    [SerializeField] private Sprite[] worldBannerSprites;
    [SerializeField] private TextMeshProUGUI worldNameText;
    [SerializeField] private UnityEngine.UI.Button worldSwitchBtn;
    [SerializeField] private TextMeshProUGUI worldSwitchBtnText;

    private static readonly string[] WORLD_NAMES = { "Coral Reef", "Deep Ocean", "Lost City" };
    private GameObject _insufficientEnergyDialog;

    private void Start()
    {
        SaveManager.EnsureInitialized();
        worldIndex = Mathf.Clamp(SaveManager.GetSelectedWorld(), 0, WorldConfig.TOTAL_WORLDS - 1);

        WorldConfig config = WorldConfig.GetConfig(worldIndex);

        if (worldTitleText != null)
        {
            worldTitleText.gameObject.SetActive(true);
            worldTitleText.text = config.displayName;
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }

        // Apply world-specific background in Level Selection
        var canvas = GetLocalCanvas();
        if (canvas != null)
        {
            var uiBg = canvas.transform.Find("Background")?.GetComponent<Image>();
            if (uiBg != null)
            {
                Sprite bgSprite = config.backgroundSprite 
                    ?? Resources.Load<Sprite>(WorldConfig.GetDefaultBackgroundPath(worldIndex));
                if (bgSprite != null)
                {
                    uiBg.sprite = bgSprite;
                    uiBg.color = Color.white;
                }
            }
        }

        SetupWorldNavigation();
        SetupEnergyDisplay();
        PopulateLevelButtons();
        AudioManager.Instance?.PlayMenuMusic();

        // Environment effects strictly isolated to configured worlds
        if (config.hasAtmosphereEffects && GetComponent<DeepOceanEnvironment>() == null)
        {
            gameObject.AddComponent<DeepOceanEnvironment>();
        }
        else if (!config.hasAtmosphereEffects)
        {
            var env = GetComponent<DeepOceanEnvironment>();
            if (env != null) Destroy(env);
        }

        if (canvas != null)
        {
            if (canvas.GetComponent<UIBubbleEffect>() == null)
            {
                canvas.gameObject.AddComponent<UIBubbleEffect>();
            }
            UIButtonFeedback.AttachRecursively(canvas.transform);
        }

        EnergyManager.OnEnergyChanged += UpdateEnergyUI;
        UpdateEnergyUI();
    }

    private GameObject _headerObj;
    private GameObject _switchBtnObj;

    private Canvas GetLocalCanvas()
    {
        var canvases = FindObjectsOfType<Canvas>();
        foreach (var c in canvases)
        {
            if (c.gameObject.scene.name == "LevelSelectionScene" && c.gameObject.name != "FadeCanvas")
                return c;
        }
        return GetComponentInParent<Canvas>();
    }

    private void SetupWorldNavigation()
    {
        var canvas = GetLocalCanvas();
        if (canvas == null) return;

        // Auto-wire references if unassigned
        if (worldNavigationHeader == null)
        {
            var navObj = canvas.transform.Find("WorldNavigationHeader");
            if (navObj != null) worldNavigationHeader = navObj.gameObject;
        }

        if (worldNameText == null && worldNavigationHeader != null)
        {
            worldNameText = worldNavigationHeader.transform.Find("WorldNameText")?.GetComponent<TextMeshProUGUI>() 
                         ?? worldNavigationHeader.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (worldSwitchBtn == null)
        {
            var switchBtnTr = canvas.transform.Find("WorldSwitchBtn");
            if (switchBtnTr != null) worldSwitchBtn = switchBtnTr.GetComponent<UnityEngine.UI.Button>();
        }

        if (worldSwitchBtnText == null && worldSwitchBtn != null)
        {
            worldSwitchBtnText = worldSwitchBtn.transform.Find("Text")?.GetComponent<TextMeshProUGUI>() 
                              ?? worldSwitchBtn.GetComponentInChildren<TextMeshProUGUI>();
        }

        // 1. World Banner Image
        if (worldBannerImage == null && worldNavigationHeader != null)
        {
            worldBannerImage = worldNavigationHeader.GetComponentInChildren<Image>();
        }

        Sprite targetBanner = null;
        if (worldBannerSprites != null && worldIndex >= 0 && worldIndex < worldBannerSprites.Length && worldBannerSprites[worldIndex] != null)
        {
            targetBanner = worldBannerSprites[worldIndex];
        }
        else
        {
            WorldConfig config = WorldConfig.GetConfig(worldIndex);
            if (config != null && config.bannerSprite != null)
            {
                targetBanner = config.bannerSprite;
            }
        }

        if (worldBannerImage != null && targetBanner != null)
        {
            worldBannerImage.sprite = targetBanner;
            worldBannerImage.color = Color.white;
        }

        // 2. World Subtitle / Header
        if (worldNavigationHeader != null)
        {
            if (hideWorldHeader)
            {
                worldNavigationHeader.SetActive(false);
            }
            else
            {
                worldNavigationHeader.SetActive(true);
                // ONLY overwrite text if user turned OFF manualHeaderControl
                if (!manualHeaderControl && worldNameText != null)
                {
                    worldNameText.text = $"[ World {worldIndex + 1}: {WORLD_NAMES[Mathf.Clamp(worldIndex, 0, WORLD_NAMES.Length - 1)]} ]";
                }
            }
        }

        // 3. World Switch Button
        int nextUnlockedWorld = -1;
        for (int step = 1; step < WorldConfig.TOTAL_WORLDS; step++)
        {
            int candidate = (worldIndex + step) % WorldConfig.TOTAL_WORLDS;
            if (SaveManager.IsWorldUnlocked(candidate))
            {
                nextUnlockedWorld = candidate;
                break;
            }
        }

        if (worldSwitchBtn != null)
        {
            if (hideWorldSwitchButton || nextUnlockedWorld == -1)
            {
                worldSwitchBtn.gameObject.SetActive(false);
            }
            else
            {
                worldSwitchBtn.gameObject.SetActive(true);
                // Update button text to display target destination world
                if (worldSwitchBtnText != null)
                {
                    worldSwitchBtnText.text = $"{WorldConfig.GetConfig(nextUnlockedWorld).displayName} >";
                }

                int targetWorld = nextUnlockedWorld;
                worldSwitchBtn.onClick.RemoveAllListeners();
                worldSwitchBtn.onClick.AddListener(() =>
                {
                    SaveManager.SetSelectedWorld(targetWorld);
                    SceneLoader.LoadScene(SceneLoader.LEVEL_SELECT);
                });
            }
        }
    }

    private void OnDestroy()
    {
        EnergyManager.OnEnergyChanged -= UpdateEnergyUI;
        if (_headerObj != null) Destroy(_headerObj);
        if (_switchBtnObj != null) Destroy(_switchBtnObj);
    }

    private void SetupEnergyDisplay()
    {
        if (energyText != null) return;

        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        var existing = canvas.transform.Find("SafeAreaContainer/TopBar/EnergyBadge")
                    ?? canvas.transform.Find("TopBar/EnergyBadge") 
                    ?? canvas.transform.Find("EnergyBadge");
        if (existing != null)
        {
            energyText = existing.GetComponentInChildren<TextMeshProUGUI>();
            if (energyText != null)
            {
                energyText.enableAutoSizing = true;
                energyText.fontSizeMin = 18;
                energyText.fontSizeMax = 28;
                energyText.fontStyle = FontStyles.Bold;
                energyText.alignment = TextAlignmentOptions.Midline;
            }
            return;
        }
    }

    private void UpdateEnergyUI()
    {
        if (energyText != null && EnergyManager.Instance != null)
        {
            energyText.text = EnergyManager.Instance.GetFormattedEnergy();
        }
    }

    public void OnLevelSelected(int levelIndex)
    {
        // Guard: only proceed if the level is unlocked
        if (!SaveManager.IsLevelUnlocked(worldIndex, levelIndex))
        {
            Debug.Log($"[LevelSelectionManager] Level {levelIndex + 1} of world {worldIndex + 1} is locked.");
            return;
        }

        // Check Energy
        if (EnergyManager.Instance != null && !EnergyManager.Instance.HasEnoughEnergy(EnergyManager.ENERGY_COST_PER_LEVEL))
        {
            ShowInsufficientEnergyPopup();
            return;
        }

        // Consume Energy on valid level entry
        EnergyManager.Instance?.ConsumeEnergy(EnergyManager.ENERGY_COST_PER_LEVEL);

        SaveManager.SetSelectedWorld(worldIndex);
        SaveManager.SetSelectedLevel(levelIndex);
        SceneLoader.LoadScene(SceneLoader.GAMEPLAY);
    }

    private void ShowInsufficientEnergyPopup()
    {
        if (_insufficientEnergyDialog != null)
        {
            _insufficientEnergyDialog.SetActive(true);
            return;
        }

        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Create modal popup
        _insufficientEnergyDialog = new GameObject("InsufficientEnergyDialog");
        _insufficientEnergyDialog.transform.SetParent(canvas.transform, false);
        var dialogRT = _insufficientEnergyDialog.AddComponent<RectTransform>();
        dialogRT.anchorMin = Vector2.zero;
        dialogRT.anchorMax = Vector2.one;
        dialogRT.offsetMin = Vector2.zero;
        dialogRT.offsetMax = Vector2.zero;

        // Dark blocker
        var blockerImg = _insufficientEnergyDialog.AddComponent<Image>();
        blockerImg.color = new Color(0f, 0f, 0f, 0.75f);

        // Center card
        var cardGO = new GameObject("Card");
        cardGO.transform.SetParent(_insufficientEnergyDialog.transform, false);
        var cardRT = cardGO.AddComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.2f, 0.25f);
        cardRT.anchorMax = new Vector2(0.8f, 0.75f);
        cardRT.offsetMin = Vector2.zero;
        cardRT.offsetMax = Vector2.zero;
        var cardImg = cardGO.AddComponent<Image>();
        cardImg.color = new Color(0.08f, 0.22f, 0.38f, 0.98f);

        // Title
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(cardGO.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0.05f, 0.75f);
        titleRT.anchorMax = new Vector2(0.95f, 0.95f);
        titleRT.offsetMin = Vector2.zero;
        titleRT.offsetMax = Vector2.zero;
        var titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "Out of Energy!";
        titleTxt.alignment = TextAlignmentOptions.Center;
        titleTxt.fontSize = 36;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = new Color(1f, 0.85f, 0.2f);

        // Message
        var msgGO = new GameObject("Msg");
        msgGO.transform.SetParent(cardGO.transform, false);
        var msgRT = msgGO.AddComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0.08f, 0.40f);
        msgRT.anchorMax = new Vector2(0.92f, 0.72f);
        msgRT.offsetMin = Vector2.zero;
        msgRT.offsetMax = Vector2.zero;
        var msgTxt = msgGO.AddComponent<TextMeshProUGUI>();
        int cur = EnergyManager.Instance != null ? EnergyManager.Instance.CurrentEnergy : 0;
        string timer = EnergyManager.Instance != null ? EnergyManager.Instance.GetFormattedTimer() : "";
        msgTxt.text = $"Need {EnergyManager.ENERGY_COST_PER_LEVEL} Energy to enter this level.\nCurrent Energy: {cur}/{EnergyManager.MAX_ENERGY}\nNext energy in: {timer}\n\nRefill with Coins or in Shop!";
        msgTxt.alignment = TextAlignmentOptions.Center;
        msgTxt.fontSize = 22;
        msgTxt.color = Color.white;

        // Button: Refill for 50 Coins
        var refillBtnGO = new GameObject("RefillBtn");
        refillBtnGO.transform.SetParent(cardGO.transform, false);
        var refillRT = refillBtnGO.AddComponent<RectTransform>();
        refillRT.anchorMin = new Vector2(0.1f, 0.12f);
        refillRT.anchorMax = new Vector2(0.48f, 0.32f);
        refillRT.offsetMin = Vector2.zero;
        refillRT.offsetMax = Vector2.zero;
        var refillImg = refillBtnGO.AddComponent<Image>();
        refillImg.color = new Color(0.15f, 0.65f, 0.3f);
        var refillBtn = refillBtnGO.AddComponent<Button>();
        var rTextGO = new GameObject("Text");
        rTextGO.transform.SetParent(refillBtnGO.transform, false);
        var rTextRT = rTextGO.AddComponent<RectTransform>();
        rTextRT.anchorMin = Vector2.zero; rTextRT.anchorMax = Vector2.one;
        rTextRT.offsetMin = Vector2.zero; rTextRT.offsetMax = Vector2.zero;
        var rTxt = rTextGO.AddComponent<TextMeshProUGUI>();
        rTxt.text = "+5 Energy (50 Coins)";
        rTxt.alignment = TextAlignmentOptions.Center;
        rTxt.fontSize = 22;
        rTxt.fontStyle = FontStyles.Bold;

        refillBtn.onClick.AddListener(() =>
        {
            if (SaveManager.SpendCoins(50))
            {
                EnergyManager.Instance.AddEnergy(5);
                _insufficientEnergyDialog.SetActive(false);
                UpdateEnergyUI();
            }
            else
            {
                msgTxt.text = "Not enough Coins! Need 50 Coins.\nEarn coins by solving puzzles!";
                msgTxt.color = Color.red;
            }
        });

        // Button: Close
        var closeBtnGO = new GameObject("CloseBtn");
        closeBtnGO.transform.SetParent(cardGO.transform, false);
        var closeRT = closeBtnGO.AddComponent<RectTransform>();
        closeRT.anchorMin = new Vector2(0.52f, 0.12f);
        closeRT.anchorMax = new Vector2(0.9f, 0.32f);
        closeRT.offsetMin = Vector2.zero;
        closeRT.offsetMax = Vector2.zero;
        var closeImg = closeBtnGO.AddComponent<Image>();
        closeImg.color = new Color(0.65f, 0.2f, 0.2f);
        var closeBtn = closeBtnGO.AddComponent<Button>();
        var cTextGO = new GameObject("Text");
        cTextGO.transform.SetParent(closeBtnGO.transform, false);
        var cTextRT = cTextGO.AddComponent<RectTransform>();
        cTextRT.anchorMin = Vector2.zero; cTextRT.anchorMax = Vector2.one;
        cTextRT.offsetMin = Vector2.zero; cTextRT.offsetMax = Vector2.zero;
        var cTxt = cTextGO.AddComponent<TextMeshProUGUI>();
        cTxt.text = "Close";
        cTxt.alignment = TextAlignmentOptions.Center;
        cTxt.fontSize = 22;
        cTxt.fontStyle = FontStyles.Bold;

        closeBtn.onClick.AddListener(() =>
        {
            _insufficientEnergyDialog.SetActive(false);
        });
    }

    public void OnBackClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneLoader.LoadScene(SceneLoader.WORLD_SELECT);
    }

    public void RefreshLevelButtons()
    {
        PopulateLevelButtons();
    }

    private void PopulateLevelButtons()
    {
        if (levelButtons == null) return;

        // Find the current active progression level
        int currentProgressionIndex = 0;
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (SaveManager.IsLevelUnlocked(worldIndex, i))
            {
                currentProgressionIndex = i;
                if (SaveManager.GetLevelStars(worldIndex, i) == 0)
                {
                    break; // First uncompleted unlocked level
                }
            }
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null) continue;
            int   levelNum      = i + 1;
            int   stars         = SaveManager.GetLevelStars(worldIndex, i);
            bool  unlocked      = SaveManager.IsLevelUnlocked(worldIndex, i);
            int   capturedIndex = i;
            bool  isCurrent     = (i == currentProgressionIndex);

            levelButtons[i].Setup(levelNum, stars, unlocked, isCurrent);
            levelButtons[i].SetClickCallback(() => OnLevelSelected(capturedIndex));
        }
    }
}
