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

    [Header("Visual References")]
    private SpriteRenderer _barrierSr;
    private SpriteRenderer _badgeSr;
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
        _spriteLocked     = Resources.Load<Sprite>("Sprites/icon_locked");
        _spriteUnlocked   = Resources.Load<Sprite>("Sprites/icon_unlocked");
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
        _barrierSr = GetComponent<SpriteRenderer>();
        if (_barrierSr == null) _barrierSr = gameObject.AddComponent<SpriteRenderer>();
        _barrierSr.sortingOrder = 7;
        transform.localScale = new Vector3(0.75f, 0.75f, 1f);

        // Assign sprite depending on type
        if (gateType == GateType.BreakableRock)
        {
            _barrierSr.sprite = _spriteRocks != null ? _spriteRocks : _spriteBarrier;
        }
        else
        {
            _barrierSr.sprite = _spriteBarrier;
        }

        // Badge child (lock icon, shield, or power symbol)
        var badgeObj = transform.Find("GateBadge");
        if (badgeObj == null)
        {
            var go = new GameObject("GateBadge");
            go.transform.SetParent(transform, false);
            _badgeSr = go.AddComponent<SpriteRenderer>();
            _badgeSr.sortingOrder = 8;
            go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
            go.transform.localPosition = new Vector3(0f, 0f, -0.1f);
        }
        else
        {
            _badgeSr = badgeObj.GetComponent<SpriteRenderer>();
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

    private void OnMouseDown()
    {
        if (Time.timeScale == 0f) return;
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
        InteractWithAbility();
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

        switch (gateType)
        {
            case GateType.LockedGate:
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = isOpen ? _spriteUnlocked : _spriteLocked;
                    _badgeSr.color  = isOpen ? new Color(0.2f, 1f, 0.4f, 0.9f) : Color.white;
                }
                ApplyBarrierColor(isOpen ? new Color(0.3f, 0.8f, 1f, 0.25f) : new Color(1f, 0.4f, 0.4f, 0.95f), immediate);
                break;

            case GateType.PressureGate:
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteGoldShield;
                    _badgeSr.color  = isOpen ? Color.cyan : Color.gold;
                }
                ApplyBarrierColor(isOpen ? new Color(0.1f, 1f, 1f, 0.25f) : new Color(0.9f, 0.7f, 0.2f, 0.95f), immediate);
                break;

            case GateType.ElectricGate:
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteBlueShield;
                    _badgeSr.color  = isOpen ? Color.cyan : Color.yellow;
                }
                ApplyBarrierColor(isOpen ? new Color(0.2f, 0.9f, 1f, 0.25f) : new Color(0.5f, 0.8f, 1f, 0.95f), immediate);
                break;

            case GateType.BreakableRock:
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteLocked;
                    _badgeSr.color  = isOpen ? Color.clear : new Color(1f, 0.8f, 0.4f, 0.8f);
                }
                ApplyBarrierColor(isOpen ? new Color(1f, 1f, 1f, 0f) : new Color(0.7f, 0.7f, 0.75f, 1f), immediate);
                break;

            case GateType.InkedHazard:
                if (_badgeSr != null)
                {
                    _badgeSr.sprite = _spriteLocked;
                    _badgeSr.color  = isOpen ? Color.clear : new Color(0.8f, 0.2f, 0.9f, 0.9f);
                }
                ApplyBarrierColor(isOpen ? new Color(0.15f, 0.05f, 0.25f, 0.25f) : new Color(0.8f, 0.15f, 0.3f, 0.95f), immediate);
                break;
        }
    }

    private void ApplyBarrierColor(Color targetColor, bool immediate)
    {
        if (immediate)
        {
            _barrierSr.color = targetColor;
        }
        else
        {
            StartCoroutine(AnimateBarrier(targetColor));
        }
    }

    private IEnumerator AnimateBarrier(Color targetColor)
    {
        Color start = _barrierSr.color;
        float elapsed = 0f;
        const float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _barrierSr.color = Color.Lerp(start, targetColor, elapsed / duration);
            yield return null;
        }

        _barrierSr.color = targetColor;
    }
}
