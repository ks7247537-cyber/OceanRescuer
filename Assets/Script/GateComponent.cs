using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Gate & Obstacle Component for Ocean Rescuers.
/// Supports:
/// 1. Locked Gate: Starts locked, opens when linked valve is ON.
/// 2. Pressure Gate: Opens when required water pressure is achieved.
/// 3. Electric Gate: Energized & opened by Jellyfish electric shock!
/// 4. Breakable Rock: Cleared & destroyed by Turtle shell smash!
/// 5. Inked Hazard: Temporarily neutralized & disabled by Octopus ink!
/// Supports mouse click and Android touch input.
/// </summary>
public class GateComponent : MonoBehaviour
{
    [Header("Gate Configuration")]
    public GateType gateType = GateType.LockedGate;
    public Vector2Int gridPos;
    public bool isOpen = false;

    [Tooltip("For Pressure Gate: Minimum connected pipes upstream required to open")]
    public int requiredPressure = 5;

    [Tooltip("For Locked Gate: Linked valve coordinates that trigger opening (-1,-1 if none)")]
    public Vector2Int linkedValvePos = new Vector2Int(-1, -1);

    [Header("Visual References (Inspector Editable)")]
    public SpriteRenderer barrierRenderer;
    public SpriteRenderer badgeRenderer;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    private SpriteRenderer _barrierSr => barrierRenderer;
    private SpriteRenderer _badgeSr => badgeRenderer;
    private TextMeshPro _pressureText;
    private TextMeshPro _hintText;

