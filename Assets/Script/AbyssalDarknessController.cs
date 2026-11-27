using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abyssal Darkness & Visibility Challenge for Ocean Rescuers (World 1 — Deep Ocean ONLY).
/// Simulates the deep-sea bathypelagic darkness:
/// - Smooth vignette overlay dimming the abyss while keeping the puzzle grid readable.
/// - Localized bioluminescent visibility halos around the Source, Sink, and interactive nodes.
/// - Dynamic illumination integration with Sonar Scan: an acoustic pulse temporarily clears
///   the darkness, illuminating the abyssal puzzle grid.
/// - Lightweight procedural textures optimized for Android mobile GPUs (zero multi-pass shader overhead).
/// - Fully editable Inspector parameters for intensity, radius, color, and pulse speed.
/// - Strictly isolated to World 1 (Deep Ocean).
/// </summary>
[DisallowMultipleComponent]
public class AbyssalDarknessController : MonoBehaviour
{
    private static AbyssalDarknessController _instance;
    public static AbyssalDarknessController Instance => _instance;

    [Header("Darkness Configuration")]
    [Tooltip("Overall darkness intensity (0 = clear water, 1 = abyssal midnight)")]
    [Range(0f, 1f)]
    public float darknessIntensity = 0.55f;

    [Tooltip("Radius of the primary clear visibility window around the puzzle center")]
    public float visibilityRadius = 4.2f;

    [Tooltip("Deep abyssal vignette color")]
    public Color darknessColor = new Color(0.01f, 0.03f, 0.09f, 0.65f);

    [Tooltip("Bioluminescent halo glow color")]
    public Color bioluminescentGlowColor = new Color(0.12f, 0.85f, 1.0f, 0.28f);

    [Header("Animation & Transitions")]
    [Tooltip("Speed of the ambient abyssal pulsation")]
    public float pulseSpeed = 1.2f;

    [Tooltip("Speed of illumination transitions (e.g. Sonar flares)")]
    public float transitionSpeed = 2.5f;

    [Header("Child References")]
    public SpriteRenderer vignetteRenderer;
    public Transform haloContainer;

    // Runtime state
    private float _currentDarknessAlpha = 0.55f;
    private float _targetDarknessAlpha = 0.55f;
    private Coroutine _sonarFlareCoroutine;
    private readonly List<Transform> _spotlights = new List<Transform>();

    private void Awake()
    {
        _instance = this;
        SetupDarknessOverlay();
    }

    private void Start()
    {
        // Strict World Isolation: Deactivate immediately if not Deep Ocean
        if (SaveManager.GetSelectedWorld() != 1)
        {
            gameObject.SetActive(false);
            return;
        }

        _currentDarknessAlpha = darknessIntensity;
        _targetDarknessAlpha = darknessIntensity;

        ApplyFocalHalos();
        UpdateVisuals();
    }

    private void Update()
    {
        if (SaveManager.GetSelectedWorld() != 1) return;

        // Smooth transition towards target alpha
        if (!Mathf.Approximately(_currentDarknessAlpha, _targetDarknessAlpha))
        {
            _currentDarknessAlpha = Mathf.MoveTowards(_currentDarknessAlpha, _targetDarknessAlpha, Time.deltaTime * transitionSpeed);
            UpdateVisuals();
        }

        // Subtle ambient breathing pulsation
        if (vignetteRenderer != null && _sonarFlareCoroutine == null)
        {
            float pulse = 1f + 0.05f * Mathf.Sin(Time.time * pulseSpeed);
            Color c = darknessColor;
            c.a = _currentDarknessAlpha * pulse;
            vignetteRenderer.color = c;
        }
    }

    /// <summary>
    /// Configures the darkness vignette quad with a smooth radial falloff texture.
    /// </summary>
    public void SetupDarknessOverlay()
    {
        if (vignetteRenderer == null)
        {
            var vObj = transform.Find("VignetteOverlay");
            if (vObj == null)
            {
                var go = new GameObject("VignetteOverlay");
                go.transform.SetParent(transform, false);
                vignetteRenderer = go.AddComponent<SpriteRenderer>();
            }
            else
            {
                vignetteRenderer = vObj.GetComponent<SpriteRenderer>();
            }
        }

        // Position slightly in front of background/pipes, behind UI canvas (Z = -0.15f)
        vignetteRenderer.transform.localPosition = new Vector3(0f, 0f, -0.15f);
        vignetteRenderer.transform.localScale = new Vector3(22f, 13f, 1f); // Covers 1920x1080 orthographic viewport
        vignetteRenderer.sortingOrder = 6; // Between pipe layer (0) and HUD UI (100+)

        if (vignetteRenderer.sprite == null)
        {
            vignetteRenderer.sprite = GenerateVignetteSprite();
        }

        Color c = darknessColor;
        c.a = darknessIntensity;
        vignetteRenderer.color = c;

        // Halo container
        if (haloContainer == null)
        {
            var hObj = transform.Find("HaloContainer");
            if (hObj == null)
            {
                var go = new GameObject("HaloContainer");
                go.transform.SetParent(transform, false);
                haloContainer = go.transform;
            }
            else
            {
                haloContainer = hObj;
            }
        }
    }

