using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Abyssal Beacon & Bioluminescent Checkpoint mechanic for Ocean Rescuers (World 1 — Deep Ocean ONLY).
/// Represents an ancient deep-sea bioluminescent monolith resting on the ocean floor:
/// - Initially dormant, emitting a subtle resting abyssal glow.
/// - When water successfully routes into or through the beacon's pipeline node, the beacon ignites.
/// - Radiant flare animation illuminates the surrounding bathypelagic abyss.
/// - Triggers dynamic illumination in AbyssalDarknessController.
/// - Provides celebratory visual, auditory, and textual progression feedback.
/// - Integrates seamlessly with Sonar Scan.
/// - Strictly isolated to World 1 (Deep Ocean).
/// </summary>
[DisallowMultipleComponent]
public class AbyssalBeacon : MonoBehaviour
{
    [Header("Beacon Configuration")]
    public Vector2Int gridPos;
    public string beaconName = "Abyssal Sanctuary Beacon";

    [Header("Visual Colors")]
    public Color dormantColor  = new Color(0.20f, 0.60f, 0.90f, 0.50f); // Resting cyan
    public Color ignitedColor  = new Color(0.30f, 1.00f, 0.70f, 0.95f); // Radiant emerald/gold
    public Color coronaColor   = new Color(0.95f, 0.85f, 0.35f, 0.85f); // Ethereal corona

    [Header("Child References")]
    public SpriteRenderer crystalBase;
    public SpriteRenderer beaconCorona;
    public SpriteRenderer pulseRing;
    public TextMeshPro feedbackText;

    // Runtime state
    [SerializeField] private bool _isIgnited = false;
    private float _lastCheckTime = 0f;
    private Coroutine _feedbackCoroutine;
    private Coroutine _flareCoroutine;

    public bool IsIgnited => _isIgnited;

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

    public void SetupComponents()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);
            box.isTrigger = true;
        }
        transform.localScale = new Vector3(0.44f, 0.44f, 1f);

        // 1. CrystalBase
        if (crystalBase == null)
        {
            var cObj = transform.Find("CrystalBase");
            if (cObj == null)
            {
                var go = new GameObject("CrystalBase");
                go.transform.SetParent(transform, false);
                crystalBase = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                crystalBase = cObj.GetComponent<SpriteRenderer>();
            }
        }
        if (crystalBase.sprite == null)
            crystalBase.sprite = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield") 
                              ?? Resources.Load<Sprite>("Sprites/obstacle");
        crystalBase.color = dormantColor;
        crystalBase.sortingOrder = 8;

        // 2. BeaconCorona
        if (beaconCorona == null)
        {
            var coObj = transform.Find("BeaconCorona");
            if (coObj == null)
            {
                var go = new GameObject("BeaconCorona");
                go.transform.SetParent(transform, false);
                beaconCorona = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                beaconCorona = coObj.GetComponent<SpriteRenderer>();
            }
        }
        if (beaconCorona.sprite == null)
            beaconCorona.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") 
                               ?? Resources.Load<Sprite>("Sprites/bubbles");
        beaconCorona.color = coronaColor;
        beaconCorona.sortingOrder = 9;
        beaconCorona.gameObject.SetActive(false);

        // 3. PulseRing
        if (pulseRing == null)
        {
            var pObj = transform.Find("PulseRing");
            if (pObj == null)
            {
                var go = new GameObject("PulseRing");
                go.transform.SetParent(transform, false);
                pulseRing = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                pulseRing = pObj.GetComponent<SpriteRenderer>();
            }
        }
        if (pulseRing.sprite == null)
            pulseRing.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        pulseRing.color = new Color(0.2f, 0.9f, 1f, 0.6f);
        pulseRing.sortingOrder = 7;

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

        // Check for connected water flow every 0.25 seconds
        if (!_isIgnited && Time.unscaledTime - _lastCheckTime > 0.25f)
        {
            _lastCheckTime = Time.unscaledTime;
            CheckWaterActivation();
        }

        // Idle breathing pulse
        if (pulseRing != null)
        {
            float speed = _isIgnited ? 6f : 2.5f;
            float pulse = 0.85f + 0.25f * Mathf.Sin(Time.time * speed);
            pulseRing.transform.localScale = Vector3.one * pulse;
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
        AudioManager.Instance?.PlayButtonClick();
        if (_isIgnited)
        {
            ShowFeedback("<color=#00FFFF>🌟 BEACON ACTIVE</color>");
        }
        else
        {
            ShowFeedback("<color=#38BDF8>✨ ROUTE WATER HERE</color>");
        }
    }

    private void CheckWaterActivation()
    {
        var grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        var tile = grid.GetTileAt(gridPos.x, gridPos.y);
        bool connected = tile != null && tile.isConnected;

        // Also check if any neighbor has connected water
        if (!connected)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            foreach (var d in dirs)
            {
                Vector2Int np = gridPos + d;
                if (np.x >= 0 && np.x < grid.Cols && np.y >= 0 && np.y < grid.Rows)
                {
                    var nt = grid.GetTileAt(np.x, np.y);
                    if (nt != null && nt.isConnected)
                    {
                        connected = true;
                        break;
                    }
                }
            }
        }

        if (connected)
        {
            IgniteBeacon();
        }
    }

    public void IgniteBeacon()
    {
        if (_isIgnited) return;
        _isIgnited = true;

        AudioManager.Instance?.PlayWaterFlow();

        // Illuminate surrounding abyssal darkness
        if (AbyssalDarknessController.Instance != null)
        {
            AbyssalDarknessController.Instance.TemporarilyIlluminate(4.5f);
        }

        if (_flareCoroutine != null) StopCoroutine(_flareCoroutine);
        _flareCoroutine = StartCoroutine(AnimateIgnition());

        ShowFeedback("<color=#00FFFF>🌟 BEACON ACTIVE</color>");
    }

    private IEnumerator AnimateIgnition()
    {
        if (beaconCorona != null) beaconCorona.gameObject.SetActive(true);
        if (crystalBase != null) crystalBase.color = ignitedColor;

        Vector3 startScale = Vector3.one * 0.44f;
        Vector3 flareScale = Vector3.one * 0.65f;
        float elapsed = 0f;
        float dur = 0.6f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            transform.localScale = Vector3.Lerp(startScale, flareScale, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        transform.localScale = startScale;
        UpdateVisuals(immediate: false);
    }

    public void UpdateVisuals(bool immediate)
    {
        if (crystalBase != null)
        {
            crystalBase.color = _isIgnited ? ignitedColor : dormantColor;
        }
        if (beaconCorona != null)
        {
            beaconCorona.gameObject.SetActive(_isIgnited);
        }
        if (pulseRing != null)
        {
            pulseRing.color = _isIgnited ? new Color(0.3f, 1f, 0.7f, 0.85f) : new Color(0.2f, 0.6f, 0.9f, 0.5f);
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
