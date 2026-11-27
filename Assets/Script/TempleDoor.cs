using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Ancient Temple Door mechanic for Ocean Rescuers (World 2 — Lost City ONLY).
/// Represents an ancient carved stone barrier blocking passage across an underwater ruin corridor:
/// - Unlike Deep Ocean hydraulic gates, this door is bound by ancient rune magic.
/// - Unseals when its required Ancient Rune (by requiredRuneId) is energized by water flow,
///   or when its linked Ancient Seal is activated.
/// - When opened, the carved stone doors part with a mystical rumbling effect and golden flash.
/// - Unblocks water flow in FlowManager BFS traversal.
/// - Strictly isolated to World 2 (Lost City).
/// </summary>
[DisallowMultipleComponent]
public class TempleDoor : GateComponent
{
    [Header("Temple Door Configuration")]
    public int requiredRuneId = 0;
    public string doorTitle = "Sanctuary Door";

    [Header("Temple Door Visuals")]
    public SpriteRenderer doorLeftSlab;
    public SpriteRenderer doorRightSlab;
    public SpriteRenderer sealGlyph;
    public TextMeshPro statusText;

    private Coroutine _doorAnimCoroutine;
    private Coroutine _feedbackCoroutine;

    private void Awake()
    {
        gateType = GateType.TempleDoor;
        SetupTempleDoorComponents();
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
        // Check if required rune is already energized
        if (AncientRune.IsRuneEnergized(requiredRuneId))
        {
            SetDoorOpen(true, immediate: true);
        }
        else
        {
            SetDoorOpen(isOpen, immediate: true);
        }
    }

    private void SetupTempleDoorComponents()
    {
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);
        }
        transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        // 1. Left Door Slab
        if (doorLeftSlab == null)
        {
            var lObj = transform.Find("DoorLeftSlab") ?? transform.Find("DoorLeft");
            if (lObj == null)
            {
                var go = new GameObject("DoorLeft");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(-0.25f, 0f, 0f);
                doorLeftSlab = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                doorLeftSlab = lObj.GetComponent<SpriteRenderer>();
            }
        }
        if (doorLeftSlab.sprite == null)
            doorLeftSlab.sprite = Resources.Load<Sprite>("Sprites/obstacle");
        doorLeftSlab.color = new Color(0.85f, 0.70f, 0.35f, 0.95f);
        doorLeftSlab.sortingOrder = 7;

        // 2. Right Door Slab
        if (doorRightSlab == null)
        {
            var rObj = transform.Find("DoorRightSlab") ?? transform.Find("DoorRight");
            if (rObj == null)
            {
                var go = new GameObject("DoorRight");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0.25f, 0f, 0f);
                doorRightSlab = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                doorRightSlab = rObj.GetComponent<SpriteRenderer>();
            }
        }
        if (doorRightSlab.sprite == null)
            doorRightSlab.sprite = Resources.Load<Sprite>("Sprites/obstacle");
        doorRightSlab.color = new Color(0.85f, 0.70f, 0.35f, 0.95f);
        doorRightSlab.sortingOrder = 7;

        // 3. Seal Glyph Badge
        if (sealGlyph == null)
        {
            var sObj = transform.Find("SealGlyph");
            if (sObj == null)
            {
                var go = new GameObject("SealGlyph");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                sealGlyph = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                sealGlyph = sObj.GetComponent<SpriteRenderer>();
            }
        }
        if (sealGlyph.sprite == null)
            sealGlyph.sprite = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield");
        sealGlyph.color = Color.white;
        sealGlyph.sortingOrder = 8;

        // 4. Status Text
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
                        OnDoorTapped();
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
                    OnDoorTapped();
                    break;
                }
            }
        }
    }

    private void OnDoorTapped()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (!isOpen)
        {
            SetDoorOpen(true);
        }
        else
        {
            ShowFeedback($"<color=#00FFFF>⛩️ {doorTitle.ToUpper()} OPEN</color>\n<size=75%>Sanctuary Passage Clear</size>");
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
        if (runeId == requiredRuneId)
        {
            SetDoorOpen(energized, immediate: false);
        }
    }

    public void SetDoorOpen(bool open, bool immediate = false)
    {
        if (isOpen == open && !immediate) return;
        isOpen = open;

        // When door is open, disable collider so clicks hit the underlying pipe tile freely!
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = !isOpen;

        if (doorLeftSlab != null && doorRightSlab != null)
        {
            if (_doorAnimCoroutine != null) StopCoroutine(_doorAnimCoroutine);
            _doorAnimCoroutine = StartCoroutine(AnimateDoorOpening(open, immediate));
        }

        if (open)
        {
            AudioManager.Instance?.PlayWaterFlow();
            ShowFeedback($"<color=#FFD700>⛩️ {doorTitle.ToUpper()} UNSEALED!</color>\n<size=75%>Sanctuary Passage Opened</size>");

            var flow = FlowManager.Instance ?? FindObjectOfType<FlowManager>();
            if (flow != null) flow.RunFlow();
        }
    }

    private IEnumerator AnimateDoorOpening(bool open, bool immediate)
    {
        Vector3 leftClosed = new Vector3(-0.25f, 0f, 0f);
        Vector3 leftOpen   = new Vector3(-0.65f, 0f, 0f);
        Vector3 rightClosed = new Vector3(0.25f, 0f, 0f);
        Vector3 rightOpen   = new Vector3(0.65f, 0f, 0f);

        Vector3 targetLeft  = open ? leftOpen : leftClosed;
        Vector3 targetRight = open ? rightOpen : rightClosed;

        if (immediate)
        {
            doorLeftSlab.transform.localPosition = targetLeft;
            doorRightSlab.transform.localPosition = targetRight;
            if (sealGlyph != null)
            {
                sealGlyph.sprite = Resources.Load<Sprite>(open ? "Sprites/icon_unlocked" : "Sprites/icon_locked_gold_shield");
                sealGlyph.color  = open ? new Color(0.2f, 1.0f, 0.45f, 1.0f) : Color.white;
                sealGlyph.sortingOrder = 10;
                sealGlyph.transform.localScale = open ? Vector3.one * 1.15f : Vector3.one;
            }
            yield break;
        }

        Vector3 startLeft = doorLeftSlab.transform.localPosition;
        Vector3 startRight = doorRightSlab.transform.localPosition;
        float elapsed = 0f;
        float dur = 0.45f;

        if (sealGlyph != null)
        {
            sealGlyph.sprite = Resources.Load<Sprite>(open ? "Sprites/icon_unlocked" : "Sprites/icon_locked_gold_shield");
            sealGlyph.sortingOrder = 10;
            sealGlyph.transform.localScale = open ? Vector3.one * 1.15f : Vector3.one;
        }

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
            doorLeftSlab.transform.localPosition = Vector3.Lerp(startLeft, targetLeft, t);
            doorRightSlab.transform.localPosition = Vector3.Lerp(startRight, targetRight, t);

            if (sealGlyph != null)
            {
                sealGlyph.color = Color.Lerp(Color.white, open ? new Color(0.2f, 1.0f, 0.45f, 1.0f) : Color.white, t);
            }

            yield return null;
        }

        doorLeftSlab.transform.localPosition = targetLeft;
        doorRightSlab.transform.localPosition = targetRight;
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
