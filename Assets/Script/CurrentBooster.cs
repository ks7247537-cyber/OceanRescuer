using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Current Booster tile mechanic for Ocean Rescuers (World 1 — Deep Ocean).
/// Represents a powerful underwater current stream on the puzzle grid:
/// - When water flows through this tile aligned with the boost direction,
///   the hydrodynamic current accelerates the flow and reduces hydraulic pressure
///   burden by 15 PSI in DeepOceanPressureManager.
/// - Features distinctive underwater styling: glowing directional indicator,
///   pulsing electric cyan flow streams, and informative 3D feedback.
/// - Strictly isolated to World 1 (Deep Ocean).
/// 
/// Hierarchy:
/// CurrentBooster
/// ├── BoosterBase (deep cyan/teal metallic grid mount)
/// ├── CurrentDirectionIndicator (illuminated directional arrow)
/// ├── FlowEffect (high-velocity stream effect)
/// └── InteractionFeedback (floating hint & activation text)
/// </summary>
[DisallowMultipleComponent]
public class CurrentBooster : MonoBehaviour
{
    [Header("Booster Configuration")]
    public Vector2Int gridPos;

    [Tooltip("Boost direction: 0=Up, 1=Right, 2=Down, 3=Left")]
    [Range(0, 3)]
    public int boostDirection = 1;

    [Tooltip("Effective hydraulic pressure relief (PSI) granted when flow is boosted.")]
    public float pressureRelief = 15f;

    [Header("Visual Colors (Inspector Editable)")]
    public Color basePlateColor        = new Color(0.12f, 0.40f, 0.65f, 0.90f);
    public Color indicatorIdleColor    = new Color(0.20f, 0.80f, 1.0f, 0.70f);
    public Color indicatorActiveColor  = new Color(0.0f, 1.0f, 1.0f, 1.0f);
    public Color flowEffectColor       = new Color(0.15f, 1.0f, 0.95f, 0.85f);

    [Header("Child References")]
    public SpriteRenderer boosterBase;
    public SpriteRenderer directionIndicator;
    public SpriteRenderer flowEffect;
    public TextMeshPro feedbackText;

    [Header("Runtime State")]
    [SerializeField] private bool _isBoosting = false;
    public bool IsBoosting => _isBoosting;

