using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ShopManager — Ocean Rescuers in-game shop.
/// 4 tabs: Coins, Gems, Energy, No Ads.
/// Buy buttons deduct currency, give reward, refresh balance display.
/// Attach to ShopPanel root GameObject in MainMenuScene.
/// </summary>
public class ShopManager : MonoBehaviour
{
    // ── Balance display ───────────────────────────────────────────────────────
    [Header("Balance Display (inside ShopPanel)")]
    [SerializeField] private TextMeshProUGUI shopCoinText;
    [SerializeField] private TextMeshProUGUI shopGemText;

    // ── Tab buttons ───────────────────────────────────────────────────────────
    [Header("Tab Buttons")]
    [SerializeField] private Button tabCoins;
    [SerializeField] private Button tabGems;
    [SerializeField] private Button tabEnergy;
    [SerializeField] private Button tabNoAds;

    // Active tab highlight color
    [SerializeField] private Color activeTabColor   = new Color(0.2f, 0.7f, 1f);
    [SerializeField] private Color inactiveTabColor = new Color(0.1f, 0.3f, 0.5f);

    // ── Tab content panels ────────────────────────────────────────────────────
    [Header("Tab Content Panels")]
    [SerializeField] private GameObject contentCoins;
    [SerializeField] private GameObject contentGems;
    [SerializeField] private GameObject contentEnergy;
    [SerializeField] private GameObject contentNoAds;

    // ── Close button ──────────────────────────────────────────────────────────
    [Header("Close")]
    [SerializeField] private Button closeButton;

    // ── Status message ────────────────────────────────────────────────────────
    [Header("Status Message")]
    [SerializeField] private TextMeshProUGUI statusText;

    // ── Shop item definitions ─────────────────────────────────────────────────
    public enum Currency { Coins, Gems }

    [System.Serializable]
    public class ShopItem
    {
        public string   itemId;         // unique key for PlayerPrefs
        public string   displayName;
        public int      coinCost;       // 0 = free / gem cost
        public int      gemCost;        // 0 = coin cost
        public int      rewardCoins;    // coins awarded on purchase
        public int      rewardGems;     // gems  awarded on purchase
        public bool     oneTimePurchase;// if true, disable button after first buy
    }

    // ── Coin tab items ────────────────────────────────────────────────────────
    private static readonly ShopItem[] COIN_ITEMS =
    {
        new ShopItem { itemId="coin_pack_sm",  displayName="Small Coin Pack",   gemCost=5,  rewardCoins=200,  oneTimePurchase=false },
        new ShopItem { itemId="coin_pack_md",  displayName="Medium Coin Pack",  gemCost=10, rewardCoins=500,  oneTimePurchase=false },
        new ShopItem { itemId="coin_pack_lg",  displayName="Large Coin Pack",   gemCost=20, rewardCoins=1200, oneTimePurchase=false },
        new ShopItem { itemId="coin_pack_xl",  displayName="Mega Coin Pack",    gemCost=50, rewardCoins=3000, oneTimePurchase=false },
    };

    // ── Gem tab items ─────────────────────────────────────────────────────────
    private static readonly ShopItem[] GEM_ITEMS =
    {
        new ShopItem { itemId="gem_pack_sm",   displayName="Small Gem Pack",    coinCost=100,  rewardGems=5,   oneTimePurchase=false },
        new ShopItem { itemId="gem_pack_md",   displayName="Medium Gem Pack",   coinCost=250,  rewardGems=15,  oneTimePurchase=false },
        new ShopItem { itemId="gem_pack_lg",   displayName="Large Gem Pack",    coinCost=500,  rewardGems=35,  oneTimePurchase=false },
        new ShopItem { itemId="gem_starter",   displayName="Starter Gems",      coinCost=0,    rewardGems=10,  oneTimePurchase=true  },
    };

    // ── Energy tab items ──────────────────────────────────────────────────────
    private static readonly ShopItem[] ENERGY_ITEMS =
    {
        new ShopItem { itemId="energy_sm",     displayName="5 Energy Refill",   coinCost=50,  rewardCoins=0,  oneTimePurchase=false },
        new ShopItem { itemId="energy_full",   displayName="Full Energy",        gemCost=10,   rewardCoins=0,  oneTimePurchase=false },
        new ShopItem { itemId="energy_inf",    displayName="Unlimited (1 hr)",   gemCost=20,   rewardCoins=0,  oneTimePurchase=false },
    };

