using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Underwater Tremor & Pipe Instability mechanic for Ocean Rescuers (World 1 — Deep Ocean ONLY).
/// Simulates unstable bathypelagic trench tectonic tremors:
/// - Predictable cyclic state: Dormant -> Warning (2s) -> Active Tremor (2.5s).
/// - During Warning: Audible rumble alert, pulsating amber fissure ring, and floating warning notice.
/// - During Active Tremor: Localized vibration effect, underwater silt disturbance, and a temporary
///   +8 PSI hydraulic backpressure surge registered in DeepOceanPressureManager.
/// - Tactical Player Counterplay: Tapping the unstable tremor zone during the Warning phase allows
///   the player to strategically stabilize the trench, preventing the pressure spike!
/// - Integrates seamlessly with Sonar Scan.
/// - Does NOT randomly rotate pipes or consume moves unfairly.
/// - Strictly isolated to World 1 (Deep Ocean).
/// </summary>
[DisallowMultipleComponent]
public class TremorZone : MonoBehaviour
{
    public enum TremorState { Dormant, Warning, Active }

    [Header("Tremor Configuration")]
    public Vector2Int gridPos;

    [Tooltip("Cycle interval between tremors (seconds)")]
    public float cycleInterval = 12.0f;

    [Tooltip("Duration of the warning rumble before tremor activates")]
    public float warningDuration = 2.5f;

    [Tooltip("Duration of the active tremor phase")]
    public float tremorDuration = 3.0f;

    [Tooltip("Hydraulic pressure surge (PSI) inflicted during active tremor")]
    public float pressureSurge = 8.0f;

    [Header("Visual Feedback Colors")]
    public Color fissureColor = new Color(0.85f, 0.45f, 0.15f, 0.90f);
    public Color warningPulseColor = new Color(1.0f, 0.75f, 0.20f, 0.85f);
    public Color activeTremorColor = new Color(1.0f, 0.25f, 0.15f, 0.95f);

    [Header("Child References")]
    public SpriteRenderer fissureOverlay;
    public SpriteRenderer warningRing;
    public SpriteRenderer tremorBurst;
    public TextMeshPro feedbackText;

    // Runtime state
    [SerializeField] private TremorState _currentState = TremorState.Dormant;
    private float _stateTimer = 0f;
    private bool _isStabilizedThisCycle = false;
    private Vector3 _originalLocalPos;
    private Coroutine _feedbackCoroutine;

    public TremorState CurrentState => _currentState;
    public bool IsActive => _currentState == TremorState.Active;

    private void Awake()
    {
        _originalLocalPos = transform.localPosition;
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

        _stateTimer = cycleInterval * 0.5f; // Initial offset so tremor doesn't trigger on start
        UpdateVisuals();
    }

    private void OnDestroy()
    {
        ResetPressureSurge();
    }

    private void OnDisable()
    {
        ResetPressureSurge();
    }

