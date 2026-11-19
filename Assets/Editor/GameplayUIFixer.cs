#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Fixes the Gameplay UI panels:
///  - Replaces LevelCompletePanel image with Underwater Level Complete Game Popup.png
///  - Replaces LevelFailedPanel image with Ocean Level Failed Popup.png
///  - Adds transparent Home + NextLevel buttons on Complete panel
///  - Adds transparent Retry + Home buttons on Failed panel
///  - Adds Gem display next to Coin display in HUD
///  - Wires all button onClick events
///
/// Menu: OceanRescuer > Fix Gameplay UI Panels
/// </summary>
public static class GameplayUIFixer
{
    private const string SCENE_PATH   = "Assets/Scenes/GameplayScene.unity";
    private const string WIN_PNG_PATH = "Assets/Art/UI/Buttons/Underwater Level Complete Game Popup.png";
    private const string FAIL_PNG_PATH= "Assets/Art/UI/Buttons/Ocean Level Failed Popup.png";
    private const string COIN_ICON    = "Assets/Art/UI/Icons/icon_coin.png";
    private const string GEM_ICON     = "Assets/Art/UI/Icons/icon_gem.png";

    // ── Entry Point ────────────────────────────────────────────────────────────
    [MenuItem("OceanRescuer/Fix Gameplay UI Panels")]
    public static void FixGameplayUI()
    {
        // Open GameplayScene
        var scene = EditorSceneManager.GetSceneByPath(SCENE_PATH);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            else { Debug.LogWarning("[UIFixer] Scene open cancelled."); return; }
        }

        // Find HUD_Canvas
        Canvas hudCanvas = FindInScene<Canvas>("HUD_Canvas");
        if (hudCanvas == null)
        {
            Debug.LogError("[UIFixer] HUD_Canvas not found in GameplayScene!");
            return;
        }

        // Find or create GameplayHUD component
        GameplayHUD hud = FindInScene<GameplayHUD>("GameplayHUD");

        // 1. Load sprites
        Sprite winSprite  = LoadSprite(WIN_PNG_PATH);
        Sprite failSprite = LoadSprite(FAIL_PNG_PATH);
        Sprite coinSprite = LoadSprite(COIN_ICON);
        Sprite gemSprite  = LoadSprite(GEM_ICON);

        // 2. Fix Win Panel
        FixWinPanel(hudCanvas.transform, winSprite, hud);

        // 3. Fix Fail Panel
        FixFailPanel(hudCanvas.transform, failSprite, hud);

        // 4. Fix / Add Currency HUD bar
        FixCurrencyHUD(hudCanvas.transform, coinSprite, gemSprite, hud);

