using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SettingsManager — Sound and Music toggles for Ocean Rescuers.
/// Reads/writes PlayerPrefs via AudioManager.
/// Attach to SettingsPanel root GameObject.
/// 
/// PlayerPrefs keys used:
///   MusicEnabled  → 1 = on, 0 = off
///   SFXEnabled    → 1 = on, 0 = off
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // ── Button references ─────────────────────────────────────────────────────
    [Header("Toggle Buttons")]
    [SerializeField] private Button musicButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button closeButton;

    // ── Button icons / images for ON / OFF states ─────────────────────────────
    [Header("Music Button Sprites")]
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    [Header("Sound Button Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;

    // ── Status labels (optional) ──────────────────────────────────────────────
    [Header("Status Labels (optional)")]
    [SerializeField] private TextMeshProUGUI musicLabel;
    [SerializeField] private TextMeshProUGUI soundLabel;

    // ── Colors for ON / OFF ───────────────────────────────────────────────────
    [Header("Toggle Colors")]
    [SerializeField] private Color onColor  = new Color(0.2f, 0.9f, 0.4f);
    [SerializeField] private Color offColor = new Color(0.8f, 0.2f, 0.2f);

    // ── State ─────────────────────────────────────────────────────────────────
    private bool _musicOn;
    private bool _soundOn;

    // ─────────────────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        // Read current state
        _musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        _soundOn = PlayerPrefs.GetInt("SFXEnabled",   1) == 1;
        RefreshUI();
    }

    private void Start()
    {
        if (musicButton != null) musicButton.onClick.AddListener(OnMusicToggle);
        if (soundButton != null) soundButton.onClick.AddListener(OnSoundToggle);
        if (closeButton != null) closeButton.onClick.AddListener(CloseSettings);
    }

    // ── Toggle handlers ───────────────────────────────────────────────────────

    public void OnMusicToggle()
    {
        _musicOn = !_musicOn;
        // Persist
        PlayerPrefs.SetInt("MusicEnabled", _musicOn ? 1 : 0);
        PlayerPrefs.Save();
        // Apply to AudioManager
        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance.IsMusicEnabled != _musicOn)
                AudioManager.Instance.ToggleMusic();
        }
        else
        {
            // Fallback: mute AudioListener if no AudioManager
            AudioListener.volume = (_musicOn || _soundOn) ? 1f : 0f;
        }
        RefreshUI();
        AudioManager.Instance?.PlayButtonClick();
        Debug.Log($"[SettingsManager] Music: {(_musicOn ? "ON" : "OFF")}");
    }

    public void OnSoundToggle()
    {
        _soundOn = !_soundOn;
        PlayerPrefs.SetInt("SFXEnabled", _soundOn ? 1 : 0);
        PlayerPrefs.Save();
        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance.IsSFXEnabled != _soundOn)
                AudioManager.Instance.ToggleSFX();
        }
        else
        {
            AudioListener.volume = (_musicOn || _soundOn) ? 1f : 0f;
        }
        RefreshUI();
        Debug.Log($"[SettingsManager] Sound: {(_soundOn ? "ON" : "OFF")}");
    }

    // ── UI refresh ────────────────────────────────────────────────────────────

    private void RefreshUI()
    {
        UpdateButton(musicButton, _musicOn, musicOnSprite, musicOffSprite,  musicLabel, "Music");
        UpdateButton(soundButton, _soundOn, soundOnSprite, soundOffSprite,  soundLabel, "Sound");
    }

    private void UpdateButton(Button btn, bool isOn,
        Sprite onSprite, Sprite offSprite,
        TextMeshProUGUI label, string labelName)
    {
        if (btn == null) return;

        // Swap sprite
        var img = btn.GetComponent<Image>();
        if (img != null)
        {
            if (isOn  && onSprite  != null) img.sprite = onSprite;
            if (!isOn && offSprite != null) img.sprite = offSprite;
            // If no sprites, use color tint
            if (onSprite == null && offSprite == null)
                img.color = isOn ? onColor : offColor;
        }

        // Update label
        if (label != null)
        {
            label.text  = $"{labelName}: {(isOn ? "<color=#33FF66>ON</color>" : "<color=#FF4444>OFF</color>")}";
        }
    }

    // ── Close ─────────────────────────────────────────────────────────────────

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }

    // ── Public getters (for UI queries) ──────────────────────────────────────
    public bool IsMusicOn => _musicOn;
    public bool IsSoundOn => _soundOn;
}
