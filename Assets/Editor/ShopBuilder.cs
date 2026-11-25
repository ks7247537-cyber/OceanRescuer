#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Professional Shop Panel Builder for Ocean Rescuers.
/// Constructs the complete, beautiful underwater Shop Panel with actual Unity UI GameObjects,
/// item cards, bright green buy buttons, tab navigation, balance counters, and UIButtonFeedback.
/// Menu: OceanRescuer > Build Professional Shop Panel
/// </summary>
public static class ShopBuilder
{
    private const string SCENE_PATH = "Assets/Scenes/MainMenuScene.unity";
    private const string FONT_PATH  = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    // Sprite asset paths
    private const string SP_PANEL       = "Assets/Art/UI/Panels/pop up panel.png";
    private const string SP_BTN_BLUE    = "Assets/Art/UI/Buttons/button_blue_blank.png";
    private const string SP_BTN_GREEN   = "Assets/Art/UI/Buttons/button_green_blank.png";
    private const string SP_BTN_ORANGE  = "Assets/Art/UI/Buttons/button_orange_blank.png";
    private const string SP_BTN_PURPLE  = "Assets/Art/UI/Buttons/button_purple_blank.png";
    private const string SP_BTN_CLOSE   = "Assets/Art/UI/Buttons/button_exit.png";
    private const string SP_BANNER_WOOD = "Assets/Art/UI/WorldCards/banner_coral_reef.png";
    private const string SP_ICON_COIN   = "Assets/Art/UI/Icons/icon_coin.png";
    private const string SP_ICON_GEM    = "Assets/Art/UI/Icons/icon_gem.png";
    private const string SP_BUBBLES     = "Assets/Art/UI/Icons/bubbles.png";
    private const string SP_SHELL       = "Assets/Art/UI/Icons/shell_pearl.png";
    private const string SP_STAR        = "Assets/Art/UI/Icons/hud_star.png";

    private const string SP_PU_WRENCH   = "Assets/Art/PowerUps/powerup_wrench.png";
    private const string SP_PU_FREEZE   = "Assets/Art/PowerUps/powerup_freeze.png";
    private const string SP_PU_MAGNET   = "Assets/Art/PowerUps/powerup.png";
    private const string SP_PU_HINT     = "Assets/Art/PowerUps/powerup hint.png";
    private const string SP_PU_BUBBLE   = "Assets/Art/PowerUps/powerup_bubble.png";
    private const string SP_PU_LIGHTNING= "Assets/Art/PowerUps/powerup_lightning.png";
    private const string SP_PU_LIFE     = "Assets/Art/PowerUps/powerup_lifebuoy.png";

    private const string SP_CORAL_PINK  = "Assets/Art/Effects/coral_pink.png";
    private const string SP_CORAL_ORANGE= "Assets/Art/Effects/coral_orange.png";
    private const string SP_CORAL_WEED  = "Assets/Art/Effects/coral_seaweed.png";

