#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// One-click builder: creates all 5 required scenes for Ocean Rescuers.
/// Menu: OceanRescuer > Build All Scenes
/// 
/// Creates: SplashScene, MainMenuScene, WorldSelectionScene,
///          LevelSelectionScene, GameplayScene
/// Then adds all to Build Settings.
/// </summary>
public static class AllScenesBuilder
{
    private const string SCENE_PATH   = "Assets/Scenes/";
    private const string REF_RES_W    = "1080";
    private const string REF_RES_H    = "1920";

    // Art paths
    private const string ART_BG   = "Assets/Art/Backgrounds/";
    private const string ART_UI_BTN= "Assets/Art/UI/Buttons/";
    private const string ART_UI_ICO= "Assets/Art/UI/Icons/";
    private const string ART_UI_PAN= "Assets/Art/UI/Panels/";
    private const string ART_UI_WC = "Assets/Art/UI/WorldCards/";
    private const string ART_UI_LOG= "Assets/Art/UI/Logo/";

    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("OceanRescuer/Build All Scenes (5 Scenes)")]
    public static void BuildAllScenes()
    {
        if (!EditorUtility.DisplayDialog("Build All Scenes",
            "Yeh script 5 scenes create karegi:\n" +
            "✅ SplashScene\n✅ MainMenuScene\n✅ WorldSelectionScene\n" +
            "✅ LevelSelectionScene\n✅ GameplayScene\n\n" +
            "Existing scenes OVERWRITE nahi hongi.\n" +
            "Continue?", "Build!", "Cancel")) return;

        try
        {
            EnsureFolder(SCENE_PATH);
            EditorUtility.DisplayProgressBar("Building Scenes", "SplashScene...", 0.1f);
            BuildSplashScene();

            EditorUtility.DisplayProgressBar("Building Scenes", "MainMenuScene...", 0.3f);
            BuildMainMenuScene();

            EditorUtility.DisplayProgressBar("Building Scenes", "WorldSelectionScene...", 0.5f);
            BuildWorldSelectionScene();

            EditorUtility.DisplayProgressBar("Building Scenes", "LevelSelectionScene...", 0.7f);
            BuildLevelSelectionScene();

            EditorUtility.DisplayProgressBar("Building Scenes", "GameplayScene...", 0.85f);
            BuildGameplayScene();

            EditorUtility.DisplayProgressBar("Building Scenes", "Build Settings...", 0.95f);
            AddScenesToBuildSettings();

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Done!",
                "5 scenes created!\n\n" +
                "• SplashScene\n• MainMenuScene\n• WorldSelectionScene\n" +
                "• LevelSelectionScene\n• GameplayScene\n\n" +
                "Build Settings mein bhi add ho gayi hain.\n" +
                "File > Build Settings se verify karo.", "OK");
        }
        catch (System.Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[AllScenesBuilder] Error: " + ex.Message + "\n" + ex.StackTrace);
            EditorUtility.DisplayDialog("Error!", ex.Message, "OK");
        }
    }

    // ═════════════════════════════════════════════════════════════════════
    // SCENE 1: SplashScene
    // ═════════════════════════════════════════════════════════════════════
    private static void BuildSplashScene()
    {
        string path = SCENE_PATH + "SplashScene.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var cam = CreateCamera("Main Camera", new Color(0.02f, 0.08f, 0.18f));

        // Canvas
        var canvas = CreateCanvas("SplashCanvas");
        var cg = canvas.AddComponent<CanvasGroup>();

        // Background
        var bg = CreateImage(canvas, "Background",
            LoadSprite(ART_BG + "coral reaf background.png"),
            new Color(1,1,1,1));
        StretchFull(bg);

        // Logo container with CanvasGroup for fade
        var logoPanel = CreateEmptyRect(canvas, "LogoPanel");
        logoPanel.SetAsLastSibling();
        var logoCG = logoPanel.gameObject.AddComponent<CanvasGroup>();
        logoCG.alpha = 0f;
        var logoPanelRT = logoPanel.GetComponent<RectTransform>();
        logoPanelRT.anchorMin = new Vector2(0.1f, 0.3f);
        logoPanelRT.anchorMax = new Vector2(0.9f, 0.75f);
        logoPanelRT.offsetMin = Vector2.zero;
        logoPanelRT.offsetMax = Vector2.zero;

        // Logo image
        var logo = CreateImage(logoPanel.gameObject, "Logo",
            LoadSprite(ART_UI_LOG + "logo.png"), Color.white);
        var logoRT = logo.GetComponent<RectTransform>();
        logoRT.anchorMin = new Vector2(0.1f, 0.1f);
        logoRT.anchorMax = new Vector2(0.9f, 0.9f);
        logoRT.offsetMin = Vector2.zero;
        logoRT.offsetMax = Vector2.zero;

        // Loading text
        var loadingTxt = CreateTMPText(canvas, "LoadingText", "Loading...", 28);
        var loadRT = loadingTxt.GetComponent<RectTransform>();
        loadRT.anchorMin = new Vector2(0.2f, 0.08f);
        loadRT.anchorMax = new Vector2(0.8f, 0.15f);
        loadRT.offsetMin = Vector2.zero;
        loadRT.offsetMax = Vector2.zero;

        // SplashManager
        var mgr = new GameObject("SplashManager");
        var sm = mgr.AddComponent<SplashManager>();
        // Wire logoGroup via SerializedObject
        var so = new SerializedObject(sm);
        so.FindProperty("logoGroup").objectReferenceValue = logoCG;
        so.ApplyModifiedProperties();

        // EventSystem
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[AllScenesBuilder] SplashScene saved: " + path);
    }

    // ═════════════════════════════════════════════════════════════════════
    // SCENE 2: MainMenuScene
    // ═════════════════════════════════════════════════════════════════════
    private static void BuildMainMenuScene()
    {
        string path = SCENE_PATH + "MainMenuScene.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera("Main Camera", new Color(0.02f, 0.08f, 0.18f));
        var canvas = CreateCanvas("MainMenuCanvas");

        // Background
        var bg = CreateImage(canvas, "Background",
            LoadSprite(ART_BG + "coral reaf background.png"), Color.white);
        StretchFull(bg);

        // Logo at top
        var logoImg = CreateImage(canvas, "Logo",
            LoadSprite(ART_UI_LOG + "logo.png"), Color.white);
        var logoRT = logoImg.GetComponent<RectTransform>();
        logoRT.anchorMin = new Vector2(0.15f, 0.72f);
        logoRT.anchorMax = new Vector2(0.85f, 0.95f);
        logoRT.offsetMin = Vector2.zero; logoRT.offsetMax = Vector2.zero;

        // Coin display (top-left)
        var coinPanel = CreateEmptyRect(canvas, "CoinPanel");
        SetAnchors(coinPanel, 0.02f, 0.9f, 0.35f, 0.98f);
        CreateImage(coinPanel.gameObject, "CoinIcon", LoadSprite(ART_UI_ICO + "icon_coin.png"), Color.white);
        var coinTxt = CreateTMPText(coinPanel.gameObject, "CoinText", "0", 32);
        coinTxt.alignment = TextAlignmentOptions.Left;
        var coinRT = coinTxt.GetComponent<RectTransform>();
        coinRT.anchorMin = new Vector2(0.4f, 0f); coinRT.anchorMax = new Vector2(1f, 1f);
        coinRT.offsetMin = Vector2.zero; coinRT.offsetMax = Vector2.zero;

        // Gem display (top-right)
        var gemPanel = CreateEmptyRect(canvas, "GemPanel");
        SetAnchors(gemPanel, 0.65f, 0.9f, 0.98f, 0.98f);
        CreateImage(gemPanel.gameObject, "GemIcon", LoadSprite(ART_UI_ICO + "icon_gem.png"), Color.white);
        var gemTxt = CreateTMPText(gemPanel.gameObject, "GemText", "0", 32);
        gemTxt.alignment = TextAlignmentOptions.Left;

        // Play Button
        var playBtn = CreateButton(canvas, "PlayButton",
            LoadSprite(ART_UI_BTN + "play button.png"), "PLAY", 48);
        SetAnchors(playBtn, 0.2f, 0.52f, 0.8f, 0.70f);

        // Shop Button
        var shopBtn = CreateButton(canvas, "ShopButton",
            LoadSprite(ART_UI_BTN + "button_shop.png"), "SHOP", 36);
        SetAnchors(shopBtn, 0.08f, 0.38f, 0.46f, 0.52f);

        // Settings Button
        var settBtn = CreateButton(canvas, "SettingsButton",
            LoadSprite(ART_UI_BTN + "button_settings.png"), "SETTINGS", 36);
        SetAnchors(settBtn, 0.54f, 0.38f, 0.92f, 0.52f);

        // Shop Panel (hidden)
        var shopPanel = CreatePanel(canvas, "ShopPanel",
            LoadSprite(ART_UI_PAN + "pop up panel.png"));
        shopPanel.SetActive(false);
        CreateTMPText(shopPanel, "ShopTitle", "Shop\n(Coming Soon)", 42);
        var closeShopBtn = CreateButton(shopPanel, "CloseShopBtn",
            LoadSprite(ART_UI_BTN + "close button.png"), "X", 32);
        SetAnchors(closeShopBtn, 0.75f, 0.75f, 0.95f, 0.95f);

        // Settings Panel (hidden)
        var settPanel = CreatePanel(canvas, "SettingsPanel",
            LoadSprite(ART_UI_PAN + "setting panel.png"));
        settPanel.SetActive(false);
        CreateTMPText(settPanel, "SettingsTitle", "Settings", 42);
        var closeSettBtn = CreateButton(settPanel, "CloseSettBtn",
            LoadSprite(ART_UI_BTN + "close button.png"), "X", 32);
        SetAnchors(closeSettBtn, 0.75f, 0.75f, 0.95f, 0.95f);

        // MainMenuManager
        var mgrGO = new GameObject("MainMenuManager");
        var mm = mgrGO.AddComponent<MainMenuManager>();
        var so = new SerializedObject(mm);
        so.FindProperty("shopPanel").objectReferenceValue     = shopPanel;
        so.FindProperty("settingsPanel").objectReferenceValue = settPanel;
        so.FindProperty("coinText").objectReferenceValue      = coinTxt;
        so.FindProperty("gemText").objectReferenceValue       = gemTxt;
        so.ApplyModifiedProperties();

        // Wire buttons
        WireButtonCall(playBtn.GetComponent<Button>(), mgrGO, "OnPlayClicked");
        WireButtonCall(shopBtn.GetComponent<Button>(), mgrGO, "OnShopClicked");
        WireButtonCall(settBtn.GetComponent<Button>(), mgrGO, "OnSettingsClicked");
        WireButtonCall(closeShopBtn.GetComponent<Button>(), mgrGO, "OnCloseShopClicked");
        WireButtonCall(closeSettBtn.GetComponent<Button>(), mgrGO, "OnCloseSettingsClicked");

        // SceneLoader singleton
        CreateSceneLoader();
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[AllScenesBuilder] MainMenuScene saved.");
    }

    // ═════════════════════════════════════════════════════════════════════
    // SCENE 3: WorldSelectionScene
    // ═════════════════════════════════════════════════════════════════════
    private static void BuildWorldSelectionScene()
    {
        string path = SCENE_PATH + "WorldSelectionScene.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera("Main Camera", new Color(0.02f, 0.08f, 0.18f));
        var canvas = CreateCanvas("WorldSelCanvas");

        // Background
        var bg = CreateImage(canvas, "Background",
            LoadSprite(ART_BG + "Deep-Sea Coral Cave background.png"), Color.white);
        StretchFull(bg);

        // Header
        var header = CreateTMPText(canvas, "Header", "Select World", 60);
        header.fontStyle = FontStyles.Bold;
        SetAnchors(header.gameObject, 0.1f, 0.84f, 0.9f, 0.97f);

        // World cards container
        var cardsParent = CreateEmptyRect(canvas, "WorldCards");
        SetAnchors(cardsParent, 0.03f, 0.20f, 0.97f, 0.82f);

        // 3 world cards
        string[] worldNames  = { "Coral Reef", "Deep Ocean", "Lost City" };
        string[] cardSprites = {
            ART_UI_WC + "coral leaf card.png",
            ART_UI_WC + "Deep ocean card.png",
            ART_UI_WC + "lost city card.png"
        };
        float[] xMins = { 0.01f, 0.34f, 0.67f };
        float[] xMaxs = { 0.32f, 0.65f, 0.98f };

        var worldCardData = new System.Collections.Generic.List<SerializedProperty>();
        var mgrGO = new GameObject("WorldSelectionManager");
        var wsm = mgrGO.AddComponent<WorldSelectionManager>();
        var wsmSO = new SerializedObject(wsm);
        var cardsProp = wsmSO.FindProperty("worldCards");
        cardsProp.arraySize = 3;

        for (int i = 0; i < 3; i++)
        {
            var card = CreateEmptyRect(cardsParent.gameObject, $"WorldCard_{i}");
            SetAnchors(card, xMins[i], 0.0f, xMaxs[i], 1.0f);

            // Card background image
            var cardBg = CreateImage(card.gameObject, "CardBg",
                LoadSprite(cardSprites[i]), Color.white);
            StretchFull(cardBg);

            // World name label
            var nameTxt = CreateTMPText(card.gameObject, "WorldName", worldNames[i], 36);
            nameTxt.fontStyle = FontStyles.Bold;
            nameTxt.alignment = TextAlignmentOptions.Center;
            var nameRT = nameTxt.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0,0); nameRT.anchorMax = new Vector2(1, 0.2f);
            nameRT.offsetMin = Vector2.zero; nameRT.offsetMax = Vector2.zero;

            // Locked overlay (shown for worlds 1,2)
            var lockOverlay = CreateImage(card.gameObject, "LockedOverlay",
                LoadSprite(ART_UI_ICO + "icon_locked_gold_shield.png"),
                new Color(0, 0, 0, 0.7f));
            StretchFull(lockOverlay);
            lockOverlay.gameObject.SetActive(i > 0);

            // Button
            var btn = card.gameObject.AddComponent<Button>();

            // Wire card data
            var elem = cardsProp.GetArrayElementAtIndex(i);
            elem.FindPropertyRelative("button").objectReferenceValue         = btn;
            elem.FindPropertyRelative("lockedOverlay").objectReferenceValue  = lockOverlay.gameObject;
            elem.FindPropertyRelative("worldNameText").objectReferenceValue  = nameTxt;
            elem.FindPropertyRelative("worldBannerImage").objectReferenceValue = cardBg;
        }
        wsmSO.ApplyModifiedProperties();

        // Back button
        var backBtn = CreateButton(canvas, "BackButton",
            LoadSprite(ART_UI_BTN + "button_back_arrow.png"), "BACK", 32);
        SetAnchors(backBtn, 0.03f, 0.04f, 0.3f, 0.14f);
        WireButtonCall(backBtn.GetComponent<Button>(), mgrGO, "OnBackClicked");

        CreateSceneLoader();
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[AllScenesBuilder] WorldSelectionScene saved.");
    }

    // ═════════════════════════════════════════════════════════════════════
    // SCENE 4: LevelSelectionScene
    // ═════════════════════════════════════════════════════════════════════
    private static void BuildLevelSelectionScene()
    {
        string path = SCENE_PATH + "LevelSelectionScene.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera("Main Camera", new Color(0.02f, 0.08f, 0.18f));
        var canvas = CreateCanvas("LevelSelCanvas");

        var bg = CreateImage(canvas, "Background",
            LoadSprite(ART_BG + "coral reaf background.png"), Color.white);
        StretchFull(bg);

        var header = CreateTMPText(canvas, "Header", "Select Level", 56);
        header.fontStyle = FontStyles.Bold;
        SetAnchors(header.gameObject, 0.1f, 0.86f, 0.9f, 0.97f);

        // Level buttons grid: 3 columns × 4 rows = 12 levels
        var gridParent = CreateEmptyRect(canvas, "LevelGrid");
        SetAnchors(gridParent, 0.04f, 0.14f, 0.96f, 0.84f);

        int totalLevels = 12;
        int cols = 3, rows = 4;
        var lvlButtons = new GameObject[totalLevels];

        var mgrGO = new GameObject("LevelSelectionManager");
        var lsm = mgrGO.AddComponent<LevelSelectionManager>();
        var lsmSO = new SerializedObject(lsm);
        var btnsProp = lsmSO.FindProperty("levelButtons");
        btnsProp.arraySize = totalLevels;

        for (int i = 0; i < totalLevels; i++)
        {
            int col = i % cols;
            int row = rows - 1 - (i / cols);

            float xMin = col * (1f / cols) + 0.01f;
            float xMax = xMin + (1f / cols) - 0.02f;
            float yMin = row * (1f / rows) + 0.01f;
            float yMax = yMin + (1f / rows) - 0.02f;

            var lvlGO = new GameObject($"Level_{i + 1}");
            lvlGO.transform.SetParent(gridParent.transform, false);
            lvlGO.AddComponent<RectTransform>();
            SetAnchors(lvlGO, xMin, yMin, xMax, yMax);

            // Background button image
            var btnImg = lvlGO.AddComponent<Image>();
            btnImg.sprite = LoadSprite(ART_UI_BTN + "button_blue_blank.png");
            btnImg.type = Image.Type.Sliced;
            var btn = lvlGO.AddComponent<Button>();

            // Level number text
            var numTxt = CreateTMPText(lvlGO, "LevelNum", (i + 1).ToString(), 42);
            numTxt.alignment = TextAlignmentOptions.Center;
            numTxt.fontStyle = FontStyles.Bold;
            var numRT = numTxt.GetComponent<RectTransform>();
            numRT.anchorMin = new Vector2(0.1f, 0.3f); numRT.anchorMax = new Vector2(0.9f, 0.75f);
            numRT.offsetMin = Vector2.zero; numRT.offsetMax = Vector2.zero;

            // 3 stars (small)
            var starsParent = CreateEmptyRect(lvlGO, "Stars");
            var starsRT = starsParent.GetComponent<RectTransform>();
            starsRT.anchorMin = new Vector2(0.05f, 0.68f); starsRT.anchorMax = new Vector2(0.95f, 0.95f);
            starsRT.offsetMin = Vector2.zero; starsRT.offsetMax = Vector2.zero;

            var starImgs = new Image[3];
            for (int s = 0; s < 3; s++)
            {
                var starGO = new GameObject($"Star{s}");
                starGO.transform.SetParent(starsParent.transform, false);
                var starImg = starGO.AddComponent<Image>();
                starImg.sprite = LoadSprite(ART_UI_ICO + "icon_star_gold.png");
                starImg.color = new Color(0.4f, 0.4f, 0.4f);
                var starRT = starGO.GetComponent<RectTransform>();
                float sw = 1f / 3f;
                starRT.anchorMin = new Vector2(s * sw + 0.02f, 0f);
                starRT.anchorMax = new Vector2((s + 1) * sw - 0.02f, 1f);
                starRT.offsetMin = Vector2.zero; starRT.offsetMax = Vector2.zero;
                starImgs[s] = starImg;
            }

            // Lock overlay (locked by default for levels > 0)
            var lockGO = new GameObject("LockedOverlay");
            lockGO.transform.SetParent(lvlGO.transform, false);
            var lockImg = lockGO.AddComponent<Image>();
            lockImg.sprite = LoadSprite(ART_UI_ICO + "icon_locked.png");
            lockImg.color = new Color(0, 0, 0, 0.75f);
            var lockRT = lockGO.GetComponent<RectTransform>();
            lockRT.anchorMin = Vector2.zero; lockRT.anchorMax = Vector2.one;
            lockRT.offsetMin = Vector2.zero; lockRT.offsetMax = Vector2.zero;
            lockGO.SetActive(i > 0); // Level 1 unlocked

            // LevelButton component
            var lb = lvlGO.AddComponent<LevelButton>();
            var lbSO = new SerializedObject(lb);
            var lbStarsProp = lbSO.FindProperty("starImages");
            lbStarsProp.arraySize = 3;
            for (int s = 0; s < 3; s++)
                lbStarsProp.GetArrayElementAtIndex(s).objectReferenceValue = starImgs[s];
            lbSO.FindProperty("lockedOverlay").objectReferenceValue = lockGO;
            lbSO.FindProperty("levelNumberText").objectReferenceValue = numTxt;
            lbSO.ApplyModifiedProperties();

            btnsProp.GetArrayElementAtIndex(i).objectReferenceValue = lb;
            lvlButtons[i] = lvlGO;
        }
        lsmSO.ApplyModifiedProperties();

        // Back button
        var backBtn = CreateButton(canvas, "BackButton",
            LoadSprite(ART_UI_BTN + "button_back_arrow.png"), "BACK", 32);
        SetAnchors(backBtn, 0.03f, 0.02f, 0.3f, 0.12f);
        WireButtonCall(backBtn.GetComponent<Button>(), mgrGO, "OnBackClicked");

        CreateSceneLoader();
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[AllScenesBuilder] LevelSelectionScene saved.");
    }

    // ═════════════════════════════════════════════════════════════════════
    // SCENE 5: GameplayScene
    // ═════════════════════════════════════════════════════════════════════
    private static void BuildGameplayScene()
    {
        string path = SCENE_PATH + "GameplayScene.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = new Color(0.04f, 0.13f, 0.27f);
        camGO.tag = "MainCamera";
        camGO.transform.position = new Vector3(0, 0, -10f);

        // Background
        var bgGO = new GameObject("Background");
        var bgSR = bgGO.AddComponent<SpriteRenderer>();
        bgSR.sprite = LoadSprite(ART_BG + "coral reaf background.png");
        bgSR.sortingOrder = -10;
        bgGO.transform.localScale = new Vector3(5f, 9f, 1f);

        // GameManager container
        var gmGO = new GameObject("GameManager");
        var gm   = gmGO.AddComponent<GameManager>();
        var flow = gmGO.AddComponent<FlowManager>();
        var grid = gmGO.AddComponent<GridManager>();
        var move = gmGO.AddComponent<MoveManager>();

        grid.Cols             = 9;
        grid.Rows             = 5;
        grid.TileSize         = 1.1f;
        grid.useScenePlacedTiles = true;

        flow.sourceCell = new Vector2Int(0, 0);
        flow.sinkCell   = new Vector2Int(8, 0);

        var gmSO   = new SerializedObject(gm);
        gmSO.FindProperty("flowManager").objectReferenceValue = flow;
        gmSO.ApplyModifiedProperties();

        var flowSO = new SerializedObject(flow);
        flowSO.FindProperty("gridManager").objectReferenceValue = grid;
        flowSO.ApplyModifiedProperties();

        // Containers
        var pipeContainer      = new GameObject("PipeContainer");
        var characterContainer = new GameObject("CharacterContainer");
        var obstacleContainer  = new GameObject("ObstacleContainer");

        // ── HUD Canvas ──────────────────────────────────────────────────
        var hudCanvas = CreateCanvas("HUD_Canvas");

        // Top bar background
        var topBar = CreateImage(hudCanvas, "TopBar",
            null, new Color(0, 0.1f, 0.25f, 0.85f));
        SetAnchors(topBar, 0f, 0.90f, 1f, 1.0f);

        // Level text
        var levelTxt = CreateTMPText(hudCanvas, "LevelText", "Level 1", 44);
        levelTxt.alignment = TextAlignmentOptions.Center;
        levelTxt.fontStyle = FontStyles.Bold;
        SetAnchors(levelTxt.gameObject, 0.3f, 0.91f, 0.7f, 0.99f);

        // Moves counter
        var movesTxt = CreateTMPText(hudCanvas, "MovesText", "Moves: 0", 34);
        movesTxt.alignment = TextAlignmentOptions.Left;
        SetAnchors(movesTxt.gameObject, 0.02f, 0.91f, 0.35f, 0.99f);

        // Coin display
        var coinPanel = CreateEmptyRect(hudCanvas, "CoinPanel");
        SetAnchors(coinPanel, 0.65f, 0.91f, 0.98f, 0.99f);
        CreateImage(coinPanel.gameObject, "CoinIcon", LoadSprite(ART_UI_ICO + "icon_coin.png"), Color.white);
        var coinTxt = CreateTMPText(coinPanel.gameObject, "CoinText", "0", 34);

        // Pause button
        var pauseBtn = CreateButton(hudCanvas, "PauseButton",
            LoadSprite(ART_UI_BTN + "button_pause.png"), "", 0);
        SetAnchors(pauseBtn, 0.85f, 0.91f, 0.98f, 0.995f);

        // ── Undo Button ──────────────────────────────────────────────────
        var undoBtn = CreateButton(hudCanvas, "UndoButton",
            LoadSprite(ART_UI_BTN + "button_replay.png"), "Undo(3)", 24);
        SetAnchors(undoBtn, 0.02f, 0.01f, 0.3f, 0.09f);

        // Restart button
        var restartBtn = CreateButton(hudCanvas, "RestartButton",
            LoadSprite(ART_UI_BTN + "restart button.png"), "Restart", 24);
        SetAnchors(restartBtn, 0.35f, 0.01f, 0.65f, 0.09f);

        // ── Pause Panel ──────────────────────────────────────────────────
        var pausePanel = CreatePanel(hudCanvas, "PausePanel",
            LoadSprite(ART_UI_PAN + "existing game panel.png"));
        pausePanel.SetActive(false);
        CreateTMPText(pausePanel, "PauseTitle", "PAUSED", 60);
        var resumeBtn = CreateButton(pausePanel, "ResumeButton",
            LoadSprite(ART_UI_BTN + "button_green_blank.png"), "RESUME", 42);
        SetAnchors(resumeBtn, 0.15f, 0.55f, 0.85f, 0.72f);
        var restartBtnP = CreateButton(pausePanel, "RestartButton",
            LoadSprite(ART_UI_BTN + "restart button.png"), "RESTART", 36);
        SetAnchors(restartBtnP, 0.15f, 0.38f, 0.85f, 0.53f);
        var menuBtnP = CreateButton(pausePanel, "MenuButton",
            LoadSprite(ART_UI_BTN + "button_home.png"), "MENU", 36);
        SetAnchors(menuBtnP, 0.15f, 0.22f, 0.85f, 0.37f);

        // ── Win Panel ────────────────────────────────────────────────────
        var winPanel = CreatePanel(hudCanvas, "LevelCompletePanel",
            LoadSprite(ART_UI_PAN + "reward pop up panel.png"));
        winPanel.SetActive(false);
        var winTitle = CreateTMPText(winPanel, "WinTitle", "RESCUE COMPLETE! 🎉", 48);
        SetAnchors(winTitle.gameObject, 0.05f, 0.68f, 0.95f, 0.88f);
        var winMovesTxt = CreateTMPText(winPanel, "WinMovesText", "Solved in 0 moves!", 34);
        SetAnchors(winMovesTxt.gameObject, 0.05f, 0.52f, 0.95f, 0.66f);
        var nextBtn = CreateButton(winPanel, "NextLevelButton",
            LoadSprite(ART_UI_BTN + "next level button.png"), "NEXT", 40);
        SetAnchors(nextBtn, 0.1f, 0.30f, 0.9f, 0.50f);
        var replayWin = CreateButton(winPanel, "ReplayButton",
            LoadSprite(ART_UI_BTN + "replay button.png"), "REPLAY", 36);
        SetAnchors(replayWin, 0.1f, 0.12f, 0.9f, 0.28f);
        var menuWin = CreateButton(winPanel, "MenuButton",
            LoadSprite(ART_UI_BTN + "button_home.png"), "MENU", 32);
        SetAnchors(menuWin, 0.1f, 0.02f, 0.5f, 0.12f);

        // ── Fail Panel ───────────────────────────────────────────────────
        var failPanel = CreatePanel(hudCanvas, "LevelFailedPanel",
            LoadSprite(ART_UI_PAN + "existing game popup panel.png"));
        failPanel.SetActive(false);
        var failTitle = CreateTMPText(failPanel, "FailTitle", "LEVEL FAILED!", 52);
        SetAnchors(failTitle.gameObject, 0.05f, 0.68f, 0.95f, 0.88f);
        var failTxt = CreateTMPText(failPanel, "FailReasonText", "Move limit reached!", 34);
        SetAnchors(failTxt.gameObject, 0.05f, 0.52f, 0.95f, 0.66f);
        var retryBtn = CreateButton(failPanel, "RetryButton",
            LoadSprite(ART_UI_BTN + "retry button.png"), "RETRY", 40);
        SetAnchors(retryBtn, 0.1f, 0.30f, 0.9f, 0.50f);
        var menuFail = CreateButton(failPanel, "MenuButton",
            LoadSprite(ART_UI_BTN + "button_home.png"), "MENU", 36);
        SetAnchors(menuFail, 0.1f, 0.12f, 0.9f, 0.28f);

        // ── GameplayHUD script ───────────────────────────────────────────
        var hudGO = new GameObject("GameplayHUD");
        var hud = hudGO.AddComponent<GameplayHUD>();
        var hudSO = new SerializedObject(hud);
        hudSO.FindProperty("levelText").objectReferenceValue     = levelTxt;
        hudSO.FindProperty("moveCountText").objectReferenceValue = movesTxt;
        hudSO.FindProperty("coinText").objectReferenceValue      = coinTxt;
        hudSO.FindProperty("pauseButton").objectReferenceValue   = pauseBtn.GetComponent<Button>();
        hudSO.FindProperty("pausePanel").objectReferenceValue    = pausePanel;
        hudSO.FindProperty("undoButton").objectReferenceValue    = undoBtn.GetComponent<Button>();
        hudSO.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
        hudSO.FindProperty("winPanel").objectReferenceValue      = winPanel;
        hudSO.FindProperty("winMovesText").objectReferenceValue  = winMovesTxt;
        hudSO.FindProperty("nextLevelBtn").objectReferenceValue  = nextBtn.GetComponent<Button>();
        hudSO.FindProperty("replayBtnWin").objectReferenceValue  = replayWin.GetComponent<Button>();
        hudSO.FindProperty("mainMenuBtnWin").objectReferenceValue= menuWin.GetComponent<Button>();
        hudSO.FindProperty("failPanel").objectReferenceValue     = failPanel;
        hudSO.FindProperty("failReasonText").objectReferenceValue= failTxt;
        hudSO.FindProperty("retryBtn").objectReferenceValue      = retryBtn.GetComponent<Button>();
        hudSO.FindProperty("menuBtn").objectReferenceValue       = menuFail.GetComponent<Button>();
        hudSO.ApplyModifiedProperties();

        // Wire GameManager → hud
        gmSO.FindProperty("hud").objectReferenceValue    = hud;
        gmSO.FindProperty("winPanel").objectReferenceValue= winPanel;
        gmSO.ApplyModifiedProperties();

        // Wire pause/resume/menu button calls
        WireButtonCall(pauseBtn.GetComponent<Button>(), hudGO, "OnPauseClicked");
        WireButtonCall(resumeBtn.GetComponent<Button>(), hudGO, "OnResumeClicked");
        WireButtonCall(restartBtnP.GetComponent<Button>(), hudGO, "OnRestartClicked");
        WireButtonCall(menuBtnP.GetComponent<Button>(), hudGO, "OnMenuClicked");
        WireButtonCall(restartBtn.GetComponent<Button>(), hudGO, "OnRestartClicked");
        WireButtonCall(nextBtn.GetComponent<Button>(), hudGO, "OnNextLevelClicked");
        WireButtonCall(replayWin.GetComponent<Button>(), hudGO, "OnRestartClicked");
        WireButtonCall(menuWin.GetComponent<Button>(), hudGO, "OnMenuClicked");
        WireButtonCall(retryBtn.GetComponent<Button>(), hudGO, "OnRestartClicked");
        WireButtonCall(menuFail.GetComponent<Button>(), hudGO, "OnMenuClicked");

        CreateSceneLoader();
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("[AllScenesBuilder] GameplayScene saved.");
    }

    // ═════════════════════════════════════════════════════════════════════
    // BUILD SETTINGS
    // ═════════════════════════════════════════════════════════════════════
    private static void AddScenesToBuildSettings()
    {
        var scenes = new[]
        {
            "Assets/Scenes/SplashScene.unity",
            "Assets/Scenes/MainMenuScene.unity",
            "Assets/Scenes/WorldSelectionScene.unity",
            "Assets/Scenes/LevelSelectionScene.unity",
            "Assets/Scenes/GameplayScene.unity",
            "Assets/Scenes/Level01.unity",
        };

        var builds = new EditorBuildSettingsScene[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            builds[i] = new EditorBuildSettingsScene(scenes[i], true);

        EditorBuildSettings.scenes = builds;
        Debug.Log("[AllScenesBuilder] Build Settings updated with 6 scenes.");
    }

    // ═════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═════════════════════════════════════════════════════════════════════

    private static Camera CreateCamera(string name, Color bgColor)
    {
        var go = new GameObject(name);
        var cam = go.AddComponent<Camera>();
        go.AddComponent<AudioListener>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.backgroundColor = bgColor;
        cam.clearFlags = CameraClearFlags.SolidColor;
        go.tag = "MainCamera";
        go.transform.position = new Vector3(0, 0, -10f);
        return cam;
    }

    private static GameObject CreateCanvas(string name)
    {
        var go = new GameObject(name);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private static Image CreateImage(GameObject parent, string name, Sprite sprite, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        img.color = color;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return img;
    }

    private static TextMeshProUGUI CreateTMPText(GameObject parent, string name, string text, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.05f);
        rt.anchorMax = new Vector2(0.95f, 0.95f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return tmp;
    }

    private static GameObject CreateButton(GameObject parent, string name, Sprite sprite, string label, int fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);

        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        else img.color = new Color(0.2f, 0.6f, 1f);

        var btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = colors;

        if (!string.IsNullOrEmpty(label) && fontSize > 0)
        {
            var lbl = new GameObject("Label");
            lbl.transform.SetParent(go.transform, false);
            var tmp = lbl.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            var rt = lbl.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        var goRT = go.GetComponent<RectTransform>();
        goRT.anchorMin = new Vector2(0.1f, 0.1f);
        goRT.anchorMax = new Vector2(0.9f, 0.9f);
        goRT.offsetMin = Vector2.zero;
        goRT.offsetMax = Vector2.zero;
        return go;
    }

    private static GameObject CreatePanel(GameObject parent, string name, Sprite sprite)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.AddComponent<Image>();
        if (sprite != null) img.sprite = sprite;
        else img.color = new Color(0, 0.05f, 0.15f, 0.95f);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.2f);
        rt.anchorMax = new Vector2(0.95f, 0.85f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    private static Transform CreateEmptyRect(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        return go.transform;
    }

    private static Transform CreateEmptyRect(Transform parent, string name)
        => CreateEmptyRect(parent.gameObject, name);

    private static void StretchFull(Component comp)
    {
        var rt = comp.GetComponent<RectTransform>();
        if (rt == null) rt = comp.gameObject.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(Component comp, float xMin, float yMin, float xMax, float yMax)
    {
        var rt = comp.GetComponent<RectTransform>();
        if (rt == null) rt = comp.gameObject.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            // Try loading first sprite from spritesheet
            var objs = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var o in objs) if (o is Sprite s) { sprite = s; break; }
        }
        return sprite; // null is acceptable — image will show white box
    }

    private static void WireButtonCall(Button btn, GameObject target, string methodName)
    {
        if (btn == null || target == null) return;
        var so = new SerializedObject(btn);
        var onClick = so.FindProperty("m_OnClick");
        var calls = onClick.FindPropertyRelative("m_PersistentCalls.m_Calls");
        int idx = calls.arraySize;
        calls.InsertArrayElementAtIndex(idx);
        var call = calls.GetArrayElementAtIndex(idx);

        // Find the first MonoBehaviour on target that has the method
        MonoBehaviour comp = null;
        foreach (var mb in target.GetComponents<MonoBehaviour>())
        {
            var type = mb.GetType();
            if (type.GetMethod(methodName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance) != null)
            { comp = mb; break; }
        }

        call.FindPropertyRelative("m_Target").objectReferenceValue
            = (comp != null) ? (UnityEngine.Object)comp : target.transform;
        call.FindPropertyRelative("m_MethodName").stringValue = methodName;
        call.FindPropertyRelative("m_Mode").intValue
            = (int)UnityEngine.Events.PersistentListenerMode.Void;
        call.FindPropertyRelative("m_CallState").intValue
            = (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
        so.ApplyModifiedProperties();
    }

    private static void CreateEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null) return;
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private static void CreateSceneLoader()
    {
        if (Object.FindObjectOfType<SceneLoader>() != null) return;
        var go = new GameObject("SceneLoader");
        go.AddComponent<SceneLoader>();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path.TrimEnd('/'))) return;
        string[] parts = path.TrimEnd('/').Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string nxt = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(nxt))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = nxt;
        }
    }
}
#endif
