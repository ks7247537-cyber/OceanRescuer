using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Poore game ka master controller.
/// Win condition check karta hai aur UI trigger karta hai.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton pattern
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public FlowManager    flowManager;
    public CharacterMover characterMover;  // Fish/Turtle character
    public GameplayHUD    hud;             // UI controller
    public GameObject     winPanel;        // Legacy — prefer hud.ShowWinPanel()
    public GameObject     levelUI;         // Normal game UI

    [Header("Win Settings")]
    [Tooltip("Win ke baad agli scene ka naam (optional)")]
    public string nextSceneName = "";

    [Header("Demo & Testing")]
    [Tooltip("Keep false for normal gameplay — player must solve puzzle by rotating pipes!")]
    public bool autoPlayDemo = false;

    private bool _hasWon = false;
    private bool _isFailed = false;

    /// <summary>
    /// Returns true if level has ended (either won or failed).
    /// Prevents further moves or rotations.
    /// </summary>
    public bool IsGameOver => _hasWon || _isFailed;
    public bool HasWon => _hasWon;
    public bool IsFailed => _isFailed;

    /// <summary>
    /// Resumes active play if the level was failed due to running out of moves (used by Extra Moves power-up).
    /// </summary>
    public void ResumeFromFail()
    {
        if (!_isFailed) return;
        _isFailed = false;

        UnlockAllTiles();

        if (hud != null && hud.failPanel != null)
        {
            hud.failPanel.SetActive(false);
        }
    }

    private void UnlockAllTiles()
    {
        int worldIndex = SaveManager.GetSelectedWorld();
        int levelIndex = SaveManager.GetSelectedLevel();
        LevelData ld = LevelData.LoadLevel(worldIndex, levelIndex);
        Vector2Int src = ld != null ? ld.sourceCell : new Vector2Int(0, 0);
        Vector2Int snk = ld != null ? ld.sinkCell : new Vector2Int(8, 0);

        var gridMgr = FindObjectOfType<GridManager>();
        if (gridMgr != null && gridMgr.Grid != null)
        {
            for (int x = 0; x < gridMgr.Cols; x++)
            {
                for (int y = 0; y < gridMgr.Rows; y++)
                {
                    var t = gridMgr.Grid[x, y];
                    if (t != null)
                    {
                        bool isEdge = (x == src.x && y == src.y) || (x == snk.x && y == snk.y);
                        t.isLocked = isEdge;
                    }
                }
            }
        }
    }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (flowManager == null)
            flowManager = FindObjectOfType<FlowManager>();
        if (hud == null)
            hud = FindObjectOfType<GameplayHUD>();
        if (characterMover == null)
            characterMover = FindObjectOfType<CharacterMover>();

        // Calculate initial water glow from source for starting pipes
        bool connected = flowManager != null && flowManager.RunFlow();

        if (connected && autoPlayDemo)
        {
            Invoke(nameof(TriggerDemo), 1.2f);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        // Editor test helper: Spacebar instantly tests flow
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _hasWon = false;
            _isFailed = false;
            CheckWinCondition();
        }
#endif
    }

    /// <summary>
    /// Triggers the full win sequence (water animation + clownfish swimming).
    /// </summary>
    public void TriggerDemo()
    {
        if (IsGameOver) return;
        CheckWinCondition();
    }

    /// <summary>
    /// Har pipe rotate hone ke baad call hoti hai.
    /// Evaluates both win condition and move-limit fail condition.
    /// </summary>
    public void CheckWinCondition()
    {
        if (IsGameOver) return;

        bool won = flowManager != null && flowManager.RunFlow();

        if (won)
        {
            _hasWon = true;
            OnWin();
        }
        else if (MoveManager.Instance != null && MoveManager.Instance.maxMoves > 0 && MoveManager.Instance.RemainingMoves <= 0)
        {
            _isFailed = true;
            OnFail();
        }
    }

    private void OnFail()
    {
        Debug.Log("❌ Level Failed: Move limit reached!");

        // Lock all tiles to prevent further rotations
        LockAllTiles();

        // Show Fail Panel via HUD
        if (hud != null)
        {
            hud.ShowFailPanel();
        }
    }

    private void OnWin()
    {
        Debug.Log("🎉 Win! Water path completed!");

        // Lock all tiles during victory sequence
        LockAllTiles();

        // 1. Play sequential water animation through all pipes
        if (flowManager != null)
        {
            flowManager.AnimateWaterFlow(() =>
            {
                // 2. Once water fills the pipeline, clownfish swims to the destination!
                if (characterMover != null)
                {
                    characterMover.OnDestinationReached += HandleDestinationReached;
                    characterMover.StartJourney();
                    Invoke(nameof(HandleDestinationReached), 8.0f); // Safety timeout
                }
                else
                {
                    Invoke(nameof(ShowVictoryDialog), 1.0f);
                }
            });
        }
        else if (characterMover != null)
        {
            characterMover.OnDestinationReached += HandleDestinationReached;
            characterMover.StartJourney();
            Invoke(nameof(HandleDestinationReached), 8.0f);
        }
        else
        {
            Invoke(nameof(ShowVictoryDialog), 1.0f);
        }
    }

    private void HandleDestinationReached()
    {
        CancelInvoke(nameof(HandleDestinationReached));
        if (characterMover != null)
            characterMover.OnDestinationReached -= HandleDestinationReached;

        Vector3 destinationPos = characterMover != null ? characterMover.transform.position : Vector3.zero;
        Transform creatureTransform = characterMover != null ? characterMover.transform : null;

        if (RescueCelebrationController.Instance != null)
        {
            RescueCelebrationController.Instance.PlayRescueCelebration(destinationPos, creatureTransform, ShowVictoryDialog);
        }
        else
        {
            ShowVictoryDialog();
        }
    }

    private void ShowVictoryDialog()
    {
        // HUD win panel displays stars, awards coins/gems, and unlocks next level
        if (hud != null)
        {
            hud.ShowWinPanel();
        }
        else
        {
            // Fallback unlock if hud is missing
            int w = SaveManager.GetSelectedWorld();
            int l = SaveManager.GetSelectedLevel();
            SaveManager.SaveLevelStars(w, l, 3);
            SaveManager.UnlockLevel(w, l + 1);

            if (winPanel != null)
                winPanel.SetActive(true);
        }

        if (levelUI != null) levelUI.SetActive(false);

        // Optional delayed scene transition if nextSceneName is explicitly set
        if (!string.IsNullOrEmpty(nextSceneName))
            Invoke(nameof(LoadNextScene), 2f);
    }

    private void LockAllTiles()
    {
        var tiles = FindObjectsOfType<PipeTile>();
        foreach (var t in tiles)
        {
            if (t != null) t.isLocked = true;
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
