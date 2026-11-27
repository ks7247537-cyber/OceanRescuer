using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Modular Companion Ability HUD for Ocean Rescuers.
/// Displays active companion portrait, unique ability name, description, charges count,
/// and live readiness state with pulsing glows and feedback animations.
/// 
/// Completely editable in Unity Hierarchy and Inspector.
/// </summary>
[DisallowMultipleComponent]
public class CreatureAbilityHUD : MonoBehaviour
{
    private static CreatureAbilityHUD _instance;
    public static CreatureAbilityHUD Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CreatureAbilityHUD>(true);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Hierarchy Child References")]
    [Tooltip("Root RectTransform of the ability panel.")]
    public RectTransform rootPanel;

    [Tooltip("Background panel image (e.g. rounded glossy underwater frame).")]
    public Image panelBackground;

    [Tooltip("Subtle panel border / decoration sheen.")]
    public Image panelDecoration;

    [Tooltip("Container holding the companion icon and icon glow.")]
    public RectTransform companionIconContainer;

    [Tooltip("Image showing the active companion portrait.")]
    public Image companionIcon;

    [Tooltip("Pulsing circular glow outline behind or around the icon.")]
    public Image iconGlow;

    [Tooltip("Container holding ability textual information.")]
    public RectTransform abilityInformation;

    [Tooltip("Text displaying the ability name (e.g. Shell Smash, Ink Clear, Electric Shock).")]
    public TextMeshProUGUI abilityNameText;

    [Tooltip("Text displaying the ability purpose description.")]
    public TextMeshProUGUI abilityDescriptionText;

    [Tooltip("Text displaying the ability readiness status (Ready, Empty, Passive).")]
    public TextMeshProUGUI abilityStatusText;

    [Tooltip("Optional background image behind ability status text (e.g. pill button badge).")]
    public Image statusBadgeBackground;

    [Tooltip("Container holding the charge counter badge.")]
    public RectTransform chargesContainer;

    [Tooltip("Background image for the charges counter pill.")]
    public Image chargesBackground;

    [Tooltip("Icon representing charges (e.g. shell / pearl / lightning).")]
    public Image chargesIcon;

    [Tooltip("Text displaying remaining charges (e.g. 2/2 Charges Left, Rescuer Active).")]
    public TextMeshProUGUI chargesCountText;

    [Tooltip("Small circular availability dot indicator.")]
    public Image abilityAvailabilityIndicator;

    [Tooltip("Pulsing perimeter outline around the card when ready.")]
    public Image abilityPulseEffect;

    [Tooltip("Feedback effect overlay group for flashes and punch bounces.")]
    public CanvasGroup abilityFeedbackEffect;

    [Tooltip("CanvasGroup controlling overall widget opacity and disabled state.")]
    public CanvasGroup mainCanvasGroup;

    [Tooltip("Optional interactive button on the card for tap tooltip/feedback.")]
    public Button panelButton;

    [Header("Color Palette (Inspector Configurable)")]
    public Color readyColor       = new Color(0.15f, 0.88f, 0.45f, 1f);   // Emerald green / mint
    public Color depletedColor    = new Color(0.55f, 0.62f, 0.70f, 0.85f); // Muted slate gray
    public Color passiveColor     = new Color(0.20f, 0.75f, 0.95f, 1f);   // Vibrant aquatic cyan
    public Color cooldownColor    = new Color(0.98f, 0.75f, 0.20f, 1f);   // Warm amber

    public Color panelNormalColor   = Color.white;                            // Clean natural card artwork brightness
    public Color panelDepletedColor = new Color(0.72f, 0.75f, 0.82f, 0.85f);  // Subtle dim when depleted

    [Header("Visual & State Settings")]
    [Range(0.2f, 1f)]
    public float disabledOpacity = 0.65f;
    public bool pulseWhenReady = true;

    [Header("Animation Settings")]
    [SerializeField] private float punchScaleDuration = 0.22f;
    [SerializeField] private float punchScaleAmount = 0.15f;
    [SerializeField] private bool showFloatingTooltip = false;

