using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ambient underwater UI bubble particle effect.
/// Creates subtle cartoon bubbles floating upward with gentle sine swaying.
/// Adds immediate living underwater atmosphere across HUD and menus without blocking raycasts.
/// </summary>
[DisallowMultipleComponent]
public class UIBubbleEffect : MonoBehaviour
{
    [Header("Bubble Settings")]
    [SerializeField] private int maxBubbles = 16;
    [SerializeField] private float minSpeed = 50f;
    [SerializeField] private float maxSpeed = 120f;
    [SerializeField] private float minSize = 18f;
    [SerializeField] private float maxSize = 42f;
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 0.65f;
    [SerializeField] private float swayFrequency = 2f;
    [SerializeField] private float swayAmplitude = 25f;

    [Header("Sprite")]
    [SerializeField] private Sprite[] bubbleSprites;

    private RectTransform _container;
    private List<BubbleData> _bubbles = new List<BubbleData>();
    private float _canvasWidth = 1920f;
    private float _canvasHeight = 1080f;

    private class BubbleData
    {
        public RectTransform rect;
        public Image image;
        public float speed;
        public float swayPhase;
        public float baseX;
        public float currentY;
        public float amplitude;
        public float targetAlpha;
    }

    private void Awake()
    {
        // Load bubble sprites from Resources/Sprites/bubbles
        if (bubbleSprites == null || bubbleSprites.Length == 0)
        {
            bubbleSprites = Resources.LoadAll<Sprite>("Sprites/bubbles");
            if (bubbleSprites == null || bubbleSprites.Length == 0)
            {
                bubbleSprites = Resources.LoadAll<Sprite>("bubbles");
            }
        }

        // If no bubble sprite is available, disable rather than spawning plain white squares
        if (bubbleSprites == null || bubbleSprites.Length == 0)
        {
            enabled = false;
            return;
        }

        SetupContainer();
    }

    private void Start()
    {
        UpdateCanvasBounds();
        InitializeBubbles();
    }

    private void SetupContainer()
    {
        // Create dedicated non-blocking container
        GameObject containerObj = new GameObject("UIBubbleOverlay", typeof(RectTransform));
        containerObj.transform.SetParent(transform, false);
        containerObj.transform.SetAsFirstSibling(); // Render behind interactive elements

        _container = containerObj.GetComponent<RectTransform>();
        _container.anchorMin = Vector2.zero;
        _container.anchorMax = Vector2.one;
        _container.offsetMin = Vector2.zero;
        _container.offsetMax = Vector2.zero;
    }

    private void UpdateCanvasBounds()
    {
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            _canvasWidth = rt.rect.width > 100 ? rt.rect.width : 1920f;
            _canvasHeight = rt.rect.height > 100 ? rt.rect.height : 1080f;
        }
    }

    private void InitializeBubbles()
    {
        for (int i = 0; i < maxBubbles; i++)
        {
            var b = CreateBubbleObject();
            ResetBubble(b, isFirstSpawn: true);
            _bubbles.Add(b);
        }
    }

    private BubbleData CreateBubbleObject()
    {
        GameObject obj = new GameObject("Bubble", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(_container, false);

        var rt = obj.GetComponent<RectTransform>();
        var img = obj.GetComponent<Image>();
        img.raycastTarget = false; // Never block clicks

        if (bubbleSprites != null && bubbleSprites.Length > 0)
        {
            img.sprite = bubbleSprites[Random.Range(0, bubbleSprites.Length)];
        }
        else
        {
            img.color = Color.clear;
        }

        return new BubbleData
        {
            rect = rt,
            image = img
        };
    }

    private void ResetBubble(BubbleData b, bool isFirstSpawn)
    {
        b.speed = Random.Range(minSpeed, maxSpeed);
        b.baseX = Random.Range(-_canvasWidth * 0.48f, _canvasWidth * 0.48f);
        b.swayPhase = Random.Range(0f, Mathf.PI * 2f);
        b.amplitude = Random.Range(swayAmplitude * 0.6f, swayAmplitude * 1.4f);
        b.targetAlpha = Random.Range(minAlpha, maxAlpha);

        float size = Random.Range(minSize, maxSize);
        b.rect.sizeDelta = new Vector2(size, size);

        float startY = isFirstSpawn 
            ? Random.Range(-_canvasHeight * 0.5f, _canvasHeight * 0.5f) 
            : -_canvasHeight * 0.55f;

        b.currentY = startY;

        // Choose random sprite
        if (bubbleSprites != null && bubbleSprites.Length > 0)
        {
            b.image.sprite = bubbleSprites[Random.Range(0, bubbleSprites.Length)];
        }

        // Tint soft cyan/white
        Color c = new Color(0.85f, 0.95f, 1f, b.targetAlpha);
        b.image.color = c;

        UpdateBubblePosition(b);
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        float time = Time.unscaledTime;
        float topBound = _canvasHeight * 0.55f;

        for (int i = 0; i < _bubbles.Count; i++)
        {
            var b = _bubbles[i];
            b.currentY += b.speed * dt;

            // Sway horizontally
            float sway = Mathf.Sin(time * swayFrequency + b.swayPhase) * b.amplitude;
            b.rect.anchoredPosition = new Vector2(b.baseX + sway, b.currentY);

            // Fade in from bottom, fade out near top
            float normY = Mathf.InverseLerp(-_canvasHeight * 0.5f, topBound, b.currentY);
            float alpha = Mathf.Sin(normY * Mathf.PI) * b.targetAlpha;
            Color c = b.image.color;
            c.a = Mathf.Clamp(alpha, 0f, b.targetAlpha);
            b.image.color = c;

            // Respawn when reaching top
            if (b.currentY > topBound)
            {
                ResetBubble(b, isFirstSpawn: false);
            }
        }
    }

    private void UpdateBubblePosition(BubbleData b)
    {
        float sway = Mathf.Sin(b.swayPhase) * b.amplitude;
        b.rect.anchoredPosition = new Vector2(b.baseX + sway, b.currentY);
    }
}
