using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Ancient Seal pedestal mechanic for Ocean Rescuers (World 2 — Lost City ONLY).
/// Represents an interactive stone seal pedestal that unlocks a linked Temple Door:
/// - When water flows to this seal node or player taps it, the ancient seal activates.
/// - Unlocks the linked Temple Door at linkedDoorPos.
/// - Emits glowing golden glyph burst and audio feedback.
/// - Strictly isolated to World 2 (Lost City).
/// </summary>
[DisallowMultipleComponent]
public class AncientSeal : MonoBehaviour
{
    [Header("Seal Configuration")]
    public Vector2Int gridPos;
    public Vector2Int linkedDoorPos = new Vector2Int(-1, -1);
    public string sealName = "Golden Seal";

    [Header("Visual Colors")]
    public Color sealLockedColor   = new Color(1.00f, 0.75f, 0.20f, 0.95f);
    public Color sealUnlockedColor = new Color(0.25f, 1.00f, 0.55f, 0.95f);

    [Header("Child References")]
    public SpriteRenderer pedestalBase;
    public SpriteRenderer sealCrest;
    public SpriteRenderer auraGlow;
    public TextMeshPro feedbackText;

    // Runtime state
    [SerializeField] private bool _isUnlocked = false;
    private float _lastCheckTime = 0f;
    private Coroutine _feedbackCoroutine;

    public bool IsUnlocked => _isUnlocked;

    private void Awake()
    {
        SetupComponents();
    }

    private void Start()
    {
        UpdateVisuals();
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
        transform.localScale = new Vector3(0.40f, 0.40f, 1f);

        // 1. PedestalBase
        if (pedestalBase == null)
        {
            var pObj = transform.Find("PedestalBase");
            if (pObj == null)
            {
                var go = new GameObject("PedestalBase");
                go.transform.SetParent(transform, false);
                pedestalBase = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                pedestalBase = pObj.GetComponent<SpriteRenderer>();
            }
        }
        if (pedestalBase.sprite == null)
            pedestalBase.sprite = Resources.Load<Sprite>("Sprites/obstacle");
        pedestalBase.color = new Color(0.85f, 0.70f, 0.40f, 0.95f);
        pedestalBase.sortingOrder = 7;

        // 2. SealCrest
        if (sealCrest == null)
        {
            var cObj = transform.Find("SealCrest");
            if (cObj == null)
            {
                var go = new GameObject("SealCrest");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                sealCrest = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                sealCrest = cObj.GetComponent<SpriteRenderer>();
            }
        }
        if (sealCrest.sprite == null)
            sealCrest.sprite = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield");
        sealCrest.color = _isUnlocked ? sealUnlockedColor : sealLockedColor;
        sealCrest.sortingOrder = 8;

        // 3. AuraGlow
        if (auraGlow == null)
        {
            var aObj = transform.Find("AuraGlow");
            if (aObj == null)
            {
                var go = new GameObject("AuraGlow");
                go.transform.SetParent(transform, false);
                auraGlow = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                auraGlow = aObj.GetComponent<SpriteRenderer>();
            }
        }
        if (auraGlow.sprite == null)
            auraGlow.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        auraGlow.color = new Color(0.2f, 0.9f, 1f, 0.5f);
        auraGlow.sortingOrder = 6;
        auraGlow.gameObject.SetActive(_isUnlocked);

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

        // Check if water flow is connected
        if (!_isUnlocked && Time.unscaledTime - _lastCheckTime > 0.25f)
        {
            _lastCheckTime = Time.unscaledTime;
            EvaluateWaterFlow();
        }

        // Idle emblem pulse
        if (sealCrest != null)
        {
            float speed = _isUnlocked ? 4f : 1.5f;
            float pulse = 0.9f + 0.1f * Mathf.Sin(Time.time * speed);
            sealCrest.transform.localScale = Vector3.one * pulse;
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
                        OnSealTapped();
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
                    OnSealTapped();
                    break;
                }
            }
        }
    }

    private void OnSealTapped()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (!_isUnlocked)
        {
            UnlockSeal();
        }
        else
        {
            ShowFeedback($"<color=#00FFFF>🛡️ {sealName.ToUpper()} UNSEALED</color>\n<size=75%>Water Channel Active</size>");
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
            // Check if water reached any adjacent pipe
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

        if (connected)
        {
            UnlockSeal();
        }
    }

    public void UnlockSeal()
    {
        if (_isUnlocked) return;
        _isUnlocked = true;

        UpdateVisuals();
        AudioManager.Instance?.PlayWaterFlow();
        ShowFeedback($"<color=#00FFFF>🛡️ {sealName.ToUpper()} UNLOCKED!</color>\n<size=75%>Water Flow Channeled</size>");

        // Trigger linked temple door
        if (linkedDoorPos.x >= 0 && linkedDoorPos.y >= 0)
        {
            var doors = FindObjectsOfType<TempleDoor>();
            foreach (var d in doors)
            {
                if (d.gridPos == linkedDoorPos)
                {
                    d.SetDoorOpen(true);
                    break;
                }
            }
        }

        var flow = FlowManager.Instance ?? FindObjectOfType<FlowManager>();
        if (flow != null) flow.RunFlow();
    }

    public void UpdateVisuals()
    {
        if (sealCrest != null)
        {
            sealCrest.sprite = Resources.Load<Sprite>(_isUnlocked ? "Sprites/icon_unlocked" : "Sprites/icon_locked_gold_shield");
            sealCrest.color = _isUnlocked ? new Color(0.2f, 1.0f, 0.45f, 1.0f) : sealLockedColor;
            sealCrest.sortingOrder = 10;
            sealCrest.transform.localScale = _isUnlocked ? Vector3.one * 1.15f : Vector3.one;
        }
        if (pedestalBase != null)
        {
            pedestalBase.color = _isUnlocked ? new Color(0.3f, 1.0f, 0.7f, 0.7f) : new Color(0.85f, 0.70f, 0.40f, 0.95f);
        }
        if (auraGlow != null)
        {
            auraGlow.gameObject.SetActive(_isUnlocked);
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
