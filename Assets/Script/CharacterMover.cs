using UnityEngine;

/// <summary>
/// Fish/Turtle character ko pipe path ke saath move karta hai.
/// GameManager Win call karta hai toh character animate hota hai.
/// 
/// Setup:
/// 1. Character sprite ko Scene mein rakho
/// 2. Yeh script attach karo
/// 3. Inspector mein Source tile ki position set karo
/// </summary>
public class CharacterMover : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed (units per second)")]
    public float moveSpeed = 2f;

    [Tooltip("Character kitna smooth rotate kare (degrees per second)")]
    public float rotationSpeed = 180f;

    [Header("References")]
    public FlowManager flowManager;
    public GridManager  gridManager;

    [Header("Start Position (Grid Cell)")]
    public Vector2Int startCell = new Vector2Int(0, 0);

    [Header("Idle Bob")]
    [Tooltip("Character thoda upar neecha bob kare")]
    public bool idleBob = true;
    public float bobSpeed  = 1.5f;
    public float bobAmount = 0.08f;

    // ── State ─────────────────────────────────────────────
    private enum State { Idle, Moving, Done }
    private State _state = State.Idle;

    private Vector2Int _currentCell;
    private Vector3    _targetWorldPos;
    private Vector3    _basePos;
    private float      _bobTimer;

    // Up=0, Right=1, Down=2, Left=3
    private static readonly int[] DX = {  0, 1,  0, -1 };
    private static readonly int[] DY = {  1, 0, -1,  0 };
    private static readonly int[] Opposite = { 2, 3, 0, 1 };

    // ─────────────────────────────────────────────────────
    private void Start()
    {
        if (flowManager == null) flowManager = FindObjectOfType<FlowManager>();
        if (gridManager  == null) gridManager  = FindObjectOfType<GridManager>();

        _currentCell = startCell;
        _basePos     = transform.position;
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Idle:
                DoIdleBob();
                break;

            case State.Moving:
                MoveTowardsTarget();
                break;

            case State.Done:
                DoIdleBob(); // celebrate!
                break;
        }
    }

    // ─────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// GameManager ya koi bhi call kar sakta hai —
    /// character connected path follow karta hai.
    /// </summary>
    public void StartJourney()
    {
        if (_state == State.Moving) return;

        _currentCell = startCell;
        _state       = State.Moving;

        _targetWorldPos = GridToWorld(_currentCell);
        transform.position = _targetWorldPos;
        _basePos = _targetWorldPos;

        // Re‑calculate flow so the connected flags are current
        flowManager?.RunFlow();
        TryMoveNext();
    }

    // ─────────────────────────────────────────────────────
    // Movement
    // ─────────────────────────────────────────────────────

    private void MoveTowardsTarget()
    {
        // Smoothly move karo target tak
        Vector3 dir = (_targetWorldPos - transform.position);
        if (dir.magnitude > 0.01f)
        {
            // Rotation (face direction of movement)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Move
            transform.position = Vector3.MoveTowards(
                transform.position, _targetWorldPos, moveSpeed * Time.deltaTime);
        }
        else
        {
            // Reached target — next cell
            transform.position = _targetWorldPos;
            _currentCell = WorldToGrid(transform.position);
            TryMoveNext();
        }
    }

    private void TryMoveNext()
    {
        PipeTile[,] grid = gridManager.Grid;
        if (grid == null) return;

        PipeTile current = GetTile(_currentCell);
        if (current == null) { _state = State.Idle; return; }

        // Check karo connected path mein koi adjacent connected tile hai?
        for (int dir = 0; dir < 4; dir++)
        {
            if (!current.HasOpening(dir)) continue;

            int nx = _currentCell.x + DX[dir];
            int ny = _currentCell.y + DY[dir];

            if (!InBounds(nx, ny)) continue;

            PipeTile neighbor = grid[nx, ny];
            if (neighbor == null || !neighbor.isConnected) continue;

            // Neighbor ki opposite direction mein opening hai?
            if (!neighbor.HasOpening(Opposite[dir])) continue;

            // Move!
            _currentCell    = new Vector2Int(nx, ny);
            _targetWorldPos = GridToWorld(_currentCell);
            _basePos        = _targetWorldPos;

            // Sink tak pahuncha?
            if (_currentCell == flowManager.sinkCell)
            {
                _state = State.Done;
                Debug.Log("[Character] Destination reached!");
                return;
            }

            return; // ek step kafi hai, baaki Update() mein hoga
        }

        // Koi connected neighbor nahi — wait
        _state = State.Idle;
    }

    // ─────────────────────────────────────────────────────
    // Idle animation
    // ─────────────────────────────────────────────────────
    private void DoIdleBob()
    {
        if (!idleBob) return;
        _bobTimer += Time.deltaTime * bobSpeed;
        Vector3 p = _basePos;
        p.y += Mathf.Sin(_bobTimer) * bobAmount;
        transform.position = p;
    }

    // ─────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────
    private Vector3 GridToWorld(Vector2Int cell)
    {
        float ox = -(gridManager.Cols - 1) * gridManager.TileSize * 0.5f;
        float oy = -(gridManager.Rows - 1) * gridManager.TileSize * 0.5f;
        return new Vector3(ox + cell.x * gridManager.TileSize,
                           oy + cell.y * gridManager.TileSize, -1f);
    }

    private Vector2Int WorldToGrid(Vector3 world)
    {
        float ox = -(gridManager.Cols - 1) * gridManager.TileSize * 0.5f;
        float oy = -(gridManager.Rows - 1) * gridManager.TileSize * 0.5f;
        int gx = Mathf.RoundToInt((world.x - ox) / gridManager.TileSize);
        int gy = Mathf.RoundToInt((world.y - oy) / gridManager.TileSize);
        return new Vector2Int(gx, gy);
    }

    private PipeTile GetTile(Vector2Int cell)
    {
        if (!InBounds(cell.x, cell.y)) return null;
        return gridManager.Grid[cell.x, cell.y];
    }

    private bool InBounds(int x, int y)
        => x >= 0 && x < gridManager.Cols && y >= 0 && y < gridManager.Rows;
}
