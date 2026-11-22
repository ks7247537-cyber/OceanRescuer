#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Builds and wires the SettingsPanel in MainMenuScene.
/// Creates soundButton and musicButton with ON/OFF indicators,
/// attaches SettingsManager and hooks all onClick events.
///
/// Menu: OceanRescuer > Fix Settings Panel
/// </summary>
public static class SettingsPanelFixer
{
    private const string SCENE_PATH = "Assets/Scenes/MainMenuScene.unity";

    // Sprite GUIDs — user-provided icons (icon_sound_on.png / icon_sound_off.png)
    // These are the blue speaker icons provided by the user for the toggle buttons.
    private const string SOUND_ON_GUID   = "e3429685d5c2d554bb921b39feb76c53"; // icon_sound_on.png  (blue speaker ON)
    private const string SOUND_OFF_GUID  = "8eca980a4f53b73409b10f486d8081c1"; // icon_sound_off.png (blue muted speaker OFF)

    // Fallback: original button art (used if new icons aren't importable yet)
    private const string FALLBACK_ON_GUID  = "95ddebbd5b023374d9c6d74b4c2401b1"; // button_sound.png
    private const string FALLBACK_OFF_GUID = "74dabcd7704d68b49aae099595e163fa"; // button_mute.png

    [MenuItem("OceanRescuer/Fix Settings Panel")]
    public static void Run()
    {
        // ── 0. Force reimport new icon sprites so Unity picks them up ─────────
        AssetDatabase.ImportAsset(
            "Assets/Art/UI/Buttons/icon_sound_on.png",
            ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(
            "Assets/Art/UI/Buttons/icon_sound_off.png",
            ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        // ── 1. Open MainMenuScene ─────────────────────────────────────────────
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.name != "MainMenuScene")
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            activeScene = SceneManager.GetActiveScene();
        }

        // ── 2. Load sprites (new user icons first, fallback to original art) ──
        string soundOnPath  = AssetDatabase.GUIDToAssetPath(SOUND_ON_GUID);
        string soundOffPath = AssetDatabase.GUIDToAssetPath(SOUND_OFF_GUID);

        Sprite soundOnSprite  = LoadFirstSprite(soundOnPath,  "icon_sound_on");
        Sprite soundOffSprite = LoadFirstSprite(soundOffPath, "icon_sound_off");

        // Fallback to original button_sound / button_mute if new icons not yet imported
        if (soundOnSprite == null)
        {
            string fbPath = AssetDatabase.GUIDToAssetPath(FALLBACK_ON_GUID);
            soundOnSprite = LoadFirstSprite(fbPath, "button_sound");
            if (soundOnSprite != null)
                Debug.LogWarning("[SettingsPanelFixer] icon_sound_on.png not ready — using button_sound.png fallback.");
            else
                Debug.LogWarning("[SettingsPanelFixer] ⚠ No sound-ON sprite found.");
        }
        else
        {
            Debug.Log("[SettingsPanelFixer] ✓ icon_sound_on.png loaded.");
        }

        if (soundOffSprite == null)
        {
            string fbPath = AssetDatabase.GUIDToAssetPath(FALLBACK_OFF_GUID);
            soundOffSprite = LoadFirstSprite(fbPath, "button_mute");
            if (soundOffSprite != null)
                Debug.LogWarning("[SettingsPanelFixer] icon_sound_off.png not ready — using button_mute.png fallback.");
            else
                Debug.LogWarning("[SettingsPanelFixer] ⚠ No sound-OFF sprite found.");
        }
        else
        {
            Debug.Log("[SettingsPanelFixer] ✓ icon_sound_off.png loaded.");
        }

        // ── 3. Find SettingsPanel ────────────────────────────────────────────
        GameObject settingsPanel = GameObject.Find("SettingsPanel");
        if (settingsPanel == null)
        {
            Debug.LogError("[SettingsPanelFixer] SettingsPanel not found in scene!");
            return;
        }

        Debug.Log("[SettingsPanelFixer] SettingsPanel found: " + settingsPanel.name);

        // Fix scale — should be (1,1,1), was (0.4,1,1)
        RectTransform spRT = settingsPanel.GetComponent<RectTransform>();
        if (spRT != null && Mathf.Abs(spRT.localScale.x - 1f) > 0.01f)
        {
            spRT.localScale = Vector3.one;
            Debug.Log("[SettingsPanelFixer] SettingsPanel scale fixed to (1,1,1).");
        }

        // Fix anchor to cover center of screen properly (landscape)
        if (spRT != null)
        {
            spRT.anchorMin        = new Vector2(0.3f, 0.15f);
            spRT.anchorMax        = new Vector2(0.7f, 0.85f);
            spRT.anchoredPosition = Vector2.zero;
            spRT.sizeDelta        = Vector2.zero;
        }

        // ── 4. Add / find SettingsTitle TMP ────────────────────────────────
        GameObject titleGO = settingsPanel.transform.Find("SettingsTitle")?.gameObject;
        if (titleGO == null)
        {
            titleGO = new GameObject("SettingsTitle");
            titleGO.transform.SetParent(settingsPanel.transform, false);
        }
        var titleRT = titleGO.GetOrAddComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0.05f, 0.78f);
        titleRT.anchorMax        = new Vector2(0.95f, 0.98f);
        titleRT.anchoredPosition = Vector2.zero;
        titleRT.sizeDelta        = Vector2.zero;
        var titleTMP = titleGO.GetOrAddComponent<TextMeshProUGUI>();
        titleTMP.text      = "SETTINGS";
        titleTMP.fontSize  = 52;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color     = Color.white;
        titleTMP.raycastTarget = false;

