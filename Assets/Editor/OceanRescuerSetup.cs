// DISABLED: OceanRescuerSetup is superseded by Level01Setup.cs and AllScenesBuilder.cs
// Kept for reference only. Do not delete.
#if UNITY_EDITOR && OCEAN_RESCUER_OLD_SETUP_ENABLED
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// OceanRescuer ke liye one-click auto setup.
/// Menu: OceanRescuer > Auto Setup Scene
/// </summary>
public static class OceanRescuerSetup
{
    // ── Sprite GUIDs ─────────────────────────────────────────
    private const string GUID_STRAIGHT  = "6d2c7962ab9f0694d81a057861194a5f";
    private const string GUID_ELBOW     = "bf3e79218254d904c955fb4058850ce3";
    private const string GUID_TJUNCTION = "1111dda2ddff32948a0d42a4f6bbed68";
    private const string GUID_CROSS     = "6d636c6bd62c5b14093ee393a667369a";

    private const string PREFAB_FOLDER  = "Assets/Art/Pipes/Prefabs";

    // ─────────────────────────────────────────────────────────
    [MenuItem("OceanRescuer/Setup Pipe Grid")]
    public static void SetupPipeGrid()
    {
        if (!EditorUtility.DisplayDialog("OceanRescuer Setup",
            "Yeh script:\n✅ Pipe prefabs banayegi\n✅ Level01 scene mein 5x5 grid setup karegi\n✅ GameManager configure karegi\n\nContinue?",
            "Haan, Setup Karo!", "Cancel")) return;

        try
        {
            EditorUtility.DisplayProgressBar("Setup", "Prefab folder...", 0.1f);
            EnsureFolder(PREFAB_FOLDER);

            EditorUtility.DisplayProgressBar("Setup", "Pipe prefabs bana raha hai...", 0.3f);
            var straight = CreatePipePrefab(GUID_STRAIGHT,  PipeTile.PipeType.Straight,  "Pipe_Straight");
            var elbow    = CreatePipePrefab(GUID_ELBOW,     PipeTile.PipeType.Elbow,     "Pipe_Elbow");
            var tJunc    = CreatePipePrefab(GUID_TJUNCTION, PipeTile.PipeType.TJunction, "Pipe_T");
            var cross    = CreatePipePrefab(GUID_CROSS,     PipeTile.PipeType.Cross,     "Pipe_Cross");

            EditorUtility.DisplayProgressBar("Setup", "Scene setup...", 0.6f);
            SetupScene(straight, elbow, tJunc, cross);

            EditorUtility.DisplayProgressBar("Setup", "Saving...", 0.9f);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Done!", "Setup complete!\n\n▶ Play dabao aur pipes click karo.\nCyan tiles = connected path\nSource(0,0) → Sink(4,4) = WIN!", "OK!");
        }
        catch (System.Exception e)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[OceanRescuer] Error: " + e.Message + "\n" + e.StackTrace);
            EditorUtility.DisplayDialog("Error!", "Setup fail hua. Console mein error dekho.\n\n" + e.Message, "OK");
        }
    }

    // ─────────────────────────────────────────────────────────
    // Prefab creation
    // ─────────────────────────────────────────────────────────
    private static GameObject CreatePipePrefab(string guid, PipeTile.PipeType type, string prefabName)
    {
        string prefabPath = PREFAB_FOLDER + "/" + prefabName + ".prefab";
        if (File.Exists(prefabPath))
        {
            var ex = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (ex != null) return ex;
        }

        string spritePath = AssetDatabase.GUIDToAssetPath(guid);
        Sprite sprite = null;

        // Try direct load
        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            // Spritesheet — load all and pick first Sprite
            Object[] all = AssetDatabase.LoadAllAssetsAtPath(spritePath);
            foreach (var o in all)
                if (o is Sprite s) { sprite = s; break; }
        }

        // Build GameObject
        var go = new GameObject(prefabName);

        var sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        var pipe = go.AddComponent<PipeTile>();
        pipe.pipeType = type;

        go.AddComponent<GridCell>();

        var saved = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        Debug.Log("[Setup] Prefab created: " + prefabName);
        return saved;
    }

    // ─────────────────────────────────────────────────────────
    // Scene setup
    // ─────────────────────────────────────────────────────────
    private static void SetupScene(GameObject straight, GameObject elbow, GameObject tJunc, GameObject cross)
    {
        // Clean slate
        DestroyIfExists("GameManager");
        DestroyIfExists("Grid");

        // ── GameManager ──
        var gmObj = new GameObject("GameManager");
        var gm    = gmObj.AddComponent<GameManager>();
        var flow  = gmObj.AddComponent<FlowManager>();
        var grid  = gmObj.AddComponent<GridManager>();
        gmObj.AddComponent<MoveManager>();

        grid.Cols                = 5;
        grid.Rows                = 5;
        grid.TileSize            = 1.1f;   // 1.1 → pipes just slightly apart; sprites ~1 unit with PPU=1024
        grid.useScenePlacedTiles = true;

        var gridRoot = new GameObject("Grid");

        // ═══════════════════════════════════════════════════════
        //  SNAKE PATH  (verified opening-by-opening)
        //
        //  Coordinate system: x=col(0=left), y=row(0=bottom)
        //
        //  PATH:
        //  (0,0)→(1,0)→(2,0)→(3,0)→(4,0)   row-0, left→right
        //                              ↑↓
        //  (4,2)→(3,2)→(2,2)→(1,2)→(0,2)   row-2, right→left (via col-4 up)
        //    ↑
        //  (0,4)→(1,4)→(2,4)→(3,4)→(4,4)   row-4, left→right (via col-0 up)
        //
        //  Pipe openings (PipeTile.cs):
        //   Straight  base [0]=Up,[2]=Down  → rot0=Vertical, rot1=Horizontal
        //   Elbow     base [0]=Up,[1]=Right → rot0=U+R, rot1=R+D, rot2=D+L, rot3=L+U
        //   TJunction base [0]=Up,[1]=Right,[3]=Left → rot0=U+R+L, rot1=U+R+D, rot2=R+D+L, rot3=U+D+L
        //
        //  HasOpening(dir): adjusted = (dir - rotStep + 4) % 4; return base[adjusted]
        //
        //  PUZZLE VALVES at col-2: TJunction tiles start scrambled.
        //  Correct rotations: TJ(2,0)=rot2, TJ(2,2)=rot0, TJ(2,4)=rot2
        //  Puzzle  rotations: TJ(2,0)=rot1, TJ(2,2)=rot3, TJ(2,4)=rot1  (each needs 1 click)
        //
        //  NON-PATH filler strategy:
        //   • (0,1),(2,1) = S-Horizontal → no Up/Down, won't bridge path rows
        //   • (4,3),(2,3) = S-Horizontal → same reason
        //   • All other fillers = S-Vertical (safe; no adjacent H-path tile has Up/Down)
        // ═══════════════════════════════════════════════════════

        // layout[4-y, x]  →  layout[0,x]=y=4(top) … layout[4,x]=y=0(bottom)
        var E = PipeTile.PipeType.Elbow;
        var S = PipeTile.PipeType.Straight;
        var T = PipeTile.PipeType.TJunction;

        PipeTile.PipeType[,] layout = new PipeTile.PipeType[5, 5]
        {
            //      col0  col1  col2  col3  col4
            /* y4 */ { E,    S,    T,    S,    E },
            /* y3 */ { S,    S,    S,    S,    S },
            /* y2 */ { E,    S,    T,    S,    E },
            /* y1 */ { S,    S,    S,    S,    S },
            /* y0 */ { E,    S,    T,    S,    E },
        };

        // initRot[4-y, x]
        int[,] initRot = new int[5, 5]
        {
            //      col0  col1  col2(puzzle)  col3  col4
            /* y4 */ { 1,    1,    1,           1,    2 },  // E:R+D  S:H  TJ:wrong  S:H  E:D+L
            /* y3 */ { 0,    0,    1,           0,    1 },  // S:V[path] filler filler filler S:H[no Up]
            /* y2 */ { 0,    1,    3,           1,    2 },  // E:U+R  S:H  TJ:wrong  S:H  E:D+L
            /* y1 */ { 1,    0,    1,           0,    0 },  // S:H[no Up/Dn] filler S:H filler S:V[path]
            /* y0 */ { 0,    1,    1,           1,    3 },  // E:U+R  S:H  TJ:wrong  S:H  E:U+L
        };

        float ts = grid.TileSize;  // = 1.1f
        float ox = -(grid.Cols - 1) * ts * 0.5f;
        float oy = -(grid.Rows - 1) * ts * 0.5f;

        for (int x = 0; x < 5; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                int ri = 4 - y;   // row index in layout array (0=top/y4, 4=bottom/y0)
                PipeTile.PipeType pType = layout[ri, x];
                GameObject prefab = PrefabForType(pType, straight, elbow, tJunc, cross);

                var tileGO = (GameObject)PrefabUtility.InstantiatePrefab(prefab, gridRoot.transform);
                tileGO.name = "Pipe_" + x + "_" + y;
                tileGO.transform.position   = new Vector3(ox + x * ts, oy + y * ts, 0f);
                // Keep sprites at natural 1x scale — gap between tiles shows as pipe joints
                tileGO.transform.localScale  = Vector3.one;

                // Apply visual rotation so sprites face correct direction
                int rot = initRot[ri, x];
                tileGO.transform.rotation = Quaternion.Euler(0f, 0f, -90f * rot);

                var tile = tileGO.GetComponent<PipeTile>();
                var cell = tileGO.GetComponent<GridCell>();
                if (tile != null) { tile.pipeType = pType; tile.rotationStep = rot; }
                if (cell != null) { cell.gridX = x; cell.gridY = y; }
            }
        }

        // Source (0,0): Elbow rot0 = Up+Right  →  Right connects to (1,0). Green, locked.
        LockTile(gridRoot, 0, 0, 0, new Color(0.3f, 1f, 0.3f));
        // Sink (4,4): Elbow rot2 = Down+Left  →  Left connects to (3,4). Red, locked.
        LockTile(gridRoot, 4, 4, 2, new Color(1f, 0.3f, 0.3f));

        // ── Wire GameManager/FlowManager ──
        var flowSO = new SerializedObject(flow);
        flowSO.FindProperty("gridManager").objectReferenceValue = grid;
        flowSO.FindProperty("sourceCell").vector2IntValue       = new Vector2Int(0, 0);
        flowSO.FindProperty("sinkCell").vector2IntValue         = new Vector2Int(4, 4);
        flowSO.ApplyModifiedProperties();

        var gmSO = new SerializedObject(gm);
        gmSO.FindProperty("flowManager").objectReferenceValue = flow;
        gmSO.ApplyModifiedProperties();

        // ── Camera ──
        // Grid: 4×1.1 = 4.4 units wide & tall (centered at origin)
        // Portrait 9:16 → visibleW = orthoSize×2×(9/16)
        // orthoSize=4.0 → visibleW = 8×(9/16) = 4.5 → grid(4.4) fits ✓
        // orthoSize=4.0 → visibleH = 8.0 → grid(4.4 tall) fits + 3.6 for HUD ✓
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic       = true;
            cam.orthographicSize   = 4.0f;
            cam.backgroundColor    = new Color(0.04f, 0.13f, 0.27f);
        }

        EditorUtility.SetDirty(gmObj);
        EditorUtility.SetDirty(gridRoot);
        Debug.Log("[OceanRescuer] Snake-path setup done! 3 TJ valves need 1 click each to solve.");
    }

    // ─────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────
    private static void LockTile(GameObject gridRoot, int x, int y, int rotation, Color color)
    {
        var t = gridRoot.transform.Find("Pipe_" + x + "_" + y);
        if (t == null) return;
        var tile = t.GetComponent<PipeTile>();
        if (tile != null) { tile.rotationStep = rotation; tile.isLocked = true; EditorUtility.SetDirty(tile); }
        var sr = t.GetComponent<SpriteRenderer>();
        if (sr != null) { sr.color = color; EditorUtility.SetDirty(sr); }
    }

    private static GameObject PrefabForType(PipeTile.PipeType type, GameObject s, GameObject e, GameObject t, GameObject c)
    {
        if (type == PipeTile.PipeType.Elbow)     return e;
        if (type == PipeTile.PipeType.TJunction) return t;
        if (type == PipeTile.PipeType.Cross)     return c;
        return s;
    }

    private static void DestroyIfExists(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) { Object.DestroyImmediate(go); Debug.Log("[Setup] Deleted old: " + name); }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
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
