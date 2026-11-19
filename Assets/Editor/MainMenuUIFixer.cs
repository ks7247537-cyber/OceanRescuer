#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Builds / rebuilds ShopPanel and SettingsPanel in MainMenuScene.
/// Menu: OceanRescuer > Fix MainMenu Panels
///
/// Shop panel:
///   - Full-screen dark overlay + centered panel card
///   - 4 tab buttons: Coins / Gems / Energy / No Ads
///   - 4 product cards per tab (3 for Energy, 1 for No Ads)
///   - Coin + Gem balance bar at top
///   - Status text, Close button
///
/// Settings panel:
///   - Centered card using "setting panel.png" background
///   - Music ON/OFF button
///   - Sound ON/OFF button
///   - Close button
///   - Labels with live state
/// </summary>
public static class MainMenuUIFixer
{
    // ── Art paths ─────────────────────────────────────────────────────────────
    private const string SCENE_PATH    = "Assets/Scenes/MainMenuScene.unity";
    private const string ART_BTN      = "Assets/Art/UI/Buttons/";
    private const string ART_ICO      = "Assets/Art/UI/Icons/";
    private const string ART_PAN      = "Assets/Art/UI/Panels/";
    private const string ART_BG       = "Assets/Art/Backgrounds/";

    // ── Colors ─────────────────────────────────────────────────────────────────
    private static readonly Color PANEL_BG     = new Color(0.04f, 0.12f, 0.28f, 0.97f);
    private static readonly Color TAB_ACTIVE   = new Color(0.15f, 0.65f, 1f,   1f);
    private static readonly Color TAB_INACTIVE = new Color(0.08f, 0.25f, 0.45f, 1f);
    private static readonly Color CARD_BG      = new Color(0.06f, 0.18f, 0.35f, 0.95f);
    private static readonly Color BUY_BTN      = new Color(0.15f, 0.80f, 0.35f, 1f);
    private static readonly Color CLOSE_BTN    = new Color(0.85f, 0.20f, 0.20f, 1f);
    private static readonly Color OVERLAY      = new Color(0f,    0f,    0f,    0.70f);
    private static readonly Color MUSIC_ON     = new Color(0.2f,  0.9f,  0.4f,  1f);
    private static readonly Color SOUND_ON     = new Color(0.2f,  0.7f,  1f,    1f);
    private static readonly Color TOGGLE_OFF   = new Color(0.6f,  0.15f, 0.15f, 1f);

    // ─────────────────────────────────────────────────────────────────────────
    [MenuItem("OceanRescuer/Fix MainMenu Panels")]
    public static void FixMainMenuPanels()
    {
        // Open scene
        var scene = EditorSceneManager.GetSceneByPath(SCENE_PATH);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        }

        // Find main canvas
        Canvas mainCanvas = FindFirst<Canvas>();
        if (mainCanvas == null)
        {
            Debug.LogError("[MainMenuUIFixer] No Canvas found in MainMenuScene!");
            return;
        }
        Transform cr = mainCanvas.transform;

        // ── Sprites ───────────────────────────────────────────────────────────
        Sprite shopPanelBg  = LoadSprite(ART_ICO + "shop panel.png");
        Sprite settingBg    = LoadSprite(ART_PAN + "setting panel.png");
        Sprite coinIcon     = LoadSprite(ART_ICO + "icon_coin.png");
        Sprite gemIcon      = LoadSprite(ART_ICO + "icon_gem.png");
        Sprite closeX       = LoadSprite(ART_ICO + "Glossy Purple and Blue X Button (1).png");
        Sprite soundOnSpr   = LoadSprite(ART_BTN + "button_sound.png");
        Sprite soundOffSpr  = LoadSprite(ART_BTN + "button_mute.png");
        Sprite greenBlank   = LoadSprite(ART_BTN + "button_green_blank.png");
        Sprite blueBlank    = LoadSprite(ART_BTN + "button_blue_blank.png");
        Sprite orangeBlank  = LoadSprite(ART_BTN + "button_orange_blank.png");
        Sprite purpleBlank  = LoadSprite(ART_BTN + "button_purple_blank.png");

        // ── Build Panels ───────────────────────────────────────────────────────
        BuildShopPanel(cr, coinIcon, gemIcon, closeX, greenBlank, blueBlank, orangeBlank, purpleBlank);
        BuildSettingsPanel(cr, settingBg, soundOnSpr, soundOffSpr, closeX);

