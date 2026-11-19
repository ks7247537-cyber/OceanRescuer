using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Move counter aur undo system manage karta hai.
/// PipeTile isko call karta hai jab bhi rotate hoti hai.
/// </summary>
public class MoveManager : MonoBehaviour
{
    public static MoveManager Instance { get; private set; }

    [Header("Move Limit")]
    [Tooltip("0 = unlimited moves")]
    public int maxMoves = 0;

    // Current move count
    public int MoveCount { get; private set; } = 0;

    // Undo stack — har entry mein tile reference + old rotationStep
    private readonly Stack<(PipeTile tile, int oldStep)> _undoStack = new();
    private const int MaxUndos = 3;

    // Events
    public event System.Action<int> OnMoveCountChanged;  // move count
    public event System.Action OnUndoStackChanged;
    public event System.Action OnMoveLimitReached;

    // ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────
    // Called by PipeTile before rotating
    // ─────────────────────────────────────────────────────────
    public void RecordMove(PipeTile tile, int oldStep)
    {
        // Move limit check
        if (maxMoves > 0 && MoveCount >= maxMoves)
        {
            OnMoveLimitReached?.Invoke();
            return;
        }

        MoveCount++;
        OnMoveCountChanged?.Invoke(MoveCount);

        // Undo stack mein push karo (max 3 rakhenge)
        _undoStack.Push((tile, oldStep));
        if (_undoStack.Count > MaxUndos)
        {
            // Stack rebuild karke oldest remove karo
            var temp = new Stack<(PipeTile, int)>();
            int keep = MaxUndos;
            foreach (var entry in _undoStack)
            {
                if (keep-- > 0) temp.Push(entry);
            }
            _undoStack.Clear();
            foreach (var entry in temp) _undoStack.Push(entry);
        }

        OnUndoStackChanged?.Invoke();
    }

    // ─────────────────────────────────────────────────────────
    // Undo last move
    // ─────────────────────────────────────────────────────────
    // Undo helpers (auto‑implemented)
    public bool CanUndo { get { return _undoStack.Count > 0; } }
    public int UndosLeft { get { return _undoStack.Count; } }

    public void Undo()
    {
        if (!CanUndo) return;

        var (tile, oldStep) = _undoStack.Pop();
        if (tile == null) return;

        // Tile ka rotation wapas set karo (without recording another move)
        tile.ForceRotationStep(oldStep);

        // Move count wapas ghata do
        if (MoveCount > 0) MoveCount--;
        OnMoveCountChanged?.Invoke(MoveCount);
        OnUndoStackChanged?.Invoke();

        // Flow recalculate karo
        GameManager.Instance?.CheckWinCondition();
    }

    public void Reset()
    {
        MoveCount = 0;
        _undoStack.Clear();
        OnMoveCountChanged?.Invoke(MoveCount);
        OnUndoStackChanged?.Invoke();
    }
}
