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

    [Header("Star Sprites (Editable in Inspector)")]
    public Sprite goldStarSprite;
    public Sprite silverStarSprite;

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

    [Header("Creature Ability HUD")]
    public CreatureAbilityHUD creatureAbilityHUD;

    private bool _isPaused    = false;
    private bool _rewardGiven = false;
    private float _timerTick   = 0f;
    private bool _isTransitioning = false;
    private int _pendingAwardedCoins = 0;
    private int _pendingAwardedGems = 0;
    private int _prevCoinsBeforeReward = 0;
    private int _prevGemsBeforeReward = 0;

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

        // Initialize Creature Ability HUD
        if (creatureAbilityHUD == null)
        {
            creatureAbilityHUD = CreatureAbilityHUD.Instance ?? FindObjectOfType<CreatureAbilityHUD>();
            if (creatureAbilityHUD == null)
            {
                var hudCanvas = GameObject.Find("HUD_Canvas")?.transform;
                if (hudCanvas != null)
                {
                    creatureAbilityHUD = CreatureAbilityHUD.CreateWidget(hudCanvas);
                }
            }
        }

        // Fallback discovery for Pause Button and Pause Panel
        if (pauseButton == null)
        {
            var pb = GameObject.Find("PauseButton") 
                  ?? GameObject.Find("HUD_Canvas/SafeAreaContainer/TopBar/PauseButton")
                  ?? GameObject.Find("HUD_Canvas/SafeAreaContainer/PauseButton") 
                  ?? GameObject.Find("HUD_Canvas/PauseButton");
            if (pb != null) pauseButton = pb.GetComponent<Button>();
        }
        if (pausePanel == null)
        {
            var pp = GameObject.Find("PausePanel") 
                  ?? GameObject.Find("HUD_Canvas/SafeAreaContainer/PausePanel") 
                  ?? GameObject.Find("HUD_Canvas/PausePanel");
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
            MoveManager.Instance.OnMoveLimitChanged += UpdateMoveLimitDisplay;
            MoveManager.Instance.OnUndoStackChanged += UpdateUndoDisplay;
            MoveManager.Instance.OnMoveLimitReached += ShowFailPanel;
        }

        EnergyManager.OnEnergyChanged += UpdateCurrencyDisplay;

        // Level header displays (1-based for player view)
        int levelIndex = SaveManager.GetSelectedLevel();
        int worldIndex = SaveManager.GetSelectedWorld();
        int displayLevel = levelIndex + 1;
        int displayWorld = worldIndex + 1;

        LevelData ld = LevelData.LoadLevel(worldIndex, levelIndex);
        string diffTag = ld != null ? $" ({ld.difficulty})" : "";

        if (levelText     != null) levelText.text = $"Level {displayLevel}";
        if (levelNameText != null) levelNameText.text = $"World {displayWorld} · Level {displayLevel}{diffTag}";

        int maxMoves = ld != null && ld.maxMoves > 0 ? ld.maxMoves : GameplayBootstrapper.GetDefaultMaxMoves(levelIndex);
        MoveManager.Instance.ResetForLevel(levelIndex);
        MoveManager.Instance.SetMaxMoves(maxMoves);

        SetupEnergyBadge();
        SetupVisualPolish();

        // Initial displays
        UpdateMoveDisplay(MoveManager.Instance.MoveCount);
        UpdateUndoDisplay();
        UpdateCurrencyDisplay();

        // Switch background music to peaceful gameplay theme
        AudioManager.Instance?.PlayGameplayMusic();
    }

    private void SetupVisualPolish()
    {
        var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            if (canvas.GetComponent<UIBubbleEffect>() == null)
            {
                canvas.gameObject.AddComponent<UIBubbleEffect>();
            }
            UIButtonFeedback.AttachRecursively(canvas.transform);

            // Mobile Safe Area Protection for TopBar
            var topBar = canvas.transform.Find("TopBar");
            if (topBar != null && topBar.GetComponent<UISafeAreaHandler>() == null)
            {
                topBar.gameObject.AddComponent<UISafeAreaHandler>();
            }
        }

        EnsurePanelTransition(winPanel);
        EnsurePanelTransition(failPanel);
        EnsurePanelTransition(pausePanel);
        EnsurePanelTransition(rewardPanel);
    }

    private void EnsurePanelTransition(GameObject panel)
    {
        if (panel != null && panel.GetComponent<UIPanelTransition>() == null)
        {
            panel.AddComponent<UIPanelTransition>();
        }
    }

    private void SetupEnergyBadge()
    {
        var canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
        if (canvas == null) return;

        if (energyText == null)
        {
            var existing = canvas.transform.Find("SafeAreaContainer/TopBar/EnergyBadge")
                        ?? canvas.transform.Find("TopBar/EnergyBadge") 
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
            var ct = canvas.transform.Find("SafeAreaContainer/TopBar/CoinBadge/CoinText")
                  ?? canvas.transform.Find("TopBar/CoinBadge/CoinText") 
                  ?? canvas.transform.Find("SafeAreaContainer/CoinPanel/CoinText")
                  ?? canvas.transform.Find("CoinPanel/CoinText");
            if (ct != null) coinText = ct.GetComponent<TextMeshProUGUI>();
        }

        if (gemText == null)
        {
            var gt = canvas.transform.Find("SafeAreaContainer/TopBar/GemBadge/GemText")
                  ?? canvas.transform.Find("TopBar/GemBadge/GemText") 
                  ?? canvas.transform.Find("SafeAreaContainer/GemPanel/GemText")
                  ?? canvas.transform.Find("GemPanel/GemText");
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
            MoveManager.Instance.OnMoveLimitChanged -= UpdateMoveLimitDisplay;
            MoveManager.Instance.OnUndoStackChanged -= UpdateUndoDisplay;
            MoveManager.Instance.OnMoveLimitReached -= ShowFailPanel;
        }

        EnergyManager.OnEnergyChanged -= UpdateCurrencyDisplay;
    }

    private void UpdateMoveLimitDisplay(int limit)
    {
        UpdateMoveDisplay(MoveManager.Instance != null ? MoveManager.Instance.MoveCount : 0);
    }

    public void UpdateCurrencyDisplay()
    {
        int coins = SaveManager.GetCoins();
        int gems  = SaveManager.GetGems();
        if (coinText != null) coinText.text = coins.ToString();
        if (gemText  != null) gemText.text  = gems.ToString();
        if (energyText != null && EnergyManager.Instance != null)
        {
            // Display clean current/max energy format (e.g., 30/30, 19/30) matching reference design
            energyText.text = EnergyManager.Instance.GetFormattedEnergy();
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

        // Urgency color feedback
        if (remaining <= 3 && remaining > 0)
        {
            moveCountText.color = new Color(1f, 0.40f, 0.30f, 1f); // Urgent soft coral
        }
        else if (remaining <= 5)
        {
            moveCountText.color = new Color(1f, 0.85f, 0.30f, 1f); // Warning warm yellow
        }
        else
        {
            moveCountText.color = Color.white;
        }
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

        AudioManager.Instance?.PlayWin();

        var transition = winPanel.GetComponent<UIPanelTransition>() ?? winPanel.AddComponent<UIPanelTransition>();
        transition.PlayOpen();

        if (nextLevelBtn != null && nextLevelBtn.GetComponent<UIGlowPulse>() == null)
        {
            nextLevelBtn.gameObject.AddComponent<UIGlowPulse>();
        }

        if (undoButton != null) undoButton.interactable = false;

        int moves = MoveManager.Instance != null ? MoveManager.Instance.MoveCount : 0;

        // Calculate and display star rating
        int stars = CalculateStars(moves);
        UpdateStarDisplay(stars);
        transition.AnimateStars(starImages, stars, 0.22f);

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

            int prevCoins = SaveManager.GetCoins();
            int prevGems  = SaveManager.GetGems();

            // Persist rewards idempotently
            SaveManager.AddCoins(awardedCoins);
            SaveManager.SaveGems(prevGems + awardedGems);

            // Display previous numbers initially so flying tokens and counter roll-up are visible
            if (coinText != null) coinText.text = prevCoins.ToString();
            if (gemText  != null) gemText.text  = prevGems.ToString();

            // Celebratory particles & bubbles on Win Panel
            RewardPresentationController.Instance?.SpawnCelebratoryConfetti(winPanel.transform, 14);

            if (winMovesText != null)
            {
                winMovesText.text = $"Solved in {moves} moves! ({stars} ⭐)\n{rewardBreakdown.TrimEnd()}";
            }

            if (rewardPanel != null && awardedCoins > 0)
            {
                ShowRewardPopup(awardedCoins, awardedGems, prevCoins, prevGems);
            }
            else
            {
                Vector3 launchPos = winMovesText != null ? winMovesText.transform.position : winPanel.transform.position;
                TriggerFlyingRewards(prevCoins, prevCoins + awardedCoins, prevGems, prevGems + awardedGems, awardedCoins, awardedGems, launchPos);
            }

            Debug.Log($"[GameplayHUD] Level {levelIndex + 1} completed! Awarded: +{awardedCoins} coins, +{awardedGems} gems. Total Coins: {SaveManager.GetCoins()}, Gems: {SaveManager.GetGems()}.");
        }
        else
        {
            UpdateCurrencyDisplay();
        }
    }

    public void ShowRewardPopup(int coins, int gems, int prevCoins, int prevGems)
    {
        _pendingAwardedCoins = coins;
        _pendingAwardedGems = gems;
        _prevCoinsBeforeReward = prevCoins;
        _prevGemsBeforeReward = prevGems;

        if (rewardPanel == null) return;
        rewardPanel.SetActive(true);
        rewardPanel.transform.SetAsLastSibling();

        var transition = rewardPanel.GetComponent<UIPanelTransition>() ?? rewardPanel.AddComponent<UIPanelTransition>();
        transition.PlayOpen();

        if (rewardCollectBtn != null && rewardCollectBtn.GetComponent<UIGlowPulse>() == null)
        {
            rewardCollectBtn.gameObject.AddComponent<UIGlowPulse>();
        }

        if (rewardCoinsText != null)
        {
            rewardCoinsText.text = "+0 Coins";
            RewardPresentationController.Instance?.AnimateCounter(
                rewardCoinsText,
                0,
                coins,
                0.65f,
                "+",
                gems > 0 ? $" Coins  +{gems} Gems" : " Coins");
        }
    }

    public void OnRewardCollectClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        Vector3 collectPos = (rewardCollectBtn != null) 
            ? rewardCollectBtn.transform.position 
            : (rewardPanel != null ? rewardPanel.transform.position : Vector3.zero);

        if (rewardPanel != null)
        {
            var transition = rewardPanel.GetComponent<UIPanelTransition>();
            if (transition != null)
            {
                transition.PlayClose();
            }
            else
            {
                rewardPanel.SetActive(false);
            }
        }

        TriggerFlyingRewards(
            _prevCoinsBeforeReward,
            _prevCoinsBeforeReward + _pendingAwardedCoins,
            _prevGemsBeforeReward,
            _prevGemsBeforeReward + _pendingAwardedGems,
            _pendingAwardedCoins,
            _pendingAwardedGems,
            collectPos);

        Debug.Log("[GameplayHUD] Reward collected.");
    }

    private void TriggerFlyingRewards(int fromCoins, int toCoins, int fromGems, int toGems, int coinsAmount, int gemsAmount, Vector3 spawnWorldPos)
    {
        Canvas canvas = GetComponentInParent<Canvas>() ?? FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            UpdateCurrencyDisplay();
            return;
        }

        RectTransform coinBadgeTarget = coinText != null 
            ? (coinText.transform.parent as RectTransform ?? coinText.rectTransform) 
            : null;

        RectTransform gemBadgeTarget = gemText != null 
            ? (gemText.transform.parent as RectTransform ?? gemText.rectTransform) 
            : null;

        if (coinsAmount > 0 && coinBadgeTarget != null)
        {
            RewardPresentationController.Instance?.PlayFlyingCurrency(
                spawnWorldPos,
                coinBadgeTarget,
                coinsAmount,
                RewardPresentationController.CurrencyType.Coin,
                canvas,
                null,
                () => {
                    RewardPresentationController.Instance?.AnimateCounter(coinText, fromCoins, toCoins, 0.45f, "", "", UpdateCurrencyDisplay);
                }
            );
        }
        else
        {
            if (coinText != null) coinText.text = toCoins.ToString();
        }

        if (gemsAmount > 0 && gemBadgeTarget != null)
        {
            RewardPresentationController.Instance?.PlayFlyingCurrency(
                spawnWorldPos,
                gemBadgeTarget,
                gemsAmount,
                RewardPresentationController.CurrencyType.Gem,
                canvas,
                null,
                () => {
                    RewardPresentationController.Instance?.AnimateCounter(gemText, fromGems, toGems, 0.45f, "", "", UpdateCurrencyDisplay);
                }
            );
        }
        else
        {
            if (gemText != null) gemText.text = toGems.ToString();
        }
    }

    public void ShowFailPanel()
    {
        if (failPanel == null) return;
        failPanel.SetActive(true);

        AudioManager.Instance?.PlayFail();

        var transition = failPanel.GetComponent<UIPanelTransition>() ?? failPanel.AddComponent<UIPanelTransition>();
        transition.PlayOpen();

        if (retryBtn != null && retryBtn.GetComponent<UIGlowPulse>() == null)
        {
            retryBtn.gameObject.AddComponent<UIGlowPulse>();
        }

        if (failReasonText != null)
            failReasonText.text = "Move limit reached!\nTry again?";

        if (undoButton != null)
            undoButton.interactable = false;
    }

    private int CalculateStars(int moves)
    {
        int levelIndex = SaveManager.GetSelectedLevel();
        int worldIndex = SaveManager.GetSelectedWorld();
        LevelData ld = LevelData.LoadLevel(worldIndex, levelIndex);
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

    public Sprite GetGoldStarSprite()
    {
        if (goldStarSprite != null) return goldStarSprite;
        var s = Resources.Load<Sprite>("Sprites/icon_star_gold") ?? Resources.Load<Sprite>("icon_star_gold");
        if (s != null) return s;
        var all = Resources.LoadAll<Sprite>("Sprites/icon_star_gold");
        if (all != null && all.Length > 0)
        {
            Sprite best = all[0];
            foreach (var sp in all) { if (sp.rect.width > best.rect.width) best = sp; }
            return best;
        }
        return null;
    }

    public Sprite GetSilverStarSprite()
    {
        if (silverStarSprite != null) return silverStarSprite;
        var s = Resources.Load<Sprite>("Sprites/icon_star_silver") ?? Resources.Load<Sprite>("icon_star_silver");
        if (s != null) return s;
        var all = Resources.LoadAll<Sprite>("Sprites/icon_star_silver");
        if (all != null && all.Length > 0) return all[0];
        return null;
    }

    private void SetupStarDisplayIfMissing()
    {
        if (winPanel == null) return;
        if (starImages != null && starImages.Length > 0 && starImages[0] != null) return;

        var existingStars = winPanel.transform.Find("StarsContainer");
        if (existingStars == null)
        {
            var starsObj = new GameObject("StarsContainer", typeof(RectTransform));
            starsObj.transform.SetParent(winPanel.transform, false);
            var rt = starsObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 220);
            rt.sizeDelta = new Vector2(240, 80);

            Sprite goldStar = GetGoldStarSprite();
            Sprite silverStar = GetSilverStarSprite();

            starImages = new Image[3];
            Vector2[] positions = new Vector2[] { new Vector2(-75, -5), new Vector2(0, 10), new Vector2(75, -5) };
            float[] sizes = new float[] { 64f, 78f, 64f };

            for (int i = 0; i < 3; i++)
            {
                var sObj = new GameObject($"Star_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                sObj.transform.SetParent(starsObj.transform, false);
                var sRt = sObj.GetComponent<RectTransform>();
                sRt.anchoredPosition = positions[i];
                sRt.sizeDelta = new Vector2(sizes[i], sizes[i]);

                var sImg = sObj.GetComponent<Image>();
                sImg.sprite = silverStar != null ? silverStar : goldStar;
                sImg.preserveAspect = true;
                sImg.color = new Color(0.6f, 0.6f, 0.65f, 0.85f);
                starImages[i] = sImg;
            }
        }
        else
        {
            var imgs = existingStars.GetComponentsInChildren<Image>(true);
            if (imgs != null)
            {
                starImages = System.Array.FindAll(imgs, img => img.gameObject != existingStars.gameObject);
            }
        }
    }

    private void UpdateStarDisplay(int stars)
    {
        SetupStarDisplayIfMissing();
        if (starImages == null) return;

        Sprite goldStar = GetGoldStarSprite();
        Sprite silverStar = GetSilverStarSprite();

        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].preserveAspect = true;
            bool earned = i < stars;
            if (earned && goldStar != null)
            {
                starImages[i].sprite = goldStar;
                starImages[i].color = Color.white;
            }
            else if (silverStar != null)
            {
                starImages[i].sprite = silverStar;
                starImages[i].color = earned ? Color.white : new Color(0.6f, 0.6f, 0.65f, 0.85f);
            }
            else
            {
                starImages[i].color = earned ? new Color(1f, 0.88f, 0.2f, 1f) : new Color(0.35f, 0.35f, 0.4f, 0.8f);
            }
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
            var transition = pausePanel.GetComponent<UIPanelTransition>() ?? pausePanel.AddComponent<UIPanelTransition>();
            transition.PlayOpen();
        }
        Time.timeScale = 0f;
        AudioManager.Instance?.PlayButtonClick();
        Debug.Log($"[GameplayHUD] Pause opened. _isPaused={_isPaused}, Time.timeScale={Time.timeScale}");
    }

    public void OnResumeClicked()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        AudioManager.Instance?.PlayButtonClick();

        if (pausePanel != null)
        {
            var transition = pausePanel.GetComponent<UIPanelTransition>();
            if (transition != null)
            {
                transition.PlayClose();
            }
            else
            {
                pausePanel.SetActive(false);
            }
        }
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
        if (_isTransitioning) return;
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
                    failReasonText.text = $"Need 5 Energy to retry!\nCurrent: {cur}/30 ({timer}).\nReturn to menu to refill in Shop!";
                }
                return;
            }
            EnergyManager.Instance?.ConsumeEnergy(EnergyManager.ENERGY_COST_PER_LEVEL);
        }

        _isTransitioning = true;
        if (retryBtn != null) retryBtn.interactable = false;
        if (restartButton != null) restartButton.interactable = false;
        if (pauseRestartBtn != null) pauseRestartBtn.interactable = false;

        MoveManager.Instance?.ResetForLevel(levelIndex);
        SceneLoader.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void OnMenuClicked()
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        Time.timeScale = 1f;
        _isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (menuBtn != null) menuBtn.interactable = false;
        if (mainMenuBtnWin != null) mainMenuBtnWin.interactable = false;
        if (pauseMenuBtn != null) pauseMenuBtn.interactable = false;

        AudioManager.Instance?.PlayButtonClick();
        SceneLoader.LoadScene(SceneLoader.LEVEL_SELECT);
    }

    public void OnNextLevelClicked()
    {
        if (_isTransitioning) return;
        Time.timeScale = 1f;
        int worldIndex = SaveManager.GetSelectedWorld();
        int levelIndex = SaveManager.GetSelectedLevel();
        int nextLevel = levelIndex + 1;

        if (nextLevel < 12)
        {
            // Energy check
            if (EnergyManager.Instance != null && !EnergyManager.Instance.HasEnoughEnergy(EnergyManager.ENERGY_COST_PER_LEVEL))
            {
                // Auto-refill with 50 coins if available so player is not blocked
                if (SaveManager.SpendCoins(50))
                {
                    EnergyManager.Instance.AddEnergy(5);
                    UpdateCurrencyDisplay();
                }
                else
                {
                    if (winMovesText != null)
                    {
                        int cur = EnergyManager.Instance.CurrentEnergy;
                        string timer = EnergyManager.Instance.GetFormattedTimer();
                        winMovesText.text = $"Need 5 Energy to start Level {nextLevel + 1}!\nCurrent: {cur}/30 ({timer}).\nNeed 50 Coins to refill!";
                    }
                    return;
                }
            }

            _isTransitioning = true;
            if (nextLevelBtn != null) nextLevelBtn.interactable = false;

            EnergyManager.Instance?.ConsumeEnergy(EnergyManager.ENERGY_COST_PER_LEVEL);

            SaveManager.UnlockLevel(worldIndex, nextLevel);
            SaveManager.SetSelectedLevel(nextLevel);
            MoveManager.Instance?.ResetForLevel(nextLevel);
            SceneLoader.LoadScene("GameplayScene");
        }
        else
        {
            // Completed world's 12th level -> Unlock and transition to next world
            int nextWorld = worldIndex + 1;
            SaveManager.UnlockWorld(nextWorld);
            SaveManager.UnlockLevel(nextWorld, 0);
            SaveManager.SetSelectedWorld(nextWorld);
            SaveManager.SetSelectedLevel(0);

            _isTransitioning = true;
            if (nextLevelBtn != null) nextLevelBtn.interactable = false;

            SceneLoader.LoadScene(SceneLoader.LEVEL_SELECT);
        }
    }
}
