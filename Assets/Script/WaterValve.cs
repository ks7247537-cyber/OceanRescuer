using UnityEngine;
using System.Collections;

/// <summary>
/// Interactive Water Valve for Ocean Rescuers.
/// Supports:
/// 1. ON/OFF Water Valve (toggles water flow on/off with visual wheel rotation & color state).
/// 2. Directional Valve (one-way check valve controlling permitted water flow direction).
/// Supports mouse click and Android touch input.
/// </summary>
public class WaterValve : MonoBehaviour
{
    [Header("Valve Configuration")]
    public ValveType valveType = ValveType.OnOff;
    public Vector2Int gridPos;
    public bool isOpen = false;

    [Tooltip("Allowed flow direction for Directional Valve: 0=Up, 1=Right, 2=Down, 3=Left")]
    public int allowedDirection = 1;

    [Header("Visual Colors")]
    public Color closedColor = new Color(1f, 0.35f, 0.35f, 1f);
    public Color openColor   = new Color(0.2f, 1f, 0.5f, 1f);
    public Color activeFlowColor = new Color(0.1f, 1f, 1f, 1f);

    private SpriteRenderer _wheelSr;
    private SpriteRenderer _arrowSr;
    private bool _isAnimating = false;
    private Quaternion _targetRotation;
    private int _lastInteractFrame = -1;

    public static readonly Vector3[] DIR_VECTORS = {
        Vector3.up, Vector3.right, Vector3.down, Vector3.left
    };

    private void Awake()
    {
        SetupComponents();
    }

    private void Start()
    {
        UpdateVisuals(immediate: true);
    }

    private void SetupComponents()
    {
        // Ensure Collider2D for touch / click
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var circle = gameObject.AddComponent<CircleCollider2D>();
            circle.radius = 0.45f;
        }

        // Wheel renderer
        _wheelSr = GetComponent<SpriteRenderer>();
        if (_wheelSr == null) _wheelSr = gameObject.AddComponent<SpriteRenderer>();
        if (_wheelSr.sprite == null)
        {
            _wheelSr.sprite = Resources.Load<Sprite>("Sprites/red_valve");
        }
        _wheelSr.sortingOrder = 5; // Above pipe tile

        // Child arrow for Directional Valve
        var arrowObj = transform.Find("DirectionArrow");
        if (arrowObj == null && valveType == ValveType.Directional)
        {
            var go = new GameObject("DirectionArrow");
            go.transform.SetParent(transform, false);
            _arrowSr = go.AddComponent<SpriteRenderer>();
            _arrowSr.sprite = Resources.Load<Sprite>("Sprites/red_valve");
            _arrowSr.color = Color.cyan;
            _arrowSr.sortingOrder = 6;
            go.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
        }
        else if (arrowObj != null)
        {
            _arrowSr = arrowObj.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        // Android primary touch input safety
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (Camera.main != null)
                {
                    Vector2 worldPoint = Camera.main.ScreenToWorldPoint(touch.position);
                    var hit = Physics2D.OverlapPoint(worldPoint);
                    if (hit != null && hit.gameObject == gameObject)
                    {
                        Interact();
                    }
                }
            }
        }
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
        Interact();
    }

    /// <summary>
    /// Handles user interaction (toggle On/Off or rotate direction).
    /// </summary>
    public void Interact()
    {
        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        if (_isAnimating) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        bool prevOpen = isOpen;
        int prevDir = allowedDirection;

        // Record move via MoveManager
        if (MoveManager.Instance != null)
        {
            bool allowed = MoveManager.Instance.RecordValveToggle(this, prevOpen, prevDir);
            if (!allowed) return;
        }

        if (valveType == ValveType.OnOff)
        {
            isOpen = !isOpen;
        }
        else if (valveType == ValveType.Directional)
        {
            allowedDirection = (allowedDirection + 1) % 4;
        }

        UpdateVisuals(immediate: false);

        // Notify FlowManager to check locked gates and re-verify water flow
        var flow = FindObjectOfType<FlowManager>();
        if (flow != null)
        {
            flow.OnValveStateChanged(this);
        }

        GameManager.Instance?.CheckWinCondition();
    }

    /// <summary>
    /// Checks whether water can pass through this valve in the given direction.
    /// direction: 0=Up, 1=Right, 2=Down, 3=Left
    /// </summary>
    public bool CanPassFlow(int flowDir)
    {
        if (valveType == ValveType.OnOff)
        {
            return isOpen;
        }
        else // Directional
        {
            return flowDir == allowedDirection;
        }
    }

    /// <summary>
    /// Reverts state without recording an undo move (used by Undo system).
    /// </summary>
    public void ForceState(bool open, int dir)
    {
        isOpen = open;
        allowedDirection = dir;
        UpdateVisuals(immediate: true);

        var flow = FindObjectOfType<FlowManager>();
        if (flow != null)
        {
            flow.OnValveStateChanged(this);
        }
    }

    public void SetConnectedVisual(bool connected)
    {
        if (_wheelSr != null)
        {
            if (valveType == ValveType.OnOff)
            {
                _wheelSr.color = connected ? activeFlowColor : (isOpen ? openColor : closedColor);
            }
            else
            {
                _wheelSr.color = connected ? activeFlowColor : openColor;
            }
        }
    }

    private void UpdateVisuals(bool immediate)
    {
        if (_wheelSr == null) _wheelSr = GetComponent<SpriteRenderer>();

        if (valveType == ValveType.OnOff)
        {
            float targetZ = isOpen ? 90f : 0f;
            _targetRotation = Quaternion.Euler(0f, 0f, targetZ);
            if (_wheelSr != null)
            {
                _wheelSr.color = isOpen ? openColor : closedColor;
            }
        }
        else // Directional
        {
            float targetZ = -90f * allowedDirection;
            _targetRotation = Quaternion.Euler(0f, 0f, targetZ);
            if (_wheelSr != null) _wheelSr.color = openColor;
            if (_arrowSr != null)
            {
                _arrowSr.transform.rotation = _targetRotation;
                _arrowSr.color = activeFlowColor;
            }
        }

        if (immediate)
        {
            transform.rotation = _targetRotation;
        }
        else
        {
            StartCoroutine(AnimateToggle());
        }
    }

    private IEnumerator AnimateToggle()
    {
        _isAnimating = true;
        Quaternion start = transform.rotation;
        float elapsed = 0f;
        const float duration = 0.18f;

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
}
