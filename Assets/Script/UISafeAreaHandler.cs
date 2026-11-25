using UnityEngine;

/// <summary>
/// Professional, robust Safe Area handler for Ocean Rescuers.
/// Automatically adapts UI elements to mobile notches, camera punch-holes,
/// home indicator bars, and rounded screen corners on Android and iOS.
/// 
/// Can be attached to:
/// 1. A dedicated 'SafeAreaContainer' parent under a Canvas.
/// 2. Directly to edge-sensitive UI components (TopBar, BackButton, CoinPanel, etc.)
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class UISafeAreaHandler : MonoBehaviour
{
    [Header("Safe Area Edges")]
    [Tooltip("Apply padding to the left edge (e.g. left camera cutout in landscape).")]
    [SerializeField] private bool applyLeft = true;

    [Tooltip("Apply padding to the right edge (e.g. right camera cutout in landscape).")]
    [SerializeField] private bool applyRight = true;

    [Tooltip("Apply padding to the top edge (e.g. status bar / notch).")]
    [SerializeField] private bool applyTop = true;

    [Tooltip("Apply padding to the bottom edge (e.g. navigation bar / home indicator).")]
    [SerializeField] private bool applyBottom = true;

    [Header("Editor Simulation (Preview mobile cutouts in Editor)")]
    [SerializeField] private bool simulateInEditor = false;
    [SerializeField] private float simulatedLeftCutout = 80f;
    [SerializeField] private float simulatedRightCutout = 80f;

    private RectTransform _rectTransform;
    private Rect _lastSafeArea = Rect.zero;
    private Vector2Int _lastScreenSize = Vector2Int.zero;
    private ScreenOrientation _lastOrientation = ScreenOrientation.AutoRotation;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Start()
    {
        ApplySafeArea();
    }

    private void OnEnable()
    {
        ApplySafeArea();
    }

    private void Update()
    {
        // Detect safe area, screen dimension or orientation changes
        if (HasGeometryChanged())
        {
            ApplySafeArea();
        }
    }

    private bool HasGeometryChanged()
    {
        Rect currentSafeArea = GetEffectiveSafeArea();
        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
        ScreenOrientation currentOrientation = Screen.orientation;

        return currentSafeArea != _lastSafeArea ||
               currentScreenSize != _lastScreenSize ||
               currentOrientation != _lastOrientation;
    }

    private Rect GetEffectiveSafeArea()
    {
        Rect safeArea = Screen.safeArea;

#if UNITY_EDITOR
        if (simulateInEditor && !Application.isPlaying)
        {
            // Simulate typical landscape camera cutouts in Unity Editor
            float leftInset = applyLeft ? simulatedLeftCutout : 0f;
            float rightInset = applyRight ? simulatedRightCutout : 0f;
            safeArea = new Rect(
                leftInset, 
                0f, 
                Mathf.Max(100f, Screen.width - leftInset - rightInset), 
                Screen.height
            );
        }
#endif

        return safeArea;
    }

    /// <summary>
    /// Computes and applies safe area bounds to this RectTransform.
    /// Preserves internal UI layout while protecting edges from cutouts.
    /// </summary>
    public void ApplySafeArea()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform == null) return;

        Rect safeArea = GetEffectiveSafeArea();
        float screenW = Screen.width;
        float screenH = Screen.height;

        if (screenW <= 0 || screenH <= 0) return;

        _lastSafeArea = safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        _lastOrientation = Screen.orientation;

        // Check if this RectTransform is a full-stretch container
        bool isFullStretch = Mathf.Approximately(_rectTransform.anchorMin.x, 0f) &&
                             Mathf.Approximately(_rectTransform.anchorMin.y, 0f) &&
                             Mathf.Approximately(_rectTransform.anchorMax.x, 1f) &&
                             Mathf.Approximately(_rectTransform.anchorMax.y, 1f);

        if (isFullStretch)
        {
            // Standard Safe Area Container approach
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;

            if (applyLeft)   anchorMin.x = safeArea.xMin / screenW;
            if (applyBottom) anchorMin.y = safeArea.yMin / screenH;
            if (applyRight)  anchorMax.x = safeArea.xMax / screenW;
            if (applyTop)    anchorMax.y = safeArea.yMax / screenH;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            // Edge-anchored UI element (e.g. BackButton anchored to Top-Left, PauseButton anchored to Top-Right)
            // Calculate pixel insets from screen boundary
            float insetLeft   = applyLeft   ? safeArea.xMin : 0f;
            float insetRight  = applyRight  ? (screenW - safeArea.xMax) : 0f;
            float insetBottom = applyBottom ? safeArea.yMin : 0f;
            float insetTop    = applyTop    ? (screenH - safeArea.yMax) : 0f;

            // Apply insets proportionally as pixel offsets
            float xOffset = (insetLeft > 0f) ? insetLeft : (insetRight > 0f ? -insetRight : 0f);
            float yOffset = (insetBottom > 0f) ? insetBottom : (insetTop > 0f ? -insetTop : 0f);

            // Shift element away from the cutout zone if in proximity to edge
            if (_rectTransform.anchorMin.x < 0.2f && insetLeft > 0f)
            {
                var curPos = _rectTransform.anchoredPosition;
                _rectTransform.anchoredPosition = new Vector2(insetLeft, curPos.y);
            }
            else if (_rectTransform.anchorMax.x > 0.8f && insetRight > 0f)
            {
                var curPos = _rectTransform.anchoredPosition;
                _rectTransform.anchoredPosition = new Vector2(-insetRight, curPos.y);
            }
        }
    }
}
