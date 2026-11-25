using UnityEngine;

/// <summary>
/// Responsive Gameplay Camera for Ocean Rescuers.
/// Ensures the entire 9x5 pipe grid, source pipe, destination pipe,
/// and interactive mechanisms remain 100% visible across all landscape mobile screen ratios:
/// - 16:9 (Standard Android reference)
/// - 18:9 (2:1 modern Android)
/// - 19.5:9 / 20:9 (Tall modern phones with cutouts)
/// - 16:10 (Android tablets)
/// - 4:3 (Landscape tablets and foldables)
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class ResponsiveGameplayCamera : MonoBehaviour
{
    [Header("Aspect Settings")]
    [Tooltip("Target reference aspect ratio (16:9 = ~1.7778)")]
    [SerializeField] private float referenceAspect = 16f / 9f;

    [Tooltip("Target reference orthographic size calibrated at 16:9 (covers 9x5 grid)")]
    [SerializeField] private float referenceOrthoSize = 3.2f;

    [Header("Grid Fit Safeguards")]
    [Tooltip("Minimum horizontal half-width in world units needed to fit 9-column grid")]
    [SerializeField] private float minHorizontalHalfWidth = 5.68889f;

    private Camera _cam;
    private float _lastAspect = 0f;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        AdjustCamera();
    }

    private void Start()
    {
        AdjustCamera();
    }

    private void OnEnable()
    {
        AdjustCamera();
    }

    private void Update()
    {
        AdjustCamera();
    }

    public void AdjustCamera()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam == null || !_cam.orthographic) return;

        float currentAspect = _cam.aspect;
        if (currentAspect <= 0f)
        {
            if (Screen.height > 0)
                currentAspect = (float)Screen.width / Screen.height;
            else
                currentAspect = referenceAspect;
        }

        if (Mathf.Abs(currentAspect - _lastAspect) < 0.001f) return;
        _lastAspect = currentAspect;

        if (currentAspect < referenceAspect)
        {
            // Screen is narrower/taller than 16:9 (e.g. 16:10, 4:3 tablets)
            // Expand orthographic size to guarantee complete 9-column horizontal grid visibility
            float requiredOrthoSize = referenceOrthoSize * (referenceAspect / currentAspect);
            _cam.orthographicSize = Mathf.Max(referenceOrthoSize, requiredOrthoSize);
        }
        else
        {
            // Screen is standard or wider (16:9, 18:9, 19.5:9, 20:9)
            // Keep reference ortho size so vertical grid framing and TopBar layout are preserved
            _cam.orthographicSize = referenceOrthoSize;
        }
    }
}
