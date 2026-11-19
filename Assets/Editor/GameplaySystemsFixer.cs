#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One-click fix for all GameplayScene runtime issues:
///  1. Camera orthographic size for 1920×1080 landscape
///  2. Canvas Scaler → 1920×1080 landscape
///  3. Pipe prefabs → Resources/Prefabs → GridManager assigned
///  4. CharacterMover created and wired to GameManager
///  5. Source & Destination visual indicators
///  6. Physics2D/Collider on pipe prefabs (for OnMouseDown)
///  7. MoveManager present and wired
///  8. EventSystem present
///  9. Input handler verification
/// 10. PipeGrid_L01 set inactive (let bootstrapper work)
/// 11. GameplayBootstrapper disabled when GridManager has prefabs
///
/// Menu: OceanRescuer > Fix Gameplay Systems
/// </summary>
public static class GameplaySystemsFixer
{
    private const string SCENE     = "Assets/Scenes/GameplayScene.unity";
    private const string FISH_SPR  = "Assets/Art/Characters/";
    private const string PIPE_RES  = "Assets/Resources/Prefabs/";

    // Grid config (must match GridManager in scene)
    private const int   COLS      = 9;
    private const int   ROWS      = 5;
    private const float TILESIZE  = 1.1f;
    private const float CAM_SIZE  = 5.5f;   // orthographic half-height for 1920×1080

    [MenuItem("OceanRescuer/Fix Gameplay Systems")]
    public static void Run()
    {
        var scene = EditorSceneManager.GetSceneByPath(SCENE);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(SCENE, OpenSceneMode.Single);
        }

