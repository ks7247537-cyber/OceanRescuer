using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Interactive Pressure Release Valve for Ocean Rescuers (World 1 — Deep Ocean).
/// Strategically vents excessive deep-sea hydraulic pressure from connected pipe routes.
/// 
/// Hierarchy:
/// PressureReleaseValve
/// ├── ValveBase (bronze heavy base housing)
/// ├── ValveHandle (rotatable golden brass valve wheel)
/// ├── PressureReleaseEffect (high-velocity water jet burst)
/// ├── BubbleEffect (pulsing rising bubbles)
/// └── InteractionFeedback (3D floating hint text)
/// 
/// Interactivity:
/// - Tap to open valve and vent excessive pipeline backpressure (-30 PSI).
/// - Plays smooth wheel spin, water jet, bubble stream, and sound effect.
/// - DeepOceanPressureManager calculates relief, smoothly dropping the Pressure HUD gauge.
/// - Tap again to re-seal if player needs to accumulate pressure for a Pressure Gate!
/// </summary>
[DisallowMultipleComponent]
public class PressureReleaseValve : MonoBehaviour
{
    [Header("Valve Configuration")]
    public Vector2Int gridPos;
    public bool isVenting = false;

    [Tooltip("Amount of hydraulic pressure relieved when venting (PSI).")]
    public float reliefAmount = 30f;

    [Header("Visual Colors (Inspector Editable)")]
    public Color baseHousingColor = new Color(0.85f, 0.70f, 0.25f, 1f); // Metallic Bronze
    public Color handleActiveColor = new Color(1.0f, 0.85f, 0.30f, 1f);  // Golden Brass
    public Color ventWaterColor    = new Color(0.15f, 1.0f, 1.0f, 0.95f); // High-pressure Cyan
    public Color bubbleTint        = new Color(0.60f, 0.95f, 1.0f, 0.85f); // Soft Aqua

    [Header("Child References")]
    public SpriteRenderer valveBase;
    public SpriteRenderer valveHandle;
    public SpriteRenderer releaseEffect;
    public SpriteRenderer bubbleEffect;
    public TextMeshPro feedbackText;

    private bool _isAnimating = false;
    private int _lastInteractFrame = -1;
    private Coroutine _floatingHintCoroutine;
    private Coroutine _ventingAnimCoroutine;

    private void Awake()
    {
        SetupComponents();
    }

    private void Start()
    {
        // Strict World Isolation: Deactivate immediately if not Deep Ocean
        if (SaveManager.GetSelectedWorld() != 1)
        {
            gameObject.SetActive(false);
            return;
        }

        UpdateVisuals(immediate: true);
    }

    private void SetupComponents()
    {
        // Touch/click collider
        var circle = GetComponent<CircleCollider2D>();
        if (circle == null) circle = gameObject.AddComponent<CircleCollider2D>();
        circle.radius = 1.05f;

        // 1. ValveBase (Heavy Bronze Housing)
        if (valveBase == null)
        {
            var baseObj = transform.Find("ValveBase");
            if (baseObj == null)
            {
                var go = new GameObject("ValveBase");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.zero;
                valveBase = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                valveBase = baseObj.GetComponent<SpriteRenderer>();
            }
        }
        if (valveBase.sprite == null)
            valveBase.sprite = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield") 
                            ?? Resources.Load<Sprite>("Sprites/red_valve");
        valveBase.color = baseHousingColor;
        valveBase.sortingOrder = 6;

        // 2. ValveHandle (Golden Brass Wheel)
        if (valveHandle == null)
        {
            var handleObj = transform.Find("ValveHandle");
            if (handleObj == null)
            {
                var go = new GameObject("ValveHandle");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                valveHandle = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                valveHandle = handleObj.GetComponent<SpriteRenderer>();
            }
        }
        if (valveHandle.sprite == null)
            valveHandle.sprite = Resources.Load<Sprite>("Sprites/red_valve");
        valveHandle.color = handleActiveColor;
        valveHandle.sortingOrder = 7;

        // 3. PressureReleaseEffect (Water Jet Burst)
        if (releaseEffect == null)
        {
            var effectObj = transform.Find("PressureReleaseEffect");
            if (effectObj == null)
            {
                var go = new GameObject("PressureReleaseEffect");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.45f, -0.08f);
                go.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
                releaseEffect = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                releaseEffect = effectObj.GetComponent<SpriteRenderer>();
            }
        }
        if (releaseEffect.sprite == null)
            releaseEffect.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") 
                                ?? Resources.Load<Sprite>("Sprites/bubbles");
        releaseEffect.color = ventWaterColor;
        releaseEffect.sortingOrder = 8;
        releaseEffect.gameObject.SetActive(false);

