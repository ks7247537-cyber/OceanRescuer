using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Grid-based BFS flood fill — source se pani flow karta hai,
/// interactive valves (ON/OFF, Directional) aur gates (Locked, Pressure)
/// ko evaluate karta hai aur connected pipes ko light up karta hai.
/// </summary>
public class FlowManager : MonoBehaviour
{
    [Header("Grid Reference")]
    [Tooltip("GridManager se grid automatically milegi")]
    public GridManager gridManager;

    [Header("Source & Sink")]
    [Tooltip("Source tile ka grid position (pani yahan se shuru hoga)")]
    public Vector2Int sourceCell = new Vector2Int(0, 0);

    [Tooltip("Sink tile ka grid position (yahan pahunchna hai)")]
    public Vector2Int sinkCell = new Vector2Int(8, 0);

    /// <summary>
    /// Active interactive valves and gates indexed by their grid position.
    /// </summary>
    public Dictionary<Vector2Int, WaterValve> Valves { get; } = new Dictionary<Vector2Int, WaterValve>();
    public Dictionary<Vector2Int, GateComponent> Gates { get; } = new Dictionary<Vector2Int, GateComponent>();
    public Dictionary<Vector2Int, RouteDiversion> Diversions { get; } = new Dictionary<Vector2Int, RouteDiversion>();

    /// <summary>
    /// Ordered list of grid coordinates from sourceCell to sinkCell when connected.
    /// CharacterMover and water animation follow this path.
    /// </summary>
    public List<Vector2Int> CurrentPath { get; private set; } = new List<Vector2Int>();