        // ── 5. Create / fix SoundButton ─────────────────────────────────────
        Button soundBtn = CreateOrFixToggleButton(
            settingsPanel, "soundButton",
            anchorMin: new Vector2(0.08f, 0.42f),
            anchorMax: new Vector2(0.48f, 0.75f),
            labelText: "Sound",
            onSprite: soundOnSprite,
            offSprite: soundOffSprite
        );

        // ── 6. Create / fix MusicButton ─────────────────────────────────────
        Button musicBtn = CreateOrFixToggleButton(
            settingsPanel, "musicButton",
            anchorMin: new Vector2(0.52f, 0.42f),
            anchorMax: new Vector2(0.92f, 0.75f),
            labelText: "Music",
            onSprite: soundOnSprite,
            offSprite: soundOffSprite
        );

        // ── 7. Fix / find CloseSettBtn ──────────────────────────────────────
        GameObject closeGO = settingsPanel.transform.Find("CloseSettBtn")?.gameObject;
        Button closeBtn = null;
        if (closeGO != null)
        {
            closeBtn = closeGO.GetOrAddComponent<Button>();
            var closeRT = closeGO.GetOrAddComponent<RectTransform>();
            closeRT.anchorMin        = new Vector2(0.35f, 0.05f);
            closeRT.anchorMax        = new Vector2(0.65f, 0.3f);
            closeRT.anchoredPosition = Vector2.zero;
            closeRT.sizeDelta        = Vector2.zero;
            // Ensure label text
            var closeLbl = closeGO.GetComponentInChildren<TextMeshProUGUI>();
            if (closeLbl != null) { closeLbl.text = "Close"; closeLbl.raycastTarget = false; }
            Debug.Log("[SettingsPanelFixer] CloseSettBtn found and positioned.");
        }
        else
        {
            Debug.LogWarning("[SettingsPanelFixer] CloseSettBtn not found — close button missing.");
        }

        // ── 8. Add SettingsManager to SettingsPanel ──────────────────────────
        var sm = settingsPanel.GetOrAddComponent<SettingsManager>();
        var soSM = new SerializedObject(sm);

        // Wire button references
        AssignObjectRef(soSM, "musicButton", musicBtn);
        AssignObjectRef(soSM, "soundButton", soundBtn);
        AssignObjectRef(soSM, "closeButton",  closeBtn);