        // ── Save ───────────────────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[MainMenuUIFixer] ✅ MainMenu Panels fixed and saved!");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SHOP PANEL
    // ══════════════════════════════════════════════════════════════════════════

    static void BuildShopPanel(Transform canvasRoot,
        Sprite coinIcon, Sprite gemIcon, Sprite closeX,
        Sprite greenBtn, Sprite blueBtn, Sprite orangeBtn, Sprite purpleBtn)
    {
        // Remove old
        DestroyChild(canvasRoot, "ShopPanel");

        // ── Root: full-screen overlay ─────────────────────────────────────────
        var root = MakeGO("ShopPanel", canvasRoot);
        var rootRect = root.AddComponent<RectTransform>();
        Stretch(rootRect);
        var rootImg = root.AddComponent<Image>();
        rootImg.color = OVERLAY;
        root.AddComponent<CanvasGroup>();

        // ── Centered card ─────────────────────────────────────────────────────
        var card = MakeGO("Card", root.transform);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot     = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(960f, 1500f);
        var cardImg = card.AddComponent<Image>();
        cardImg.color = PANEL_BG;

        // ── Close button ──────────────────────────────────────────────────────
        var closeBtn = MakeButton("CloseButton", card.transform,
            new Vector2(880f, 1430f), new Vector2(100f, 100f),
            closeX, CLOSE_BTN, "");
        closeBtn.GetComponent<RectTransform>().anchorMin =
        closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        closeBtn.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        closeBtn.GetComponent<RectTransform>().anchorMin = new Vector2(1f, 1f);
        closeBtn.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
        closeBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(-40f, -40f);
        closeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(90f, 90f);

        // ── Title ─────────────────────────────────────────────────────────────
        MakeLabel("TitleText", card.transform, "🛒 Shop", 52f, Color.white,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), new Vector2(0f, -130f));

        // ── Balance bar (top) ─────────────────────────────────────────────────
        var balanceBar = MakeGO("BalanceBar", card.transform);
        var bbRect = balanceBar.AddComponent<RectTransform>();
        bbRect.anchorMin = new Vector2(0f, 1f);
        bbRect.anchorMax = new Vector2(1f, 1f);
        bbRect.pivot     = new Vector2(0.5f, 1f);
        bbRect.anchoredPosition = new Vector2(0f, -130f);
        bbRect.sizeDelta = new Vector2(0f, 80f);
        var bbHlg = balanceBar.AddComponent<HorizontalLayoutGroup>();
        bbHlg.spacing = 20f;
        bbHlg.childAlignment = TextAnchor.MiddleCenter;
        bbHlg.padding = new RectOffset(30, 30, 0, 0);
        bbHlg.childControlWidth = bbHlg.childControlHeight = false;
        bbHlg.childForceExpandWidth = bbHlg.childForceExpandHeight = false;

        // Coin pill
        var coinPill = MakeCurrencyPill("CoinPill", balanceBar.transform, coinIcon, "shopCoinText", new Color(0.1f, 0.3f, 0.05f, 0.8f));
        // Gem pill
        var gemPill  = MakeCurrencyPill("GemPill",  balanceBar.transform, gemIcon,  "shopGemText",  new Color(0.2f, 0.05f, 0.35f, 0.8f));

        // ── Tab bar ───────────────────────────────────────────────────────────
        var tabBar = MakeGO("TabBar", card.transform);
        var tbRect = tabBar.AddComponent<RectTransform>();
        tbRect.anchorMin = new Vector2(0f, 1f);
        tbRect.anchorMax = new Vector2(1f, 1f);
        tbRect.pivot     = new Vector2(0.5f, 1f);
        tbRect.anchoredPosition = new Vector2(0f, -215f);
        tbRect.sizeDelta = new Vector2(-20f, 85f);
        var tbHlg = tabBar.AddComponent<HorizontalLayoutGroup>();
        tbHlg.spacing = 8f;
        tbHlg.childAlignment = TextAnchor.MiddleCenter;
        tbHlg.childControlWidth = tbHlg.childControlHeight = true;
        tbHlg.childForceExpandWidth = true;
        tbHlg.childForceExpandHeight = true;
        tbHlg.padding = new RectOffset(10, 10, 5, 5);

