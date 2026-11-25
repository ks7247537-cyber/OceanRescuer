using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Reusable button animation, visual feedback, and audio feedback script.
/// Implements shrink on press, return on release, and quick bright flash on click.
/// Also provides clear disabled state visualization and selected state highlights.
/// Supports both mouse and mobile touch input.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Scale Animation")]
    [SerializeField] private float pressScale = 0.92f;
    [SerializeField] private float animDuration = 0.08f;

    [Header("Flash Animation")]
    [SerializeField] private bool enableFlash = true;
    [SerializeField] private float flashDuration = 0.12f;

    [Header("Audio")]
    [SerializeField] private bool playClickSound = true;

    [Header("States")]
    [SerializeField] private bool autoUpdateDisabledVisual = true;
    [SerializeField] private Color disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);

    private Vector3 _originalScale = Vector3.one;
    private Coroutine _scaleCoroutine;
    private Coroutine _flashCoroutine;
    private Image _targetImage;
    private Color _originalColor = Color.white;
    private Selectable _selectable;
    private bool _lastInteractable = true;

    private void Awake()
    {
        _originalScale = transform.localScale;
        if (_originalScale == Vector3.zero) _originalScale = Vector3.one;

        _selectable = GetComponent<Selectable>();
        _targetImage = GetComponent<Image>();
        if (_targetImage != null)
        {
            _originalColor = _targetImage.color;
        }

        if (_selectable != null)
        {
            _lastInteractable = _selectable.interactable;
            UpdateDisabledState(!_lastInteractable);
        }
    }

    private void Update()
    {
        if (autoUpdateDisabledVisual && _selectable != null)
        {
            if (_selectable.interactable != _lastInteractable)
            {
                _lastInteractable = _selectable.interactable;
                UpdateDisabledState(!_lastInteractable);
            }
        }
    }

    private void UpdateDisabledState(bool isDisabled)
    {
        if (_targetImage == null) return;
        _targetImage.color = isDisabled ? disabledColor : _originalColor;
    }

    private void OnDisable()
    {
        // Reset scale and color on disable
        transform.localScale = _originalScale;
        if (_targetImage != null)
        {
            _targetImage.color = _originalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_selectable != null && !_selectable.interactable) return;

        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(AnimateScale(_originalScale * pressScale));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(AnimateScale(_originalScale));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_selectable != null && !_selectable.interactable) return;

        if (playClickSound)
        {
            AudioManager.Instance?.PlayButtonClick();
        }

        if (enableFlash && gameObject.activeInHierarchy)
        {
            if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator AnimateScale(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }

    private IEnumerator FlashRoutine()
    {
        if (_targetImage == null) yield break;

        Color startColor = _targetImage.color;
        Color flashColor = Color.Lerp(startColor, Color.white, 0.75f);
        flashColor.a = startColor.a;

        _targetImage.color = flashColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            _targetImage.color = Color.Lerp(flashColor, startColor, t);
            yield return null;
        }

        _targetImage.color = startColor;
    }

    /// <summary>
    /// Utility to ensure every button under a parent transform has UIButtonFeedback.
    /// </summary>
    public static void AttachRecursively(Transform root)
    {
        if (root == null) return;
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (b.GetComponent<UIButtonFeedback>() == null)
            {
                b.gameObject.AddComponent<UIButtonFeedback>();
            }
        }
    }
}
