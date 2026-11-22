#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// SettingsToggleFixer — ek click mein SettingsPanel completely fix karta hai.
/// Menu: OceanRescuer > Fix Settings Toggle
///
/// Kya karta hai:
///   1. MainMenuScene open karta hai
///   2. SettingsPanel dhundhta/banata hai
///   3. SoundButton + MusicButton GameObjects banata/dhundhta hai
///   4. SoundStateIcon + MusicStateIcon child Images banata hai
///   5. SettingsToggle script attach karta hai
///   6. button_sound.png aur button_mute.png sprites assign karta hai
///   7. Scene save karta hai
/// </summary>
public static class SettingsToggleFixer
{
    private const string SCENE_PATH    = "Assets/Scenes/MainMenuScene.unity";
    private const string SOUND_ON_PATH = "Assets/Art/UI/Buttons/button_sound.png";
    private const string SOUND_OFF_PATH= "Assets/Art/UI/Buttons/button_mute.png";

    [MenuItem("OceanRescuer/Fix Settings Toggle")]
    public static void Run()
    {
        // ── 1. Open MainMenuScene ─────────────────────────────────────────────
        if (SceneManager.GetActiveScene().name != "MainMenuScene")
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
        }
        var scene = SceneManager.GetActiveScene();

        // ── 2. Load sprites ───────────────────────────────────────────────────
        Sprite soundOn  = AssetDatabase.LoadAssetAtPath<Sprite>(SOUND_ON_PATH);
        Sprite soundOff = AssetDatabase.LoadAssetAtPath<Sprite>(SOUND_OFF_PATH);

        if (soundOn  == null) Debug.LogWarning("[SettingsToggleFixer] button_sound.png not found at " + SOUND_ON_PATH);
        if (soundOff == null) Debug.LogWarning("[SettingsToggleFixer] button_mute.png not found at "  + SOUND_OFF_PATH);

        // ── 3. Find SettingsPanel ─────────────────────────────────────────────
        GameObject settingsPanel = GameObject.Find("SettingsPanel");
        if (settingsPanel == null)
        {
            Debug.LogError("[SettingsToggleFixer] SettingsPanel not found! Make sure MainMenuScene is open.");
            return;
        }
        Debug.Log("[SettingsToggleFixer] Found SettingsPanel: " + settingsPanel.name);

        // Fix scale if squished
        var spRT = settingsPanel.GetComponent<RectTransform>();
        if (spRT != null && Mathf.Abs(spRT.localScale.x - 1f) > 0.05f)
        {
            spRT.localScale = Vector3.one;
            Debug.Log("[SettingsToggleFixer] SettingsPanel scale fixed to (1,1,1)");
        }

        // ── 4. Create / Find SoundButton ─────────────────────────────────────
        Button soundBtn = GetOrCreateButton(settingsPanel, "SoundButton",
            new Vector2(0.05f, 0.4f), new Vector2(0.47f, 0.7f),
            new Color(0.15f, 0.7f, 0.3f, 0.6f));  // green tint

        // ── 5. Create / Find MusicButton ─────────────────────────────────────
        Button musicBtn = GetOrCreateButton(settingsPanel, "MusicButton",
            new Vector2(0.53f, 0.4f), new Vector2(0.95f, 0.7f),
            new Color(0.15f, 0.7f, 0.3f, 0.6f));

        // ── 6. Create icon Images inside buttons ──────────────────────────────
        Image soundIcon = GetOrCreateIcon(soundBtn?.gameObject, "SoundStateIcon", soundOn);
        Image musicIcon = GetOrCreateIcon(musicBtn?.gameObject, "MusicStateIcon", soundOn);

        // ── 7. Find CloseSettBtn ──────────────────────────────────────────────
        Button closeBtn = null;
        var closeT = settingsPanel.transform.Find("CloseSettBtn");
        if (closeT != null) closeBtn = closeT.GetComponent<Button>();

        // ── 8. Remove old SettingsManager if present (avoid conflict) ─────────
        var oldSM = settingsPanel.GetComponent<SettingsManager>();
        // Keep SettingsManager if it exists (don't remove — just also add SettingsToggle)

        // ── 9. Add SettingsToggle to SettingsPanel ────────────────────────────
        var toggle = settingsPanel.GetComponent<SettingsToggle>();
        if (toggle == null) toggle = settingsPanel.AddComponent<SettingsToggle>();

        // Wire all references
        toggle.soundButton   = soundBtn;
        toggle.musicButton   = musicBtn;
        toggle.closeButton   = closeBtn;
        toggle.soundIconImage= soundIcon;
        toggle.musicIconImage= musicIcon;
        toggle.soundOnSprite = soundOn;
        toggle.soundOffSprite= soundOff;
        toggle.settingsPanel = settingsPanel;

