#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Complete wiring of ShopPanel + SettingsPanel in MainMenuScene.
/// Finds existing objects, adds missing children, wires all onClick events.
/// Menu: OceanRescuer > Complete Shop and Settings Panels
/// </summary>
public static class ShopSettingsFixer
{
    private const string SCENE_PATH  = "Assets/Scenes/MainMenuScene.unity";
    private const string ART_BTN     = "Assets/Art/UI/Buttons/";
    private const string ART_ICO     = "Assets/Art/UI/Icons/";
    private const string ART_PAN     = "Assets/Art/UI/Panels/";

    // ── Colors ────────────────────────────────────────────────────────────────
    static readonly Color COL_PANEL      = new Color(0.03f, 0.10f, 0.25f, 0.98f);
    static readonly Color COL_CARD       = new Color(0.05f, 0.15f, 0.32f, 0.95f);
    static readonly Color COL_TAB_ON     = new Color(0.10f, 0.55f, 1.00f, 1.00f);
    static readonly Color COL_TAB_OFF    = new Color(0.07f, 0.22f, 0.42f, 1.00f);
    static readonly Color COL_BUY        = new Color(0.10f, 0.75f, 0.30f, 1.00f);
    static readonly Color COL_CLOSE      = new Color(0.80f, 0.18f, 0.18f, 1.00f);
    static readonly Color COL_MUS_ON     = new Color(0.15f, 0.80f, 0.40f, 1.00f);
    static readonly Color COL_SND_ON     = new Color(0.10f, 0.60f, 1.00f, 1.00f);
    static readonly Color COL_TOGGLE_OFF = new Color(0.60f, 0.12f, 0.12f, 1.00f);
    static readonly Color COL_OVERLAY    = new Color(0.00f, 0.00f, 0.00f, 0.72f);
    static readonly Color COL_PILL_COIN  = new Color(0.08f, 0.26f, 0.04f, 0.85f);
    static readonly Color COL_PILL_GEM   = new Color(0.18f, 0.04f, 0.30f, 0.85f);
    static readonly Color COL_ROW        = new Color(0.05f, 0.16f, 0.32f, 0.85f);

    // ── Entry point ───────────────────────────────────────────────────────────
    [MenuItem("OceanRescuer/Complete Shop and Settings Panels")]
    public static void Run()
    {
        // Open scene if needed
        var scene = EditorSceneManager.GetSceneByPath(SCENE_PATH);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        }

        // Sprites
        Sprite spCoin   = Spr(ART_ICO + "icon_coin.png");
        Sprite spGem    = Spr(ART_ICO + "icon_gem.png");
        Sprite spGreen  = Spr(ART_BTN + "button_green_blank.png");
        Sprite spBlue   = Spr(ART_BTN + "button_blue_blank.png");
        Sprite spOrange = Spr(ART_BTN + "button_orange_blank.png");
        Sprite spPurple = Spr(ART_BTN + "button_purple_blank.png");
        Sprite spSound  = Spr(ART_BTN + "button_sound.png");
        Sprite spMute   = Spr(ART_BTN + "button_mute.png");
        Sprite spClose  = Spr(ART_ICO + "Glossy Purple and Blue X Button (1).png");
        Sprite spSett   = Spr(ART_PAN + "setting panel.png");

