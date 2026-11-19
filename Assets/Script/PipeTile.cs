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
    public Color connectedColor = new Color(0.3f, 0.9f, 1f);
    [Tooltip("Default color when not connected.")]
    public Color normalColor = Color.white;

    private SpriteRenderer _sr;
    private bool _isAnimating = false;
    private Quaternion _targetRotation;

    // Base openings for each pipe type (Up, Right, Down, Left) before rotation.
    private static bool[][] _baseOpenings;

    private void Awake()
    {
        // Initialize base openings lazily (once per run).
        if (_baseOpenings == null)
        {
            _baseOpenings = new bool[4][];
            // Straight: Up & Down
            _baseOpenings[(int)PipeType.Straight] = new bool[] { true, false, true, false };
            // Elbow: Up & Right
            _baseOpenings[(int)PipeType.Elbow] = new bool[] { true, true, false, false };
            // TJunction: Up, Right, Left
            _baseOpenings[(int)PipeType.TJunction] = new bool[] { true, true, false, true };
            // Cross: all four sides
            _baseOpenings[(int)PipeType.Cross] = new bool[] { true, true, true, true };
        }

        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = gameObject.AddComponent<SpriteRenderer>();
        SetConnected(false);
    }

    private void OnMouseDown()
    {
        if (isLocked || _isAnimating) return;
        Rotate();
    }

    public void Rotate()
    {
        // Record move before changing state.
        MoveManager.Instance?.RecordMove(this, rotationStep);
        rotationStep = (rotationStep + 1) % 4;
        _targetRotation = Quaternion.Euler(0f, 0f, -90f * rotationStep);
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
            _sr.color = connected ? connectedColor : normalColor;
        }
    }
}
