using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Deep Ocean Sonar Scan HUD controller (World 1 — Deep Ocean ONLY).
/// Allows the player to emit an exploratory acoustic sonar pulse that cuts through
/// the dark ocean abyss to sweep the gameplay board and ping/reveal hidden or murky
/// hazards, gates, water valves, beacons, and pressure relief boosters.
/// Strictly isolated to World 1. Automatically deactivates in World 0 & World 2.
/// 
/// Hierarchy:
/// SafeAreaContainer
/// └── DeepOceanSonarHUD
///     ├── SonarPanel (underwater glass panel background)
///     ├── SonarButton
///     │   ├── ButtonBackground (tactile cyan orb button)
///     │   ├── SonarIcon (acoustic radar pulse bubble)
///     │   ├── ButtonLabel ("SONAR SCAN")
///     │   ├── CooldownText
///     │   ├── CooldownFill
///     │   └── ActivationFeedback (floating scan status text)
///     └── SonarPulseEffect (expanding circular sonar wave)
/// </summary>
[DisallowMultipleComponent]
public class DeepOceanSonarHUD : MonoBehaviour
{
    private static DeepOceanSonarHUD _instance;
    public static DeepOceanSonarHUD Instance => _instance;

    [Header("UI Component References")]
    public Button sonarButton;
    public Image buttonBackground;
    public Image sonarIcon;
    public TextMeshProUGUI buttonLabel;
    public TextMeshProUGUI cooldownText;
    public Image cooldownFill;
    public GameObject activationFeedback;
    public TextMeshProUGUI feedbackText;
    public SonarPulseEffect pulseEffect;

    [Header("Sonar Configuration")]
    [Tooltip("Cooldown in seconds between sonar pulses.")]
    public float cooldownDuration = 8.0f;

    [Tooltip("Duration in seconds that revealed targets stay illuminated.")]
    public float pingDuration = 5.0f;

    [Header("Styling Colors (Inspector Editable)")]
    public Color readyColor      = new Color(0.0f, 0.85f, 1.0f, 1.0f);   // Bright cyan
    public Color cooldownColor   = new Color(0.35f, 0.45f, 0.55f, 0.7f); // Subdued slate
    public Color activeEchoColor = new Color(0.2f, 1.0f, 0.6f, 1.0f);    // Bioluminescent green

    private float _lastSonarTime = -999f;
    private Coroutine _feedbackCoroutine;
    private Color _initialButtonBgColor = Color.white;

    public bool IsInDeepOcean
    {
        get
        {
            // LevelData world name check has highest authority if loaded
            if (GameplayBootstrapper.CurrentLevelData != null && !string.IsNullOrEmpty(GameplayBootstrapper.CurrentLevelData.worldName))
            {
                return GameplayBootstrapper.CurrentLevelData.worldName.IndexOf("Deep", System.StringComparison.OrdinalIgnoreCase) >= 0;
            }

            // SaveManager selected world check (World 1 = Deep Ocean)
            int selectedWorld = SaveManager.GetSelectedWorld();
            return (selectedWorld == 1);
        }
    }

    public bool IsReady => (Time.unscaledTime - _lastSonarTime) >= cooldownDuration;
    public float RemainingCooldown => Mathf.Max(0f, cooldownDuration - (Time.unscaledTime - _lastSonarTime));

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        SetupReferences();
        if (buttonBackground != null) _initialButtonBgColor = buttonBackground.color;

        if (!IsInDeepOcean)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnEnable()
    {
        SetupButtonListener();
    }

    private void Start()
    {
        if (!IsInDeepOcean)
        {
            gameObject.SetActive(false);
            return;
        }

        SetupReferences();
        SetupButtonListener();
        UpdateVisuals(immediate: true);
    }