        // ── 10. Wire label text on buttons ────────────────────────────────────
        SetButtonLabel(soundBtn?.gameObject, "Sound");
        SetButtonLabel(musicBtn?.gameObject, "Music");

        // ── 11. Save ──────────────────────────────────────────────────────────
        EditorUtility.SetDirty(settingsPanel);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("[SettingsToggleFixer] ✅ Done! SoundButton + MusicButton + SettingsToggle wired and saved.");
        Debug.Log("  SoundButton: " + (soundBtn != null ? "OK" : "FAIL"));
        Debug.Log("  MusicButton: " + (musicBtn != null ? "OK" : "FAIL"));
        Debug.Log("  SoundIcon:   " + (soundIcon != null ? "OK" : "FAIL"));
        Debug.Log("  MusicIcon:   " + (musicIcon != null ? "OK" : "FAIL"));
        Debug.Log("  CloseBtn:    " + (closeBtn != null ? "OK" : "not found"));
        Debug.Log("  SoundOnSpr:  " + (soundOn  != null ? "OK" : "MISSING"));
        Debug.Log("  SoundOffSpr: " + (soundOff != null ? "OK" : "MISSING"));
    }

    // ── Helper: Create or find a Button child ─────────────────────────────────
    static Button GetOrCreateButton(GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Color tint)
    {
        // Try find existing
        var existing = parent.transform.Find(name);
        if (existing != null)
        {
            var btn = existing.GetComponent<Button>();
            if (btn == null) btn = existing.gameObject.AddComponent<Button>();
            Debug.Log($"[SettingsToggleFixer] Found existing {name}");
            return btn;
        }

        // Create new
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.layer = 5; // UI layer

        // RectTransform
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;
        rt.pivot            = new Vector2(0.5f, 0.5f);

        // Image (semi-transparent — artwork ke upar overlay)
        var img = go.AddComponent<Image>();
        img.color         = tint;
        img.raycastTarget = true;

        // Button
        var btn2 = go.AddComponent<Button>();
        btn2.targetGraphic = img;

        // Label text neeche
        SetButtonLabel(go, name.Replace("Button", ""));

        Debug.Log($"[SettingsToggleFixer] Created {name}");
        return btn2;
    }

    // ── Helper: Create or find icon Image child ───────────────────────────────
    static Image GetOrCreateIcon(GameObject btnGO, string name, Sprite sprite)
    {
        if (btnGO == null) return null;

        var existing = btnGO.transform.Find(name);
        Image img;

        if (existing != null)
        {
            img = existing.GetComponent<Image>();
            if (img == null) img = existing.gameObject.AddComponent<Image>();
        }
        else
        {
            var go = new GameObject(name);
            go.transform.SetParent(btnGO.transform, false);
            go.layer = 5;

            var rt = go.AddComponent<RectTransform>();
            // Right side of button, square, centered vertically
            rt.anchorMin        = new Vector2(0.6f, 0.1f);
            rt.anchorMax        = new Vector2(0.98f, 0.9f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = Vector2.zero;
            rt.pivot            = new Vector2(0.5f, 0.5f);

            go.AddComponent<CanvasRenderer>();
            img = go.AddComponent<Image>();
        }

        // Assign sprite
        if (sprite != null) img.sprite = sprite;
        img.preserveAspect = true;   // distort nahi hoga
        img.raycastTarget  = false;  // clicks pass through karo
        img.color          = Color.white;

        Debug.Log($"[SettingsToggleFixer] Icon {name} ready. Sprite: {(sprite != null ? sprite.name : "none")}");
        return img;
    }

    // ── Helper: Add/update label TMP text on button ───────────────────────────
    static void SetButtonLabel(GameObject btnGO, string labelText)
    {
        if (btnGO == null) return;

        // Try find existing TMP label
        var existingTMP = btnGO.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
        if (existingTMP != null)
        {
            existingTMP.text = labelText;
            existingTMP.raycastTarget = false;
            return;
        }

        // Create label GO
        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(btnGO.transform, false);
        labelGO.layer = 5;

        var rt = labelGO.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.02f, 0.05f);
        rt.anchorMax        = new Vector2(0.58f, 0.95f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;

        var tmp = labelGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text           = labelText;
        tmp.fontSize       = 30;
        tmp.fontStyle      = TMPro.FontStyles.Bold;
        tmp.alignment      = TMPro.TextAlignmentOptions.MidlineLeft;
        tmp.color          = Color.white;
        tmp.raycastTarget  = false;
    }
}
#endif