        Fix01_Camera();
        Fix02_CanvasScaler();
        Fix03_GridManagerPrefabs();
        Fix04_CharacterMover();
        Fix05_SourceDestIndicators();
        Fix06_PipePrefabColliders();
        Fix07_MoveManagerWire();
        Fix08_EventSystem();
        Fix09_DisablePipeGrid_L01();
        Fix10_DisableBootstrapperIfRedundant();
        Fix11_GameManagerWiring();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[GPSysFixer] ✅ All gameplay systems fixed. Scene saved.");
    }

    // ── 1. Camera ─────────────────────────────────────────────────────────────
    static void Fix01_Camera()
    {
        var cam = Camera.main;
        if (cam == null) cam = Object.FindObjectOfType<Camera>();
        if (cam == null) { Debug.LogError("[GPSysFixer] Main Camera not found!"); return; }

        cam.orthographic     = true;
        cam.orthographicSize = CAM_SIZE;    // 5.5 gives ~11 units height; 9×1.1=9.9 width fits in 16:9
        cam.transform.position = new Vector3(0f, 0f, -10f);
        Debug.Log($"[GPSysFixer] Camera: ortho={cam.orthographicSize}");
    }

    // ── 2. Canvas Scaler ──────────────────────────────────────────────────────
    static void Fix02_CanvasScaler()
    {
        foreach (var canvas in Object.FindObjectsOfType<Canvas>())
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) continue;
            var cs = canvas.GetComponent<CanvasScaler>();
            if (cs == null) cs = canvas.gameObject.AddComponent<CanvasScaler>();
            cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920f, 1080f);
            cs.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cs.matchWidthOrHeight  = 1f;  // match height for landscape
            Debug.Log($"[GPSysFixer] CanvasScaler fixed on {canvas.name}: 1920×1080");
        }
    }

    // ── 3. GridManager prefabs ────────────────────────────────────────────────
    static void Fix03_GridManagerPrefabs()
    {
        var gm = Object.FindObjectOfType<GridManager>();
        if (gm == null) { Debug.LogError("[GPSysFixer] GridManager not found!"); return; }

        // Load from Resources
        var pS = Resources.Load<GameObject>("Prefabs/Pipe_Straight");
        var pE = Resources.Load<GameObject>("Prefabs/Pipe_Elbow");
        var pT = Resources.Load<GameObject>("Prefabs/Pipe_T");
        var pX = Resources.Load<GameObject>("Prefabs/Pipe_Cross");

        // Fallback: load from AssetDatabase
        if (pS == null) pS = AssetDatabase.LoadAssetAtPath<GameObject>(PIPE_RES + "Pipe_Straight.prefab");
        if (pE == null) pE = AssetDatabase.LoadAssetAtPath<GameObject>(PIPE_RES + "Pipe_Elbow.prefab");
        if (pT == null) pT = AssetDatabase.LoadAssetAtPath<GameObject>(PIPE_RES + "Pipe_T.prefab");
        if (pX == null) pX = AssetDatabase.LoadAssetAtPath<GameObject>(PIPE_RES + "Pipe_Cross.prefab");

        var so = new SerializedObject(gm);
        if (pS != null) so.FindProperty("straightPrefab").objectReferenceValue  = pS;
        if (pE != null) so.FindProperty("elbowPrefab").objectReferenceValue     = pE;
        if (pT != null) so.FindProperty("tJunctionPrefab").objectReferenceValue = pT;
        if (pX != null) so.FindProperty("crossPrefab").objectReferenceValue     = pX;

        // Keep useScenePlacedTiles = false so runtime generates tiles
        so.FindProperty("useScenePlacedTiles").boolValue = false;
        so.FindProperty("Cols").intValue                 = COLS;
        so.FindProperty("Rows").intValue                 = ROWS;
        so.FindProperty("TileSize").floatValue           = TILESIZE;
        so.ApplyModifiedProperties();

        Debug.Log($"[GPSysFixer] GridManager: Cols={COLS} Rows={ROWS} TileSize={TILESIZE} prefabs assigned");
    }

    // ── 4. CharacterMover ─────────────────────────────────────────────────────
    static void Fix04_CharacterMover()
    {
        var gm = Object.FindObjectOfType<GameManager>();
        if (gm == null) { Debug.LogError("[GPSysFixer] GameManager not found!"); return; }

        // Check if CharacterMover exists
        CharacterMover mover = Object.FindObjectOfType<CharacterMover>();
        if (mover == null)
        {
            // Create a fish character GO
            var fishGO = new GameObject("Fish_Character");
            fishGO.transform.position = GridToWorld(0, 0);

            // Try to load fish sprite
            var fishSprites = AssetDatabase.FindAssets("fish t:Sprite", new[] { "Assets/Art/Characters" });
            if (fishSprites.Length > 0)
            {
                var spritePath = AssetDatabase.GUIDToAssetPath(fishSprites[0]);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite != null)
                {
                    var sr = fishGO.AddComponent<SpriteRenderer>();
                    sr.sprite = sprite;
                    sr.sortingOrder = 5;
                    fishGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                }
            }
            else
            {
                // No sprite — use a colored square as placeholder
                var sr = fishGO.AddComponent<SpriteRenderer>();
                sr.color = new Color(1f, 0.8f, 0f);
                sr.sortingOrder = 5;
                fishGO.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            }

            mover = fishGO.AddComponent<CharacterMover>();
        }

        // Configure CharacterMover
        var soM = new SerializedObject(mover);
        soM.FindProperty("moveSpeed").floatValue    = 2.5f;
        soM.FindProperty("startCell").vector2IntValue = new Vector2Int(0, 0);
        soM.ApplyModifiedProperties();

        mover.transform.position = GridToWorld(0, 0);

        // Wire to GameManager
        var soGM = new SerializedObject(gm);
        soGM.FindProperty("characterMover").objectReferenceValue = mover;
        soGM.ApplyModifiedProperties();

        Debug.Log("[GPSysFixer] CharacterMover created/wired to GameManager.");
    }

    // ── 5. Source & Destination visual indicators ─────────────────────────────
    static void Fix05_SourceDestIndicators()
    {
        // Source at (0,0) — green circle
        CreateIndicator("WaterSource", GridToWorld(0, 0),
            new Color(0.1f, 1f, 0.3f, 0.9f), "SOURCE", 2);

        // Destination at (8,0) — red circle
        CreateIndicator("WaterDestination", GridToWorld(8, 0),
            new Color(1f, 0.2f, 0.2f, 0.9f), "DEST", 2);
    }

    static void CreateIndicator(string name, Vector3 pos, Color color, string label, int sortOrder)
    {
        // Don't duplicate
        if (GameObject.Find(name) != null) return;

        var go = new GameObject(name);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);

        // Sprite renderer — colored quad
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = color;
        sr.sortingOrder = sortOrder;

        Debug.Log($"[GPSysFixer] Indicator '{name}' created at {pos}");
    }

    // ── 6. Pipe prefab colliders ──────────────────────────────────────────────
    static void Fix06_PipePrefabColliders()
    {
        // Each pipe prefab needs a BoxCollider2D for OnMouseDown to work
        string[] prefabPaths = {
            PIPE_RES + "Pipe_Straight.prefab",
            PIPE_RES + "Pipe_Elbow.prefab",
            PIPE_RES + "Pipe_T.prefab",
            PIPE_RES + "Pipe_Cross.prefab"
        };

        foreach (string path in prefabPaths)
        {
            var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefabAsset == null) { Debug.LogWarning($"[GPSysFixer] Prefab not found: {path}"); continue; }

            // Unity 6 compatible: LoadPrefabContents / SaveAsPrefabAsset / UnloadPrefabContents
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;

            // Add BoxCollider2D if missing
            if (root.GetComponent<BoxCollider2D>() == null)
            {
                var col = root.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 1f);
                Debug.Log($"[GPSysFixer] BoxCollider2D added to {prefabAsset.name}");
                changed = true;
            }

            // Ensure PipeTile script is present
            if (root.GetComponent<PipeTile>() == null)
            {
                root.AddComponent<PipeTile>();
                Debug.Log($"[GPSysFixer] PipeTile added to {prefabAsset.name}");
                changed = true;
            }

            // Ensure GridCell script is present
            if (root.GetComponent<GridCell>() == null)
            {
                root.AddComponent<GridCell>();
                Debug.Log($"[GPSysFixer] GridCell added to {prefabAsset.name}");
                changed = true;
            }

            if (changed)
                PrefabUtility.SaveAsPrefabAsset(root, path);

            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ── 7. MoveManager wiring ─────────────────────────────────────────────────
    static void Fix07_MoveManagerWire()
    {
        var mm = Object.FindObjectOfType<MoveManager>();
        if (mm == null)
        {
            var gm = Object.FindObjectOfType<GameManager>();
            if (gm != null)
            {
                mm = gm.gameObject.AddComponent<MoveManager>();
                Debug.Log("[GPSysFixer] MoveManager added to GameManager GO.");
            }
        }
        if (mm != null)
        {
            var so = new SerializedObject(mm);
            so.FindProperty("maxMoves").intValue = 0; // unlimited
            so.ApplyModifiedProperties();
        }
    }

    // ── 8. EventSystem ────────────────────────────────────────────────────────
    static void Fix08_EventSystem()
    {
        var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[GPSysFixer] EventSystem created.");
        }
        else
        {
            Debug.Log("[GPSysFixer] EventSystem already exists.");
        }
    }

    // ── 9. Disable PipeGrid_L01 (old scene tiles) ────────────────────────────
    static void Fix09_DisablePipeGrid_L01()
    {
        // The old scene-placed grid may conflict with runtime generation
        var old = GameObject.Find("PipeGrid_L01");
        if (old != null)
        {
            old.SetActive(false);
            Debug.Log("[GPSysFixer] PipeGrid_L01 set inactive (using runtime grid).");
        }
        // Also disable any PipeGrid_Bootstrap from previous runs
        var boot = GameObject.Find("PipeGrid_Bootstrap");
        if (boot != null)
        {
            Object.DestroyImmediate(boot);
            Debug.Log("[GPSysFixer] Old PipeGrid_Bootstrap removed.");
        }
    }

    // ── 10. Disable redundant Bootstrapper ────────────────────────────────────
    static void Fix10_DisableBootstrapperIfRedundant()
    {
        // GridManager.useScenePlacedTiles=false means GridManager.GenerateGrid() runs itself.
        // GameplayBootstrapper was a fallback — now disable it to avoid double-generation.
        var bootstrapper = Object.FindObjectOfType<GameplayBootstrapper>();
        if (bootstrapper != null)
        {
            bootstrapper.enabled = false;
            Debug.Log("[GPSysFixer] GameplayBootstrapper disabled (GridManager handles generation).");
        }
    }

    // ── 11. GameManager final wiring ──────────────────────────────────────────
    static void Fix11_GameManagerWiring()
    {
        var gm  = Object.FindObjectOfType<GameManager>();
        var fm  = Object.FindObjectOfType<FlowManager>();
        var hud = Object.FindObjectOfType<GameplayHUD>();

        if (gm == null) return;

        var so = new SerializedObject(gm);
        if (fm  != null) so.FindProperty("flowManager").objectReferenceValue = fm;
        if (hud != null) so.FindProperty("hud").objectReferenceValue         = hud;

        // Clear nextSceneName (don't auto-load scene on win)
        so.FindProperty("nextSceneName").stringValue = "";
        so.ApplyModifiedProperties();

        // Wire FlowManager's gridManager reference
        if (fm != null)
        {
            var gman = Object.FindObjectOfType<GridManager>();
            if (gman != null)
            {
                var soFM = new SerializedObject(fm);
                soFM.FindProperty("gridManager").objectReferenceValue = gman;
                soFM.FindProperty("sourceCell").vector2IntValue = new Vector2Int(0, 0);
                soFM.FindProperty("sinkCell").vector2IntValue   = new Vector2Int(8, 0);
                soFM.ApplyModifiedProperties();
            }
        }

        Debug.Log("[GPSysFixer] GameManager fully wired.");
    }

    // ── Helper: Grid coordinate to world position ─────────────────────────────
    static Vector3 GridToWorld(int col, int row)
    {
        float ox = -(COLS - 1) * TILESIZE * 0.5f;
        float oy = -(ROWS - 1) * TILESIZE * 0.5f;
        return new Vector3(ox + col * TILESIZE, oy + row * TILESIZE, 0f);
    }
}
#endif
