using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Ancient Stone Bridge mechanic for Ocean Rescuers (World 2 — Lost City ONLY).
/// Represents an ancient sunken aqueduct or stone bridge across a ruined chasm:
/// - Initially raised / disconnected (isOpen = false), blocking water flow across the gap.
/// - Lowers into alignment when its linked Ancient Rune is energized (or when water reaches its threshold).
/// - Inherits from GateComponent so FlowManager seamlessly treats it as an openable obstacle.
/// - When lowered, stone slabs slide into place, allowing water to cross into the next corridor.
/// - Strictly isolated to World 2 (Lost City).
/// </summary>
[DisallowMultipleComponent]
public class StoneBridge : GateComponent
{
    [Header("Stone Bridge Configuration")]
    public int activationRuneId = -1;
    public string bridgeName = "Ancient Stone Bridge";

    [Header("Visual References")]
    public SpriteRenderer bridgeSpan;
    public SpriteRenderer waterRivulet;
    public SpriteRenderer anchorLeft;
    public SpriteRenderer anchorRight;
    public TextMeshPro statusText;

    private Coroutine _bridgeAnimCoroutine;
    private Coroutine _feedbackCoroutine;

    private void Awake()
    {
        gateType = GateType.StoneBridge;
        SetupBridgeComponents();
    }

    private void OnEnable()
    {
        AncientRune.OnRuneStateChanged += HandleRuneStateChanged;
    }

    private void OnDisable()
    {
        AncientRune.OnRuneStateChanged -= HandleRuneStateChanged;
    }

    private void Start()
    {
        // Check if activation rune is already energized
        if (activationRuneId >= 0 && AncientRune.IsRuneEnergized(activationRuneId))
        {
            SetBridgeLowered(true, immediate: true);
        }
        else
        {
            SetBridgeLowered(isOpen, immediate: true);
        }
    }

    private void SetupBridgeComponents()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);
        }
        transform.localScale = new Vector3(0.44f, 0.44f, 1f);

        // 1. BridgeSpan (Stone Aqueduct Arch)
        if (bridgeSpan == null)
        {
            var sObj = transform.Find("BridgeSpan");
            if (sObj == null)
            {
                var go = new GameObject("BridgeSpan");
                go.transform.SetParent(transform, false);
                bridgeSpan = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                bridgeSpan = sObj.GetComponent<SpriteRenderer>();
            }
        }
        if (bridgeSpan.sprite == null)
            bridgeSpan.sprite = Resources.Load<Sprite>("Sprites/obstacle") ?? Resources.Load<Sprite>("Sprites/rocks_blue");
        bridgeSpan.color = new Color(0.80f, 0.70f, 0.45f, 0.95f);
        bridgeSpan.sortingOrder = 7;

        // 2. WaterRivulet (Flowing Aqueduct Trench)
        if (waterRivulet == null)
        {
            var rObj = transform.Find("WaterRivulet");
            if (rObj == null)
            {
                var go = new GameObject("WaterRivulet");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                waterRivulet = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                waterRivulet = rObj.GetComponent<SpriteRenderer>();
            }
        }
        if (waterRivulet.sprite == null)
            waterRivulet.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        waterRivulet.color = new Color(0.2f, 0.9f, 1f, 0.85f);
        waterRivulet.sortingOrder = 8;
        waterRivulet.gameObject.SetActive(isOpen);

        // 3. StatusText
        if (statusText == null)
        {
            var stObj = transform.Find("StatusText");
            if (stObj == null)
            {
                var go = new GameObject("StatusText");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.95f, -0.2f);
                statusText = go.AddComponent<TextMeshPro>();
            }
            else
            {
                statusText = stObj.GetComponent<TextMeshPro>();
            }
        }
        statusText.fontSize = 2.4f;
        statusText.fontStyle = FontStyles.Bold;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.sortingOrder = 22;
        statusText.gameObject.SetActive(false);
    }

    private void Update()
    {
        HandleInput();
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
                        OnBridgeTapped();
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
                    OnBridgeTapped();
                    break;
                }
            }
        }
    }

    private void OnBridgeTapped()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (!isOpen)
        {
            SetBridgeLowered(true, immediate: false);
        }
        else
        {
            ShowFeedback($"<color=#00FFFF>🌉 {bridgeName.ToUpper()} LOWERED</color>\n<size=75%>Chasm Crossed & Connected</size>");
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

    private void HandleRuneStateChanged(int runeId, bool energized)
    {
        if (activationRuneId >= 0 && runeId == activationRuneId)
        {
            SetBridgeLowered(energized, immediate: false);
        }
    }

    public void SetBridgeLowered(bool lowered, bool immediate = false)
    {
        if (isOpen == lowered && !immediate) return;
        isOpen = lowered;

        // When lowered, disable collider so clicks hit the underlying pipe tile freely!
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = !isOpen;

        if (bridgeSpan != null)
        {
            if (_bridgeAnimCoroutine != null) StopCoroutine(_bridgeAnimCoroutine);
            _bridgeAnimCoroutine = StartCoroutine(AnimateBridgeLowering(lowered, immediate));
        }

        if (lowered)
        {
            AudioManager.Instance?.PlayWaterFlow();
            ShowFeedback($"<color=#FFD700>🌉 {bridgeName.ToUpper()} LOWERED!</color>\n<size=75%>Chasm Crossed</size>");

            var flow = FlowManager.Instance ?? FindObjectOfType<FlowManager>();
            if (flow != null) flow.RunFlow();
        }
    }

    private IEnumerator AnimateBridgeLowering(bool lowered, bool immediate)
    {
        Vector3 raisedPos = new Vector3(0f, 0.35f, 0f);
        Vector3 loweredPos = Vector3.zero;
        Vector3 targetPos = lowered ? loweredPos : raisedPos;

        if (immediate)
        {
            bridgeSpan.transform.localPosition = targetPos;
            if (waterRivulet != null) waterRivulet.gameObject.SetActive(lowered);
            yield break;
        }

        Vector3 startPos = bridgeSpan.transform.localPosition;
        float elapsed = 0f;
        float dur = 0.5f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
            bridgeSpan.transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        bridgeSpan.transform.localPosition = targetPos;
        if (waterRivulet != null) waterRivulet.gameObject.SetActive(lowered);
    }

    private void ShowFeedback(string msg)
    {
        if (statusText == null) return;
        if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(AnimateFeedback(msg));
    }

    private IEnumerator AnimateFeedback(string msg)
    {
        statusText.text = msg;
        statusText.gameObject.SetActive(true);

        Vector3 startPos = new Vector3(0f, 0.8f, -0.2f);
        Vector3 endPos = new Vector3(0f, 1.25f, -0.2f);
        float elapsed = 0f;
        float dur = 1.3f;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            statusText.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            statusText.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - (t * t)));
            yield return null;
        }

        statusText.gameObject.SetActive(false);
    }
}
