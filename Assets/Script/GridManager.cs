using UnityEngine;

/// <summary>
/// Grid setup karta hai — tiles ko spawn karta hai ya
/// Scene mein placed tiles ko grid array mein register karta hai.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    public int Cols = 5;
    public int Rows = 5;

    [Header("Tile Size (units)")]
    public float TileSize = 0.6f; // smaller tiles for better fit on screen

    [Header("Tile Prefabs")]
    public GameObject straightPrefab;
    public GameObject elbowPrefab;
    public GameObject tJunctionPrefab;
    public GameObject crossPrefab;

    // Public grid array — FlowManager issi se padhta hai
    public PipeTile[,] Grid { get; private set; }

    [Tooltip("True = scene mein already tiles placed hain (Editor Setup ne kiya). False = runtime generate karo.")]
    public bool useScenePlacedTiles = false;

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        Grid = new PipeTile[Cols, Rows];
    }

    private void Start()
    {
        if (useScenePlacedTiles)
        {
            // Editor Setup ne scene mein tiles place ki hain — bas register karo
            RegisterSceneTiles();
        }
        else if (straightPrefab != null)
        {
            // Runtime mein generate karo
            GenerateGrid();
        }
        else
        {
            RegisterSceneTiles();
        }
    }

    // ─────────────────────────────────────────────────────────
    // Option A: Procedural generation (testing ke liye)
    // ─────────────────────────────────────────────────────────

    private void GenerateGrid()
    {
        // Grid ko center mein rakhne ke liye offset calculate karo
        float offsetX = -(Cols - 1) * TileSize * 0.5f;
        float offsetY = -(Rows - 1) * TileSize * 0.5f;

        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                // Simple pattern: corners = elbow, edges = straight, rest = cross
                GameObject prefab = ChoosePrefab(x, y);

                Vector3 worldPos = new Vector3(
                    offsetX + x * TileSize,
                    offsetY + y * TileSize,
                    0f
                );

                GameObject go = Instantiate(prefab, worldPos, Quaternion.identity, transform);
                go.name = $"Pipe_{x}_{y}";

                PipeTile tile = go.GetComponent<PipeTile>();
                if (tile != null)
                {
                    tile.rotationStep = Random.Range(0, 4); // random rotation
                    Grid[x, y] = tile;
                }
            }
        }
    }

    private GameObject ChoosePrefab(int x, int y)
    {
        bool edgeX = (x == 0 || x == Cols - 1);
        bool edgeY = (y == 0 || y == Rows - 1);

        if (edgeX && edgeY) return elbowPrefab ?? straightPrefab;
        if (edgeX || edgeY) return straightPrefab;
        return crossPrefab ?? straightPrefab;
    }

    // ─────────────────────────────────────────────────────────
    // Option B: Scene-placed tiles ko register karo
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Scene mein placed PipeTile objects ko unke
    /// GridCell component ke hisaab se register karta hai.
    /// </summary>
    public void RegisterSceneTiles()
    {
        PipeTile[] allTiles = FindObjectsOfType<PipeTile>();
        foreach (PipeTile tile in allTiles)
        {
            GridCell cell = tile.GetComponent<GridCell>();
            if (cell == null)
            {
                Debug.LogWarning($"{tile.name} mein GridCell component nahi hai!", tile);
                continue;
            }

            int gx = cell.gridX;
            int gy = cell.gridY;

            if (gx >= 0 && gx < Cols && gy >= 0 && gy < Rows)
                Grid[gx, gy] = tile;
            else
                Debug.LogWarning($"{tile.name} grid ke bahar hai: ({gx},{gy})", tile);
        }
    }
}
