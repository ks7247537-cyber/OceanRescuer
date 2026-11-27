using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Ancient Energy Relic mechanic for Ocean Rescuers (World 2 — Lost City ONLY).
/// Represents a sacred ancient artifact resting on an altar in the ruins:
/// - When water flows to or through the relic node, it absorbs aquatic energy and becomes CHARGED.
/// - Grants immediate bonus moves to the player's move limit via MoveManager.Instance.AddExtraMoves().
/// - Emits a radiant golden flare animation, mystical chime, and floating celebratory feedback.
/// - Strictly isolated to World 2 (Lost City).
/// </summary>
[DisallowMultipleComponent]
public class EnergyRelic : MonoBehaviour
{
    [Header("Relic Configuration")]
    public Vector2Int gridPos;
    public string relicName = "Sunken Golden Trident";
    public int bonusMoves = 3;

    [Header("Visual Colors")]
    public Color relicDormantColor = new Color(0.85f, 0.70f, 0.35f, 0.70f);
    public Color relicChargedColor = new Color(1.00f, 0.90f, 0.30f, 1.00f);
    public Color energyFlareColor  = new Color(0.20f, 0.95f, 1.00f, 0.85f);

    [Header("Child References")]
    public SpriteRenderer altarBase;
    public SpriteRenderer relicIcon;
    public SpriteRenderer energyFlare;
    public TextMeshPro feedbackText;

    // Runtime state
    [SerializeField] private bool _isCharged = false;
    private float _lastCheckTime = 0f;
    private Coroutine _feedbackCoroutine;
    private Coroutine _flareCoroutine;

    public bool IsCharged => _isCharged;

    private void Awake()
    {
        SetupComponents();
    }

    private void Start()
    {
        UpdateVisuals(immediate: true);
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

        // 1. AltarBase
        if (altarBase == null)
        {
            var aObj = transform.Find("AltarBase");
            if (aObj == null)
            {
                var go = new GameObject("AltarBase");
                go.transform.SetParent(transform, false);
                altarBase = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                altarBase = aObj.GetComponent<SpriteRenderer>();
            }
        }
        if (altarBase.sprite == null)
            altarBase.sprite = Resources.Load<Sprite>("Sprites/obstacle");
        altarBase.color = new Color(0.75f, 0.65f, 0.40f, 0.95f);
        altarBase.sortingOrder = 7;

        // 2. RelicIcon
        if (relicIcon == null)
        {
            var rObj = transform.Find("RelicIcon");
            if (rObj == null)
            {
                var go = new GameObject("RelicIcon");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.1f, -0.05f);
                relicIcon = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                relicIcon = rObj.GetComponent<SpriteRenderer>();
            }
        }
        if (relicIcon.sprite == null)
            relicIcon.sprite = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield") 
                            ?? Resources.Load<Sprite>("Sprites/powerup_lightning");
        relicIcon.color = _isCharged ? relicChargedColor : relicDormantColor;
        relicIcon.sortingOrder = 8;

        // 3. EnergyFlare
        if (energyFlare == null)
        {
            var fObj = transform.Find("EnergyFlare");
            if (fObj == null)
            {
                var go = new GameObject("EnergyFlare");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.1f, -0.08f);
                energyFlare = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                energyFlare = fObj.GetComponent<SpriteRenderer>();
            }
        }
        if (energyFlare.sprite == null)
            energyFlare.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") 
                              ?? Resources.Load<Sprite>("Sprites/bubbles");
        energyFlare.color = energyFlareColor;
        energyFlare.sortingOrder = 9;
        energyFlare.gameObject.SetActive(false);

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
        HandleInput();

        // Check water connectivity periodically
        if (!_isCharged && Time.unscaledTime - _lastCheckTime > 0.25f)
        {
            _lastCheckTime = Time.unscaledTime;
            EvaluateWaterFlow();
        }

        // Idle pulsation
        if (relicIcon != null)
        {
            float speed = _isCharged ? 5f : 1.6f;
            float pulse = 0.9f + 0.1f * Mathf.Sin(Time.time * speed);
            relicIcon.transform.localScale = Vector3.one * pulse;
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
                        OnRelicTapped();
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
                    OnRelicTapped();
                    break;
                }
            }
        }
    }

    private void OnRelicTapped()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (_isCharged)
        {
            ShowFeedback($"<color=#FFD700>🏆 {relicName.ToUpper()}</color>\n<size=75%>Status: Charged (+{bonusMoves} Moves Claimed)</size>");
        }
        else
        {
            ShowFeedback($"<color=#38BDF8>🏆 {relicName.ToUpper()}</color>\n<size=75%>Route water here for +{bonusMoves} Moves!</size>");
        }
    }

    private void EvaluateWaterFlow()
    {
        var grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        var tile = grid.GetTileAt(gridPos.x, gridPos.y);
        if (tile != null && tile.isConnected)
        {
            ChargeRelic();
        }
    }

    public void ChargeRelic()
    {
        if (_isCharged) return;
        _isCharged = true;

        UpdateVisuals(immediate: false);

        // Grant bonus moves
        if (MoveManager.Instance != null && bonusMoves > 0)
        {
            MoveManager.Instance.AddExtraMoves(bonusMoves);
        }

        AudioManager.Instance?.PlayWaterFlow();
        ShowFeedback($"<color=#FFD700>🏆 {relicName.ToUpper()} CHARGED!</color>\n<size=75%>+{bonusMoves} Bonus Moves Granted</size>");
    }

    public void UpdateVisuals(bool immediate)
    {
        if (relicIcon != null)
        {
            if (_isCharged)
            {
                relicIcon.sprite = Resources.Load<Sprite>("Sprites/icon_unlocked");
                relicIcon.color  = new Color(0.2f, 1.0f, 0.45f, 1.0f);
                relicIcon.sortingOrder = 10;
                relicIcon.transform.localScale = Vector3.one * 1.15f;
            }
            else
            {
                relicIcon.color = relicDormantColor;
                relicIcon.sortingOrder = 8;
                relicIcon.transform.localScale = Vector3.one;
            }
        }
        if (energyFlare != null)
        {
            energyFlare.gameObject.SetActive(_isCharged);
        }

        if (!immediate && _isCharged)
        {
            if (_flareCoroutine != null) StopCoroutine(_flareCoroutine);
            _flareCoroutine = StartCoroutine(AnimateCharge());
        }
    }

    private IEnumerator AnimateCharge()
    {
        Vector3 baseScale = new Vector3(0.42f, 0.42f, 1f);
        Vector3 flareScale = new Vector3(0.58f, 0.58f, 1f);
        float elapsed = 0f;
        float dur = 0.5f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            transform.localScale = Vector3.Lerp(baseScale, flareScale, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        transform.localScale = baseScale;
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
