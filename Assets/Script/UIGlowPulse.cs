using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Soft breathing/glow pulsation for important action buttons.
/// Highlights primary call-to-actions (Play, Next Level, Collect, current LevelButton)
/// without interfering with click events or button scaling.
/// </summary>
[DisallowMultipleComponent]
public class UIGlowPulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float minScale = 0.98f;
    [SerializeField] private float maxScale = 1.05f;
    [SerializeField] private float pulseSpeed = 3.2f;
    [SerializeField] private bool pulseAlpha = false;
    [SerializeField] private float minAlpha = 0.7f;
    [SerializeField] private float maxAlpha = 1.0f;

    private Vector3 _baseScale = Vector3.one;
    private Image _targetImage;
    private Color _baseColor = Color.white;
    private bool _isInitialized = false;
    private float _phaseOffset = 0f;

    private void Awake()
    {
        _baseScale = transform.localScale;
        if (_baseScale == Vector3.zero) _baseScale = Vector3.one;

        _targetImage = GetComponent<Image>();
        if (_targetImage != null)
        {
            _baseColor = _targetImage.color;
        }
        _phaseOffset = transform.GetSiblingIndex() * 0.45f;
        _isInitialized = true;
    }

    private void OnEnable()
    {
        if (!_isInitialized) Awake();
        transform.localScale = _baseScale;
    }

    private void OnDisable()
    {
        transform.localScale = _baseScale;
        if (_targetImage != null && pulseAlpha)
        {
            _targetImage.color = _baseColor;
        }
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed + _phaseOffset) + 1f) * 0.5f;

        // Scale pulse
        float s = Mathf.Lerp(minScale, maxScale, t);
        transform.localScale = _baseScale * s;

        // Alpha pulse (optional)
        if (pulseAlpha && _targetImage != null)
        {
            float a = Mathf.Lerp(minAlpha, maxAlpha, t);
            Color c = _baseColor;
            c.a = a;
            _targetImage.color = c;
        }
    }
}