        // Wire sprite references
        AssignObjectRef(soSM, "musicOnSprite",  soundOnSprite);
        AssignObjectRef(soSM, "musicOffSprite", soundOffSprite);
        AssignObjectRef(soSM, "soundOnSprite",  soundOnSprite);
        AssignObjectRef(soSM, "soundOffSprite", soundOffSprite);

        // Wire optional labels
        var soundLabel = soundBtn?.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        var musicLabel = musicBtn?.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
        AssignObjectRef(soSM, "soundLabel", soundLabel);
        AssignObjectRef(soSM, "musicLabel", musicLabel);

        soSM.ApplyModifiedProperties();
        Debug.Log("[SettingsPanelFixer] SettingsManager wired.");

        // ── 9. Wire Button onClick events via SerializedObject ───────────────
        //  soundButton → SettingsManager.OnSoundToggle
        WireButtonClick(soundBtn, sm, "OnSoundToggle");
        //  musicButton → SettingsManager.OnMusicToggle
        WireButtonClick(musicBtn, sm, "OnMusicToggle");
        //  closeButton → SettingsManager.CloseSettings (or MainMenuManager)
        if (closeBtn != null)
        {
            // Wire to SettingsManager.CloseSettings
            WireButtonClick(closeBtn, sm, "CloseSettings");
            // Also wire to MainMenuManager.OnCloseSettingsClicked for backward compat
            var mmm = Object.FindObjectOfType<MainMenuManager>();
            if (mmm != null) WireButtonClick(closeBtn, mmm, "OnCloseSettingsClicked");
        }

        // ── 10. Disable RaycastTarget on all child Images/Texts ─────────────
        // that are NOT buttons themselves (so clicks pass through to Button)
        DisableChildRaycasts(settingsPanel, soundBtn?.gameObject, musicBtn?.gameObject, closeGO);

        // ── 11. Save scene ───────────────────────────────────────────────────
        EditorSceneManager.MarkSceneDirty(activeScene);
        EditorSceneManager.SaveScene(activeScene);
        AssetDatabase.SaveAssets();

        Debug.Log("[SettingsPanelFixer] ✅ SettingsPanel fixed and saved!");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Button CreateOrFixToggleButton(
        GameObject parent, string name,
        Vector2 anchorMin, Vector2 anchorMax,
        string labelText, Sprite onSprite, Sprite offSprite = null)
    {
        // Find existing or create new
        GameObject go = parent.transform.Find(name)?.gameObject;
        if (go == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            Debug.Log($"[SettingsPanelFixer] Created {name}");
        }

        // RectTransform
        var rt = go.GetOrAddComponent<RectTransform>();
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta        = Vector2.zero;

        // Image (button background — solid color, no sprite, so icon shows clearly)
        var img = go.GetOrAddComponent<Image>();
        img.sprite        = null;
        img.color         = new Color(0.15f, 0.15f, 0.35f, 0.92f); // dark blue panel
        img.raycastTarget = true;   // Button needs this

        // Button component
        var btn = go.GetOrAddComponent<Button>();
        btn.targetGraphic = img;
        var colors = btn.colors;
        colors.normalColor      = Color.white;
        colors.highlightedColor = new Color(0.85f, 0.85f, 0.85f);
        colors.pressedColor     = new Color(0.65f, 0.65f, 0.65f);
        btn.colors = colors;
        btn.interactable = true;

        // Clear existing onClick so we don't duplicate
        btn.onClick.RemoveAllListeners();

        // ── Icon Image (shows ON sprite, swapped by SettingsManager.RefreshUI) ─
        GameObject iconGO = go.transform.Find("Icon")?.gameObject;
        if (iconGO == null)
        {
            iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(go.transform, false);
        }
        var iconRT = iconGO.GetOrAddComponent<RectTransform>();
        iconRT.anchorMin        = new Vector2(0.15f, 0.25f);
        iconRT.anchorMax        = new Vector2(0.85f, 0.90f);
        iconRT.anchoredPosition = Vector2.zero;
        iconRT.sizeDelta        = Vector2.zero;
        var iconImg = iconGO.GetOrAddComponent<Image>();
        if (onSprite != null) iconImg.sprite = onSprite;
        iconImg.color         = Color.white;
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;  // ← must NOT block button clicks

        // ── State label (Sound: ON / Music: ON) ───────────────────────────────
        GameObject labelGO = go.transform.Find("Label")?.gameObject;
        if (labelGO == null)
        {
            labelGO = new GameObject("Label");
            labelGO.transform.SetParent(go.transform, false);
        }
        var lblRT = labelGO.GetOrAddComponent<RectTransform>();
        lblRT.anchorMin        = new Vector2(0f, 0.02f);
        lblRT.anchorMax        = new Vector2(1f, 0.28f);
        lblRT.anchoredPosition = Vector2.zero;
        lblRT.sizeDelta        = Vector2.zero;
        var lblTMP = labelGO.GetOrAddComponent<TextMeshProUGUI>();
        lblTMP.text            = labelText + ": <color=#33FF66>ON</color>";
        lblTMP.fontSize        = 28;
        lblTMP.alignment       = TextAlignmentOptions.Center;
        lblTMP.color           = Color.white;
        lblTMP.fontStyle       = FontStyles.Bold;
        lblTMP.raycastTarget   = false;   // ← IMPORTANT: don't block clicks

        return btn;
    }