    private void SetupButtonListener()
    {
        if (sonarButton != null)
        {
            sonarButton.onClick.RemoveListener(OnSonarButtonClicked);
            sonarButton.onClick.AddListener(OnSonarButtonClicked);

            // Ensure SonarButton has a fallback click handler so no other component (e.g. UIButtonFeedback) can swallow it
            if (sonarButton.GetComponent<SonarButtonClickReceiver>() == null)
            {
                sonarButton.gameObject.AddComponent<SonarButtonClickReceiver>();
            }
        }
    }

    private void SetupReferences()
    {
        if (sonarButton == null)
            sonarButton = transform.Find("SonarButton")?.GetComponent<Button>() 
                       ?? GetComponentInChildren<Button>();

        if (buttonBackground == null && sonarButton != null)
            buttonBackground = sonarButton.transform.Find("ButtonBackground")?.GetComponent<Image>()
                            ?? sonarButton.GetComponent<Image>();

        // Ensure any untextured blank Image component on the root SonarButton does NOT obscure the buttonBackground
        if (sonarButton != null)
        {
            var selfImg = sonarButton.GetComponent<Image>();
            if (selfImg != null && selfImg != buttonBackground)
            {
                selfImg.enabled = false;
            }
            if (buttonBackground != null)
            {
                sonarButton.targetGraphic = buttonBackground;
            }
        }

        if (sonarIcon == null && sonarButton != null)
            sonarIcon = sonarButton.transform.Find("SonarIcon")?.GetComponent<Image>();

        if (sonarIcon != null && sonarIcon.sprite == null)
        {
            sonarIcon.sprite = Resources.Load<Sprite>("UI/Generated/sonar_scan_icon") 
                            ?? Resources.Load<Sprite>("Sprites/bubbles");
            sonarIcon.preserveAspect = true;
        }

        if (buttonLabel == null && sonarButton != null)
            buttonLabel = sonarButton.transform.Find("ButtonLabel")?.GetComponent<TextMeshProUGUI>();

        if (cooldownText == null && sonarButton != null)
            cooldownText = sonarButton.transform.Find("CooldownText")?.GetComponent<TextMeshProUGUI>();

        if (cooldownFill == null && sonarButton != null)
            cooldownFill = sonarButton.transform.Find("CooldownFill")?.GetComponent<Image>();

        if (cooldownFill != null)
        {
            if (cooldownFill.sprite == null && buttonBackground != null && buttonBackground.sprite != null)
            {
                cooldownFill.sprite = buttonBackground.sprite;
            }
            if (cooldownFill.sprite != null)
            {
                cooldownFill.type = Image.Type.Filled;
                cooldownFill.fillMethod = Image.FillMethod.Radial360;
                cooldownFill.fillOrigin = (int)Image.Origin360.Top;
                cooldownFill.fillClockwise = true;
                cooldownFill.preserveAspect = true;
                cooldownFill.raycastTarget = false;
                if (cooldownFill.color == Color.white)
                {
                    cooldownFill.color = new Color(0.05f, 0.25f, 0.45f, 0.72f);
                }
            }
            else
            {
                cooldownFill.enabled = false;
            }
        }

        if (activationFeedback == null)
        {
            var af = transform.Find("SonarButton/ActivationFeedback") 
                  ?? transform.Find("ActivationFeedback");
            if (af != null) activationFeedback = af.gameObject;
        }

        if (activationFeedback != null)
        {
            // Disable all Image components on activationFeedback to guarantee no rectangle box appears
            var fbImg = activationFeedback.GetComponent<Image>();
            if (fbImg != null) fbImg.enabled = false;
            foreach (var img in activationFeedback.GetComponentsInChildren<Image>(true))
            {
                img.enabled = false;
            }

            if (feedbackText == null)
                feedbackText = activationFeedback.GetComponentInChildren<TextMeshProUGUI>();

            if (feedbackText != null)
            {
                feedbackText.outlineWidth = 0.24f;
                feedbackText.outlineColor = new Color32(0, 15, 35, 230);
                feedbackText.alignment = TextAlignmentOptions.Center;
                feedbackText.raycastTarget = false;
            }
        }

        if (pulseEffect == null)
            pulseEffect = GetComponentInChildren<SonarPulseEffect>(true);
    }

