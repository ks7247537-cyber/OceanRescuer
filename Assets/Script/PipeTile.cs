using UnityEngine;
using System.Collections;

/// <summary>
/// Represents a single pipe tile in the puzzle.
/// Handles rotation, connection logic and visual feedback.
/// </summary>
public class PipeTile : MonoBehaviour
{
    public enum PipeType { Straight, Elbow, TJunction, Cross }

    [Header("Pipe Settings")]
    public PipeType pipeType = PipeType.Straight;
    // 0 = Up, 1 = Right, 2 = Down, 3 = Left
    [Tooltip("Current rotation step (0‑3). 0 = original orientation.")]
    public int rotationStep = 0;
    public bool isLocked = false;

    [Header("Visuals")]
    [Tooltip("Color when the tile is part of a valid flow.")]
    public Color connectedColor = new Color(0.1f, 1.0f, 1.0f, 1.0f);
    [Tooltip("Default color when not connected.")]
    public Color normalColor = new Color(0.5f, 0.62f, 0.78f, 0.65f);

    private SpriteRenderer _sr;
    private bool _isAnimating = false;
    private Quaternion _targetRotation;
    private int _lastRotateFrame = -1;

    // Base openings for each pipe type (Up, Right, Down, Left) before rotation.
    private static bool[][] _baseOpenings;

    private void Awake()
    {
        // Initialize base openings lazily (once per run).
        if (_baseOpenings == null)
        {
            _baseOpenings = new bool[4][];
            // Straight: Right & Left (Horizontal at rot 0)
            _baseOpenings[(int)PipeType.Straight] = new bool[] { false, true, false, true };
            // Elbow: Down & Left at rot 0
            _baseOpenings[(int)PipeType.Elbow] = new bool[] { false, false, true, true };
            // TJunction: Up, Right, Left
            _baseOpenings[(int)PipeType.TJunction] = new bool[] { true, true, false, true };
            // Cross: all four sides
            _baseOpenings[(int)PipeType.Cross] = new bool[] { true, true, true, true };
        }

        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
        SetConnected(false);
    }