    private Sprite _spriteLocked;
    private Sprite _spriteUnlocked;
    private Sprite _spriteGoldShield;
    private Sprite _spriteBlueShield;
    private Sprite _spriteBarrier;
    private Sprite _spriteRocks;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        SetupComponents();
    }

    private void Start()
    {
        UpdateVisuals(immediate: true);
    }

    private void SetupComponents()
    {
        if (lockedSprite == null)   lockedSprite   = Resources.Load<Sprite>("Sprites/icon_locked");
        if (unlockedSprite == null) unlockedSprite = Resources.Load<Sprite>("Sprites/icon_unlocked");
        _spriteLocked     = lockedSprite;
        _spriteUnlocked   = unlockedSprite;
        _spriteGoldShield = Resources.Load<Sprite>("Sprites/icon_locked_gold_shield");
        _spriteBlueShield = Resources.Load<Sprite>("Sprites/icon_locked_blue_shield");
        _spriteBarrier    = Resources.Load<Sprite>("Sprites/obstacle");
        _spriteRocks      = Resources.Load<Sprite>("Sprites/rocks_blue");

        // Collider for touch / tap interaction on Android & mouse
        var col = GetComponent<Collider2D>();
        if (col == null)
        {
            var box = gameObject.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.9f, 0.9f);
        }

        // Barrier renderer (physical obstacle on pipe)
        if (barrierRenderer == null) barrierRenderer = GetComponent<SpriteRenderer>();
        if (barrierRenderer == null) barrierRenderer = gameObject.AddComponent<SpriteRenderer>();
        barrierRenderer.sortingOrder = 7;
        // Compact scale: ~38% of tile so it sits neatly on the pipe coupling without covering the pipe
        transform.localScale = new Vector3(0.38f, 0.38f, 1f);

        // Assign sprite depending on type
        if (gateType == GateType.BreakableRock)
        {
            barrierRenderer.sprite = _spriteRocks != null ? _spriteRocks : _spriteBarrier;
        }
        else
        {
            barrierRenderer.sprite = _spriteBarrier;
        }

        // Badge child (lock icon, shield, or power symbol)
        var badgeObj = transform.Find("GateBadge");
        if (badgeObj == null)
        {
            var go = new GameObject("GateBadge");
            go.transform.SetParent(transform, false);
            badgeRenderer = go.AddComponent<SpriteRenderer>();
            badgeRenderer.sortingOrder = 8;
            go.transform.localScale = new Vector3(1f, 1f, 1f);
            go.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        }
        else
        {
            badgeRenderer = badgeObj.GetComponent<SpriteRenderer>();
        }

        // Pressure text display
        if (gateType == GateType.PressureGate)
        {
            var textObj = transform.Find("PressureText");
            if (textObj == null)
            {
                var tgo = new GameObject("PressureText");
                tgo.transform.SetParent(transform, false);
                _pressureText = tgo.AddComponent<TextMeshPro>();
                _pressureText.fontSize = 2.5f;
                _pressureText.alignment = TextAlignmentOptions.Center;
                _pressureText.sortingOrder = 9;
                _pressureText.color = Color.white;
                tgo.transform.localPosition = new Vector3(0f, -0.45f, -0.2f);
                _pressureText.text = $"P:{requiredPressure}";
            }
        }
    }

    private void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (isOpen) return;

        Vector2? inputPos = null;
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    inputPos = touch.position;
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                inputPos = Input.mousePosition;
            }
        }

        if (inputPos.HasValue && Camera.main != null)
        {
            Vector2 wp = Camera.main.ScreenToWorldPoint(inputPos.Value);
            var hit = Physics2D.OverlapPoint(wp);
            if (hit != null && (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)))
            {
                OnGateTapped();
            }
        }
    }

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

        OnGateTapped();
    }

    public void OnGateTapped()
    {
        if (isOpen)
        {
            RotatePipeUnderneath();
            return;
        }

        // 1. Try ability first if matching companion
        bool abilityUsed = false;
        if (CreatureAbilityController.Instance != null && CreatureAbilityController.Instance.CanUseAbility)
        {
            abilityUsed = CreatureAbilityController.Instance.TryUseAbilityOnGate(this);
        }

        // 2. If ability wasn't used or not matching companion, allow direct tap unlock/break so gameplay is never blocked!
        if (!abilityUsed && !isOpen)
        {
            UnlockOrBreakDirectly();
        }

        // Always rotate pipe tile on this grid position
        RotatePipeUnderneath();
    }

    public void UnlockOrBreakDirectly()
    {
        if (isOpen) return;

        switch (gateType)
        {
            case GateType.BreakableRock:
                AudioManager.Instance?.PlayButtonClick();
                ShowHint("<color=#00FFFF>💥 ROCK SMASHED!</color>");
                break;
            case GateType.ElectricGate:
                AudioManager.Instance?.PlayButtonClick();
                ShowHint("<color=#00FFFF>⚡ GATE DISCHARGED!</color>");
                break;
            case GateType.LockedGate:
                AudioManager.Instance?.PlayWaterFlow();
                ShowHint("<color=#00FFFF>🔓 GATE UNLOCKED!</color>");
                break;
            case GateType.PressureGate:
                AudioManager.Instance?.PlayWaterFlow();
                ShowHint("<color=#00FFFF>🌊 PRESSURE RELEASED!</color>");
                break;
            case GateType.InkedHazard:
                AudioManager.Instance?.PlayWaterFlow();
                ShowHint("<color=#00FFFF>✨ HAZARD CLEARED!</color>");
                break;
            default:
                AudioManager.Instance?.PlayWaterFlow();
                ShowHint("<color=#00FFFF>🔓 UNLOCKED!</color>");
                break;
        }

        SetOpen(true);
        FlowManager.Instance?.RunFlow();
    }

    private void RotatePipeUnderneath()
    {
        var grid = GridManager.Instance != null ? GridManager.Instance : FindObjectOfType<GridManager>();
        if (grid != null)
        {
            var pipe = grid.GetTileAt(gridPos.x, gridPos.y);
            if (pipe != null)
            {
                pipe.Rotate();
            }
        }
    }

    public void InteractWithAbility()
    {
        if (isOpen) return;

        // Route tap to CreatureAbilityController
        if (CreatureAbilityController.Instance != null)
        {
            CreatureAbilityController.Instance.TryUseAbilityOnGate(this);
        }
    }

    public void ShowHint(string message)
    {
        StopCoroutine(nameof(ShowHintRoutine));
        StartCoroutine(ShowHintRoutine(message));
    }

    private IEnumerator ShowHintRoutine(string message)
    {
        if (_hintText == null)
        {
            var hgo = new GameObject("HintText");
            hgo.transform.SetParent(transform, false);
            hgo.transform.localPosition = new Vector3(0f, 0.55f, -0.3f);
            _hintText = hgo.AddComponent<TextMeshPro>();
            _hintText.fontSize = 2.4f;
            _hintText.alignment = TextAlignmentOptions.Center;
            _hintText.sortingOrder = 20;
        }

        _hintText.text = message;
        _hintText.color = Color.yellow;
        _hintText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.2f);
        if (_hintText != null) _hintText.gameObject.SetActive(false);
    }

    public void SetOpen(bool open)
    {
        if (isOpen == open) return;
        isOpen = open;
        UpdateVisuals(immediate: false);
    }

    public bool EvaluatePressure(int currentPressure)
    {
        if (gateType != GateType.PressureGate) return isOpen;

        bool satisfied = currentPressure >= requiredPressure;
        SetOpen(satisfied);

        if (_pressureText != null)
        {
            _pressureText.text = satisfied 
                ? $"P:{currentPressure}/{requiredPressure} ✓" 
                : $"P:{currentPressure}/{requiredPressure}";
            _pressureText.color = satisfied ? Color.cyan : Color.yellow;
        }

        return satisfied;
    }

    public void UpdateVisuals(bool immediate)
    {
        if (_barrierSr == null) return;

        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = !isOpen; // Disable collider when unlocked so clicks directly hit the pipe tile!

        if (isOpen)
        {
            // Whenever any gate/obstacle is unlocked, proudly display the vibrant green UNLOCKED icon!
            if (_badgeSr != null)
            {
                _badgeSr.sprite = _spriteUnlocked != null ? _spriteUnlocked : Resources.Load<Sprite>("Sprites/icon_unlocked");
                _badgeSr.color  = new Color(0.2f, 1.0f, 0.45f, 1.0f); // Radiant Emerald Green
                _badgeSr.sortingOrder = 10;
                _badgeSr.transform.localScale = Vector3.one * 1.15f;
            }
            if (_barrierSr != null)
            {
                _barrierSr.color = new Color(0.3f, 1.0f, 0.7f, 0.45f); // Glowing translucent base
            }
            return;
        }

        switch (gateType)
        {
            case GateType.LockedGate:
                if (_barrierSr != null) _barrierSr.sprite = _spriteBarrier != null ? _spriteBarrier : _spriteLocked;
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteLocked;
                    _badgeSr.sortingOrder = 8;
                    _badgeSr.transform.localScale = Vector3.one;
                }
                ApplyBarrierColor(Color.white, immediate);
                break;

            case GateType.PressureGate:
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteGoldShield;
                    _badgeSr.color  = Color.gold;
                    _badgeSr.sortingOrder = 8;
                    _badgeSr.transform.localScale = Vector3.one;
                }
                ApplyBarrierColor(new Color(0.9f, 0.7f, 0.2f, 0.95f), immediate);
                break;

            case GateType.ElectricGate:
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteBlueShield;
                    _badgeSr.color  = Color.yellow;
                    _badgeSr.sortingOrder = 8;
                    _badgeSr.transform.localScale = Vector3.one;
                }
                ApplyBarrierColor(new Color(0.5f, 0.8f, 1f, 0.95f), immediate);
                break;

            case GateType.BreakableRock:
                if (_barrierSr != null) _barrierSr.sprite = _spriteRocks != null ? _spriteRocks : _spriteBarrier;
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteLocked;
                    _badgeSr.color  = new Color(1f, 0.8f, 0.4f, 0.8f);
                    _badgeSr.sortingOrder = 8;
                    _badgeSr.transform.localScale = Vector3.one;
                }
                ApplyBarrierColor(new Color(0.7f, 0.7f, 0.75f, 1f), immediate);
                break;

            case GateType.InkedHazard:
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteLocked;
                    _badgeSr.color  = new Color(0.8f, 0.2f, 0.9f, 0.9f);
                    _badgeSr.sortingOrder = 8;
                    _badgeSr.transform.localScale = Vector3.one;
                }
                ApplyBarrierColor(new Color(0.8f, 0.15f, 0.3f, 0.95f), immediate);
                break;
        }
    }

    private void ApplyBarrierColor(Color targetColor, bool immediate)
    {
        if (immediate)
        {
            if (_barrierSr != null) _barrierSr.color = targetColor;
            if (_badgeSr != null)   _badgeSr.color   = targetColor;
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(AnimateBarrier(targetColor));
        }
    }

    private IEnumerator AnimateBarrier(Color targetColor)
    {
        Color start = _barrierSr != null ? _barrierSr.color : Color.white;
        Color badgeStart = _badgeSr != null ? _badgeSr.color : Color.white;
        float elapsed = 0f;
        const float duration = 0.25f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (_barrierSr != null) _barrierSr.color = Color.Lerp(start, targetColor, t);
            if (_badgeSr != null)   _badgeSr.color   = Color.Lerp(badgeStart, targetColor, t);
            yield return null;
        }

        if (_barrierSr != null) _barrierSr.color = targetColor;
        if (_badgeSr != null)   _badgeSr.color   = targetColor;
    }
}