    [MenuItem("OceanRescuer/Build Professional Shop Panel")]
    public static void BuildShopPanel()
    {
        var scene = EditorSceneManager.GetSceneByPath(SCENE_PATH);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        }

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);

        // Load Sprites
        Sprite spPanel    = LoadSprite(SP_PANEL);
        Sprite spBtnBlue  = LoadSprite(SP_BTN_BLUE);
        Sprite spBtnGreen = LoadSprite(SP_BTN_GREEN);
        Sprite spBtnOrange= LoadSprite(SP_BTN_ORANGE);
        Sprite spBtnPurple= LoadSprite(SP_BTN_PURPLE);
        Sprite spBtnClose = LoadSprite(SP_BTN_CLOSE);
        Sprite spBanner   = LoadSprite(SP_BANNER_WOOD);
        Sprite spCoin     = LoadSprite(SP_ICON_COIN);
        Sprite spGem      = LoadSprite(SP_ICON_GEM);
        Sprite spBubbles  = LoadSprite(SP_BUBBLES);
        Sprite spShell    = LoadSprite(SP_SHELL);
        Sprite spStar     = LoadSprite(SP_STAR);

        Sprite spWrench   = LoadSprite(SP_PU_WRENCH);
        Sprite spFreeze   = LoadSprite(SP_PU_FREEZE);
        Sprite spMagnet   = LoadSprite(SP_PU_MAGNET);
        Sprite spHint     = LoadSprite(SP_PU_HINT);
        Sprite spBubble   = LoadSprite(SP_PU_BUBBLE);
        Sprite spLightning= LoadSprite(SP_PU_LIGHTNING);
        Sprite spLife     = LoadSprite(SP_PU_LIFE);

        Sprite spCoralPink  = LoadSprite(SP_CORAL_PINK);
        Sprite spCoralOrange= LoadSprite(SP_CORAL_ORANGE);
        Sprite spCoralWeed  = LoadSprite(SP_CORAL_WEED);

        GameObject shopPanelGO = GameObject.Find("ShopPanel");
        if (shopPanelGO == null)
        {
            Debug.LogError("[ShopBuilder] ShopPanel GameObject not found in MainMenuScene!");
            return;
        }

        // 1. Reset ShopPanel Root RectTransform and Backdrop Image
        RectTransform rootRT = shopPanelGO.GetComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero;
        rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = Vector2.zero;
        rootRT.offsetMax = Vector2.zero;
        rootRT.pivot = new Vector2(0.5f, 0.5f);
        rootRT.localScale = Vector3.one;

        Image rootImg = shopPanelGO.GetComponent<Image>();
        if (rootImg == null) rootImg = shopPanelGO.AddComponent<Image>();
        rootImg.sprite = null;
        rootImg.color = new Color(0.01f, 0.05f, 0.12f, 0.78f); // semi-transparent dark ocean overlay
        rootImg.raycastTarget = true;

        CanvasGroup cg = shopPanelGO.GetComponent<CanvasGroup>();
        if (cg == null) cg = shopPanelGO.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // 2. Clear old children cleanly
        int childCount = shopPanelGO.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(shopPanelGO.transform.GetChild(i).gameObject);
        }

        // 3. Create Main Window Container (ShopWindow)
        GameObject windowGO = CreateUIObject("ShopWindow", shopPanelGO.transform);
        RectTransform windowRT = windowGO.GetComponent<RectTransform>();
        windowRT.anchorMin = new Vector2(0.5f, 0.5f);
        windowRT.anchorMax = new Vector2(0.5f, 0.5f);
        windowRT.anchoredPosition = new Vector2(0f, -20f);
        windowRT.sizeDelta = new Vector2(1380f, 860f);

        Image windowImg = windowGO.AddComponent<Image>();
        windowImg.sprite = spPanel;
        windowImg.type = Image.Type.Sliced;
        windowImg.color = new Color(0.08f, 0.32f, 0.52f, 0.98f); // Glossy cyan/blue marine frame

        // Inner frame container
        GameObject innerGO = CreateUIObject("InnerFrame", windowGO.transform);
        RectTransform innerRT = innerGO.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(24f, 24f);
        innerRT.offsetMax = new Vector2(-24f, -24f);

        Image innerImg = innerGO.AddComponent<Image>();
        innerImg.sprite = spBtnBlue;
        innerImg.type = Image.Type.Sliced;
        innerImg.color = new Color(0.03f, 0.12f, 0.24f, 0.92f); // Deep underwater dark blue interior

        // 4. Underwater Decorations
        // Bottom left corals
        CreateDecorImage("Decor_CoralPink", innerGO.transform, spCoralPink,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(160f, 160f));
        CreateDecorImage("Decor_CoralWeedL", innerGO.transform, spCoralWeed,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(110f, -10f), new Vector2(130f, 180f));

        // Bottom right corals
        CreateDecorImage("Decor_CoralOrange", innerGO.transform, spCoralOrange,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-10f, 0f), new Vector2(160f, 160f));
        CreateDecorImage("Decor_CoralWeedR", innerGO.transform, spCoralWeed,
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-110f, -10f), new Vector2(130f, 180f));

        // Floating bubbles on sides
        CreateDecorImage("Decor_BubblesL", innerGO.transform, spBubbles,
            new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 40f), new Vector2(110f, 150f));
        CreateDecorImage("Decor_BubblesR", innerGO.transform, spBubbles,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 80f), new Vector2(110f, 150f));

        // 5. Wooden Title Banner (Shop)
        GameObject bannerGO = CreateUIObject("WoodenHeaderBanner", windowGO.transform);
        RectTransform bannerRT = bannerGO.GetComponent<RectTransform>();
        bannerRT.anchorMin = new Vector2(0.5f, 1f);
        bannerRT.anchorMax = new Vector2(0.5f, 1f);
        bannerRT.anchoredPosition = new Vector2(0f, 26f);
        bannerRT.sizeDelta = new Vector2(460f, 115f);

        Image bannerImg = bannerGO.AddComponent<Image>();
        bannerImg.sprite = spBanner;
        bannerImg.type = Image.Type.Simple;
        bannerImg.preserveAspect = true;

        GameObject titleTxtGO = CreateUIObject("TitleText", bannerGO.transform);
        RectTransform titleTxtRT = titleTxtGO.GetComponent<RectTransform>();
        titleTxtRT.anchorMin = Vector2.zero;
        titleTxtRT.anchorMax = Vector2.one;
        titleTxtRT.anchoredPosition = new Vector2(0f, 6f);
        titleTxtRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI titleTMP = titleTxtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) titleTMP.font = font;
        titleTMP.text = "SHOP";
        titleTMP.fontSize = 52;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(1f, 0.94f, 0.40f, 1f);
        titleTMP.outlineColor = new Color(0.28f, 0.12f, 0.02f, 1f);
        titleTMP.outlineWidth = 0.22f;

        // 6. Close Button
        GameObject closeBtnGO = CreateUIObject("CloseButton", windowGO.transform);
        RectTransform closeBtnRT = closeBtnGO.GetComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(1f, 1f);
        closeBtnRT.anchorMax = new Vector2(1f, 1f);
        closeBtnRT.anchoredPosition = new Vector2(-15f, 15f);
        closeBtnRT.sizeDelta = new Vector2(85f, 85f);

        Image closeImg = closeBtnGO.AddComponent<Image>();
        closeImg.sprite = spBtnClose;
        closeImg.preserveAspect = true;

        Button closeBtn = closeBtnGO.AddComponent<Button>();
        closeBtnGO.AddComponent<UIButtonFeedback>();

        // 7. Balance Displays (Top Right inside window)
        GameObject balanceBarGO = CreateUIObject("BalanceBar", windowGO.transform);
        RectTransform balanceBarRT = balanceBarGO.GetComponent<RectTransform>();
        balanceBarRT.anchorMin = new Vector2(1f, 1f);
        balanceBarRT.anchorMax = new Vector2(1f, 1f);
        balanceBarRT.anchoredPosition = new Vector2(-130f, -42f);
        balanceBarRT.sizeDelta = new Vector2(360f, 52f);

        // Coin Pill
        GameObject coinPillGO = CreateUIObject("CoinPill", balanceBarGO.transform);
        RectTransform coinPillRT = coinPillGO.GetComponent<RectTransform>();
        coinPillRT.anchorMin = new Vector2(0f, 0.5f);
        coinPillRT.anchorMax = new Vector2(0f, 0.5f);
        coinPillRT.anchoredPosition = new Vector2(85f, 0f);
        coinPillRT.sizeDelta = new Vector2(165f, 48f);

        Image coinPillImg = coinPillGO.AddComponent<Image>();
        coinPillImg.sprite = spBtnBlue;
        coinPillImg.type = Image.Type.Sliced;
        coinPillImg.color = new Color(0.04f, 0.20f, 0.36f, 0.95f);

        GameObject coinIconGO = CreateUIObject("CoinIcon", coinPillGO.transform);
        RectTransform coinIconRT = coinIconGO.GetComponent<RectTransform>();
        coinIconRT.anchorMin = new Vector2(0f, 0.5f);
        coinIconRT.anchorMax = new Vector2(0f, 0.5f);
        coinIconRT.anchoredPosition = new Vector2(26f, 0f);
        coinIconRT.sizeDelta = new Vector2(38f, 38f);
        Image coinIconImg = coinIconGO.AddComponent<Image>();
        coinIconImg.sprite = spCoin;
        coinIconImg.preserveAspect = true;

        GameObject coinTxtGO = CreateUIObject("CoinText", coinPillGO.transform);
        RectTransform coinTxtRT = coinTxtGO.GetComponent<RectTransform>();
        coinTxtRT.anchorMin = new Vector2(0f, 0f);
        coinTxtRT.anchorMax = new Vector2(1f, 1f);
        coinTxtRT.offsetMin = new Vector2(52f, 0f);
        coinTxtRT.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI shopCoinTMP = coinTxtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) shopCoinTMP.font = font;
        shopCoinTMP.text = "0";
        shopCoinTMP.fontSize = 22;
        shopCoinTMP.fontStyle = FontStyles.Bold;
        shopCoinTMP.alignment = TextAlignmentOptions.MidlineLeft;
        shopCoinTMP.color = new Color(1f, 0.88f, 0.25f, 1f);

        // Gem Pill
        GameObject gemPillGO = CreateUIObject("GemPill", balanceBarGO.transform);
        RectTransform gemPillRT = gemPillGO.GetComponent<RectTransform>();
        gemPillRT.anchorMin = new Vector2(1f, 0.5f);
        gemPillRT.anchorMax = new Vector2(1f, 0.5f);
        gemPillRT.anchoredPosition = new Vector2(-85f, 0f);
        gemPillRT.sizeDelta = new Vector2(165f, 48f);

        Image gemPillImg = gemPillGO.AddComponent<Image>();
        gemPillImg.sprite = spBtnBlue;
        gemPillImg.type = Image.Type.Sliced;
        gemPillImg.color = new Color(0.18f, 0.06f, 0.32f, 0.95f);

        GameObject gemIconGO = CreateUIObject("GemIcon", gemPillGO.transform);
        RectTransform gemIconRT = gemIconGO.GetComponent<RectTransform>();
        gemIconRT.anchorMin = new Vector2(0f, 0.5f);
        gemIconRT.anchorMax = new Vector2(0f, 0.5f);
        gemIconRT.anchoredPosition = new Vector2(26f, 0f);
        gemIconRT.sizeDelta = new Vector2(38f, 38f);
        Image gemIconImg = gemIconGO.AddComponent<Image>();
        gemIconImg.sprite = spGem;
        gemIconImg.preserveAspect = true;

        GameObject gemTxtGO = CreateUIObject("GemText", gemPillGO.transform);
        RectTransform gemTxtRT = gemTxtGO.GetComponent<RectTransform>();
        gemTxtRT.anchorMin = new Vector2(0f, 0f);
        gemTxtRT.anchorMax = new Vector2(1f, 1f);
        gemTxtRT.offsetMin = new Vector2(52f, 0f);
        gemTxtRT.offsetMax = new Vector2(-10f, 0f);
        TextMeshProUGUI shopGemTMP = gemTxtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) shopGemTMP.font = font;
        shopGemTMP.text = "0";
        shopGemTMP.fontSize = 22;
        shopGemTMP.fontStyle = FontStyles.Bold;
        shopGemTMP.alignment = TextAlignmentOptions.MidlineLeft;
        shopGemTMP.color = new Color(0.95f, 0.55f, 1f, 1f);

        // 8. Category Navigation Bar (Items, Energy, Rewards, Gems)
        GameObject tabsBarGO = CreateUIObject("CategoryTabsBar", windowGO.transform);
        RectTransform tabsBarRT = tabsBarGO.GetComponent<RectTransform>();
        tabsBarRT.anchorMin = new Vector2(0.5f, 1f);
        tabsBarRT.anchorMax = new Vector2(0.5f, 1f);
        tabsBarRT.anchoredPosition = new Vector2(0f, -118f);
        tabsBarRT.sizeDelta = new Vector2(1180f, 62f);

        float tabW = 280f;
        float tabSpacing = 16f;
        float startX = -((4 * tabW + 3 * tabSpacing) / 2f) + (tabW / 2f);

        Button tabItemsBtn = CreateTabButton("Tab_Items", tabsBarGO.transform, "Items", spBtnBlue, font,
            new Vector2(startX + 0 * (tabW + tabSpacing), 0f), new Vector2(tabW, 58f));
        Button tabEnergyBtn = CreateTabButton("Tab_Energy", tabsBarGO.transform, "Energy", spBtnBlue, font,
            new Vector2(startX + 1 * (tabW + tabSpacing), 0f), new Vector2(tabW, 58f));
        Button tabRewardsBtn = CreateTabButton("Tab_Rewards", tabsBarGO.transform, "Rewards", spBtnBlue, font,
            new Vector2(startX + 2 * (tabW + tabSpacing), 0f), new Vector2(tabW, 58f));
        Button tabGemsBtn = CreateTabButton("Tab_Gems", tabsBarGO.transform, "Gems", spBtnBlue, font,
            new Vector2(startX + 3 * (tabW + tabSpacing), 0f), new Vector2(tabW, 58f));

        // 9. Content Area
        GameObject contentAreaGO = CreateUIObject("ContentArea", windowGO.transform);
        RectTransform contentAreaRT = contentAreaGO.GetComponent<RectTransform>();
        contentAreaRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentAreaRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentAreaRT.anchoredPosition = new Vector2(0f, -48f);
        contentAreaRT.sizeDelta = new Vector2(1280f, 550f);

        // A. Content_Items Panel (6 cards: 2 rows of 3)
        GameObject contentItemsGO = CreateUIObject("Content_Items", contentAreaGO.transform);
        SetupPanelFull(contentItemsGO);

        CreateItemCard(contentItemsGO.transform, "Card_0", "Coin Magnet", "Pulls nearby coins", spMagnet, "🪙 100",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(-410f, 135f), new Vector2(380f, 240f));
        CreateItemCard(contentItemsGO.transform, "Card_1", "Freeze Timer", "Freezes level timer", spFreeze, "🪙 100",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(0f, 135f), new Vector2(380f, 240f));
        CreateItemCard(contentItemsGO.transform, "Card_2", "Super Wrench", "Auto-solves next pipe", spWrench, "🪙 150",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(410f, 135f), new Vector2(380f, 240f));

        CreateItemCard(contentItemsGO.transform, "Card_3", "Path Hint", "Shows flow path", spHint, "🪙 50",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(-410f, -125f), new Vector2(380f, 240f));
        CreateItemCard(contentItemsGO.transform, "Card_4", "Bubble Shield", "Shields water pipes", spBubble, "🪙 100",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(0f, -125f), new Vector2(380f, 240f));
        CreateItemCard(contentItemsGO.transform, "Card_5", "Zap Blast", "Clears pipe obstacles", spLightning, "🪙 150",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(410f, -125f), new Vector2(380f, 240f));

        // B. Content_Energy Panel (3 large cards: 1 row of 3)
        GameObject contentEnergyGO = CreateUIObject("Content_Energy", contentAreaGO.transform);
        SetupPanelFull(contentEnergyGO);

        CreateFeatureCard(contentEnergyGO.transform, "Card_0", "5 Energy Refill", "+5 Instant Energy for levels", spLife, "🪙 50",
            spBtnBlue, spBtnGreen, spBtnOrange, font, new Vector2(-410f, 0f), new Vector2(380f, 440f));
        CreateFeatureCard(contentEnergyGO.transform, "Card_1", "Full Energy Refill", "Instantly restore to 30/30 max", spLife, "💎 10",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(0f, 0f), new Vector2(380f, 440f));
        CreateFeatureCard(contentEnergyGO.transform, "Card_2", "1 Hr Unlimited", "Play non-stop with infinite energy", spLightning, "💎 20",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(410f, 0f), new Vector2(380f, 440f));

        // C. Content_Rewards Panel (3 cards: 1 row of 3)
        GameObject contentRewardsGO = CreateUIObject("Content_Rewards", contentAreaGO.transform);
        SetupPanelFull(contentRewardsGO);

        CreateFeatureCard(contentRewardsGO.transform, "Card_0", "Daily Pearl Gift", "+100 Bonus Coins Free!", spShell, "CLAIM",
            spBtnBlue, spBtnGreen, spBtnOrange, font, new Vector2(-410f, 0f), new Vector2(380f, 440f));
        CreateFeatureCard(contentRewardsGO.transform, "Card_1", "Starter Package", "+10 Gems & +200 Coins", spStar, "CLAIM",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(0f, 0f), new Vector2(380f, 440f));
        CreateFeatureCard(contentRewardsGO.transform, "Card_2", "Explorer Bundle", "+15 Gems + 2x Path Hints", spMagnet, "🪙 400",
            spBtnBlue, spBtnGreen, spBtnBlue, font, new Vector2(410f, 0f), new Vector2(380f, 440f));

        // D. Content_Gems Panel (4 cards: 1 row of 4)
        GameObject contentGemsGO = CreateUIObject("Content_Gems", contentAreaGO.transform);
        SetupPanelFull(contentGemsGO);

        float gemW = 295f;
        float gemSpacing = 20f;
        float gStartX = -((4 * gemW + 3 * gemSpacing) / 2f) + (gemW / 2f);

        CreateFeatureCard(contentGemsGO.transform, "Card_0", "Small Gem Pouch", "+5 Shiny Gems", spGem, "🪙 100",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(gStartX + 0 * (gemW + gemSpacing), 0f), new Vector2(gemW, 440f));
        CreateFeatureCard(contentGemsGO.transform, "Card_1", "Medium Gem Bag", "+15 Shiny Gems", spGem, "🪙 250",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(gStartX + 1 * (gemW + gemSpacing), 0f), new Vector2(gemW, 440f));
        CreateFeatureCard(contentGemsGO.transform, "Card_2", "Large Gem Chest", "+35 Shiny Gems", spGem, "🪙 500",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(gStartX + 2 * (gemW + gemSpacing), 0f), new Vector2(gemW, 440f));
        CreateFeatureCard(contentGemsGO.transform, "Card_3", "Treasure Vault", "+80 Shiny Gems", spGem, "🪙 1000",
            spBtnBlue, spBtnGreen, spBtnPurple, font, new Vector2(gStartX + 3 * (gemW + gemSpacing), 0f), new Vector2(gemW, 440f));

        // 10. Status Feedback Toast Banner
        GameObject statusBannerGO = CreateUIObject("StatusFeedbackBanner", windowGO.transform);
        RectTransform statusBannerRT = statusBannerGO.GetComponent<RectTransform>();
        statusBannerRT.anchorMin = new Vector2(0.5f, 1f);
        statusBannerRT.anchorMax = new Vector2(0.5f, 1f);
        statusBannerRT.anchoredPosition = new Vector2(0f, -190f);
        statusBannerRT.sizeDelta = new Vector2(560f, 65f);

        Image statusBannerImg = statusBannerGO.AddComponent<Image>();
        statusBannerImg.sprite = spBtnGreen;
        statusBannerImg.type = Image.Type.Sliced;
        statusBannerImg.color = new Color(0.15f, 0.85f, 0.35f, 0.98f);

        GameObject statusTxtGO = CreateUIObject("StatusText", statusBannerGO.transform);
        RectTransform statusTxtRT = statusTxtGO.GetComponent<RectTransform>();
        statusTxtRT.anchorMin = Vector2.zero;
        statusTxtRT.anchorMax = Vector2.one;
        statusTxtRT.offsetMin = new Vector2(20f, 0f);
        statusTxtRT.offsetMax = new Vector2(-20f, 0f);

        TextMeshProUGUI statusTMP = statusTxtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) statusTMP.font = font;
        statusTMP.text = "Purchased!";
        statusTMP.fontSize = 24;
        statusTMP.fontStyle = FontStyles.Bold;
        statusTMP.alignment = TextAlignmentOptions.Center;
        statusTMP.color = Color.white;
        statusBannerGO.SetActive(false);

        // 11. Wire ShopManager Component on ShopPanel
        ShopManager shopManager = shopPanelGO.GetComponent<ShopManager>();
        if (shopManager == null) shopManager = shopPanelGO.AddComponent<ShopManager>();

        SerializedObject so = new SerializedObject(shopManager);
        so.Update();

        so.FindProperty("shopCoinText").objectReferenceValue = shopCoinTMP;
        so.FindProperty("shopGemText").objectReferenceValue = shopGemTMP;

        so.FindProperty("tabItems").objectReferenceValue = tabItemsBtn;
        so.FindProperty("tabEnergy").objectReferenceValue = tabEnergyBtn;
        so.FindProperty("tabRewards").objectReferenceValue = tabRewardsBtn;
        so.FindProperty("tabGems").objectReferenceValue = tabGemsBtn;

        so.FindProperty("contentItems").objectReferenceValue = contentItemsGO;
        so.FindProperty("contentEnergy").objectReferenceValue = contentEnergyGO;
        so.FindProperty("contentRewards").objectReferenceValue = contentRewardsGO;
        so.FindProperty("contentGems").objectReferenceValue = contentGemsGO;

        so.FindProperty("closeButton").objectReferenceValue = closeBtn;

        so.FindProperty("statusBanner").objectReferenceValue = statusBannerGO;
        so.FindProperty("statusText").objectReferenceValue = statusTMP;
        so.FindProperty("statusImage").objectReferenceValue = statusBannerImg;

        so.ApplyModifiedProperties();

        // 12. Persistent OnClick Listeners via UnityEventTools
        WireButtonClick(closeBtn, shopManager, "CloseShop");
        WireButtonClick(tabItemsBtn, shopManager, "OnTabItemsClicked");
        WireButtonClick(tabEnergyBtn, shopManager, "OnTabEnergyClicked");
        WireButtonClick(tabRewardsBtn, shopManager, "OnTabRewardsClicked");
        WireButtonClick(tabGemsBtn, shopManager, "OnTabGemsClicked");

        // Wire Items Buy Buttons
        WireCardButton(contentItemsGO.transform, "Card_0", shopManager, "BuyItems0");
        WireCardButton(contentItemsGO.transform, "Card_1", shopManager, "BuyItems1");
        WireCardButton(contentItemsGO.transform, "Card_2", shopManager, "BuyItems2");
        WireCardButton(contentItemsGO.transform, "Card_3", shopManager, "BuyItems3");
        WireCardButton(contentItemsGO.transform, "Card_4", shopManager, "BuyItems4");
        WireCardButton(contentItemsGO.transform, "Card_5", shopManager, "BuyItems5");

        // Wire Energy Buy Buttons
        WireCardButton(contentEnergyGO.transform, "Card_0", shopManager, "BuyEnergy0");
        WireCardButton(contentEnergyGO.transform, "Card_1", shopManager, "BuyEnergy1");
        WireCardButton(contentEnergyGO.transform, "Card_2", shopManager, "BuyEnergy2");

        // Wire Rewards Buy Buttons
        WireCardButton(contentRewardsGO.transform, "Card_0", shopManager, "BuyRewards0");
        WireCardButton(contentRewardsGO.transform, "Card_1", shopManager, "BuyRewards1");
        WireCardButton(contentRewardsGO.transform, "Card_2", shopManager, "BuyRewards2");

        // Wire Gems Buy Buttons
        WireCardButton(contentGemsGO.transform, "Card_0", shopManager, "BuyGems0");
        WireCardButton(contentGemsGO.transform, "Card_1", shopManager, "BuyGems1");
        WireCardButton(contentGemsGO.transform, "Card_2", shopManager, "BuyGems2");
        WireCardButton(contentGemsGO.transform, "Card_3", shopManager, "BuyGems3");

        // Initial tab state
        contentItemsGO.SetActive(true);
        contentEnergyGO.SetActive(false);
        contentRewardsGO.SetActive(false);
        contentGemsGO.SetActive(false);

        // Hide shop panel by default so game starts in main menu
        shopPanelGO.SetActive(false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[ShopBuilder] ✅ Successfully built and wired the complete professional Shop Panel!");
    }

    // ── Helper Methods ────────────────────────────────────────────────────────

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go;
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void SetupPanelFull(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void CreateDecorImage(string name, Transform parent, Sprite sp,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        if (sp == null) return;
        GameObject go = CreateUIObject(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.sprite = sp;
        img.preserveAspect = true;
        img.raycastTarget = false;
    }

    private static Button CreateTabButton(string name, Transform parent, string label, Sprite spBtn,
        TMP_FontAsset font, Vector2 pos, Vector2 size)
    {
        GameObject go = CreateUIObject(name, parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.sprite = spBtn;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.08f, 0.28f, 0.48f, 0.9f);

        Button btn = go.AddComponent<Button>();
        go.AddComponent<UIButtonFeedback>();

        GameObject txtGO = CreateUIObject("Label", go.transform);
        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.7f, 0.88f, 1f, 0.85f);

        return btn;
    }

    private static void CreateItemCard(Transform parent, string cardName, string title, string desc,
        Sprite icon, string priceText, Sprite spCard, Sprite spBtnGreen, Sprite spIconBg,
        TMP_FontAsset font, Vector2 pos, Vector2 size)
    {
        GameObject cardGO = CreateUIObject(cardName, parent);
        RectTransform cardRT = cardGO.GetComponent<RectTransform>();
        cardRT.anchoredPosition = pos;
        cardRT.sizeDelta = size;

        Image cardImg = cardGO.AddComponent<Image>();
        cardImg.sprite = spCard;
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.06f, 0.22f, 0.38f, 0.95f); // Deep glossy cyan card background

        // Title
        GameObject titleGO = CreateUIObject("Title", cardGO.transform);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -22f);
        titleRT.sizeDelta = new Vector2(-20f, 32f);

        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        if (font != null) titleTMP.font = font;
        titleTMP.text = title;
        titleTMP.fontSize = 22;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;

        // Icon circular badge
        GameObject badgeGO = CreateUIObject("IconBadge", cardGO.transform);
        RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
        badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
        badgeRT.anchoredPosition = new Vector2(0f, 22f);
        badgeRT.sizeDelta = new Vector2(85f, 85f);

        Image badgeImg = badgeGO.AddComponent<Image>();
        badgeImg.sprite = spIconBg;
        badgeImg.type = Image.Type.Sliced;
        badgeImg.color = new Color(0.04f, 0.16f, 0.30f, 0.9f);

        // Icon
        GameObject iconGO = CreateUIObject("Icon", badgeGO.transform);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = Vector2.zero;
        iconRT.anchorMax = Vector2.one;
        iconRT.offsetMin = new Vector2(8f, 8f);
        iconRT.offsetMax = new Vector2(-8f, -8f);

        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite = icon;
        iconImg.preserveAspect = true;

        // Description
        GameObject descGO = CreateUIObject("Desc", cardGO.transform);
        RectTransform descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0f, 0.5f);
        descRT.anchorMax = new Vector2(1f, 0.5f);
        descRT.anchoredPosition = new Vector2(0f, -32f);
        descRT.sizeDelta = new Vector2(-20f, 24f);

        TextMeshProUGUI descTMP = descGO.AddComponent<TextMeshProUGUI>();
        if (font != null) descTMP.font = font;
        descTMP.text = desc;
        descTMP.fontSize = 15;
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.color = new Color(0.68f, 0.86f, 0.98f, 0.9f);

        // Bright Green Buy Button
        GameObject buyBtnGO = CreateUIObject("BuyButton", cardGO.transform);
        RectTransform buyBtnRT = buyBtnGO.GetComponent<RectTransform>();
        buyBtnRT.anchorMin = new Vector2(0.5f, 0f);
        buyBtnRT.anchorMax = new Vector2(0.5f, 0f);
        buyBtnRT.anchoredPosition = new Vector2(0f, 32f);
        buyBtnRT.sizeDelta = new Vector2(240f, 48f);

        Image buyBtnImg = buyBtnGO.AddComponent<Image>();
        buyBtnImg.sprite = spBtnGreen;
        buyBtnImg.type = Image.Type.Sliced;
        buyBtnImg.color = Color.white;

        buyBtnGO.AddComponent<Button>();
        buyBtnGO.AddComponent<UIButtonFeedback>();

        GameObject buyTxtGO = CreateUIObject("Text", buyBtnGO.transform);
        RectTransform buyTxtRT = buyTxtGO.GetComponent<RectTransform>();
        buyTxtRT.anchorMin = Vector2.zero;
        buyTxtRT.anchorMax = Vector2.one;
        buyTxtRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI buyTMP = buyTxtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) buyTMP.font = font;
        buyTMP.text = priceText;
        buyTMP.fontSize = 22;
        buyTMP.fontStyle = FontStyles.Bold;
        buyTMP.alignment = TextAlignmentOptions.Center;
        buyTMP.color = Color.white;
    }

    private static void CreateFeatureCard(Transform parent, string cardName, string title, string desc,
        Sprite icon, string priceText, Sprite spCard, Sprite spBtnGreen, Sprite spIconBg,
        TMP_FontAsset font, Vector2 pos, Vector2 size)
    {
        GameObject cardGO = CreateUIObject(cardName, parent);
        RectTransform cardRT = cardGO.GetComponent<RectTransform>();
        cardRT.anchoredPosition = pos;
        cardRT.sizeDelta = size;

        Image cardImg = cardGO.AddComponent<Image>();
        cardImg.sprite = spCard;
        cardImg.type = Image.Type.Sliced;
        cardImg.color = new Color(0.06f, 0.22f, 0.38f, 0.95f);

        // Title
        GameObject titleGO = CreateUIObject("Title", cardGO.transform);
        RectTransform titleRT = titleGO.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f);
        titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -28f);
        titleRT.sizeDelta = new Vector2(-20f, 36f);

        TextMeshProUGUI titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        if (font != null) titleTMP.font = font;
        titleTMP.text = title;
        titleTMP.fontSize = 24;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;

        // Large Icon Badge
        GameObject badgeGO = CreateUIObject("IconBadge", cardGO.transform);
        RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0.5f, 0.5f);
        badgeRT.anchorMax = new Vector2(0.5f, 0.5f);
        badgeRT.anchoredPosition = new Vector2(0f, 35f);
        badgeRT.sizeDelta = new Vector2(140f, 140f);

        Image badgeImg = badgeGO.AddComponent<Image>();
        badgeImg.sprite = spIconBg;
        badgeImg.type = Image.Type.Sliced;
        badgeImg.color = new Color(0.04f, 0.16f, 0.30f, 0.9f);

        GameObject iconGO = CreateUIObject("Icon", badgeGO.transform);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = Vector2.zero;
        iconRT.anchorMax = Vector2.one;
        iconRT.offsetMin = new Vector2(12f, 12f);
        iconRT.offsetMax = new Vector2(-12f, -12f);

        Image iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite = icon;
        iconImg.preserveAspect = true;

        // Description
        GameObject descGO = CreateUIObject("Desc", cardGO.transform);
        RectTransform descRT = descGO.GetComponent<RectTransform>();
        descRT.anchorMin = new Vector2(0f, 0.5f);
        descRT.anchorMax = new Vector2(1f, 0.5f);
        descRT.anchoredPosition = new Vector2(0f, -70f);
        descRT.sizeDelta = new Vector2(-30f, 50f);

        TextMeshProUGUI descTMP = descGO.AddComponent<TextMeshProUGUI>();
        if (font != null) descTMP.font = font;
        descTMP.text = desc;
        descTMP.fontSize = 17;
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.color = new Color(0.70f, 0.88f, 1f, 0.95f);

        // Buy Button
        GameObject buyBtnGO = CreateUIObject("BuyButton", cardGO.transform);
        RectTransform buyBtnRT = buyBtnGO.GetComponent<RectTransform>();
        buyBtnRT.anchorMin = new Vector2(0.5f, 0f);
        buyBtnRT.anchorMax = new Vector2(0.5f, 0f);
        buyBtnRT.anchoredPosition = new Vector2(0f, 45f);
        buyBtnRT.sizeDelta = new Vector2(230f, 55f);

        Image buyBtnImg = buyBtnGO.AddComponent<Image>();
        buyBtnImg.sprite = spBtnGreen;
        buyBtnImg.type = Image.Type.Sliced;
        buyBtnImg.color = Color.white;

        buyBtnGO.AddComponent<Button>();
        buyBtnGO.AddComponent<UIButtonFeedback>();

        GameObject buyTxtGO = CreateUIObject("Text", buyBtnGO.transform);
        RectTransform buyTxtRT = buyTxtGO.GetComponent<RectTransform>();
        buyTxtRT.anchorMin = Vector2.zero;
        buyTxtRT.anchorMax = Vector2.one;
        buyTxtRT.sizeDelta = Vector2.zero;

        TextMeshProUGUI buyTMP = buyTxtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) buyTMP.font = font;
        buyTMP.text = priceText;
        buyTMP.fontSize = 24;
        buyTMP.fontStyle = FontStyles.Bold;
        buyTMP.alignment = TextAlignmentOptions.Center;
        buyTMP.color = Color.white;
    }

    private static void WireButtonClick(Button btn, UnityEngine.Object target, string methodName)
    {
        if (btn == null) return;
        var method = target.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (method != null)
        {
            var action = (UnityEngine.Events.UnityAction)System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), target, method);
            UnityEventTools.AddPersistentListener(btn.onClick, action);
        }
    }

    private static void WireCardButton(Transform parent, string cardName, UnityEngine.Object target, string methodName)
    {
        var card = parent.Find(cardName);
        if (card == null) return;
        var btn = card.Find("BuyButton")?.GetComponent<Button>();
        if (btn != null)
        {
            WireButtonClick(btn, target, methodName);
        }
    }
}
#endif
