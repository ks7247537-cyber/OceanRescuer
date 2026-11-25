using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Modular Super Wrench System for Ocean Rescuers.
/// Automatically identifies and rotates the next critical unsolved pipe tile directly
/// into its LevelData solution rotation without consuming player move limit.
/// Features smooth wrench tool animation, particle feedback, win checking, and debounce protection.
/// </summary>
public class SuperWrenchManager : MonoBehaviour
{
    private static SuperWrenchManager _instance;
    public static SuperWrenchManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SuperWrenchManager>();
                if (_instance == null)
                {
                    var go = new GameObject("SuperWrenchManager");
                    _instance = go.AddComponent<SuperWrenchManager>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Super Wrench Configuration")]
    [Tooltip("Duration of the spinning wrench tool animation (in seconds).")]
    [SerializeField] private float wrenchAnimDuration = 0.40f;

    [Tooltip("Whether to trigger GameManager.CheckWinCondition() immediately after solving the tile.")]
    [SerializeField] private bool triggerWinCheckOnSolve = true;

    // Events
    public event Action<PipeTile, int> OnTileAutoSolved;

    private bool _isWrenchActive = false;
    private Sprite _wrenchSprite;
    private Sprite _starSprite;

    public bool IsWrenchActive => _isWrenchActive;

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

    private void OnDestroy()
    {
        StopAllCoroutines();
        _isWrenchActive = false;
        if (_instance == this) _instance = null;
    }

    private void LoadSprites()
    {
        _wrenchSprite = Resources.Load<Sprite>("Sprites/powerup_wrench")
                     ?? Resources.Load<Sprite>("Art/PowerUps/powerup_wrench")
                     ?? Resources.Load<Sprite>("powerup_wrench");

        _starSprite = Resources.Load<Sprite>("Sprites/icon_star_gold")
                   ?? Resources.Load<Sprite>("icon_star_gold");
    }

    /// <summary>
    /// Checks whether an unsolved tile is available to be auto-solved.
    /// </summary>
    public bool CanAutoSolve()
    {
        if (_isWrenchActive) return false;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return false;
        return PuzzleSolutionHelper.FindNextUnsolvedTile(out _);
    }

    /// <summary>
    /// Auto-solves the next unsolved tile along the primary flow path.
    /// Returns true if a tile was located and auto-solve started, false otherwise.
    /// </summary>
    public bool AutoSolveNextTile()
    {
        return AutoSolveNextTile(out _, out _);
    }

    /// <summary>
    /// Auto-solves the next unsolved tile along the primary flow path with output parameters.
    /// </summary>
    public bool AutoSolveNextTile(out PipeTile solvedTile, out int targetRotation)
    {
        solvedTile = null;
        targetRotation = 0;

        if (_isWrenchActive)
        {
            Debug.Log("[SuperWrenchManager] Super Wrench is already running.");
            return false;
        }

        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            Debug.Log("[SuperWrenchManager] Cannot use Super Wrench: Game is over.");
            return false;
        }

        if (!PuzzleSolutionHelper.FindNextUnsolvedTile(out var info))
        {
            Debug.Log("[SuperWrenchManager] No unsolved tiles found to solve.");
            return false;
        }

        solvedTile = info.tile;
        targetRotation = info.solutionRotation;

        // If Straight pipe, match 2-fold symmetry
        if (info.pipeType == PipeTile.PipeType.Straight)
        {
            targetRotation = targetRotation % 2;
        }
        else
        {
            targetRotation = targetRotation % 4;
        }

        StartCoroutine(WrenchRoutine(solvedTile, targetRotation, info.gridPos));
        return true;
    }

    private IEnumerator WrenchRoutine(PipeTile tile, int targetRotation, Vector2Int gridPos)
    {
        _isWrenchActive = true;

        Vector3 tilePos = tile.transform.position;

        // 1. Dismiss any active hint on this tile
        HintManager.Instance?.ClearActiveHint();

        // 2. Play wrench sound
        AudioManager.Instance?.PlayPipeRotate();

        // 3. Spawn animated wrench tool FX
        GameObject wrenchObj = new GameObject("Wrench_Tool_FX");
        wrenchObj.transform.position = new Vector3(tilePos.x, tilePos.y, -0.7f);
        wrenchObj.transform.localScale = Vector3.zero;

        var sr = wrenchObj.AddComponent<SpriteRenderer>();
        sr.sprite = _wrenchSprite;
        sr.color = Color.white;
        sr.sortingOrder = 18;

        float elapsed = 0f;
        while (elapsed < wrenchAnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / wrenchAnimDuration);

            // Pop in and shrink slightly
            float scale = (t < 0.4f) ? Mathf.Lerp(0f, 1.35f, t / 0.4f) : Mathf.Lerp(1.35f, 1.0f, (t - 0.4f) / 0.6f);
            wrenchObj.transform.localScale = Vector3.one * scale;

            // 360 spin
            wrenchObj.transform.rotation = Quaternion.Euler(0f, 0f, -t * 360f);

            yield return null;
        }

        // 4. Force tile into solved orientation (does not consume player move count)
        tile.ForceRotationStep(targetRotation);

        // 5. Spawn sparkle burst
        SpawnSparkles(tilePos);

        // 6. Clean up wrench tool FX
        Destroy(wrenchObj);

        _isWrenchActive = false;
        OnTileAutoSolved?.Invoke(tile, targetRotation);

        Debug.Log($"[SuperWrenchManager] Auto-solved tile at {gridPos} to rotation step {targetRotation}!");

        // 7. Re-evaluate win condition
        if (triggerWinCheckOnSolve && GameManager.Instance != null)
        {
            GameManager.Instance.CheckWinCondition();
        }
    }

    private void SpawnSparkles(Vector3 center)
    {
        const int count = 6;
        for (int i = 0; i < count; i++)
        {
            var sp = new GameObject($"WrenchSparkle_{i}");
            sp.transform.position = center + new Vector3(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f), -0.8f);
            var sr = sp.AddComponent<SpriteRenderer>();
            sr.sprite = _starSprite;
            sr.color = new Color(1f, 0.95f, 0.35f, 0.95f);
            sr.sortingOrder = 19;

            float angle = (i / (float)count) * Mathf.PI * 2f;
            Vector3 vel = new Vector3(Mathf.Cos(angle) * 1.5f, Mathf.Sin(angle) * 1.5f, 0f);
            StartCoroutine(AnimateSparkle(sp, vel, 0.45f));
        }
    }

    private IEnumerator AnimateSparkle(GameObject sp, Vector3 vel, float duration)
    {
        float elapsed = 0f;
        var sr = sp.GetComponent<SpriteRenderer>();
        Color startCol = sr != null ? sr.color : Color.white;

        while (elapsed < duration && sp != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            sp.transform.position += vel * Time.unscaledDeltaTime;
            sp.transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 0f, t);

            if (sr != null)
            {
                Color c = startCol;
                c.a = Mathf.Lerp(1f, 0f, t * t);
                sr.color = c;
            }

            yield return null;
        }

        if (sp != null) Destroy(sp);
    }
}
