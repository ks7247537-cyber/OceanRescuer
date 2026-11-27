using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lost City Environment Manager for Ocean Rescuers (World 2 ONLY).
/// Automatically configures the visual atmosphere when the player enters Lost City:
/// - Submerged ancient golden temples and ruins backdrop.
/// - Flanking ancient temple pillars and carved stone monument silhouettes.
/// - Ambient floating golden motes / ancient magical energy particles.
/// - Mysterious glowing runic glyphs breathing with ancient blue-green and gold luminescence.
/// - Strictly isolated to World 2 (Lost City). Automatically deactivates in World 0 and World 1.
/// </summary>
[DisallowMultipleComponent]
public class LostCityEnvironment : MonoBehaviour
{
    [Header("Atmosphere Settings")]
    public Color ancientGoldGlow = new Color(0.95f, 0.80f, 0.25f, 0.45f);
    public Color mysticCyanGlow  = new Color(0.15f, 0.90f, 0.85f, 0.35f);
    public int particleMoteCount = 14;

    private List<GameObject> _spawnedDecorations = new List<GameObject>();
    private bool _isInitialized = false;

    private void Start()
    {
        ApplyEnvironment();
    }

    private void OnDestroy()
    {
        var decor = GameObject.Find("LostCity_Atmosphere");
        if (decor != null) Destroy(decor);
    }

    public void ApplyEnvironment()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        int worldIndex = SaveManager.GetSelectedWorld();
        if (worldIndex != 2) return; // Strict isolation: World 2 ONLY

        // 1. Update Background Sprite in World space and UI Canvas space
        ApplyBackground();

        // 2. Spawn Ambient Underwater Temple Elements
        SpawnAtmosphereElements();
    }

    private void ApplyBackground()
    {
        var bgSprite = Resources.Load<Sprite>("Backgrounds/LostCityBackground");
        if (bgSprite == null) return;

        // In 2D World space (GameplayScene)
        var bgObj = GameObject.Find("Background");
        if (bgObj != null)
        {
            var sr = bgObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = bgSprite;
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
                uiBg.sprite = bgSprite;
                uiBg.color = Color.white;
            }
        }
    }

    private void SpawnAtmosphereElements()
    {
        var container = new GameObject("LostCity_Atmosphere");
        container.transform.position = Vector3.zero;

        // A. Flanking Ancient Temple Pillars (Z = -0.05f to sit behind grid, in front of background)
        CreateTemplePillar(container.transform, new Vector3(-5.2f, 0f, -0.05f), new Vector3(0.9f, 2.8f, 1f));
        CreateTemplePillar(container.transform, new Vector3(5.2f, 0f, -0.05f), new Vector3(0.9f, 2.8f, 1f));

        // B. Ancient Glowing Glyphs in Background
        CreateAncientGlyph(container.transform, new Vector3(-3.8f, 2.4f, -0.04f), ancientGoldGlow);
        CreateAncientGlyph(container.transform, new Vector3(3.8f, 2.4f, -0.04f), mysticCyanGlow);

        // C. Floating Ancient Golden Motes / Energy Dust
        CreateEnergyDust(container.transform);
    }

    private void CreateTemplePillar(Transform parent, Vector3 pos, Vector3 scale)
    {
        var pillarObj = new GameObject("AncientPillar");
        pillarObj.transform.SetParent(parent, false);
        pillarObj.transform.position = pos;
        pillarObj.transform.localScale = scale;

        var sr = pillarObj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Sprites/rocks_blue") ?? Resources.Load<Sprite>("Sprites/obstacle");
        sr.color = new Color(0.85f, 0.75f, 0.45f, 0.35f); // Ancient golden stone tint
        sr.sortingOrder = -2;
    }

    private void CreateAncientGlyph(Transform parent, Vector3 pos, Color glowColor)
    {
        var glyphObj = new GameObject("AncientGlyph");
        glyphObj.transform.SetParent(parent, false);
        glyphObj.transform.position = pos;
        glyphObj.transform.localScale = Vector3.one * 0.75f;

        var sr = glyphObj.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("Sprites/powerup_lightning") ?? Resources.Load<Sprite>("Sprites/bubbles");
        sr.color = glowColor;
        sr.sortingOrder = -1;

        var pulser = glyphObj.AddComponent<GlyphPulser>();
        pulser.baseColor = glowColor;
    }

    private void CreateEnergyDust(Transform parent)
    {
        var dustContainer = new GameObject("AncientEnergyDust");
        dustContainer.transform.SetParent(parent, false);

        for (int i = 0; i < particleMoteCount; i++)
        {
            var mote = new GameObject($"Mote_{i}");
            mote.transform.SetParent(dustContainer.transform, false);

            float rx = Random.Range(-5f, 5f);
            float ry = Random.Range(-3f, 3f);
            mote.transform.position = new Vector3(rx, ry, -0.06f);

            var sr = mote.AddComponent<SpriteRenderer>();
            sr.sprite = Resources.Load<Sprite>("Sprites/bubbles");
            Color c = (i % 2 == 0) ? ancientGoldGlow : mysticCyanGlow;
            c.a = Random.Range(0.2f, 0.6f);
            sr.color = c;
            sr.sortingOrder = -1;

            var drifter = mote.AddComponent<FloatingMote>();
            drifter.basePos = mote.transform.position;
            drifter.driftSpeed = Random.Range(0.4f, 0.9f);
            drifter.driftRadius = Random.Range(0.25f, 0.5f);
        }
    }
}

/// <summary>
/// Gentle pulsation for ancient temple glyphs.
/// </summary>
public class GlyphPulser : MonoBehaviour
{
    public Color baseColor;
    public float pulseSpeed = 1.6f;
    private SpriteRenderer _sr;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_sr == null) return;
        float pulse = 0.75f + 0.25f * Mathf.Sin(Time.time * pulseSpeed);
        Color c = baseColor;
        c.a = baseColor.a * pulse;
        _sr.color = c;
    }
}

/// <summary>
/// Floating ancient energy dust drift.
/// </summary>
public class FloatingMote : MonoBehaviour
{
    public Vector3 basePos;
    public float driftSpeed = 0.6f;
    public float driftRadius = 0.35f;
    private float _timeOffset;

    private void Start()
    {
        _timeOffset = Random.Range(0f, 10f);
    }

    private void Update()
    {
        float t = Time.time * driftSpeed + _timeOffset;
        float ox = Mathf.Sin(t) * driftRadius;
        float oy = Mathf.Cos(t * 1.3f) * driftRadius;
        transform.position = basePos + new Vector3(ox, oy, 0f);
    }
}
