using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Pressure Lock Gate for Ocean Rescuers (World 1 — Deep Ocean ONLY).
/// A specialized hydraulic security bulkhead that remains locked until:
/// 1. Water reaches its designated activation route (adjacent connected pipe).
/// 2. Deep ocean hydraulic pipeline pressure enters the safe operating window
///    [minActivationPressure, maxActivationPressure].
/// 3. The required pressure condition remains satisfied continuously for
///    requiredStableDuration (e.g. 1.0s).
/// 
/// Once unlocked:
/// - Smooth visual opening animation (bars slide open, hydraulic glow pulses).
/// - Unblocks water route, allowing flow to continue downstream.
/// - Interacts with Pressure Release Valves (to vent overpressure) and Current Boosters.
/// - Strictly isolated to World 1 (Deep Ocean).
/// 
/// Hierarchy:
/// PressureLockGate
/// ├── GateBase (heavy reinforced metallic bulkhead frame)
/// ├── GateBars (sliding reinforced hydraulic barrier doors)
/// ├── PressureGauge (acoustic PSI dial & fill indicator)
/// ├── HydraulicGlow (cyan bioluminescent pressurization aura)
/// └── InteractionFeedback (3D floating feedback & status text)
/// </summary>
[DisallowMultipleComponent]
public class PressureLockGate : GateComponent
{
    [Header("Pressure Activation Configuration")]
    [Tooltip("Minimum hydraulic PSI required to unlock.")]
    public float minActivationPressure = 40.0f;

    [Tooltip("Maximum hydraulic PSI allowed to unlock (prevents overpressure blowouts).")]
    public float maxActivationPressure = 100.0f;

    [Tooltip("Duration in seconds that pressure must remain stable in target window.")]
    public float requiredStableDuration = 0.35f;

    [Header("Visual Colors (Inspector Editable)")]
    public Color lockedBaseColor      = new Color(0.25f, 0.38f, 0.48f, 1.0f); // Heavy metallic slate
    public Color lockedBarsColor      = new Color(0.95f, 0.75f, 0.25f, 1.0f); // Golden brass
    public Color activatingGlowColor  = new Color(0.0f, 1.0f, 1.0f, 0.95f);   // Cyan hydraulic glow
    public Color overpressureColor    = new Color(1.0f, 0.25f, 0.2f, 1.0f);   // Warning red
    public Color unlockedColor        = new Color(0.2f, 1.0f, 0.5f, 0.25f);   // Retracted translucent

    [Header("Child References")]
    public SpriteRenderer gateBase;
    public SpriteRenderer gateBars;
    public SpriteRenderer pressureGauge;
    public SpriteRenderer hydraulicGlow;
    public TextMeshPro statusBadgeText;
    public TextMeshPro feedbackText;

    private float _stableTimer = 0f;
    private bool _isUnlocking = false;
    private Coroutine _feedbackCoroutine;
    private Coroutine _openAnimationCoroutine;

    public float CurrentPSI => (DeepOceanPressureManager.Instance != null) 
        ? DeepOceanPressureManager.Instance.CurrentPressure 
        : 55f;

    public bool IsPressureInRange => CurrentPSI >= minActivationPressure && CurrentPSI <= maxActivationPressure;
    public float ActivationProgress => Mathf.Clamp01(_stableTimer / Mathf.Max(0.1f, requiredStableDuration));

    private void Awake()
    {
        gateType = GateType.PressureLockGate;
        SetupGateComponents();
    }

    private void Start()
    {
        UpdateVisuals(immediate: true);
    }

