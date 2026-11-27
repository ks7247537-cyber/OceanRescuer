with open('Assets/Script/GameplayHUD.cs', 'r', encoding='utf-8') as f:
    text = f.read()

# 1. Add cached fields
cached_fields_target = '''    private int _prevCoinsBeforeReward = 0;
    private int _prevGemsBeforeReward = 0;'''

cached_fields_replacement = '''    private int _prevCoinsBeforeReward = 0;
    private int _prevGemsBeforeReward = 0;
    private int _cachedMoves = 0;
    private int _cachedStars = 0;
    private string _cachedRewardBreakdown = "";'''

assert cached_fields_target in text, "cached_fields_target not found"
text = text.replace(cached_fields_target, cached_fields_replacement, 1)

# 2. Replace ShowWinPanel through OnRewardCollectClicked
flow_target = '''    public void ShowWinPanel()
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
            WorldConfig config = WorldConfig.GetConfig(worldIndex);
            bool isWorldGrandFinish = (levelIndex == config.totalLevels - 1 && !SaveManager.HasClaimedWorldReward(worldIndex));

            int awardedCoins = 0;
            int awardedGems = 0;
            string rewardBreakdown = "";

            if (isFirstTime)
            {
                awardedCoins += FIRST_TIME_COINS;
                awardedGems += FIRST_TIME_GEMS;
                SaveManager.SetLevelRewardClaimed(worldIndex, levelIndex, true);
                rewardBreakdown += $"First Clear: +{FIRST_TIME_COINS} 🪙 +{FIRST_TIME_GEMS} 💎\\n";
            }
            else
            {
                awardedCoins += REPLAY_COINS;
                rewardBreakdown += $"Replay: +{REPLAY_COINS} 🪙\\n";
            }

            // 3-Star Bonus
            if (stars == 3 && !had3StarReward)
            {
                awardedCoins += BONUS_3STAR_COINS;
                awardedGems += BONUS_3STAR_GEMS;
                SaveManager.Set3StarRewardClaimed(worldIndex, levelIndex, true);
                rewardBreakdown += $"⭐⭐⭐ Bonus: +{BONUS_3STAR_COINS} 🪙 +{BONUS_3STAR_GEMS} 💎\\n";
            }

            // World completion grand reward
            if (isWorldGrandFinish)
            {
                awardedCoins += config.worldClearCoins;
                awardedGems += config.worldClearGems;
                SaveManager.SetWorldRewardClaimed(worldIndex, true);
                rewardBreakdown += $"🏆 {config.displayName} Cleared! Grand Reward: +{config.worldClearCoins} 🪙 +{config.worldClearGems} 💎!\\n";
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
                winMovesText.text = $"Solved in {moves} moves! ({stars} ⭐)\\n{rewardBreakdown.TrimEnd()}";
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
    }'''