        MakeTabButton("TabCoins",  tabBar.transform, "🪙 Coins",  TAB_ACTIVE,   blueBtn);
        MakeTabButton("TabGems",   tabBar.transform, "💎 Gems",   TAB_INACTIVE, blueBtn);
        MakeTabButton("TabEnergy", tabBar.transform, "⚡ Energy", TAB_INACTIVE, blueBtn);
        MakeTabButton("TabNoAds",  tabBar.transform, "🚫 No Ads", TAB_INACTIVE, blueBtn);

        // ── Tab content panels ────────────────────────────────────────────────
        var contentArea = MakeGO("ContentArea", card.transform);
        var caRect = contentArea.AddComponent<RectTransform>();
        caRect.anchorMin = new Vector2(0f, 0f);
        caRect.anchorMax = new Vector2(1f, 1f);
        caRect.pivot     = new Vector2(0.5f, 0.5f);
        caRect.offsetMin = new Vector2(10f, 10f);
        caRect.offsetMax = new Vector2(-10f, -310f);  // below tab bar

        // Coins content
        var coinContent = BuildCoinTabContent("ContentCoins", contentArea.transform,
            coinIcon, gemIcon, blueBtn, greenBtn);

        // Gems content
        var gemContent = BuildGemTabContent("ContentGems", contentArea.transform,
            coinIcon, gemIcon, blueBtn, greenBtn);
        gemContent.SetActive(false);

        // Energy content
        var energyContent = BuildEnergyTabContent("ContentEnergy", contentArea.transform,
            coinIcon, gemIcon, blueBtn, greenBtn, orangeBtn);
        energyContent.SetActive(false);

        // No Ads content
        var noAdsContent = BuildNoAdsContent("ContentNoAds", contentArea.transform, purpleBtn);
        noAdsContent.SetActive(false);