    static Sprite LoadFirstSprite(string path, string fallbackName)
    {
        if (string.IsNullOrEmpty(path)) return null;

        // Try direct load (Single mode)
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s != null) return s;

        // Try loading all sub-assets (Multiple mode)
        Object[] all = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var obj in all)
            if (obj is Sprite sp) return sp;

        return null;
    }

    static void AssignObjectRef(SerializedObject so, string propName, Object value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
        else
        {
            Debug.LogWarning($"[SettingsPanelFixer] Property '{propName}' not found on {so.targetObject}");
        }
    }

    static void WireButtonClick(Button btn, Object target, string methodName)
    {
        if (btn == null || target == null) return;

        var soPersistent = new SerializedObject(btn);
        var onClickProp  = soPersistent.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (onClickProp == null) return;

        // Check if already wired to avoid duplicates
        for (int i = 0; i < onClickProp.arraySize; i++)
        {
            var call       = onClickProp.GetArrayElementAtIndex(i);
            var targetProp = call.FindPropertyRelative("m_Target");
            var methodProp = call.FindPropertyRelative("m_MethodName");
            if (targetProp.objectReferenceValue == target &&
                methodProp.stringValue == methodName)
            {
                Debug.Log($"[SettingsPanelFixer] {btn.name} → {methodName} already wired.");
                soPersistent.Dispose();
                return;
            }
        }

        // Add new listener entry
        onClickProp.InsertArrayElementAtIndex(onClickProp.arraySize);
        var newCall = onClickProp.GetArrayElementAtIndex(onClickProp.arraySize - 1);
        newCall.FindPropertyRelative("m_Target").objectReferenceValue        = target;
        newCall.FindPropertyRelative("m_MethodName").stringValue             = methodName;
        newCall.FindPropertyRelative("m_Mode").intValue                      = 1; // void
        newCall.FindPropertyRelative("m_CallState").intValue                 = 2; // RuntimeOnly
        newCall.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = target.GetType().AssemblyQualifiedName;

        soPersistent.ApplyModifiedProperties();
        Debug.Log($"[SettingsPanelFixer] Wired {btn.name}.onClick → {target.GetType().Name}.{methodName}");
    }

    static void DisableChildRaycasts(GameObject panel, params GameObject[] buttons)
    {
        // Turn off RaycastTarget on all Images and TMP texts that are not Button targets
        foreach (var img in panel.GetComponentsInChildren<Image>(true))
        {
            bool isButtonTarget = false;
            foreach (var b in buttons)
            {
                if (b == null) continue;
                if (img.gameObject == b) { isButtonTarget = true; break; }
            }
            if (!isButtonTarget)
                img.raycastTarget = false;
        }
    }
}

#endif