    [Header("Companion Sprites (Inspector Configurable)")]
    public Sprite turtleSprite;
    public Sprite octopusSprite;
    public Sprite jellyfishSprite;
    public Sprite clownfishSprite;
    public Sprite chargeIconSprite;

    private Coroutine _punchCoroutine;
    private Coroutine _glowPulseCoroutine;
    private Coroutine _tooltipCoroutine;
    private GameObject _floatingTooltipObj;
    private CreatureType _currentCreature = CreatureType.Clownfish;
    private int _lastCharges = -1;
    private bool _subscribed = false;

    private Color _initialPanelBgColor = Color.white;
    private Color _initialIndicatorColor = Color.white;
    private Color _initialPulseColor = new Color(0.2f, 0.95f, 1f, 0.7f);
    private Color _initialIconGlowColor = new Color(0.1f, 0.9f, 0.95f, 0.5f);
    private Color _initialStatusBadgeColor = Color.white;
    private Vector3 _initialPulseScale = Vector3.one;
    private Vector3 _initialIconGlowScale = Vector3.one;

    private void Awake()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameplayScene")
        {
            Destroy(gameObject);
            return;
        }

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (mainCanvasGroup == null) mainCanvasGroup = GetComponent<CanvasGroup>();
        if (mainCanvasGroup == null) mainCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (rootPanel == null) rootPanel = GetComponent<RectTransform>();

