using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gameplay HUD — move counter, undo button, win/fail panels, currency display.
/// Portrait layout ke liye designed.
/// Upgraded: UnityEngine.UI.Text → TextMeshPro
/// v2: gemText added, one-time reward logic, UpdateCurrencyDisplay()
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [Header("Top Bar")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI moveCountText;

    [Header("Coin / Gem Display")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI gemText;        // ← NEW: gem count display
    public Image[] starImages;             // 3 star images for rating

    [Header("Undo Button")]
    public Button undoButton;
    public TextMeshProUGUI undoCountText;  // "Undo (3)"

    [Header("Restart Button")]
    public Button restartButton;

    [Header("Pause Button")]
    public Button pauseButton;
    public GameObject pausePanel;

    [Header("Win Panel")]
    public GameObject winPanel;
    public TextMeshProUGUI winMovesText;   // "Solved in 12 moves!"
    public Button nextLevelBtn;
    public Button replayBtnWin;
    public Button mainMenuBtnWin;

    [Header("Fail Panel")]
    public GameObject failPanel;
    public TextMeshProUGUI failReasonText;
    public Button retryBtn;
    public Button menuBtn;

    [Header("Level Info")]
    public TextMeshProUGUI levelNameText;

    // ── Private state ─────────────────────────────────────────────────────────
    private bool _isPaused    = false;
    private bool _rewardGiven = false;    // ← guard: reward is given only once per session

    // ── Reward amounts ────────────────────────────────────────────────────────
    private const int REWARD_COINS = 500;
    private const int REWARD_GEMS  = 10;

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        // Hide panels at start
        if (winPanel   != null) winPanel.SetActive(false);
        if (failPanel  != null) failPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Button listeners
        if (undoButton    != null) undoButton.onClick.AddListener(OnUndoClicked);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (retryBtn      != null) retryBtn.onClick.AddListener(OnRestartClicked);
        if (replayBtnWin  != null) replayBtnWin.onClick.AddListener(OnRestartClicked);
        if (menuBtn       != null) menuBtn.onClick.AddListener(OnMenuClicked);
        if (mainMenuBtnWin!= null) mainMenuBtnWin.onClick.AddListener(OnMenuClicked);
        if (pauseButton   != null) pauseButton.onClick.AddListener(OnPauseClicked);
        if (nextLevelBtn  != null) nextLevelBtn.onClick.AddListener(OnNextLevelClicked);

        // Subscribe to events
        if (MoveManager.Instance != null)
        {
            MoveManager.Instance.OnMoveCountChanged += UpdateMoveDisplay;
            MoveManager.Instance.OnUndoStackChanged += UpdateUndoDisplay;
            MoveManager.Instance.OnMoveLimitReached += ShowFailPanel;
        }

        // Initial display
        UpdateMoveDisplay(0);
        UpdateUndoDisplay();
        UpdateCurrencyDisplay();

        int levelNum = PlayerPrefs.GetInt("SelectedLevel", 1);
        int worldNum = PlayerPrefs.GetInt("SelectedWorld", 1);
        if (levelText     != null) levelText.text = $"Level {levelNum}";
        if (levelNameText != null) levelNameText.text = $"World {worldNum} · Level {levelNum}";
    }

    private void OnDestroy()
    {
        if (MoveManager.Instance != null)
        {
            MoveManager.Instance.OnMoveCountChanged -= UpdateMoveDisplay;
            MoveManager.Instance.OnUndoStackChanged -= UpdateUndoDisplay;
            MoveManager.Instance.OnMoveLimitReached -= ShowFailPanel;
        }
    }

    // ── Currency display ──────────────────────────────────────────────────────

    public void UpdateCurrencyDisplay()
    {
        int coins = PlayerPrefs.GetInt("Coins", 0);
        int gems  = PlayerPrefs.GetInt("Gems",  0);
        if (coinText != null) coinText.text = coins.ToString();
        if (gemText  != null) gemText.text  = gems.ToString();
    }

    // ── Move / Undo display ───────────────────────────────────────────────────

    private void UpdateMoveDisplay(int count)
    {
        if (moveCountText == null) return;
        int max = MoveManager.Instance != null ? MoveManager.Instance.maxMoves : 0;
        moveCountText.text = max > 0 ? $"Moves: {count}/{max}" : $"Moves: {count}";
    }

    private void UpdateUndoDisplay()
    {
        if (MoveManager.Instance == null) return;
        bool canUndo = MoveManager.Instance.CanUndo;
        if (undoButton  != null) undoButton.interactable = canUndo;
        if (undoCountText != null)
            undoCountText.text = $"Undo ({MoveManager.Instance.UndosLeft})";
    }

    // ── Win / Fail ────────────────────────────────────────────────────────────

    public void ShowWinPanel()
    {
        if (winPanel == null) return;
        winPanel.SetActive(true);

        int moves = MoveManager.Instance != null ? MoveManager.Instance.MoveCount : 0;
        if (winMovesText != null)
            winMovesText.text = $"Solved in {moves} moves! 🎉";

        // Calculate star rating
        int stars = CalculateStars(moves);
        UpdateStarDisplay(stars);

        // Save progress
        int world    = PlayerPrefs.GetInt("SelectedWorld", 1);
        int level    = PlayerPrefs.GetInt("SelectedLevel", 1);
        int existing = PlayerPrefs.GetInt($"Stars_W{world}_L{level}", 0);
        if (stars > existing) PlayerPrefs.SetInt($"Stars_W{world}_L{level}", stars);

        // Unlock next level
        PlayerPrefs.SetInt($"Unlocked_W{world}_L{level + 1}", 1);

        // ── One-time reward ────────────────────────────────────────────────
        if (!_rewardGiven)
        {
            _rewardGiven = true;

            // Add coins
            int currentCoins = PlayerPrefs.GetInt("Coins", 0);
            PlayerPrefs.SetInt("Coins", currentCoins + REWARD_COINS);

            // Add gems
            int currentGems = PlayerPrefs.GetInt("Gems", 0);
            PlayerPrefs.SetInt("Gems", currentGems + REWARD_GEMS);

            PlayerPrefs.Save();
            UpdateCurrencyDisplay();

            Debug.Log($"[GameplayHUD] Reward given: +{REWARD_COINS} coins, +{REWARD_GEMS} gems");
        }
        else
        {
            PlayerPrefs.Save();
        }
    }

    public void ShowFailPanel()
    {
        if (failPanel == null) return;
        failPanel.SetActive(true);
        if (failReasonText != null)
            failReasonText.text = "Move limit reached!\nTry again?";
    }

    private int CalculateStars(int moves)
    {
        if (moves <= 10) return 3;
        if (moves <= 20) return 2;
        return 1;
    }

    private void UpdateStarDisplay(int stars)
    {
        if (starImages == null) return;
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] != null)
                starImages[i].color = i < stars ? Color.yellow : new Color(0.4f, 0.4f, 0.4f);
        }
    }

    // ── Pause ─────────────────────────────────────────────────────────────────

    public void OnPauseClicked()
    {
        _isPaused = !_isPaused;
        if (pausePanel != null) pausePanel.SetActive(_isPaused);
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void OnResumeClicked()
    {
        _isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    public void OnUndoClicked() => MoveManager.Instance?.Undo();

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnMenuClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
    }

    public void OnNextLevelClicked()
    {
        Time.timeScale = 1f;
        int world = PlayerPrefs.GetInt("SelectedWorld", 1);
        int level = PlayerPrefs.GetInt("SelectedLevel", 1);
        PlayerPrefs.SetInt("SelectedLevel", level + 1);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
    }
}
