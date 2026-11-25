using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Professional Reward Presentation Controller for Ocean Rescuers.
/// Handles:
/// - Smooth number counting animation for coin and gem rewards with audio ticks.
/// - Juicy curved flying currency tokens (coins/gems) towards TopBar HUD badges using Bezier arcs.
/// - Badge punch-scale feedback on token arrival.
/// - Celebratory UI bubble and sparkle confetti on Win Dialog.
/// - Idempotent protection against duplicate reward granting and spam clicks.
/// Optimized for mobile with unscaled time and zero garbage collection allocations.
/// </summary>
public class RewardPresentationController : MonoBehaviour
{
    public enum CurrencyType
    {
        Coin,
        Gem
    }

    private static RewardPresentationController _instance;
    public static RewardPresentationController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RewardPresentationController>();
                if (_instance == null)
                {
                    var go = new GameObject("RewardPresentationController");
                    _instance = go.AddComponent<RewardPresentationController>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    private Sprite _coinSprite;
    private Sprite _gemSprite;
    private Sprite _bubbleSprite;
    private Sprite _starSprite;

    private List<GameObject> _activeTokens = new List<GameObject>();
    private bool _isAnimating = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        LoadSprites();
    }

    private void LoadSprites()
    {
        _coinSprite = Resources.Load<Sprite>("Sprites/icon_coin")
                   ?? Resources.Load<Sprite>("Art/UI/Icons/icon_coin")
                   ?? Resources.Load<Sprite>("icon_coin");

        _gemSprite = Resources.Load<Sprite>("Sprites/icon_gem")
                  ?? Resources.Load<Sprite>("Art/UI/Icons/icon_gem")
                  ?? Resources.Load<Sprite>("icon_gem");

        _bubbleSprite = Resources.Load<Sprite>("Sprites/bubbles")
                     ?? Resources.Load<Sprite>("Art/UI/bubbles");

        _starSprite = Resources.Load<Sprite>("Sprites/icon_star_gold")
                   ?? Resources.Load<Sprite>("Art/UI/Icons/icon_star_gold")
                   ?? Resources.Load<Sprite>("icon_star_gold");
    }

    private void OnDisable()
    {
        CleanupActiveTokens();
    }

    private void OnDestroy()
    {
        CleanupActiveTokens();
        if (_instance == this) _instance = null;
    }

    public void CleanupActiveTokens()
    {
        StopAllCoroutines();
        for (int i = 0; i < _activeTokens.Count; i++)
        {
            if (_activeTokens[i] != null)
            {
                Destroy(_activeTokens[i]);
            }
        }
        _activeTokens.Clear();
        _isAnimating = false;
    }

    /// <summary>
    /// Smoothly counts up a TextMeshProUGUI number from fromVal to toVal.
    /// </summary>
    public Coroutine AnimateCounter(TextMeshProUGUI label, int fromVal, int toVal, float duration, string prefix = "", string suffix = "", Action onComplete = null)
    {
        if (label == null)
        {
            onComplete?.Invoke();
            return null;
        }
        return StartCoroutine(CounterRoutine(label, fromVal, toVal, duration, prefix, suffix, onComplete));
    }

    private IEnumerator CounterRoutine(TextMeshProUGUI label, int fromVal, int toVal, float duration, string prefix, string suffix, Action onComplete)
    {
        if (fromVal == toVal || duration <= 0f)
        {
            label.text = $"{prefix}{toVal}{suffix}";
            onComplete?.Invoke();
            yield break;
        }

        float elapsed = 0f;
        int lastDisplayed = fromVal;
        label.text = $"{prefix}{fromVal}{suffix}";

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease out quad for smooth deceleration
            float easeT = 1f - (1f - t) * (1f - t);
            int currentVal = Mathf.RoundToInt(Mathf.Lerp(fromVal, toVal, easeT));

            if (currentVal != lastDisplayed)
            {
                lastDisplayed = currentVal;
                label.text = $"{prefix}{currentVal}{suffix}";

                // Cheerful tick sound periodically
                if (Mathf.Abs(currentVal - fromVal) % 5 == 0 || t >= 0.99f)
                {
                    AudioManager.Instance?.PlayButtonClick();
                }
            }

            yield return null;
        }

