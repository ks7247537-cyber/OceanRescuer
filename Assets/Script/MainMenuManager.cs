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

    [Header("Currency Display (top bar)")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI gemText;

    private void Start()
    {
        if (shopPanel     != null) shopPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        RefreshCurrencyDisplay();
        AudioManager.Instance?.PlayMenuMusic();
    }

    private void OnEnable() => RefreshCurrencyDisplay();

    public void RefreshCurrencyDisplay()
    {
        if (coinText != null) coinText.text = SaveManager.GetCoins().ToString();
        if (gemText  != null) gemText.text  = SaveManager.GetGems().ToString();
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
