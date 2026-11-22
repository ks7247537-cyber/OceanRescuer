using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SettingsToggle — Sound aur Music toggle ke liye simple, self-contained script.
///
/// HOW TO USE:
///   1. MainMenuScene mein kisi bhi GameObject pe attach karo (e.g. MainMenuManager)
///   2. Inspector mein SoundButton, MusicButton aur CloseButton assign karo
///   3. Optionally SoundIcon aur MusicIcon Images assign karo
///   4. SoundOnSprite = button_sound.png, SoundOffSprite = button_mute.png set karo
///   5. Play karo — bas!
///
/// Script automatically:
///   - PlayerPrefs se ON/OFF state load karti hai
///   - Button click pe toggle karti hai
///   - AudioManager ko notify karti hai
///   - Icons immediately swap karti hai
///   - State save karti hai
/// </summary>
public class SettingsToggle : MonoBehaviour
{
    // ── Inspector Fields ───────────────────────────────────────────────────────

    [Header("=== Buttons (SettingsPanel ke andar) ===")]
    [Tooltip("SoundButton GameObject ka Button component")]
    public Button soundButton;

    [Tooltip("MusicButton GameObject ka Button component")]
    public Button musicButton;

    [Tooltip("CloseSettBtn ka Button component")]
    public Button closeButton;

    [Header("=== Icon Images (optional — right side of buttons) ===")]
    [Tooltip("SoundStateIcon Image component")]
    public Image soundIconImage;

    [Tooltip("MusicStateIcon Image component")]
    public Image musicIconImage;

    [Header("=== Sprites ===")]
    [Tooltip("Speaker ON sprite (button_sound.png)")]
    public Sprite soundOnSprite;

    [Tooltip("Mute OFF sprite (button_mute.png)")]
    public Sprite soundOffSprite;

    [Header("=== Settings Panel ===")]
    [Tooltip("SettingsPanel GameObject reference")]
    public GameObject settingsPanel;

    // ── Internal State ─────────────────────────────────────────────────────────

    private bool _soundOn;
    private bool _musicOn;

    // ── Unity Lifecycle ────────────────────────────────────────────────────────

    private void Start()
    {
        // 1. Load saved state from PlayerPrefs
        _soundOn = PlayerPrefs.GetInt("SFXEnabled",    1) == 1;
        _musicOn = PlayerPrefs.GetInt("MusicEnabled",  1) == 1;

        // 2. Auto-find references if not assigned in Inspector
        AutoFindReferences();

        // 3. Wire button click listeners
        if (soundButton != null)
        {
            soundButton.onClick.RemoveAllListeners();
            soundButton.onClick.AddListener(OnSoundClicked);
        }
        if (musicButton != null)
        {
            musicButton.onClick.RemoveAllListeners();
            musicButton.onClick.AddListener(OnMusicClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        // 4. Apply saved state to icons immediately
        RefreshIcons();

        Debug.Log($"[SettingsToggle] Ready — Sound:{(_soundOn ? "ON" : "OFF")} Music:{(_musicOn ? "ON" : "OFF")}");
    }

    // ── Button Click Handlers ──────────────────────────────────────────────────

    public void OnSoundClicked()
    {
        _soundOn = !_soundOn;
        PlayerPrefs.SetInt("SFXEnabled", _soundOn ? 1 : 0);
        PlayerPrefs.Save();

        // AudioManager ko bhi update karo
        if (AudioManager.Instance != null &&
            AudioManager.Instance.IsSFXEnabled != _soundOn)
        {
            AudioManager.Instance.ToggleSFX();
        }

        RefreshIcons();
        Debug.Log($"[SettingsToggle] Sound: {(_soundOn ? "ON" : "OFF")}");
    }

    public void OnMusicClicked()
    {
        _musicOn = !_musicOn;
        PlayerPrefs.SetInt("MusicEnabled", _musicOn ? 1 : 0);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null &&
            AudioManager.Instance.IsMusicEnabled != _musicOn)
        {
            AudioManager.Instance.ToggleMusic();
        }

        RefreshIcons();
        Debug.Log($"[SettingsToggle] Music: {(_musicOn ? "ON" : "OFF")}");
    }

    public void OnCloseClicked()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ── Icon Refresh ───────────────────────────────────────────────────────────

    private void RefreshIcons()
    {
        // Sound icon: ON = soundOnSprite, OFF = soundOffSprite
        ApplyIcon(soundIconImage, _soundOn);

        // Music icon: same sprites (music bhi speaker icon use karta hai)
        ApplyIcon(musicIconImage, _musicOn);

        // Button background color: green = ON, red = OFF
        TintButton(soundButton, _soundOn);
        TintButton(musicButton, _musicOn);
    }

    private void ApplyIcon(Image iconImg, bool isOn)
    {
        if (iconImg == null) return;

        Sprite target = isOn ? soundOnSprite : soundOffSprite;
        if (target != null)
        {
            iconImg.sprite         = target;
            iconImg.preserveAspect = true;
            iconImg.enabled        = true;
        }
        else
        {
            // Sprite nahi mili — sirf color se dikhaao
            iconImg.color = isOn
                ? new Color(0.2f, 0.9f, 0.4f)   // green = ON
                : new Color(0.9f, 0.2f, 0.2f);   // red   = OFF
        }
    }

    private void TintButton(Button btn, bool isOn)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img == null) return;