    public void SetupComponents()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);
            box.isTrigger = true;
        }
        transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        // 1. FissureOverlay
        if (fissureOverlay == null)
        {
            var fObj = transform.Find("FissureOverlay");
            if (fObj == null)
            {
                var go = new GameObject("FissureOverlay");
                go.transform.SetParent(transform, false);
                fissureOverlay = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                fissureOverlay = fObj.GetComponent<SpriteRenderer>();
            }
        }
        if (fissureOverlay.sprite == null)
            fissureOverlay.sprite = Resources.Load<Sprite>("Sprites/obstacle") ?? Resources.Load<Sprite>("Sprites/red_obstacle");
        fissureOverlay.color = fissureColor;
        fissureOverlay.sortingOrder = 7;

        // 2. WarningRing
        if (warningRing == null)
        {
            var wObj = transform.Find("WarningRing");
            if (wObj == null)
            {
                var go = new GameObject("WarningRing");
                go.transform.SetParent(transform, false);
                warningRing = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                warningRing = wObj.GetComponent<SpriteRenderer>();
            }
        }
        if (warningRing.sprite == null)
            warningRing.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        warningRing.color = warningPulseColor;
        warningRing.sortingOrder = 8;
        warningRing.gameObject.SetActive(false);

        // 3. TremorBurst
        if (tremorBurst == null)
        {
            var tObj = transform.Find("TremorBurst");
            if (tObj == null)
            {
                var go = new GameObject("TremorBurst");
                go.transform.SetParent(transform, false);
                tremorBurst = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                tremorBurst = tObj.GetComponent<SpriteRenderer>();
            }
        }
        if (tremorBurst.sprite == null)
            tremorBurst.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") ?? Resources.Load<Sprite>("Sprites/bubbles");
        tremorBurst.color = activeTremorColor;
        tremorBurst.sortingOrder = 9;
        tremorBurst.gameObject.SetActive(false);

        // 4. FeedbackText
        if (feedbackText == null)
        {
            var fbObj = transform.Find("FeedbackText");
            if (fbObj == null)
            {
                var go = new GameObject("FeedbackText");
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
        if (SaveManager.GetSelectedWorld() != 1) return;

        HandleInput();
        UpdateStateCycle();
    }

    private void UpdateStateCycle()
    {
        _stateTimer += Time.deltaTime;

        switch (_currentState)
        {
            case TremorState.Dormant:
                if (_stateTimer >= cycleInterval)
                {
                    _stateTimer = 0f;
                    _isStabilizedThisCycle = false;
                    EnterWarningState();
                }
                break;

            case TremorState.Warning:
                // Animate warning ring pulsation
                if (warningRing != null)
                {
                    float pulse = 1f + 0.35f * Mathf.Sin(Time.time * 8f);
                    warningRing.transform.localScale = Vector3.one * pulse;
                }

                if (_stateTimer >= warningDuration)
                {
                    _stateTimer = 0f;
                    if (!_isStabilizedThisCycle)
                    {
                        EnterActiveState();
                    }
                    else
                    {
                        EnterDormantState();
                    }
                }
                break;

            case TremorState.Active:
                // Localized vibration shake
                float shake = Mathf.Sin(Time.time * 30f) * 0.06f;
                transform.localPosition = _originalLocalPos + new Vector3(shake, -shake * 0.5f, 0f);

                if (tremorBurst != null)
                {
                    float burstScale = 0.8f + 0.3f * Mathf.Sin(Time.time * 12f);
                    tremorBurst.transform.localScale = Vector3.one * burstScale;
                }

                if (_stateTimer >= tremorDuration)
                {
                    _stateTimer = 0f;
                    transform.localPosition = _originalLocalPos;
                    EnterDormantState();
                }
                break;
        }
    }

    private void EnterWarningState()
    {
        _currentState = TremorState.Warning;
        AudioManager.Instance?.PlayButtonClick();
        if (warningRing != null) warningRing.gameObject.SetActive(true);
        ShowFeedback("<color=#FFA500>⚠️ SEABED TREMOR IMMINENT!</color>\n<size=75%>Tap trench to stabilize</size>");
    }

    private void EnterActiveState()
    {
        _currentState = TremorState.Active;
        AudioManager.Instance?.PlayWaterFlow();
        if (warningRing != null) warningRing.gameObject.SetActive(false);
        if (tremorBurst != null) tremorBurst.gameObject.SetActive(true);

        // Apply pressure surge
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.VentPressure(-pressureSurge); // Negative vent = pressure surge
        }

        ShowFeedback($"<color=#FF4444>🌋 TRENCH TREMOR ACTIVE!</color>\n<size=75%>Pressure Surge: +{pressureSurge:F0} PSI</size>");
    }

    private void EnterDormantState()
    {
        _currentState = TremorState.Dormant;
        ResetPressureSurge();
        UpdateVisuals();
    }

    private void ResetPressureSurge()
    {
        if (transform != null) transform.localPosition = _originalLocalPos;
        if (warningRing != null) warningRing.gameObject.SetActive(false);
        if (tremorBurst != null) tremorBurst.gameObject.SetActive(false);
        if (fissureOverlay != null) fissureOverlay.color = fissureColor;
    }

    private void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && Camera.main != null)
            {
                Vector2 wp = Camera.main.ScreenToWorldPoint(touch.position);
                var hits = Physics2D.OverlapPointAll(wp);
                for (int i = 0; i < hits.Length; i++)
                {
                    var hit = hits[i];
                    if (hit != null && (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)))
                    {
                        OnTapped();
                        break;
                    }
                }
            }
        }
        else if (Input.GetMouseButtonDown(0) && Camera.main != null)
        {
            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hits = Physics2D.OverlapPointAll(wp);
            for (int i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit != null && (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)))
                {
                    OnTapped();
                    break;
                }
            }
        }
    }

    private void OnTapped()
    {
        if (_currentState == TremorState.Warning && !_isStabilizedThisCycle)
        {
            _isStabilizedThisCycle = true;
            AudioManager.Instance?.PlayWaterFlow();
            if (warningRing != null) warningRing.gameObject.SetActive(false);
            ShowFeedback("<color=#00FFFF>🛡️ TRENCH STABILIZED!</color>\n<size=75%>Tremor Diverted</size>");
        }
        else if (_currentState == TremorState.Active)
        {
            ShowFeedback("<color=#FFA500>⚠️ SEABED UNSTABLE</color>\n<size=75%>Await Stabilization</size>");
        }
    }

    private void UpdateVisuals()
    {
        if (fissureOverlay != null) fissureOverlay.color = fissureColor;
        if (warningRing != null) warningRing.gameObject.SetActive(_currentState == TremorState.Warning);
        if (tremorBurst != null) tremorBurst.gameObject.SetActive(_currentState == TremorState.Active);
    }

    private void ShowFeedback(string msg)
    {
        if (feedbackText == null) return;
        if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(AnimateFeedback(msg));
    }

    private IEnumerator AnimateFeedback(string msg)
    {
        feedbackText.text = msg;
        feedbackText.gameObject.SetActive(true);

        Vector3 startPos = new Vector3(0f, 0.8f, -0.2f);
        Vector3 endPos = new Vector3(0f, 1.25f, -0.2f);
        float elapsed = 0f;
        float dur = 1.3f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            feedbackText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            feedbackText.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - (t * t)));
            yield return null;
        }

        feedbackText.gameObject.SetActive(false);
    }
}
