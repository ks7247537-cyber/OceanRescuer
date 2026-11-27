using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Deep Ocean Pressure Meter UI for Ocean Rescuers (World 1 ONLY).
/// Displays a hydraulic underwater pressure gauge:
/// - Dynamic fill level showing live deep-sea water pressure (PSI).
/// - Moving glowing bubble pressure indicator needle.
/// - Distinct color transitions: Low (Cyan), Optimal (Emerald), High (Amber), Warning (Coral Red).
/// - Warning icon with pulsing alert when backpressure becomes dangerous.
/// - Clear, readable state status text (FLOW RESTRICTED, OPTIMAL FLOW, HIGH PRESSURE, OVERPRESSURE).
/// 
/// Strictly isolated to World 1 (Deep Ocean). Completely hidden in World 0 & World 2.
/// Fully editable in Unity Hierarchy, Inspector, and Prefab Mode.
/// </summary>
[DisallowMultipleComponent]
public class DeepOceanPressureHUD : MonoBehaviour
{
    private static DeepOceanPressureHUD _instance;
    public static DeepOceanPressureHUD Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<DeepOceanPressureHUD>(true);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Hierarchy Child References")]
    [Tooltip("Root panel RectTransform.")]
    public RectTransform pressurePanel;

    [Tooltip("Background frame of the pressure panel.")]
    public Image panelBackground;

    [Tooltip("Gauge container holding background and fill.")]
    public RectTransform pressureGauge;

    [Tooltip("Background track of the pressure gauge.")]
    public Image gaugeBackground;

    [Tooltip("Filled image representing current pressure level.")]
    public Image gaugeFill;

    [Tooltip("Moving indicator icon (e.g. bubble marker).")]
    public RectTransform pressureIndicator;

    [Tooltip("Text displaying numerical pressure in PSI.")]
    public TextMeshProUGUI pressureValueText;

    [Tooltip("Text displaying current pressure status description.")]
    public TextMeshProUGUI pressureStateText;

    [Tooltip("Warning indicator icon for high/danger pressure.")]
    public Image pressureWarningIcon;

    [Tooltip("Glow / pulse feedback effect.")]
    public GameObject pressureFeedbackEffect;

    [Header("State Colors (Inspector Configurable)")]
    public Color lowColor = new Color(0.15f, 0.75f, 1.0f, 1.0f);     // Electric Cyan
    public Color optimalColor = new Color(0.2f, 1.0f, 0.55f, 1.0f);   // Emerald Green
    public Color highColor = new Color(1.0f, 0.7f, 0.15f, 1.0f);      // Amber Orange
    public Color warningColor = new Color(1.0f, 0.25f, 0.25f, 1.0f);   // Coral Red

    [Header("Animation Settings")]
    [SerializeField] private float smoothSpeed = 5.0f;
    [SerializeField] private float gaugeTrackWidth = 140f;
    [SerializeField] private float needleRestRatio = 0.45f;
    [SerializeField] private float needleSweepDegrees = 140f;

    private float _displayedPressure = 45f;
    private float _targetPressure = 45f;
    private float _maxPressure = 100f;
    private DeepOceanPressureManager.PressureState _currentState = DeepOceanPressureManager.PressureState.Optimal;
    private bool _hasPlayedWarningSound = false;

    private Color _initialGaugeFillColor = Color.white;
    private Quaternion _initialIndicatorRotation = Quaternion.identity;
    private Color _initialValueTextColor = Color.white;

