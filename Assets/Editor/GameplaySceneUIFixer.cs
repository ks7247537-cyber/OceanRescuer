#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fixes GameplayScene UI completely:
///   - LevelCompletePanel: correct scale, full-screen overlay, transparent buttons wired
///   - LevelFailedPanel:   correct scale, full-screen overlay, transparent buttons wired
///   - GemPanel:           creates/wires GemText next to CoinPanel
///   - GameplayHUD:        wires gemText field
///   - All panels hidden at start
///
/// Menu: OceanRescuer > Fix Gameplay Scene UI
/// </summary>
public static class GameplaySceneUIFixer
{
    private const string SCENE_PATH    = "Assets/Scenes/GameplayScene.unity";
    private const string WIN_PNG       = "Assets/Art/UI/Buttons/Underwater Level Complete Game Popup.png";
    private const string FAIL_PNG      = "Assets/Art/UI/Buttons/Ocean Level Failed Popup.png";
    private const string COIN_ICON     = "Assets/Art/UI/Icons/icon_coin.png";
    private const string GEM_ICON      = "Assets/Art/UI/Icons/icon_gem.png";
    private const string GREEN_BTN     = "Assets/Art/UI/Buttons/button_green_blank.png";
    private const string BLUE_BTN      = "Assets/Art/UI/Buttons/button_blue_blank.png";
    private const string ORANGE_BTN    = "Assets/Art/UI/Buttons/button_orange_blank.png";

    static readonly Color OVERLAY_COL  = new Color(0f, 0f, 0f, 0.72f);
    static readonly Color PILL_COIN    = new Color(0.06f, 0.22f, 0.04f, 0.82f);
    static readonly Color PILL_GEM     = new Color(0.16f, 0.04f, 0.28f, 0.82f);
    static readonly Color BTN_HOME     = new Color(0.85f, 0.20f, 0.15f, 1f);
    static readonly Color BTN_RETRY    = new Color(0.15f, 0.65f, 1.00f, 1f);
    static readonly Color BTN_NEXT     = new Color(0.10f, 0.78f, 0.28f, 1f);

    // ── ENTRY POINT ───────────────────────────────────────────────────────────
    [MenuItem("OceanRescuer/Fix Gameplay Scene UI")]
    public static void Run()
    {
        // Open scene
        var scene = EditorSceneManager.GetSceneByPath(SCENE_PATH);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        }

        // Load sprites
        Sprite spWin    = LoadSpr(WIN_PNG);
        Sprite spFail   = LoadSpr(FAIL_PNG);
        Sprite spCoin   = LoadSpr(COIN_ICON);
        Sprite spGem    = LoadSpr(GEM_ICON);
        Sprite spGreen  = LoadSpr(GREEN_BTN);
        Sprite spBlue   = LoadSpr(BLUE_BTN);
        Sprite spOrange = LoadSpr(ORANGE_BTN);

        // Find HUD_Canvas
        Canvas canvas = FindFirst<Canvas>("HUD_Canvas");
        if (canvas == null) { Debug.LogError("[GPFixer] HUD_Canvas not found!"); return; }
        Transform cr = canvas.transform;

        // Find GameplayHUD
        GameplayHUD hud = Object.FindObjectOfType<GameplayHUD>();
        if (hud == null) { Debug.LogError("[GPFixer] GameplayHUD not found!"); return; }

        // 1. Fix LevelCompletePanel
        FixCompletePanel(cr, hud, spWin, spGreen, spOrange, spBlue);

        // 2. Fix LevelFailedPanel
        FixFailedPanel(cr, hud, spFail, spBlue, spGreen);

        // 3. Fix / Create Gem HUD display
        FixGemHUD(cr, hud, spCoin, spGem);

        // 4. Fix Canvas Scaler for 1080×1920
        FixCanvasScaler(canvas);

