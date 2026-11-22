using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gameplay HUD — move counter, undo button, win/fail panels, currency display.
/// Supports both Landscape and Portrait layouts.
/// Handles star calculation, level unlocking via SaveManager, and scene navigation.
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [Header("Top Bar")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI moveCountText;

    [Header("Coin / Gem / Energy Display")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI gemText;
    public TextMeshProUGUI energyText;
    public Image[] starImages;

    [Header("Undo Button")]
    public Button undoButton;
    public TextMeshProUGUI undoCountText;

    [Header("Restart Button")]
    public Button restartButton;

    [Header("Pause Button")]
    public Button pauseButton;
    public GameObject pausePanel;
    public Button resumeButton;
    public Button pauseRestartBtn;
    public Button pauseMenuBtn;
    public Button pauseSettingsBtn;

    [Header("Win Panel")]
    public GameObject winPanel;
    public TextMeshProUGUI winMovesText;
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

    [Header("Special Reward Popup")]
    public GameObject rewardPanel;
    public TextMeshProUGUI rewardCoinsText;
    public Button rewardCollectBtn;

    private bool _isPaused    = false;
    private bool _rewardGiven = false;
    private float _timerTick   = 0f;

    // Reward amounts
    private const int FIRST_TIME_COINS = 200;
    private const int FIRST_TIME_GEMS  = 5;
    private const int BONUS_3STAR_COINS = 100;
    private const int BONUS_3STAR_GEMS  = 5;
    private const int REPLAY_COINS      = 25;
    private const int WORLD_CLEAR_COINS = 1000;
    private const int WORLD_CLEAR_GEMS  = 50;

    private void Start()
    {
        // Hide panels at start
        if (winPanel   != null) winPanel.SetActive(false);
        if (failPanel  != null) failPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (rewardPanel!= null) rewardPanel.SetActive(false);

        if (rewardCollectBtn == null && rewardPanel != null)
        {
            rewardCollectBtn = rewardPanel.transform.Find("RewardCard/CollectButton")?.GetComponent<Button>()
                            ?? rewardPanel.GetComponentInChildren<Button>();
        }

        if (rewardCollectBtn != null)
        {
            rewardCollectBtn.onClick.RemoveAllListeners();
            rewardCollectBtn.onClick.AddListener(OnRewardCollectClicked);
        }

        // Fallback discovery for Pause Button and Pause Panel
        if (pauseButton == null)
        {
            var pb = GameObject.Find("PauseButton") ?? GameObject.Find("HUD_Canvas/PauseButton");
            if (pb != null) pauseButton = pb.GetComponent<Button>();
        }
        if (pausePanel == null)
        {
            var pp = GameObject.Find("PausePanel") ?? GameObject.Find("HUD_Canvas/PausePanel");
            if (pp != null) pausePanel = pp;
        }

        // Button listeners (clear any previous to avoid duplicates)
        if (undoButton    != null) { undoButton.onClick.RemoveAllListeners(); undoButton.onClick.AddListener(OnUndoClicked); }
        if (restartButton != null) { restartButton.onClick.RemoveAllListeners(); restartButton.onClick.AddListener(OnRestartClicked); }
        if (retryBtn      != null) { retryBtn.onClick.RemoveAllListeners(); retryBtn.onClick.AddListener(OnRestartClicked); }
        if (replayBtnWin  != null) { replayBtnWin.onClick.RemoveAllListeners(); replayBtnWin.onClick.AddListener(OnRestartClicked); }
        if (menuBtn       != null) { menuBtn.onClick.RemoveAllListeners(); menuBtn.onClick.AddListener(OnMenuClicked); }
        if (mainMenuBtnWin!= null) { mainMenuBtnWin.onClick.RemoveAllListeners(); mainMenuBtnWin.onClick.AddListener(OnMenuClicked); }
        if (pauseButton   != null) { pauseButton.onClick.RemoveAllListeners(); pauseButton.onClick.AddListener(OnPauseClicked); }
        if (nextLevelBtn  != null) { nextLevelBtn.onClick.RemoveAllListeners(); nextLevelBtn.onClick.AddListener(OnNextLevelClicked); }

        // Wire Pause Panel internal buttons
        if (pausePanel != null)
        {
            if (resumeButton == null)
                resumeButton = pausePanel.transform.Find("ResumeButton")?.GetComponent<Button>();
            if (pauseRestartBtn == null)
                pauseRestartBtn = pausePanel.transform.Find("RestartButton")?.GetComponent<Button>();
            if (pauseMenuBtn == null)
                pauseMenuBtn = pausePanel.transform.Find("MenuButton")?.GetComponent<Button>();
            if (pauseSettingsBtn == null)
                pauseSettingsBtn = pausePanel.transform.Find("SettingsButton")?.GetComponent<Button>();

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveAllListeners();
                resumeButton.onClick.AddListener(OnResumeClicked);
            }
            if (pauseRestartBtn != null)
            {
                pauseRestartBtn.onClick.RemoveAllListeners();
                pauseRestartBtn.onClick.AddListener(OnRestartClicked);
            }
            if (pauseMenuBtn != null)
            {
                pauseMenuBtn.onClick.RemoveAllListeners();
                pauseMenuBtn.onClick.AddListener(OnMenuClicked);
            }
            if (pauseSettingsBtn != null)
            {
                pauseSettingsBtn.onClick.RemoveAllListeners();
                pauseSettingsBtn.onClick.AddListener(OnPauseSettingsClicked);
            }
        }

        // Subscribe to MoveManager events
        if (MoveManager.Instance != null)
        {
            MoveManager.Instance.OnMoveCountChanged += UpdateMoveDisplay;
            MoveManager.Instance.OnUndoStackChanged += UpdateUndoDisplay;
            MoveManager.Instance.OnMoveLimitReached += ShowFailPanel;
        }

        EnergyManager.OnEnergyChanged += UpdateCurrencyDisplay;

        // Level header displays (1-based for player view)
        int levelIndex = SaveManager.GetSelectedLevel();
        int worldIndex = SaveManager.GetSelectedWorld();
        int displayLevel = levelIndex + 1;
        int displayWorld = worldIndex + 1;

        LevelData ld = Resources.Load<LevelData>($"Levels/Level_{displayLevel:D2}");
        string diffTag = ld != null ? $" ({ld.difficulty})" : "";

        if (levelText     != null) levelText.text = $"Level {displayLevel}";
        if (levelNameText != null) levelNameText.text = $"World {displayWorld} · Level {displayLevel}{diffTag}";

        int maxMoves = ld != null && ld.maxMoves > 0 ? ld.maxMoves : GameplayBootstrapper.GetDefaultMaxMoves(levelIndex);
        MoveManager.Instance.ResetForLevel(levelIndex);
        MoveManager.Instance.SetMaxMoves(maxMoves);

        SetupEnergyBadge();

        // Initial displays
        UpdateMoveDisplay(MoveManager.Instance.MoveCount);
        UpdateUndoDisplay();
        UpdateCurrencyDisplay();
    }

    private void SetupEnergyBadge()
    {
        var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
        if (canvas == null) return;

        if (energyText == null)
        {
            var existing = canvas.transform.Find("TopBar/EnergyBadge") 
                        ?? canvas.transform.Find("EnergyBadge") 
                        ?? canvas.transform.Find("EnergyBadge_Gameplay");
            if (existing != null) energyText = existing.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (energyText != null)
        {
            energyText.enableAutoSizing = true;
            energyText.fontSizeMin = 18;
            energyText.fontSizeMax = 28;
            energyText.fontStyle = FontStyles.Bold;
            energyText.alignment = TextAlignmentOptions.Midline;
        }

        if (coinText == null)
        {
            var ct = canvas.transform.Find("TopBar/CoinBadge/CoinText") ?? canvas.transform.Find("CoinPanel/CoinText");
            if (ct != null) coinText = ct.GetComponent<TextMeshProUGUI>();
        }

        if (gemText == null)
        {
            var gt = canvas.transform.Find("TopBar/GemBadge/GemText") ?? canvas.transform.Find("GemPanel/GemText");
            if (gt != null) gemText = gt.GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        _timerTick += Time.unscaledDeltaTime;
        if (_timerTick >= 1f)
        {
            _timerTick = 0f;
            UpdateCurrencyDisplay();
        }
    }

    private void OnDestroy()
    {
        if (MoveManager.Instance != null)
        {
            MoveManager.Instance.OnMoveCountChanged -= UpdateMoveDisplay;
            MoveManager.Instance.OnUndoStackChanged -= UpdateUndoDisplay;
            MoveManager.Instance.OnMoveLimitReached -= ShowFailPanel;
        }

        EnergyManager.OnEnergyChanged -= UpdateCurrencyDisplay;
    }

    public void UpdateCurrencyDisplay()
    {
        int coins = SaveManager.GetCoins();
        int gems  = SaveManager.GetGems();
        if (coinText != null) coinText.text = coins.ToString();
        if (gemText  != null) gemText.text  = gems.ToString();
        if (energyText != null && EnergyManager.Instance != null)
        {
            string energyStr = EnergyManager.Instance.GetFormattedEnergy();
            if (EnergyManager.Instance.CurrentEnergy < EnergyManager.Instance.MaxEnergy)
            {
                int sec = EnergyManager.Instance.SecondsUntilNextEnergy;
                int m = sec / 60;
                int s = sec % 60;
                energyText.text = $"{energyStr} ({m:D2}:{s:D2})";
            }
            else
            {
                energyText.text = energyStr;
            }
        }
    }

    private void UpdateMoveDisplay(int count)
    {
        if (moveCountText == null) return;
        int levelIndex = SaveManager.GetSelectedLevel();
        int max = MoveManager.Instance != null && MoveManager.Instance.maxMoves > 0 
            ? MoveManager.Instance.maxMoves 
            : GameplayBootstrapper.GetDefaultMaxMoves(levelIndex);

        int remaining = Mathf.Max(0, max - count);
        moveCountText.text = $"Moves Left: {remaining}";
    }

    private void UpdateUndoDisplay()
    {
        if (MoveManager.Instance == null) return;
        bool canUndo = MoveManager.Instance.CanUndo;
        if (undoButton    != null) undoButton.interactable = canUndo;
        if (undoCountText != null)
            undoCountText.text = $"Undo ({MoveManager.Instance.UndosLeft})";
    }

    public void ShowWinPanel()
    {
        if (winPanel == null) return;
        winPanel.SetActive(true);

        if (undoButton != null) undoButton.interactable = false;

        int moves = MoveManager.Instance != null ? MoveManager.Instance.MoveCount : 0;

        // Calculate and display star rating
        int stars = CalculateStars(moves);
        UpdateStarDisplay(stars);

        // Save progress using SaveManager
        int worldIndex = SaveManager.GetSelectedWorld();
        int levelIndex = SaveManager.GetSelectedLevel();
        SaveManager.SaveLevelStars(worldIndex, levelIndex, stars);

        // Unlock next level
        SaveManager.UnlockLevel(worldIndex, levelIndex + 1);

        // Reward calculation
        if (!_rewardGiven)
        {
            _rewardGiven = true;

            bool isFirstTime = !SaveManager.HasClaimedLevelReward(worldIndex, levelIndex);
            bool had3StarReward = SaveManager.HasClaimed3StarReward(worldIndex, levelIndex);
            bool isCoralReefGrandFinish = (worldIndex == 0 && levelIndex == 11 && !SaveManager.HasClaimedWorldReward(0));

            int awardedCoins = 0;
            int awardedGems = 0;
            string rewardBreakdown = "";

            if (isFirstTime)
            {
                awardedCoins += FIRST_TIME_COINS;
                awardedGems += FIRST_TIME_GEMS;
                SaveManager.SetLevelRewardClaimed(worldIndex, levelIndex, true);
                rewardBreakdown += $"First Clear: +{FIRST_TIME_COINS} 🪙 +{FIRST_TIME_GEMS} 💎\n";
            }
            else
            {
                awardedCoins += REPLAY_COINS;
                rewardBreakdown += $"Replay: +{REPLAY_COINS} 🪙\n";
            }

            // 3-Star Bonus
            if (stars == 3 && !had3StarReward)
            {
                awardedCoins += BONUS_3STAR_COINS;
                awardedGems += BONUS_3STAR_GEMS;
                SaveManager.Set3StarRewardClaimed(worldIndex, levelIndex, true);
                rewardBreakdown += $"⭐⭐⭐ Bonus: +{BONUS_3STAR_COINS} 🪙 +{BONUS_3STAR_GEMS} 💎\n";
            }

            // Special 12-level Coral Reef completion reward
            if (isCoralReefGrandFinish)
            {
                awardedCoins += WORLD_CLEAR_COINS;
                awardedGems += WORLD_CLEAR_GEMS;
                SaveManager.SetWorldRewardClaimed(0, true);
                rewardBreakdown += $"🏆 Coral Reef Cleared! Grand Reward: +{WORLD_CLEAR_COINS} 🪙 +{WORLD_CLEAR_GEMS} 💎!\n";
            }

            SaveManager.AddCoins(awardedCoins);
            SaveManager.SaveGems(SaveManager.GetGems() + awardedGems);
            UpdateCurrencyDisplay();

            if (rewardPanel != null && awardedCoins > 0)
            {
                ShowRewardPopup(awardedCoins, awardedGems);
            }

            if (winMovesText != null)
            {
                winMovesText.text = $"Solved in {moves} moves! ({stars} ⭐)\n{rewardBreakdown.TrimEnd()}";
            }

            Debug.Log($"[GameplayHUD] Level {levelIndex + 1} completed! Awarded: +{awardedCoins} coins, +{awardedGems} gems. Total Coins: {SaveManager.GetCoins()}, Gems: {SaveManager.GetGems()}.");
        }
        else
        {
            UpdateCurrencyDisplay();
        }
    }

    public void ShowRewardPopup(int coins, int gems)
    {
        if (rewardPanel == null) return;
        rewardPanel.SetActive(true);
        rewardPanel.transform.SetAsLastSibling();
        if (rewardCoinsText != null)
        {
            rewardCoinsText.text = $"+{coins} Coins" + (gems > 0 ? $"  +{gems} Gems" : "");
        }
    }

    public void OnRewardCollectClicked()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
        AudioManager.Instance?.PlayButtonClick();
        Debug.Log("[GameplayHUD] Reward collected.");
    }

    public void ShowFailPanel()
    {
        if (failPanel == null) return;
        failPanel.SetActive(true);
        if (failReasonText != null)
            failReasonText.text = "Move limit reached!\nTry again?";

        if (undoButton != null)
            undoButton.interactable = false;
    }

    private int CalculateStars(int moves)
    {
        int levelIndex = SaveManager.GetSelectedLevel();
        LevelData ld = Resources.Load<LevelData>($"Levels/Level_{levelIndex + 1:D2}");
        if (ld != null && ld.targetMovesFor3Stars > 0)
        {
            if (moves <= ld.targetMovesFor3Stars) return 3;
            if (moves <= ld.maxMoves) return 2;
            return 1;
        }

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

    public void OnPauseClicked()
    {
        if ((winPanel != null && winPanel.activeSelf) || (failPanel != null && failPanel.activeSelf)) return;

        _isPaused = true;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
        }
        Time.timeScale = 0f;
        AudioManager.Instance?.PlayButtonClick();
        Debug.Log($"[GameplayHUD] Pause opened. _isPaused={_isPaused}, Time.timeScale={Time.timeScale}");
    }

    public void OnResumeClicked()
    {
        _isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        AudioManager.Instance?.PlayButtonClick();
        Debug.Log("[GameplayHUD] Game Resumed.");
    }

    public void OnPauseSettingsClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            var sett = canvas.transform.Find("SettingsPanel");
            if (sett != null)
            {
                sett.gameObject.SetActive(true);
                sett.SetAsLastSibling();
            }
        }
    }

    public void OnUndoClicked() => MoveManager.Instance?.Undo();

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        AudioManager.Instance?.PlayButtonClick();
        int levelIndex = SaveManager.GetSelectedLevel();

        // If retrying from Fail panel, check and consume energy
        if (failPanel != null && failPanel.activeSelf)
        {
            if (EnergyManager.Instance != null && !EnergyManager.Instance.HasEnoughEnergy(EnergyManager.ENERGY_COST_PER_LEVEL))
            {
                if (failReasonText != null)
                {
                    int cur = EnergyManager.Instance.CurrentEnergy;
                    string timer = EnergyManager.Instance.GetFormattedTimer();
                    failReasonText.text = $"⚡ Need 5 Energy to retry!\nCurrent: {cur}/30 ({timer}).\nReturn to menu to refill in Shop!";
                }
                return;
            }
            EnergyManager.Instance?.ConsumeEnergy(EnergyManager.ENERGY_COST_PER_LEVEL);
        }

        MoveManager.Instance?.ResetForLevel(levelIndex);
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnMenuClicked()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        AudioManager.Instance?.PlayButtonClick();
        SceneLoader.LoadScene(SceneLoader.LEVEL_SELECT);
    }

    public void OnNextLevelClicked()
    {
        Time.timeScale = 1f;
        int worldIndex = SaveManager.GetSelectedWorld();
        int levelIndex = SaveManager.GetSelectedLevel();
        int nextLevel = levelIndex + 1;

        if (nextLevel < 12)
        {
            // Energy check
            if (EnergyManager.Instance != null && !EnergyManager.Instance.HasEnoughEnergy(EnergyManager.ENERGY_COST_PER_LEVEL))
            {
                if (winMovesText != null)
                {
                    int cur = EnergyManager.Instance.CurrentEnergy;
                    string timer = EnergyManager.Instance.GetFormattedTimer();
                    winMovesText.text = $"⚡ Need 5 Energy to start Level {nextLevel + 1}!\nCurrent: {cur}/30 ({timer}).\nReturn to Menu to refill in Shop!";
                }
                return;
            }

            EnergyManager.Instance?.ConsumeEnergy(EnergyManager.ENERGY_COST_PER_LEVEL);

            SaveManager.UnlockLevel(worldIndex, nextLevel);
            SaveManager.SetSelectedLevel(nextLevel);
            MoveManager.Instance?.ResetForLevel(nextLevel);
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
        }
        else if (SaveManager.IsWorldUnlocked(worldIndex + 1))
        {
            SaveManager.SetSelectedWorld(worldIndex + 1);
            SaveManager.SetSelectedLevel(0);
            SceneLoader.LoadScene(SceneLoader.LEVEL_SELECT);
        }
        else
        {
            SceneLoader.LoadScene(SceneLoader.LEVEL_SELECT);
        }
    }
}
