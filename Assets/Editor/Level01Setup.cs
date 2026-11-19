#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Level 01 — 9×5 pipe puzzle layout based on reference image.
/// Menu: OceanRescuer > Build Level 01
///
/// Reference layout (9 cols × 5 rows, x=col 0=left, y=row 0=bottom):
///
///   y=4  [E]─[S]─[S]─[T]─[S]─[S]─[T]─[S]─[E]
///         |               |               |
///   y=3  [S]  [  ]  [  ] [S] [  ] [  ]  [S]
///         |               |               |
///   y=2  [T]─[S]─[S]─[X]─[S]─[S]─[X]─[S]─[T]
///         |               |               |
///   y=1  [S]  [  ]  [  ] [S] [  ] [  ]  [S]
///         |               |               |
///   y=0  [E]─[S]─[S]─[T]─[S]─[S]─[T]─[S]─[E]
///       SOURCE(0,0)                        SINK(8,0)
///
/// E=Elbow, S=Straight, T=TJunction(valve), X=Cross
/// Non-path cells are EMPTY (no pipe placed) so path is clear to see.
/// </summary>
public static class Level01Setup
{
    // ── Asset GUIDs — these are your existing pipe sprites ────────────────
    private const string GUID_STRAIGHT  = "6d2c7962ab9f0694d81a057861194a5f";   // Glossy Blue Water Pipe Asset.png
    private const string GUID_ELBOW     = "bf3e79218254d904c955fb4058850ce3";   // Glossy Blue Elbow Pipe with Clamps.png
    private const string GUID_TJUNCTION = "1111dda2ddff32948a0d42a4f6bbed68";   // Glossy Transparent Pipe T-Junction.png
    private const string GUID_CROSS     = "6d636c6bd62c5b14093ee393a667369a";   // Glossy Blue Cross Pipe Connector.png

    private const string PREFAB_FOLDER  = "Assets/Art/Pipes/Prefabs";

    // ── Grid constants ────────────────────────────────────────────────────
    private const int   COLS      = 9;
    private const int   ROWS      = 5;
    private const float TILE_SIZE = 1.1f;    // world units between pipe centers (matches ~1-unit sprites)
    // Per-type uniform scale so each sprite fits exactly inside TILE_SIZE world units.
    // Formula: TILE_SIZE / (spritePixelWidth / PPU)  at PPU=1024
    //   Straight  : 2135px / 1024 = 2.085 world units → 1.1 / 2.085 = 0.528
    //   Elbow     : 1382px / 1024 = 1.350 world units → 1.1 / 1.350 = 0.815
    //   TJunction : 1511px / 1024 = 1.475 world units → 1.1 / 1.475 = 0.746
    //   Cross     : 1271px / 1024 = 1.241 world units → 1.1 / 1.241 = 0.887
    // Index order: S=0, E=1, T=2, X=3
    private static readonly float[] PIPE_SCALE = { 0.528f, 0.815f, 0.746f, 0.887f };

    // ── Pipe type shorthand ───────────────────────────────────────────────
    private const int NONE = -1;   // empty cell (no pipe)
    private const int S    =  0;   // Straight
    private const int E    =  1;   // Elbow
    private const int T    =  2;   // TJunction  (valve / puzzle piece)
    private const int X    =  3;   // Cross

