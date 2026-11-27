using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Ancient Rune component for Ocean Rescuers (World 2 — Lost City ONLY).
/// Represents an ancient sunken runic obelisk:
/// - When water flows through the underlying pipe tile, the rune channels aquatic magic and becomes ENERGIZED.
/// - Emits radiant glowing glyph flare, expanding magical energy rings, and an ancient harmonic chime.
/// - Unlocks linked Temple Doors and activates Ancient Stone Bridges sharing the same runeId.
/// - Tap interaction displays tactical rune status.
/// - Strictly isolated to World 2 (Lost City).
/// </summary>
[DisallowMultipleComponent]
public class AncientRune : MonoBehaviour
{
    public static event System.Action<int, bool> OnRuneStateChanged;

    [Header("Rune Configuration")]
    public Vector2Int gridPos;
    public int runeId = 0;
    public string runeName = "Solar Rune";

    [Header("Visual Colors (Inspector Editable)")]
    public Color runeDormantColor = new Color(0.45f, 0.40f, 0.30f, 0.65f); // Dormant weathered stone
    public Color runeActiveColor  = new Color(1.00f, 0.85f, 0.20f, 0.95f); // Radiant gold energy
    public Color auraColor        = new Color(0.25f, 0.95f, 1.00f, 0.75f); // Mystical cyan halo

    [Header("Child References")]
    public SpriteRenderer runeBase;
    public SpriteRenderer glyphSymbol;
    public SpriteRenderer auraRing;
    public TextMeshPro feedbackText;

    // Runtime state
    [SerializeField] private bool _isEnergized = false;
    private float _lastCheckTime = 0f;
    private Coroutine _feedbackCoroutine;
    private Coroutine _flareCoroutine;

    public bool IsEnergized => _isEnergized;

    private static readonly Dictionary<int, bool> _activeRuneStates = new Dictionary<int, bool>();

    public static bool IsRuneEnergized(int id)
    {
        return _activeRuneStates.TryGetValue(id, out bool state) && state;
    }

    public static void ResetRuneStates()
    {
        _activeRuneStates.Clear();
    }

    private void Awake()
    {
        SetupComponents();
    }

    private void Start()
    {
        UpdateVisuals(immediate: true);
    }

    private void OnDestroy()
    {
        _activeRuneStates.Remove(runeId);
    }