    // ── Active tab tracking ───────────────────────────────────────────────────
    private enum Tab { Coins, Gems, Energy, NoAds }
    private Tab _currentTab = Tab.Coins;

    // ─────────────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        RefreshBalance();
        SwitchTab(Tab.Coins);
        HideStatus();
    }

    private void Start()
    {
        // Runtime fallback listeners (Editor wiring is preferred; these are backup)
        if (tabCoins   != null) tabCoins.onClick.AddListener(OnTabCoins);
        if (tabGems    != null) tabGems.onClick.AddListener(OnTabGems);
        if (tabEnergy  != null) tabEnergy.onClick.AddListener(OnTabEnergy);
        if (tabNoAds   != null) tabNoAds.onClick.AddListener(OnTabNoAds);
        if (closeButton != null) closeButton.onClick.AddListener(CloseShop);
    }

    // ── Tab switching ─────────────────────────────────────────────────────────

    private void SwitchTab(Tab tab)
    {
        _currentTab = tab;

        if (contentCoins   != null) contentCoins.SetActive(tab == Tab.Coins);
        if (contentGems    != null) contentGems.SetActive(tab == Tab.Gems);
        if (contentEnergy  != null) contentEnergy.SetActive(tab == Tab.Energy);
        if (contentNoAds   != null) contentNoAds.SetActive(tab == Tab.NoAds);

        UpdateTabHighlights();
        HideStatus();
    }

    private void UpdateTabHighlights()
    {
        SetTabColor(tabCoins,  _currentTab == Tab.Coins);
        SetTabColor(tabGems,   _currentTab == Tab.Gems);
        SetTabColor(tabEnergy, _currentTab == Tab.Energy);
        SetTabColor(tabNoAds,  _currentTab == Tab.NoAds);
    }

    private void SetTabColor(Button btn, bool active)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = active ? activeTabColor : inactiveTabColor;
        var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
    }

    // ── Purchase logic ────────────────────────────────────────────────────────

    /// <summary>
    /// Called by Buy button on each ShopItemCard.
    /// itemIndex matches the array index in the active tab's item list.
    /// </summary>
    public void OnBuyClicked(int itemIndex)
    {
        ShopItem[] items = GetCurrentItems();
        if (items == null || itemIndex < 0 || itemIndex >= items.Length) return;

        ShopItem item = items[itemIndex];

        // Check one-time purchase
        if (item.oneTimePurchase && PlayerPrefs.GetInt("Bought_" + item.itemId, 0) == 1)
        {
            ShowStatus("Already purchased!", Color.yellow);
            return;
        }

        // Try to spend currency
        if (item.gemCost > 0)
        {
            int gems = SaveManager.GetGems();
            if (gems < item.gemCost)
            {
                ShowStatus($"Not enough Gems! Need {item.gemCost} 💎", Color.red);
                return;
            }
            SaveManager.SaveGems(gems - item.gemCost);
        }
        else if (item.coinCost > 0)
        {
            if (!SaveManager.SpendCoins(item.coinCost))
            {
                ShowStatus($"Not enough Coins! Need {item.coinCost} 🪙", Color.red);
                return;
            }
        }

        // Give reward
        if (item.rewardCoins > 0) SaveManager.AddCoins(item.rewardCoins);
        if (item.rewardGems  > 0) SaveManager.SaveGems(SaveManager.GetGems() + item.rewardGems);

        // Mark one-time purchase
        if (item.oneTimePurchase)
        {
            PlayerPrefs.SetInt("Bought_" + item.itemId, 1);
            PlayerPrefs.Save();
        }

        RefreshBalance();
        ShowStatus($"Purchased: {item.displayName}! ✅", new Color(0.2f, 1f, 0.4f));
        AudioManager.Instance?.PlayButtonClick();

        Debug.Log($"[ShopManager] Bought '{item.displayName}'. Coins: {SaveManager.GetCoins()}, Gems: {SaveManager.GetGems()}");
    }

    // ── Convenience wrappers for each tab + index ─────────────────────────────
    // These are called by hardcoded button onClick events in the Editor script.

    // ── Tab switch public methods (called by Editor-wired onClick) ───────────
    public void OnTabCoins()   => SwitchTab(Tab.Coins);
    public void OnTabGems()    => SwitchTab(Tab.Gems);
    public void OnTabEnergy()  => SwitchTab(Tab.Energy);
    public void OnTabNoAds()   => SwitchTab(Tab.NoAds);

    // ── Buy methods (called by Editor-wired BuyButton onClick) ───────────────
    public void BuyCoins0()   => OnBuyClicked_InTab(Tab.Coins,  0);
    public void BuyCoins1()   => OnBuyClicked_InTab(Tab.Coins,  1);
    public void BuyCoins2()   => OnBuyClicked_InTab(Tab.Coins,  2);
    public void BuyCoins3()   => OnBuyClicked_InTab(Tab.Coins,  3);

    public void BuyGems0()    => OnBuyClicked_InTab(Tab.Gems,   0);
    public void BuyGems1()    => OnBuyClicked_InTab(Tab.Gems,   1);
    public void BuyGems2()    => OnBuyClicked_InTab(Tab.Gems,   2);
    public void BuyGems3()    => OnBuyClicked_InTab(Tab.Gems,   3);

    public void BuyEnergy0()  => OnBuyClicked_InTab(Tab.Energy, 0);
    public void BuyEnergy1()  => OnBuyClicked_InTab(Tab.Energy, 1);
    public void BuyEnergy2()  => OnBuyClicked_InTab(Tab.Energy, 2);

    private void OnBuyClicked_InTab(Tab tab, int index)
    {
        ShopItem[] items = GetItemsForTab(tab);
        if (items == null || index >= items.Length) return;
        ShopItem item = items[index];

        // Check one-time
        if (item.oneTimePurchase && PlayerPrefs.GetInt("Bought_" + item.itemId, 0) == 1)
        { ShowStatus("Already purchased!", Color.yellow); return; }

        if (item.gemCost > 0)
        {
            if (SaveManager.GetGems() < item.gemCost)
            { ShowStatus($"Need {item.gemCost} 💎 Gems!", Color.red); return; }
            SaveManager.SaveGems(SaveManager.GetGems() - item.gemCost);
        }
        else if (item.coinCost > 0)
        {
            if (!SaveManager.SpendCoins(item.coinCost))
            { ShowStatus($"Need {item.coinCost} 🪙 Coins!", Color.red); return; }
        }

        if (item.rewardCoins > 0) SaveManager.AddCoins(item.rewardCoins);
        if (item.rewardGems  > 0) SaveManager.SaveGems(SaveManager.GetGems() + item.rewardGems);

        if (item.oneTimePurchase)
        { PlayerPrefs.SetInt("Bought_" + item.itemId, 1); PlayerPrefs.Save(); }

        RefreshBalance();
        ShowStatus($"✅ {item.displayName} purchased!", new Color(0.2f, 1f, 0.4f));
        AudioManager.Instance?.PlayButtonClick();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ShopItem[] GetCurrentItems() => GetItemsForTab(_currentTab);

    private ShopItem[] GetItemsForTab(Tab tab)
    {
        return tab switch
        {
            Tab.Coins  => COIN_ITEMS,
            Tab.Gems   => GEM_ITEMS,
            Tab.Energy => ENERGY_ITEMS,
            _          => null
        };
    }

    public void RefreshBalance()
    {
        if (shopCoinText != null) shopCoinText.text = SaveManager.GetCoins().ToString();
        if (shopGemText  != null) shopGemText.text  = SaveManager.GetGems().ToString();
    }

    private void ShowStatus(string msg, Color color)
    {
        if (statusText == null) return;
        statusText.text  = msg;
        statusText.color = color;
        statusText.gameObject.SetActive(true);
        CancelInvoke(nameof(HideStatus));
        Invoke(nameof(HideStatus), 2.5f);
    }

    private void HideStatus()
    {
        if (statusText != null) statusText.gameObject.SetActive(false);
    }

    public void CloseShop()
    {
        gameObject.SetActive(false);
    }
}
