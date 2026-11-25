using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ShopManager — Ocean Rescuers fully functional, animated in-game shop.
/// Supports 4 categories: Items, Energy, Rewards, Gems.
/// Real-time balance updates, one-time purchase protection, and juicy visual feedback.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public enum ShopCategory { Items, Energy, Rewards, Gems }

    [System.Serializable]
    public class ShopItemData
    {
        public string itemId;
        public string displayName;
        public string description;
        public int coinCost;
        public int gemCost;
        public int rewardCoins;
        public int rewardGems;
        public int rewardEnergy;
        public float rewardUnlimitedHours;
        public string rewardPowerUp;
        public int rewardPowerUpCount;
        public bool oneTimePurchase;

        public bool IsFree => coinCost == 0 && gemCost == 0;
    }

    // ── Static Catalog ────────────────────────────────────────────────────────
    public static readonly ShopItemData[] ITEMS_CATALOG = new[]
    {
        new ShopItemData { itemId = "item_magnet",    displayName = "Coin Magnet",   description = "Attracts nearby coins", coinCost = 100, rewardPowerUp = "Magnet", rewardPowerUpCount = 1 },
        new ShopItemData { itemId = "item_freeze",    displayName = "Freeze Timer",  description = "Freezes level timer",   coinCost = 100, rewardPowerUp = "Freeze", rewardPowerUpCount = 1 },
        new ShopItemData { itemId = "item_wrench",    displayName = "Super Wrench",  description = "Auto-rotates next pipe",coinCost = 150, rewardPowerUp = "Wrench", rewardPowerUpCount = 1 },
        new ShopItemData { itemId = "item_hint",      displayName = "Path Hint",     description = "Highlights flow path",  coinCost = 50,  rewardPowerUp = "Hint",   rewardPowerUpCount = 1 },
        new ShopItemData { itemId = "item_bubble",    displayName = "Bubble Shield", description = "Shields water pipes",   coinCost = 100, rewardPowerUp = "Bubble", rewardPowerUpCount = 1 },
        new ShopItemData { itemId = "item_lightning", displayName = "Zap Blast",     description = "Clears pipe obstacles", coinCost = 150, rewardPowerUp = "Lightning", rewardPowerUpCount = 1 },
    };

    public static readonly ShopItemData[] ENERGY_CATALOG = new[]
    {
        new ShopItemData { itemId = "energy_5",     displayName = "5 Energy",         description = "+5 Instant Energy",  coinCost = 50, rewardEnergy = 5 },
        new ShopItemData { itemId = "energy_full",  displayName = "Full Energy",      description = "Refill to 30/30 max",gemCost = 10,  rewardEnergy = 30 },
        new ShopItemData { itemId = "energy_inf_1h",displayName = "1 Hr Unlimited",   description = "Unlimited plays",    gemCost = 20,  rewardUnlimitedHours = 1f },
    };

    public static readonly ShopItemData[] REWARDS_CATALOG = new[]
    {
        new ShopItemData { itemId = "reward_daily",   displayName = "Daily Pearl Gift",  description = "+100 Free Coins",    coinCost = 0, gemCost = 0, rewardCoins = 100, oneTimePurchase = true },
        new ShopItemData { itemId = "reward_starter", displayName = "Starter Package",   description = "+10 Gems & 200 Coins",coinCost = 0, gemCost = 0, rewardCoins = 200, rewardGems = 10, oneTimePurchase = true },
        new ShopItemData { itemId = "reward_bundle",  displayName = "Explorer Bundle",   description = "+15 Gems + 2x Hints", coinCost = 400, rewardGems = 15, rewardPowerUp = "Hint", rewardPowerUpCount = 2 },
    };

    public static readonly ShopItemData[] GEMS_CATALOG = new[]
    {
        new ShopItemData { itemId = "gem_pack_sm", displayName = "Small Gem Pouch", description = "+5 Shiny Gems",  coinCost = 100,  rewardGems = 5 },
        new ShopItemData { itemId = "gem_pack_md", displayName = "Medium Gem Bag",  description = "+15 Shiny Gems", coinCost = 250,  rewardGems = 15 },
        new ShopItemData { itemId = "gem_pack_lg", displayName = "Large Gem Chest", description = "+35 Shiny Gems", coinCost = 500,  rewardGems = 35 },
        new ShopItemData { itemId = "gem_pack_xl", displayName = "Treasure Vault",  description = "+80 Shiny Gems", coinCost = 1000, rewardGems = 80 },
    };

    // ── UI References ─────────────────────────────────────────────────────────
    [Header("Currency Balance Displays")]
    [SerializeField] private TextMeshProUGUI shopCoinText;
    [SerializeField] private TextMeshProUGUI shopGemText;

    [Header("Category Navigation Buttons")]
    [SerializeField] private Button tabItems;
    [SerializeField] private Button tabEnergy;
    [SerializeField] private Button tabRewards;
    [SerializeField] private Button tabGems;

    [Header("Category Content Panels")]
    [SerializeField] private GameObject contentItems;
    [SerializeField] private GameObject contentEnergy;
    [SerializeField] private GameObject contentRewards;
    [SerializeField] private GameObject contentGems;

    [Header("Close Button")]
    [SerializeField] private Button closeButton;

    [Header("Status Feedback Banner")]
    [SerializeField] private GameObject statusBanner;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image statusImage;

    [Header("Colors & Styling")]
    [SerializeField] private Color tabActiveColor = new Color(0.12f, 0.70f, 0.95f, 1f);
    [SerializeField] private Color tabInactiveColor = new Color(0.08f, 0.28f, 0.48f, 0.9f);
    [SerializeField] private Color successColor = new Color(0.15f, 0.85f, 0.35f, 0.95f);
    [SerializeField] private Color failureColor = new Color(0.90f, 0.20f, 0.20f, 0.95f);

    private ShopCategory _currentCategory = ShopCategory.Items;
    private Coroutine _statusCoroutine;

    private void OnEnable()
    {
        RefreshBalance();
        SwitchCategory(ShopCategory.Items);
        RefreshAllButtons();
        if (statusBanner != null) statusBanner.SetActive(false);
    }

    private void Start()
    {
        // Wire listeners if not already wired in editor
        if (tabItems != null) tabItems.onClick.AddListener(OnTabItemsClicked);
        if (tabEnergy != null) tabEnergy.onClick.AddListener(OnTabEnergyClicked);
        if (tabRewards != null) tabRewards.onClick.AddListener(OnTabRewardsClicked);
        if (tabGems != null) tabGems.onClick.AddListener(OnTabGemsClicked);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
    }

    // ── Category Switching ────────────────────────────────────────────────────
    public void OnTabItemsClicked()   => SwitchCategory(ShopCategory.Items);
    public void OnTabEnergyClicked()  => SwitchCategory(ShopCategory.Energy);
    public void OnTabRewardsClicked() => SwitchCategory(ShopCategory.Rewards);
    public void OnTabGemsClicked()    => SwitchCategory(ShopCategory.Gems);

    // Backward compatibility for legacy naming
    public void OnTabCoins()  => OnTabItemsClicked();
    public void OnTabNoAds()  => OnTabRewardsClicked();

    public void SwitchCategory(ShopCategory category)
    {
        _currentCategory = category;

        if (contentItems != null) contentItems.SetActive(category == ShopCategory.Items);
        if (contentEnergy != null) contentEnergy.SetActive(category == ShopCategory.Energy);
        if (contentRewards != null) contentRewards.SetActive(category == ShopCategory.Rewards);
        if (contentGems != null) contentGems.SetActive(category == ShopCategory.Gems);

        UpdateCategoryHighlights();
        RefreshAllButtons();
    }

    private void UpdateCategoryHighlights()
    {
        SetTabState(tabItems, _currentCategory == ShopCategory.Items);
        SetTabState(tabEnergy, _currentCategory == ShopCategory.Energy);
        SetTabState(tabRewards, _currentCategory == ShopCategory.Rewards);
        SetTabState(tabGems, _currentCategory == ShopCategory.Gems);
    }

    private void SetTabState(Button tabBtn, bool isActive)
    {
        if (tabBtn == null) return;

        var img = tabBtn.GetComponent<Image>();
        if (img != null)
        {
            img.color = isActive ? tabActiveColor : tabInactiveColor;
        }

        var tmp = tabBtn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.color = isActive ? Color.white : new Color(0.7f, 0.88f, 1f, 0.85f);
            tmp.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
        }

        tabBtn.transform.localScale = isActive ? new Vector3(1.05f, 1.05f, 1f) : Vector3.one;
    }

    // ── Purchase Processing ───────────────────────────────────────────────────

    public bool BuyItem(ShopItemData item)
    {
        if (item == null) return false;

        // Check if one-time item was already purchased
        if (item.oneTimePurchase && PlayerPrefs.GetInt("Bought_" + item.itemId, 0) == 1)
        {
            ShowFeedback("Already Claimed!", failureColor);
            return false;
        }

        // Validate & Spend Currency
        if (item.gemCost > 0)
        {
            int currentGems = SaveManager.GetGems();
            if (currentGems < item.gemCost)
            {
                ShowFeedback($"Not enough Gems! Need {item.gemCost} \ud83d\udc8e", failureColor);
                return false;
            }
            SaveManager.SaveGems(currentGems - item.gemCost);
        }
        else if (item.coinCost > 0)
        {
            if (!SaveManager.SpendCoins(item.coinCost))
            {
                ShowFeedback($"Not enough Coins! Need {item.coinCost} \U0001fa99", failureColor);
                return false;
            }
        }

        // Grant Rewards
        if (item.rewardCoins > 0) SaveManager.AddCoins(item.rewardCoins);
        if (item.rewardGems > 0) SaveManager.SaveGems(SaveManager.GetGems() + item.rewardGems);

        if (item.rewardEnergy > 0)
        {
            if (item.rewardEnergy >= 30) EnergyManager.Instance?.RefillFullEnergy();
            else EnergyManager.Instance?.AddEnergy(item.rewardEnergy);
        }
        if (item.rewardUnlimitedHours > 0)
        {
            EnergyManager.Instance?.GrantUnlimitedEnergy(item.rewardUnlimitedHours);
        }

        if (!string.IsNullOrEmpty(item.rewardPowerUp))
        {
            SaveManager.AddPowerUp(item.rewardPowerUp, Mathf.Max(1, item.rewardPowerUpCount));
        }

        // Record one-time purchase
        if (item.oneTimePurchase)
        {
            PlayerPrefs.SetInt("Bought_" + item.itemId, 1);
            PlayerPrefs.Save();
        }

        // Update displays
        RefreshBalance();
        RefreshAllButtons();

        // Success audio & visual feedback
        AudioManager.Instance?.PlayButtonClick();
        ShowFeedback($"Purchased: {item.displayName}! \u2705", successColor);

        Debug.Log($"[ShopManager] Purchased '{item.displayName}'. Coins: {SaveManager.GetCoins()}, Gems: {SaveManager.GetGems()}");
        return true;
    }

    // ── Purchase Wrappers for Editor Buttons ───────────────────────────────────

    public void BuyItems0() => BuyItem(ITEMS_CATALOG[0]);
    public void BuyItems1() => BuyItem(ITEMS_CATALOG[1]);
    public void BuyItems2() => BuyItem(ITEMS_CATALOG[2]);
    public void BuyItems3() => BuyItem(ITEMS_CATALOG[3]);
    public void BuyItems4() => BuyItem(ITEMS_CATALOG[4]);
    public void BuyItems5() => BuyItem(ITEMS_CATALOG[5]);

    public void BuyEnergy0() => BuyItem(ENERGY_CATALOG[0]);
    public void BuyEnergy1() => BuyItem(ENERGY_CATALOG[1]);
    public void BuyEnergy2() => BuyItem(ENERGY_CATALOG[2]);

    public void BuyRewards0() => BuyItem(REWARDS_CATALOG[0]);
    public void BuyRewards1() => BuyItem(REWARDS_CATALOG[1]);
    public void BuyRewards2() => BuyItem(REWARDS_CATALOG[2]);

    public void BuyGems0() => BuyItem(GEMS_CATALOG[0]);
    public void BuyGems1() => BuyItem(GEMS_CATALOG[1]);
    public void BuyGems2() => BuyItem(GEMS_CATALOG[2]);
    public void BuyGems3() => BuyItem(GEMS_CATALOG[3]);

    // Legacy wrappers for backward compatibility
    public void BuyCoins0() => BuyGems0();
    public void BuyCoins1() => BuyGems1();
    public void BuyCoins2() => BuyGems2();
    public void BuyCoins3() => BuyGems3();

    // ── UI Updates ────────────────────────────────────────────────────────────

    public void RefreshBalance()
    {
        if (shopCoinText != null) shopCoinText.text = SaveManager.GetCoins().ToString("N0");
        if (shopGemText != null) shopGemText.text = SaveManager.GetGems().ToString("N0");

        // Also refresh MainMenu currency display if present
        var mmm = Object.FindAnyObjectByType<MainMenuManager>();
        if (mmm != null) mmm.RefreshCurrencyDisplay();
    }

    public void RefreshAllButtons()
    {
        // Update any one-time buy buttons
        UpdateButtonForOneTime(contentRewards != null ? contentRewards.transform : null, "reward_daily", 0);
        UpdateButtonForOneTime(contentRewards != null ? contentRewards.transform : null, "reward_starter", 1);
    }

    private void UpdateButtonForOneTime(Transform parent, string itemId, int cardIndex)
    {
        if (parent == null) return;
        Transform card = null;
        if (cardIndex >= 0 && cardIndex < parent.childCount)
        {
            card = parent.GetChild(cardIndex);
        }
        else
        {
            string cardName = $"Card_{cardIndex}";
            card = parent.Find(cardName);
        }
        if (card == null) return;

        var buyBtn = card.Find("BuyButton")?.GetComponent<Button>();
        var buyTxt = card.Find("BuyButton/Text")?.GetComponent<TextMeshProUGUI>();
        if (buyTxt == null) buyTxt = card.Find("BuyButton/PriceText")?.GetComponent<TextMeshProUGUI>();

        if (buyBtn != null && PlayerPrefs.GetInt("Bought_" + itemId, 0) == 1)
        {
            buyBtn.interactable = false;
            if (buyTxt != null) buyTxt.text = "CLAIMED";
        }
    }

    // ── Feedback Banner ───────────────────────────────────────────────────────

    private void ShowFeedback(string message, Color bgColor)
    {
        if (statusBanner == null || statusText == null) return;

        statusText.text = message;
        if (statusImage != null) statusImage.color = bgColor;
        statusBanner.SetActive(true);

        if (_statusCoroutine != null) StopCoroutine(_statusCoroutine);
        _statusCoroutine = StartCoroutine(AnimateFeedbackBanner());
    }

    private IEnumerator AnimateFeedbackBanner()
    {
        statusBanner.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        float elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / 0.15f;
            statusBanner.transform.localScale = Vector3.Lerp(new Vector3(0.8f, 0.8f, 1f), Vector3.one, t);
            yield return null;
        }
        statusBanner.transform.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(2.0f);

        elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / 0.2f;
            statusBanner.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.8f, 0f, 1f), t);
            yield return null;
        }

        statusBanner.SetActive(false);
        statusBanner.transform.localScale = Vector3.one;
    }

    public void CloseShop()
    {
        AudioManager.Instance?.PlayButtonClick();
        var transition = GetComponent<UIPanelTransition>();
        if (transition != null)
        {
            transition.PlayClose(() =>
            {
                var mmm = Object.FindAnyObjectByType<MainMenuManager>();
                if (mmm != null) mmm.RefreshCurrencyDisplay();
            });
        }
        else
        {
            gameObject.SetActive(false);
            var mmm = Object.FindAnyObjectByType<MainMenuManager>();
            if (mmm != null) mmm.RefreshCurrencyDisplay();
        }
    }
}