    private void Update()
    {
        if (!IsInDeepOcean) return;

        UpdateVisuals(immediate: false);
    }

    private void UpdateVisuals(bool immediate)
    {
        float rem = RemainingCooldown;
        bool ready = rem <= 0f;

        if (sonarButton != null)
        {
            sonarButton.interactable = ready;
        }

        if (buttonBackground != null)
        {
            // Preserve user's Inspector-configured buttonBackground color when ready
            if (buttonBackground.sprite != null)
            {
                buttonBackground.color = ready
                    ? _initialButtonBgColor
                    : new Color(_initialButtonBgColor.r * 0.6f, _initialButtonBgColor.g * 0.6f, _initialButtonBgColor.b * 0.6f, _initialButtonBgColor.a * 0.8f);
            }
            else
            {
                buttonBackground.color = ready ? readyColor : cooldownColor;
            }
        }

        if (cooldownFill != null)
        {
            float fill = ready ? 0f : (rem / cooldownDuration);
            cooldownFill.fillAmount = fill;
            cooldownFill.gameObject.SetActive(!ready);
        }

        if (cooldownText != null)
        {
            if (ready)
            {
                cooldownText.text = "";
                cooldownText.gameObject.SetActive(false);
            }
            else
            {
                cooldownText.text = $"{Mathf.CeilToInt(rem)}s";
                cooldownText.gameObject.SetActive(true);
            }
        }

        // Idle pulsing animation on icon when ready
        if (ready && sonarIcon != null)
        {
            float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 3f);
            sonarIcon.transform.localScale = Vector3.one * pulse;
        }
    }

    public void OnSonarButtonClicked()
    {
        if (!IsInDeepOcean) return;

        if (!IsReady)
        {
            ShowFeedback($"<color=#FFA500>⏳ RECHARGING ({Mathf.CeilToInt(RemainingCooldown)}s)</color>");
            return;
        }

        ExecuteSonarScan();
    }

    /// <summary>
    /// Executes the full acoustic sonar scan across the puzzle grid.
    /// Emits expanding radar wave in gameplay area, detects all Deep Ocean obstacles,
    /// illuminates abyssal darkness, and initiates cooldown.
    /// </summary>
    public void ExecuteSonarScan()
    {
        // Cooldown starts immediately upon successful activation
        _lastSonarTime = Time.unscaledTime;

        // 1. Trigger visible circular expanding sonar wave effect in gameplay board and HUD
        if (pulseEffect != null)
        {
            pulseEffect.PlayPulse();
        }

        // 2. Illuminate abyssal darkness with acoustic resonance flare
        if (AbyssalDarknessController.Instance != null)
        {
            AbyssalDarknessController.Instance.TemporarilyIlluminate(pingDuration);
        }

        // 3. Audio feedback
        AudioManager.Instance?.PlayWaterFlow();

        // 4. Scan the grid for all Deep Ocean targets, Gates, Valves, Boosters, and Hazards
        int targetsPinged = 0;
        int concealedRevealed = 0;

        // A. Dedicated SonarTargets
        var targets = FindObjectsOfType<SonarTarget>(true);
        foreach (var t in targets)
        {
            if (t != null && t.gameObject.activeInHierarchy)
            {
                if (t.IsConcealed) concealedRevealed++;
                t.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // B. PressureLockGates (World 1 specific dual-pressure lock gates)
        var plGates = FindObjectsOfType<PressureLockGate>(true);
        foreach (var pl in plGates)
        {
            if (pl != null && pl.gameObject.activeInHierarchy && pl.GetComponent<SonarTarget>() == null)
            {
                var st = pl.gameObject.AddComponent<SonarTarget>();
                st.targetLabel = "Pressure Lock Gate";
                st.tacticalInfo = $"Maintain {pl.minActivationPressure:F0}-{pl.maxActivationPressure:F0} PSI";
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // C. Interactive Water Valves & Directional Valves
        var valves = FindObjectsOfType<WaterValve>(true);
        foreach (var v in valves)
        {
            if (v != null && v.gameObject.activeInHierarchy && v.GetComponent<SonarTarget>() == null)
            {
                var st = v.gameObject.AddComponent<SonarTarget>();
                st.targetLabel = v.valveType == ValveType.Directional ? "Directional Valve" : "Water Valve";
                st.tacticalInfo = v.isOpen ? "Valve Open (Flow Active)" : "Tap to Open Valve";
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // D. General GateComponents (BreakableRock, InkedHazard, ElectricGate, PressureGate, LockedGate)
        var gates = FindObjectsOfType<GateComponent>(true);
        foreach (var g in gates)
        {
            if (g != null && g.gameObject.activeInHierarchy && g.GetComponent<SonarTarget>() == null)
            {
                var st = g.gameObject.AddComponent<SonarTarget>();
                st.targetLabel = g.gateType switch
                {
                    GateType.BreakableRock => "Breakable Rock",
                    GateType.InkedHazard => "Inked Hazard",
                    GateType.ElectricGate => "Electric Gate",
                    GateType.PressureGate => $"Pressure Gate ({g.requiredPressure} PSI)",
                    GateType.PressureLockGate => "Pressure Lock Gate",
                    _ => "Locked Gate"
                };
                st.tacticalInfo = g.gateType switch
                {
                    GateType.BreakableRock => "Requires Turtle Shell Smash",
                    GateType.InkedHazard => "Requires Octopus Ink Clear",
                    GateType.ElectricGate => "Requires Jellyfish Shock",
                    GateType.PressureGate => "Accumulate Pipe Flow",
                    GateType.PressureLockGate => "Balance Hydraulic Pressure",
                    _ => "Open Linked Valve"
                };
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // E. CurrentBoosters
        var boosters = FindObjectsOfType<CurrentBooster>(true);
        foreach (var b in boosters)
        {
            if (b != null && b.gameObject.activeInHierarchy && b.GetComponent<SonarTarget>() == null)
            {
                var st = b.gameObject.AddComponent<SonarTarget>();
                string dir = b.boostDirection == 0 ? "Up" : b.boostDirection == 1 ? "Right" : b.boostDirection == 2 ? "Down" : "Left";
                st.targetLabel = "Current Booster";
                st.tacticalInfo = $"Flow {dir} → -15 PSI Relief";
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // F. Pressure Release Valves
        var prValves = FindObjectsOfType<PressureReleaseValve>(true);
        foreach (var prv in prValves)
        {
            if (prv != null && prv.gameObject.activeInHierarchy && prv.GetComponent<SonarTarget>() == null)
            {
                var st = prv.gameObject.AddComponent<SonarTarget>();
                st.targetLabel = "Release Valve";
                st.tacticalInfo = "Tap to Vent -30 PSI";
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // G. OxygenLeaks
        var leaks = FindObjectsOfType<OxygenLeak>(true);
        foreach (var lk in leaks)
        {
            if (lk != null && lk.gameObject.activeInHierarchy && lk.GetComponent<SonarTarget>() == null)
            {
                var st = lk.gameObject.AddComponent<SonarTarget>();
                st.targetLabel = "Oxygen Leak";
                st.tacticalInfo = lk.IsRepaired ? "Pipeline Sealed & Stabilized" : $"Tap to Repair (-{lk.pressureLoss:F0} PSI Loss)";
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // H. TremorZones
        var tremors = FindObjectsOfType<TremorZone>(true);
        foreach (var tz in tremors)
        {
            if (tz != null && tz.gameObject.activeInHierarchy && tz.GetComponent<SonarTarget>() == null)
            {
                var st = tz.gameObject.AddComponent<SonarTarget>();
                st.targetLabel = "Tremor Zone";
                st.tacticalInfo = "Unstable Trench (Tap on Warning to Stabilize)";
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // I. RouteDiversions
        var diversions = FindObjectsOfType<RouteDiversion>(true);
        foreach (var rd in diversions)
        {
            if (rd != null && rd.gameObject.activeInHierarchy && rd.GetComponent<SonarTarget>() == null)
            {
                var st = rd.gameObject.AddComponent<SonarTarget>();
                string dir = rd.restrictedDirection == 0 ? "North" : rd.restrictedDirection == 1 ? "East" : rd.restrictedDirection == 2 ? "South" : "West";
                st.targetLabel = "Route Diversion";
                st.tacticalInfo = $"{rd.hazardName}: Flow Blocked {dir} (Route Around)";
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // J. AbyssalBeacons
        var beacons = FindObjectsOfType<AbyssalBeacon>(true);
        foreach (var b in beacons)
        {
            if (b != null && b.gameObject.activeInHierarchy && b.GetComponent<SonarTarget>() == null)
            {
                var st = b.gameObject.AddComponent<SonarTarget>();
                st.targetLabel = "Abyssal Beacon";
                st.tacticalInfo = b.IsIgnited ? "Sanctuary Restored & Ignited" : "Route Water to Ignite Beacon";
                st.Ping(pingDuration);
                targetsPinged++;
            }
        }

        // 5. Short and clean tactical feedback banner
        if (concealedRevealed > 0)
        {
            ShowFeedback($"<color=#00FFFF>📡 {concealedRevealed} HAZARDS REVEALED</color>");
        }
        else if (targetsPinged > 0)
        {
            ShowFeedback($"<color=#38BDF8>📡 {targetsPinged} TARGETS PINGED</color>");
        }
        else
        {
            ShowFeedback("<color=#38BDF8>📡 SECTOR CLEAR</color>");
        }

        UpdateVisuals(immediate: true);
    }

    public void ShowFeedback(string message)
    {
        if (activationFeedback == null || feedbackText == null) return;

        if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
        _feedbackCoroutine = StartCoroutine(FeedbackRoutine(message));
    }

    private IEnumerator FeedbackRoutine(string message)
    {
        // Guarantee no background rectangle box renders
        var fbImg = activationFeedback.GetComponent<Image>();
        if (fbImg != null) fbImg.enabled = false;
        foreach (var childImg in activationFeedback.GetComponentsInChildren<Image>(true))
        {
            childImg.enabled = false;
        }

        activationFeedback.SetActive(true);
        feedbackText.text = message;
        feedbackText.outlineWidth = 0.24f;
        feedbackText.outlineColor = new Color32(0, 15, 35, 230);

        RectTransform fbRect = activationFeedback.GetComponent<RectTransform>();
        Vector2 startAnchorPos = new Vector2(0f, 18f);
        Vector2 endAnchorPos = new Vector2(0f, 44f);
        float elapsed = 0f;
        float duration = 2.0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            if (fbRect != null)
            {
                fbRect.anchoredPosition = Vector2.Lerp(startAnchorPos, endAnchorPos, t);
            }
            float alpha = t < 0.7f ? 1f : Mathf.Clamp01(1f - ((t - 0.7f) / 0.3f));
            feedbackText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        activationFeedback.SetActive(false);
    }
}

/// <summary>
/// Helper click receiver attached to SonarButton to ensure pointer click events
/// reliably trigger OnSonarButtonClicked even if other scripts attach to the same button.
/// </summary>
public class SonarButtonClickReceiver : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        var btn = GetComponent<Button>();
        if (btn != null && !btn.interactable) return;

        DeepOceanSonarHUD.Instance?.OnSonarButtonClicked();
    }
}