        // 5. Save
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[GPFixer] ✅ GameplayScene UI fixed and saved.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  LEVEL COMPLETE PANEL
    // ══════════════════════════════════════════════════════════════════════════
    static void FixCompletePanel(Transform canvasRoot, GameplayHUD hud,
        Sprite spWin, Sprite spGreen, Sprite spOrange, Sprite spBlue)
    {
        GameObject panel = FindGO("LevelCompletePanel");
        if (panel == null)
        {
            panel = new GameObject("LevelCompletePanel");
            panel.transform.SetParent(canvasRoot, false);
        }

        // ── Root: full-screen overlay ─────────────────────────────────────────
        var rootRect = EnsureComp<RectTransform>(panel);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
        // Fix scale (was 0.5, 1, 1)
        rootRect.localScale = Vector3.one;

        var rootImg = EnsureComp<Image>(panel);
        rootImg.color = OVERLAY_COL;
        rootImg.raycastTarget = true;

        EnsureComp<CanvasGroup>(panel);
        panel.transform.SetAsLastSibling();

        // ── Popup image (centered card) ───────────────────────────────────────
        GameObject popupGO = FindOrCreateChild(panel.transform, "PopupImage");
        var popupRect = EnsureComp<RectTransform>(popupGO);
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot     = new Vector2(0.5f, 0.5f);
        popupRect.localScale = Vector3.one;
        // Set size based on native resolution — fit within 1080×1920
        popupRect.sizeDelta = new Vector2(1000f, 1200f);
        popupRect.anchoredPosition = Vector2.zero;

        var popupImg = EnsureComp<Image>(popupGO);
        if (spWin != null)
        {
            popupImg.sprite = spWin;
            popupImg.preserveAspect = true;
        }
        else
        {
            popupImg.color = new Color(0.05f, 0.15f, 0.35f, 0.98f);
        }
        popupImg.raycastTarget = false;

        // ── Solve-text (shows how many moves) ─────────────────────────────────
        GameObject movesGO = FindOrCreateChild(popupGO.transform, "SolvedText");
        var movesRect = EnsureComp<RectTransform>(movesGO);
        movesRect.anchorMin = new Vector2(0.1f, 0.65f);
        movesRect.anchorMax = new Vector2(0.9f, 0.80f);
        movesRect.offsetMin = movesRect.offsetMax = Vector2.zero;
        var movesTMP = EnsureComp<TextMeshProUGUI>(movesGO);
        movesTMP.text = ""; // filled at runtime
        movesTMP.fontSize = 38f;
        movesTMP.color = Color.white;
        movesTMP.fontStyle = FontStyles.Bold;
        movesTMP.alignment = TextAlignmentOptions.Center;

        // ── HOME button (bottom-left of popup) ────────────────────────────────
        GameObject homeBtn = FindOrCreateChild(popupGO.transform, "HomeButton");
        SetupTransparentBtn(homeBtn,
            new Vector2(0.04f, 0.04f), new Vector2(0.44f, 0.18f));

        // ── NEXT LEVEL button (bottom-right of popup) ─────────────────────────
        GameObject nextBtn = FindOrCreateChild(popupGO.transform, "NextLevelButton");
        SetupTransparentBtn(nextBtn,
            new Vector2(0.56f, 0.04f), new Vector2(0.96f, 0.18f));

        // ── REPLAY button (bottom-center, smaller) ────────────────────────────
        // Keep existing ReplayButton if it exists as a named child
        GameObject replayBtn = FindOrCreateChild(popupGO.transform, "ReplayButton");
        SetupTransparentBtn(replayBtn,
            new Vector2(0.25f, 0.19f), new Vector2(0.75f, 0.30f));

        // ── Wire GameplayHUD ──────────────────────────────────────────────────
        var so = new SerializedObject(hud);
        so.FindProperty("winPanel").objectReferenceValue      = panel;
        so.FindProperty("winMovesText").objectReferenceValue  = movesTMP;
        so.FindProperty("nextLevelBtn").objectReferenceValue  = nextBtn.GetComponent<Button>();
        so.FindProperty("replayBtnWin").objectReferenceValue  = replayBtn.GetComponent<Button>();
        so.FindProperty("mainMenuBtnWin").objectReferenceValue = homeBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        // Wire onClick
        WireBtn(homeBtn,  hud, "OnMenuClicked");
        WireBtn(nextBtn,  hud, "OnNextLevelClicked");
        WireBtn(replayBtn, hud, "OnRestartClicked");

        panel.SetActive(false);
        Debug.Log("[GPFixer] LevelCompletePanel fixed.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  LEVEL FAILED PANEL
    // ══════════════════════════════════════════════════════════════════════════
    static void FixFailedPanel(Transform canvasRoot, GameplayHUD hud,
        Sprite spFail, Sprite spBlue, Sprite spGreen)
    {
        GameObject panel = FindGO("LevelFailedPanel");
        if (panel == null)
        {
            panel = new GameObject("LevelFailedPanel");
            panel.transform.SetParent(canvasRoot, false);
        }

        // ── Root: full-screen overlay ─────────────────────────────────────────
        var rootRect = EnsureComp<RectTransform>(panel);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;
        rootRect.localScale = Vector3.one;

        var rootImg = EnsureComp<Image>(panel);
        rootImg.color = OVERLAY_COL;
        rootImg.raycastTarget = true;

        EnsureComp<CanvasGroup>(panel);
        panel.transform.SetAsLastSibling();

        // ── Popup image ───────────────────────────────────────────────────────
        GameObject popupGO = FindOrCreateChild(panel.transform, "PopupImage");
        var popupRect = EnsureComp<RectTransform>(popupGO);
        popupRect.anchorMin = new Vector2(0.5f, 0.5f);
        popupRect.anchorMax = new Vector2(0.5f, 0.5f);
        popupRect.pivot     = new Vector2(0.5f, 0.5f);
        popupRect.localScale = Vector3.one;
        popupRect.sizeDelta = new Vector2(1000f, 1100f);
        popupRect.anchoredPosition = Vector2.zero;

        var popupImg = EnsureComp<Image>(popupGO);
        if (spFail != null)
        {
            popupImg.sprite = spFail;
            popupImg.preserveAspect = true;
        }
        else
        {
            popupImg.color = new Color(0.25f, 0.03f, 0.03f, 0.97f);
        }
        popupImg.raycastTarget = false;

        // ── Fail reason text ──────────────────────────────────────────────────
        GameObject reasonGO = FindOrCreateChild(popupGO.transform, "ReasonText");
        var reasonRect = EnsureComp<RectTransform>(reasonGO);
        reasonRect.anchorMin = new Vector2(0.08f, 0.62f);
        reasonRect.anchorMax = new Vector2(0.92f, 0.78f);
        reasonRect.offsetMin = reasonRect.offsetMax = Vector2.zero;
        var reasonTMP = EnsureComp<TextMeshProUGUI>(reasonGO);
        reasonTMP.text = ""; // filled at runtime
        reasonTMP.fontSize = 36f;
        reasonTMP.color = new Color(1f, 0.85f, 0.3f);
        reasonTMP.alignment = TextAlignmentOptions.Center;

        // ── RETRY button (bottom-left) ────────────────────────────────────────
        GameObject retryBtn = FindOrCreateChild(popupGO.transform, "RetryButton");
        SetupTransparentBtn(retryBtn,
            new Vector2(0.04f, 0.04f), new Vector2(0.47f, 0.19f));

        // ── HOME button (bottom-right) ────────────────────────────────────────
        GameObject homeBtn = FindOrCreateChild(popupGO.transform, "HomeButton");
        SetupTransparentBtn(homeBtn,
            new Vector2(0.53f, 0.04f), new Vector2(0.96f, 0.19f));

        // ── Wire GameplayHUD ──────────────────────────────────────────────────
        var so = new SerializedObject(hud);
        so.FindProperty("failPanel").objectReferenceValue      = panel;
        so.FindProperty("failReasonText").objectReferenceValue = reasonTMP;
        so.FindProperty("retryBtn").objectReferenceValue       = retryBtn.GetComponent<Button>();
        so.FindProperty("menuBtn").objectReferenceValue        = homeBtn.GetComponent<Button>();
        so.ApplyModifiedProperties();

        WireBtn(retryBtn, hud, "OnRestartClicked");
        WireBtn(homeBtn,  hud, "OnMenuClicked");

        panel.SetActive(false);
        Debug.Log("[GPFixer] LevelFailedPanel fixed.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GEM HUD DISPLAY
    // ══════════════════════════════════════════════════════════════════════════
    static void FixGemHUD(Transform canvasRoot, GameplayHUD hud,
        Sprite spCoin, Sprite spGem)
    {
        // Fix existing CoinPanel positioning (top-right area)
        GameObject coinPanel = FindGO("CoinPanel");
        if (coinPanel != null)
        {
            var cp = EnsureComp<RectTransform>(coinPanel);
            // Position top-right, leaving room for PauseButton
            cp.anchorMin = new Vector2(0.62f, 0.91f);
            cp.anchorMax = new Vector2(0.85f, 0.99f);
            cp.offsetMin = cp.offsetMax = Vector2.zero;
            cp.localScale = Vector3.one;

            // Fix coin icon sprite
            var iconGO = FindChildNamed(coinPanel.transform, "CoinIcon");
            if (iconGO != null && spCoin != null)
            {
                var img = EnsureComp<Image>(iconGO);
                img.sprite = spCoin; img.preserveAspect = true;
            }
        }

        // Find or create GemPanel (next to CoinPanel, left of it)
        GameObject gemPanel = FindGO("GemPanel");
        if (gemPanel == null)
        {
            gemPanel = new GameObject("GemPanel");
            gemPanel.transform.SetParent(canvasRoot, false);
        }

        var gp = EnsureComp<RectTransform>(gemPanel);
        gp.anchorMin = new Vector2(0.37f, 0.91f);
        gp.anchorMax = new Vector2(0.60f, 0.99f);
        gp.offsetMin = gp.offsetMax = Vector2.zero;
        gp.localScale = Vector3.one;

        // Background pill
        var gpImg = EnsureComp<Image>(gemPanel);
        gpImg.color = PILL_GEM;
        gpImg.raycastTarget = false;

        // HorizontalLayoutGroup
        var hlg = EnsureComp<HorizontalLayoutGroup>(gemPanel);
        hlg.spacing = 6f;
        hlg.padding = new RectOffset(8, 8, 4, 4);
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = hlg.childControlHeight = false;
        hlg.childForceExpandWidth = hlg.childForceExpandHeight = false;

        // Gem Icon
        GameObject gemIconGO = FindOrCreateChild(gemPanel.transform, "GemIcon");
        var giRect = EnsureComp<RectTransform>(gemIconGO);
        giRect.sizeDelta = new Vector2(44f, 44f);
        var giImg = EnsureComp<Image>(gemIconGO);
        if (spGem != null) { giImg.sprite = spGem; giImg.preserveAspect = true; }
        else giImg.color = new Color(0.6f, 0.1f, 0.9f);
        giImg.raycastTarget = false;

        // Gem Text
        GameObject gemTextGO = FindOrCreateChild(gemPanel.transform, "GemText");
        var gtRect = EnsureComp<RectTransform>(gemTextGO);
        gtRect.sizeDelta = new Vector2(100f, 48f);
        var gemTMP = EnsureComp<TextMeshProUGUI>(gemTextGO);
        gemTMP.text = "0";
        gemTMP.fontSize = 32f;
        gemTMP.color = Color.white;
        gemTMP.fontStyle = FontStyles.Bold;
        gemTMP.alignment = TextAlignmentOptions.MidlineLeft;
        gemTMP.raycastTarget = false;

        // Wire gemText into GameplayHUD
        var so = new SerializedObject(hud);
        so.FindProperty("gemText").objectReferenceValue = gemTMP;
        so.ApplyModifiedProperties();

        Debug.Log("[GPFixer] GemPanel created and gemText wired.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CANVAS SCALER
    // ══════════════════════════════════════════════════════════════════════════
    static void FixCanvasScaler(Canvas canvas)
    {
        var cs = canvas.GetComponent<CanvasScaler>();
        if (cs == null) cs = canvas.gameObject.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080f, 1920f);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;
        Debug.Log("[GPFixer] CanvasScaler set to 1080×1920.");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  HELPER: Transparent button setup
    // ══════════════════════════════════════════════════════════════════════════
    static void SetupTransparentBtn(GameObject go,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var rect = EnsureComp<RectTransform>(go);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        var img = EnsureComp<Image>(go);
        img.color = new Color(1f, 1f, 1f, 0f); // fully transparent — still raycast hits
        img.raycastTarget = true;

        var btn = EnsureComp<Button>(go);
        btn.targetGraphic = img;

        // Subtle visual response on press
        var cols = btn.colors;
        cols.normalColor      = new Color(1, 1, 1, 0.00f);
        cols.highlightedColor = new Color(1, 1, 1, 0.10f);
        cols.pressedColor     = new Color(1, 1, 1, 0.25f);
        btn.colors = cols;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  HELPER: Wire onClick
    // ══════════════════════════════════════════════════════════════════════════
    static void WireBtn(GameObject go, Object target, string method)
    {
        if (go == null || target == null) return;
        var btn = go.GetComponent<Button>();
        if (btn == null) return;

        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");

        // Remove any stale entry for same method
        for (int i = calls.arraySize - 1; i >= 0; i--)
        {
            if (calls.GetArrayElementAtIndex(i)
                     .FindPropertyRelative("m_MethodName").stringValue == method)
                calls.DeleteArrayElementAtIndex(i);
        }

        calls.InsertArrayElementAtIndex(calls.arraySize);
        var c = calls.GetArrayElementAtIndex(calls.arraySize - 1);

        // Resolve component that owns the method
        Component comp = null;
        if (target is Component tc)
        {
            foreach (var mb in tc.gameObject.GetComponents<MonoBehaviour>())
            {
                if (mb.GetType().GetMethod(method,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance) != null)
                { comp = mb; break; }
            }
        }

        c.FindPropertyRelative("m_Target").objectReferenceValue  = (Object)comp ?? target;
        c.FindPropertyRelative("m_MethodName").stringValue       = method;
        c.FindPropertyRelative("m_Mode").intValue
            = (int)UnityEngine.Events.PersistentListenerMode.Void;
        c.FindPropertyRelative("m_CallState").intValue
            = (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
        so.ApplyModifiedProperties();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  GENERIC HELPERS
    // ══════════════════════════════════════════════════════════════════════════
    static T EnsureComp<T>(GameObject go) where T : Component
        => go.GetComponent<T>() ?? go.AddComponent<T>();

    static T FindFirst<T>(string name) where T : Object
    {
        foreach (var o in Object.FindObjectsOfType<T>())
            if (o is Component c && c.gameObject.name == name) return o;
        return Object.FindObjectOfType<T>();
    }

    static GameObject FindGO(string name) => GameObject.Find(name);

    static GameObject FindOrCreateChild(Transform parent, string name)
    {
        var existing = parent.Find(name);
        if (existing != null) return existing.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    static GameObject FindChildNamed(Transform parent, string name)
    {
        for (int i = 0; i < parent.childCount; i++)
            if (parent.GetChild(i).name == name)
                return parent.GetChild(i).gameObject;
        return null;
    }

    static Sprite LoadSpr(string path)
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
        if (s == null) Debug.LogWarning("[GPFixer] Sprite not found: " + path);
        return s;
    }
}
#endif
