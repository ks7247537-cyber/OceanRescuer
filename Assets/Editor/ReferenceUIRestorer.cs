using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ReferenceUIRestorer:
/// Restores and aligns UI across all scenes to match the 15-screen reference image (media_1795344354527_80a011ba.jpg)
/// using assets existing in Assets/Art/.
/// - Fixes PauseButton scale, position, and hit-testing so clicking it reliably opens PausePanel.
/// - Fixes PausePanel scale, background sprite (paused button.png), and child buttons.
/// - Fixes LevelComplete and LevelFailed panels and button scales.
/// - Fixes BackButton on WorldSelection and LevelSelection scenes.
/// - Ensures all scales are Vector3.one (no squished buttons) and preserveAspect is true.
/// </summary>
public static class ReferenceUIRestorer
{
    private const string ART_BTN = "Assets/Art/UI/Buttons/";
    private const string ART_PAN = "Assets/Art/UI/Panels/";
    private const string ART_WLD = "Assets/Art/UI/WorldCards/";
    private const string ART_ICO = "Assets/Art/UI/Icons/";

    [MenuItem("Ocean Rescuers/Restore Reference UI Layouts")]
    public static void RestoreAll()
    {
        RestoreGameplayScene();
        RestoreWorldSelectionScene();
        RestoreLevelSelectionScene();
        RestoreMainMenuScene();
        AssetDatabase.SaveAssets();
        Debug.Log("[ReferenceUIRestorer] All scenes successfully restored to match Reference UI!");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // GAMEPLAY SCENE
    // ──────────────────────────────────────────────────────────────────────────
    public static void RestoreGameplayScene()
    {
        string scenePath = "Assets/Scenes/GameplayScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[ReferenceUIRestorer] No Canvas found in GameplayScene.");
            return;
        }