    /// <summary>
    /// Spawns soft bioluminescent halos over the puzzle's Source and Sink cells.
    /// </summary>
    public void ApplyFocalHalos()
    {
        if (haloContainer == null) return;

        // Clear existing spotlights
        for (int i = haloContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(haloContainer.GetChild(i).gameObject);
        }
        _spotlights.Clear();

        var grid = FindObjectOfType<GridManager>();
        if (grid == null) return;

        // Halo over Source
        CreateSpotlight("SourceGlow", new Vector3(-3.5f, 0f, -0.05f), 2.2f, new Color(0.2f, 1f, 0.4f, 0.35f));

        // Halo over Sink
        CreateSpotlight("SinkGlow", new Vector3(3.5f, 0f, -0.05f), 2.2f, new Color(1f, 0.35f, 0.35f, 0.35f));
    }

    private void CreateSpotlight(string name, Vector3 localPos, float radius, Color glowColor)
    {
        var spotObj = new GameObject(name);
        spotObj.transform.SetParent(haloContainer, false);
        spotObj.transform.localPosition = localPos;
        spotObj.transform.localScale = Vector3.one * radius;

        var sr = spotObj.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateRadialGlowSprite();
        sr.color = glowColor;
        sr.sortingOrder = 5;

        _spotlights.Add(spotObj.transform);
    }

    /// <summary>
    /// Generates a smooth radial vignette texture (clear center, dark outer edges).
    /// </summary>
    private Sprite GenerateVignetteSprite()
    {
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxDist = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                // Smooth cubic falloff: transparent at center, opaque at edge
                float alpha = Mathf.SmoothStep(0.15f, 1.0f, dist);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Generates a smooth circular bioluminescent glow texture.
    /// </summary>
    private Sprite GenerateRadialGlowSprite()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxDist = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center) / maxDist;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = alpha * alpha; // Quadratic soft falloff
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// Temporarily illuminates the deep ocean when Sonar Scan is executed.
    /// </summary>
    public void TemporarilyIlluminate(float duration)
    {
        if (!gameObject.activeInHierarchy) return;
        if (_sonarFlareCoroutine != null) StopCoroutine(_sonarFlareCoroutine);
        _sonarFlareCoroutine = StartCoroutine(SonarIlluminationRoutine(duration));
    }

    private IEnumerator SonarIlluminationRoutine(float duration)
    {
        float startAlpha = _currentDarknessAlpha;
        float illuminatedAlpha = startAlpha * 0.20f; // 80% light flare

        // Quick flash to illuminated state
        float elapsed = 0f;
        float flareTime = 0.35f;
        while (elapsed < flareTime)
        {
            elapsed += Time.deltaTime;
            _currentDarknessAlpha = Mathf.Lerp(startAlpha, illuminatedAlpha, elapsed / flareTime);
            UpdateVisuals();
            yield return null;
        }

        // Hold illumination for pulse duration
        yield return new WaitForSeconds(Mathf.Max(0.5f, duration - 1.0f));

        // Smooth fade back to deep abyssal darkness
        elapsed = 0f;
        float fadeTime = 0.75f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            _currentDarknessAlpha = Mathf.Lerp(illuminatedAlpha, darknessIntensity, elapsed / fadeTime);
            UpdateVisuals();
            yield return null;
        }

        _currentDarknessAlpha = darknessIntensity;
        UpdateVisuals();
        _sonarFlareCoroutine = null;
    }

    public void SetIntensity(float intensity)
    {
        darknessIntensity = Mathf.Clamp01(intensity);
        _targetDarknessAlpha = darknessIntensity;
    }

    private void UpdateVisuals()
    {
        if (vignetteRenderer != null)
        {
            Color c = darknessColor;
            c.a = _currentDarknessAlpha;
            vignetteRenderer.color = c;
        }
    }
}
