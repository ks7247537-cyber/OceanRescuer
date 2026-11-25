using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SettingsManager — Manages Sound and Music toggles for Ocean Rescuers.
/// Supports both separate dual ON / OFF buttons and legacy single toggle button.
/// Reads/writes PlayerPrefs and integrates directly with AudioManager.
/// Attached to SettingsPanel root GameObject.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // ── Dual ON / OFF Buttons ─────────────────────────────────────────────────
    [Header("Dual ON / OFF Buttons")]
    [SerializeField] private Button soundOnButton;
    [SerializeField] private Button soundOffButton;
    [SerializeField] private Button musicOnButton;
    [SerializeField] private Button musicOffButton;

    // ── Single Toggle / Close Buttons (Legacy & Standard) ─────────────────────
    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button soundButton;

    // ── Sprites for ON / OFF states ───────────────────────────────────────────
    [Header("Sprites")]
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    // ── Labels ────────────────────────────────────────────────────────────────
    [Header("Section Text Labels")]
    [SerializeField] private TextMeshProUGUI musicText;
    [SerializeField] private TextMeshProUGUI soundText;

    // ── State Icon Images ─────────────────────────────────────────────────────
    [Header("State Icon Images")]
    [SerializeField] private Image soundIcon;
    [SerializeField] private Image musicIcon;

    // ── Toggle Indicators ─────────────────────────────────────────────────────
    [Header("Toggle Indicators")]
    [SerializeField] private Image soundToggleIndicator;
    [SerializeField] private Image musicToggleIndicator;
    [SerializeField] private TextMeshProUGUI soundToggleText;
    [SerializeField] private TextMeshProUGUI musicToggleText;

    // ── Colors for ON / OFF ───────────────────────────────────────────────────
    [Header("Toggle Colors")]
    [SerializeField] private Color onColor  = new Color(0.18f, 0.85f, 0.35f, 1f);
    [SerializeField] private Color offColor = new Color(0.85f, 0.22f, 0.22f, 1f);
    [SerializeField] private Color inactiveDimColor = new Color(0.5f, 0.5f, 0.5f, 0.45f);

    // ── State ─────────────────────────────────────────────────────────────────
    private bool _musicOn;
    private bool _soundOn;

    // ── Backward Compatibility Properties ─────────────────────────────────────
    public Image soundIconImage { get => soundIcon; set => soundIcon = value; }
    public Image musicIconImage { get => musicIcon; set => musicIcon = value; }
    public TextMeshProUGUI soundLabel { get => soundText; set => soundText = value; }
    public TextMeshProUGUI musicLabel { get => musicText; set => musicText = value; }

    // ─────────────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        // Auto-find references if missing
        AutoFindReferences();

        // Load saved state
        _musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        _soundOn = PlayerPrefs.GetInt("SFXEnabled",   1) == 1;

        RefreshUI();
    }

    private void Start()
    {
        // Hook up Dual ON / OFF buttons
        if (soundOnButton != null)
        {
            soundOnButton.onClick.RemoveListener(OnSoundOnClicked);
            soundOnButton.onClick.AddListener(OnSoundOnClicked);
        }
        if (soundOffButton != null)
        {
            soundOffButton.onClick.RemoveListener(OnSoundOffClicked);
            soundOffButton.onClick.AddListener(OnSoundOffClicked);
        }
        if (musicOnButton != null)
        {
            musicOnButton.onClick.RemoveListener(OnMusicOnClicked);
            musicOnButton.onClick.AddListener(OnMusicOnClicked);
        }
        if (musicOffButton != null)
        {
            musicOffButton.onClick.RemoveListener(OnMusicOffClicked);
            musicOffButton.onClick.AddListener(OnMusicOffClicked);
        }

        // Hook up legacy single buttons
        if (musicButton != null)
        {
            musicButton.onClick.RemoveListener(OnMusicToggle);
            musicButton.onClick.AddListener(OnMusicToggle);
        }
        if (soundButton != null)
        {
            soundButton.onClick.RemoveListener(OnSoundToggle);
            soundButton.onClick.AddListener(OnSoundToggle);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseSettings);
            closeButton.onClick.AddListener(CloseSettings);
        }
    }

    private void AutoFindReferences()
    {
        if (closeButton == null)
            closeButton = transform.Find("PanelFrame/CloseButton")?.GetComponent<Button>();

        // Dual buttons auto-find
        if (soundOnButton == null)
            soundOnButton = transform.Find("PanelFrame/SoundSection/SoundOnButton")?.GetComponent<Button>();
        if (soundOffButton == null)
            soundOffButton = transform.Find("PanelFrame/SoundSection/SoundOffButton")?.GetComponent<Button>();

        if (musicOnButton == null)
            musicOnButton = transform.Find("PanelFrame/MusicSection/MusicOnButton")?.GetComponent<Button>();
        if (musicOffButton == null)
            musicOffButton = transform.Find("PanelFrame/MusicSection/MusicOffButton")?.GetComponent<Button>();

        // Legacy single buttons
        if (soundButton == null)
            soundButton = transform.Find("PanelFrame/SoundSection/SoundButton")?.GetComponent<Button>();
        if (musicButton == null)
            musicButton = transform.Find("PanelFrame/MusicSection/MusicButton")?.GetComponent<Button>();

        // Icons
        if (soundIcon == null)
            soundIcon = transform.Find("PanelFrame/SoundSection/SoundIcon")?.GetComponent<Image>()
                     ?? soundButton?.transform.Find("SoundIcon")?.GetComponent<Image>();

        if (musicIcon == null)
            musicIcon = transform.Find("PanelFrame/MusicSection/MusicIcon")?.GetComponent<Image>()
                     ?? musicButton?.transform.Find("MusicIcon")?.GetComponent<Image>();

        // Indicators & text
        if (soundToggleIndicator == null)
            soundToggleIndicator = soundButton?.transform.Find("SoundToggleIndicator")?.GetComponent<Image>()
                                ?? soundButton?.GetComponent<Image>();

        if (musicToggleIndicator == null)
            musicToggleIndicator = musicButton?.transform.Find("MusicToggleIndicator")?.GetComponent<Image>()
                                ?? musicButton?.GetComponent<Image>();

        if (soundToggleText == null)
            soundToggleText = soundButton?.transform.Find("SoundToggleIndicator/Text")?.GetComponent<TextMeshProUGUI>()
                           ?? soundButton?.GetComponentInChildren<TextMeshProUGUI>();

        if (musicToggleText == null)
            musicToggleText = musicButton?.transform.Find("MusicToggleIndicator/Text")?.GetComponent<TextMeshProUGUI>()
                           ?? musicButton?.GetComponentInChildren<TextMeshProUGUI>();

        if (soundText == null)
            soundText = transform.Find("PanelFrame/SoundSection/SoundText")?.GetComponent<TextMeshProUGUI>();

        if (musicText == null)
            musicText = transform.Find("PanelFrame/MusicSection/MusicText")?.GetComponent<TextMeshProUGUI>();

        // Default sprites if null
        if (soundOnSprite == null) soundOnSprite = Resources.Load<Sprite>("Sprites/button_sound");
        if (soundOffSprite == null) soundOffSprite = Resources.Load<Sprite>("Sprites/button_mute");
        if (musicOnSprite == null) musicOnSprite = Resources.Load<Sprite>("Sprites/button_sound");
        if (musicOffSprite == null) musicOffSprite = Resources.Load<Sprite>("Sprites/button_mute");
    }

    // ── Dedicated ON / OFF Click Handlers ─────────────────────────────────────

    public void OnSoundOnClicked()
    {
        SetSound(true);
    }

    public void OnSoundOffClicked()
    {
        SetSound(false);
    }

    public void OnMusicOnClicked()
    {
        SetMusic(true);
    }

    public void OnMusicOffClicked()
    {
        SetMusic(false);
    }

    public void SetSound(bool enable)
    {
        _soundOn = enable;
        PlayerPrefs.SetInt("SFXEnabled", _soundOn ? 1 : 0);
        PlayerPrefs.Save();

        AudioManager.Instance?.SetSFXEnabled(_soundOn);

        if (_soundOn)
        {
            AudioManager.Instance?.PlayButtonClick();
        }

        RefreshUI();
        Debug.Log($"[SettingsManager] Sound set to: {(_soundOn ? "ON" : "OFF")}");
    }

    public void SetMusic(bool enable)
    {
        _musicOn = enable;
        PlayerPrefs.SetInt("MusicEnabled", _musicOn ? 1 : 0);
        PlayerPrefs.Save();

        AudioManager.Instance?.SetMusicEnabled(_musicOn);

        AudioManager.Instance?.PlayButtonClick();
        RefreshUI();
        Debug.Log($"[SettingsManager] Music set to: {(_musicOn ? "ON" : "OFF")}");
    }

    // ── Legacy Toggle Handlers ────────────────────────────────────────────────

    public void OnMusicToggle()
    {
        SetMusic(!_musicOn);
    }

    public void OnSoundToggle()
    {
        SetSound(!_soundOn);
    }

    // ── UI Refresh ────────────────────────────────────────────────────────────

    public void RefreshUI()
    {
        // 1. Update Dual Sound Buttons (ON / OFF)
        UpdateDualButtons(soundOnButton, soundOffButton, _soundOn);

        // 2. Update Dual Music Buttons (ON / OFF)
        UpdateDualButtons(musicOnButton, musicOffButton, _musicOn);

        // 3. Update Sound Icon
        if (soundIcon != null)
        {
            Sprite target = _soundOn ? soundOnSprite : soundOffSprite;
            if (target != null) soundIcon.sprite = target;
            soundIcon.preserveAspect = true;
            soundIcon.color = _soundOn ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.9f);
        }

        // 4. Update Music Icon
        if (musicIcon != null)
        {
            Sprite target = _musicOn ? musicOnSprite : musicOffSprite;
            if (target != null) musicIcon.sprite = target;
            musicIcon.preserveAspect = true;
            musicIcon.color = _musicOn ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.9f);
        }

        // 5. Update Legacy Single Indicators
        if (soundToggleIndicator != null)
        {
            soundToggleIndicator.color = _soundOn ? onColor : offColor;
        }
        if (soundToggleText != null)
        {
            soundToggleText.text = _soundOn ? "ON" : "OFF";
        }

        if (musicToggleIndicator != null)
        {
            musicToggleIndicator.color = _musicOn ? onColor : offColor;
        }
        if (musicToggleText != null)
        {
            musicToggleText.text = _musicOn ? "ON" : "OFF";
        }
    }

    private void UpdateDualButtons(Button onBtn, Button offBtn, bool isCurrentlyOn)
    {
        if (onBtn != null)
        {
            var img = onBtn.GetComponent<Image>();
            if (img != null)
            {
                img.color = isCurrentlyOn ? Color.white : inactiveDimColor;
            }
            var txt = onBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.color = isCurrentlyOn ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.5f);
            }
            onBtn.transform.localScale = isCurrentlyOn ? new Vector3(1.05f, 1.05f, 1f) : Vector3.one;
        }

        if (offBtn != null)
        {
            var img = offBtn.GetComponent<Image>();
            if (img != null)
            {
                img.color = !isCurrentlyOn ? Color.white : inactiveDimColor;
            }
            var txt = offBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null)
            {
                txt.color = !isCurrentlyOn ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.5f);
            }
            offBtn.transform.localScale = !isCurrentlyOn ? new Vector3(1.05f, 1.05f, 1f) : Vector3.one;
        }
    }

    // ── Close ─────────────────────────────────────────────────────────────────

    public void CloseSettings()
    {
        AudioManager.Instance?.PlayButtonClick();
        var transition = GetComponent<UIPanelTransition>();
        if (transition != null)
        {
            transition.PlayClose();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    // ── Public Getters ────────────────────────────────────────────────────────
    public bool IsMusicOn => _musicOn;
    public bool IsSoundOn => _soundOn;
}