    private Coroutine _floatingHintCoroutine;
    private Coroutine _flowAnimCoroutine;
    private float _lastEvaluateTime = -1f;

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
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.RegisterBooster(this);
        }
    }

    private void OnDestroy()
    {
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.UnregisterBooster(this);
        }
    }

    private void OnDisable()
    {
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.UnregisterBooster(this);
        }
    }

    private void OnEnable()
    {
        if (SaveManager.GetSelectedWorld() == 1 && DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.RegisterBooster(this);
        }
    }

    public void SetupComponents()
    {
        // Touch / click collider
        var box = GetComponent<BoxCollider2D>();
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
        box.size = new Vector2(0.9f, 0.9f);
        box.isTrigger = true;

        // 1. BoosterBase (Deep Oceanic Plate)
        if (boosterBase == null)
        {
            var baseObj = transform.Find("BoosterBase");
            if (baseObj == null)
            {
                var go = new GameObject("BoosterBase");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.zero;
                boosterBase = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                boosterBase = baseObj.GetComponent<SpriteRenderer>();
            }
        }
        if (boosterBase.sprite == null)
            boosterBase.sprite = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield") 
                              ?? Resources.Load<Sprite>("Sprites/red_valve");
        boosterBase.color = basePlateColor;
        boosterBase.sortingOrder = 4; // Sits just below pipe graphics

        // 2. CurrentDirectionIndicator (Directional Arrow)
        if (directionIndicator == null)
        {
            var indObj = transform.Find("CurrentDirectionIndicator");
            if (indObj == null)
            {
                var go = new GameObject("CurrentDirectionIndicator");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                directionIndicator = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                directionIndicator = indObj.GetComponent<SpriteRenderer>();
            }
        }
        if (directionIndicator.sprite == null)
            directionIndicator.sprite = Resources.Load<Sprite>("Sprites/icon_unlocked") 
                                     ?? Resources.Load<Sprite>("Sprites/red_valve");
        directionIndicator.color = indicatorIdleColor;
        directionIndicator.sortingOrder = 7;

        // 3. FlowEffect (Pulsing high-velocity current streams)
        if (flowEffect == null)
        {
            var flowObj = transform.Find("FlowEffect");
            if (flowObj == null)
            {
                var go = new GameObject("FlowEffect");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                flowEffect = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                flowEffect = flowObj.GetComponent<SpriteRenderer>();
            }
        }
        if (flowEffect.sprite == null)
            flowEffect.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") 
                             ?? Resources.Load<Sprite>("Sprites/bubbles");
        flowEffect.color = flowEffectColor;
        flowEffect.sortingOrder = 8;
        flowEffect.gameObject.SetActive(false);

        // 4. InteractionFeedback (Floating 3D Text)
        if (feedbackText == null)
        {
            var textObj = transform.Find("InteractionFeedback");
            if (textObj == null)
            {
                var go = new GameObject("InteractionFeedback");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.95f, -0.2f);
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
        feedbackText.sortingOrder = 20;
        feedbackText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (SaveManager.GetSelectedWorld() != 1) return;

        // Periodic evaluation of flow passing through this booster tile
        if (Time.unscaledTime - _lastEvaluateTime > 0.2f)
        {
            _lastEvaluateTime = Time.unscaledTime;
            EvaluateCurrentFlow();
        }

        // Tap/click interaction for informative feedback
        HandleInputInteraction();

        // Idle animation
        if (!_isBoosting && directionIndicator != null)
        {
            float pulse = 0.9f + 0.1f * Mathf.Sin(Time.time * 2.5f);
            directionIndicator.transform.localScale = new Vector3(pulse, pulse, 1f);
        }
    }

    private void HandleInputInteraction()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && Camera.main != null)
            {
                Vector2 wp = Camera.main.ScreenToWorldPoint(touch.position);
                var hit = Physics2D.OverlapPoint(wp);
                if (hit != null && (hit.gameObject == gameObject || (hit.transform.parent != null && hit.transform.parent.gameObject == gameObject)))
                {
                    OnTileTapped();
                }
            }
        }
        else if (Input.GetMouseButtonDown(0) && Camera.main != null)
        {
            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.OverlapPoint(wp);
            if (hit != null && (hit.gameObject == gameObject || (hit.transform.parent != null && hit.transform.parent.gameObject == gameObject)))
            {
                OnTileTapped();
            }
        }
    }

    private void OnTileTapped()
    {
        if (_isBoosting)
        {
            ShowFloatingFeedback("<color=#00FFFF>🌊 -15 PSI</color>");
        }
        else
        {
            string dirName = boostDirection == 0 ? "UP" : boostDirection == 1 ? "RIGHT" : boostDirection == 2 ? "DOWN" : "LEFT";
            ShowFloatingFeedback($"<color=#38BDF8>🌊 ALIGN {dirName}</color>");
        }
    }

    /// <summary>
    /// Checks whether water is actively flowing through this booster cell in the boosted direction.
    /// </summary>
    public void EvaluateCurrentFlow()
    {
        bool wasBoosting = _isBoosting;
        bool isNowBoosting = false;

        if (GridManager.Instance != null)
        {
            var tile = GridManager.Instance.GetTileAt(gridPos.x, gridPos.y);
            // Connected tile with opening aligned with current booster direction
            if (tile != null && tile.isConnected && tile.HasOpening(boostDirection))
            {
                isNowBoosting = true;
            }
        }

        if (isNowBoosting != wasBoosting)
        {
            _isBoosting = isNowBoosting;
            OnBoostingStateChanged(isNowBoosting);
        }
    }

    private void OnBoostingStateChanged(bool active)
    {
        UpdateVisuals(immediate: false);

        if (active)
        {
            AudioManager.Instance?.PlayWaterFlow();
            ShowFloatingFeedback("<color=#00FFFF>🌊 -15 PSI</color>");
            if (_flowAnimCoroutine != null) StopCoroutine(_flowAnimCoroutine);
            _flowAnimCoroutine = StartCoroutine(ActiveFlowRoutine());
        }
        else
        {
            if (_flowAnimCoroutine != null)
            {
                StopCoroutine(_flowAnimCoroutine);
                _flowAnimCoroutine = null;
            }
            if (flowEffect != null) flowEffect.gameObject.SetActive(false);
        }

        // Notify DeepOceanPressureManager to update pressure immediately
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.EvaluatePressure();
        }
    }

    public void UpdateVisuals(bool immediate)
    {
        // 1. Rotate direction indicator to face boostDirection
        // 0=Up (+90), 1=Right (0), 2=Down (270), 3=Left (180)
        float angle = boostDirection switch
        {
            0 => 90f,
            1 => 0f,
            2 => 270f,
            3 => 180f,
            _ => 0f
        };

        if (directionIndicator != null)
        {
            directionIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            directionIndicator.color = _isBoosting ? indicatorActiveColor : indicatorIdleColor;
        }

        if (boosterBase != null)
        {
            boosterBase.color = _isBoosting 
                ? new Color(0.18f, 0.65f, 0.90f, 0.95f) 
                : basePlateColor;
        }

        if (flowEffect != null)
        {
            flowEffect.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            flowEffect.gameObject.SetActive(_isBoosting);
        }
    }

    private IEnumerator ActiveFlowRoutine()
    {
        if (flowEffect == null) yield break;
        flowEffect.gameObject.SetActive(true);

        Vector3 minScale = new Vector3(0.7f, 0.7f, 1f);
        Vector3 maxScale = new Vector3(1.15f, 1.15f, 1f);

        while (_isBoosting)
        {
            float t = (Mathf.Sin(Time.time * 6f) + 1f) * 0.5f;
            flowEffect.transform.localScale = Vector3.Lerp(minScale, maxScale, t);
            flowEffect.color = new Color(flowEffectColor.r, flowEffectColor.g, flowEffectColor.b, 0.4f + 0.6f * t);
            yield return null;
        }

        flowEffect.gameObject.SetActive(false);
    }

    public void ShowFloatingFeedback(string message)
    {
        if (feedbackText == null) return;
        if (_floatingHintCoroutine != null) StopCoroutine(_floatingHintCoroutine);
        _floatingHintCoroutine = StartCoroutine(AnimateFloatingText(message));
    }

    private IEnumerator AnimateFloatingText(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        Vector3 startPos = new Vector3(0f, 0.75f, -0.2f);
        Vector3 endPos = new Vector3(0f, 1.25f, -0.2f);
        float elapsed = 0f;
        float duration = 1.3f;

        Color startColor = feedbackText.color;
        startColor.a = 1f;
        feedbackText.color = startColor;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            feedbackText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            
            Color c = startColor;
            c.a = Mathf.Clamp01(1f - (t * t));
            feedbackText.color = c;
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
    }
}