    private void OnDisable()
    {
        _activeRuneStates.Remove(runeId);
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

        // 1. RuneBase (Carved Ancient Stone)
        if (runeBase == null)
        {
            var bObj = transform.Find("RuneBase");
            if (bObj == null)
            {
                var go = new GameObject("RuneBase");
                go.transform.SetParent(transform, false);
                runeBase = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                runeBase = bObj.GetComponent<SpriteRenderer>();
            }
        }
        if (runeBase.sprite == null)
            runeBase.sprite = Resources.Load<Sprite>("Sprites/rocks_blue") ?? Resources.Load<Sprite>("Sprites/obstacle");
        runeBase.color = new Color(0.85f, 0.75f, 0.50f, 0.95f); // Ancient golden sandstone
        runeBase.sortingOrder = 7;

        // 2. GlyphSymbol (Ancient Mystical Inscription)
        if (glyphSymbol == null)
        {
            var gObj = transform.Find("GlyphSymbol");
            if (gObj == null)
            {
                var go = new GameObject("GlyphSymbol");
                go.transform.SetParent(transform, false);
                glyphSymbol = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                glyphSymbol = gObj.GetComponent<SpriteRenderer>();
            }
        }
        if (glyphSymbol.sprite == null)
            glyphSymbol.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") ?? Resources.Load<Sprite>("Sprites/icon_unlocked");
        glyphSymbol.color = _isEnergized ? runeActiveColor : runeDormantColor;
        glyphSymbol.sortingOrder = 8;

        // 3. AuraRing (Pulsing Energy Halo)
        if (auraRing == null)
        {
            var aObj = transform.Find("AuraRing");
            if (aObj == null)
            {
                var go = new GameObject("AuraRing");
                go.transform.SetParent(transform, false);
                auraRing = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                auraRing = aObj.GetComponent<SpriteRenderer>();
            }
        }
        if (auraRing.sprite == null)
            auraRing.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        auraRing.color = auraColor;
        auraRing.sortingOrder = 6;
        auraRing.gameObject.SetActive(_isEnergized);

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
        if (Time.unscaledTime - _lastCheckTime > 0.2f)
        {
            _lastCheckTime = Time.unscaledTime;
            EvaluateWaterFlow();
        }

        // Idle pulsating glow
        if (glyphSymbol != null)
        {
            float speed = _isEnergized ? 5f : 1.8f;
            float pulse = 0.85f + 0.15f * Mathf.Sin(Time.time * speed);
            glyphSymbol.transform.localScale = Vector3.one * pulse;
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
        if (!_isEnergized)
        {
            SetEnergized(true);
        }
        else
        {
            ShowFeedback($"<color=#FFD700>ᚱ {runeName.ToUpper()}</color>\n<size=75%>Status: Energized & Channeled</size>");
        }

        RotatePipeUnderneath();
    }

    private void RotatePipeUnderneath()
    {
        var grid = FindObjectOfType<GridManager>();
        if (grid != null)
        {
            var pipe = grid.GetTileAt(gridPos.x, gridPos.y);
            if (pipe != null && !pipe.isLocked)
            {
                pipe.Rotate();
            }
        }
    }

    private void EvaluateWaterFlow()
    {
        var grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        var tile = grid.GetTileAt(gridPos.x, gridPos.y);
        bool connected = tile != null && tile.isConnected;

        if (!connected)
        {
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            foreach (var d in dirs)
            {
                var np = gridPos + d;
                var neighbor = grid.GetTileAt(np.x, np.y);
                if (neighbor != null && neighbor.isConnected)
                {
                    connected = true;
                    break;
                }
            }
        }

        if (connected && !_isEnergized)
        {
            SetEnergized(true);
        }
    }

    public void SetEnergized(bool energized)
    {
        if (_isEnergized == energized) return;
        _isEnergized = energized;
        _activeRuneStates[runeId] = energized;

        UpdateVisuals(immediate: false);

        if (_isEnergized)
        {
            AudioManager.Instance?.PlayWaterFlow();
            ShowFeedback($"<color=#00FF66>🔓 {runeName.ToUpper()} UNLOCKED!</color>\n<size=75%>Ancient Power Channeled</size>");
        }

        OnRuneStateChanged?.Invoke(runeId, _isEnergized);

        // Re-evaluate puzzle flow
        var flow = FlowManager.Instance ?? FindObjectOfType<FlowManager>();
        if (flow != null) flow.RunFlow();
    }

    public void UpdateVisuals(bool immediate)
    {
        if (glyphSymbol != null)
        {
            if (_isEnergized)
            {
                glyphSymbol.sprite = Resources.Load<Sprite>("Sprites/icon_unlocked");
                glyphSymbol.color = new Color(0.2f, 1.0f, 0.45f, 1.0f); // Radiant Emerald Green Unlocked!
                glyphSymbol.sortingOrder = 10;
                glyphSymbol.transform.localScale = Vector3.one * 1.15f;
            }
            else
            {
                glyphSymbol.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") ?? Resources.Load<Sprite>("Sprites/icon_locked");
                glyphSymbol.color = runeDormantColor;
                glyphSymbol.sortingOrder = 8;
                glyphSymbol.transform.localScale = Vector3.one;
            }
        }

        if (runeBase != null)
        {
            runeBase.color = _isEnergized ? new Color(0.3f, 1.0f, 0.7f, 0.7f) : new Color(0.85f, 0.75f, 0.50f, 0.95f);
        }

        if (auraRing != null)
        {
            auraRing.gameObject.SetActive(_isEnergized);
        }

        if (!immediate && _isEnergized)
        {
            if (_flareCoroutine != null) StopCoroutine(_flareCoroutine);
            _flareCoroutine = StartCoroutine(AnimateFlare());
        }
    }

    private IEnumerator AnimateFlare()
    {
        Vector3 baseScale = new Vector3(0.42f, 0.42f, 1f);
        Vector3 flareScale = new Vector3(0.55f, 0.55f, 1f);
        float elapsed = 0f;
        float duration = 0.4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
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
