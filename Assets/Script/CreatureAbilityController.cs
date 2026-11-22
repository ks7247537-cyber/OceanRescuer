using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Controller for the special abilities of the 4 sea creatures:
/// 1. Clownfish: Normal swimming and rescue behavior (baseline).
/// 2. Turtle: Shell Smash ability to clear specifically designated Breakable Obstacles.
/// 3. Octopus: Ink Cloud ability to temporarily disable designated hazardous obstacles.
/// 4. Jellyfish: Electric Shock ability to energize and open compatible Electric Gates.
/// Supports mouse and Android touch controls, cooldowns, limited uses, and undo integration.
/// </summary>
public class CreatureAbilityController : MonoBehaviour
{
    public static CreatureAbilityController Instance { get; private set; }

    [Header("Current Creature State")]
    public CreatureType activeCreature = CreatureType.Clownfish;
    public int abilityUsesRemaining = 0;
    public int maxAbilityUses = 0;
    public float abilityCooldown = 1.0f;
    private float _cooldownTimer = 0f;

    [Header("Visual References")]
    private CharacterMover _mover;
    private TextMeshPro _abilityHudText;
    private GameObject _abilityHudBadge;
    private int _lastAbilityFrame = -1;

    public bool CanUseAbility => 
        abilityUsesRemaining > 0 && 
        _cooldownTimer <= 0f && 
        (GameManager.Instance == null || !GameManager.Instance.IsGameOver);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _mover = FindObjectOfType<CharacterMover>();
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
        }

        Vector2? clickPos = null;
        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                clickPos = Input.mousePosition;
            }
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            int touchId = Input.GetTouch(0).fingerId;
            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touchId))
            {
                clickPos = Input.GetTouch(0).position;
            }
        }

        if (clickPos.HasValue && Camera.main != null)
        {
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(clickPos.Value);
            var hit = Physics2D.OverlapPoint(worldPoint);
            if (hit != null)
            {
                var gate = hit.GetComponent<GateComponent>();
                if (gate != null)
                {
                    TryUseAbilityOnGate(gate);
                }
            }
        }
    }

    /// <summary>
    /// Configures active creature and ability charges for current level.
    /// </summary>
    public void Setup(CreatureType creature, int uses)
    {
        activeCreature = creature;
        maxAbilityUses = uses;
        abilityUsesRemaining = uses;
        _cooldownTimer = 0f;

        if (_mover == null) _mover = FindObjectOfType<CharacterMover>();
        if (_mover != null)
        {
            // The rescued swimmer inside the pipes must always remain Clownfish!
            _mover.SetCreature(CreatureType.Clownfish);
        }

        CreateOrUpdateHudBadge();
    }

    /// <summary>
    /// Attempts to apply creature ability on a tapped gate / obstacle.
    /// </summary>
    public bool TryUseAbilityOnGate(GateComponent gate)
    {
        if (Time.frameCount == _lastAbilityFrame) return false;
        _lastAbilityFrame = Time.frameCount;

        if (gate == null || gate.IsOpen) return false;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return false;

        // 1. Turtle ability on BreakableRock
        if (gate.gateType == GateType.BreakableRock)
        {
            if (activeCreature == CreatureType.Turtle)
            {
                if (!CanUseAbility) return false;
                ConsumeUse(gate);
                StartCoroutine(TurtleSmashRoutine(gate));
                return true;
            }
            else
            {
                gate.ShowHint("Requires Turtle to Break! 🐢");
                return false;
            }
        }

        // 2. Octopus ability on InkedHazard
        if (gate.gateType == GateType.InkedHazard)
        {
            if (activeCreature == CreatureType.Octopus)
            {
                if (!CanUseAbility) return false;
                ConsumeUse(gate);
                StartCoroutine(OctopusInkRoutine(gate));
                return true;
            }
            else
            {
                gate.ShowHint("Requires Octopus Ink! 🐙");
                return false;
            }
        }

        // 3. Jellyfish ability on ElectricGate
        if (gate.gateType == GateType.ElectricGate)
        {
            if (activeCreature == CreatureType.Jellyfish)
            {
                if (!CanUseAbility) return false;
                ConsumeUse(gate);
                StartCoroutine(JellyfishElectricRoutine(gate));
                return true;
            }
            else
            {
                gate.ShowHint("Requires Jellyfish Shock! ⚡");
                return false;
            }
        }

        return false;
    }

    private void ConsumeUse(GateComponent gate)
    {
        bool oldGateOpen = gate.IsOpen;
        abilityUsesRemaining = Mathf.Max(0, abilityUsesRemaining - 1);
        _cooldownTimer = abilityCooldown;

        // Record in MoveManager for Undo support
        if (MoveManager.Instance != null)
        {
            MoveManager.Instance.RecordGateAbility(gate, oldGateOpen);
        }

        UpdateHudBadge();
    }

    public void RestoreAbilityCharge()
    {
        abilityUsesRemaining = Mathf.Min(maxAbilityUses, abilityUsesRemaining + 1);
        _cooldownTimer = 0f;
        UpdateHudBadge();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Visual Effects
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator TurtleSmashRoutine(GateComponent gate)
    {
        Transform activeActor = (_abilityHudBadge != null && _abilityHudBadge.activeSelf) 
            ? _abilityHudBadge.transform 
            : (_mover != null ? _mover.transform : transform);

        Vector3 startPos = activeActor.position;
        Vector3 targetPos = gate.transform.position;

        // Turtle fast charge lunge
        float elapsed = 0f;
        while (elapsed < 0.16f)
        {
            elapsed += Time.deltaTime;
            activeActor.position = Vector3.Lerp(startPos, targetPos, elapsed / 0.16f);
            yield return null;
        }

        // Rock shatter visual burst effect
        CreateRockShatterEffect(targetPos);
        gate.SetOpen(true);

        // Return turtle back to companion spot
        elapsed = 0f;
        while (elapsed < 0.20f)
        {
            elapsed += Time.deltaTime;
            activeActor.position = Vector3.Lerp(targetPos, startPos, elapsed / 0.20f);
            yield return null;
        }
        activeActor.position = startPos;

        GameManager.Instance?.CheckWinCondition();
    }

    private IEnumerator OctopusInkRoutine(GateComponent gate)
    {
        Vector3 targetPos = gate.transform.position;

        // Create dark ink cloud effect covering obstacle
        CreateInkCloudEffect(targetPos);

        yield return new WaitForSeconds(0.15f);
        gate.SetOpen(true);

        GameManager.Instance?.CheckWinCondition();
    }

    private IEnumerator JellyfishElectricRoutine(GateComponent gate)
    {
        Vector3 startPos = (_abilityHudBadge != null && _abilityHudBadge.activeSelf) 
            ? _abilityHudBadge.transform.position 
            : (_mover != null ? _mover.transform.position : transform.position);
        Vector3 targetPos = gate.transform.position;

        // Electric lightning bolt arc
        CreateElectricArcEffect(startPos, targetPos);

        yield return new WaitForSeconds(0.2f);
        gate.SetOpen(true);

        GameManager.Instance?.CheckWinCondition();
    }

    private void CreateRockShatterEffect(Vector3 pos)
    {
        var fxObj = new GameObject("FX_RockShatter");
        fxObj.transform.position = pos;
        var sr = fxObj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Sprites/rocks_blue");
        sr.sortingOrder = 12;
        sr.color = Color.white;
        StartCoroutine(FadeAndScaleOut(fxObj, 0.35f, 1.4f));
    }

    private void CreateInkCloudEffect(Vector3 pos)
    {
        var fxObj = new GameObject("FX_InkCloud");
        fxObj.transform.position = pos;
        var sr = fxObj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Sprites/obstacle");
        sr.color = new Color(0.18f, 0.05f, 0.28f, 0.95f); // Deep dark purple ink
        sr.sortingOrder = 12;
        StartCoroutine(FadeAndScaleOut(fxObj, 0.6f, 1.3f));
    }

    private void CreateElectricArcEffect(Vector3 start, Vector3 end)
    {
        var fxObj = new GameObject("FX_ElectricLightning");
        fxObj.transform.position = (start + end) * 0.5f;
        var sr = fxObj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning");
        sr.color = Color.cyan;
        sr.sortingOrder = 12;

        Vector3 diff = end - start;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        fxObj.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        fxObj.transform.localScale = new Vector3(diff.magnitude * 0.7f, 0.6f, 1f);

        StartCoroutine(FlashAndDestroy(fxObj, 0.25f));
    }

    private IEnumerator FadeAndScaleOut(GameObject obj, float duration, float targetScale)
    {
        if (obj == null) yield break;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        Vector3 startScale = obj.transform.localScale;
        Color startCol = sr != null ? sr.color : Color.white;
        float elapsed = 0f;

        while (elapsed < duration && obj != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.transform.localScale = Vector3.Lerp(startScale, startScale * targetScale, t);
            if (sr != null)
            {
                sr.color = new Color(startCol.r, startCol.g, startCol.b, Mathf.Lerp(startCol.a, 0f, t));
            }
            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    private IEnumerator FlashAndDestroy(GameObject obj, float duration)
    {
        if (obj == null) yield break;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < duration && obj != null)
        {
            elapsed += Time.deltaTime;
            if (sr != null)
            {
                sr.color = (Mathf.Sin(elapsed * 40f) > 0f) ? Color.yellow : Color.cyan;
            }
            yield return null;
        }
        if (obj != null) Destroy(obj);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HUD Companion Display
    // ─────────────────────────────────────────────────────────────────────────

    private void CreateOrUpdateHudBadge()
    {
        if (activeCreature == CreatureType.Clownfish || maxAbilityUses <= 0)
        {
            if (_abilityHudBadge != null) _abilityHudBadge.SetActive(false);
            return;
        }

        if (_abilityHudBadge == null)
        {
            _abilityHudBadge = new GameObject("CreatureAbilityHUD");
            // Placed neatly on the far left coral reef (off the pipe grid which starts at x = -2.75)
            _abilityHudBadge.transform.position = new Vector3(-6.2f, 0.4f, -2f);

            var iconObj = new GameObject("CreatureIcon");
            iconObj.transform.SetParent(_abilityHudBadge.transform, false);
            var isr = iconObj.AddComponent<SpriteRenderer>();
            isr.sortingOrder = 15;
            // PPU is 1800, scale 1.0 gives standard 0.85 unit companion size
            iconObj.transform.localScale = new Vector3(1.0f, 1.0f, 1f);

            var textObj = new GameObject("AbilityUsesText");
            textObj.transform.SetParent(_abilityHudBadge.transform, false);
            textObj.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            _abilityHudText = textObj.AddComponent<TextMeshPro>();
            _abilityHudText.fontSize = 2.4f;
            _abilityHudText.sortingOrder = 16;
            _abilityHudText.alignment = TextAlignmentOptions.Center;
        }

        _abilityHudBadge.SetActive(true);
        var iconSr = _abilityHudBadge.transform.Find("CreatureIcon")?.GetComponent<SpriteRenderer>();
        if (iconSr != null)
        {
            string spriteName = activeCreature.ToString();
            Sprite[] subSprites = Resources.LoadAll<Sprite>($"Characters/{spriteName}");
            if (subSprites != null && subSprites.Length > 0)
            {
                iconSr.sprite = subSprites[0];
            }
            else
            {
                iconSr.sprite = Resources.Load<Sprite>($"Characters/{spriteName}");
            }
        }

        UpdateHudBadge();
    }

    private void UpdateHudBadge()
    {
        if (_abilityHudText != null)
        {
            _abilityHudText.text = $"x{abilityUsesRemaining}";
            _abilityHudText.color = abilityUsesRemaining > 0 ? Color.yellow : Color.gray;
        }
    }
}
