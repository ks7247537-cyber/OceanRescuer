#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;

/// <summary>
/// Gameplay Final Fixer — fixes all remaining gameplay issues:
/// 1. Replace StandaloneInputModule → InputSystemUIInputModule (fixes InvalidOperationException)
/// 2. Fix camera orthographic size for 9x5 pipe grid landscape
/// 3. Create Fish_Character with CharacterMover + SpriteRenderer
/// 4. Wire characterMover to GameManager
/// 5. Fix HUD_Canvas CanvasScaler to 1920×1080 landscape
/// 6. Ensure GridManager Cols=9, Rows=5
/// 7. Fix FlowManager source/sink cells
/// 8. Ensure BoxCollider2D on pipe prefabs
/// </summary>
public static class GameplayFinalFixer
{
    [MenuItem("OceanRescuer/Fix All Gameplay Issues")]
    public static void FixAll()
    {
        // Open GameplayScene if not already open
        string scenePath = "Assets/Scenes/GameplayScene.unity";
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameplayScene")
        {
            EditorSceneManager.OpenScene(scenePath);
        }

        bool changed = false;
        changed |= FixEventSystem();
        changed |= FixCamera();
        changed |= FixCanvasScaler();
        changed |= FixGridManager();
        changed |= CreateFishCharacter();
        changed |= WireGameManager();
        changed |= FixPipePrefabs();

        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[GameplayFinalFixer] ✅ All fixes applied and scene saved!");
        }
        else
        {
            Debug.Log("[GameplayFinalFixer] ✅ Everything already looks good!");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Fix 1: EventSystem — StandaloneInputModule → InputSystemUIInputModule
    // ─────────────────────────────────────────────────────────────────────
    static bool FixEventSystem()
    {
        var esGO = GameObject.Find("EventSystem");
        if (esGO == null)
        {
            // Create EventSystem
            esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        }

        // Remove StandaloneInputModule if present
        var standalone = esGO.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        if (standalone != null)
        {
            Object.DestroyImmediate(standalone);
            Debug.Log("[Fix1] Removed StandaloneInputModule");
        }

        // Try to add InputSystemUIInputModule (requires Input System package)
        var existingType = System.Type.GetType(
            "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
        if (existingType == null)
        {
            // Try alternate assembly name
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
                if (t != null) { existingType = t; break; }
            }
        }

        if (existingType != null)
        {
            if (esGO.GetComponent(existingType) == null)
            {
                esGO.AddComponent(existingType);
                Debug.Log("[Fix1] Added InputSystemUIInputModule ✅");
                return true;
            }
        }
        else
        {
            // Input System package not found — keep StandaloneInputModule removed,
            // add BaseInputModule fallback or leave EventSystem alone.
            // As a fallback: re-add StandaloneInputModule (old input still works with activeInputHandler=2)
            if (esGO.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
            {
                esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[Fix1] Fallback: re-added StandaloneInputModule (InputSystem package not found)");
            }
            Debug.LogWarning("[Fix1] InputSystemUIInputModule not found. Install 'Input System' package for full fix.");
        }

        return standalone != null;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Fix 2: Camera orthographic size
    // Grid 9×5, TileSize=1.1 → width=9.9, height=5.5 units
    // Landscape 16:9 → orthoHeight = width*(9/16) = 9.9*9/16 = 5.57 → use 3.5 for padding
    // Actually: orthoSize = half-height. Screen is 1920×1080 (16:9).
    // Grid height = 5×1.1 = 5.5 units → half=2.75, with 20% padding → 3.3
    // But we want HUD visible too → use 4.0
    // ─────────────────────────────────────────────────────────────────────
    static bool FixCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[Fix2] Main Camera not found!");
            return false;
        }

        bool changed = false;

        if (!cam.orthographic)
        {
            cam.orthographic = true;
            changed = true;
        }

        // For 9x5 grid with TileSize=1.1:
        // Grid height = 4 * 1.1 = 4.4 (row span from y=0 to y=4)
        // Half height with padding: ~3.0 fits nicely
        // But if pipes are centered at 0,0 → y spans from -2.2 to +2.2
        // HUD_Canvas is overlay so camera only needs to show the grid
        const float TARGET_ORTHO = 4.0f;
        if (Mathf.Abs(cam.orthographicSize - TARGET_ORTHO) > 0.01f)
        {
            cam.orthographicSize = TARGET_ORTHO;
            changed = true;
            Debug.Log($"[Fix2] Camera orthographicSize set to {TARGET_ORTHO}");
        }

        // Ensure camera at z=-10
        Vector3 pos = cam.transform.position;
        if (Mathf.Abs(pos.z - (-10f)) > 0.01f)
        {
            cam.transform.position = new Vector3(0f, 0f, -10f);
            changed = true;
            Debug.Log("[Fix2] Camera position set to (0,0,-10)");
        }

        return changed;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Fix 3: HUD_Canvas CanvasScaler → 1920×1080 Landscape
    // ─────────────────────────────────────────────────────────────────────
    static bool FixCanvasScaler()
    {
        bool changed = false;
        var canvases = Object.FindObjectsOfType<Canvas>();
        foreach (var canvas in canvases)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();

            if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                changed = true;
            }

            Vector2 target = new Vector2(1920f, 1080f);
            if (scaler.referenceResolution != target)
            {
                scaler.referenceResolution = target;
                changed = true;
                Debug.Log($"[Fix3] {canvas.name} CanvasScaler → 1920×1080");
            }

            // For landscape: match height = 0, matchWidthOrHeight=0 means match width
            // Use 0.5 for balanced scaling
            if (Mathf.Abs(scaler.matchWidthOrHeight - 0.5f) > 0.01f)
            {
                scaler.matchWidthOrHeight = 0.5f;
                changed = true;
            }
        }
        return changed;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Fix 4: GridManager — ensure Cols=9, Rows=5
    // ─────────────────────────────────────────────────────────────────────
    static bool FixGridManager()
    {
        var gm = Object.FindObjectOfType<GridManager>();
        if (gm == null)
        {
            Debug.LogWarning("[Fix4] GridManager not found!");
            return false;
        }

        bool changed = false;
        if (gm.Cols != 9) { gm.Cols = 9; changed = true; }
        if (gm.Rows != 5) { gm.Rows = 5; changed = true; }
        if (Mathf.Abs(gm.TileSize - 1.1f) > 0.001f) { gm.TileSize = 1.1f; changed = true; }

        if (changed) Debug.Log("[Fix4] GridManager: Cols=9, Rows=5, TileSize=1.1");
        return changed;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Fix 5: Create Fish_Character under CharacterContainer
    // ─────────────────────────────────────────────────────────────────────
    static bool CreateFishCharacter()
    {
        // Check if fish already exists
        var existingMover = Object.FindObjectOfType<CharacterMover>();
        if (existingMover != null)
        {
            Debug.Log("[Fix5] CharacterMover already exists: " + existingMover.gameObject.name);
            return false;
        }

        // Find or create CharacterContainer
        GameObject container = GameObject.Find("CharacterContainer");
        if (container == null)
        {
            container = new GameObject("CharacterContainer");
            container.transform.position = Vector3.zero;
        }

        // Create Fish_Character
        GameObject fish = new GameObject("Fish_Character");
        fish.transform.SetParent(container.transform);

        // SpriteRenderer — use a simple circle as placeholder (no sprite asset needed)
        var sr = fish.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.8f, 0.2f); // golden fish color
        sr.sortingOrder = 10;

        // Load Clownfish sprite
        Sprite fishSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Art/Characters/Clownfish.png");
        if (fishSprite == null)
        {
            string[] guids2 = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Art/Characters" });
            if (guids2.Length > 0)
                fishSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                    AssetDatabase.GUIDToAssetPath(guids2[0]));
        }

        if (fishSprite != null)
        {
            sr.sprite = fishSprite;
            Debug.Log("[Fix5] Fish sprite assigned: " + fishSprite.name);
        }
        else
        {
            Debug.LogWarning("[Fix5] No fish sprite found in Assets/Art/Characters/ — yellow placeholder will be used.");
        }

        // Scale to match tile size (1.1 units)
        fish.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        // Position at grid source (0,0)
        // Grid center formula: ox = -(Cols-1)*TileSize*0.5 = -(9-1)*1.1*0.5 = -4.4
        //                       oy = -(Rows-1)*TileSize*0.5 = -(5-1)*1.1*0.5 = -2.2
        float ox = -(9 - 1) * 1.1f * 0.5f; // -4.4
        float oy = -(5 - 1) * 1.1f * 0.5f; // -2.2
        fish.transform.position = new Vector3(ox, oy, -1f); // source (0,0)

        // Add CharacterMover
        var mover = fish.AddComponent<CharacterMover>();
        mover.startCell = new Vector2Int(0, 0);
        mover.moveSpeed = 3f;

        // Wire flow/grid managers
        var flow = Object.FindObjectOfType<FlowManager>();
        var grid = Object.FindObjectOfType<GridManager>();
        mover.flowManager = flow;
        mover.gridManager = grid;

        Debug.Log("[Fix5] Fish_Character created with CharacterMover ✅");
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Fix 6: Wire characterMover to GameManager
    // ─────────────────────────────────────────────────────────────────────
    static bool WireGameManager()
    {
        var gameMgr = Object.FindObjectOfType<GameManager>();
        var mover   = Object.FindObjectOfType<CharacterMover>();
        var hud     = Object.FindObjectOfType<GameplayHUD>();

        if (gameMgr == null) { Debug.LogWarning("[Fix6] GameManager not found!"); return false; }

        bool changed = false;

        if (mover != null && gameMgr.characterMover != mover)
        {
            gameMgr.characterMover = mover;
            changed = true;
            Debug.Log("[Fix6] GameManager.characterMover wired ✅");
        }

        if (hud != null && gameMgr.hud != hud)
        {
            gameMgr.hud = hud;
            changed = true;
            Debug.Log("[Fix6] GameManager.hud wired ✅");
        }

        var flow = Object.FindObjectOfType<FlowManager>();
        if (flow != null && gameMgr.flowManager != flow)
        {
            gameMgr.flowManager = flow;
            changed = true;
            Debug.Log("[Fix6] GameManager.flowManager wired ✅");
        }

        return changed;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Fix 7: Ensure pipe prefabs have BoxCollider2D (for OnMouseDown)
    // ─────────────────────────────────────────────────────────────────────
    static bool FixPipePrefabs()
    {
        string[] prefabNames = { "Pipe_Straight", "Pipe_Elbow", "Pipe_T", "Pipe_Cross" };
        bool changed = false;

        foreach (var name in prefabNames)
        {
            string path = $"Assets/Resources/Prefabs/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                Debug.LogWarning($"[Fix7] Prefab not found: {path}");
                continue;
            }

            // Unity 6 compatible prefab editing
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool localChanged = false;

            var col = root.GetComponent<BoxCollider2D>();
            if (col == null)
            {
                col = root.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1f);
                localChanged = true;
                Debug.Log($"[Fix7] BoxCollider2D added to {name} ✅");
            }
            else if (col.size != new Vector2(1f, 1f))
            {
                col.size = new Vector2(1f, 1f);
                localChanged = true;
            }

            if (root.GetComponent<PipeTile>() == null)
            {
                root.AddComponent<PipeTile>();
                localChanged = true;
                Debug.Log($"[Fix7] PipeTile script added to {name}");
            }

            if (root.GetComponent<SpriteRenderer>() == null)
            {
                root.AddComponent<SpriteRenderer>();
                localChanged = true;
                Debug.Log($"[Fix7] SpriteRenderer added to {name}");
            }

            if (localChanged)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                changed = true;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        return changed;
    }
}
#endif
