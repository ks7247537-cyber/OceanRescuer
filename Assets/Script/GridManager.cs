using UnityEngine;

/// <summary>
/// Grid array maintain karta hai aur FlowManager/CharacterMover ko data deta hai.
///
/// Grid generation strategy (sirf ek active hogi):
///   • GameplayBootstrapper [DefaultExecutionOrder(-100)] Awake() mein grid banata hai
///     aur useScenePlacedTiles = true set karta hai.
///   • Yeh Start() dekhta hai: useScenePlacedTiles=true → sirf RegisterSceneTiles() call karta hai.
///   • Agar kisi reason se Bootstrapper na chale, toh fallback GenerateGrid() available hai.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    public int Cols = 9;
    public int Rows = 5;

    [Header("Tile Size (units)")]
    public float TileSize = 1.0f;

    [Header("Tile Prefabs (Bootstrapper runtime mein assign karta hai)")]
    public GameObject straightPrefab;
    public GameObject elbowPrefab;
    public GameObject tJunctionPrefab;
    public GameObject crossPrefab;

    // Public grid array — FlowManager issi se padhta hai
    public PipeTile[,] Grid { get; private set; }

    /// <summary>
    /// GameplayBootstrapper Awake() mein isse true karta hai.
    /// True hone par Start() sirf RegisterSceneTiles() call karta hai — GenerateGrid() nahi.
    /// </summary>
    [Tooltip("GameplayBootstrapper automatically set karta hai. Manually mat change karo.")]
    public bool useScenePlacedTiles = false;

    // ─────────────────────────────────────────────────────────

    private void Awake()
    {
        // Grid array allocate karo — Bootstrapper Awake() ke baad bhi sahi rahega
        // kyunki Bootstrapper ka ExecutionOrder -100 hai (pehle chalta hai).
        Grid = new PipeTile[Cols, Rows];
    }

    /// <summary>
    /// GameplayBootstrapper Awake() mein call karta hai jab Cols/Rows update hote hain.
    /// Purana array discard karke naya allocate karta hai.
    /// </summary>
    public void ResizeGrid(int newCols, int newRows)
    {
        Cols = newCols;
        Rows = newRows;
        Grid = new PipeTile[Cols, Rows];
    }

    private void Start()
    {
        if (useScenePlacedTiles)
        {
            // GameplayBootstrapper ne runtime mein pipes spawn kiye hain.
            // Sirf unhe register karo — duplicate generation mat karo.
            RegisterSceneTiles();
        }
        else if (straightPrefab != null)
        {
            // Fallback: Bootstrapper nahi chala (disabled tha).
            // GridManager khud generate karta hai.
            Debug.LogWarning("[GridManager] GameplayBootstrapper did not run — using fallback GenerateGrid().");
            GenerateGrid();
        }
        else
        {
            // Koi prefab nahi, koi bootstrapper nahi — scene tiles dhundo
            RegisterSceneTiles();
        }
    }

    // ─────────────────────────────────────────────────────────
    // Fallback: Procedural generation
    // Only runs if GameplayBootstrapper is disabled.
    // ─────────────────────────────────────────────────────────

    private void GenerateGrid()
    {
        float offsetX = -(Cols - 1) * TileSize * 0.5f;
        float offsetY = -(Rows - 1) * TileSize * 0.5f;

        for (int x = 0; x < Cols; x++)
        {
            for (int y = 0; y < Rows; y++)
            {
                GameObject prefab = ChoosePrefab(x, y);
                Vector3 worldPos = new Vector3(
                    offsetX + x * TileSize,
                    offsetY + y * TileSize,
                    0f
                );

                GameObject go = Instantiate(prefab, worldPos, Quaternion.identity, transform);
                go.name = $"Pipe_{x}_{y}";

                // GridCell set karo taaki RegisterSceneTiles bhi kaam kare
                GridCell cell = go.GetComponent<GridCell>();
                if (cell == null) cell = go.AddComponent<GridCell>();
                cell.gridX = x;
                cell.gridY = y;

                PipeTile tile = go.GetComponent<PipeTile>();
                if (tile != null)
                {
                    tile.rotationStep = 0; // deterministic — random avoid karo
                    Grid[x, y] = tile;
                }
            }
        }

        Debug.Log("[GridManager] Fallback GenerateGrid() complete.");
    }

    private GameObject ChoosePrefab(int x, int y)
    {
        bool edgeX = (x == 0 || x == Cols - 1);
        bool edgeY = (y == 0 || y == Rows - 1);

        if (edgeX && edgeY) return elbowPrefab  ?? straightPrefab;
        if (edgeX || edgeY) return straightPrefab;
        return crossPrefab ?? straightPrefab;
    }

    // ─────────────────────────────────────────────────────────
    // Register scene / runtime-spawned tiles into Grid array
    // ─────────────────────────────────────────────────────────

    /// <summary>
    /// Runtime mein spawn hue ya scene mein placed PipeTile objects ko
    /// GridCell.gridX / gridY ke hisaab se Grid[,] mein register karta hai.
    /// GameplayBootstrapper ke baad call hota hai.
    /// </summary>
    public void RegisterSceneTiles()
    {
        // Grid array ko naye Cols/Rows ke saath resize karo agar Bootstrapper ne change kiya ho
        if (Grid == null || Grid.GetLength(0) != Cols || Grid.GetLength(1) != Rows)
            Grid = new PipeTile[Cols, Rows];

        PipeTile[] allTiles = FindObjectsOfType<PipeTile>();
        int registered = 0;
        foreach (PipeTile tile in allTiles)
        {
            GridCell cell = tile.GetComponent<GridCell>();
            if (cell == null)
            {
                Debug.LogWarning($"[GridManager] {tile.name} mein GridCell nahi hai — skip.", tile);
                continue;
            }

            int gx = cell.gridX;
            int gy = cell.gridY;

            if (gx >= 0 && gx < Cols && gy >= 0 && gy < Rows)
            {
                Grid[gx, gy] = tile;
                registered++;
            }
            else
            {
                Debug.LogWarning($"[GridManager] {tile.name} out of bounds: ({gx},{gy})", tile);
            }
        }

        Debug.Log($"[GridManager] RegisterSceneTiles: {registered}/{allTiles.Length} tiles registered into {Cols}x{Rows} grid.");
        }
}
