using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Oxygen Leak & Emergency Pipe Repair mechanic for Ocean Rescuers (World 1 — Deep Ocean ONLY).
/// Represents a fractured underwater pipe section leaking oxygen bubbles into the abyss:
/// - While leaking, inflicts hydraulic pipeline pressure loss (default: -15 PSI) in DeepOceanPressureManager.
/// - Produces visible animated escaping bubbles and a subtle warning glow.
/// - Mobile touch/tap interaction allows the player to perform an emergency pipe repair.
/// - Upon repair, the fracture is sealed with a reinforced metallic clamp, bubbles cease,
///   pressure loss is removed, and a satisfying repair confirmation effect plays.
/// - Integrates seamlessly with Deep Ocean Sonar scan.
/// - Strictly isolated to World 1 (Deep Ocean).
/// 
/// Hierarchy:
/// OxygenLeak
/// ├── DamageOverlay (fissure / crack overlay sprite)
/// ├── LeakBubbles (pulsing rising escaping bubbles)
/// ├── WarningGlow (cyan/amber depressurization aura)
/// ├── RepairPatch (reinforced golden/metallic repair band, initially inactive)
/// └── InteractionFeedback (3D floating repair confirmation text)
/// </summary>
[DisallowMultipleComponent]
public class OxygenLeak : MonoBehaviour
{
    [Header("Leak Configuration")]
    public Vector2Int gridPos;

    [Tooltip("Hydraulic pipeline pressure loss (PSI) inflicted while this leak is active.")]
    public float pressureLoss = 15.0f;

    [Tooltip("Number of touch taps required to seal the fracture.")]
    public int tapsToRepair = 1;

    [Header("Visual Colors (Inspector Editable)")]
    public Color crackColor         = new Color(0.25f, 0.20f, 0.15f, 0.95f); // Fractured pipe fissure
    public Color bubbleTint         = new Color(0.60f, 0.95f, 1.0f, 0.90f);  // Oceanic oxygen bubbles
    public Color warningGlowColor   = new Color(0.10f, 0.90f, 1.0f, 0.75f);  // Cyan/amber warning aura
    public Color repairPatchColor   = new Color(0.95f, 0.85f, 0.35f, 1.0f);  // Gleaming brass clamp

    [Header("Child References")]
    public SpriteRenderer damageOverlay;
    public SpriteRenderer leakBubbles;
    public SpriteRenderer warningGlow;
    public SpriteRenderer repairPatch;
    public TextMeshPro feedbackText;

    [Header("Runtime State")]
    [SerializeField] private bool _isRepaired = false;
    [SerializeField] private int _currentTaps = 0;
    private int _lastTapFrame = -1;

    public bool IsRepaired => _isRepaired;
    public bool IsLeaking => !_isRepaired && IsWaterConnected();

