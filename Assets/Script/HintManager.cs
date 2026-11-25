using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Modular Hint System for Ocean Rescuers.
/// Identifies the next critical unsolved pipe tile using PuzzleSolutionHelper.
/// Displays an aquatic pulsating glow ring and indicator arrow over the target tile.
/// Does NOT rotate pipes automatically.
/// Automatically dismisses after a configurable duration or as soon as the player makes a move.
/// </summary>
public class HintManager : MonoBehaviour
{
    private static HintManager _instance;
    public static HintManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<HintManager>();
                if (_instance == null)
                {
                    var go = new GameObject("HintManager");
                    _instance = go.AddComponent<HintManager>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Hint Configuration")]
    [Tooltip("How long the visual hint highlight remains active before auto-despawning (in seconds).")]
    [SerializeField] private float hintDuration = 3.5f;

    [Tooltip("Primary glow color of the hint ring and indicator.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.90f, 0.25f, 0.95f);

    [Tooltip("Speed of the breathing pulse scale effect.")]
    [SerializeField] private float pulseSpeed = 4.5f;

    [Tooltip("Scale pulse amplitude.")]
    [SerializeField] private float pulseScaleAmount = 0.22f;

    [Tooltip("Whether to automatically clear the hint as soon as any move is made.")]
    [SerializeField] private bool autoDismissOnMove = true;

    // Events
    public event Action<PipeTile> OnHintShown;
    public event Action OnHintCleared;

    private GameObject _activeHintObject;
    private Coroutine _activeHintCoroutine;
    private PipeTile _hintedTile;
    private Sprite _glowSprite;
    private Sprite _starSprite;

    public bool IsHintActive => _activeHintObject != null;

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

    private void Start()
    {
        if (autoDismissOnMove && MoveManager.Instance != null)
        {
            MoveManager.Instance.OnMoveCountChanged += OnMoveCountChanged;
        }
    }

    private void OnDisable()
    {
        ClearActiveHint();
        if (MoveManager.Instance != null)
        {
            MoveManager.Instance.OnMoveCountChanged -= OnMoveCountChanged;
        }
    }

    private void OnDestroy()
    {
        ClearActiveHint();
        if (_instance == this) _instance = null;
    }

    private void LoadSprites()
    {
        _starSprite = Resources.Load<Sprite>("Sprites/icon_star_gold")
                   ?? Resources.Load<Sprite>("Art/UI/Icons/icon_star_gold")
                   ?? Resources.Load<Sprite>("icon_star_gold");

        _glowSprite = CreateCircleTextureSprite();
    }

    private void OnMoveCountChanged(int moveCount)
    {
        if (IsHintActive && autoDismissOnMove)
        {
            ClearActiveHint();
        }
    }

    /// <summary>
    /// Checks whether an unsolved tile is available for a hint.
    /// </summary>
    public bool HasAvailableHint()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return false;
        return PuzzleSolutionHelper.FindNextUnsolvedTile(out _);
    }

    /// <summary>
    /// Highlights the next unsolved tile along the primary flow path.
    /// Returns true if a tile was highlighted, false if no unsolved tile was found or game is over.
    /// </summary>
    public bool ShowHint()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            Debug.Log("[HintManager] Cannot show hint: Game is over.");
            return false;
        }

        if (!PuzzleSolutionHelper.FindNextUnsolvedTile(out var info))
        {
            Debug.Log("[HintManager] No unsolved tiles found. Level pipeline is already complete!");
            return false;
        }

        ClearActiveHint();

        _hintedTile = info.tile;
        Vector3 targetPos = info.tile.transform.position;

        _activeHintObject = CreateHintVisual(targetPos);
        _activeHintCoroutine = StartCoroutine(HintAnimationRoutine(_hintedTile, _activeHintObject, hintDuration));

        AudioManager.Instance?.PlayButtonClick();
        OnHintShown?.Invoke(_hintedTile);

        Debug.Log($"[HintManager] Hint displayed on tile at {info.gridPos} ({info.pipeType}). Target rotation: {info.solutionRotation}");
        return true;
    }

    /// <summary>
    /// Immediately dismisses the active visual hint highlight.
    /// </summary>
    public void ClearActiveHint()
    {
        if (_activeHintCoroutine != null)
        {
            StopCoroutine(_activeHintCoroutine);
            _activeHintCoroutine = null;
        }

        if (_activeHintObject != null)
        {
            Destroy(_activeHintObject);
            _activeHintObject = null;
        }

        _hintedTile = null;
        OnHintCleared?.Invoke();
    }

    private GameObject CreateHintVisual(Vector3 worldPos)
    {
        var root = new GameObject("Hint_Indicator");
        root.transform.position = new Vector3(worldPos.x, worldPos.y, -0.6f);

        // 1. Pulsing Outer Glow Ring
        var ringObj = new GameObject("GlowRing");
        ringObj.transform.SetParent(root.transform, false);
        var srRing = ringObj.AddComponent<SpriteRenderer>();
        srRing.sprite = _glowSprite;
        srRing.color = highlightColor;
        srRing.sortingOrder = 15;
        ringObj.transform.localScale = Vector3.one * 1.35f;

        // 2. Corner Sparkles
        for (int i = 0; i < 4; i++)
        {
            var sp = new GameObject($"Sparkle_{i}");
            sp.transform.SetParent(root.transform, false);
            var sr = sp.AddComponent<SpriteRenderer>();
            sr.sprite = _starSprite != null ? _starSprite : _glowSprite;
            sr.color = Color.white;
            sr.sortingOrder = 16;
            sp.transform.localScale = Vector3.one * 0.35f;

            float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
            sp.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.52f, Mathf.Sin(angle) * 0.52f, 0f);
        }

        return root;
    }

    private IEnumerator HintAnimationRoutine(PipeTile targetTile, GameObject hintObj, float duration)
    {
        float elapsed = 0f;
        Vector3 baseScale = hintObj.transform.localScale;

        var renderers = hintObj.GetComponentsInChildren<SpriteRenderer>();

        while (elapsed < duration && hintObj != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Breathing pulse effect
            float pulse = 1f + Mathf.Sin(elapsed * pulseSpeed) * pulseScaleAmount;
            hintObj.transform.localScale = baseScale * pulse;

            // Gentle rotation of sparkles
            hintObj.transform.rotation = Quaternion.Euler(0f, 0f, elapsed * 30f);

            // Fade out during last 0.5 seconds
            if (duration - elapsed < 0.5f)
            {
                float fadeT = (duration - elapsed) / 0.5f;
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] != null)
                    {
                        Color c = renderers[i].color;
                        c.a = Mathf.Lerp(0f, highlightColor.a, fadeT);
                        renderers[i].color = c;
                    }
                }
            }

            yield return null;
        }

        ClearActiveHint();
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
                // Ring halo shape: high near 0.7 - 0.9, transparent at center and outer edge
                float alpha = 0f;
                if (dist <= 1f)
                {
                    float ringProfile = Mathf.Sin(dist * Mathf.PI);
                    alpha = Mathf.Pow(ringProfile, 1.5f);
                }
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}