        AutoDiscoverReferences();
        CaptureInitialColors();
        LoadDefaultSprites();
        BindButtonListeners();
    }

    private void BindButtonListeners()
    {
        if (panelButton != null)
        {
            panelButton.onClick.RemoveAllListeners();
            panelButton.onClick.AddListener(OnWidgetTapped);
        }

        var statusBtn = abilityStatusText != null && abilityStatusText.transform.parent != transform
            ? abilityStatusText.transform.parent.GetComponent<Button>()
            : null;

        if (statusBtn != null && statusBtn != panelButton)
        {
            statusBtn.onClick.RemoveAllListeners();
            statusBtn.onClick.AddListener(OnWidgetTapped);
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        if (!_subscribed)
        {
            // Initial baseline refresh if controller is not yet ready
            Refresh(CreatureType.Turtle, 2, 2);
        }
    }

    private void Update()
    {
        if (!_subscribed)
        {
            TrySubscribe();
        }
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;
        if (CreatureAbilityController.Instance != null)
        {
            CreatureAbilityController.Instance.OnChargesChanged -= OnChargesChanged;
            CreatureAbilityController.Instance.OnAbilityUsed -= OnAbilityUsed;
            CreatureAbilityController.Instance.OnChargesChanged += OnChargesChanged;
            CreatureAbilityController.Instance.OnAbilityUsed += OnAbilityUsed;
            _subscribed = true;

            Refresh(
                CreatureAbilityController.Instance.activeCreature,
                CreatureAbilityController.Instance.abilityUsesRemaining,
                CreatureAbilityController.Instance.maxAbilityUses
            );
        }
    }

    private void OnDestroy()
    {
        if (CreatureAbilityController.Instance != null)
        {
            CreatureAbilityController.Instance.OnChargesChanged -= OnChargesChanged;
            CreatureAbilityController.Instance.OnAbilityUsed -= OnAbilityUsed;
        }
        _subscribed = false;

        if (_floatingTooltipObj != null) Destroy(_floatingTooltipObj);
        if (_instance == this) _instance = null;
    }

    /// <summary>
    /// Helper to find component in any child under this hierarchy regardless of depth or restructuring.
    /// </summary>
    private T FindComponentRecursive<T>(string childName) where T : Component
    {
        var transforms = GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
        {
            if (t != null && t.name.Equals(childName, StringComparison.OrdinalIgnoreCase))
            {
                var comp = t.GetComponent<T>();
                if (comp != null) return comp;
            }
        }
        return null;
    }

    /// <summary>
    /// Instantiates the modular CompanionAbilityHUD prefab under the given parent Transform.
    /// Used as a safe runtime fallback if the widget is not already placed in the scene.
    /// </summary>
    public static CreatureAbilityHUD CreateWidget(Transform parent)
    {
        if (parent == null) return null;

        var prefab = Resources.Load<GameObject>("Prefabs/UI/CompanionAbilityHUD");
        if (prefab != null)
        {
            var inst = Instantiate(prefab, parent, false);
            inst.name = "CompanionAbilityHUD";
            var hud = inst.GetComponent<CreatureAbilityHUD>();
            hud?.AutoDiscoverReferences();
            return hud;
        }
        return null;
    }

    /// <summary>
    /// Discovers child references automatically with robust fallbacks for user hierarchy modifications.
    /// </summary>
    public void AutoDiscoverReferences()
    {
        if (rootPanel == null) rootPanel = GetComponent<RectTransform>();

        if (panelBackground == null)
            panelBackground = transform.Find("AbilityPanelBackground")?.GetComponent<Image>()
                           ?? transform.Find("Background")?.GetComponent<Image>();

        if (panelDecoration == null)
            panelDecoration = transform.Find("PanelDecoration")?.GetComponent<Image>();

        if (companionIconContainer == null)
            companionIconContainer = transform.Find("CompanionIconContainer")?.GetComponent<RectTransform>();

        if (companionIcon == null)
            companionIcon = transform.Find("CompanionIconContainer/CompanionIcon")?.GetComponent<Image>()
                         ?? transform.Find("CompanionIcon")?.GetComponent<Image>()
                         ?? FindComponentRecursive<Image>("CompanionIcon");

        if (iconGlow == null)
            iconGlow = transform.Find("CompanionIconContainer/IconGlow")?.GetComponent<Image>()
                    ?? transform.Find("IconGlow")?.GetComponent<Image>()
                    ?? FindComponentRecursive<Image>("IconGlow");

        if (abilityInformation == null)
            abilityInformation = transform.Find("AbilityInformation")?.GetComponent<RectTransform>();

        if (abilityNameText == null)
            abilityNameText = transform.Find("AbilityInformation/AbilityNameText")?.GetComponent<TextMeshProUGUI>()
                           ?? FindComponentRecursive<TextMeshProUGUI>("AbilityNameText");

        if (abilityDescriptionText == null)
            abilityDescriptionText = transform.Find("AbilityInformation/AbilityDescriptionText")?.GetComponent<TextMeshProUGUI>()
                                  ?? FindComponentRecursive<TextMeshProUGUI>("AbilityDescriptionText");

        if (abilityStatusText == null)
            abilityStatusText = transform.Find("background/AbilityStatusText")?.GetComponent<TextMeshProUGUI>()
                             ?? transform.Find("Background/AbilityStatusText")?.GetComponent<TextMeshProUGUI>()
                             ?? transform.Find("AbilityInformation/AbilityStatusText")?.GetComponent<TextMeshProUGUI>()
                             ?? FindComponentRecursive<TextMeshProUGUI>("AbilityStatusText");

        if (statusBadgeBackground == null && abilityStatusText != null && abilityStatusText.transform.parent != transform)
        {
            statusBadgeBackground = abilityStatusText.transform.parent.GetComponent<Image>();
        }
        if (statusBadgeBackground == null)
        {
            statusBadgeBackground = transform.Find("background")?.GetComponent<Image>()
                                 ?? transform.Find("Background")?.GetComponent<Image>();
        }

        if (chargesContainer == null)
            chargesContainer = transform.Find("ChargesContainer")?.GetComponent<RectTransform>();

        if (chargesBackground == null)
            chargesBackground = transform.Find("ChargesContainer/ChargesBackground")?.GetComponent<Image>()
                             ?? chargesContainer?.GetComponent<Image>()
                             ?? FindComponentRecursive<Image>("ChargesBackground");

        if (chargesIcon == null)
            chargesIcon = transform.Find("ChargesContainer/ChargesIcon")?.GetComponent<Image>()
                       ?? FindComponentRecursive<Image>("ChargesIcon");

        if (chargesCountText == null)
            chargesCountText = transform.Find("ChargesContainer/ChargesCountText")?.GetComponent<TextMeshProUGUI>()
                            ?? chargesContainer?.GetComponentInChildren<TextMeshProUGUI>()
                            ?? FindComponentRecursive<TextMeshProUGUI>("ChargesCountText");

        if (abilityAvailabilityIndicator == null)
            abilityAvailabilityIndicator = transform.Find("AbilityAvailabilityIndicator")?.GetComponent<Image>()
                                        ?? FindComponentRecursive<Image>("AbilityAvailabilityIndicator");

        if (abilityPulseEffect == null)
            abilityPulseEffect = transform.Find("AbilityPulseEffect")?.GetComponent<Image>()
                              ?? FindComponentRecursive<Image>("AbilityPulseEffect");

        if (abilityFeedbackEffect == null)
            abilityFeedbackEffect = transform.Find("AbilityFeedbackEffect")?.GetComponent<CanvasGroup>()
                                 ?? FindComponentRecursive<CanvasGroup>("AbilityFeedbackEffect");

        if (panelButton == null)
            panelButton = GetComponent<Button>();

        // Disable conflicting UIGlowPulse components on abilityPulseEffect and iconGlow so GlowPulseRoutine controls them cleanly
        if (abilityPulseEffect != null)
        {
            var gp = abilityPulseEffect.GetComponent<UIGlowPulse>();
            if (gp != null) gp.enabled = false;
        }
        if (iconGlow != null)
        {
            var gp = iconGlow.GetComponent<UIGlowPulse>();
            if (gp != null) gp.enabled = false;
        }
    }

    private void CaptureInitialColors()
    {
        if (panelBackground != null) _initialPanelBgColor = panelBackground.color;
        if (abilityAvailabilityIndicator != null) _initialIndicatorColor = abilityAvailabilityIndicator.color;
        if (abilityPulseEffect != null)
        {
            _initialPulseColor = abilityPulseEffect.color;
            _initialPulseScale = abilityPulseEffect.transform.localScale;
        }
        if (iconGlow != null)
        {
            _initialIconGlowColor = iconGlow.color;
            _initialIconGlowScale = iconGlow.transform.localScale;
        }
        if (statusBadgeBackground != null)
        {
            _initialStatusBadgeColor = statusBadgeBackground.color;
        }
    }

    private void LoadDefaultSprites()
    {
        if (clownfishSprite == null) clownfishSprite = LoadCreatureSprite("Clownfish");
        if (turtleSprite == null)    turtleSprite    = LoadCreatureSprite("Turtle");
        if (octopusSprite == null)   octopusSprite   = LoadCreatureSprite("Octopus");
        if (jellyfishSprite == null) jellyfishSprite = LoadCreatureSprite("Jellyfish");

        if (chargeIconSprite == null)
        {
            chargeIconSprite = Resources.Load<Sprite>("Sprites/shell_pearl") 
                            ?? Resources.Load<Sprite>("UI/Icons/shell_pearl")
                            ?? Resources.Load<Sprite>("Sprites/icon_gem");
        }

        if (abilityAvailabilityIndicator != null && abilityAvailabilityIndicator.sprite == null)
        {
            abilityAvailabilityIndicator.sprite = Resources.Load<Sprite>("UI/ability_available_indicator")
                                               ?? Resources.Load<Sprite>("UI/Icons/ability_available_indicator");
            abilityAvailabilityIndicator.preserveAspect = true;
        }

        if (abilityPulseEffect != null && abilityPulseEffect.sprite == null)
        {
            abilityPulseEffect.sprite = Resources.Load<Sprite>("UI/ability_pulse_ring")
                                     ?? Resources.Load<Sprite>("UI/Icons/ability_pulse_ring");
            abilityPulseEffect.preserveAspect = true;
        }

        if (abilityFeedbackEffect != null)
        {
            var fbImg = abilityFeedbackEffect.GetComponent<Image>() ?? abilityFeedbackEffect.GetComponentInChildren<Image>();
            if (fbImg != null && fbImg.sprite == null)
            {
                fbImg.sprite = Resources.Load<Sprite>("UI/ability_feedback_energy_ring")
                            ?? Resources.Load<Sprite>("UI/Icons/ability_feedback_energy_ring");
                fbImg.preserveAspect = true;
            }
        }
    }

    private Sprite LoadCreatureSprite(string name)
    {
        Sprite s = Resources.Load<Sprite>($"Characters/{name}");
        if (s == null)
        {
            Sprite[] subs = Resources.LoadAll<Sprite>($"Characters/{name}");
            if (subs != null && subs.Length > 0) s = subs[0];
        }
        if (s == null) s = Resources.Load<Sprite>($"Sprites/{name}");
        return s;
    }

    private void OnChargesChanged(CreatureType creature, int remaining, int max)
    {
        Refresh(creature, remaining, max);
    }

    private void OnAbilityUsed(CreatureType creature)
    {
        PlayAbilityFeedback();
    }

    /// <summary>
    /// Synchronizes the HUD display with the active companion creature and ability charge state.
    /// </summary>
    public void Refresh(CreatureType creature, int currentCharges, int maxCharges)
    {
        _currentCreature = creature;

        // 1. Companion Sprite
        if (companionIcon != null)
        {
            companionIcon.sprite = GetSpriteForCreature(creature);
            companionIcon.preserveAspect = true;
        }

        // 2. Creature Ability Info (Exact definitions per Phase 2 requirements)
        GetCompanionAbilityInfo(creature, out string aName, out string aDesc, out string targetDesc);

        if (abilityNameText != null)        abilityNameText.text = aName;
        if (abilityDescriptionText != null) abilityDescriptionText.text = aDesc;

        // 3. Charge Counter Text & Icon
        bool isPassive = (creature == CreatureType.Clownfish);
        bool hasCharges = (currentCharges > 0);

        if (chargesCountText != null)
        {
            if (isPassive)
            {
                chargesCountText.text = "Rescuer Active";
            }
            else
            {
                chargesCountText.text = $"{currentCharges}/{maxCharges} Charges Left";
            }
        }

        if (chargesIcon != null && chargesIcon.sprite == null && chargeIconSprite != null)
        {
            chargesIcon.sprite = chargeIconSprite;
            chargesIcon.preserveAspect = true;
        }

        // Animate counter punch scale on charge change
        if (_lastCharges != -1 && _lastCharges != currentCharges)
        {
            TriggerPunchScale();
        }
        _lastCharges = currentCharges;

        // 4. Status Text & Availability Indicator
        if (abilityStatusText != null)
        {
            if (isPassive)
            {
                abilityStatusText.text = "PASSIVE";
                abilityStatusText.color = passiveColor;
            }
            else
            {
                abilityStatusText.text = hasCharges ? "Ready" : "Empty";
                abilityStatusText.color = hasCharges ? readyColor : depletedColor;
            }
        }

        if (statusBadgeBackground != null)
        {
            statusBadgeBackground.color = (hasCharges || isPassive)
                ? _initialStatusBadgeColor
                : new Color(_initialStatusBadgeColor.r * 0.7f, _initialStatusBadgeColor.g * 0.7f, _initialStatusBadgeColor.b * 0.7f, 0.65f);
        }

        if (abilityAvailabilityIndicator != null)
        {
            if (abilityAvailabilityIndicator.sprite != null)
            {
                abilityAvailabilityIndicator.color = (hasCharges || isPassive)
                    ? _initialIndicatorColor
                    : new Color(_initialIndicatorColor.r * 0.5f, _initialIndicatorColor.g * 0.5f, _initialIndicatorColor.b * 0.5f, _initialIndicatorColor.a * 0.55f);
            }
            else
            {
                abilityAvailabilityIndicator.color = isPassive ? passiveColor : (hasCharges ? readyColor : depletedColor);
            }
        }

        // 5. Panel Background Color & Opacity (preserve user's Inspector color)
        if (panelBackground != null)
        {
            if (panelBackground.sprite != null)
            {
                panelBackground.color = (hasCharges || isPassive)
                    ? _initialPanelBgColor
                    : new Color(_initialPanelBgColor.r * 0.72f, _initialPanelBgColor.g * 0.75f, _initialPanelBgColor.b * 0.82f, _initialPanelBgColor.a * 0.85f);
            }
            else
            {
                panelBackground.color = (hasCharges || isPassive) ? panelNormalColor : panelDepletedColor;
            }
        }

        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = (hasCharges || isPassive) ? 1.0f : disabledOpacity;
        }

        // 6. Glow Pulse Handling
        UpdateGlowState(hasCharges && !isPassive && pulseWhenReady);
    }

    private Sprite GetSpriteForCreature(CreatureType creature)
    {
        switch (creature)
        {
            case CreatureType.Turtle:    return turtleSprite ?? LoadCreatureSprite("Turtle");
            case CreatureType.Octopus:   return octopusSprite ?? LoadCreatureSprite("Octopus");
            case CreatureType.Jellyfish: return jellyfishSprite ?? LoadCreatureSprite("Jellyfish");
            case CreatureType.Clownfish:
            default:                     return clownfishSprite ?? LoadCreatureSprite("Clownfish");
        }
    }

    private void GetCompanionAbilityInfo(CreatureType creature, out string aName, out string aDesc, out string targetDesc)
    {
        switch (creature)
        {
            case CreatureType.Turtle:
                aName = "Shell Smash";
                aDesc = "Clear Breakable Rocks";
                targetDesc = "Tap Breakable Rock obstacles to smash them!";
                break;
            case CreatureType.Octopus:
                aName = "Ink Clear";
                aDesc = "Neutralize Inked Hazards";
                targetDesc = "Tap Inked Hazards to clear them!";
                break;
            case CreatureType.Jellyfish:
                aName = "Electric Shock";
                aDesc = "Interact with Electric Gates";
                targetDesc = "Tap Electric Gates to energize & open them!";
                break;
            case CreatureType.Clownfish:
            default:
                aName = "Water Swimmer";
                aDesc = "Rescued Friend";
                targetDesc = "Rescued swimmer flows through completed pipes!";
                break;
        }
    }

    private void UpdateGlowState(bool enableGlow)
    {
        if (abilityPulseEffect != null)
        {
            abilityPulseEffect.gameObject.SetActive(enableGlow);
        }

        if (iconGlow != null)
        {
            iconGlow.gameObject.SetActive(enableGlow);
        }

        if (enableGlow)
        {
            if (_glowPulseCoroutine == null)
            {
                _glowPulseCoroutine = StartCoroutine(GlowPulseRoutine());
            }
        }
        else
        {
            if (_glowPulseCoroutine != null)
            {
                StopCoroutine(_glowPulseCoroutine);
                _glowPulseCoroutine = null;
            }
        }
    }

    private IEnumerator GlowPulseRoutine()
    {
        while (enabled)
        {
            float t = (Mathf.Sin(Time.unscaledTime * 3.8f) + 1f) * 0.5f;
            float alpha = Mathf.Lerp(0.25f, 0.85f, t);

            if (abilityPulseEffect != null)
            {
                abilityPulseEffect.color = new Color(_initialPulseColor.r, _initialPulseColor.g, _initialPulseColor.b, alpha);
                float s = Mathf.Lerp(0.98f, 1.05f, t);
                abilityPulseEffect.transform.localScale = _initialPulseScale * s;
            }
            if (iconGlow != null)
            {
                iconGlow.color = new Color(_initialIconGlowColor.r, _initialIconGlowColor.g, _initialIconGlowColor.b, alpha * 0.75f);
                float s = Mathf.Lerp(0.97f, 1.03f, t);
                iconGlow.transform.localScale = _initialIconGlowScale * s;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Subtle punch-scale bounce on the widget when an ability is used or restored.
    /// </summary>
    public void PlayAbilityFeedback()
    {
        TriggerPunchScale();
        if (abilityFeedbackEffect != null)
        {
            StartCoroutine(FlashFeedbackRoutine());
        }
    }

    private IEnumerator FlashFeedbackRoutine()
    {
        if (abilityFeedbackEffect == null) yield break;

        Transform fbT = abilityFeedbackEffect.transform;
        Vector3 startScale = Vector3.one * 0.9f;
        Vector3 endScale   = Vector3.one * 1.25f;

        abilityFeedbackEffect.alpha = 1f;
        float elapsed = 0f;
        float dur = 0.38f;

        while (elapsed < dur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / dur);

            // Ease-out cubic for explosive luminous burst
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            if (fbT != null)
            {
                fbT.localScale = Vector3.Lerp(startScale, endScale, ease);
            }

            // Smooth fade out
            abilityFeedbackEffect.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        abilityFeedbackEffect.alpha = 0f;
        if (fbT != null) fbT.localScale = Vector3.one;
    }

    private void TriggerPunchScale()
    {
        if (!gameObject.activeInHierarchy) return;
        if (_punchCoroutine != null) StopCoroutine(_punchCoroutine);
        _punchCoroutine = StartCoroutine(PunchScaleRoutine());
    }

    private IEnumerator PunchScaleRoutine()
    {
        Vector3 baseScale = Vector3.one;
        float elapsed = 0f;

        while (elapsed < punchScaleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / punchScaleDuration;
            float bounce = 1f + Mathf.Sin(t * Mathf.PI) * punchScaleAmount;
            transform.localScale = baseScale * bounce;
            yield return null;
        }

        transform.localScale = baseScale;
    }

    /// <summary>
    /// Displays a helpful floating instruction banner when the companion widget is tapped.
    /// </summary>
    public void OnWidgetTapped()
    {
        AudioManager.Instance?.PlayButtonClick();
        TriggerPunchScale();

        if (abilityFeedbackEffect != null)
        {
            StartCoroutine(FlashFeedbackRoutine());
        }

        HighlightCompatibleObstacles();

        if (!showFloatingTooltip) return;

        GetCompanionAbilityInfo(_currentCreature, out string aName, out string aDesc, out string targetDesc);

        string hintMsg;
        if (_currentCreature == CreatureType.Clownfish)
        {
            hintMsg = $"Clownfish: Follows completed water pipes to safety!";
        }
        else
        {
            int remaining = CreatureAbilityController.Instance != null ? CreatureAbilityController.Instance.abilityUsesRemaining : 0;
            hintMsg = $"{_currentCreature} - {aName} ({remaining} left)\n{targetDesc}";
        }

        ShowFloatingTooltip(hintMsg);
    }

    /// <summary>
    /// Highlights matching obstacles on the board to clearly inform player where to use the active ability.
    /// </summary>
    public void HighlightCompatibleObstacles()
    {
        var gates = FindObjectsOfType<GateComponent>();
        if (gates == null || gates.Length == 0) return;

        foreach (var g in gates)
        {
            if (g == null || g.IsOpen) continue;
            bool isCompatible = false;
            if (_currentCreature == CreatureType.Turtle && g.gateType == GateType.BreakableRock) isCompatible = true;
            else if (_currentCreature == CreatureType.Octopus && g.gateType == GateType.InkedHazard) isCompatible = true;
            else if (_currentCreature == CreatureType.Jellyfish && g.gateType == GateType.ElectricGate) isCompatible = true;

            if (isCompatible)
            {
                g.ShowHint("Tap me to use ability!");
            }
        }
    }

    private void ShowFloatingTooltip(string message)
    {
        if (_tooltipCoroutine != null) StopCoroutine(_tooltipCoroutine);
        if (_floatingTooltipObj != null) Destroy(_floatingTooltipObj);

        _tooltipCoroutine = StartCoroutine(FloatingTooltipRoutine(message));
    }

    private IEnumerator FloatingTooltipRoutine(string message)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) yield break;

        _floatingTooltipObj = new GameObject("AbilityTooltip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        _floatingTooltipObj.transform.SetParent(canvas.transform, false);

        var rt = _floatingTooltipObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-40f, 115f);
        rt.sizeDelta = new Vector2(320f, 65f);

        var img = _floatingTooltipObj.GetComponent<Image>();
        img.color = new Color(0.02f, 0.12f, 0.24f, 0.95f);

        var textObj = new GameObject("TooltipText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(_floatingTooltipObj.transform, false);
        var textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = new Vector2(10f, 5f);
        textRt.offsetMax = new Vector2(-10f, -5f);

        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = message;
        tmp.fontSize = 17;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;

        var cg = _floatingTooltipObj.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / 0.15f);
            yield return null;
        }
        cg.alpha = 1f;

        yield return new WaitForSecondsRealtime(2.5f);

        elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / 0.25f);
            yield return null;
        }

        if (_floatingTooltipObj != null)
        {
            Destroy(_floatingTooltipObj);
            _floatingTooltipObj = null;
        }
    }
}
