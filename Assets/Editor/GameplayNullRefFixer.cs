#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Targeted fixer for the 3 specific null references found in GameplayScene:
/// 1. gemText — no GemText object exists → create one next to CoinText
/// 2. undoCountText — missing → create label inside UndoButton
/// 3. levelNameText — missing → create label in TopBar
/// Preserves all existing objects. Only adds what is missing.
/// </summary>
public static class GameplayNullRefFixer
{
    [MenuItem("OceanRescuer/Fix Null HUD References")]
    public static void FixNullRefs()
    {
        string scenePath = "Assets/Scenes/GameplayScene.unity";
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.name != "GameplayScene")
        {
            EditorSceneManager.OpenScene(scenePath);
            activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        var hud = Object.FindObjectOfType<GameplayHUD>();
        if (hud == null)
        {
            Debug.LogError("[NullRefFixer] GameplayHUD not found in scene!");
            return;
        }

        bool changed = false;
        changed |= FixGemText(hud);
        changed |= FixUndoCountText(hud);
        changed |= FixLevelNameText(hud);

        if (changed)
        {
            EditorUtility.SetDirty(hud);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("[NullRefFixer] ✅ Null references fixed and scene saved!");
        }
        else
        {
            Debug.Log("[NullRefFixer] ✅ All references already wired — nothing to fix.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 1. gemText — create GemPanel next to CoinPanel
    // ─────────────────────────────────────────────────────────────────────
    static bool FixGemText(GameplayHUD hud)
    {
        if (hud.gemText != null)
        {
            Debug.Log("[NullRefFixer] gemText already wired: " + hud.gemText.name);
            return false;
        }

        // Find CoinPanel to use as parent reference
        var coinPanel = GameObject.Find("CoinPanel");
        Transform parent = coinPanel != null ? coinPanel.transform.parent : hud.transform;

        // Check if GemPanel already exists
        var existingGem = GameObject.Find("GemPanel");
        if (existingGem != null)
        {
            var existingText = existingGem.GetComponentInChildren<TextMeshProUGUI>();
            if (existingText != null)
            {
                hud.gemText = existingText;
                Debug.Log("[NullRefFixer] gemText wired from existing GemPanel");
                return true;
            }
        }

        // Create GemPanel
        var gemPanel = new GameObject("GemPanel");
        gemPanel.transform.SetParent(parent, false);

        var rt = gemPanel.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 40f);

        // Position it below or beside the CoinPanel
        if (coinPanel != null)
        {
            var coinRT = coinPanel.GetComponent<RectTransform>();
            if (coinRT != null)
            {
                rt.anchoredPosition = coinRT.anchoredPosition + new Vector2(0f, -50f);
            }
        }

        // Add gem icon (optional — use icon_gem.png if available)
        var gemIconGO = new GameObject("GemIcon");
        gemIconGO.transform.SetParent(gemPanel.transform, false);
        var gemImg = gemIconGO.AddComponent<Image>();
        gemImg.rectTransform.sizeDelta = new Vector2(32f, 32f);
        gemImg.rectTransform.anchoredPosition = new Vector2(-40f, 0f);

        // Try load gem icon
        string[] guids = AssetDatabase.FindAssets("icon_gem t:Sprite");
        if (guids.Length > 0)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
            if (sprite != null) gemImg.sprite = sprite;
        }

        // Add GemText TMP
        var gemTextGO = new GameObject("GemText");
        gemTextGO.transform.SetParent(gemPanel.transform, false);
        var tmp = gemTextGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "0";
        tmp.fontSize = 24f;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(80f, 40f);
        tmp.rectTransform.anchoredPosition = new Vector2(10f, 0f);

        // Find and assign font from existing TMP objects
        var existingTMP = Object.FindObjectOfType<TextMeshProUGUI>();
        if (existingTMP != null && existingTMP.font != null)
            tmp.font = existingTMP.font;

        hud.gemText = tmp;
        Debug.Log("[NullRefFixer] ✅ GemPanel + GemText created and gemText wired");
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 2. undoCountText — add label to UndoButton
    // ─────────────────────────────────────────────────────────────────────
    static bool FixUndoCountText(GameplayHUD hud)
    {
        if (hud.undoCountText != null)
        {
            Debug.Log("[NullRefFixer] undoCountText already wired");
            return false;
        }

        // Check if UndoButton has a child TMP
        var undoBtn = GameObject.Find("UndoButton");
        if (undoBtn == null)
        {
            Debug.LogWarning("[NullRefFixer] UndoButton not found — skipping undoCountText");
            return false;
        }

        var existingTMP = undoBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (existingTMP != null)
        {
            hud.undoCountText = existingTMP;
            Debug.Log("[NullRefFixer] undoCountText wired from existing child TMP");
            return true;
        }

        // Create label
        var labelGO = new GameObject("UndoCountText");
        labelGO.transform.SetParent(undoBtn.transform, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Undo (3)";
        tmp.fontSize = 18f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(100f, 30f);
        tmp.rectTransform.anchoredPosition = Vector2.zero;

        var existingFont = Object.FindObjectOfType<TextMeshProUGUI>();
        if (existingFont != null && existingFont.font != null)
            tmp.font = existingFont.font;

        hud.undoCountText = tmp;
        Debug.Log("[NullRefFixer] ✅ UndoCountText created and wired");
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 3. levelNameText — add to TopBar
    // ─────────────────────────────────────────────────────────────────────
    static bool FixLevelNameText(GameplayHUD hud)
    {
        if (hud.levelNameText != null)
        {
            Debug.Log("[NullRefFixer] levelNameText already wired");
            return false;
        }

        // Find TopBar
        var topBar = GameObject.Find("TopBar");
        Transform parent = topBar != null ? topBar.transform : hud.transform;

        // Check existing
        var existing = GameObject.Find("LevelNameText");
        if (existing != null)
        {
            var tmp2 = existing.GetComponent<TextMeshProUGUI>();
            if (tmp2 != null) { hud.levelNameText = tmp2; return true; }
        }

        // Create
        var labelGO = new GameObject("LevelNameText");
        labelGO.transform.SetParent(parent, false);
        var tmp = labelGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "World 1 · Level 1";
        tmp.fontSize = 20f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.rectTransform.sizeDelta = new Vector2(300f, 40f);
        tmp.rectTransform.anchoredPosition = new Vector2(0f, -20f);

        var existingFont = Object.FindObjectOfType<TextMeshProUGUI>();
        if (existingFont != null && existingFont.font != null)
            tmp.font = existingFont.font;

        hud.levelNameText = tmp;
        Debug.Log("[NullRefFixer] ✅ LevelNameText created and wired");
        return true;
    }
}
#endif