    public bool IsInDeepOcean
    {
        get
        {
            if (GameplayBootstrapper.CurrentLevelData != null && !string.IsNullOrEmpty(GameplayBootstrapper.CurrentLevelData.worldName))
            {
                return GameplayBootstrapper.CurrentLevelData.worldName.IndexOf("Deep", System.StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return (SaveManager.GetSelectedWorld() == 1);
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        AutoWireReferences();
        CaptureInitialVisuals();

        if (!IsInDeepOcean)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void CaptureInitialVisuals()
    {
        if (gaugeFill != null) _initialGaugeFillColor = gaugeFill.color;
        if (pressureIndicator != null) _initialIndicatorRotation = pressureIndicator.localRotation;
        if (pressureValueText != null) _initialValueTextColor = pressureValueText.color;
    }

    private void Start()
    {
        if (!IsInDeepOcean)
        {
            gameObject.SetActive(false);
            return;
        }

        SubscribeToManager();
        RefreshUI(immediate: true);
    }

    private void OnEnable()
    {
        if (!IsInDeepOcean && SaveManager.GetSelectedWorld() != 1)
        {
            gameObject.SetActive(false);
            return;
        }

        SubscribeToManager();
        RefreshUI(immediate: true);
    }

    private void OnDisable()
    {
        if (DeepOceanPressureManager.Instance != null)
        {
            DeepOceanPressureManager.Instance.OnPressureUpdated -= HandlePressureUpdated;
        }
    }

    private void SubscribeToManager()
    {
        var mgr = DeepOceanPressureManager.Instance;
        if (mgr != null)
        {
            mgr.OnPressureUpdated -= HandlePressureUpdated;
            mgr.OnPressureUpdated += HandlePressureUpdated;
            _targetPressure = mgr.CurrentPressure;
            _maxPressure = mgr.MaxPressure;
            _currentState = mgr.CurrentState;
        }
    }

    private void Update()
    {
        if (!IsInDeepOcean) return;

        var mgr = DeepOceanPressureManager.Instance;
        if (mgr != null)
        {
            _targetPressure = mgr.CurrentPressure;
            _maxPressure = mgr.MaxPressure;
            _currentState = mgr.CurrentState;
        }

        // Smoothly interpolate displayed pressure
        _displayedPressure = Mathf.Lerp(_displayedPressure, _targetPressure, Time.unscaledDeltaTime * smoothSpeed);

        UpdateGaugeVisuals();
    }

    /// <summary>
    /// Auto-discovers child components by canonical hierarchy names if unassigned.
    /// </summary>
    public void AutoWireReferences()
    {
        if (pressurePanel == null)
            pressurePanel = GetComponent<RectTransform>();

        if (panelBackground == null)
            panelBackground = transform.Find("PressurePanel")?.GetComponent<Image>() ?? GetComponent<Image>();

        Transform gaugeTr = transform.Find("PressureGauge");
        if (gaugeTr != null)
        {
            pressureGauge = gaugeTr.GetComponent<RectTransform>();
            if (gaugeBackground == null) gaugeBackground = gaugeTr.Find("GaugeBackground")?.GetComponent<Image>();
            if (gaugeFill == null) gaugeFill = gaugeTr.Find("GaugeFill")?.GetComponent<Image>();
            try
            {
                var foundIndicator = gaugeTr.Find("PressureIndicator")?.GetComponent<RectTransform>();
                if (foundIndicator != null) pressureIndicator = foundIndicator;
            }
            catch
            {
                pressureIndicator = null;
            }
            if (pressureValueText == null) pressureValueText = gaugeTr.Find("PressureValueText")?.GetComponent<TextMeshProUGUI>();
        }

        if (pressureStateText == null)
            pressureStateText = transform.Find("PressureStateText")?.GetComponent<TextMeshProUGUI>();

        if (pressureWarningIcon == null)
            pressureWarningIcon = transform.Find("PressureWarningIcon")?.GetComponent<Image>();

        if (pressureFeedbackEffect == null)
            pressureFeedbackEffect = transform.Find("PressureFeedbackEffect")?.gameObject;
    }

    private void HandlePressureUpdated(float current, float max, DeepOceanPressureManager.PressureState state)
    {
        _targetPressure = current;
        _maxPressure = max;
        _currentState = state;

        if (state == DeepOceanPressureManager.PressureState.Warning && !_hasPlayedWarningSound)
        {
            _hasPlayedWarningSound = true;
            AudioManager.Instance?.PlayFail();
        }
        else if (state != DeepOceanPressureManager.PressureState.Warning)
        {
            _hasPlayedWarningSound = false;
        }
    }

    public void RefreshUI(bool immediate)
    {
        if (DeepOceanPressureManager.Instance != null)
        {
            _targetPressure = DeepOceanPressureManager.Instance.CurrentPressure;
            _maxPressure = DeepOceanPressureManager.Instance.MaxPressure;
            _currentState = DeepOceanPressureManager.Instance.CurrentState;
        }

        if (immediate)
        {
            _displayedPressure = _targetPressure;
        }

        UpdateGaugeVisuals();
    }

    private void UpdateGaugeVisuals()
    {
        float ratio = Mathf.Clamp01(_displayedPressure / Mathf.Max(1f, _maxPressure));

        // 1. Update Fill (preserve user's Scene/Inspector gauge artwork & colors without blue/state tinting)
        if (gaugeFill != null)
        {
            if (gaugeFill.type == Image.Type.Filled)
            {
                gaugeFill.fillAmount = ratio;
            }
            gaugeFill.color = _initialGaugeFillColor;
        }

        // 2. Rotate Dial Needle smoothly around its pivot without overwriting Scene position
        try
        {
            if (pressureIndicator != null && pressureIndicator.gameObject != null)
            {
                float deltaAngle = (needleRestRatio - ratio) * needleSweepDegrees;
                pressureIndicator.localRotation = _initialIndicatorRotation * Quaternion.Euler(0f, 0f, deltaAngle);
            }
        }
        catch
        {
            pressureIndicator = null;
        }

        // 3. Update Pressure Value Text (preserve user's Inspector-configured text color)
        if (pressureValueText != null)
        {
            pressureValueText.text = $"{Mathf.RoundToInt(_displayedPressure)} <size=70%>PSI</size>";
            pressureValueText.color = _initialValueTextColor;
        }

        // 4. Update Pressure State Description
        if (pressureStateText != null)
        {
            pressureStateText.text = GetStateDescription(_currentState);
            pressureStateText.color = GetStateColor(_currentState);
        }

        // 5. Update Warning Icon & Feedback
        bool isWarning = _currentState == DeepOceanPressureManager.PressureState.Warning || _currentState == DeepOceanPressureManager.PressureState.High;
        if (pressureWarningIcon != null)
        {
            pressureWarningIcon.gameObject.SetActive(isWarning);
            if (isWarning)
            {
                pressureWarningIcon.color = (_currentState == DeepOceanPressureManager.PressureState.Warning)
                    ? warningColor
                    : highColor;
            }
        }

        if (pressureFeedbackEffect != null)
        {
            pressureFeedbackEffect.SetActive(isWarning);
        }
    }

    private Color GetStateColor(DeepOceanPressureManager.PressureState state)
    {
        switch (state)
        {
            case DeepOceanPressureManager.PressureState.Low:     return lowColor;
            case DeepOceanPressureManager.PressureState.Optimal: return optimalColor;
            case DeepOceanPressureManager.PressureState.High:    return highColor;
            case DeepOceanPressureManager.PressureState.Warning: return warningColor;
            default: return optimalColor;
        }
    }

    private string GetStateDescription(DeepOceanPressureManager.PressureState state)
    {
        switch (state)
        {
            case DeepOceanPressureManager.PressureState.Low:     return "<color=#00C8FF>FLOW LOW</color>";
            case DeepOceanPressureManager.PressureState.Optimal: return "<color=#33FF88>STABLE</color>";
            case DeepOceanPressureManager.PressureState.High:    return "<color=#FFAA22>HIGH PSI</color>";
            case DeepOceanPressureManager.PressureState.Warning: return "<color=#FF3333>DANGER!</color>";
            default: return "STABLE";
        }
    }
}
