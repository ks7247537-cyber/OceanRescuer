using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Interactive Water Valve for Ocean Rescuers.
/// Controls water flow through pipes:
/// - ON/OFF Valve: Blocks water when CLOSED (red lock), allows flow when OPEN (green unlocked).
/// - Directional Valve: One-way check valve permitting water flow only in specified direction.
/// 
/// Compactly sized (~38% of tile) so the underlying pipe geometry, joints, and flowing water
/// are 100% visible at all times.
/// Provides clear visual badges, status labels, audio feedback, and floating notifications on tap.
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
    public Color closedColor     = new Color(1f, 0.35f, 0.35f, 1f); // Vibrant red
    public Color openColor       = new Color(0.25f, 1f, 0.55f, 1f); // Emerald green
    public Color activeFlowColor = new Color(0.1f, 1f, 1f, 1f);     // Electric cyan

    private SpriteRenderer _wheelSr;
    private SpriteRenderer _badgeSr;
    private SpriteRenderer _arrowSr;
    private TextMeshPro _statusText;
    private TextMeshPro _floatingHintText;

    private Sprite _spriteLocked;
    private Sprite _spriteUnlocked;

    private bool _isAnimating = false;
    private Quaternion _targetRotation;
    private int _lastInteractFrame = -1;
    private Coroutine _floatingHintCoroutine;

    public static readonly Vector3[] DIR_VECTORS = {
        Vector3.up, Vector3.right, Vector3.down, Vector3.left
    };

    private static readonly string[] DIR_NAMES = { "UP ⬆", "RIGHT ➡", "DOWN ⬇", "LEFT ⬅" };

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
        // Compact scale: ~38% of tile so the underlying pipe is completely visible
        transform.localScale = new Vector3(0.38f, 0.38f, 1f);

        _spriteLocked   = Resources.Load<Sprite>("Sprites/icon_locked");
        _spriteUnlocked = Resources.Load<Sprite>("Sprites/icon_unlocked");

        // Touch / click collider
        var circle = GetComponent<CircleCollider2D>();
        if (circle == null) circle = gameObject.AddComponent<CircleCollider2D>();
        circle.radius = 0.95f; // Touch radius scaled by 0.38 = ~0.36 world units

        // 1. Wheel renderer (main rotating valve handle)
        _wheelSr = GetComponent<SpriteRenderer>();
        if (_wheelSr == null) _wheelSr = gameObject.AddComponent<SpriteRenderer>();
        if (_wheelSr.sprite == null)
        {
            _wheelSr.sprite = Resources.Load<Sprite>("Sprites/red_valve");
        }
        _wheelSr.sortingOrder = 6;

        // 2. Center status badge (Lock / Unlocked icon)
        var badgeObj = transform.Find("ValveBadge");
        if (badgeObj == null)
        {
            var go = new GameObject("ValveBadge");
            go.transform.SetParent(transform, false);
            _badgeSr = go.AddComponent<SpriteRenderer>();
            _badgeSr.sortingOrder = 8;
            go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            go.transform.localPosition = new Vector3(0f, 0f, -0.05f);
        }
        else
        {
            _badgeSr = badgeObj.GetComponent<SpriteRenderer>();
        }

        // 3. Direction arrow for Directional Valve
        var arrowObj = transform.Find("DirectionArrow");
        if (arrowObj == null && valveType == ValveType.Directional)
        {
            var go = new GameObject("DirectionArrow");
            go.transform.SetParent(transform, false);
            _arrowSr = go.AddComponent<SpriteRenderer>();
            _arrowSr.sprite = Resources.Load<Sprite>("Sprites/red_valve");
            _arrowSr.color = Color.cyan;
            _arrowSr.sortingOrder = 7;
            go.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
            go.transform.localPosition = new Vector3(0f, 0f, -0.04f);
        }
        else if (arrowObj != null)
        {
            _arrowSr = arrowObj.GetComponent<SpriteRenderer>();
        }

        // 4. World Status Label (OPEN / CLOSED badge under the valve)
        var labelObj = transform.Find("StatusLabel");
        if (labelObj == null)
        {
            var go = new GameObject("StatusLabel");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, -1.05f, -0.1f);
            // Counteract local scale so text remains crisp and readable
            go.transform.localScale = new Vector3(1f / 0.38f, 1f / 0.38f, 1f);

            _statusText = go.AddComponent<TextMeshPro>();
            _statusText.fontSize = 2.0f;
            _statusText.fontStyle = FontStyles.Bold;
            _statusText.alignment = TextAlignmentOptions.Center;
            _statusText.sortingOrder = 15;
        }
        else
        {
            _statusText = labelObj.GetComponent<TextMeshPro>();
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
                    if (hit != null && (hit.gameObject == gameObject || (hit.transform.parent != null && hit.transform.parent.gameObject == gameObject)))
                    {
                        if (valveType == ValveType.OnOff && isOpen)
                        {
                            RotatePipeUnderneath();
                        }
                        else
                        {
                            Interact();
                        }
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        // Guarantee text badges never rotate with the wheel
        if (_statusText != null)
        {
            _statusText.transform.rotation = Quaternion.identity;
        }
        if (_floatingHintText != null && _floatingHintText.gameObject.activeSelf)
        {
            _floatingHintText.transform.rotation = Quaternion.identity;
        }
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        if (valveType == ValveType.OnOff && isOpen)
        {
            RotatePipeUnderneath();
            return;
        }

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

        // If this OnOff valve is already open, any tap on this tile rotates the pipe underneath!
        if (valveType == ValveType.OnOff && isOpen)
        {
            RotatePipeUnderneath();
            return;
        }

        bool prevOpen = isOpen;
        int prevDir = allowedDirection;

        // Record move via MoveManager
        if (MoveManager.Instance != null)
        {
            bool allowed = MoveManager.Instance.RecordValveToggle(this, prevOpen, prevDir);
            if (!allowed) return;
        }

        // Play valve click audio
        AudioManager.Instance?.PlayPipeRotate();

        if (valveType == ValveType.OnOff)
        {
            isOpen = !isOpen;
            if (isOpen)
            {
                ShowFloatingHint("OPEN", new Color(0.3f, 1f, 0.6f));
            }
            else
            {
                ShowFloatingHint("CLOSED", new Color(1f, 0.4f, 0.4f));
            }
        }
        else if (valveType == ValveType.Directional)
        {
            allowedDirection = (allowedDirection + 1) % 4;
            ShowFloatingHint($"➜ FLOW: {DIR_NAMES[allowedDirection]}", Color.cyan);
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
    /// Displays a temporary floating status feedback badge that gently rises and fades.
    /// </summary>
    public void ShowFloatingHint(string message, Color color)
    {
        if (_floatingHintCoroutine != null) StopCoroutine(_floatingHintCoroutine);
        _floatingHintCoroutine = StartCoroutine(FloatingHintRoutine(message, color));
    }

    private IEnumerator FloatingHintRoutine(string message, Color color)
    {
        if (_floatingHintText == null)
        {
            var go = new GameObject("FloatingHint");
            go.transform.SetParent(transform, false);
            go.transform.localScale = new Vector3(1f / 0.38f, 1f / 0.38f, 1f);
            _floatingHintText = go.AddComponent<TextMeshPro>();
            _floatingHintText.fontSize = 2.4f;
            _floatingHintText.fontStyle = FontStyles.Bold;
            _floatingHintText.alignment = TextAlignmentOptions.Center;
            _floatingHintText.sortingOrder = 25;
        }

        _floatingHintText.text = message;
        _floatingHintText.color = color;
        _floatingHintText.gameObject.SetActive(true);

        Vector3 startPos = new Vector3(0f, 1.15f, -0.2f);
        Vector3 endPos = new Vector3(0f, 1.75f, -0.2f);

        float elapsed = 0f;
        const float duration = 1.1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            _floatingHintText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            Color c = color;
            c.a = Mathf.Clamp01(1f - (t * t)); // Smooth fade out
            _floatingHintText.color = c;
            yield return null;
        }

        if (_floatingHintText != null)
            _floatingHintText.gameObject.SetActive(false);
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

    /// <summary>
    /// Updates visuals when water connection flow reaches this valve.
    /// </summary>
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

        if (_statusText != null && connected && (isOpen || valveType == ValveType.Directional))
        {
            _statusText.text = "";
        }
    }

    public void RotatePipeUnderneath()
    {
        var grid = GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        if (grid != null)
        {
            var pipe = grid.GetTileAt(gridPos.x, gridPos.y);
            if (pipe != null)
            {
                pipe.Rotate();
            }
        }
    }

    private void UpdateVisuals(bool immediate)
    {
        if (_wheelSr == null) _wheelSr = GetComponent<SpriteRenderer>();

        if (valveType == ValveType.OnOff)
        {
            var circle = GetComponent<CircleCollider2D>();
            if (circle != null) circle.enabled = true; // Always keep enabled so clicks/taps on open valve rotate the underlying pipe!

            float targetZ = isOpen ? 90f : 0f;
            _targetRotation = Quaternion.Euler(0f, 0f, targetZ);

            if (_wheelSr != null)
            {
                _wheelSr.color = isOpen ? openColor : closedColor;
            }

            if (_badgeSr != null)
            {
                _badgeSr.sprite = isOpen ? _spriteUnlocked : _spriteLocked;
                _badgeSr.color  = isOpen ? new Color(0.3f, 1f, 0.6f, 1f) : new Color(1f, 0.4f, 0.4f, 1f);
            }

            if (_statusText != null)
            {
                _statusText.text = isOpen 
                    ? "" 
                    : "<color=#FF4444>CLOSED</color>";
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

            if (_statusText != null)
            {
                _statusText.text = $"<color=#00FFFF>{DIR_NAMES[allowedDirection]}</color>";
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
        const float duration = 0.22f;

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