flow_replacement = '''    public void ShowWinPanel()
    {
        if (undoButton != null) undoButton.interactable = false;

        int moves = MoveManager.Instance != null ? MoveManager.Instance.MoveCount : 0;
        int stars = CalculateStars(moves);

        // Save progress using SaveManager
        int worldIndex = SaveManager.GetSelectedWorld();
        int levelIndex = SaveManager.GetSelectedLevel();
        SaveManager.SaveLevelStars(worldIndex, levelIndex, stars);
        SaveManager.UnlockLevel(worldIndex, levelIndex + 1);

        int awardedCoins = 0;
        int awardedGems = 0;
        string rewardBreakdown = "";
        int prevCoins = SaveManager.GetCoins();
        int prevGems  = SaveManager.GetGems();

        // Reward calculation
        if (!_rewardGiven)
        {
            _rewardGiven = true;

            bool isFirstTime = !SaveManager.HasClaimedLevelReward(worldIndex, levelIndex);
            bool had3StarReward = SaveManager.HasClaimed3StarReward(worldIndex, levelIndex);
            WorldConfig config = WorldConfig.GetConfig(worldIndex);
            bool isWorldGrandFinish = (levelIndex == config.totalLevels - 1 && !SaveManager.HasClaimedWorldReward(worldIndex));

            if (isFirstTime)
            {
                awardedCoins += FIRST_TIME_COINS;
                awardedGems += FIRST_TIME_GEMS;
                SaveManager.SetLevelRewardClaimed(worldIndex, levelIndex, true);
                rewardBreakdown += $"First Clear: +{FIRST_TIME_COINS} 🪙 +{FIRST_TIME_GEMS} 💎\\n";
            }
            else
            {
                awardedCoins += REPLAY_COINS;
                rewardBreakdown += $"Replay: +{REPLAY_COINS} 🪙\\n";
            }

            // 3-Star Bonus
            if (stars == 3 && !had3StarReward)
            {
                awardedCoins += BONUS_3STAR_COINS;
                awardedGems += BONUS_3STAR_GEMS;
                SaveManager.Set3StarRewardClaimed(worldIndex, levelIndex, true);
                rewardBreakdown += $"⭐⭐⭐ Bonus: +{BONUS_3STAR_COINS} 🪙 +{BONUS_3STAR_GEMS} 💎\\n";
            }

            // World completion grand reward
            if (isWorldGrandFinish)
            {
                awardedCoins += config.worldClearCoins;
                awardedGems += config.worldClearGems;
                SaveManager.SetWorldRewardClaimed(worldIndex, true);
                rewardBreakdown += $"🏆 {config.displayName} Cleared! Grand Reward: +{config.worldClearCoins} 🪙 +{config.worldClearGems} 💎!\\n";
            }

            // Persist rewards idempotently
            SaveManager.AddCoins(awardedCoins);
            SaveManager.SaveGems(prevGems + awardedGems);

            // Display previous numbers initially so flying tokens and counter roll-up are visible
            if (coinText != null) coinText.text = prevCoins.ToString();
            if (gemText  != null) gemText.text  = prevGems.ToString();

            Debug.Log($"[GameplayHUD] Level {levelIndex + 1} completed! Awarded: +{awardedCoins} coins, +{awardedGems} gems. Total Coins: {SaveManager.GetCoins()}, Gems: {SaveManager.GetGems()}.");
        }
        else
        {
            UpdateCurrencyDisplay();
        }

        _cachedMoves = moves;
        _cachedStars = stars;
        _cachedRewardBreakdown = rewardBreakdown;

        // Sequence control:
        // Agar reward panel available hai aur coins award huye hain,
        // toh pehle sirf RewardPopupPanel khulega. LevelCompletePanel (winPanel) tab tak band rahega.
        if (rewardPanel != null && awardedCoins > 0)
        {
            if (winPanel != null) winPanel.SetActive(false);
            ShowRewardPopup(awardedCoins, awardedGems, prevCoins, prevGems);
        }
        else
        {
            OpenLevelCompletePanelDirectly();
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

        if (rewardCollectBtn != null)
        {
            rewardCollectBtn.interactable = true;
            if (rewardCollectBtn.GetComponent<UIGlowPulse>() == null)
            {
                rewardCollectBtn.gameObject.AddComponent<UIGlowPulse>();
            }
        }

        var transition = rewardPanel.GetComponent<UIPanelTransition>() ?? rewardPanel.AddComponent<UIPanelTransition>();
        transition.PlayOpen();

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
        if (rewardCollectBtn != null) rewardCollectBtn.interactable = false;
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

        Debug.Log("[GameplayHUD] Reward collected. Opening Level Complete panel after delay.");

        // Wait for coins to fly into top counter + satisfying gap before LevelCompletePanel opens
        StartCoroutine(DelayedOpenLevelCompleteRoutine(0.85f));
    }

    private System.Collections.IEnumerator DelayedOpenLevelCompleteRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (this == null || !gameObject.activeInHierarchy) yield break;

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        OpenLevelCompletePanelDirectly();
    }

    public void OpenLevelCompletePanelDirectly()
    {
        if (winPanel == null) return;
        winPanel.SetActive(true);
        winPanel.transform.SetAsLastSibling();

        AudioManager.Instance?.PlayWin();

        var transition = winPanel.GetComponent<UIPanelTransition>() ?? winPanel.AddComponent<UIPanelTransition>();
        transition.PlayOpen();

        if (nextLevelBtn != null && nextLevelBtn.GetComponent<UIGlowPulse>() == null)
        {
            nextLevelBtn.gameObject.AddComponent<UIGlowPulse>();
        }

        UpdateStarDisplay(_cachedStars);
        transition.AnimateStars(starImages, _cachedStars, 0.22f);

        // Celebratory particles & bubbles on Win Panel
        RewardPresentationController.Instance?.SpawnCelebratoryConfetti(winPanel.transform, 14);

        if (winMovesText != null)
        {
            winMovesText.text = $"Solved in {_cachedMoves} moves! ({_cachedStars} ⭐)\\n{_cachedRewardBreakdown.TrimEnd()}";
        }
    }'''

assert flow_target in text, "flow_target not found"
text = text.replace(flow_target, flow_replacement, 1)

with open('Assets/Script/GameplayHUD.cs', 'w', encoding='utf-8') as f:
    f.write(text)

print('Successfully updated GameplayHUD with sequenced Reward -> Level Complete flow!')
