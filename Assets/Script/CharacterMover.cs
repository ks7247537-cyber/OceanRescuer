using UnityEngine;
using System.Collections.Generic;

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
    public float moveSpeed = 3.5f;

    [Tooltip("Character kitna smooth rotate kare (degrees per second)")]
    public float rotationSpeed = 720f;

    [Header("References")]
    public FlowManager flowManager;
    public GridManager  gridManager;

    [Header("Start Position (Grid Cell)")]
    public Vector2Int startCell = new Vector2Int(0, 0);

    [Header("Creature Companion")]
    public CreatureType creatureType = CreatureType.Clownfish;

    public void SetCreature(CreatureType creature = CreatureType.Clownfish)
    {
        // Rescued character swimming inside the pipe puzzle is always Clownfish
        creatureType = CreatureType.Clownfish;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Sprite[] subs = Resources.LoadAll<Sprite>("Characters/Clownfish");
            if (subs != null && subs.Length > 0) sr.sprite = subs[0];
            else
            {
                Sprite s = Resources.Load<Sprite>("Characters/Clownfish");
                if (s != null) sr.sprite = s;
            }
        }

        transform.localScale = new Vector3(0.8f, 0.8f, 1f);
    }

    [Header("Idle Bob")]
    [Tooltip("Character thoda upar neecha bob kare")]
    public bool idleBob = true;
    public float bobSpeed  = 1.5f;
    public float bobAmount = 0.08f;

    /// <summary>
    /// Event fired when character arrives at the sink.
    /// Used by GameManager to show the Win popup cleanly after the journey.
    /// </summary>
    public System.Action OnDestinationReached;

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
        SetCreature(CreatureType.Clownfish);
        if (gridManager != null)
        {
            transform.position = GridToWorld(_currentCell);
        }
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

    private List<Vector2Int> _path;
    private int _pathIndex = 0;

    // ─────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────

    /// <summary>
    /// Starts the clownfish swimming journey along the solved water pipeline.
    /// </summary>
    public void StartJourney()
    {
        if (_state == State.Moving) return;

        if (flowManager == null) flowManager = FindObjectOfType<FlowManager>();
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();

        _path = flowManager != null ? flowManager.CurrentPath : null;

        if (_path == null || _path.Count == 0)
        {
            Debug.LogWarning("[CharacterMover] No connected water path found for character journey.");
            OnDestinationReached?.Invoke();
            return;
        }

        _pathIndex = 0;
        _currentCell = _path[0];
        _targetWorldPos = GridToWorld(_currentCell);
        transform.position = _targetWorldPos;
        _basePos = _targetWorldPos;
        _state = State.Moving;

        MoveToNextWaypoint();
    }

    private void MoveToNextWaypoint()
    {
        _pathIndex++;
        if (_pathIndex < _path.Count)
        {
            _currentCell = _path[_pathIndex];
            _targetWorldPos = GridToWorld(_currentCell);
            _basePos = _targetWorldPos;
        }
        else
        {
            _state = State.Done;
            Debug.Log("[CharacterMover] 🎉 Clownfish reached destination sink!");
            OnDestinationReached?.Invoke();
        }
    }

    // ─────────────────────────────────────────────────────
    // Movement
    // ─────────────────────────────────────────────────────

    private void MoveTowardsTarget()
    {
        Vector3 dir = (_targetWorldPos - transform.position);
        dir.z = 0f;
        if (dir.magnitude > 0.04f)
        {
            // Rotation (face direction of movement)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            // Move (maintain z = -1 so fish stays on top of pipes)
            Vector3 newPos = Vector3.MoveTowards(
                transform.position, _targetWorldPos, moveSpeed * Time.deltaTime);
            newPos.z = -1f;
            transform.position = newPos;
        }
        else
        {
            // Reached current waypoint — proceed to next
            transform.position = new Vector3(_targetWorldPos.x, _targetWorldPos.y, -1f);
            MoveToNextWaypoint();
        }
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