    // ── SOURCE & SINK ─────────────────────────────────────────────────────
    private static readonly Vector2Int SOURCE = new Vector2Int(0, 0);
    private static readonly Vector2Int SINK   = new Vector2Int(8, 0);

    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("OceanRescuer/Build Level 01 (9x5 Snake)")]
    public static void BuildLevel01()
    {
        if (!EditorUtility.DisplayDialog("Level 01 Setup",
            "Yeh script:\n" +
            "✅ 9×5 pipe grid banayegi (reference image jaisa)\n" +
            "✅ GridManager ko 9×5 set karegi\n" +
            "✅ Camera adjust karegi\n" +
            "✅ Existing characters/background untouched rahenge\n\n" +
            "Continue?", "Build!", "Cancel")) return;

        try
        {
            EditorUtility.DisplayProgressBar("Level 01", "Prefabs...", 0.1f);
            EnsureFolder(PREFAB_FOLDER);

            var pfStraight = GetOrCreatePrefab(GUID_STRAIGHT,  PipeTile.PipeType.Straight,  "Pipe_Straight");
            var pfElbow    = GetOrCreatePrefab(GUID_ELBOW,     PipeTile.PipeType.Elbow,     "Pipe_Elbow");
            var pfT        = GetOrCreatePrefab(GUID_TJUNCTION, PipeTile.PipeType.TJunction, "Pipe_T");
            var pfCross    = GetOrCreatePrefab(GUID_CROSS,     PipeTile.PipeType.Cross,     "Pipe_Cross");

            EditorUtility.DisplayProgressBar("Level 01", "Building scene...", 0.4f);
            BuildScene(pfStraight, pfElbow, pfT, pfCross);

            EditorUtility.DisplayProgressBar("Level 01", "Saving...", 0.9f);
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog("Done!",
                "Level 01 ready!\n\n" +
                "▶ Play dabao — path ALREADY connected hai!\n" +
                "🟢 Source = (0,0)  🔴 Sink = (8,0)\n" +
                "Fish automatically source se sink tak chalegi!\n\n" +
                "(Optional: TJunction tiles click karo rotate karne ke liye — yahi puzzle hai)", "OK!");
        }
        catch (System.Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogError("[Level01] Error: " + ex.Message + "\n" + ex.StackTrace);
            EditorUtility.DisplayDialog("Error!", ex.Message, "OK");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // LAYOUT DEFINITION
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Pipe type per cell. layout[row, col], row 0 = BOTTOM (y=0).
    /// NONE = empty cell.
    ///
    /// Reference-image snake path:
    ///   Bottom row  y=0: (0)→(1)→(2)→(3)→(4)→(5)→(6)→(7)→(8)   left→right
    ///   Right col   x=8: (8,0)→(8,1)→(8,2)                        up
    ///   Middle row  y=2: (8,2)←(7)←(6)←(5)←(4)←(3)←(2)←(1)←(0)  right→left
    ///   Left col    x=0: (0,2)→(0,3)→(0,4)                         up
    ///   Top row     y=4: (0,4)→(1)→(2)→(3)→(4)→(5)→(6)→(7)→(8,4) left→right
    ///
    /// Puzzle: 6 TJunction valves scrambled — player rotates them to open path.
    ///   T at (3,0),(6,0) on bottom row horizontal
    ///   T at (3,2),(6,2) on middle row horizontal
    ///   T at (3,4),(6,4) on top row horizontal
    /// </summary>
    private static int[,] PipeLayout()
    {
        // 5 rows × 9 cols  — index [row, col]  row0=bottom
        int[,] L = new int[ROWS, COLS];

        // fill everything with NONE first
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
                L[r, c] = NONE;

        // ── y=0 bottom row (horizontal, left→right) ──
        L[0, 0] = E;   // source elbow: Up+Right (rot0 locked)
        L[0, 1] = S;   // straight H
        L[0, 2] = S;
        L[0, 3] = T;   // valve puzzle
        L[0, 4] = S;
        L[0, 5] = S;
        L[0, 6] = T;   // valve puzzle
        L[0, 7] = S;
        L[0, 8] = E;   // sink elbow: Up+Left (rot3) → connects left=path, up→col8

        // ── x=8 right column (vertical, bottom→mid) ──
        L[1, 8] = S;   // straight V

        // ── y=2 middle row (horizontal, right→left) ──
        L[2, 0] = E;   // elbow Up+Right (rot0) → up to col0, right to row2 path
        L[2, 1] = S;
        L[2, 2] = S;
        L[2, 3] = T;   // valve puzzle
        L[2, 4] = S;
        L[2, 5] = S;
        L[2, 6] = T;   // valve puzzle
        L[2, 7] = S;
        L[2, 8] = E;   // elbow Down+Left (rot2) → down to (8,1), left to row2 path

        // ── x=0 left column (vertical, mid→top) ──
        L[3, 0] = S;   // straight V

        // ── y=4 top row (horizontal, left→right) ──
        L[4, 0] = E;   // elbow Right+Down (rot1) → right to row4, down to col0
        L[4, 1] = S;
        L[4, 2] = S;
        L[4, 3] = T;   // valve puzzle
        L[4, 4] = S;
        L[4, 5] = S;
        L[4, 6] = T;   // valve puzzle
        L[4, 7] = S;
        L[4, 8] = E;   // end elbow Down+Left (rot2)  [decorative top-right corner]

        return L;
    }

    /// <summary>
    /// Initial rotation per cell [row, col].
    /// Rotation step: 0=original, 1=+90°CW, 2=180°, 3=+270°CW
    ///
    /// Pipe opening reference (PipeTile.HasOpening formula):
    ///   adjusted = (direction - rotStep + 4) % 4; return base[adjusted]
    ///   Straight base: [Up=T, R=F, Down=T, L=F] → rot0=Vert, rot1=Horiz
    ///   Elbow    base: [Up=T, R=T, D=F, L=F]    → rot0=U+R, rot1=R+D, rot2=D+L, rot3=L+U
    ///   TJunction base:[Up=T, R=T, D=F, L=T]    → rot0=U+R+L, rot1=U+R+D, rot2=R+D+L, rot3=U+D+L
    ///
    /// PATH requires (pre-solved correct rotations):
    ///   y=0 horizontal: Straight rot1(H), Elbow(0,0)rot0=U+R, Elbow(8,0)rot3=L+U
    ///   TJ at (3,0),(6,0) correct = rot2 (R+D+L → Left+Right for H-path)
    ///   y=2 horizontal: Straight rot1(H), Elbow(0,2)rot0=U+R, Elbow(8,2)rot2=D+L
    ///   TJ at (3,2),(6,2) correct = rot2
    ///   y=4 horizontal: Straight rot1(H), Elbow(0,4)rot1=R+D, Elbow(8,4)rot2=D+L
    ///   TJ at (3,4),(6,4) correct = rot2
    ///   Verticals: Straight rot0(V)
    ///
    /// PUZZLE: TJunctions start SCRAMBLED (rot1 = wrong) — player rotates once to fix.
    /// </summary>
    private static int[,] RotLayout()
    {
        int[,] R = new int[ROWS, COLS];
        // default rot = 0
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
                R[r, c] = 0;

        // ── y=0 bottom row ──
        R[0, 0] = 0;   // Elbow rot0 = U+R  (source, locked)
        R[0, 1] = 1;   // Straight H
        R[0, 2] = 1;
        R[0, 3] = 2;   // TJ rot2 = R+D+L → Left+Right CONNECTED ✓
        R[0, 4] = 1;
        R[0, 5] = 1;
        R[0, 6] = 2;   // TJ rot2 = R+D+L → Left+Right CONNECTED ✓
        R[0, 7] = 1;
        R[0, 8] = 3;   // Elbow rot3 = L+U  (sink, locked)

        // ── x=8 vertical ──
        R[1, 8] = 0;   // Straight V

        // ── y=2 middle row ──
        R[2, 0] = 0;   // Elbow rot0 = U+R
        R[2, 1] = 1;
        R[2, 2] = 1;
        R[2, 3] = 2;   // TJ rot2 = R+D+L → Left+Right CONNECTED ✓
        R[2, 4] = 1;
        R[2, 5] = 1;
        R[2, 6] = 2;   // TJ rot2 = R+D+L → Left+Right CONNECTED ✓
        R[2, 7] = 1;
        R[2, 8] = 2;   // Elbow rot2 = D+L

        // ── x=0 vertical ──
        R[3, 0] = 0;   // Straight V

        // ── y=4 top row ──
        R[4, 0] = 1;   // Elbow rot1 = R+D
        R[4, 1] = 1;
        R[4, 2] = 1;
        R[4, 3] = 2;   // TJ rot2 = R+D+L → Left+Right CONNECTED ✓
        R[4, 4] = 1;
        R[4, 5] = 1;
        R[4, 6] = 2;   // TJ rot2 = R+D+L → Left+Right CONNECTED ✓
        R[4, 7] = 1;
        R[4, 8] = 2;   // Elbow rot2 = D+L

        return R;
    }

    // ─────────────────────────────────────────────────────────────────────
    // SCENE BUILD
    // ─────────────────────────────────────────────────────────────────────

    private static void BuildScene(
        GameObject pfStraight, GameObject pfElbow,
        GameObject pfT,        GameObject pfCross)
    {
        // 1. Remove ALL old pipe grids (both old OceanRescuerSetup "Grid" + new "PipeGrid_L01")
        DestroyIfExists("PipeGrid_L01");
        DestroyIfExists("Grid");   // ← old OceanRescuerSetup grid ko bhi destroy karo

        // 2. Find / create GridManager on GameManager
        GridManager grid = Object.FindObjectOfType<GridManager>();
        FlowManager flow = Object.FindObjectOfType<FlowManager>();
        GameManager  gm  = Object.FindObjectOfType<GameManager>();

        bool createdGM = false;
        if (grid == null)
        {
            var gmObj = new GameObject("GameManager");
            gm   = gmObj.AddComponent<GameManager>();
            flow = gmObj.AddComponent<FlowManager>();
            grid = gmObj.AddComponent<GridManager>();
            gmObj.AddComponent<MoveManager>();
            createdGM = true;
        }

        // 3. Configure GridManager
        grid.Cols                = COLS;
        grid.Rows                = ROWS;
        grid.TileSize            = TILE_SIZE;
        grid.useScenePlacedTiles = true;

        // 4. Build pipe grid
        var gridRoot = new GameObject("PipeGrid_L01");
        int[,] layout = PipeLayout();
        int[,] rots   = RotLayout();

        float ox = -(COLS - 1) * TILE_SIZE * 0.5f;
        float oy = -(ROWS - 1) * TILE_SIZE * 0.5f;

        for (int col = 0; col < COLS; col++)
        {
            for (int row = 0; row < ROWS; row++)
            {
                int ptype = layout[row, col];
                if (ptype == NONE) continue;   // skip empty cells

                GameObject prefab = PickPrefab(ptype, pfStraight, pfElbow, pfT, pfCross);
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, gridRoot.transform);
                go.name = "Pipe_" + col + "_" + row;

                // Position
                go.transform.position   = new Vector3(ox + col * TILE_SIZE, oy + row * TILE_SIZE, 0f);
                // Scale — each type scaled so sprite fits exactly in TILE_SIZE
                float sc = PIPE_SCALE[ptype];
                go.transform.localScale = new Vector3(sc, sc, 1f);

                // Rotation
                int rot = rots[row, col];
                go.transform.rotation = Quaternion.Euler(0f, 0f, -90f * rot);

                // Components
                var tile = go.GetComponent<PipeTile>();
                var cell = go.GetComponent<GridCell>();
                var ptypeEnum = (PipeTile.PipeType)ptype;

                if (tile != null)
                {
                    tile.pipeType     = ptypeEnum;
                    tile.rotationStep = rot;
                }
                if (cell != null)
                {
                    cell.gridX = col;
                    cell.gridY = row;
                }
                EditorUtility.SetDirty(go);
            }
        }

        // 5. Lock Source & Sink
        LockCell(gridRoot, SOURCE.x, SOURCE.y, 0,
                 new Color(0.2f, 1f, 0.3f, 1f));   // green  = fish start
        LockCell(gridRoot, SINK.x,   SINK.y,   3,
                 new Color(1f, 0.3f, 0.3f, 1f));   // red    = rescue tank

        // 6. Wire FlowManager / GameManager
        if (flow != null)
        {
            var flowSO = new SerializedObject(flow);
            flowSO.FindProperty("gridManager").objectReferenceValue = grid;
            flowSO.FindProperty("sourceCell").vector2IntValue       = SOURCE;
            flowSO.FindProperty("sinkCell").vector2IntValue         = SINK;
            flowSO.ApplyModifiedProperties();
        }
        if (gm != null && flow != null)
        {
            var gmSO = new SerializedObject(gm);
            gmSO.FindProperty("flowManager").objectReferenceValue = flow;
            gmSO.ApplyModifiedProperties();
        }

        // 7. Camera — fit 9×5 grid in portrait (9:16)
        // TileSize=1.1 → grid: 8×1.1=8.8 wide, 4×1.1=4.4 tall
        // Portrait 9:16 → visibleW = orthoSize×2×(9/16)
        // orthoSize=8 → visibleW = 16×(9/16) = 9.0 → grid(8.8) fits ✓
        // orthoSize=8 → visibleH = 16.0 → grid(4.4) + 11.6 for background/HUD ✓
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic       = true;
            cam.orthographicSize   = 8.0f;
            cam.backgroundColor    = new Color(0.04f, 0.13f, 0.27f);
            EditorUtility.SetDirty(cam.gameObject);
        }

        EditorUtility.SetDirty(gridRoot);
        if (grid != null) EditorUtility.SetDirty(grid.gameObject);
        Debug.Log("[Level01] Built! 6 TJ valves need 1 click each. Source(0,0) → Sink(8,0).");
    }

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────

    private static void LockCell(GameObject root, int col, int row, int rot, Color color)
    {
        var t = root.transform.Find("Pipe_" + col + "_" + row);
        if (t == null) return;

        var tile = t.GetComponent<PipeTile>();
        if (tile != null) { tile.rotationStep = rot; tile.isLocked = true; }

        var sr = t.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;

        t.GetComponent<BoxCollider2D>()?.gameObject.GetComponent<PipeTile>(); // already locked above
        EditorUtility.SetDirty(t.gameObject);
    }

    private static GameObject PickPrefab(int ptype,
        GameObject pS, GameObject pE, GameObject pT, GameObject pC)
    {
        return ptype switch
        {
            E => pE,
            T => pT,
            X => pC,
            _ => pS,
        };
    }

    private static GameObject GetOrCreatePrefab(string guid, PipeTile.PipeType type, string name)
    {
        string path = PREFAB_FOLDER + "/" + name + ".prefab";

        // Return existing if valid
        if (File.Exists(path))
        {
            var ex = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (ex != null) return ex;
        }

        // Load sprite
        string spritePath = AssetDatabase.GUIDToAssetPath(guid);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(spritePath))
                if (o is Sprite s) { sprite = s; break; }
        }

        // Build prefab
        var go = new GameObject(name);
        var sr = go.AddComponent<SpriteRenderer>();
        if (sprite != null) sr.sprite = sprite;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;

        var pipe = go.AddComponent<PipeTile>();
        pipe.pipeType = type;
        go.AddComponent<GridCell>();

        var saved = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[Level01] Prefab created: " + name);
        return saved;
    }

    private static void DestroyIfExists(string goName)
    {
        var go = GameObject.Find(goName);
        if (go != null) { Object.DestroyImmediate(go); }
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