    public void SetupGateComponents()
    {
        // Root Collider
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);
        }
        transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        // 1. GateBase (Heavy Reinforced Frame)
        if (gateBase == null)
        {
            var baseObj = transform.Find("GateBase");
            if (baseObj == null)
            {
                var go = new GameObject("GateBase");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.zero;
                gateBase = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                gateBase = baseObj.GetComponent<SpriteRenderer>();
            }
        }
        if (gateBase.sprite == null)
            gateBase.sprite = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield") 
                           ?? Resources.Load<Sprite>("Sprites/red_valve");
        gateBase.color = lockedBaseColor;
        gateBase.sortingOrder = 6;

        // 2. GateBars (Hydraulic Sliding Doors)
        if (gateBars == null)
        {
            var barsObj = transform.Find("GateBars");
            if (barsObj == null)
            {
                var go = new GameObject("GateBars");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                gateBars = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                gateBars = barsObj.GetComponent<SpriteRenderer>();
            }
        }
        if (gateBars.sprite == null)
            gateBars.sprite = Resources.Load<Sprite>("Sprites/obstacle") 
                           ?? Resources.Load<Sprite>("Sprites/icon_locked");
        gateBars.color = lockedBarsColor;
        gateBars.sortingOrder = 7;

        // 3. PressureGauge (Mini Dial Indicator)
        if (pressureGauge == null)
        {
            var gaugeObj = transform.Find("PressureGauge");
            if (gaugeObj == null)
            {
                var go = new GameObject("PressureGauge");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, -0.35f, -0.1f);
                go.transform.localScale = Vector3.one * 0.6f;
                pressureGauge = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                pressureGauge = gaugeObj.GetComponent<SpriteRenderer>();
            }
        }
        if (pressureGauge.sprite == null)
            pressureGauge.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        pressureGauge.color = Color.cyan;
        pressureGauge.sortingOrder = 8;

        // 4. HydraulicGlow (Pressurization Aura)
        if (hydraulicGlow == null)
        {
            var glowObj = transform.Find("HydraulicGlow");
            if (glowObj == null)
            {
                var go = new GameObject("HydraulicGlow");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.12f);
                hydraulicGlow = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                hydraulicGlow = glowObj.GetComponent<SpriteRenderer>();
            }
        }
        if (hydraulicGlow.sprite == null)
            hydraulicGlow.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning");
        hydraulicGlow.color = activatingGlowColor;
        hydraulicGlow.sortingOrder = 9;
        hydraulicGlow.gameObject.SetActive(false);

        // 5. StatusBadgeText (Target PSI range display)
        if (statusBadgeText == null)
        {
            var textObj = transform.Find("StatusBadgeText");
            if (textObj == null)
            {
                var go = new GameObject("StatusBadgeText");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, -0.65f, -0.15f);
                statusBadgeText = go.AddComponent<TextMeshPro>();
            }
            else
            {
                statusBadgeText = textObj.GetComponent<TextMeshPro>();
            }
        }
        statusBadgeText.fontSize = 2.0f;
        statusBadgeText.fontStyle = FontStyles.Bold;
        statusBadgeText.alignment = TextAlignmentOptions.Center;
        statusBadgeText.sortingOrder = 10;
        statusBadgeText.text = $"{minActivationPressure:F0}-{maxActivationPressure:F0} PSI";

        // 6. InteractionFeedback (Floating feedback)
        if (feedbackText == null)
        {
            var fbObj = transform.Find("InteractionFeedback");
            if (fbObj == null)
            {
                var go = new GameObject("InteractionFeedback");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.95f, -0.2f);
                feedbackText = go.AddComponent<TextMeshPro>();
            }
            else
            {
                feedbackText = fbObj.GetComponent<TextMeshPro>();
            }
        }
        feedbackText.fontSize = 2.4f;
        feedbackText.fontStyle = FontStyles.Bold;
        feedbackText.alignment = TextAlignmentOptions.Center;
        feedbackText.sortingOrder = 22;
        feedbackText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isOpen || _isUnlocking) return;

        HandleTouchInput();
        EvaluatePressureActivation();
    }

    private void HandleTouchInput()
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
                    OnGateTapped();
                }
            }
        }
        else if (Input.GetMouseButtonDown(0) && Camera.main != null)
        {
            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.OverlapPoint(wp);
            if (hit != null && (hit.gameObject == gameObject || (hit.transform.parent != null && hit.transform.parent.gameObject == gameObject)))
            {
                OnGateTapped();
            }
        }
    }

    private void OnGateTapped()
    {
        if (!isOpen)
        {
            UnlockGate();
        }
        else
        {
            ShowFloatingFeedback("<color=#00FFFF>🔓 OPEN</color>");
        }

        RotatePipeUnderneath();
    }

    private void RotatePipeUnderneath()
    {
        var grid = (GridManager.Instance != null) ? GridManager.Instance : FindObjectOfType<GridManager>();
        if (grid != null)
        {
            var pipe = grid.GetTileAt(gridPos.x, gridPos.y);
            if (pipe != null)
            {
                pipe.Rotate();
            }
        }
    }

    private void EvaluatePressureActivation()
    {
        bool waterReached = IsNeighborConnected();
        float p = CurrentPSI;
        bool inRange = (p >= minActivationPressure && p <= maxActivationPressure);

        if (waterReached && inRange)
        {
            _stableTimer += Time.deltaTime;

            if (hydraulicGlow != null)
            {
                hydraulicGlow.gameObject.SetActive(true);
                float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 8f);
                hydraulicGlow.transform.localScale = Vector3.one * pulse;
            }

            if (statusBadgeText != null)
            {
                statusBadgeText.text = $"⚡ {p:F0} PSI ({ActivationProgress:P0})";
                statusBadgeText.color = Color.cyan;
            }

            if (_stableTimer >= requiredStableDuration)
            {
                UnlockGate();
            }
        }
        else
        {
            _stableTimer = Mathf.Max(0f, _stableTimer - Time.deltaTime * 2.5f);

            if (hydraulicGlow != null && hydraulicGlow.gameObject.activeSelf)
            {
                hydraulicGlow.gameObject.SetActive(false);
            }

            if (statusBadgeText != null)
            {
                if (p > maxActivationPressure)
                {
                    statusBadgeText.text = $"⚠️ {p:F0} PSI (HIGH)";
                    statusBadgeText.color = overpressureColor;
                }
                else
                {
                    statusBadgeText.text = $"{minActivationPressure:F0}-{maxActivationPressure:F0} PSI";
                    statusBadgeText.color = Color.white;
                }
            }
        }
    }

    public void UnlockGate()
    {
        if (isOpen || _isUnlocking) return;
        _isUnlocking = true;
        isOpen = true;

        if (_openAnimationCoroutine != null) StopCoroutine(_openAnimationCoroutine);
        _openAnimationCoroutine = StartCoroutine(AnimateGateOpenRoutine());
    }

    private IEnumerator AnimateGateOpenRoutine()
    {
        AudioManager.Instance?.PlayWaterFlow();
        ShowFloatingFeedback("<color=#00FFFF>🔓 UNLOCKED</color>");

        // Disable collider so clicks hit the pipe directly
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        float elapsed = 0f;
        float duration = 0.6f;

        Vector3 barsStartScale = (gateBars != null) ? gateBars.transform.localScale : Vector3.one;
        Vector3 barsEndScale = new Vector3(0.1f, barsStartScale.y, barsStartScale.z);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            if (gateBars != null)
            {
                gateBars.transform.localScale = Vector3.Lerp(barsStartScale, barsEndScale, smooth);
                gateBars.color = Color.Lerp(lockedBarsColor, unlockedColor, smooth);
            }

            if (gateBase != null)
            {
                gateBase.color = Color.Lerp(lockedBaseColor, new Color(lockedBaseColor.r, lockedBaseColor.g, lockedBaseColor.b, 0.2f), smooth);
            }

            yield return null;
        }

        if (gateBars != null)
        {
            gateBars.gameObject.SetActive(true);
            gateBars.sprite = Resources.Load<Sprite>("Sprites/icon_unlocked");
            gateBars.color  = new Color(0.2f, 1.0f, 0.45f, 1.0f); // Radiant Emerald Green Unlocked!
            gateBars.sortingOrder = 10;
            gateBars.transform.localScale = Vector3.one * 0.95f;
        }
        if (gateBase != null)
        {
            gateBase.color = new Color(0.3f, 1.0f, 0.7f, 0.45f);
        }
        if (hydraulicGlow != null) hydraulicGlow.gameObject.SetActive(false);
        if (statusBadgeText != null) statusBadgeText.gameObject.SetActive(false);

        _isUnlocking = false;

        // Re-run water flow and win condition check
        var flow = FindObjectOfType<FlowManager>();
        if (flow != null) flow.RunFlow();
        GameManager.Instance?.CheckWinCondition();
    }

    public bool IsNeighborConnected()
    {
        var grid = (GridManager.Instance != null) ? GridManager.Instance : FindObjectOfType<GridManager>();
        if (grid == null) return false;

        var myTile = grid.GetTileAt(gridPos.x, gridPos.y);
        if (myTile != null && myTile.isConnected) return true;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        foreach (var d in dirs)
        {
            Vector2Int np = gridPos + d;
            if (np.x >= 0 && np.x < grid.Cols && np.y >= 0 && np.y < grid.Rows)
            {
                var t = grid.GetTileAt(np.x, np.y);
                if (t != null && t.isConnected) return true;
            }
        }
        return false;
    }

    public void ShowFloatingFeedback(string message)
    {
        if (feedbackText == null) return;
        if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(AnimateFloatingFeedback(message));
    }

    private IEnumerator AnimateFloatingFeedback(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);

        Vector3 startPos = new Vector3(0f, 0.8f, -0.2f);
        Vector3 endPos = new Vector3(0f, 1.3f, -0.2f);
        float elapsed = 0f;
        float duration = 1.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            feedbackText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            feedbackText.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - (t * t)));
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
    }
}