        // 4. BubbleEffect (Ascending Bubbles)
        if (bubbleEffect == null)
        {
            var bubbleObj = transform.Find("BubbleEffect");
            if (bubbleObj == null)
            {
                var go = new GameObject("BubbleEffect");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.65f, -0.1f);
                go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                bubbleEffect = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                bubbleEffect = bubbleObj.GetComponent<SpriteRenderer>();
            }
        }
        if (bubbleEffect.sprite == null)
            bubbleEffect.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        bubbleEffect.color = bubbleTint;
        bubbleEffect.sortingOrder = 9;
        bubbleEffect.gameObject.SetActive(false);

        // 5. InteractionFeedback (Floating 3D Text)
        if (feedbackText == null)
        {
            var textObj = transform.Find("InteractionFeedback");
            if (textObj == null)
            {
                var go = new GameObject("InteractionFeedback");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.95f, -0.15f);
                feedbackText = go.AddComponent<TextMeshPro>();
            }
            else
            {
                feedbackText = textObj.GetComponent<TextMeshPro>();
            }
        }
        feedbackText.fontSize = 2.4f;
        feedbackText.fontStyle = FontStyles.Bold;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.sortingOrder = 16;
        feedbackText.gameObject.SetActive(false);
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

    public void Interact()
    {
        if (Time.frameCount == _lastInteractFrame) return;
        _lastInteractFrame = Time.frameCount;

        if (_isAnimating) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Toggle venting state
        isVenting = !isVenting;

        if (isVenting)
        {
            // Vent excessive backpressure
            if (DeepOceanPressureManager.Instance != null)
            {
                DeepOceanPressureManager.Instance.VentPressure(reliefAmount);
            }

            AudioManager.Instance?.PlayWaterFlow();
            ShowFloatingHint($"<color=#00FFFF>💨 VENTED! -{Mathf.RoundToInt(reliefAmount)} PSI</color>", Color.cyan);
        }
        else
        {
            // Re-seal valve
            if (DeepOceanPressureManager.Instance != null)
            {
                DeepOceanPressureManager.Instance.ResetVentedPressure();
            }

            AudioManager.Instance?.PlayPipeRotate();
            ShowFloatingHint("<color=#FFDD44>🔒 VALVE SEALED</color>", Color.yellow);
        }

        UpdateVisuals(immediate: false);
    }

    private void UpdateVisuals(bool immediate)
    {
        if (releaseEffect != null) releaseEffect.gameObject.SetActive(isVenting);
        if (bubbleEffect != null) bubbleEffect.gameObject.SetActive(isVenting);

        if (immediate)
        {
            if (valveHandle != null)
                valveHandle.transform.localRotation = isVenting ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;
        }
        else
        {
            if (_ventingAnimCoroutine != null) StopCoroutine(_ventingAnimCoroutine);
            _ventingAnimCoroutine = StartCoroutine(AnimateHandleRotation());
        }
    }

    private IEnumerator AnimateHandleRotation()
    {
        _isAnimating = true;
        Quaternion start = valveHandle != null ? valveHandle.transform.localRotation : Quaternion.identity;
        Quaternion target = isVenting ? Quaternion.Euler(0f, 0f, 180f) : Quaternion.identity;

        float elapsed = 0f;
        const float duration = 0.22f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            if (valveHandle != null)
                valveHandle.transform.localRotation = Quaternion.Slerp(start, target, t);
            yield return null;
        }

        if (valveHandle != null) valveHandle.transform.localRotation = target;
        _isAnimating = false;
    }

    private void ShowFloatingHint(string text, Color color)
    {
        if (feedbackText == null) return;
        if (_floatingHintCoroutine != null) StopCoroutine(_floatingHintCoroutine);
        _floatingHintCoroutine = StartCoroutine(FloatingHintRoutine(text, color));
    }

    private IEnumerator FloatingHintRoutine(string text, Color color)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.text = text;
        feedbackText.color = color;

        Vector3 startPos = new Vector3(0f, 0.95f, -0.15f);
        Vector3 endPos   = new Vector3(0f, 1.45f, -0.15f);
        feedbackText.transform.localPosition = startPos;

        float elapsed = 0f;
        const float duration = 1.1f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            feedbackText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            Color c = color;
            c.a = Mathf.Clamp01(1f - (t * t));
            feedbackText.color = c;
            yield return null;
        }

        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
    }

    public void RotatePipeUnderneath()
    {
        var grid = GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        if (grid != null)
        {
            var pipe = grid.GetTileAt(gridPos.x, gridPos.y);
            if (pipe != null) pipe.Rotate();
        }
    }
}
