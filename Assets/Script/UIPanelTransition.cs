using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Professional popup panel animation component.
/// Provides smooth bounce-in, fade-in, and star sequential popping effects.
/// Uses unscaled time so animations work seamlessly even during Pause (Time.timeScale = 0).
/// </summary>
[DisallowMultipleComponent]
public class UIPanelTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float openDuration = 0.28f;
    [SerializeField] private float closeDuration = 0.20f;
    [SerializeField] private bool animateOnEnable = true;
    [SerializeField] private bool useAlphaFade = true;

    [Header("Backdrop Dimmer (Optional)")]
    [SerializeField] private Image backdropDimmer;
    [SerializeField] private Color backdropTargetColor = new Color(0.02f, 0.08f, 0.18f, 0.78f);

    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Vector3 _originalScale = Vector3.one;
    private Coroutine _activeTransition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (_rectTransform != null)
        {
            _originalScale = _rectTransform.localScale;
            if (_originalScale == Vector3.zero) _originalScale = Vector3.one;
        }

        if (useAlphaFade)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void OnEnable()
    {
        if (animateOnEnable)
        {
            PlayOpen();
        }
    }

    /// <summary>
    /// Play juicy cartoon bounce-in animation.
    /// </summary>
    public void PlayOpen(Action onComplete = null)
    {
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);

        if (_activeTransition != null) StopCoroutine(_activeTransition);
        _activeTransition = StartCoroutine(OpenRoutine(onComplete));
    }

    /// <summary>
    /// Play smooth shrink-out animation, then deactivate.
    /// </summary>
    public void PlayClose(Action onComplete = null)
    {
        if (!gameObject.activeInHierarchy)
        {
            onComplete?.Invoke();
            return;
        }

        if (_activeTransition != null) StopCoroutine(_activeTransition);
        _activeTransition = StartCoroutine(CloseRoutine(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }));
    }

    private IEnumerator OpenRoutine(Action onComplete)
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

        // Initial state
        _rectTransform.localScale = Vector3.zero;
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        if (backdropDimmer != null)
        {
            Color c = backdropTargetColor;
            c.a = 0f;
            backdropDimmer.color = c;
        }

        float elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);

            // Ease Out Back for cartoon pop bounce
            float scaleProgress = EaseOutBack(t);
            _rectTransform.localScale = _originalScale * scaleProgress;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Clamp01(t * 1.5f);
            }

            if (backdropDimmer != null)
            {
                Color c = backdropTargetColor;
                c.a = Mathf.Lerp(0f, backdropTargetColor.a, t);
                backdropDimmer.color = c;
            }

            yield return null;
        }

        _rectTransform.localScale = _originalScale;
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        if (backdropDimmer != null) backdropDimmer.color = backdropTargetColor;

        _activeTransition = null;
        onComplete?.Invoke();
    }

    private IEnumerator CloseRoutine(Action onComplete)
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();

        float elapsed = 0f;
        Vector3 startScale = _rectTransform.localScale;
        float startAlpha = _canvasGroup != null ? _canvasGroup.alpha : 1f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);

            // Ease In Back
            float scaleProgress = EaseInBack(1f - t);
            _rectTransform.localScale = startScale * scaleProgress;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            }

            if (backdropDimmer != null)
            {
                Color c = backdropTargetColor;
                c.a = Mathf.Lerp(backdropTargetColor.a, 0f, t);
                backdropDimmer.color = c;
            }

            yield return null;
        }

        _rectTransform.localScale = Vector3.zero;
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        _activeTransition = null;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Sequential star pop animation with bounce for win screens.
    /// </summary>
    public void AnimateStars(Image[] stars, int earnedCount, float delayBeforeStart = 0.25f)
    {
        if (stars == null || stars.Length == 0) return;
        StartCoroutine(StarSequenceRoutine(stars, earnedCount, delayBeforeStart));
    }

    private IEnumerator StarSequenceRoutine(Image[] stars, int earnedCount, float delay)
    {
        // First hide all stars or scale to zero
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].transform.localScale = Vector3.zero;
            }
        }

        if (delay > 0)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] == null) continue;

            bool isEarned = i < earnedCount;
            // Pop star
            yield return StartCoroutine(PopSingleStar(stars[i], isEarned));
            yield return new WaitForSecondsRealtime(0.12f);
        }
    }

    private IEnumerator PopSingleStar(Image starImg, bool isEarned)
    {
        Transform t = starImg.transform;
        float duration = 0.22f;
        float elapsed = 0f;

        // Sound effect
        if (isEarned)
        {
            AudioManager.Instance?.PlayButtonClick();
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float scale = EaseOutBack(progress);
            t.localScale = Vector3.one * scale;
            yield return null;
        }

        t.localScale = Vector3.one;
    }

    // Mathematical easing helpers
    private static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
    }

    private static float EaseInBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return c3 * x * x * x - c1 * x * x;
    }
}
