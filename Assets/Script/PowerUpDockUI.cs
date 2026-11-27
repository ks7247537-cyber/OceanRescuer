using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Modular Gameplay Power-Up Dock UI for Ocean Rescuers.
/// Displays 3 accessible underwater-themed power-up buttons:
/// 1. Hint (ID: "Hint") -> triggers PowerUpService.Instance.TryUseHint()
/// 2. Super Wrench (ID: "Wrench") -> triggers PowerUpService.Instance.TryUseSuperWrench()
/// 3. Extra Moves (ID: "ExtraMoves") -> triggers PowerUpService.Instance.TryUseExtraMoves()
///
/// Fully synchronized with SaveManager & PowerUpService:
/// - Real-time inventory quantity badges.
/// - Clear disabled/dimmed visual state when count == 0.
/// - Tactile punch/feedback on tap via UIButtonFeedback.
/// - Floating feedback toast message on activation failure/success.
/// - Rapid tap debounce protection.
///
/// Completely editable in Unity Hierarchy, Inspector, and Prefab Mode.
/// </summary>
[DisallowMultipleComponent]
public class PowerUpDockUI : MonoBehaviour
{
    private static PowerUpDockUI _instance;
    public static PowerUpDockUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PowerUpDockUI>(true);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [System.Serializable]
    public class PowerUpButtonElements
    {
        public string powerUpId;
        public Button button;
        public Image icon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI countText;
        public GameObject quantityBadge;
        public CanvasGroup canvasGroup;
        public Image glowOutline;
    }

    [Header("Dock Layout References")]
    [Tooltip("Root RectTransform of the PowerUp Dock.")]
    public RectTransform dockRoot;

    [Tooltip("Background panel image (e.g. glossy blue container).")]
    public Image dockBackground;

    [Header("Power-Up Button References")]
    [Tooltip("Hint button elements.")]
    public PowerUpButtonElements hintButton = new PowerUpButtonElements { powerUpId = PowerUpService.POWERUP_HINT };

    [Tooltip("Super Wrench button elements.")]
    public PowerUpButtonElements wrenchButton = new PowerUpButtonElements { powerUpId = PowerUpService.POWERUP_WRENCH };

    [Tooltip("Extra Moves button elements.")]
    public PowerUpButtonElements extraMovesButton = new PowerUpButtonElements { powerUpId = PowerUpService.POWERUP_EXTRA_MOVES };

    [Header("Feedback Toast")]
    [Tooltip("Text displaying brief status/failure/success feedback.")]
    public TextMeshProUGUI feedbackToastText;

    [Tooltip("CanvasGroup for toast smooth fade-in/out.")]
    public CanvasGroup toastCanvasGroup;

    [Header("Settings")]
    [SerializeField] private float debounceCooldown = 0.35f;
    [SerializeField] private float disabledAlpha = 1.0f;
    [SerializeField] private float enabledAlpha = 1.0f;
    [SerializeField] private Color badgeNormalColor = new Color(0.2f, 0.7f, 1f, 1f);
    [SerializeField] private Color badgeZeroColor = new Color(0.35f, 0.45f, 0.55f, 0.95f);

