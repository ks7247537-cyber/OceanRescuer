using UnityEngine;

/// <summary>
/// Runtime bootstrapper for GameplayScene.
/// Automatically loads pipe prefabs and builds the 9x5 snake grid
/// when the scene loads — works even if pipe prefabs are not wired in Inspector.
/// Attach to GameManager or any persistent GameObject in GameplayScene.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameplayBootstrapper : MonoBehaviour
{
    [Header("Grid Config (matches Level01Setup)")]
    public int   cols     = 9;
    public int   rows     = 5;
    public float tileSize = 1.1f;

    // Per-type scale: TILE_SIZE / sprite-world-width at PPU=1024
    private static readonly float[] PIPE_SCALE = { 0.528f, 0.815f, 0.746f, 0.887f };

    // Layout: 0=Straight,1=Elbow,2=TJunction,3=Cross,-1=Empty
    // row 0 = bottom, row 4 = top
    private static readonly int[,] LAYOUT = new int[5, 9]
    {
        // row 0  col 0..8
        { 1, 0, 0, 2, 0, 0, 2, 0, 1 },   // bottom: E-S-S-T-S-S-T-S-E
        {-1,-1,-1,-1,-1,-1,-1,-1, 0 },   // right vertical connector x=8
        { 1, 0, 0, 2, 0, 0, 2, 0, 1 },   // middle row
        { 0,-1,-1,-1,-1,-1,-1,-1,-1 },   // left vertical connector x=0
        { 1, 0, 0, 2, 0, 0, 2, 0, 1 },   // top row
    };

    // Rotations per cell
    private static readonly int[,] ROTATIONS = new int[5, 9]
    {
        { 0, 1, 1, 2, 1, 1, 2, 1, 3 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 1, 1, 2, 1, 1, 2, 1, 2 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 1, 1, 1, 2, 1, 1, 2, 1, 2 },
    };

    private GridManager _grid;

    private void Awake()
    {
        _grid = GetComponent<GridManager>();
        if (_grid == null) _grid = FindObjectOfType<GridManager>();
        if (_grid == null) return;

        // Only bootstrap if no tiles placed yet
        bool tilesExist = FindObjectOfType<PipeTile>() != null;
        if (tilesExist) return;

        // Try load prefabs from Resources first, then from project assets
        var pS = LoadPrefab("Pipe_Straight");
        var pE = LoadPrefab("Pipe_Elbow");
        var pT = LoadPrefab("Pipe_T");
        var pX = LoadPrefab("Pipe_Cross");

        if (pS == null || pE == null || pT == null || pX == null)
        {
            Debug.LogWarning("[GameplayBootstrapper] One or more pipe prefabs not found. " +
                             "Run OceanRescuer > Build Level 01 from the Editor menu, " +
                             "or assign prefabs to GridManager in Inspector.");
            return;
        }

        BuildGrid(pS, pE, pT, pX);
    }

    private void BuildGrid(GameObject pS, GameObject pE, GameObject pT, GameObject pX)
    {
        _grid.Cols             = cols;
        _grid.Rows             = rows;
        _grid.TileSize         = tileSize;
        _grid.useScenePlacedTiles = true;

        // Wire prefabs
        _grid.straightPrefab  = pS;
        _grid.elbowPrefab     = pE;
        _grid.tJunctionPrefab = pT;
        _grid.crossPrefab     = pX;

        float ox = -(cols - 1) * tileSize * 0.5f;
        float oy = -(rows - 1) * tileSize * 0.5f;

        var container = new GameObject("PipeGrid_Bootstrap");

        for (int col = 0; col < cols; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                int ptype = LAYOUT[row, col];
                if (ptype < 0) continue;

                GameObject prefab = ptype == 0 ? pS : ptype == 1 ? pE : ptype == 2 ? pT : pX;
                var go = Instantiate(prefab, container.transform);
                go.name = $"Pipe_{col}_{row}";

                float sc = PIPE_SCALE[ptype];
                go.transform.position   = new Vector3(ox + col * tileSize, oy + row * tileSize, 0f);
                go.transform.localScale = new Vector3(sc, sc, 1f);

                int rot = ROTATIONS[row, col];
                go.transform.rotation = Quaternion.Euler(0f, 0f, -90f * rot);

                var tile = go.GetComponent<PipeTile>();
                var cell = go.GetComponent<GridCell>();
                if (tile != null) { tile.pipeType = (PipeTile.PipeType)ptype; tile.rotationStep = rot; }
                if (cell != null) { cell.gridX = col; cell.gridY = row; }

                // Lock and color Source (0,0) and Sink (8,0)
                bool isSource = col == 0 && row == 0;
                bool isSink   = col == 8 && row == 0;
                if (isSource || isSink)
                {
                    if (tile != null) tile.isLocked = true;
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = isSource
                        ? new Color(0.2f, 1f, 0.3f)
                        : new Color(1f, 0.3f, 0.3f);
                }
            }
        }

        Debug.Log("[GameplayBootstrapper] Grid built at runtime.");
    }

    private static GameObject LoadPrefab(string name)
    {
        // Try Resources folder first
        var r = Resources.Load<GameObject>("Prefabs/" + name);
        if (r != null) return r;

        // Fall back: search all loaded assets (Editor only via AssetDatabase — not available at runtime)
        // At runtime we rely on Resources or Inspector assignment
        return null;
    }
}