        // 5. Mark dirty and save
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[UIFixer] ✅ Gameplay UI Panels fixed and scene saved!");
    }

    // ── Win Panel ─────────────────────────────────────────────────────────────
    static void FixWinPanel(Transform canvasRoot, Sprite winSprite, GameplayHUD hud)
    {
        // Find existing win panel (may be named LevelCompletePanel or WinPanel)
        Transform winTF = canvasRoot.Find("LevelCompletePanel")
                       ?? canvasRoot.Find("WinPanel")
                       ?? canvasRoot.Find("win_panel")
                       ?? canvasRoot.Find("Panel_Win");

        if (winTF == null)
        {
            // Create fresh
            var go = new GameObject("LevelCompletePanel");
            go.transform.SetParent(canvasRoot, false);
            winTF = go.transform;
        }
        else
        {
            winTF.name = "LevelCompletePanel";
        }
        winTF.SetAsLastSibling(); // render on top

        // Clear old children
        for (int i = winTF.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(winTF.GetChild(i).gameObject);

        // Root panel: full-screen dark overlay
        var rootRect = winTF.gameObject.GetOrAddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;

        var rootImg = winTF.gameObject.GetOrAddComponent<Image>();
        rootImg.color = new Color(0f, 0f, 0f, 0.65f);
        rootImg.raycastTarget = true;

        // Add canvas group so we can hide/show cleanly
        var cg = winTF.gameObject.GetOrAddComponent<CanvasGroup>();
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // Popup image (centered, fixed size)
        var popupGO = new GameObject("PopupImage");
        popupGO.transform.SetParent(winTF, false);
        var popupRect = popupGO.AddComponent<RectTransform>();
        SetAnchorsCenter(popupRect, new Vector2(900f, 1100f)); // ~w×h in px for 1080×1920
        var popupImg = popupGO.AddComponent<Image>();
        if (winSprite != null)
        {
            popupImg.sprite = winSprite;
            popupImg.preserveAspect = true;
        }
        popupImg.SetNativeSize();
        // Clamp size for portrait screen
        ScaleImageToFit(popupRect, 920f, 1100f);
        popupImg.raycastTarget = false;

        // ── Transparent Buttons ───────────────────────────────────────────────
        // Home button  — bottom-left area of popup image (approx 20% x, 12% y from bottom)
        var homeBtn = CreateTransparentButton("HomeButton", popupRect,
            anchorMin: new Vector2(0.08f, 0.05f),
            anchorMax: new Vector2(0.46f, 0.22f));

        // Next Level button — bottom-right area
        var nextBtn = CreateTransparentButton("NextLevelButton", popupRect,
            anchorMin: new Vector2(0.54f, 0.05f),
            anchorMax: new Vector2(0.92f, 0.22f));

        // Wire onClick
        if (hud != null)
        {
            homeBtn.onClick.RemoveAllListeners();
            homeBtn.onClick.AddListener(hud.OnMenuClicked);
            nextBtn.onClick.RemoveAllListeners();
            nextBtn.onClick.AddListener(hud.OnNextLevelClicked);

            // Wire hud fields via SerializedObject
            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("winPanel").objectReferenceValue = winTF.gameObject;
            so.ApplyModifiedProperties();
        }

        winTF.gameObject.SetActive(false); // hidden during gameplay
        Debug.Log("[UIFixer] Win panel rebuilt.");
    }

    // ── Fail Panel ────────────────────────────────────────────────────────────
    static void FixFailPanel(Transform canvasRoot, Sprite failSprite, GameplayHUD hud)
    {
        Transform failTF = canvasRoot.Find("LevelFailedPanel")
                        ?? canvasRoot.Find("FailPanel")
                        ?? canvasRoot.Find("fail_panel")
                        ?? canvasRoot.Find("Panel_Fail");

        if (failTF == null)
        {
            var go = new GameObject("LevelFailedPanel");
            go.transform.SetParent(canvasRoot, false);
            failTF = go.transform;
        }
        else
        {
            failTF.name = "LevelFailedPanel";
        }
        failTF.SetAsLastSibling();

        // Clear old children
        for (int i = failTF.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(failTF.GetChild(i).gameObject);

        // Dark overlay
        var rootRect = failTF.gameObject.GetOrAddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = rootRect.offsetMax = Vector2.zero;

        var rootImg = failTF.gameObject.GetOrAddComponent<Image>();
        rootImg.color = new Color(0f, 0f, 0f, 0.65f);
        rootImg.raycastTarget = true;

        var cg = failTF.gameObject.GetOrAddComponent<CanvasGroup>();
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // Popup image
        var popupGO = new GameObject("PopupImage");
        popupGO.transform.SetParent(failTF, false);
        var popupRect = popupGO.AddComponent<RectTransform>();
        SetAnchorsCenter(popupRect, new Vector2(900f, 1000f));
        var popupImg = popupGO.AddComponent<Image>();
        if (failSprite != null)
        {
            popupImg.sprite = failSprite;
            popupImg.preserveAspect = true;
        }
        popupImg.SetNativeSize();
        ScaleImageToFit(popupRect, 920f, 1000f);
        popupImg.raycastTarget = false;

        // Retry button — bottom-left
        var retryBtn = CreateTransparentButton("RetryButton", popupRect,
            anchorMin: new Vector2(0.08f, 0.05f),
            anchorMax: new Vector2(0.46f, 0.22f));

        // Home button — bottom-right
        var homeBtn = CreateTransparentButton("HomeButton", popupRect,
            anchorMin: new Vector2(0.54f, 0.05f),
            anchorMax: new Vector2(0.92f, 0.22f));

        if (hud != null)
        {
            retryBtn.onClick.RemoveAllListeners();
            retryBtn.onClick.AddListener(hud.OnRestartClicked);
            homeBtn.onClick.RemoveAllListeners();
            homeBtn.onClick.AddListener(hud.OnMenuClicked);

            SerializedObject so = new SerializedObject(hud);
            so.FindProperty("failPanel").objectReferenceValue = failTF.gameObject;
            so.ApplyModifiedProperties();
        }

        failTF.gameObject.SetActive(false);
        Debug.Log("[UIFixer] Fail panel rebuilt.");
    }

    // ── Currency HUD ──────────────────────────────────────────────────────────
    static void FixCurrencyHUD(Transform canvasRoot, Sprite coinSprite, Sprite gemSprite, GameplayHUD hud)
    {
        // Find or create CurrencyBar at top-left area
        Transform bar = canvasRoot.Find("CurrencyBar");
        if (bar == null)
        {
            var go = new GameObject("CurrencyBar");
            go.transform.SetParent(canvasRoot, false);
            bar = go.transform;
        }

        var barRect = bar.gameObject.GetOrAddComponent<RectTransform>();
        // Top-left corner, 420px wide, 80px tall
        barRect.anchorMin = new Vector2(0f, 1f);
        barRect.anchorMax = new Vector2(0f, 1f);
        barRect.pivot     = new Vector2(0f, 1f);
        barRect.anchoredPosition = new Vector2(10f, -10f);
        barRect.sizeDelta = new Vector2(420f, 80f);

        // HorizontalLayoutGroup
        var hlg = bar.gameObject.GetOrAddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth  = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        // --- Coin Panel ---
        Transform coinPanel = bar.Find("CoinPanel");
        if (coinPanel == null)
        {
            var go2 = new GameObject("CoinPanel");
            go2.transform.SetParent(bar, false);
            coinPanel = go2.transform;
        }
        BuildCurrencyPanel(coinPanel, coinSprite, "0", out TextMeshProUGUI coinTMP);

        // --- Gem Panel ---
        Transform gemPanel = bar.Find("GemPanel");
        if (gemPanel == null)
        {
            var go3 = new GameObject("GemPanel");
            go3.transform.SetParent(bar, false);
            gemPanel = go3.transform;
        }
        BuildCurrencyPanel(gemPanel, gemSprite, "0", out TextMeshProUGUI gemTMP);

        // Wire to GameplayHUD
        if (hud != null)
        {
            SerializedObject so = new SerializedObject(hud);
            var coinProp = so.FindProperty("coinText");
            var gemProp  = so.FindProperty("gemText");
            if (coinProp != null) coinProp.objectReferenceValue = coinTMP;
            if (gemProp  != null) gemProp.objectReferenceValue  = gemTMP;
            so.ApplyModifiedProperties();
        }

        Debug.Log("[UIFixer] Currency HUD fixed.");
    }

    static void BuildCurrencyPanel(Transform parent, Sprite icon,
        string defaultText, out TextMeshProUGUI tmp)
    {
        // Clear and rebuild
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);

        var panelRect = parent.gameObject.GetOrAddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(200f, 70f);

        // Background pill
        var panelImg = parent.gameObject.GetOrAddComponent<Image>();
        panelImg.color = new Color(0f, 0.1f, 0.3f, 0.75f);
        panelImg.raycastTarget = false;

        var hlg = parent.gameObject.GetOrAddComponent<HorizontalLayoutGroup>();
        hlg.padding = new RectOffset(8, 8, 5, 5);
        hlg.spacing = 6f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth  = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;

        // Icon
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(parent, false);
        var iconRect = iconGO.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(50f, 50f);
        var iconImg = iconGO.AddComponent<Image>();
        iconImg.sprite = icon;
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        // Text
        var txtGO = new GameObject("CountText");
        txtGO.transform.SetParent(parent, false);
        var txtRect = txtGO.AddComponent<RectTransform>();
        txtRect.sizeDelta = new Vector2(130f, 50f);
        tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = 36f;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Button CreateTransparentButton(string name, RectTransform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0f); // fully transparent, still raycast hits
        img.raycastTarget = true;

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;

        // No visual transition — it's a transparent overlay
        var colors = btn.colors;
        colors.normalColor      = new Color(1,1,1,0);
        colors.highlightedColor = new Color(1,1,1,0.1f);
        colors.pressedColor     = new Color(1,1,1,0.2f);
        btn.colors = colors;

        return btn;
    }

    static void SetAnchorsCenter(RectTransform rect, Vector2 size)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    static void ScaleImageToFit(RectTransform rect, float maxW, float maxH)
    {
        float w = rect.sizeDelta.x;
        float h = rect.sizeDelta.y;
        if (w <= 0 || h <= 0) { rect.sizeDelta = new Vector2(maxW, maxH); return; }
        float scaleW = maxW / w;
        float scaleH = maxH / h;
        float scale  = Mathf.Min(scaleW, scaleH, 1f);
        rect.sizeDelta = new Vector2(w * scale, h * scale);
    }

    static Sprite LoadSprite(string path)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            // Try setting texture import settings
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            if (sprite == null)
                Debug.LogWarning($"[UIFixer] Could not load sprite at: {path}");
        }
        return sprite;
    }

    static T FindInScene<T>(string name) where T : Object
    {
        var all = Object.FindObjectsOfType<T>();
        foreach (var obj in all)
        {
            if (obj is Component c && c.gameObject.name == name) return obj;
            if (obj is GameObject go && go.name == name) return obj;
        }
        // Fallback: return first found
        return all.Length > 0 ? all[0] : null;
    }
}

// ── Extension ─────────────────────────────────────────────────────────────────
public static class GameObjectExtensions
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        return go.GetComponent<T>() ?? go.AddComponent<T>();
    }
}
#endif