    private static FlowManager _instance;
    public static FlowManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<FlowManager>();
            return _instance;
        }
        private set => _instance = value;
    }

    private void Awake()
    {
        _instance = this;
    }

    // ─────────────────────────────────────────────────────────────────────────

    private static readonly int[] DX = {  0, 1,  0, -1 };   // Up, Right, Down, Left
    private static readonly int[] DY = {  1, 0, -1,  0 };
    private static readonly int[] Opposite = { 2, 3, 0, 1 }; // opposite direction

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<GridManager>();
    }

    public void RegisterValve(WaterValve v)
    {
        if (v != null) Valves[v.gridPos] = v;
    }

    public void RegisterGate(GateComponent g)
    {
        if (g != null) Gates[g.gridPos] = g;
    }

    public void RegisterDiversion(RouteDiversion d)
    {
        if (d != null) Diversions[d.gridPos] = d;
    }

    public void UnregisterDiversion(RouteDiversion d)
    {
        if (d != null && Diversions.ContainsKey(d.gridPos)) Diversions.Remove(d.gridPos);
    }

    public void ClearValvesAndGates()
    {
        Valves.Clear();
        Gates.Clear();
        Diversions.Clear();
    }

    public WaterValve GetValveAt(Vector2Int pos) => Valves.TryGetValue(pos, out var v) ? v : null;
    public GateComponent GetGateAt(Vector2Int pos) => Gates.TryGetValue(pos, out var g) ? g : null;

    /// <summary>
    /// Called when any valve is tapped. Unlocks linked gates and triggers flow recheck.
    /// </summary>
    public void OnValveStateChanged(WaterValve valve)
    {
        if (valve == null) return;

        foreach (var kvp in Gates)
        {
            var gate = kvp.Value;
            if (gate != null && gate.gateType == GateType.LockedGate)
            {
                if (gate.linkedValvePos == valve.gridPos)
                {
                    gate.SetOpen(valve.isOpen);
                }
            }
        }
    }

    /// <summary>
    /// Source se BFS flood-fill chalao.
    /// Evaluates pipe orientations, ON/OFF valves, directional valves, locked gates, and pressure gates.
    /// Returns: kya sink tak pahuncha?
    /// </summary>
    public bool RunFlow()
    {
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (gridManager == null || gridManager.Grid == null) return false;

        PipeTile[,] grid = gridManager.Grid;
        int cols = gridManager.Cols;
        int rows = gridManager.Rows;

        // Reset pipe connected states
        for (int x = 0; x < cols; x++)
            for (int y = 0; y < rows; y++)
                grid[x, y]?.SetConnected(false);

        // Update locked gates based on current linked valve states
        foreach (var kvp in Gates)
        {
            var gate = kvp.Value;
            if (gate != null && gate.gateType == GateType.LockedGate)
            {
                if (gate.linkedValvePos.x >= 0 && gate.linkedValvePos.y >= 0)
                {
                    var linkedV = GetValveAt(gate.linkedValvePos);
                    gate.SetOpen(linkedV != null && linkedV.isOpen);
                }
            }
        }

        bool reachedSink = false;
        var parent = new Dictionary<Vector2Int, Vector2Int>();

        // Multi-pass BFS: Allows water reaching a Rune/Seal to unlock a linked TempleDoor / StoneBridge
        // and immediately continue flowing through in the same evaluation pass!
        for (int pass = 0; pass < 4; pass++)
        {
            bool newFeatureActivated = false;
            var visited = new bool[cols, rows];
            var queue = new Queue<Vector2Int>();
            parent.Clear();

            queue.Enqueue(sourceCell);
            visited[sourceCell.x, sourceCell.y] = true;
            grid[sourceCell.x, sourceCell.y]?.SetConnected(true);

            while (queue.Count > 0)
            {
                Vector2Int curr = queue.Dequeue();
                PipeTile currTile = grid[curr.x, curr.y];
                if (currTile == null) continue;

                // Check if current tile has a valve blocking outgoing water
                var currValve = GetValveAt(curr);

                for (int dir = 0; dir < 4; dir++)
                {
                    if (!currTile.HasOpening(dir)) continue;

                    // Valve check on current tile
                    if (currValve != null && !currValve.CanPassFlow(dir)) continue;

                    // Route Diversion check on current tile (cannot exit towards restricted hazard)
                    if (Diversions.TryGetValue(curr, out var currDiv) && !currDiv.CanExitFlow(dir)) continue;

                    int nx = curr.x + DX[dir];
                    int ny = curr.y + DY[dir];

                    if (nx < 0 || nx >= cols || ny < 0 || ny >= rows) continue;
                    if (visited[nx, ny]) continue;

                    PipeTile neighbor = grid[nx, ny];
                    if (neighbor == null) continue;

                    if (!neighbor.HasOpening(Opposite[dir])) continue;

                    Vector2Int nextPos = new Vector2Int(nx, ny);

                    // Check valve on neighbor tile (if OnOff, must be open to enter)
                    var neighborValve = GetValveAt(nextPos);
                    if (neighborValve != null && neighborValve.valveType == ValveType.OnOff && !neighborValve.isOpen)
                        continue;

                    // Route Diversion check on neighbor tile (cannot enter from restricted hazard)
                    if (Diversions.TryGetValue(nextPos, out var nextDiv) && !nextDiv.CanEnterFlow(Opposite[dir]))
                        continue;

                    // Check gate on neighbor tile
                    var neighborGate = GetGateAt(nextPos);
                    if (neighborGate != null)
                    {
                        if (neighborGate.gateType == GateType.PressureGate)
                        {
                            int pressure = CalculateUpstreamPressure(parent, curr);
                            if (!neighborGate.EvaluatePressure(pressure))
                            {
                                continue; // Insufficient pressure blocks water!
                            }
                        }
                        else
                        {
                            if (!neighborGate.IsOpen) continue; // Locked, electric gate or obstacle blocks water!
                        }
                    }

                    // Water connects!
                    visited[nx, ny] = true;
                    parent[nextPos] = curr;
                    neighbor.SetConnected(true);
                    queue.Enqueue(nextPos);

                    // Automatically activate Runes, Seals, Relics, and Pressure Locks touched by water!
                    if (ActivateWaterFeatures(nextPos))
                    {
                        newFeatureActivated = true;
                    }

                    if (nx == sinkCell.x && ny == sinkCell.y)
                        reachedSink = true;
                }
            }

            if (reachedSink || !newFeatureActivated)
                break;
        }

        // Reconstruct path if sink was reached
        CurrentPath.Clear();
        if (reachedSink)
        {
            Vector2Int curr = sinkCell;
            while (curr != sourceCell)
            {
                CurrentPath.Add(curr);
                if (!parent.ContainsKey(curr)) break;
                curr = parent[curr];
            }
            CurrentPath.Add(sourceCell);
            CurrentPath.Reverse();
            Debug.Log($"[FlowManager] Path solved! Length: {CurrentPath.Count} tiles.");
        }

        // Update visual connection states of all valves
        foreach (var kvp in Valves)
        {
            bool isOnPath = CurrentPath.Contains(kvp.Key);
            kvp.Value?.SetConnectedVisual(isOnPath);
        }

        return reachedSink;
    }

    private int CalculateUpstreamPressure(Dictionary<Vector2Int, Vector2Int> parentDict, Vector2Int node)
    {
        int pressure = 1;
        Vector2Int curr = node;
        while (curr != sourceCell && parentDict.ContainsKey(curr))
        {
            pressure++;
            curr = parentDict[curr];
        }
        return pressure;
    }

    /// <summary>
    /// Sequential water animation that flows tile-by-tile from source to sink.
    /// </summary>
    public void AnimateWaterFlow(System.Action onComplete = null)
    {
        AudioManager.Instance?.PlayWaterFlow();
        StartCoroutine(WaterFlowRoutine(onComplete));
    }

    private IEnumerator WaterFlowRoutine(System.Action onComplete)
    {
        if (gridManager == null || CurrentPath == null || CurrentPath.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        PipeTile[,] grid = gridManager.Grid;

        for (int x = 0; x < gridManager.Cols; x++)
            for (int y = 0; y < gridManager.Rows; y++)
                grid[x, y]?.SetConnected(false);

        for (int i = 0; i < CurrentPath.Count; i++)
        {
            Vector2Int pos = CurrentPath[i];
            PipeTile tile = grid[pos.x, pos.y];
            if (tile != null)
            {
                tile.SetConnected(true);
                tile.PlayWaterPulse();
            }

            var valve = GetValveAt(pos);
            if (valve != null) valve.SetConnectedVisual(true);

            var gate = GetGateAt(pos);
            if (gate != null) gate.SetOpen(true);

            yield return new WaitForSeconds(0.08f);
        }

        yield return new WaitForSeconds(0.25f);
        onComplete?.Invoke();
    }

    private bool ActivateWaterFeatures(Vector2Int pos)
    {
        bool changed = false;

        // Ancient Runes
        var runes = FindObjectsOfType<AncientRune>();
        for (int i = 0; i < runes.Length; i++)
        {
            var r = runes[i];
            if (r != null && !r.IsEnergized && (r.gridPos == pos || IsAdjacent(r.gridPos, pos)))
            {
                r.SetEnergized(true);
                changed = true;
            }
        }

        // Ancient Seals
        var seals = FindObjectsOfType<AncientSeal>();
        for (int i = 0; i < seals.Length; i++)
        {
            var s = seals[i];
            if (s != null && !s.IsUnlocked && (s.gridPos == pos || IsAdjacent(s.gridPos, pos)))
            {
                s.UnlockSeal();
                changed = true;
            }
        }

        // Energy Relics
        var relics = FindObjectsOfType<EnergyRelic>();
        for (int i = 0; i < relics.Length; i++)
        {
            var e = relics[i];
            if (e != null && !e.IsCharged && (e.gridPos == pos || IsAdjacent(e.gridPos, pos)))
            {
                e.ChargeRelic();
                changed = true;
            }
        }

        return changed;
    }

    private static bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
    }
}