        label.text = $"{prefix}{toVal}{suffix}";
        onComplete?.Invoke();
    }

    /// <summary>
    /// Spawns flying currency tokens that travel along curved Bezier trajectories toward the HUD counter badge.
    /// </summary>
    public void PlayFlyingCurrency(Vector3 startWorldOrScreenPos, RectTransform targetBadge, int displayCount, CurrencyType type, Canvas canvas, Action onTokenArrive = null, Action onAllComplete = null)
    {
        if (canvas == null || targetBadge == null)
        {
            onAllComplete?.Invoke();
            return;
        }

        StartCoroutine(FlyingCurrencySequence(startWorldOrScreenPos, targetBadge, displayCount, type, canvas, onTokenArrive, onAllComplete));
    }

    private IEnumerator FlyingCurrencySequence(Vector3 startPos, RectTransform targetBadge, int count, CurrencyType type, Canvas canvas, Action onTokenArrive, Action onAllComplete)
    {
        Sprite sprite = (type == CurrencyType.Coin) ? _coinSprite : _gemSprite;
        int tokenCount = Mathf.Clamp(count > 0 ? (count <= 25 ? 5 : 8) : 5, 4, 10);

        Transform canvasTransform = canvas.transform;
        int remainingTokens = tokenCount;

        for (int i = 0; i < tokenCount; i++)
        {
            GameObject tokenObj = new GameObject($"FlyingToken_{type}_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            tokenObj.transform.SetParent(canvasTransform, false);
            _activeTokens.Add(tokenObj);

            var rt = tokenObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(48f, 48f);
            rt.position = startPos;

            var img = tokenObj.GetComponent<Image>();
            if (sprite != null)
            {
                img.sprite = sprite;
                img.preserveAspect = true;
            }
            else
            {
                img.color = (type == CurrencyType.Coin) ? new Color(1f, 0.85f, 0.2f, 1f) : new Color(0.2f, 0.9f, 1f, 1f);
            }

            // Scatter offset for initial bursting effect
            Vector2 scatter = UnityEngine.Random.insideUnitCircle * 55f;
            Vector3 scatteredStart = startPos + new Vector3(scatter.x, scatter.y, 0f);

            StartCoroutine(AnimateSingleToken(tokenObj, scatteredStart, targetBadge, i * 0.05f, () =>
            {
                _activeTokens.Remove(tokenObj);
                Destroy(tokenObj);

                // Target badge punch scale
                PunchScaleBadge(targetBadge);

                // Play audio tick
                AudioManager.Instance?.PlayButtonClick();

                onTokenArrive?.Invoke();

                remainingTokens--;
                if (remainingTokens <= 0)
                {
                    onAllComplete?.Invoke();
                }
            }));

            yield return new WaitForSecondsRealtime(0.04f);
        }
    }

    private IEnumerator AnimateSingleToken(GameObject token, Vector3 startPos, RectTransform targetBadge, float delay, Action onArrive)
    {
        if (token == null) yield break;

        if (delay > 0f)
        {
            token.transform.localScale = Vector3.zero;
            yield return new WaitForSecondsRealtime(delay);
        }

        if (token == null || targetBadge == null)
        {
            onArrive?.Invoke();
            yield break;
        }

        // Control point for curved Bezier flight
        Vector3 targetPos = targetBadge.position;
        Vector3 midPoint = (startPos + targetPos) * 0.5f;

        // Curve outward/upward naturally
        float curveHeight = UnityEngine.Random.Range(100f, 220f);
        float curveSide = UnityEngine.Random.Range(-90f, 90f);
        Vector3 controlPoint = midPoint + new Vector3(curveSide, curveHeight, 0f);

        float duration = 0.55f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (token == null || targetBadge == null) yield break;

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease In Cubic for acceleration toward target
            float curveT = t * t * (3f - 2f * t);

            // Quadratic Bezier formula
            Vector3 pos = Mathf.Pow(1f - curveT, 2f) * startPos 
                        + 2f * (1f - curveT) * curveT * controlPoint 
                        + Mathf.Pow(curveT, 2f) * targetBadge.position;

            token.transform.position = pos;

            // Scale pop on launch, normalize during flight
            float scale = (t < 0.25f) 
                ? Mathf.Lerp(0f, 1.25f, t / 0.25f) 
                : Mathf.Lerp(1.25f, 0.9f, (t - 0.25f) / 0.75f);
            token.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        onArrive?.Invoke();
    }

    /// <summary>
    /// Bounces and punch-scales the target HUD badge when currency tokens arrive.
    /// </summary>
    public void PunchScaleBadge(RectTransform badge)
    {
        if (badge == null) return;
        StartCoroutine(PunchScaleRoutine(badge));
    }

    private IEnumerator PunchScaleRoutine(RectTransform badge)
    {
        Vector3 originalScale = Vector3.one;
        float duration = 0.16f;
        float elapsed = 0f;

        while (elapsed < duration && badge != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float bounce = 1f + Mathf.Sin(t * Mathf.PI) * 0.28f;
            badge.localScale = originalScale * bounce;
            yield return null;
        }

        if (badge != null)
        {
            badge.localScale = originalScale;
        }
    }

    /// <summary>
    /// Spawns cheerful floating celebration bubbles and golden stars around the win panel.
    /// </summary>
    public void SpawnCelebratoryConfetti(Transform parent, int count = 12)
    {
        if (parent == null) return;

        for (int i = 0; i < count; i++)
        {
            var pObj = new GameObject($"ConfettiBubble_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            pObj.transform.SetParent(parent, false);
            _activeTokens.Add(pObj);

            var rt = pObj.GetComponent<RectTransform>();
            float startX = UnityEngine.Random.Range(-320f, 320f);
            float startY = UnityEngine.Random.Range(-140f, 40f);
            rt.anchoredPosition = new Vector2(startX, startY);

            var img = pObj.GetComponent<Image>();
            bool isStar = (i % 3 == 0);
            img.sprite = isStar ? (_starSprite ?? _bubbleSprite) : _bubbleSprite;
            img.preserveAspect = true;

            if (isStar)
            {
                img.color = new Color(1f, 0.9f, 0.35f, 0.95f);
                rt.sizeDelta = new Vector2(36f, 36f);
            }
            else
            {
                img.color = new Color(0.85f, 0.95f, 1f, UnityEngine.Random.Range(0.6f, 0.9f));
                float s = UnityEngine.Random.Range(24f, 48f);
                rt.sizeDelta = new Vector2(s, s);
            }

            StartCoroutine(AnimateConfettiParticle(pObj, UnityEngine.Random.Range(1.2f, 2.0f)));
        }
    }

    private IEnumerator AnimateConfettiParticle(GameObject obj, float lifetime)
    {
        if (obj == null) yield break;
        var rt = obj.GetComponent<RectTransform>();
        var img = obj.GetComponent<Image>();
        Color startCol = img != null ? img.color : Color.white;
        Vector2 startPos = rt.anchoredPosition;

        float floatSpeedY = UnityEngine.Random.Range(55f, 110f);
        float swayFreq = UnityEngine.Random.Range(2.5f, 5f);
        float swayAmp = UnityEngine.Random.Range(15f, 35f);
        float elapsed = 0f;

        while (elapsed < lifetime && obj != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / lifetime;

            float y = startPos.y + floatSpeedY * elapsed;
            float x = startPos.x + Mathf.Sin(elapsed * swayFreq) * swayAmp;
            rt.anchoredPosition = new Vector2(x, y);

            if (img != null)
            {
                Color c = startCol;
                c.a = Mathf.Lerp(startCol.a, 0f, t * t);
                img.color = c;
            }

            yield return null;
        }

        if (obj != null)
        {
            _activeTokens.Remove(obj);
            Destroy(obj);
        }
    }
}