        // ── Status text ───────────────────────────────────────────────────────
        MakeLabel("StatusText", card.transform, "", 34f, Color.green,
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 10f), new Vector2(0f, 70f));

        // ── Wire ShopManager ──────────────────────────────────────────────────
        var sm = root.AddComponent<ShopManager>();
        SerializedObject so = new SerializedObject(sm);
        so.FindProperty("shopCoinText").objectReferenceValue  = FindTMPInChildren(coinPill.transform, "CountText");
        so.FindProperty("shopGemText").objectReferenceValue   = FindTMPInChildren(gemPill.transform,  "CountText");
        so.FindProperty("tabCoins").objectReferenceValue      = tabBar.transform.Find("TabCoins")?.GetComponent<Button>();
        so.FindProperty("tabGems").objectReferenceValue       = tabBar.transform.Find("TabGems")?.GetComponent<Button>();
        so.FindProperty("tabEnergy").objectReferenceValue     = tabBar.transform.Find("TabEnergy")?.GetComponent<Button>();
        so.FindProperty("tabNoAds").objectReferenceValue      = tabBar.transform.Find("TabNoAds")?.GetComponent<Button>();
        so.FindProperty("contentCoins").objectReferenceValue  = coinContent;
        so.FindProperty("contentGems").objectReferenceValue   = gemContent;
        so.FindProperty("contentEnergy").objectReferenceValue = energyContent;
        so.FindProperty("contentNoAds").objectReferenceValue  = noAdsContent;
        so.FindProperty("closeButton").objectReferenceValue   = closeBtn.GetComponent<Button>();
        so.FindProperty("statusText").objectReferenceValue    = card.transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();

        // Wire tab onClick events to ShopManager.SwitchTab via convenience:
        // (Buttons call public void methods — wired via onClick in inspector after runtime)
        // For now, the ShopManager.Start() wires these dynamically.

        // Wire close button
        var closeComp = closeBtn.GetComponent<Button>();
        AddPersistentListener(closeComp, sm, "CloseShop");

        root.SetActive(false);
        Debug.Log("[MainMenuUIFixer] ShopPanel built.");
    }

    // ── Tab content builders ──────────────────────────────────────────────────

    static GameObject BuildCoinTabContent(string name, Transform parent,
        Sprite coinIcon, Sprite gemIcon, Sprite blueBtn, Sprite greenBtn)
    {
        var go = MakeGO(name, parent);
        StretchRect(go);
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 18f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childControlHeight = vlg.childControlWidth = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // "Buy coins with Gems" cards
        string[] names  = { "Small Coin Pack",  "Medium Coin Pack",  "Large Coin Pack",  "Mega Coin Pack"  };
        string[] costs  = { "5 💎",             "10 💎",             "20 💎",            "50 💎"          };
        string[] awards = { "+200 🪙",           "+500 🪙",           "+1200 🪙",         "+3000 🪙"        };
        string[] buyCbs = { "BuyCoins0",        "BuyCoins1",         "BuyCoins2",        "BuyCoins3"       };

        var shopMgr = parent.GetComponentInParent<ShopManager>();
        for (int i = 0; i < names.Length; i++)
            BuildProductCard(go.transform, names[i], costs[i], awards[i], gemIcon, coinIcon, greenBtn, buyCbs[i], shopMgr);

        return go;
    }

    static GameObject BuildGemTabContent(string name, Transform parent,
        Sprite coinIcon, Sprite gemIcon, Sprite blueBtn, Sprite greenBtn)
    {
        var go = MakeGO(name, parent);
        StretchRect(go);
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 18f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childControlHeight = vlg.childControlWidth = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        string[] names  = { "Small Gem Pack",   "Medium Gem Pack",   "Large Gem Pack",   "Starter Gems (FREE)" };
        string[] costs  = { "100 🪙",            "250 🪙",            "500 🪙",           "FREE (once)"         };
        string[] awards = { "+5 💎",             "+15 💎",            "+35 💎",           "+10 💎"              };
        string[] buyCbs = { "BuyGems0",         "BuyGems1",          "BuyGems2",         "BuyGems3"            };

        var shopMgr = parent.GetComponentInParent<ShopManager>();
        for (int i = 0; i < names.Length; i++)
            BuildProductCard(go.transform, names[i], costs[i], awards[i], coinIcon, gemIcon, greenBtn, buyCbs[i], shopMgr);

        return go;
    }

    static GameObject BuildEnergyTabContent(string name, Transform parent,
        Sprite coinIcon, Sprite gemIcon, Sprite blueBtn, Sprite greenBtn, Sprite orangeBtn)
    {
        var go = MakeGO(name, parent);
        StretchRect(go);
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 18f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.padding = new RectOffset(15, 15, 15, 15);
        vlg.childControlHeight = vlg.childControlWidth = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        string[] names  = { "5 Energy Refill",  "Full Energy",       "Unlimited (1 hr)"  };
        string[] costs  = { "50 🪙",             "10 💎",             "20 💎"              };
        string[] awards = { "+5 ⚡",             "Full ⚡",           "∞ ⚡ (1hr)"         };
        string[] buyCbs = { "BuyEnergy0",        "BuyEnergy1",        "BuyEnergy2"         };

        var shopMgr = parent.GetComponentInParent<ShopManager>();
        for (int i = 0; i < names.Length; i++)
            BuildProductCard(go.transform, names[i], costs[i], awards[i], coinIcon, gemIcon, greenBtn, buyCbs[i], shopMgr);

        return go;
    }

    static GameObject BuildNoAdsContent(string name, Transform parent, Sprite purpleBtn)
    {
        var go = MakeGO(name, parent);
        StretchRect(go);

        // Centered info card
        var card = MakeGO("NoAdsCard", go.transform);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.1f, 0.3f);
        cardRect.anchorMax = new Vector2(0.9f, 0.75f);
        cardRect.offsetMin = cardRect.offsetMax = Vector2.zero;
        var cardImg = card.AddComponent<Image>();
        cardImg.color = CARD_BG;

        var vlg = card.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 20f;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childControlHeight = vlg.childControlWidth = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // Icon area
        var iconGO = MakeGO("Icon", card.transform);
        iconGO.AddComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);
        var iconTmp = iconGO.AddComponent<TextMeshProUGUI>();
        iconTmp.text = "🚫";
        iconTmp.fontSize = 70f;
        iconTmp.alignment = TextAlignmentOptions.Center;

        // Title
        var title = MakeGO("Title", card.transform);
        title.AddComponent<RectTransform>().sizeDelta = new Vector2(600f, 70f);
        var titleTmp = title.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Remove Ads";
        titleTmp.fontSize = 46f;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.color = Color.white;
        titleTmp.alignment = TextAlignmentOptions.Center;

        // Description
        var desc = MakeGO("Desc", card.transform);
        desc.AddComponent<RectTransform>().sizeDelta = new Vector2(600f, 120f);
        var descTmp = desc.AddComponent<TextMeshProUGUI>();
        descTmp.text = "🎮 Coming Soon!\n\nThis feature will be available\nwhen the game is published.";
        descTmp.fontSize = 32f;
        descTmp.color = new Color(0.75f, 0.85f, 1f);
        descTmp.alignment = TextAlignmentOptions.Center;

        // Coming Soon button (disabled)
        var btnGO = MakeGO("ComingSoonBtn", card.transform);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(400f, 90f);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.4f, 0.4f, 0.5f, 0.7f);
        if (purpleBtn != null) btnImg.sprite = purpleBtn;
        var btn = btnGO.AddComponent<Button>();
        btn.interactable = false;
        var btnLabel = MakeGO("Label", btnGO.transform);
        btnLabel.AddComponent<RectTransform>().sizeDelta = new Vector2(380f, 70f);
        var btnTmp = btnLabel.AddComponent<TextMeshProUGUI>();
        btnTmp.text = "Coming Soon";
        btnTmp.fontSize = 36f;
        btnTmp.color = new Color(0.7f, 0.7f, 0.8f);
        btnTmp.alignment = TextAlignmentOptions.Center;

        return go;
    }

    static void BuildProductCard(Transform parent,
        string itemName, string costText, string rewardText,
        Sprite costIcon, Sprite rewardIcon,
        Sprite btnSprite, string buyCallback, ShopManager shopMgr)
    {
        var card = MakeGO($"Card_{itemName}", parent);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(880f, 155f);
        var cardImg = card.AddComponent<Image>();
        cardImg.color = CARD_BG;

        var hlg = card.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 15f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(20, 20, 10, 10);
        hlg.childControlHeight = hlg.childControlWidth = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // ── Cost icon ─────────────────────────────────────────────────────────
        var iconGO = MakeGO("CostIcon", card.transform);
        var iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(60f, 60f);
        var iconImg = iconGO.AddComponent<Image>();
        if (costIcon != null) iconImg.sprite = costIcon;
        iconImg.preserveAspect = true;

        // ── Info column ───────────────────────────────────────────────────────
        var info = MakeGO("Info", card.transform);
        var infoRect = info.AddComponent<RectTransform>();
        infoRect.sizeDelta = new Vector2(430f, 130f);
        var infoVlg = info.AddComponent<VerticalLayoutGroup>();
        infoVlg.spacing = 4f;
        infoVlg.childAlignment = TextAnchor.MiddleLeft;
        infoVlg.childControlHeight = infoVlg.childControlWidth = false;
        infoVlg.childForceExpandWidth = true;
        infoVlg.childForceExpandHeight = false;

        // Name
        var nameGO = MakeGO("Name", info.transform);
        nameGO.AddComponent<RectTransform>().sizeDelta = new Vector2(420f, 55f);
        var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
        nameTmp.text = itemName;
        nameTmp.fontSize = 34f;
        nameTmp.color = Color.white;
        nameTmp.fontStyle = FontStyles.Bold;

        // Cost
        var costGO = MakeGO("Cost", info.transform);
        costGO.AddComponent<RectTransform>().sizeDelta = new Vector2(420f, 44f);
        var costTmp = costGO.AddComponent<TextMeshProUGUI>();
        costTmp.text = "Cost: " + costText;
        costTmp.fontSize = 28f;
        costTmp.color = new Color(1f, 0.85f, 0.3f);

        // ── Reward text ───────────────────────────────────────────────────────
        var rewardGO = MakeGO("Reward", card.transform);
        rewardGO.AddComponent<RectTransform>().sizeDelta = new Vector2(190f, 60f);
        var rewardTmp = rewardGO.AddComponent<TextMeshProUGUI>();
        rewardTmp.text = rewardText;
        rewardTmp.fontSize = 32f;
        rewardTmp.color = new Color(0.3f, 1f, 0.5f);
        rewardTmp.fontStyle = FontStyles.Bold;
        rewardTmp.alignment = TextAlignmentOptions.Center;

        // ── Buy button ────────────────────────────────────────────────────────
        var btnGO = MakeGO("BuyButton", card.transform);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(145f, 70f);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = BUY_BTN;
        if (btnSprite != null) btnImg.sprite = btnSprite;
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        // Label
        var labelGO = MakeGO("Label", btnGO.transform);
        var labelRect = labelGO.AddComponent<RectTransform>();
        Stretch(labelRect);
        var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
        labelTmp.text = "BUY";
        labelTmp.fontSize = 30f;
        labelTmp.fontStyle = FontStyles.Bold;
        labelTmp.color = Color.white;
        labelTmp.alignment = TextAlignmentOptions.Center;

        // Wire onClick to ShopManager
        if (shopMgr != null)
            AddPersistentListener(btn, shopMgr, buyCallback);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SETTINGS PANEL
    // ══════════════════════════════════════════════════════════════════════════

    static void BuildSettingsPanel(Transform canvasRoot,
        Sprite settingBg, Sprite soundOnSpr, Sprite soundOffSpr, Sprite closeX)
    {
        DestroyChild(canvasRoot, "SettingsPanel");

        // ── Root: full-screen overlay ─────────────────────────────────────────
        var root = MakeGO("SettingsPanel", canvasRoot);
        var rootRect = root.AddComponent<RectTransform>();
        Stretch(rootRect);
        var rootImg = root.AddComponent<Image>();
        rootImg.color = OVERLAY;

        // ── Centered card ─────────────────────────────────────────────────────
        var card = MakeGO("Card", root.transform);
        var cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot     = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.sizeDelta = new Vector2(820f, 950f);
        var cardImg = card.AddComponent<Image>();
        if (settingBg != null)
        {
            cardImg.sprite = settingBg;
            cardImg.preserveAspect = false;
            cardImg.type = Image.Type.Sliced;
        }
        else
        {
            cardImg.color = new Color(0.04f, 0.14f, 0.30f, 0.98f);
        }

        // ── Close button ──────────────────────────────────────────────────────
        var closeBtn = MakeButton("CloseButton", card.transform,
            Vector2.zero, new Vector2(90f, 90f), closeX, CLOSE_BTN, "");
        var closeBtnRect = closeBtn.GetComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(1f, 1f);
        closeBtnRect.anchorMax = new Vector2(1f, 1f);
        closeBtnRect.pivot     = new Vector2(1f, 1f);
        closeBtnRect.anchoredPosition = new Vector2(-20f, -20f);

        // ── Title ─────────────────────────────────────────────────────────────
        MakeLabel("TitleText", card.transform, "⚙️ Settings", 52f, Color.white,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), new Vector2(0f, -130f));

        // ── Divider ───────────────────────────────────────────────────────────
        var div = MakeGO("Divider", card.transform);
        var divRect = div.AddComponent<RectTransform>();
        divRect.anchorMin = new Vector2(0.05f, 1f);
        divRect.anchorMax = new Vector2(0.95f, 1f);
        divRect.pivot     = new Vector2(0.5f, 1f);
        divRect.anchoredPosition = new Vector2(0f, -135f);
        divRect.sizeDelta = new Vector2(0f, 3f);
        var divImg = div.AddComponent<Image>();
        divImg.color = new Color(0.4f, 0.7f, 1f, 0.5f);

        // ── Toggle rows ───────────────────────────────────────────────────────
        var toggleArea = MakeGO("ToggleArea", card.transform);
        var taRect = toggleArea.AddComponent<RectTransform>();
        taRect.anchorMin = new Vector2(0f, 0f);
        taRect.anchorMax = new Vector2(1f, 1f);
        taRect.offsetMin = new Vector2(40f, 100f);
        taRect.offsetMax = new Vector2(-40f, -170f);
        var taVlg = toggleArea.AddComponent<VerticalLayoutGroup>();
        taVlg.spacing = 40f;
        taVlg.childAlignment = TextAnchor.UpperCenter;
        taVlg.padding = new RectOffset(10, 10, 20, 20);
        taVlg.childControlHeight = taVlg.childControlWidth = false;
        taVlg.childForceExpandWidth = true;
        taVlg.childForceExpandHeight = false;

        // Music row
        var musicRow = BuildToggleRow("MusicRow", toggleArea.transform,
            "🎵 Music", "musicButton", soundOnSpr, soundOffSpr, MUSIC_ON);

        // Sound row
        var soundRow = BuildToggleRow("SoundRow", toggleArea.transform,
            "🔊 Sound", "soundButton", soundOnSpr, soundOffSpr, SOUND_ON);

        // ── Version / info text ───────────────────────────────────────────────
        MakeLabel("VersionText", card.transform,
            "Ocean Rescuers v1.0\n© 2024 Studio", 24f,
            new Color(0.6f, 0.75f, 0.9f),
            new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 10f), new Vector2(0f, 80f));

        // ── Wire SettingsManager ──────────────────────────────────────────────
        var settMgr = root.AddComponent<SettingsManager>();
        SerializedObject so = new SerializedObject(settMgr);
        so.FindProperty("musicButton").objectReferenceValue  = musicRow?.transform.Find("ToggleButton")?.GetComponent<Button>();
        so.FindProperty("soundButton").objectReferenceValue  = soundRow?.transform.Find("ToggleButton")?.GetComponent<Button>();
        so.FindProperty("closeButton").objectReferenceValue  = closeBtn.GetComponent<Button>();
        so.FindProperty("musicLabel").objectReferenceValue   = musicRow?.transform.Find("StateLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("soundLabel").objectReferenceValue   = soundRow?.transform.Find("StateLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("soundOnSprite").objectReferenceValue  = soundOnSpr;
        so.FindProperty("soundOffSprite").objectReferenceValue = soundOffSpr;
        so.FindProperty("musicOnSprite").objectReferenceValue  = soundOnSpr;
        so.FindProperty("musicOffSprite").objectReferenceValue = soundOffSpr;
        so.ApplyModifiedProperties();

        // Wire close button
        AddPersistentListener(closeBtn.GetComponent<Button>(), settMgr, "CloseSettings");

        // Wire music/sound buttons
        var musicBtn = musicRow?.transform.Find("ToggleButton")?.GetComponent<Button>();
        var soundBtn = soundRow?.transform.Find("ToggleButton")?.GetComponent<Button>();
        if (musicBtn != null) AddPersistentListener(musicBtn, settMgr, "OnMusicToggle");
        if (soundBtn != null) AddPersistentListener(soundBtn, settMgr, "OnSoundToggle");

        root.SetActive(false);
        Debug.Log("[MainMenuUIFixer] SettingsPanel built.");
    }

    static GameObject BuildToggleRow(string rowName, Transform parent,
        string labelText, string fieldName,
        Sprite onSprite, Sprite offSprite, Color onColor)
    {
        var row = MakeGO(rowName, parent);
        var rowRect = row.AddComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(680f, 110f);
        var rowHlg = row.AddComponent<HorizontalLayoutGroup>();
        rowHlg.spacing = 20f;
        rowHlg.childAlignment = TextAnchor.MiddleLeft;
        rowHlg.padding = new RectOffset(15, 15, 10, 10);
        rowHlg.childControlHeight = rowHlg.childControlWidth = false;
        rowHlg.childForceExpandWidth = false;
        rowHlg.childForceExpandHeight = true;

        // Background
        var rowImg = row.AddComponent<Image>();
        rowImg.color = new Color(0.06f, 0.18f, 0.35f, 0.8f);

        // Label
        var labelGO = MakeGO("RowLabel", row.transform);
        labelGO.AddComponent<RectTransform>().sizeDelta = new Vector2(340f, 80f);
        var labelTmp = labelGO.AddComponent<TextMeshProUGUI>();
        labelTmp.text = labelText;
        labelTmp.fontSize = 38f;
        labelTmp.color = Color.white;
        labelTmp.fontStyle = FontStyles.Bold;

        // State label (ON/OFF)
        var stateGO = MakeGO("StateLabel", row.transform);
        stateGO.AddComponent<RectTransform>().sizeDelta = new Vector2(140f, 60f);
        var stateTmp = stateGO.AddComponent<TextMeshProUGUI>();
        stateTmp.text = "<color=#33FF66>ON</color>";
        stateTmp.fontSize = 32f;
        stateTmp.alignment = TextAlignmentOptions.Center;

        // Toggle button
        var btnGO = MakeGO("ToggleButton", row.transform);
        var btnRect = btnGO.AddComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(130f, 80f);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = onColor;
        if (onSprite != null) btnImg.sprite = onSprite;
        btnImg.preserveAspect = true;
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        return row;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WIRE MainMenuManager
    // ══════════════════════════════════════════════════════════════════════════

    [MenuItem("OceanRescuer/Wire MainMenu Manager")]
    public static void WireMainMenuManager()
    {
        var scene = EditorSceneManager.GetSceneByPath(SCENE_PATH);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        }

        var mgr = FindFirst<MainMenuManager>();
        if (mgr == null) { Debug.LogError("[Wire] MainMenuManager not found!"); return; }

        var shopPanel     = FindGOByName("ShopPanel");
        var settingsPanel = FindGOByName("SettingsPanel");

        SerializedObject so = new SerializedObject(mgr);
        if (shopPanel     != null) so.FindProperty("shopPanel").objectReferenceValue     = shopPanel;
        if (settingsPanel != null) so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Wire] ✅ MainMenuManager wired to panels.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  HELPER METHODS
    // ══════════════════════════════════════════════════════════════════════════

    static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject MakeButton(string name, Transform parent,
        Vector2 anchoredPos, Vector2 size,
        Sprite sprite, Color color, string labelText)
    {
        var go = MakeGO(name, parent);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        if (sprite != null) img.sprite = sprite;
        img.preserveAspect = true;
        go.AddComponent<Button>().targetGraphic = img;
        if (!string.IsNullOrEmpty(labelText))
        {
            var lblGO = MakeGO("Label", go.transform);
            var lblRect = lblGO.AddComponent<RectTransform>();
            Stretch(lblRect);
            var tmp = lblGO.AddComponent<TextMeshProUGUI>();
            tmp.text = labelText;
            tmp.fontSize = 32f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }
        return go;
    }

    static void MakeTabButton(string name, Transform parent, string label, Color bgColor, Sprite sprite)
    {
        var go = MakeGO(name, parent);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200f, 75f);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        if (sprite != null) { img.sprite = sprite; }
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var lblGO = MakeGO("Label", go.transform);
        var lblRect = lblGO.AddComponent<RectTransform>();
        Stretch(lblRect);
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 26f;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
    }

    static void MakeLabel(string name, Transform parent, string text, float fontSize, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = MakeGO(name, parent);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot     = new Vector2(0.5f, 1f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    static GameObject MakeCurrencyPill(string name, Transform parent, Sprite icon, string tmpFieldName, Color bg)
    {
        var go = MakeGO(name, parent);
        var rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(190f, 65f);
        var img = go.AddComponent<Image>();
        img.color = bg;
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.padding = new RectOffset(10, 10, 5, 5);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = hlg.childControlHeight = false;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = false;

        var iconGO = MakeGO("Icon", go.transform);
        iconGO.AddComponent<RectTransform>().sizeDelta = new Vector2(44f, 44f);
        var iconImg = iconGO.AddComponent<Image>();
        if (icon != null) iconImg.sprite = icon;
        iconImg.preserveAspect = true;

        var txtGO = MakeGO("CountText", go.transform);
        txtGO.AddComponent<RectTransform>().sizeDelta = new Vector2(110f, 50f);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "0";
        tmp.fontSize = 34f;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;

        return go;
    }

    static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }
        if (sprite == null) Debug.LogWarning($"[UIFixer] Sprite not found: {path}");
        return sprite;
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    static void StretchRect(GameObject go)
    {
        var rect = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        Stretch(rect);
    }

    static void DestroyChild(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        if (child != null) Object.DestroyImmediate(child.gameObject);
    }

    static T FindFirst<T>() where T : Object
    {
        return Object.FindObjectOfType<T>();
    }

    static GameObject FindGOByName(string name)
    {
        return GameObject.Find(name);
    }

    static TextMeshProUGUI FindTMPInChildren(Transform parent, string childName)
    {
        var child = parent.Find(childName);
        return child?.GetComponent<TextMeshProUGUI>();
    }

    static void AddPersistentListener(Button btn, Object target, string methodName)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var onClick = so.FindProperty("m_OnClick");
        var calls = onClick.FindPropertyRelative("m_PersistentCalls.m_Calls");

        // Check if already wired
        for (int i = 0; i < calls.arraySize; i++)
        {
            var call = calls.GetArrayElementAtIndex(i);
            if (call.FindPropertyRelative("m_MethodName").stringValue == methodName &&
                call.FindPropertyRelative("m_Target").objectReferenceValue == target)
                return; // already wired
        }

        calls.InsertArrayElementAtIndex(calls.arraySize);
        var newCall = calls.GetArrayElementAtIndex(calls.arraySize - 1);

        // Find component with method
        Component comp = null;
        if (target is Component c)
        {
            var allComps = c.gameObject.GetComponents<MonoBehaviour>();
            foreach (var mb in allComps)
            {
                if (mb.GetType().GetMethod(methodName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance) != null)
                { comp = mb; break; }
            }
        }

        newCall.FindPropertyRelative("m_Target").objectReferenceValue  = comp ?? target;
        newCall.FindPropertyRelative("m_MethodName").stringValue       = methodName;
        newCall.FindPropertyRelative("m_Mode").intValue                = (int)UnityEngine.Events.PersistentListenerMode.Void;
        newCall.FindPropertyRelative("m_CallState").intValue           = (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
        so.ApplyModifiedProperties();
    }
}
#endif
