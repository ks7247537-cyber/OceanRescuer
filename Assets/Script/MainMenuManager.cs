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

        var existing = canvas.transform.Find("TopBar/EnergyBadge") ?? canvas.transform.Find("EnergyBadge");
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
            string e = EnergyManager.Instance.GetFormattedEnergy();
            if (EnergyManager.Instance.CurrentEnergy < EnergyManager.Instance.MaxEnergy)
            {
                int sec = EnergyManager.Instance.SecondsUntilNextEnergy;
                energyText.text = $"{e} ({sec / 60:D2}:{sec % 60:D2})";
            }
            else
            {
                energyText.text = e;
            }
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
        bool isOpen = shopPanel != null && shopPanel.activeSelf;
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel     != null) shopPanel.SetActive(!isOpen);

        // Refresh balance when opening
        if (shopPanel != null && shopPanel.activeSelf)
        {
            var sm = shopPanel.GetComponent<ShopManager>();
            if (sm != null) sm.RefreshBalance();
            RefreshCurrencyDisplay();
        }
    }

    public void OnCloseShopClicked()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        RefreshCurrencyDisplay(); // update top bar after any purchases
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    public void OnSettingsClicked()
    {
        AudioManager.Instance?.PlayButtonClick();
        bool isOpen = settingsPanel != null && settingsPanel.activeSelf;
        if (shopPanel     != null) shopPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(!isOpen);
    }

    public void OnCloseSettingsClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    // ── Settings toggles (legacy — kept for backward compat) ─────────────────
    public void OnMusicToggleClicked() => AudioManager.Instance?.ToggleMusic();
    public void OnSFXToggleClicked()   => AudioManager.Instance?.ToggleSFX();
}
