using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Abyssal Route Diversion mechanic for Ocean Rescuers (World 1 — Deep Ocean ONLY).
/// Simulates hydrothermal fissures and abyssal thermal vents that restrict pipeline routing:
/// - Enforces a directional routing restriction (e.g. water cannot exit or enter in a restricted direction).
/// - Challenges the player to select tactical alternate routes rather than simple straight paths.
/// - Clearly communicates the restricted hazard direction and the safe bypass route with visual chevrons.
/// - Registers with FlowManager to validate flow connectivity during BFS propagation.
/// - Integrates seamlessly with Sonar Scan.
/// - Strictly isolated to World 1 (Deep Ocean).
/// </summary>
[DisallowMultipleComponent]
public class RouteDiversion : MonoBehaviour
{
    [Header("Diversion Configuration")]
    public Vector2Int gridPos;

    [Tooltip("Direction restricted by this thermal hazard: 0=Up, 1=Right, 2=Down, 3=Left")]
    public int restrictedDirection = 0;

    [Tooltip("Label for this hazard zone")]
    public string hazardName = "Hydrothermal Vent";

    [Header("Visual Colors")]
    public Color hazardColor = new Color(1.0f, 0.40f, 0.10f, 0.95f);    // Scorching amber/red
    public Color bypassSafeColor = new Color(0.20f, 0.90f, 1.0f, 0.85f); // Safe oceanic cyan
    public Color activeFlowColor = new Color(0.25f, 1.0f, 0.55f, 0.95f); // Flowing emerald

    [Header("Child References")]
    public SpriteRenderer ventBase;
    public SpriteRenderer warningChevron;
    public SpriteRenderer bypassIndicator;
    public TextMeshPro feedbackText;

    private Coroutine _feedbackCoroutine;
    private bool _isFlowActive = false;

    public bool IsFlowActive => _isFlowActive;

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

        var flow = FlowManager.Instance;
        if (flow != null)
        {
            flow.RegisterDiversion(this);
        }

        UpdateVisuals();
    }

    private void OnDestroy()
    {
        var flow = FlowManager.Instance;
        if (flow != null)
        {
            flow.UnregisterDiversion(this);
        }
    }

    private void OnDisable()
    {
        var flow = FlowManager.Instance;
        if (flow != null)
        {
            flow.UnregisterDiversion(this);
        }
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

        // 1. VentBase
        if (ventBase == null)
        {
            var bObj = transform.Find("VentBase");
            if (bObj == null)
            {
                var go = new GameObject("VentBase");
                go.transform.SetParent(transform, false);
                ventBase = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                ventBase = bObj.GetComponent<SpriteRenderer>();
            }
        }
        if (ventBase.sprite == null)
            ventBase.sprite = Resources.Load<Sprite>("Sprites/obstacle") ?? Resources.Load<Sprite>("Sprites/red_obstacle");
        ventBase.color = hazardColor;
        ventBase.sortingOrder = 7;

        // 2. WarningChevron (Points toward the restricted direction)
        if (warningChevron == null)
        {
            var cObj = transform.Find("WarningChevron");
            if (cObj == null)
            {
                var go = new GameObject("WarningChevron");
                go.transform.SetParent(transform, false);
                warningChevron = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                warningChevron = cObj.GetComponent<SpriteRenderer>();
            }
        }
        if (warningChevron.sprite == null)
            warningChevron.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") ?? Resources.Load<Sprite>("Sprites/bubbles");
        warningChevron.color = new Color(1f, 0.2f, 0.2f, 0.9f);
        warningChevron.sortingOrder = 8;
        warningChevron.transform.localRotation = Quaternion.Euler(0f, 0f, -90f * restrictedDirection);

        // 3. BypassIndicator (Soft cyan glow indicating alternate bypass channel)
        if (bypassIndicator == null)
        {
            var pObj = transform.Find("BypassIndicator");
            if (pObj == null)
            {
                var go = new GameObject("BypassIndicator");
                go.transform.SetParent(transform, false);
                bypassIndicator = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                bypassIndicator = pObj.GetComponent<SpriteRenderer>();
            }
        }
        if (bypassIndicator.sprite == null)
            bypassIndicator.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        bypassIndicator.color = bypassSafeColor;
        bypassIndicator.sortingOrder = 9;

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

        // Pulsate warning chevron
        if (warningChevron != null)
        {
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * 6f);
            warningChevron.transform.localScale = Vector3.one * pulse;
        }
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
        string dirStr = restrictedDirection switch
        {
            0 => "North (Up)",
            1 => "East (Right)",
            2 => "South (Down)",
            _ => "West (Left)"
        };

        AudioManager.Instance?.PlayButtonClick();
        ShowFeedback($"<color=#FFA500>🌋 {hazardName.ToUpper()}</color>\n<size=75%>Flow Blocked: {dirStr}\nRoute Around Hazard</size>");
    }

    /// <summary>
    /// Returns false if water flow attempts to exit in the restricted direction.
    /// </summary>
    public bool CanExitFlow(int exitDirection)
    {
        return exitDirection != restrictedDirection;
    }

    /// <summary>
    /// Returns false if water flow attempts to enter from the opposite of restricted direction.
    /// </summary>
    public bool CanEnterFlow(int enterDirection)
    {
        return enterDirection != ((restrictedDirection + 2) % 4);
    }

    public void SetFlowActive(bool active)
    {
        _isFlowActive = active;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (bypassIndicator != null)
        {
            bypassIndicator.color = _isFlowActive ? activeFlowColor : bypassSafeColor;
        }
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
