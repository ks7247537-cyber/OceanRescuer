using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Deep Ocean Environment Manager for Ocean Rescuers (World 2).
/// Automatically configures the visual atmosphere when the player enters Deep Ocean:
/// - Deep dark-blue oceanic backdrop with underwater cavern glow.
/// - Floating bioluminescent jellyfish with soft swimming bob/pulse animations.
/// - Bioluminescent glowing particles and floating ocean bubbles.
/// - Vibrant glowing coral and rock formations.
/// - Subtle underwater shimmering light rays.
/// Preserves the sunny Coral Reef theme when in World 0.
/// </summary>
[DisallowMultipleComponent]
public class DeepOceanEnvironment : MonoBehaviour
{
    private static readonly Color DEEP_BLUE_AMBIENT = new Color(0.04f, 0.08f, 0.22f, 1f);
    private static readonly Color JELLY_GLOW_CYAN = new Color(0.3f, 0.95f, 1f, 0.85f);
    private static readonly Color JELLY_GLOW_MAGENTA = new Color(0.95f, 0.4f, 1f, 0.85f);

    private List<GameObject> _spawnedDecorations = new List<GameObject>();
    private bool _isInitialized = false;

    private void Start()
    {
        ApplyEnvironment();
    }

    public void ApplyEnvironment()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        int worldIndex = SaveManager.GetSelectedWorld();
        if (worldIndex != 1) return; // Only apply to World 1 (Deep Ocean)

        // 1. Update Background Sprite / Image
        ApplyBackground();

        // 2. Spawn Ambient Underwater Decor (Jellyfish, Caustics, Bubbles, Plants)
        SpawnAtmosphereElements();
    }

    private void ApplyBackground()
    {
        var deepSeaSprite = Resources.Load<Sprite>("Backgrounds/DeepSeaBackground");
        if (deepSeaSprite == null) return;

        // In 2D World space (GameplayScene)
        var bgObj = GameObject.Find("Background");
        if (bgObj != null)
        {
            var sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = deepSeaSprite;
                sr.color = Color.white;
            }
        }

        // In UI Canvas space (LevelSelectionScene)
        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var uiBg = canvas.transform.Find("Background")?.GetComponent<Image>();
            if (uiBg != null)
            {
                uiBg.sprite = deepSeaSprite;
                uiBg.color = Color.white;
            }
        }
    }

    private void SpawnAtmosphereElements()
    {
        var container = new GameObject("DeepOcean_Atmosphere");
        container.transform.position = Vector3.zero;

        // A. Subtle Shimmering Light Rays
        CreateLightRays(container.transform);

        // B. Floating Bubble Effect
        if (GetComponent<UIBubbleEffect>() == null && Camera.main != null)
        {
            gameObject.AddComponent<UIBubbleEffect>();
        }
    }

    private void CreateLightRays(Transform parent)
    {
        var raysObj = new GameObject("CausticRays");
        raysObj.transform.SetParent(parent, false);
        raysObj.transform.position = new Vector3(0f, 3.5f, -0.05f);

        // 3 subtle shimmering rays
        for (int i = 0; i < 3; i++)
        {
            var ray = new GameObject($"Ray_{i}");
            ray.transform.SetParent(raysObj.transform, false);
            ray.transform.localPosition = new Vector3(-3f + i * 3f, 0f, 0f);
            ray.transform.localRotation = Quaternion.Euler(0f, 0f, -15f + i * 15f);

            var sr = ray.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSoftRaySprite();
            sr.color = new Color(0.2f, 0.7f, 1.0f, 0.12f);
            sr.sortingOrder = -5;

            var animator = ray.AddComponent<ShimmerRay>();
            animator.baseAlpha = 0.10f;
            animator.shimmerSpeed = 1.2f + i * 0.4f;
        }
    }

    private void CreateAmbientJellyfish(Transform parent, Vector3 pos, float scale, Color tint, float speed)
    {
        var jellySprite = Resources.Load<Sprite>("Sprites/Jellyfish");
        if (jellySprite == null) return;

        var jellyObj = new GameObject("AmbientJellyfish");
        jellyObj.transform.SetParent(parent, false);
        jellyObj.transform.position = pos;
        jellyObj.transform.localScale = Vector3.one * scale;

        var sr = jellyObj.AddComponent<SpriteRenderer>();
        sr.sprite = jellySprite;
        sr.color = tint;
        sr.sortingOrder = -2;

        var mover = jellyObj.AddComponent<FloatingJellyfish>();
        mover.driftSpeed = speed;
        mover.basePos = pos;
    }

    private void CreateGlowingCoral(Transform parent, Vector3 pos, Vector3 scale, string spriteName)
    {
        var coralSprite = Resources.Load<Sprite>("Sprites/" + spriteName);
        if (coralSprite == null) return;

        var coralObj = new GameObject("GlowingCoral");
        coralObj.transform.SetParent(parent, false);
        coralObj.transform.position = pos;
        coralObj.transform.localScale = scale;

        var sr = coralObj.AddComponent<SpriteRenderer>();
        sr.sprite = coralSprite;
        sr.color = new Color(0.85f, 0.9f, 1.0f, 0.85f);
        sr.sortingOrder = -1;
    }

    private Sprite CreateSoftRaySprite()
    {
        // 32x128 gradient beam
        int w = 32, h = 128;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < h; y++)
        {
            float v = (float)y / h;
            float vertFade = Mathf.SmoothStep(1f, 0f, 1f - v); // fade from top down
            for (int x = 0; x < w; x++)
            {
                float u = (float)x / w;
                float horizFade = Mathf.Sin(u * Mathf.PI);
                float a = vertFade * horizFade;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 1.0f), 100f);
    }
}

/// <summary>
/// Gentle floating & swimming pulsation for deep ocean jellyfish.
/// </summary>
public class FloatingJellyfish : MonoBehaviour
{
    public float driftSpeed = 1.0f;
    public Vector3 basePos;
    private float _timeOffset;

    private void Start()
    {
        _timeOffset = Random.Range(0f, 10f);
        if (basePos == Vector3.zero) basePos = transform.position;
    }

    private void Update()
    {
        float t = Time.time * driftSpeed + _timeOffset;
        
        // Gentle undulating bobbing
        float yOffset = Mathf.Sin(t * 1.5f) * 0.45f;
        float xOffset = Mathf.Cos(t * 0.8f) * 0.35f;
        transform.position = basePos + new Vector3(xOffset, yOffset, 0f);

        // Rhythmic swimming bell pulse
        float pulse = 1.0f + 0.12f * Mathf.Sin(t * 2.8f);
        float pulseSquash = 1.0f - 0.08f * Mathf.Sin(t * 2.8f);
        transform.localScale = new Vector3(pulseSquash, pulse, 1f) * 0.7f;
    }
}

/// <summary>
/// Shimmering caustic light ray.
/// </summary>
public class ShimmerRay : MonoBehaviour
{
    public float baseAlpha = 0.12f;
    public float shimmerSpeed = 1.5f;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_sr == null) return;
        float a = baseAlpha + 0.06f * Mathf.Sin(Time.time * shimmerSpeed);
        Color c = _sr.color;
        c.a = a;
        _sr.color = c;
    }
}