        FixShopPanel(spCoin, spGem, spGreen, spBlue, spOrange, spPurple, spClose);
        FixSettingsPanel(spSett, spSound, spMute, spClose);
        WireMainButtons();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[ShopSettingsFixer] ✅ Done. Scene saved.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SHOP PANEL
    // ══════════════════════════════════════════════════════════════════════════
    static void FixShopPanel(Sprite spCoin, Sprite spGem,
        Sprite spGreen, Sprite spBlue, Sprite spOrange, Sprite spPurple, Sprite spClose)
    {
        GameObject shopRoot = FindGO("ShopPanel");
        if (shopRoot == null)
        {
            Debug.LogError("[Fixer] ShopPanel not found in scene!"); return;
        }

        // Ensure overlay image on root
        var rootImg = EnsureComp<Image>(shopRoot);
        rootImg.color = COL_OVERLAY;
        rootImg.raycastTarget = true;
        EnsureComp<CanvasGroup>(shopRoot);

        // Find or create Card child
        Transform cardTF = shopRoot.transform.Find("Card");
        GameObject card  = cardTF != null ? cardTF.gameObject : MakeCard(shopRoot.transform, 960f, 1500f);

        // ── Close button ──────────────────────────────────────────────────────
        GameObject closeBtn = DeepFind(shopRoot.transform, "CloseButton");
        if (closeBtn == null) closeBtn = MakeBtnCorner("CloseButton", card.transform, spClose, COL_CLOSE);

        // ── Balance bar ───────────────────────────────────────────────────────
        GameObject bb = DeepFind(shopRoot.transform, "BalanceBar");
        if (bb == null) bb = MakeBalanceBar(card.transform, spCoin, spGem);
        TextMeshProUGUI shopCoinTxt = FindTMPNamed(bb.transform, "CountText", 0);
        TextMeshProUGUI shopGemTxt  = FindTMPNamed(bb.transform, "CountText", 1);
        // Try pill children too
        if (shopCoinTxt == null) shopCoinTxt = FindTMPInGrandchildren(bb.transform, "CoinPill", "CountText");
        if (shopGemTxt  == null) shopGemTxt  = FindTMPInGrandchildren(bb.transform, "GemPill",  "CountText");

        // ── Tab bar ───────────────────────────────────────────────────────────
        GameObject tabCoinsGO  = DeepFind(shopRoot.transform, "TabCoins");
        GameObject tabGemsGO   = DeepFind(shopRoot.transform, "TabGems");
        GameObject tabEnergyGO = DeepFind(shopRoot.transform, "TabEnergy");
        GameObject tabNoAdsGO  = DeepFind(shopRoot.transform, "TabNoAds");

        // create tab bar if missing tabs
        if (tabCoinsGO == null || tabGemsGO == null)
        {
            var tabBar = EnsureChild(card.transform, "TabBar");
            SetupTabBar(tabBar, 960f, 85f, new Vector2(0f, -215f));
            if (tabCoinsGO  == null) tabCoinsGO  = MakeTabBtn("TabCoins",  tabBar.transform, "🪙 Coins",  spBlue);
            if (tabGemsGO   == null) tabGemsGO   = MakeTabBtn("TabGems",   tabBar.transform, "💎 Gems",   spBlue);
            if (tabEnergyGO == null) tabEnergyGO = MakeTabBtn("TabEnergy", tabBar.transform, "⚡ Energy", spBlue);
            if (tabNoAdsGO  == null) tabNoAdsGO  = MakeTabBtn("TabNoAds",  tabBar.transform, "🚫 No Ads", spBlue);
        }

        // ── Content area ──────────────────────────────────────────────────────
        GameObject contentArea = DeepFind(shopRoot.transform, "ContentArea");
        if (contentArea == null) contentArea = MakeContentArea(card.transform);

        // ── Coin tab content ──────────────────────────────────────────────────
        GameObject coinContent = DeepFind(contentArea.transform, "ContentCoins");
        if (coinContent == null) coinContent = MakeGO("ContentCoins", contentArea.transform);
        RebuildTabContent(coinContent, Tab.Coins, spCoin, spGem, spGreen);

        // ── Gem tab content ───────────────────────────────────────────────────
        GameObject gemContent = EnsureChild(contentArea.transform, "ContentGems");
        RebuildTabContent(gemContent, Tab.Gems, spCoin, spGem, spGreen);

        // ── Energy tab content ────────────────────────────────────────────────
        GameObject energyContent = EnsureChild(contentArea.transform, "ContentEnergy");
        RebuildTabContent(energyContent, Tab.Energy, spCoin, spGem, spOrange);

        // ── No Ads tab content ────────────────────────────────────────────────
        GameObject noAdsContent = EnsureChild(contentArea.transform, "ContentNoAds");
        RebuildNoAdsContent(noAdsContent, spPurple);

        // Start with Coins visible, rest hidden
        coinContent.SetActive(true);
        gemContent.SetActive(false);
        energyContent.SetActive(false);
        noAdsContent.SetActive(false);

        // ── Status text ───────────────────────────────────────────────────────
        GameObject statusGO = DeepFind(shopRoot.transform, "StatusText");
        if (statusGO == null)
        {
            statusGO = MakeGO("StatusText", card.transform);
            var sr = statusGO.AddComponent<RectTransform>();
            sr.anchorMin = new Vector2(0f, 0f); sr.anchorMax = new Vector2(1f, 0f);
            sr.pivot = new Vector2(0.5f, 0f);
            sr.anchoredPosition = new Vector2(0f, 15f); sr.sizeDelta = new Vector2(-40f, 65f);
        }
        var statusTMP = EnsureComp<TextMeshProUGUI>(statusGO);
        statusTMP.text      = "";
        statusTMP.fontSize  = 32f;
        statusTMP.color     = Color.green;
        statusTMP.alignment = TextAlignmentOptions.Center;
        statusGO.SetActive(false);

        // ── Add / replace ShopManager ─────────────────────────────────────────
        var oldSM = shopRoot.GetComponent<ShopManager>();
        if (oldSM != null) Object.DestroyImmediate(oldSM);
        var sm = shopRoot.AddComponent<ShopManager>();

        var soSM = new SerializedObject(sm);
        soSM.FindProperty("shopCoinText").objectReferenceValue  = shopCoinTxt;
        soSM.FindProperty("shopGemText").objectReferenceValue   = shopGemTxt;
        soSM.FindProperty("tabCoins").objectReferenceValue      = tabCoinsGO?.GetComponent<Button>();
        soSM.FindProperty("tabGems").objectReferenceValue       = tabGemsGO?.GetComponent<Button>();
        soSM.FindProperty("tabEnergy").objectReferenceValue     = tabEnergyGO?.GetComponent<Button>();
        soSM.FindProperty("tabNoAds").objectReferenceValue      = tabNoAdsGO?.GetComponent<Button>();
        soSM.FindProperty("contentCoins").objectReferenceValue  = coinContent;
        soSM.FindProperty("contentGems").objectReferenceValue   = gemContent;
        soSM.FindProperty("contentEnergy").objectReferenceValue = energyContent;
        soSM.FindProperty("contentNoAds").objectReferenceValue  = noAdsContent;
        soSM.FindProperty("closeButton").objectReferenceValue   = closeBtn?.GetComponent<Button>();
        soSM.FindProperty("statusText").objectReferenceValue    = statusTMP;
        soSM.ApplyModifiedProperties();

        // Wire tab buttons
        WireBtn(tabCoinsGO,  sm, "OnTabCoins");
        WireBtn(tabGemsGO,   sm, "OnTabGems");
        WireBtn(tabEnergyGO, sm, "OnTabEnergy");
        WireBtn(tabNoAdsGO,  sm, "OnTabNoAds");
        WireBtn(closeBtn,    sm, "CloseShop");

        // Wire all BuyButtons inside content panels
        WireBuyButtons(coinContent,   sm, "BuyCoins");
        WireBuyButtons(gemContent,    sm, "BuyGems");
        WireBuyButtons(energyContent, sm, "BuyEnergy");

        shopRoot.SetActive(false);
        Debug.Log("[ShopFixer] ShopPanel rebuilt and wired.");
    }