    private bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }
        return false;
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;
        if (IsPointerOverUI()) return;
        if (isLocked || _isAnimating) return;
        if (HasActiveUnrepairedLeak()) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Directly interact with and unlock any obstacle / rune / seal / gate on this tile!
        InteractWithTileObstacle();

        Rotate();
    }

    private void InteractWithTileObstacle()
    {
        var cell = GetComponent<GridCell>();
        if (cell == null) return;
        Vector2Int pos = new Vector2Int(cell.gridX, cell.gridY);

        // 1. GateComponent (BreakableRock, ElectricGate, LockedGate, PressureGate, InkedHazard)
        var gates = FindObjectsOfType<GateComponent>();
        for (int i = 0; i < gates.Length; i++)
        {
            var g = gates[i];
            if (g != null && g.gridPos == pos && !g.isOpen)
            {
                g.OnGateTapped();
            }
        }

        // 2. AncientRune
        var runes = FindObjectsOfType<AncientRune>();
        for (int i = 0; i < runes.Length; i++)
        {
            var r = runes[i];
            if (r != null && r.gridPos == pos && !r.IsEnergized)
            {
                r.SetEnergized(true);
            }
        }

        // 3. AncientSeal
        var seals = FindObjectsOfType<AncientSeal>();
        for (int i = 0; i < seals.Length; i++)
        {
            var s = seals[i];
            if (s != null && s.gridPos == pos && !s.IsUnlocked)
            {
                s.UnlockSeal();
            }
        }

        // 4. PressureLockGate
        var plGates = FindObjectsOfType<PressureLockGate>();
        for (int i = 0; i < plGates.Length; i++)
        {
            var pl = plGates[i];
            if (pl != null && pl.gridPos == pos && !pl.isOpen)
            {
                pl.UnlockGate();
            }
        }

        // 5. TempleDoor
        var doors = FindObjectsOfType<TempleDoor>();
        for (int i = 0; i < doors.Length; i++)
        {
            var d = doors[i];
            if (d != null && d.gridPos == pos && !d.isOpen)
            {
                d.SetDoorOpen(true);
            }
        }

        // 6. StoneBridge
        var bridges = FindObjectsOfType<StoneBridge>();
        for (int i = 0; i < bridges.Length; i++)
        {
            var b = bridges[i];
            if (b != null && b.gridPos == pos && !b.isOpen)
            {
                b.SetBridgeLowered(true);
            }
        }
    }

    private bool HasActiveUnrepairedLeak()
    {
        var cell = GetComponent<GridCell>();
        if (cell == null) return false;
        var leaks = FindObjectsOfType<OxygenLeak>();
        for (int i = 0; i < leaks.Length; i++)
        {
            var lk = leaks[i];
            if (lk != null && !lk.IsRepaired && lk.gridPos.x == cell.gridX && lk.gridPos.y == cell.gridY)
                return true;
        }
        return false;
    }

    public void Rotate()
    {
        if (Time.frameCount == _lastRotateFrame) return;
        _lastRotateFrame = Time.frameCount;

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Record move before changing state. Abort rotation if move was rejected (e.g. out of moves)
        if (MoveManager.Instance != null)
        {
            bool allowed = MoveManager.Instance.RecordMove(this, rotationStep);
            if (!allowed) return;
        }

        rotationStep = (rotationStep + 1) % 4;
        _targetRotation = Quaternion.Euler(0f, 0f, -90f * rotationStep);
        AudioManager.Instance?.PlayPipeRotate();
        StartCoroutine(AnimateRotation());
        GameManager.Instance?.CheckWinCondition();
    }

    /// <summary>
    /// Used by the undo system – sets rotation without recording a new move.
    /// </summary>
    public void ForceRotationStep(int step)
    {
        rotationStep = step % 4;
        _targetRotation = Quaternion.Euler(0f, 0f, -90f * rotationStep);
        StartCoroutine(AnimateRotation());
    }

    private IEnumerator AnimateRotation()
    {
        _isAnimating = true;
        Quaternion start = transform.rotation;
        float elapsed = 0f;
        const float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.rotation = Quaternion.Slerp(start, _targetRotation, t);
            yield return null;
        }
        transform.rotation = _targetRotation;
        _isAnimating = false;
    }

    /// <summary>
    /// Returns true if the tile has an opening in the given world direction.
    /// direction: 0=Up, 1=Right, 2=Down, 3=Left
    /// </summary>
    public bool HasOpening(int direction)
    {
        bool[] baseArr = _baseOpenings[(int)pipeType];
        int adjusted = (direction - rotationStep + 4) % 4;
        return baseArr[adjusted];
    }

    /// <summary>
    /// Called by FlowManager to set visual connection state.
    /// </summary>
    public bool isConnected { get; private set; }

    public void SetConnected(bool connected)
    {
        isConnected = connected;
        if (_sr != null)
        {
            if (isLocked) return; // Keep Source/Sink distinct color
            _sr.color = connected ? connectedColor : normalColor;
        }
    }

    /// <summary>
    /// Visual water pulse effect when water enters this pipe tile.
    /// </summary>
    public void PlayWaterPulse()
    {
        StartCoroutine(WaterPulseRoutine());
    }

    private IEnumerator WaterPulseRoutine()
    {
        Vector3 baseScale = transform.localScale;
        Vector3 peakScale = baseScale * 1.15f;
        float elapsed = 0f;
        float duration = 0.16f;

        if (_sr != null && !isLocked) _sr.color = Color.white;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin(elapsed / duration * Mathf.PI);
            transform.localScale = Vector3.Lerp(baseScale, peakScale, t);
            yield return null;
        }
        transform.localScale = baseScale;
        if (_sr != null && !isLocked) _sr.color = connectedColor;
    }
}
