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

    private static readonly string[] WORLD_NAMES = { "Coral Reef", "Deep Ocean", "Lost City" };
    private GameObject _insufficientEnergyDialog;

    private void Start()
    {
        SaveManager.EnsureInitialized();
        worldIndex = SaveManager.GetSelectedWorld();

        // If World 0 (Coral Reef) is completed and World 1 is unlocked, switch to World 1 if World 1 still has pending levels
        if (worldIndex == 0 && SaveManager.IsWorldUnlocked(1))
        {
            bool world0AllDone = true;
            for (int i = 0; i < 12; i++)
            {
                if (SaveManager.GetLevelStars(0, i) == 0) { world0AllDone = false; break; }
            }
            if (world0AllDone)
            {
                bool world1HasPending = false;
                for (int i = 0; i < 12; i++)
                {
                    if (SaveManager.GetLevelStars(1, i) == 0) { world1HasPending = true; break; }
                }
                if (world1HasPending)
                {
                    worldIndex = 1;
                    SaveManager.SetSelectedWorld(1);
                }
            }
        }

        if (worldTitleText != null)
        {
            worldTitleText.gameObject.SetActive(true);
            worldTitleText.text = WORLD_NAMES[Mathf.Clamp(worldIndex, 0, WORLD_NAMES.Length - 1)];
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }

        SetupWorldNavigation();
        SetupEnergyDisplay();
        PopulateLevelButtons();
        AudioManager.Instance?.PlayMenuMusic();

        if (worldIndex == 1 && GetComponent<DeepOceanEnvironment>() == null)
        {
            gameObject.AddComponent<DeepOceanEnvironment>();
        }

        var canvas = FindObjectOfType<Canvas>();
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

        var navObj = canvas.transform.Find("WorldNavigationHeader");
        if (navObj == null)
        {
            _headerObj = new GameObject("WorldNavigationHeader", typeof(RectTransform));
            _headerObj.transform.SetParent(canvas.transform, false);
            var rt = _headerObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.25f, 0.88f);
            rt.anchorMax = new Vector2(0.75f, 0.94f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            // World Subtitle
            var titleGo = new GameObject("WorldNameText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGo.transform.SetParent(_headerObj.transform, false);
            var trt = titleGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.sizeDelta = Vector2.zero;
            var tmp = titleGo.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 26;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = new Color(1f, 0.95f, 0.4f, 1f);
            tmp.text = $"[ World {worldIndex + 1}: {WORLD_NAMES[Mathf.Clamp(worldIndex, 0, WORLD_NAMES.Length - 1)]} ]";

            // If World 1 is unlocked, show Toggle button
            if (SaveManager.IsWorldUnlocked(1))
            {
                _switchBtnObj = new GameObject("WorldSwitchBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                _switchBtnObj.transform.SetParent(canvas.transform, false);
                var toggleRt = _switchBtnObj.GetComponent<RectTransform>();
                toggleRt.anchorMin = new Vector2(0.97f, 0.94f);
                toggleRt.anchorMax = new Vector2(0.97f, 0.94f);
                toggleRt.pivot = new Vector2(1f, 1f);
                toggleRt.anchoredPosition = new Vector2(0f, 0f);
                toggleRt.sizeDelta = new Vector2(210f, 54f);

                var toggleImg = _switchBtnObj.GetComponent<Image>();
                toggleImg.color = new Color(0.08f, 0.45f, 0.85f, 0.95f);
                var btn = _switchBtnObj.GetComponent<Button>();

                var btnTxtGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                btnTxtGo.transform.SetParent(_switchBtnObj.transform, false);
                var btnTxtRt = btnTxtGo.GetComponent<RectTransform>();
                btnTxtRt.anchorMin = Vector2.zero; btnTxtRt.anchorMax = Vector2.one;
                btnTxtRt.sizeDelta = Vector2.zero;
                var btnTmp = btnTxtGo.GetComponent<TextMeshProUGUI>();
                btnTmp.alignment = TextAlignmentOptions.Center;
                btnTmp.fontSize = 20;
                btnTmp.fontStyle = FontStyles.Bold;
                btnTmp.color = Color.white;
                btnTmp.text = (worldIndex == 0) ? "Deep Ocean >" : "< Coral Reef";

                btn.onClick.AddListener(() =>
                {
                    int newWorld = (worldIndex == 0) ? 1 : 0;
                    SaveManager.SetSelectedWorld(newWorld);
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