    // ── Tab content types ─────────────────────────────────────────────────────
    enum Tab { Coins, Gems, Energy }

    static void RebuildTabContent(GameObject go, Tab tab, Sprite coinSpr, Sprite gemSpr, Sprite btnSpr)
    {
        // Clear existing children
        for (int i = go.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(go.transform.GetChild(i).gameObject);

        StretchRect(go);
        var vlg = EnsureComp<VerticalLayoutGroup>(go);
        vlg.spacing = 16f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.padding = new RectOffset(12, 12, 12, 12);
        vlg.childControlWidth = vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        switch (tab)
        {
            case Tab.Coins:
                BuildCard(go.transform, "Small Coin Pack",   "Cost: 5 💎",    "+200 🪙",  gemSpr,  coinSpr, btnSpr, 0);
                BuildCard(go.transform, "Medium Coin Pack",  "Cost: 10 💎",   "+500 🪙",  gemSpr,  coinSpr, btnSpr, 1);
                BuildCard(go.transform, "Large Coin Pack",   "Cost: 20 💎",   "+1200 🪙", gemSpr,  coinSpr, btnSpr, 2);
                BuildCard(go.transform, "Mega Coin Pack",    "Cost: 50 💎",   "+3000 🪙", gemSpr,  coinSpr, btnSpr, 3);
                break;
            case Tab.Gems:
                BuildCard(go.transform, "Small Gem Pack",    "Cost: 100 🪙",  "+5 💎",    coinSpr, gemSpr,  btnSpr, 0);
                BuildCard(go.transform, "Medium Gem Pack",   "Cost: 250 🪙",  "+15 💎",   coinSpr, gemSpr,  btnSpr, 1);
                BuildCard(go.transform, "Large Gem Pack",    "Cost: 500 🪙",  "+35 💎",   coinSpr, gemSpr,  btnSpr, 2);
                BuildCard(go.transform, "Starter Gems",      "FREE (once)",   "+10 💎",   coinSpr, gemSpr,  btnSpr, 3);
                break;
            case Tab.Energy:
                BuildCard(go.transform, "5 Energy Refill",   "Cost: 50 🪙",   "+5 ⚡",    coinSpr, coinSpr, btnSpr, 0);
                BuildCard(go.transform, "Full Energy",        "Cost: 10 💎",   "Full ⚡",  gemSpr,  coinSpr, btnSpr, 1);
                BuildCard(go.transform, "Unlimited (1 hr)",  "Cost: 20 💎",   "∞ ⚡ 1hr", gemSpr,  coinSpr, btnSpr, 2);
                break;
        }
    }

    static void BuildCard(Transform parent, string name, string cost, string reward,
        Sprite costIcon, Sprite rewardIcon, Sprite btnSprite, int index)
    {
        var card = MakeGO($"Card_{index}", parent);
        var cRect = card.AddComponent<RectTransform>();
        cRect.sizeDelta = new Vector2(880f, 148f);
        var cImg = card.AddComponent<Image>();
        cImg.color = COL_CARD;

        var hlg = card.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(18, 18, 8, 8);
        hlg.childControlWidth = hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;

        // Cost icon
        var icoGO = MakeGO("CostIcon", card.transform);
        icoGO.AddComponent<RectTransform>().sizeDelta = new Vector2(55f, 55f);
        var icoImg = icoGO.AddComponent<Image>();
        if (costIcon != null) { icoImg.sprite = costIcon; icoImg.preserveAspect = true; }
        else icoImg.color = new Color(1, 0.8f, 0.2f);

        // Info column
        var info = MakeGO("Info", card.transform);
        info.AddComponent<RectTransform>().sizeDelta = new Vector2(430f, 125f);
        var iVlg = info.AddComponent<VerticalLayoutGroup>();
        iVlg.spacing = 2f;
        iVlg.childAlignment = TextAnchor.MiddleLeft;
        iVlg.childControlWidth = iVlg.childControlHeight = false;
        iVlg.childForceExpandWidth = true;
        iVlg.childForceExpandHeight = false;

        // Name
        var nameGO = MakeGO("Name", info.transform);
        nameGO.AddComponent<RectTransform>().sizeDelta = new Vector2(410f, 56f);
        var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
        nameTMP.text = name; nameTMP.fontSize = 33f;
        nameTMP.color = Color.white; nameTMP.fontStyle = FontStyles.Bold;

        // Cost
        var costGO = MakeGO("Cost", info.transform);
        costGO.AddComponent<RectTransform>().sizeDelta = new Vector2(410f, 42f);
        var costTMP = costGO.AddComponent<TextMeshProUGUI>();
        costTMP.text = cost; costTMP.fontSize = 27f;
        costTMP.color = new Color(1f, 0.85f, 0.3f);

        // Reward
        var rwdGO = MakeGO("Reward", card.transform);
        rwdGO.AddComponent<RectTransform>().sizeDelta = new Vector2(188f, 55f);
        var rwdTMP = rwdGO.AddComponent<TextMeshProUGUI>();
        rwdTMP.text = reward; rwdTMP.fontSize = 31f;
        rwdTMP.color = new Color(0.3f, 1f, 0.5f);
        rwdTMP.fontStyle = FontStyles.Bold;
        rwdTMP.alignment = TextAlignmentOptions.Center;

        // Buy button
        var btnGO = MakeGO($"BuyButton_{index}", card.transform);
        btnGO.AddComponent<RectTransform>().sizeDelta = new Vector2(140f, 68f);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = COL_BUY;
        if (btnSprite != null) btnImg.sprite = btnSprite;
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        var lblGO = MakeGO("Label", btnGO.transform);
        var lblR = lblGO.AddComponent<RectTransform>();
        lblR.anchorMin = Vector2.zero; lblR.anchorMax = Vector2.one;
        lblR.offsetMin = lblR.offsetMax = Vector2.zero;
        var lblTMP = lblGO.AddComponent<TextMeshProUGUI>();
        lblTMP.text = "BUY"; lblTMP.fontSize = 29f;
        lblTMP.fontStyle = FontStyles.Bold;
        lblTMP.color = Color.white;
        lblTMP.alignment = TextAlignmentOptions.Center;
    }

    static void RebuildNoAdsContent(GameObject go, Sprite purpleBtn)
    {
        for (int i = go.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(go.transform.GetChild(i).gameObject);

        StretchRect(go);

        // Centered info card
        var inner = MakeGO("NoAdsCard", go.transform);
        var iRect = inner.AddComponent<RectTransform>();
        iRect.anchorMin = new Vector2(0.08f, 0.28f);
        iRect.anchorMax = new Vector2(0.92f, 0.78f);
        iRect.offsetMin = iRect.offsetMax = Vector2.zero;
        inner.AddComponent<Image>().color = COL_CARD;

        var vlg = inner.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 22f; vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.padding = new RectOffset(30, 30, 30, 30);
        vlg.childControlWidth = vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;

        // Emoji icon
        var emojiGO = MakeGO("Icon", inner.transform);
        emojiGO.AddComponent<RectTransform>().sizeDelta = new Vector2(600f, 90f);
        var eTMP = emojiGO.AddComponent<TextMeshProUGUI>();
        eTMP.text = "🚫 ADS"; eTMP.fontSize = 62f;
        eTMP.color = Color.white; eTMP.alignment = TextAlignmentOptions.Center;

        // Title
        var titleGO = MakeGO("Title", inner.transform);
        titleGO.AddComponent<RectTransform>().sizeDelta = new Vector2(600f, 65f);
        var tTMP = titleGO.AddComponent<TextMeshProUGUI>();
        tTMP.text = "Remove All Ads"; tTMP.fontSize = 44f;
        tTMP.fontStyle = FontStyles.Bold; tTMP.color = Color.white;
        tTMP.alignment = TextAlignmentOptions.Center;

        // Description
        var descGO = MakeGO("Desc", inner.transform);
        descGO.AddComponent<RectTransform>().sizeDelta = new Vector2(600f, 130f);
        var dTMP = descGO.AddComponent<TextMeshProUGUI>();
        dTMP.text = "Enjoy an ad-free experience!\n\n🛠 Coming Soon — available\nwhen the game is published.";
        dTMP.fontSize = 30f; dTMP.color = new Color(0.75f, 0.88f, 1f);
        dTMP.alignment = TextAlignmentOptions.Center;

        // Disabled button
        var bGO = MakeGO("ComingSoonBtn", inner.transform);
        bGO.AddComponent<RectTransform>().sizeDelta = new Vector2(420f, 88f);
        var bImg = bGO.AddComponent<Image>();
        bImg.color = new Color(0.35f, 0.35f, 0.45f, 0.75f);
        if (purpleBtn != null) bImg.sprite = purpleBtn;
        var b = bGO.AddComponent<Button>(); b.interactable = false;
        var blGO = MakeGO("Label", bGO.transform);
        var blR = blGO.AddComponent<RectTransform>();
        blR.anchorMin = Vector2.zero; blR.anchorMax = Vector2.one;
        blR.offsetMin = blR.offsetMax = Vector2.zero;
        var blTMP = blGO.AddComponent<TextMeshProUGUI>();
        blTMP.text = "Coming Soon"; blTMP.fontSize = 34f;
        blTMP.color = new Color(0.65f, 0.65f, 0.75f);
        blTMP.alignment = TextAlignmentOptions.Center;
    }

    static void WireBuyButtons(GameObject content, ShopManager sm, string methodPrefix)
    {
        // Find all BuyButton_N children recursively
        var buttons = new List<Button>();
        FindAllButtons(content.transform, buttons);
        for (int i = 0; i < buttons.Count; i++)
        {
            string method = $"{methodPrefix}{i}";
            // Check method exists on ShopManager
            var mInfo = typeof(ShopManager).GetMethod(method,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (mInfo != null)
                WireBtn(buttons[i].gameObject, sm, method);
            else
                Debug.LogWarning($"[Fixer] ShopManager.{method} not found — skipping.");
        }
    }

    static void FindAllButtons(Transform t, List<Button> result)
    {
        var btn = t.GetComponent<Button>();
        if (btn != null && t.name.StartsWith("BuyButton")) result.Add(btn);
        for (int i = 0; i < t.childCount; i++)
            FindAllButtons(t.GetChild(i), result);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  SETTINGS PANEL
    // ══════════════════════════════════════════════════════════════════════════
    static void FixSettingsPanel(Sprite settBg, Sprite soundOn, Sprite soundOff, Sprite closeX)
    {
        GameObject settRoot = FindGO("SettingsPanel");
        if (settRoot == null)
        {
            Debug.LogError("[Fixer] SettingsPanel not found!"); return;
        }

        // Root overlay
        var rootImg = EnsureComp<Image>(settRoot);
        rootImg.color = COL_OVERLAY; rootImg.raycastTarget = true;

        // Card
        Transform cardTF = settRoot.transform.Find("Card");
        GameObject card  = cardTF != null ? cardTF.gameObject : MakeCard(settRoot.transform, 820f, 950f);

        // Background image on card
        var cardImg = EnsureComp<Image>(card);
        if (settBg != null) { cardImg.sprite = settBg; cardImg.preserveAspect = false; }
        else cardImg.color = new Color(0.04f, 0.14f, 0.30f, 0.98f);

        // ── Close button ──────────────────────────────────────────────────────
        GameObject closeSett = DeepFind(settRoot.transform, "CloseButton")
                            ?? DeepFind(settRoot.transform, "CloseSettBtn");
        if (closeSett == null) closeSett = MakeBtnCorner("CloseButton", card.transform, closeX, COL_CLOSE);

        // ── Title ─────────────────────────────────────────────────────────────
        GameObject titleGO = DeepFind(settRoot.transform, "SettingsTitle")
                          ?? DeepFind(settRoot.transform, "TitleText");
        if (titleGO == null)
        {
            titleGO = MakeGO("SettingsTitle", card.transform);
            var tr = titleGO.AddComponent<RectTransform>();
            tr.anchorMin = new Vector2(0f, 1f); tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.anchoredPosition = new Vector2(0f, -55f); tr.sizeDelta = new Vector2(-20f, 80f);
        }
        var titleTMP = EnsureComp<TextMeshProUGUI>(titleGO);
        titleTMP.text = "⚙️  Settings"; titleTMP.fontSize = 50f;
        titleTMP.color = Color.white; titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.fontStyle = FontStyles.Bold;

        // ── Toggle Area ───────────────────────────────────────────────────────
        GameObject toggleArea = DeepFind(settRoot.transform, "ToggleArea");
        if (toggleArea == null)
        {
            toggleArea = MakeGO("ToggleArea", card.transform);
            var taR = toggleArea.AddComponent<RectTransform>();
            taR.anchorMin = new Vector2(0f, 0f); taR.anchorMax = new Vector2(1f, 1f);
            taR.offsetMin = new Vector2(35f, 90f); taR.offsetMax = new Vector2(-35f, -165f);
        }
        var taVlg = EnsureComp<VerticalLayoutGroup>(toggleArea);
        taVlg.spacing = 35f; taVlg.childAlignment = TextAnchor.UpperCenter;
        taVlg.padding = new RectOffset(10, 10, 20, 20);
        taVlg.childControlWidth = taVlg.childControlHeight = false;
        taVlg.childForceExpandWidth = true; taVlg.childForceExpandHeight = false;

        // Music Row
        GameObject musicRow = DeepFind(settRoot.transform, "MusicRow");
        if (musicRow == null) musicRow = BuildToggleRow("MusicRow", toggleArea.transform, "🎵  Music", soundOn);
        else RefreshToggleRow(musicRow, soundOn);
        GameObject musicBtnGO = DeepFind(musicRow.transform, "ToggleButton");

        // Sound Row
        GameObject soundRow = DeepFind(settRoot.transform, "SoundRow");
        if (soundRow == null) soundRow = BuildToggleRow("SoundRow", toggleArea.transform, "🔊  Sound", soundOn);
        else RefreshToggleRow(soundRow, soundOn);
        GameObject soundBtnGO = DeepFind(soundRow.transform, "ToggleButton");

        // ── Version label ─────────────────────────────────────────────────────
        GameObject verGO = DeepFind(settRoot.transform, "VersionText");
        if (verGO == null)
        {
            verGO = MakeGO("VersionText", card.transform);
            var vr = verGO.AddComponent<RectTransform>();
            vr.anchorMin = new Vector2(0f, 0f); vr.anchorMax = new Vector2(1f, 0f);
            vr.pivot = new Vector2(0.5f, 0f);
            vr.anchoredPosition = new Vector2(0f, 15f); vr.sizeDelta = new Vector2(-30f, 80f);
        }
        var verTMP = EnsureComp<TextMeshProUGUI>(verGO);
        verTMP.text = "Ocean Rescuers v1.0"; verTMP.fontSize = 24f;
        verTMP.color = new Color(0.55f, 0.72f, 0.88f);
        verTMP.alignment = TextAlignmentOptions.Center;

        // ── Add / replace SettingsManager ─────────────────────────────────────
        var oldSM = settRoot.GetComponent<SettingsManager>();
        if (oldSM != null) Object.DestroyImmediate(oldSM);
        var sm = settRoot.AddComponent<SettingsManager>();

        var so = new SerializedObject(sm);
        so.FindProperty("musicButton").objectReferenceValue   = musicBtnGO?.GetComponent<Button>();
        so.FindProperty("soundButton").objectReferenceValue   = soundBtnGO?.GetComponent<Button>();
        so.FindProperty("closeButton").objectReferenceValue   = closeSett?.GetComponent<Button>();
        so.FindProperty("musicLabel").objectReferenceValue    = FindTMPInGrandchildren(musicRow.transform, "StateLabel", "");
        so.FindProperty("soundLabel").objectReferenceValue    = FindTMPInGrandchildren(soundRow.transform, "StateLabel", "");
        so.FindProperty("musicOnSprite").objectReferenceValue = soundOn;
        so.FindProperty("musicOffSprite").objectReferenceValue = soundOff;
        so.FindProperty("soundOnSprite").objectReferenceValue = soundOn;
        so.FindProperty("soundOffSprite").objectReferenceValue = soundOff;
        so.ApplyModifiedProperties();

        // Wire onClick
        WireBtn(closeSett,   sm, "CloseSettings");
        WireBtn(musicBtnGO,  sm, "OnMusicToggle");
        WireBtn(soundBtnGO,  sm, "OnSoundToggle");

        settRoot.SetActive(false);
        Debug.Log("[SettingsFixer] SettingsPanel rebuilt and wired.");
    }

    static GameObject BuildToggleRow(string name, Transform parent, string label, Sprite onSprite)
    {
        var row = MakeGO(name, parent);
        var rRect = row.AddComponent<RectTransform>(); rRect.sizeDelta = new Vector2(680f, 108f);
        row.AddComponent<Image>().color = COL_ROW;
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 18f; hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(18, 18, 10, 10);
        hlg.childControlWidth = hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;

        // Label
        var lblGO = MakeGO("RowLabel", row.transform);
        lblGO.AddComponent<RectTransform>().sizeDelta = new Vector2(330f, 78f);
        var lTMP = lblGO.AddComponent<TextMeshProUGUI>();
        lTMP.text = label; lTMP.fontSize = 37f;
        lTMP.color = Color.white; lTMP.fontStyle = FontStyles.Bold;

        // State label
        var stGO = MakeGO("StateLabel", row.transform);
        stGO.AddComponent<RectTransform>().sizeDelta = new Vector2(145f, 55f);
        var sTMP = stGO.AddComponent<TextMeshProUGUI>();
        sTMP.text = "<color=#33FF66>ON</color>"; sTMP.fontSize = 31f;
        sTMP.alignment = TextAlignmentOptions.Center;

        // Toggle button
        var bGO = MakeGO("ToggleButton", row.transform);
        bGO.AddComponent<RectTransform>().sizeDelta = new Vector2(126f, 78f);
        var bImg = bGO.AddComponent<Image>();
        bImg.color = COL_MUS_ON;
        if (onSprite != null) { bImg.sprite = onSprite; bImg.preserveAspect = true; }
        bGO.AddComponent<Button>().targetGraphic = bImg;

        return row;
    }

    static void RefreshToggleRow(GameObject row, Sprite onSprite)
    {
        // Ensure ToggleButton exists
        var btnGO = DeepFind(row.transform, "ToggleButton");
        if (btnGO == null) { BuildToggleRow(row.name, row.transform.parent, row.name, onSprite); return; }
        var bImg = EnsureComp<Image>(btnGO);
        if (onSprite != null) { bImg.sprite = onSprite; bImg.preserveAspect = true; }
        bImg.color = COL_MUS_ON;
        EnsureComp<Button>(btnGO).targetGraphic = bImg;

        var stateGO = DeepFind(row.transform, "StateLabel");
        if (stateGO != null) EnsureComp<TextMeshProUGUI>(stateGO).text = "<color=#33FF66>ON</color>";
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WIRE MAIN MENU BUTTONS
    // ══════════════════════════════════════════════════════════════════════════
    static void WireMainButtons()
    {
        var mmm = Object.FindObjectOfType<MainMenuManager>();
        if (mmm == null) { Debug.LogWarning("[Fixer] MainMenuManager not found — skipping main button wiring."); return; }

        // Wire ShopButton
        var shopBtn = FindGO("ShopButton");
        if (shopBtn != null)
        {
            ClearAndWire(shopBtn, mmm, "OnShopClicked");
            Debug.Log("[Fixer] ShopButton wired.");
        }

        // Wire SettingsButton
        var settBtn = FindGO("SettingsButton");
        if (settBtn != null)
        {
            ClearAndWire(settBtn, mmm, "OnSettingsClicked");
            Debug.Log("[Fixer] SettingsButton wired.");
        }

        // Wire PlayButton
        var playBtn = FindGO("PlayButton");
        if (playBtn != null) ClearAndWire(playBtn, mmm, "OnPlayClicked");

        // Wire ShopPanel's close button via MainMenuManager too (belt+braces)
        var shopClose = DeepFind(FindGO("ShopPanel")?.transform, "CloseButton");
        // ShopManager.CloseShop() already handles this — just ensure it's wired.

        // Wire ShopPanel to MainMenuManager
        var shopPanel     = FindGO("ShopPanel");
        var settingsPanel = FindGO("SettingsPanel");
        var so = new SerializedObject(mmm);
        if (shopPanel     != null) so.FindProperty("shopPanel").objectReferenceValue     = shopPanel;
        if (settingsPanel != null) so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        so.ApplyModifiedProperties();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  HELPER METHODS
    // ══════════════════════════════════════════════════════════════════════════

    static GameObject MakeCard(Transform parent, float w, float h)
    {
        var card = MakeGO("Card", parent);
        var r = card.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = Vector2.zero; r.sizeDelta = new Vector2(w, h);
        card.AddComponent<Image>().color = COL_PANEL;
        return card;
    }

    static GameObject MakeBtnCorner(string name, Transform parent, Sprite sprite, Color color)
    {
        var go = MakeGO(name, parent);
        var r = go.AddComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(1f, 1f);
        r.anchoredPosition = new Vector2(-20f, -20f); r.sizeDelta = new Vector2(88f, 88f);
        var img = go.AddComponent<Image>(); img.color = color;
        if (sprite != null) { img.sprite = sprite; img.preserveAspect = true; }
        go.AddComponent<Button>().targetGraphic = img;
        return go;
    }

    static GameObject MakeBalanceBar(Transform parent, Sprite coinSpr, Sprite gemSpr)
    {
        var bb = MakeGO("BalanceBar", parent);
        var r = bb.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.anchoredPosition = new Vector2(0f, -130f); r.sizeDelta = new Vector2(-20f, 76f);
        var hlg = bb.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 18f; hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.padding = new RectOffset(30, 30, 0, 0);
        hlg.childControlWidth = hlg.childControlHeight = false;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = false;

        BuildPill("CoinPill", bb.transform, coinSpr, COL_PILL_COIN);
        BuildPill("GemPill",  bb.transform, gemSpr,  COL_PILL_GEM);
        return bb;
    }

    static void BuildPill(string name, Transform parent, Sprite icon, Color bg)
    {
        var go = MakeGO(name, parent);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(188f, 64f);
        go.AddComponent<Image>().color = bg;
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f; hlg.padding = new RectOffset(10, 10, 5, 5);
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = hlg.childControlHeight = false;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = false;

        var iGO = MakeGO("Icon", go.transform);
        iGO.AddComponent<RectTransform>().sizeDelta = new Vector2(44f, 44f);
        var iImg = iGO.AddComponent<Image>();
        if (icon != null) { iImg.sprite = icon; iImg.preserveAspect = true; }
        else iImg.color = Color.yellow;

        var tGO = MakeGO("CountText", go.transform);
        tGO.AddComponent<RectTransform>().sizeDelta = new Vector2(110f, 50f);
        var tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "0"; tmp.fontSize = 33f;
        tmp.color = Color.white; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
    }

    static void SetupTabBar(GameObject tabBar, float w, float h, Vector2 pos)
    {
        var r = EnsureComp<RectTransform>(tabBar);
        r.anchorMin = new Vector2(0f, 1f); r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.anchoredPosition = pos; r.sizeDelta = new Vector2(-18f, h);
        var hlg = EnsureComp<HorizontalLayoutGroup>(tabBar);
        hlg.spacing = 7f; hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.padding = new RectOffset(8, 8, 4, 4);
        hlg.childControlWidth = hlg.childControlHeight = true;
        hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
    }

    static GameObject MakeTabBtn(string name, Transform parent, string label, Sprite sprite)
    {
        var go = MakeGO(name, parent);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(200f, 72f);
        var img = go.AddComponent<Image>(); img.color = COL_TAB_OFF;
        if (sprite != null) img.sprite = sprite;
        go.AddComponent<Button>().targetGraphic = img;
        var lGO = MakeGO("Label", go.transform);
        var lR = lGO.AddComponent<RectTransform>();
        lR.anchorMin = Vector2.zero; lR.anchorMax = Vector2.one;
        lR.offsetMin = lR.offsetMax = Vector2.zero;
        var lTMP = lGO.AddComponent<TextMeshProUGUI>();
        lTMP.text = label; lTMP.fontSize = 25f;
        lTMP.color = Color.white; lTMP.fontStyle = FontStyles.Bold;
        lTMP.alignment = TextAlignmentOptions.Center;
        lTMP.enableWordWrapping = false;
        lTMP.overflowMode = TextOverflowModes.Ellipsis;
        return go;
    }

    static GameObject MakeContentArea(Transform parent)
    {
        var ca = MakeGO("ContentArea", parent);
        var r = ca.AddComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 0f); r.anchorMax = new Vector2(1f, 1f);
        r.offsetMin = new Vector2(8f, 8f); r.offsetMax = new Vector2(-8f, -308f);
        return ca;
    }

    // ── Generic helpers ───────────────────────────────────────────────────────

    static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject EnsureChild(Transform parent, string name)
    {
        var existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        return MakeGO(name, parent);
    }

    static void StretchRect(GameObject go)
    {
        var r = go.GetComponent<RectTransform>() ?? go.AddComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }

    static T EnsureComp<T>(GameObject go) where T : Component
    {
        return go.GetComponent<T>() ?? go.AddComponent<T>();
    }

    static GameObject FindGO(string name) => GameObject.Find(name);

    static GameObject DeepFind(Transform root, string name)
    {
        if (root == null) return null;
        if (root.name == name) return root.gameObject;
        for (int i = 0; i < root.childCount; i++)
        {
            var found = DeepFind(root.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    static TextMeshProUGUI FindTMPNamed(Transform root, string name, int index)
    {
        int count = 0;
        return FindTMPHelper(root, name, ref count, index);
    }

    static TextMeshProUGUI FindTMPHelper(Transform t, string name, ref int count, int target)
    {
        if (t.name == name)
        {
            var tmp = t.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                if (count == target) return tmp;
                count++;
            }
        }
        for (int i = 0; i < t.childCount; i++)
        {
            var r = FindTMPHelper(t.GetChild(i), name, ref count, target);
            if (r != null) return r;
        }
        return null;
    }

    static TextMeshProUGUI FindTMPInGrandchildren(Transform root, string childName, string grandChildName)
    {
        if (root == null) return null;
        // Search recursively
        for (int i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (child.name == childName || string.IsNullOrEmpty(childName))
            {
                for (int j = 0; j < child.childCount; j++)
                {
                    if (child.GetChild(j).name == grandChildName || string.IsNullOrEmpty(grandChildName))
                    {
                        var tmp = child.GetChild(j).GetComponent<TextMeshProUGUI>();
                        if (tmp != null) return tmp;
                    }
                }
            }
        }
        return null;
    }

    static Sprite Spr(string path)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
        {
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }
        if (s == null) Debug.LogWarning($"[Fixer] Sprite not found: {path}");
        return s;
    }

    // ── Button wiring ─────────────────────────────────────────────────────────

    static void WireBtn(GameObject go, Object target, string method)
    {
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn != null) WireBtn(btn, target, method);
    }

    static void WireBtn(Button btn, Object target, string method)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");

        // Remove stale calls to same method (re-wire cleanly)
        for (int i = calls.arraySize - 1; i >= 0; i--)
        {
            var c = calls.GetArrayElementAtIndex(i);
            if (c.FindPropertyRelative("m_MethodName").stringValue == method)
                calls.DeleteArrayElementAtIndex(i);
        }

        calls.InsertArrayElementAtIndex(calls.arraySize);
        var newCall = calls.GetArrayElementAtIndex(calls.arraySize - 1);

        // Find component with method
        Component comp = null;
        if (target is Component tc)
        {
            foreach (var mb in tc.gameObject.GetComponents<MonoBehaviour>())
            {
                if (mb.GetType().GetMethod(method,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance) != null)
                { comp = mb; break; }
            }
        }

        newCall.FindPropertyRelative("m_Target").objectReferenceValue  = (Object)comp ?? target;
        newCall.FindPropertyRelative("m_MethodName").stringValue       = method;
        newCall.FindPropertyRelative("m_Mode").intValue
            = (int)UnityEngine.Events.PersistentListenerMode.Void;
        newCall.FindPropertyRelative("m_CallState").intValue
            = (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
        so.ApplyModifiedProperties();
    }

    static void ClearAndWire(GameObject go, Object target, string method)
    {
        if (go == null) return;
        var btn = go.GetComponent<Button>();
        if (btn == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        calls.ClearArray();
        so.ApplyModifiedProperties();
        WireBtn(btn, target, method);
    }
}
#endif