        // Transparent button hona chahiye — tint se ON/OFF dikhao
        img.color = isOn
            ? new Color(0.15f, 0.7f, 0.3f, 0.5f)   // half-transparent green
            : new Color(0.7f, 0.15f, 0.15f, 0.5f);  // half-transparent red
    }

    // ── Auto-Find References ───────────────────────────────────────────────────

    private void AutoFindReferences()
    {
        // SettingsPanel dhundho
        if (settingsPanel == null)
        {
            var go = GameObject.Find("SettingsPanel");
            if (go != null) settingsPanel = go;
        }

        if (settingsPanel == null)
        {
            Debug.LogWarning("[SettingsToggle] SettingsPanel not found in scene!");
            return;
        }

        // SoundButton
        if (soundButton == null)
        {
            var t = settingsPanel.transform.Find("SoundButton");
            if (t != null) soundButton = t.GetComponent<Button>();
        }

        // MusicButton
        if (musicButton == null)
        {
            var t = settingsPanel.transform.Find("MusicButton");
            if (t != null) musicButton = t.GetComponent<Button>();
        }

        // CloseButton
        if (closeButton == null)
        {
            var t = settingsPanel.transform.Find("CloseSettBtn");
            if (t != null) closeButton = t.GetComponent<Button>();
        }

        // SoundStateIcon (child of SoundButton)
        if (soundIconImage == null && soundButton != null)
        {
            var t = soundButton.transform.Find("SoundStateIcon");
            if (t != null) soundIconImage = t.GetComponent<Image>();
        }

        // MusicStateIcon (child of MusicButton)
        if (musicIconImage == null && musicButton != null)
        {
            var t = musicButton.transform.Find("MusicStateIcon");
            if (t != null) musicIconImage = t.GetComponent<Image>();
        }

        // Sprites load karo from Resources agar Inspector mein assign nahi hain
        if (soundOnSprite == null || soundOffSprite == null)
        {
            LoadSpritesFromResources();
        }

        // Final status log
        Debug.Log($"[SettingsToggle] Refs — Sound:{soundButton != null} Music:{musicButton != null} " +
                  $"Close:{closeButton != null} SoundIcon:{soundIconImage != null} MusicIcon:{musicIconImage != null} " +
                  $"OnSprite:{soundOnSprite != null} OffSprite:{soundOffSprite != null}");
    }

    private void LoadSpritesFromResources()
    {
        // Resources/Sprites/ folder mein dhundho
        if (soundOnSprite  == null) soundOnSprite  = Resources.Load<Sprite>("Sprites/button_sound");
        if (soundOffSprite == null) soundOffSprite = Resources.Load<Sprite>("Sprites/button_mute");
    }

    // ── OnEnable: scene reopen pe state restore ────────────────────────────────

    private void OnEnable()
    {
        _soundOn = PlayerPrefs.GetInt("SFXEnabled",   1) == 1;
        _musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        RefreshIcons();
    }
}