    private Coroutine _bubbleAnimCoroutine;
    private Coroutine _feedbackCoroutine;
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
            DeepOceanPressureManager.Instance.RegisterLeak(this);
        }
    }

    private void OnDestroy()
    {
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.UnregisterLeak(this);
        }
    }

    private void OnDisable()
    {
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.UnregisterLeak(this);
        }
    }

    private void OnEnable()
    {
        if (SaveManager.GetSelectedWorld() == 1 && DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.RegisterLeak(this);
        }
    }

    public void SetupComponents()
    {
        // Collider for mobile touch interaction
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);
            box.isTrigger = true;
        }
        transform.localScale = new Vector3(0.42f, 0.42f, 1f);

        // 1. DamageOverlay (Crack / Fissure)
        if (damageOverlay == null)
        {
            var crackObj = transform.Find("DamageOverlay");
            if (crackObj == null)
            {
                var go = new GameObject("DamageOverlay");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = Vector3.zero;
                damageOverlay = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                damageOverlay = crackObj.GetComponent<SpriteRenderer>();
            }
        }
        if (damageOverlay.sprite == null)
            damageOverlay.sprite = Resources.Load<Sprite>("Sprites/obstacle") 
                                ?? Resources.Load<Sprite>("Sprites/red_obstacle");
        damageOverlay.color = crackColor;
        damageOverlay.sortingOrder = 8;

        // 2. LeakBubbles (Animated escaping bubbles)
        if (leakBubbles == null)
        {
            var bObj = transform.Find("LeakBubbles");
            if (bObj == null)
            {
                var go = new GameObject("LeakBubbles");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0.25f, -0.08f);
                leakBubbles = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                leakBubbles = bObj.GetComponent<SpriteRenderer>();
            }
        }
        if (leakBubbles.sprite == null)
            leakBubbles.sprite = Resources.Load<Sprite>("Sprites/bubbles");
        leakBubbles.color = bubbleTint;
        leakBubbles.sortingOrder = 10;

        // 3. WarningGlow (Aura)
        if (warningGlow == null)
        {
            var wObj = transform.Find("WarningGlow");
            if (wObj == null)
            {
                var go = new GameObject("WarningGlow");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.05f);
                warningGlow = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                warningGlow = wObj.GetComponent<SpriteRenderer>();
            }
        }
        if (warningGlow.sprite == null)
            warningGlow.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") 
                              ?? Resources.Load<Sprite>("Sprites/bubbles");
        warningGlow.color = warningGlowColor;
        warningGlow.sortingOrder = 9;

        // 4. RepairPatch (Reinforced metallic clamp band)
        if (repairPatch == null)
        {
            var pObj = transform.Find("RepairPatch");
            if (pObj == null)
            {
                var go = new GameObject("RepairPatch");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                repairPatch = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                repairPatch = pObj.GetComponent<SpriteRenderer>();
            }
        }
        if (repairPatch.sprite == null)
            repairPatch.sprite = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield") 
                              ?? Resources.Load<Sprite>("Sprites/icon_unlocked");
        repairPatch.color = repairPatchColor;
        repairPatch.sortingOrder = 11;
        repairPatch.gameObject.SetActive(false);

        // 5. InteractionFeedback (Floating text)
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
        if (SaveManager.GetSelectedWorld() != 1) return;

        HandleTouchInput();

        // Check if leak flow state changed
        if (Time.unscaledTime - _lastEvaluateTime > 0.25f)
        {
            _lastEvaluateTime = Time.unscaledTime;
            EvaluateLeakState();
        }
    }

    private void HandleTouchInput()
    {
        if (_isRepaired) return;

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
                        OnTileTapped();
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
                    OnTileTapped();
                    break;
                }
            }
        }
    }

    private void OnTileTapped()
    {
        if (Time.frameCount == _lastTapFrame) return;
        _lastTapFrame = Time.frameCount;

        if (_isRepaired)
        {
            ShowFloatingFeedback("<color=#00FFFF>🔧 SECURED</color>");
            return;
        }

        _currentTaps++;
        if (_currentTaps >= tapsToRepair)
        {
            PerformRepair();
        }
        else
        {
            AudioManager.Instance?.PlayButtonClick();
            ShowFloatingFeedback($"<color=#38BDF8>🔧 {_currentTaps}/{tapsToRepair}</color>");
        }
    }

    public void PerformRepair()
    {
        if (_isRepaired) return;
        _isRepaired = true;

        // Disable collider so repaired pipe tile receives future rotation taps without interference
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        UpdateVisuals(immediate: false);

        AudioManager.Instance?.PlayWaterFlow();
        ShowFloatingFeedback("<color=#00FFFF>🔧 SEALED</color>");

        // Inform DeepOceanPressureManager to remove pressure penalty immediately
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.EvaluatePressure();
        }

        // Recheck water flow
        var flow = FindObjectOfType<FlowManager>();
        if (flow != null) flow.RunFlow();
    }

    public void EvaluateLeakState()
    {
        if (_isRepaired) return;

        bool active = IsWaterConnected();

        if (active)
        {
            if (_bubbleAnimCoroutine == null)
            {
                _bubbleAnimCoroutine = StartCoroutine(AnimateBubblesRoutine());
            }
            if (warningGlow != null) warningGlow.gameObject.SetActive(true);
        }
        else
        {
            if (_bubbleAnimCoroutine != null)
            {
                StopCoroutine(_bubbleAnimCoroutine);
                _bubbleAnimCoroutine = null;
            }
            if (warningGlow != null) warningGlow.gameObject.SetActive(false);
        }
    }

    public void UpdateVisuals(bool immediate)
    {
        if (_isRepaired)
        {
            if (_bubbleAnimCoroutine != null)
            {
                StopCoroutine(_bubbleAnimCoroutine);
                _bubbleAnimCoroutine = null;
            }
            if (leakBubbles != null) leakBubbles.gameObject.SetActive(false);
            if (warningGlow != null) warningGlow.gameObject.SetActive(false);
            if (damageOverlay != null)
            {
                damageOverlay.color = new Color(crackColor.r, crackColor.g, crackColor.b, 0.3f);
            }
            if (repairPatch != null)
            {
                repairPatch.gameObject.SetActive(true);
                repairPatch.color = repairPatchColor;
            }
        }
        else
        {
            if (repairPatch != null) repairPatch.gameObject.SetActive(false);
            if (damageOverlay != null) damageOverlay.color = crackColor;
            EvaluateLeakState();
        }
    }

    private IEnumerator AnimateBubblesRoutine()
    {
        if (leakBubbles == null) yield break;
        leakBubbles.gameObject.SetActive(true);

        Vector3 startLocal = new Vector3(0f, 0.15f, -0.08f);
        Vector3 endLocal = new Vector3(0f, 0.65f, -0.08f);

        while (!_isRepaired)
        {
            float elapsed = 0f;
            float duration = 0.85f;

            while (elapsed < duration && !_isRepaired)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                leakBubbles.transform.localPosition = Vector3.Lerp(startLocal, endLocal, t);
                float alpha = Mathf.Sin(t * Mathf.PI) * bubbleTint.a;
                leakBubbles.color = new Color(bubbleTint.r, bubbleTint.g, bubbleTint.b, alpha);
                leakBubbles.transform.localScale = Vector3.one * (0.6f + 0.4f * t);

                if (warningGlow != null)
                {
                    float glowPulse = 0.8f + 0.2f * Mathf.Sin(Time.time * 6f);
                    warningGlow.transform.localScale = Vector3.one * glowPulse;
                }

                yield return null;
            }
        }

        leakBubbles.gameObject.SetActive(false);
    }

    public bool IsWaterConnected()
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
        float duration = 1.3f;

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
