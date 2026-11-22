using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Move counter aur undo system manage karta hai.
/// PipeTile, WaterValve, aur Gate/Creature Abilities isko call karte hain.
/// </summary>
[DefaultExecutionOrder(-100)]
public class MoveManager : MonoBehaviour
{
    public struct MoveAction
    {
        public PipeTile tile;
        public int oldRotationStep;
        public WaterValve valve;
        public bool oldValveOpen;
        public int oldValveDir;
        public GateComponent gate;
        public bool oldGateOpen;
    }

    private static MoveManager _instance;
    public static MoveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MoveManager>();
                if (_instance == null)
                {
                    var go = new GameObject("MoveManager");
                    _instance = go.AddComponent<MoveManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Move Limit")]
    [Tooltip("0 = unlimited moves. Set from LevelData at start of level.")]
    public int maxMoves = 0;

    // Current move count
    public int MoveCount { get; private set; } = 0;

    // Remaining moves until fail
    public int RemainingMoves => maxMoves > 0 ? Mathf.Max(0, maxMoves - MoveCount) : MoveCount;

    // Undo stack — har entry mein tile, valve ya gate action
    private readonly Stack<MoveAction> _undoStack = new();
    private const int MaxUndos = 3;

    // Events
    public event System.Action<int> OnMoveCountChanged;  // move count
    public event System.Action OnUndoStackChanged;
    public event System.Action OnMoveLimitReached;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        ResetForLevel(SaveManager.GetSelectedLevel());
    }

    private void Start()
    {
        OnMoveCountChanged?.Invoke(MoveCount);
        OnUndoStackChanged?.Invoke();
    }

    public void ResetForLevel(int levelIndex)
    {
        MoveCount = 0;
        _undoStack.Clear();

        LevelData ld = Resources.Load<LevelData>($"Levels/Level_{levelIndex + 1:D2}");
        if (ld != null && ld.maxMoves > 0)
        {
            maxMoves = ld.maxMoves;
        }
        else
        {
            maxMoves = GameplayBootstrapper.GetDefaultMaxMoves(levelIndex);
        }

        OnMoveCountChanged?.Invoke(MoveCount);
        OnUndoStackChanged?.Invoke();
    }

    public void SetMaxMoves(int moves)
    {
        maxMoves = moves;
        OnMoveCountChanged?.Invoke(MoveCount);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Called by PipeTile before rotating
    // ─────────────────────────────────────────────────────────────────────────
    public bool RecordMove(PipeTile tile, int oldStep)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return false;

        if (maxMoves > 0 && MoveCount >= maxMoves)
        {
            OnMoveLimitReached?.Invoke();
            return false;
        }

        MoveCount++;
        OnMoveCountChanged?.Invoke(MoveCount);

        _undoStack.Push(new MoveAction
        {
            tile = tile,
            oldRotationStep = oldStep
        });

        TrimUndoStack();
        OnUndoStackChanged?.Invoke();

        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Called by WaterValve when toggled/rotated
    // ─────────────────────────────────────────────────────────────────────────
    public bool RecordValveToggle(WaterValve valve, bool oldOpen, int oldDir)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return false;

        if (maxMoves > 0 && MoveCount >= maxMoves)
        {
            OnMoveLimitReached?.Invoke();
            return false;
        }

        MoveCount++;
        OnMoveCountChanged?.Invoke(MoveCount);

        _undoStack.Push(new MoveAction
        {
            valve = valve,
            oldValveOpen = oldOpen,
            oldValveDir = oldDir
        });

        TrimUndoStack();
        OnUndoStackChanged?.Invoke();

        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Called when a creature ability clears an obstacle/gate
    // ─────────────────────────────────────────────────────────────────────────
    public bool RecordGateAbility(GateComponent gate, bool oldOpen)
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            return false;

        _undoStack.Push(new MoveAction
        {
            gate = gate,
            oldGateOpen = oldOpen
        });

        TrimUndoStack();
        OnUndoStackChanged?.Invoke();
        return true;
    }

    private void TrimUndoStack()
    {
        if (_undoStack.Count > MaxUndos)
        {
            var temp = new Stack<MoveAction>();
            int keep = MaxUndos;
            foreach (var entry in _undoStack)
            {
                if (keep-- > 0) temp.Push(entry);
            }
            _undoStack.Clear();
            foreach (var entry in temp) _undoStack.Push(entry);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Undo last move / action
    // ─────────────────────────────────────────────────────────────────────────
    public bool CanUndo => _undoStack.Count > 0 && (GameManager.Instance == null || !GameManager.Instance.IsGameOver);
    public int UndosLeft => _undoStack.Count;

    public void Undo()
    {
        if (!CanUndo) return;

        var action = _undoStack.Pop();
        if (action.gate != null)
        {
            action.gate.SetOpen(action.oldGateOpen);
            CreatureAbilityController.Instance?.RestoreAbilityCharge();
        }
        else if (action.valve != null)
        {
            action.valve.ForceState(action.oldValveOpen, action.oldValveDir);
        }
        else if (action.tile != null)
        {
            action.tile.ForceRotationStep(action.oldRotationStep);
        }

        if (MoveCount > 0 && (action.tile != null || action.valve != null))
        {
            MoveCount--;
        }

        OnMoveCountChanged?.Invoke(MoveCount);
        OnUndoStackChanged?.Invoke();

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