        // Configure CanvasScaler for Landscape (1920x1080)
        var cs = canvas.GetComponent<CanvasScaler>();
        if (cs != null)
        {
            cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920f, 1080f);
            cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight = 1f; // match height
        }

        var hud = Object.FindObjectOfType<GameplayHUD>();

        // 1. Pause Button (Top-Right Circular Blue Button with ||)
        var pauseBtnGO = GameObject.Find("PauseButton");
        if (pauseBtnGO != null)
        {
            var rt = pauseBtnGO.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0.92f, 0.895f);
            rt.anchorMax = new Vector2(0.985f, 0.985f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var img = pauseBtnGO.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "button_pause.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
                img.raycastTarget = true;
                img.color = Color.white;
            }

            var btn = pauseBtnGO.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = true;
                btn.transition = Selectable.Transition.ColorTint;
            }
        }

        // 2. Pause Panel (Screen 6: Wooden board with leaves, Resumed, Restart, Settings, Main Menu)
        var pausePanelGO = GameObject.Find("PausePanel");
        if (pausePanelGO != null)
        {
            pausePanelGO.SetActive(false); // keep inactive by default
            var rt = pausePanelGO.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            // Center the wooden board nicely on 1920x1080
            rt.anchorMin = new Vector2(0.33f, 0.10f);
            rt.anchorMax = new Vector2(0.67f, 0.90f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var img = pausePanelGO.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "paused button.png");
                if (sp == null) sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_PAN + "paused button.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
                img.raycastTarget = true;
                img.color = Color.white;
            }

            // Disable raycast on PauseTitle so it doesn't block buttons
            var pauseTitle = pausePanelGO.transform.Find("PauseTitle");
            if (pauseTitle != null)
            {
                var tmp = pauseTitle.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.raycastTarget = false;
                    tmp.text = ""; // The wooden sprite already has "Paused" carved at the top!
                }
            }

            // ResumeButton (Green pill)
            var resumeGO = pausePanelGO.transform.Find("ResumeButton");
            if (resumeGO != null)
            {
                var rrt = resumeGO.GetComponent<RectTransform>();
                rrt.localScale = Vector3.one;
                rrt.anchorMin = new Vector2(0.18f, 0.58f);
                rrt.anchorMax = new Vector2(0.82f, 0.70f);
                rrt.offsetMin = rrt.offsetMax = Vector2.zero;
                var rimg = resumeGO.GetComponent<Image>();
                if (rimg != null)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "resume button.png");
                    if (sp != null) rimg.sprite = sp;
                    rimg.preserveAspect = true;
                    rimg.raycastTarget = true;
                    rimg.color = Color.white;
                }
                var txt = resumeGO.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
            }

            // RestartButton (Orange pill)
            var restartGO = pausePanelGO.transform.Find("RestartButton");
            if (restartGO != null)
            {
                var rrt = restartGO.GetComponent<RectTransform>();
                rrt.localScale = Vector3.one;
                rrt.anchorMin = new Vector2(0.18f, 0.44f);
                rrt.anchorMax = new Vector2(0.82f, 0.56f);
                rrt.offsetMin = rrt.offsetMax = Vector2.zero;
                var rimg = restartGO.GetComponent<Image>();
                if (rimg != null)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "restart button.png");
                    if (sp != null) rimg.sprite = sp;
                    rimg.preserveAspect = true;
                    rimg.raycastTarget = true;
                    rimg.color = Color.white;
                }
                var txt = restartGO.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
            }

            // SettingsButton (Blue pill - Screen 6)
            var settingsGO = pausePanelGO.transform.Find("SettingsButton");
            if (settingsGO == null)
            {
                var newSetGO = new GameObject("SettingsButton");
                newSetGO.transform.SetParent(pausePanelGO.transform, false);
                settingsGO = newSetGO.transform;
                newSetGO.AddComponent<RectTransform>();
                newSetGO.AddComponent<CanvasRenderer>();
                newSetGO.AddComponent<Image>();
                newSetGO.AddComponent<Button>();
            }
            if (settingsGO != null)
            {
                var srt = settingsGO.GetComponent<RectTransform>();
                srt.localScale = Vector3.one;
                srt.anchorMin = new Vector2(0.18f, 0.30f);
                srt.anchorMax = new Vector2(0.82f, 0.42f);
                srt.offsetMin = srt.offsetMax = Vector2.zero;
                var simg = settingsGO.GetComponent<Image>();
                if (simg != null)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "button_settings.png");
                    if (sp != null) simg.sprite = sp;
                    simg.preserveAspect = true;
                    simg.raycastTarget = true;
                    simg.color = Color.white;
                }
            }

            // MenuButton (Purple pill)
            var menuGO = pausePanelGO.transform.Find("MenuButton");
            if (menuGO != null)
            {
                var mrt = menuGO.GetComponent<RectTransform>();
                mrt.localScale = Vector3.one;
                mrt.anchorMin = new Vector2(0.18f, 0.16f);
                mrt.anchorMax = new Vector2(0.82f, 0.28f);
                mrt.offsetMin = mrt.offsetMax = Vector2.zero;
                var mimg = menuGO.GetComponent<Image>();
                if (mimg != null)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "main menu button.png");
                    if (sp != null) mimg.sprite = sp;
                    mimg.preserveAspect = true;
                    mimg.raycastTarget = true;
                    mimg.color = Color.white;
                }
                var txt = menuGO.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
            }
        }

        // 3. Level Complete Panel (Screen 7: Underwater Level Complete Game Popup.png)
        var winPanelGO = GameObject.Find("LevelCompletePanel");
        if (winPanelGO != null)
        {
            winPanelGO.SetActive(false);
            var rt = winPanelGO.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0.25f, 0.08f);
            rt.anchorMax = new Vector2(0.75f, 0.92f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            var img = winPanelGO.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "Underwater Level Complete Game Popup.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
                img.color = Color.white;
            }

            // Align buttons inside LevelComplete
            var nextBtn = winPanelGO.transform.Find("NextLevelButton");
            if (nextBtn != null)
            {
                var nrt = nextBtn.GetComponent<RectTransform>();
                nrt.localScale = Vector3.one;
                nrt.anchorMin = new Vector2(0.22f, 0.24f);
                nrt.anchorMax = new Vector2(0.78f, 0.36f);
                nrt.offsetMin = nrt.offsetMax = Vector2.zero;
                var nimg = nextBtn.GetComponent<Image>();
                if (nimg != null)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "next level button.png");
                    if (sp != null) nimg.sprite = sp;
                    nimg.preserveAspect = true;
                    nimg.raycastTarget = true;
                }
                var txt = nextBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
            }

            var replayBtn = winPanelGO.transform.Find("ReplayButton");
            if (replayBtn != null)
            {
                var rrt = replayBtn.GetComponent<RectTransform>();
                rrt.localScale = Vector3.one;
                rrt.anchorMin = new Vector2(0.25f, 0.10f);
                rrt.anchorMax = new Vector2(0.75f, 0.22f);
                rrt.offsetMin = rrt.offsetMax = Vector2.zero;
                var rimg = replayBtn.GetComponent<Image>();
                if (rimg != null)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "replay button.png");
                    if (sp != null) rimg.sprite = sp;
                    rimg.preserveAspect = true;
                    rimg.raycastTarget = true;
                }
                var txt = replayBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
            }

            var winTitle = winPanelGO.transform.Find("WinTitle");
            if (winTitle != null)
            {
                var tmp = winTitle.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.raycastTarget = false; tmp.text = ""; } // Banner image already says Level Complete!
            }
        }

        // 4. Level Failed Panel (Screen 8: Ocean Level Failed Popup.png)
        var failPanelGO = GameObject.Find("LevelFailedPanel");
        if (failPanelGO != null)
        {
            failPanelGO.SetActive(false);
            var rt = failPanelGO.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0.26f, 0.08f);
            rt.anchorMax = new Vector2(0.74f, 0.92f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            var img = failPanelGO.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "Ocean Level Failed Popup.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
                img.color = Color.white;
            }

            var retryBtn = failPanelGO.transform.Find("RetryButton");
            if (retryBtn != null)
            {
                var rrt = retryBtn.GetComponent<RectTransform>();
                rrt.localScale = Vector3.one;
                rrt.anchorMin = new Vector2(0.15f, 0.12f);
                rrt.anchorMax = new Vector2(0.48f, 0.24f);
                rrt.offsetMin = rrt.offsetMax = Vector2.zero;
                var rimg = retryBtn.GetComponent<Image>();
                if (rimg != null)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "retry button.png");
                    if (sp != null) rimg.sprite = sp;
                    rimg.preserveAspect = true;
                    rimg.raycastTarget = true;
                }
                var txt = retryBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
            }

            var menuBtn = failPanelGO.transform.Find("MenuButton");
            if (menuBtn != null)
            {
                var mrt = menuBtn.GetComponent<RectTransform>();
                mrt.localScale = Vector3.one;
                mrt.anchorMin = new Vector2(0.52f, 0.12f);
                mrt.anchorMax = new Vector2(0.85f, 0.24f);
                mrt.offsetMin = mrt.offsetMax = Vector2.zero;
                var mimg = menuBtn.GetComponent<Image>();
                if (mimg != null)
                {
                    var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "main menu button.png");
                    if (sp != null) mimg.sprite = sp;
                    mimg.preserveAspect = true;
                    mimg.raycastTarget = true;
                }
                var txt = menuBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
            }

            var failTitle = failPanelGO.transform.Find("FailTitle");
            if (failTitle != null)
            {
                var tmp = failTitle.GetComponent<TextMeshProUGUI>();
                if (tmp != null) { tmp.raycastTarget = false; tmp.text = ""; }
            }
        }

        // 5. Wire references on GameplayHUD
        if (hud != null)
        {
            var so = new SerializedObject(hud);
            if (pauseBtnGO != null)
                so.FindProperty("pauseButton").objectReferenceValue = pauseBtnGO.GetComponent<Button>();
            if (pausePanelGO != null)
            {
                so.FindProperty("pausePanel").objectReferenceValue = pausePanelGO;
                var res = pausePanelGO.transform.Find("ResumeButton")?.GetComponent<Button>();
                var rst = pausePanelGO.transform.Find("RestartButton")?.GetComponent<Button>();
                var mnu = pausePanelGO.transform.Find("MenuButton")?.GetComponent<Button>();
                var set = pausePanelGO.transform.Find("SettingsButton")?.GetComponent<Button>();

                var pRes = so.FindProperty("resumeButton");
                if (pRes != null) pRes.objectReferenceValue = res;
                var pRst = so.FindProperty("pauseRestartBtn");
                if (pRst != null) pRst.objectReferenceValue = rst;
                var pMnu = so.FindProperty("pauseMenuBtn");
                if (pMnu != null) pMnu.objectReferenceValue = mnu;
                var pSet = so.FindProperty("pauseSettingsBtn");
                if (pSet != null) pSet.objectReferenceValue = set;
            }
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ReferenceUIRestorer] GameplayScene UI layout successfully restored!");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // WORLD SELECTION SCENE
    // ──────────────────────────────────────────────────────────────────────────
    public static void RestoreWorldSelectionScene()
    {
        string scenePath = "Assets/Scenes/WorldSelectionScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var cs = canvas.GetComponent<CanvasScaler>();
            if (cs != null)
            {
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920f, 1080f);
                cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                cs.matchWidthOrHeight = 1f;
            }
        }

        // Back Button (Top-Left Circular Yellow Button with back arrow)
        var backBtnGO = GameObject.Find("BackButton");
        if (backBtnGO != null)
        {
            var rt = backBtnGO.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0.02f, 0.88f);
            rt.anchorMax = new Vector2(0.08f, 0.98f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var img = backBtnGO.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "button_back_arrow.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
                img.color = Color.white;
            }

            var txt = backBtnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
        }

        // World Cards
        var card0 = GameObject.Find("WorldCard_0");
        if (card0 != null)
        {
            var rt = card0.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var img = card0.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_WLD + "coral leaf card.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
            }
        }

        var card1 = GameObject.Find("WorldCard_1");
        if (card1 != null)
        {
            var rt = card1.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var img = card1.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_WLD + "Deep ocean card.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
            }
        }

        var card2 = GameObject.Find("WorldCard_2");
        if (card2 != null)
        {
            var rt = card2.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var img = card2.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_WLD + "lost city card.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ReferenceUIRestorer] WorldSelectionScene UI layout successfully restored!");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // LEVEL SELECTION SCENE
    // ──────────────────────────────────────────────────────────────────────────
    public static void RestoreLevelSelectionScene()
    {
        string scenePath = "Assets/Scenes/LevelSelectionScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var cs = canvas.GetComponent<CanvasScaler>();
            if (cs != null)
            {
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920f, 1080f);
                cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                cs.matchWidthOrHeight = 1f;
            }
        }

        // Back Button (Top-Left Circular Yellow Button with back arrow)
        var backBtnGO = GameObject.Find("BackButton");
        if (backBtnGO != null)
        {
            var rt = backBtnGO.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.anchorMin = new Vector2(0.02f, 0.88f);
            rt.anchorMax = new Vector2(0.08f, 0.98f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;

            var img = backBtnGO.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "button_back_arrow.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
                img.color = Color.white;
            }

            var txt = backBtnGO.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) { txt.raycastTarget = false; txt.text = ""; }
        }

        // Ensure all LevelButtons have scale Vector3.one
        var buttons = Object.FindObjectsOfType<LevelButton>(true);
        foreach (var lb in buttons)
        {
            var rt = lb.GetComponent<RectTransform>();
            if (rt != null) rt.localScale = Vector3.one;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ReferenceUIRestorer] LevelSelectionScene UI layout successfully restored!");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // MAIN MENU SCENE
    // ──────────────────────────────────────────────────────────────────────────
    public static void RestoreMainMenuScene()
    {
        string scenePath = "Assets/Scenes/MainMenuScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var cs = canvas.GetComponent<CanvasScaler>();
            if (cs != null)
            {
                cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                cs.referenceResolution = new Vector2(1920f, 1080f);
                cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                cs.matchWidthOrHeight = 1f;
            }
        }

        // Align Main Menu buttons matching Screen 2
        var playBtn = GameObject.Find("PlayButton");
        if (playBtn != null)
        {
            var rt = playBtn.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var img = playBtn.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "play button.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
            }
            var txt = playBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) { txt.raycastTarget = false; }
        }

        var shopBtn = GameObject.Find("ShopButton");
        if (shopBtn != null)
        {
            var rt = shopBtn.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var img = shopBtn.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "button_shop.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
            }
            var txt = shopBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) { txt.raycastTarget = false; }
        }

        var setBtn = GameObject.Find("SettingsButton");
        if (setBtn != null)
        {
            var rt = setBtn.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var img = setBtn.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "button_settings.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
            }
            var txt = setBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) { txt.raycastTarget = false; }
        }

        var quitBtn = GameObject.Find("QuitButton") ?? GameObject.Find("ExitButton");
        if (quitBtn != null)
        {
            var rt = quitBtn.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            var img = quitBtn.GetComponent<Image>();
            if (img != null)
            {
                var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ART_BTN + "button_exit.png");
                if (sp != null) img.sprite = sp;
                img.preserveAspect = true;
            }
            var txt = quitBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) { txt.raycastTarget = false; }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ReferenceUIRestorer] MainMenuScene UI layout successfully restored!");
    }
}