    private float _lastActionTime = -10f;
    private Coroutine _toastCoroutine;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        AutoWireReferences();
        RegisterButtonListeners();
    }

    private void Start()
    {
        RefreshAll();
    }

    private void OnEnable()
    {
        if (PowerUpService.Instance != null)
        {
            PowerUpService.Instance.OnInventoryUpdated += HandleInventoryUpdated;
            PowerUpService.Instance.OnPowerUpActivated += HandlePowerUpActivated;
        }

        RefreshAll();
    }

    private void OnDisable()
    {
        if (PowerUpService.Instance != null)
        {
            PowerUpService.Instance.OnInventoryUpdated -= HandleInventoryUpdated;
            PowerUpService.Instance.OnPowerUpActivated -= HandlePowerUpActivated;
        }
    }

    /// <summary>
    /// Automatically discovers child UI elements by canonical names if unassigned.
    /// </summary>
    public void AutoWireReferences()
    {
        if (dockRoot == null) dockRoot = GetComponent<RectTransform>();
        if (dockBackground == null) dockBackground = transform.Find("DockBackground")?.GetComponent<Image>();

        WireButtonElements(ref hintButton, "HintButton", PowerUpService.POWERUP_HINT);
        WireButtonElements(ref wrenchButton, "WrenchButton", PowerUpService.POWERUP_WRENCH);
        WireButtonElements(ref extraMovesButton, "ExtraMovesButton", PowerUpService.POWERUP_EXTRA_MOVES);

        // Feedback toast
        if (feedbackToastText == null)
        {
            var toastObj = transform.Find("FeedbackToast");
            if (toastObj != null)
            {
                feedbackToastText = toastObj.GetComponent<TextMeshProUGUI>();
                toastCanvasGroup = toastObj.GetComponent<CanvasGroup>();
            }
        }
    }

    private void WireButtonElements(ref PowerUpButtonElements elem, string childName, string defaultId)
    {
        if (elem == null) elem = new PowerUpButtonElements();
        elem.powerUpId = defaultId;

        Transform btnTr = transform.Find(childName);
        if (btnTr == null) return;

        if (elem.button == null) elem.button = btnTr.GetComponent<Button>();
        if (elem.canvasGroup == null) elem.canvasGroup = btnTr.GetComponent<CanvasGroup>();
        if (elem.icon == null) elem.icon = btnTr.Find("Icon")?.GetComponent<Image>();
        if (elem.nameText == null) elem.nameText = btnTr.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        // Ensure any overlay or FeedbackEffect child is completely deactivated
        Transform feedbackTr = btnTr.Find("FeedbackEffect");
        if (feedbackTr != null)
        {
            var fImg = feedbackTr.GetComponent<Image>();
            if (fImg != null) fImg.enabled = false;
            var pulse = feedbackTr.GetComponent<UIGlowPulse>();
            if (pulse != null) pulse.enabled = false;
            feedbackTr.gameObject.SetActive(false);
        }
        elem.glowOutline = null;

        Transform badgeTr = btnTr.Find("QuantityBadge");
        if (badgeTr != null)
        {
            elem.quantityBadge = badgeTr.gameObject;
            if (elem.countText == null)
                elem.countText = badgeTr.Find("CountText")?.GetComponent<TextMeshProUGUI>() ?? badgeTr.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void RegisterButtonListeners()
    {
        if (hintButton?.button != null)
        {
            hintButton.button.onClick.RemoveAllListeners();
            hintButton.button.onClick.AddListener(OnHintClicked);
        }

        if (wrenchButton?.button != null)
        {
            wrenchButton.button.onClick.RemoveAllListeners();
            wrenchButton.button.onClick.AddListener(OnWrenchClicked);
        }

        if (extraMovesButton?.button != null)
        {
            extraMovesButton.button.onClick.RemoveAllListeners();
            extraMovesButton.button.onClick.AddListener(OnExtraMovesClicked);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Button Click Handlers
    // ─────────────────────────────────────────────────────────────────────────

    public void OnHintClicked()
    {
        if (!CanTriggerAction(out string blockedReason))
        {
            if (!string.IsNullOrEmpty(blockedReason)) ShowFeedbackToast(blockedReason, isSuccess: false);
            return;
        }

        _lastActionTime = Time.unscaledTime;
        var service = PowerUpService.Instance;
        if (service == null)
        {
            ShowFeedbackToast("PowerUpService unavailable", isSuccess: false);
            return;
        }

        bool success = service.TryUseHint(out string message);
        ShowFeedbackToast(message, isSuccess: success);
        if (success)
        {
            TriggerButtonPunch(hintButton);
        }
        else
        {
            AudioManager.Instance?.PlayFail();
        }

        RefreshAll();
    }

    public void OnWrenchClicked()
    {
        if (!CanTriggerAction(out string blockedReason))
        {
            if (!string.IsNullOrEmpty(blockedReason)) ShowFeedbackToast(blockedReason, isSuccess: false);
            return;
        }

        _lastActionTime = Time.unscaledTime;
        var service = PowerUpService.Instance;
        if (service == null)
        {
            ShowFeedbackToast("PowerUpService unavailable", isSuccess: false);
            return;
        }

        bool success = service.TryUseSuperWrench(out string message);
        ShowFeedbackToast(message, isSuccess: success);
        if (success)
        {
            TriggerButtonPunch(wrenchButton);
        }
        else
        {
            AudioManager.Instance?.PlayFail();
        }

        RefreshAll();
    }

    public void OnExtraMovesClicked()
    {
        if (!CanTriggerAction(out string blockedReason))
        {
            if (!string.IsNullOrEmpty(blockedReason)) ShowFeedbackToast(blockedReason, isSuccess: false);
            return;
        }

        _lastActionTime = Time.unscaledTime;
        var service = PowerUpService.Instance;
        if (service == null)
        {
            ShowFeedbackToast("PowerUpService unavailable", isSuccess: false);
            return;
        }

        bool success = service.TryUseExtraMoves(out string message);
        ShowFeedbackToast(message, isSuccess: success);
        if (success)
        {
            TriggerButtonPunch(extraMovesButton);
        }
        else
        {
            AudioManager.Instance?.PlayFail();
        }

        RefreshAll();
    }

    private bool CanTriggerAction(out string blockedReason)
    {
        if (Time.unscaledTime - _lastActionTime < debounceCooldown)
        {
            blockedReason = null; // silently ignore rapid spam
            return false;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            blockedReason = "Level is already completed!";
            return false;
        }

        blockedReason = null;
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inventory Synchronization & Visual Updates
    // ─────────────────────────────────────────────────────────────────────────

    public void RefreshAll()
    {
        int hintCount = PowerUpService.Instance != null ? PowerUpService.Instance.GetHintCount() : SaveManager.GetPowerUpCount(PowerUpService.POWERUP_HINT);
        int wrenchCount = PowerUpService.Instance != null ? PowerUpService.Instance.GetWrenchCount() : SaveManager.GetPowerUpCount(PowerUpService.POWERUP_WRENCH);
        int movesCount = PowerUpService.Instance != null ? PowerUpService.Instance.GetExtraMovesCount() : SaveManager.GetPowerUpCount(PowerUpService.POWERUP_EXTRA_MOVES);

        UpdateButtonVisuals(hintButton, hintCount, "Hint");
        UpdateButtonVisuals(wrenchButton, wrenchCount, "Wrench");
        UpdateButtonVisuals(extraMovesButton, movesCount, "+5 Moves");
    }

    private void UpdateButtonVisuals(PowerUpButtonElements elem, int count, string defaultName)
    {
        if (elem == null) return;

        // Count Text
        if (elem.countText != null)
        {
            elem.countText.text = count.ToString();
        }

        // Name Text
        if (elem.nameText != null && string.IsNullOrEmpty(elem.nameText.text))
        {
            elem.nameText.text = defaultName;
        }

        bool hasInventory = count > 0;

        // Button interactable: Always keep true so clicking always displays feedback/toast!
        if (elem.button != null)
        {
            elem.button.interactable = true;
        }

        // Keep 100% full opacity so power-up icons never look washed out or blurry!
        if (elem.canvasGroup != null)
        {
            elem.canvasGroup.alpha = 1.0f;
            elem.canvasGroup.interactable = true;
            elem.canvasGroup.blocksRaycasts = true;
        }

        // Preserve user's Inspector-configured icon color (do not overwrite with Color.white)

        // Keep overlays deactivated so power-up buttons always remain crystal clear
        if (elem.glowOutline != null)
        {
            elem.glowOutline.gameObject.SetActive(false);
        }

        // Badge color tint
        if (elem.quantityBadge != null)
        {
            var badgeImg = elem.quantityBadge.GetComponent<Image>();
            if (badgeImg != null)
            {
                badgeImg.color = hasInventory ? badgeNormalColor : badgeZeroColor;
            }
        }
    }

    private void HandleInventoryUpdated(string powerUpId, int newCount)
    {
        RefreshAll();
    }

    private void HandlePowerUpActivated(string powerUpId)
    {
        RefreshAll();
    }

    private void TriggerButtonPunch(PowerUpButtonElements elem)
    {
        if (elem?.button == null) return;
        StartCoroutine(PunchScaleRoutine(elem.button.transform));
    }

    private IEnumerator PunchScaleRoutine(Transform target)
    {
        Vector3 baseScale = Vector3.one;
        float elapsed = 0f;
        const float duration = 0.22f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float scale = 1f + 0.18f * Mathf.Sin(t * Mathf.PI);
            target.localScale = baseScale * scale;
            yield return null;
        }

        target.localScale = baseScale;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Feedback Toast Presentation
    // ─────────────────────────────────────────────────────────────────────────

    public void ShowFeedbackToast(string message, bool isSuccess)
    {
        if (string.IsNullOrEmpty(message)) return;

        if (feedbackToastText != null)
        {
            feedbackToastText.color = Color.white;
            feedbackToastText.outlineWidth = 0.22f;
            feedbackToastText.outlineColor = new Color32(0, 15, 35, 230);
            feedbackToastText.alignment = TextAlignmentOptions.Center;
            feedbackToastText.enableAutoSizing = true;
            feedbackToastText.fontSizeMin = 16f;
            feedbackToastText.fontSizeMax = 24f;
            feedbackToastText.raycastTarget = false;

            feedbackToastText.text = isSuccess 
                ? $"<color=#00FFFF><b>✓</b></color>  <b>{message}</b>" 
                : $"<color=#FFCC44><b>⚡</b></color>  <b>{message}</b>";

            if (_toastCoroutine != null) StopCoroutine(_toastCoroutine);
            _toastCoroutine = StartCoroutine(ToastFadeRoutine());
        }
        else
        {
            Debug.Log($"[PowerUpDockUI] Feedback: {message} (Success: {isSuccess})");
        }
    }

    private IEnumerator ToastFadeRoutine()
    {
        if (feedbackToastText == null) yield break;

        var toastObj = feedbackToastText.gameObject;

        // Guarantee no rectangle box or background image appears behind floating feedback text
        var toastBg = toastObj.transform.Find("ToastBackground");
        if (toastBg != null)
        {
            var bgImg = toastBg.GetComponent<Image>();
            if (bgImg != null) bgImg.enabled = false;
            toastBg.gameObject.SetActive(false);
        }
        var rootImg = toastObj.GetComponent<Image>();
        if (rootImg != null) rootImg.enabled = false;
        foreach (var img in toastObj.GetComponentsInChildren<Image>(true))
        {
            img.enabled = false;
        }

        toastObj.SetActive(true);

        if (toastCanvasGroup != null) toastCanvasGroup.alpha = 1f;

        // Smooth punch scale pop animation
        float punchTime = 0.20f;
        float elapsed = 0f;
        while (elapsed < punchTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / punchTime;
            float s = 1f + 0.12f * Mathf.Sin(t * Mathf.PI);
            toastObj.transform.localScale = Vector3.one * s;
            yield return null;
        }
        toastObj.transform.localScale = Vector3.one;

        // Hold visible for 2.2 seconds so text is easily readable
        yield return new WaitForSecondsRealtime(2.2f);

        if (toastCanvasGroup != null)
        {
            elapsed = 0f;
            const float fadeTime = 0.35f;
            while (elapsed < fadeTime)
            {
                elapsed += Time.unscaledDeltaTime;
                toastCanvasGroup.alpha = 1f - (elapsed / fadeTime);
                yield return null;
            }
            toastCanvasGroup.alpha = 0f;
        }

        toastObj.SetActive(false);
    }
}
