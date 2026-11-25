using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main Menu scene controller.
/// Manages ShopPanel, SettingsPanel, currency display, navigation.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Currency & Energy Display (top bar)")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI gemText;
    [SerializeField] private TextMeshProUGUI energyText;

    private void Start()
    {
        if (shopPanel     != null) shopPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        SetupEnergyDisplay();
        RefreshCurrencyDisplay();
        AudioManager.Instance?.PlayMenuMusic();

        var canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            if (canvas.GetComponent<UIBubbleEffect>() == null)
            {
                canvas.gameObject.AddComponent<UIBubbleEffect>();
            }
            UIButtonFeedback.AttachRecursively(canvas.transform);

            var playBtn = canvas.transform.Find("PlayButton");
            if (playBtn != null && playBtn.GetComponent<UIGlowPulse>() == null)
            {
                playBtn.gameObject.AddComponent<UIGlowPulse>();
            }

            // Mobile Safe Area Protection for Top Bar Badges
            var coinPanel = canvas.transform.Find("CoinPanel");
            if (coinPanel != null && coinPanel.GetComponent<UISafeAreaHandler>() == null)
            {
                coinPanel.gameObject.AddComponent<UISafeAreaHandler>();
            }
            var gemPanel = canvas.transform.Find("GemPanel");
            if (gemPanel != null && gemPanel.GetComponent<UISafeAreaHandler>() == null)
            {
                gemPanel.gameObject.AddComponent<UISafeAreaHandler>();
            }
        }

        EnergyManager.OnEnergyChanged += RefreshCurrencyDisplay;
    }

    private void OnDestroy()
    {
        EnergyManager.OnEnergyChanged -= RefreshCurrencyDisplay;
    }

    private void OnEnable() => RefreshCurrencyDisplay();

    private void SetupEnergyDisplay()
    {
        if (energyText != null) return;

        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        var existing = canvas.transform.Find("SafeAreaContainer/TopBar/EnergyBadge")
                    ?? canvas.transform.Find("TopBar/EnergyBadge") 
                    ?? canvas.transform.Find("EnergyBadge");
        if (existing != null)
        {
            energyText = existing.GetComponentInChildren<TextMeshProUGUI>();
            if (energyText != null)
            {
                energyText.enableAutoSizing = true;
                energyText.fontSizeMin = 18;
                energyText.fontSizeMax = 28;
                energyText.fontStyle = FontStyles.Bold;
                energyText.alignment = TextAlignmentOptions.Midline;
            }
            return;
        }
    }

    public void RefreshCurrencyDisplay()
    {
        if (coinText != null) coinText.text = SaveManager.GetCoins().ToString();
        if (gemText  != null) gemText.text  = SaveManager.GetGems().ToString();
        if (energyText != null && EnergyManager.Instance != null)
        {
            energyText.text = EnergyManager.Instance.GetFormattedEnergy();
        }
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    public void OnPlayClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        SceneLoader.LoadScene(SceneLoader.WORLD_SELECT);
    }

    // ── Shop ──────────────────────────────────────────────────────────────────

    public void OnShopClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            var st = settingsPanel.GetComponent<UIPanelTransition>();
            if (st != null) st.PlayClose();
            else settingsPanel.SetActive(false);
        }

        if (shopPanel != null)
        {
            bool isOpen = shopPanel.activeSelf;
            if (!isOpen)
            {
                shopPanel.SetActive(true);
                var transition = shopPanel.GetComponent<UIPanelTransition>() ?? shopPanel.AddComponent<UIPanelTransition>();
                transition.PlayOpen();
                var sm = shopPanel.GetComponent<ShopManager>();
                if (sm != null) sm.RefreshBalance();
                RefreshCurrencyDisplay();
            }
            else
            {
                var transition = shopPanel.GetComponent<UIPanelTransition>();
                if (transition != null) transition.PlayClose();
                else shopPanel.SetActive(false);
            }
        }
    }

    public void OnCloseShopClicked()
    {
        if (shopPanel != null)
        {
            var transition = shopPanel.GetComponent<UIPanelTransition>();
            if (transition != null) transition.PlayClose();
            else shopPanel.SetActive(false);
        }
        RefreshCurrencyDisplay();
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    public void OnSettingsClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        if (shopPanel != null && shopPanel.activeSelf)
        {
            var st = shopPanel.GetComponent<UIPanelTransition>();
            if (st != null) st.PlayClose();
            else shopPanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            bool isOpen = settingsPanel.activeSelf;
            if (!isOpen)
            {
                settingsPanel.SetActive(true);
                var transition = settingsPanel.GetComponent<UIPanelTransition>() ?? settingsPanel.AddComponent<UIPanelTransition>();
                transition.PlayOpen();
            }
            else
            {
                var transition = settingsPanel.GetComponent<UIPanelTransition>();
                if (transition != null) transition.PlayClose();
                else settingsPanel.SetActive(false);
            }
        }
    }

    public void OnCloseSettingsClicked()
    {
        if (settingsPanel != null)
        {
            var transition = settingsPanel.GetComponent<UIPanelTransition>();
            if (transition != null) transition.PlayClose();
            else settingsPanel.SetActive(false);
        }
    }

    // ── Settings toggles (legacy — kept for backward compat) ─────────────────
    public void OnMusicToggleClicked() => AudioManager.Instance?.ToggleMusic();
    public void OnSFXToggleClicked()   => AudioManager.Instance?.ToggleSFX();
}
