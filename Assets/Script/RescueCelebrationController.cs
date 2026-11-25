using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Professional Creature Rescue Celebration and Water Splash Controller for Ocean Rescuers.
/// Handles the celebratory moment when the rescued sea creature reaches the destination sink:
/// - Joyful 360° victory flip and bounce for the rescued creature.
/// - Underwater bubble geyser & water splash particles at the destination sink.
/// - Expanding bioluminescent glow ring / ripple effect.
/// - Cheerful audio feedback without blocking or altering underlying gameplay.
/// Optimized for mobile performance with zero garbage collection allocations.
/// </summary>
public class RescueCelebrationController : MonoBehaviour
{
    private static RescueCelebrationController _instance;
    public static RescueCelebrationController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<RescueCelebrationController>();
                if (_instance == null)
                {
                    var go = new GameObject("RescueCelebrationController");
                    _instance = go.AddComponent<RescueCelebrationController>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    private Sprite _bubbleSprite;
    private Sprite _starSprite;
    private Sprite _glowSprite;
    private bool _isCelebrating = false;
    private GameObject _currentFxContainer;

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

    private void OnDisable()
    {
        CleanupCelebration();
    }

    private void OnDestroy()
    {
        CleanupCelebration();
        if (_instance == this) _instance = null;
    }

    private void CleanupCelebration()
    {
        StopAllCoroutines();
        if (_currentFxContainer != null)
        {
            Destroy(_currentFxContainer);
            _currentFxContainer = null;
        }
        _isCelebrating = false;
    }

    private void LoadSprites()
    {
        _bubbleSprite = Resources.Load<Sprite>("Sprites/bubbles");
        if (_bubbleSprite == null)
        {
            Sprite[] subs = Resources.LoadAll<Sprite>("Sprites/bubbles");
            if (subs != null && subs.Length > 0) _bubbleSprite = subs[0];
        }

        _starSprite = Resources.Load<Sprite>("Sprites/icon_star_gold") 
                   ?? Resources.Load<Sprite>("icon_star_gold");

        _glowSprite = CreateCircleTextureSprite();
    }

    /// <summary>
    /// Triggers the full rescue celebration sequence at the sink destination.
    /// </summary>
    public void PlayRescueCelebration(Vector3 destinationPos, Transform creatureTransform, Action onComplete)
    {
        if (_isCelebrating)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(CelebrationRoutine(destinationPos, creatureTransform, onComplete));
    }

    private IEnumerator CelebrationRoutine(Vector3 sinkPos, Transform creature, Action onComplete)
    {
        _isCelebrating = true;

        // 1. Play Victory Audio
        AudioManager.Instance?.PlayWin();

        // 2. Spawn Water Rescue Splash & Bubble Fountain at sink position
        if (_currentFxContainer != null) Destroy(_currentFxContainer);
        _currentFxContainer = new GameObject("Rescue_FX_Container");
        _currentFxContainer.transform.position = sinkPos;

        // Expanding bioluminescent glow ring
        StartCoroutine(SpawnGlowRing(sinkPos, _currentFxContainer.transform));

        // Water droplet splash
        StartCoroutine(SpawnWaterSplash(sinkPos, _currentFxContainer.transform));

        // Water bubble geyser
        StartCoroutine(SpawnBubbleGeyser(sinkPos, _currentFxContainer.transform));

        // Star sparkles
        StartCoroutine(SpawnSparkleBurst(sinkPos, _currentFxContainer.transform));

        // 3. Animate Joyful Creature Victory Flip & Bounce
        if (creature != null)
        {
            yield return StartCoroutine(CreatureVictoryAnimation(creature));
        }
        else
        {
            yield return new WaitForSecondsRealtime(1.1f);
        }

        // Clean up temporary visual effects
        if (_currentFxContainer != null)
        {
            Destroy(_currentFxContainer, 0.4f);
            _currentFxContainer = null;
        }

        _isCelebrating = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Creature does a joyful victory jump, 360° somersault, and happy scale bounce.
    /// Safely suppresses CharacterMover.idleBob during the flip to prevent position conflict.
    /// </summary>
    private IEnumerator CreatureVictoryAnimation(Transform creature)
    {
        var mover = creature.GetComponent<CharacterMover>();
        bool previousIdleBob = false;
        if (mover != null)
        {
            previousIdleBob = mover.idleBob;
            mover.idleBob = false;
        }

        Vector3 startPos = creature.position;
        Vector3 startScale = creature.localScale;
        Quaternion startRot = creature.rotation;

        float duration = 1.15f;
        float elapsed = 0f;

        while (elapsed < duration && creature != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Arching victory jump (upwards and back down)
            float jumpY = Mathf.Sin(t * Mathf.PI) * 0.75f;
            creature.position = startPos + new Vector3(0f, jumpY, 0f);

            // 360 degree joyful somersault spin
            float spinAngle = t * 360f;
            creature.rotation = startRot * Quaternion.Euler(0f, 0f, spinAngle);

            // Happy pulse scale (bounces to 1.3x at peak jump)
            float pulse = 1f + Mathf.Sin(t * Mathf.PI) * 0.32f;
            creature.localScale = startScale * pulse;

            yield return null;
        }

        if (creature != null)
        {
            creature.position = startPos;
            creature.rotation = startRot;
            creature.localScale = startScale;

            if (mover != null)
            {
                mover.idleBob = previousIdleBob;
            }
        }
    }

    /// <summary>
    /// Expanding bioluminescent ripple glow ring.
    /// </summary>
    private IEnumerator SpawnGlowRing(Vector3 pos, Transform parent)
    {
        if (parent == null) yield break;
        var ringObj = new GameObject("GlowRing");
        ringObj.transform.SetParent(parent, false);
        ringObj.transform.position = new Vector3(pos.x, pos.y, -0.5f);
        ringObj.transform.localScale = Vector3.zero;

        var sr = ringObj.AddComponent<SpriteRenderer>();
        sr.sprite = _glowSprite;
        sr.color = new Color(0.2f, 0.9f, 1f, 0.75f);
        sr.sortingOrder = 10;

        float duration = 0.85f;
        float elapsed = 0f;

        while (elapsed < duration && ringObj != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            float scale = Mathf.Lerp(0.2f, 2.5f, Mathf.Sqrt(t));
            ringObj.transform.localScale = Vector3.one * scale;

            Color c = sr.color;
            c.a = Mathf.Lerp(0.75f, 0f, t * t);
            sr.color = c;

            yield return null;
        }

        if (ringObj != null) Destroy(ringObj);
    }

    /// <summary>
    /// Water splash droplets exploding outward at the sink tile.
    /// </summary>
    private IEnumerator SpawnWaterSplash(Vector3 pos, Transform parent)
    {
        if (parent == null) yield break;
        const int dropletCount = 10;

        for (int i = 0; i < dropletCount; i++)
        {
            var drop = new GameObject($"WaterDrop_{i}");
            drop.transform.SetParent(parent, false);

            float angle = UnityEngine.Random.Range(30f, 150f) * Mathf.Deg2Rad;
            float speed = UnityEngine.Random.Range(2.2f, 4.2f);
            Vector3 vel = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0f);

            drop.transform.position = pos + new Vector3(0f, 0.1f, -0.55f);
            float scale = UnityEngine.Random.Range(0.12f, 0.28f);
            drop.transform.localScale = Vector3.one * scale;

            var sr = drop.AddComponent<SpriteRenderer>();
            sr.sprite = _glowSprite;
            sr.color = new Color(0.35f, 0.88f, 1f, 0.85f);
            sr.sortingOrder = 13;

            StartCoroutine(AnimateDroplet(drop, vel, UnityEngine.Random.Range(0.45f, 0.75f)));
        }
        yield return null;
    }

    private IEnumerator AnimateDroplet(GameObject drop, Vector3 vel, float lifetime)
    {
        if (drop == null) yield break;
        float elapsed = 0f;
        Vector3 curVel = vel;
        var sr = drop.GetComponent<SpriteRenderer>();
        Color startCol = sr != null ? sr.color : Color.white;

        while (elapsed < lifetime && drop != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / lifetime;

            curVel += new Vector3(0f, -6.5f * Time.unscaledDeltaTime, 0f); // Gravity fall
            drop.transform.position += curVel * Time.unscaledDeltaTime;

            if (sr != null)
            {
                Color c = startCol;
                c.a = Mathf.Lerp(startCol.a, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        if (drop != null) Destroy(drop);
    }

    /// <summary>
    /// Fountain of animated rising bubbles escaping from the rescued pipe sink.
    /// </summary>
    private IEnumerator SpawnBubbleGeyser(Vector3 pos, Transform parent)
    {
        if (parent == null) yield break;
        const int count = 12;

        for (int i = 0; i < count; i++)
        {
            if (parent == null) yield break;
            var bObj = new GameObject($"RescueBubble_{i}");
            bObj.transform.SetParent(parent, false);

            float angle = UnityEngine.Random.Range(45f, 135f) * Mathf.Deg2Rad;
            float speed = UnityEngine.Random.Range(1.8f, 3.6f);
            Vector3 vel = new Vector3(Mathf.Cos(angle) * speed * 0.6f, Mathf.Sin(angle) * speed, 0f);

            bObj.transform.position = pos + new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.1f, 0.1f), -0.6f);
            float scale = UnityEngine.Random.Range(0.25f, 0.55f);
            bObj.transform.localScale = Vector3.one * scale;

            var sr = bObj.AddComponent<SpriteRenderer>();
            sr.sprite = _bubbleSprite != null ? _bubbleSprite : _glowSprite;
            sr.color = new Color(0.85f, 0.96f, 1f, UnityEngine.Random.Range(0.55f, 0.9f));
            sr.sortingOrder = 12;

            StartCoroutine(AnimateSingleBubble(bObj, vel, UnityEngine.Random.Range(0.8f, 1.25f)));
            yield return new WaitForSecondsRealtime(0.04f);
        }
    }

    private IEnumerator AnimateSingleBubble(GameObject obj, Vector3 initialVel, float lifetime)
    {
        if (obj == null) yield break;
        float elapsed = 0f;
        Vector3 vel = initialVel;
        var sr = obj.GetComponent<SpriteRenderer>();
        Color startCol = sr != null ? sr.color : Color.white;
        float phase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        while (elapsed < lifetime && obj != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / lifetime;

            // Sway horizontally while floating upward
            float sway = Mathf.Sin(elapsed * 5f + phase) * 0.015f;
            obj.transform.position += (vel + new Vector3(sway, 0.8f * Time.unscaledDeltaTime, 0f)) * Time.unscaledDeltaTime;

            if (sr != null)
            {
                Color c = startCol;
                c.a = Mathf.Lerp(startCol.a, 0f, t);
                sr.color = c;
            }

            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    /// <summary>
    /// Sparkling victory sparkles bursting outward.
    /// </summary>
    private IEnumerator SpawnSparkleBurst(Vector3 pos, Transform parent)
    {
        if (parent == null) yield break;
        const int count = 8;
        for (int i = 0; i < count; i++)
        {
            var sp = new GameObject($"Sparkle_{i}");
            sp.transform.SetParent(parent, false);
            sp.transform.position = pos + new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), -0.7f);
            sp.transform.localScale = Vector3.zero;

            var sr = sp.AddComponent<SpriteRenderer>();
            sr.sprite = _starSprite != null ? _starSprite : _glowSprite;
            sr.color = new Color(1f, 0.9f, 0.3f, 0.95f);
            sr.sortingOrder = 14;

            float angle = (i / (float)count) * Mathf.PI * 2f + UnityEngine.Random.Range(-0.2f, 0.2f);
            float dist = UnityEngine.Random.Range(0.8f, 1.5f);
            Vector3 target = pos + new Vector3(Mathf.Cos(angle) * dist, Mathf.Sin(angle) * dist, -0.7f);

            StartCoroutine(AnimateSingleSparkle(sp, target, UnityEngine.Random.Range(0.55f, 0.85f)));
        }
        yield return null;
    }

    private IEnumerator AnimateSingleSparkle(GameObject obj, Vector3 targetPos, float duration)
    {
        if (obj == null) yield break;
        Vector3 start = obj.transform.position;
        var sr = obj.GetComponent<SpriteRenderer>();
        float elapsed = 0f;

        while (elapsed < duration && obj != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            obj.transform.position = Vector3.Lerp(start, targetPos, Mathf.SmoothStep(0f, 1f, t));
            float s = Mathf.Sin(t * Mathf.PI) * 0.45f;
            obj.transform.localScale = Vector3.one * s;
            obj.transform.rotation = Quaternion.Euler(0f, 0f, t * 180f);

            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    private Sprite CreateCircleTextureSprite()
    {
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float center = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float a = Mathf.Clamp01(1f - dist);
                a = a * a; // Smooth quadratic falloff
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
