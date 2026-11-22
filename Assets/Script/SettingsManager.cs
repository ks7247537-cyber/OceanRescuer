using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    // ── State icon Images (child SoundStateIcon / MusicStateIcon) ─────────────
    [Header("State Icon Images")]
    [SerializeField] private Image soundIconImage;  // child: SoundStateIcon
    [SerializeField] private Image musicIconImage;  // child: MusicStateIcon

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
        // Auto-find icon Images from children if not assigned in Inspector
        if (soundIconImage == null && soundButton != null)
        {
            var t = soundButton.transform.Find("SoundStateIcon");
            if (t != null) soundIconImage = t.GetComponent<Image>();
        }
        if (musicIconImage == null && musicButton != null)
        {
            var t = musicButton.transform.Find("MusicStateIcon");
            if (t != null) musicIconImage = t.GetComponent<Image>();
        }

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
        UpdateButton(musicButton, _musicOn, musicOnSprite, musicOffSprite, musicLabel, "Music");
        UpdateButton(soundButton, _soundOn, soundOnSprite, soundOffSprite, soundLabel, "Sound");
        // Swap state icon sprites immediately
        SwapIcon(soundIconImage, _soundOn, soundOnSprite, soundOffSprite);
        SwapIcon(musicIconImage, _musicOn, musicOnSprite, musicOffSprite);
    }

    // Swaps the sprite on a child state-icon Image (preserveAspect, no distortion)
    private void SwapIcon(Image iconImg, bool isOn, Sprite onSpr, Sprite offSpr)
    {
        if (iconImg == null) return;
        Sprite target = isOn ? onSpr : offSpr;
        if (target != null)
        {
            iconImg.sprite        = target;
            iconImg.preserveAspect = true;
            iconImg.enabled       = true;
        }
    }

    private void UpdateButton(Button btn, bool isOn,
        Sprite onSprite, Sprite offSprite,
        TextMeshProUGUI label, string labelName)
    {
        if (btn == null) return;

        // ── Swap icon sprite (child "Icon" Image) ────────────────────────────
        // The button background is a plain panel; the icon child holds the sprite.
        var iconTF = btn.transform.Find("Icon");
        if (iconTF != null)
        {
            var iconImg = iconTF.GetComponent<Image>();
            if (iconImg != null)
            {
                if (isOn  && onSprite  != null) iconImg.sprite = onSprite;
                if (!isOn && offSprite != null) iconImg.sprite = offSprite;
                // Color-tint the icon to show ON (white) / OFF (grey) even without sprites
                iconImg.color = isOn ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f);
            }
        }
        else
        {
            // Fallback: swap sprite on the button's own Image (old button design)
            var img = btn.GetComponent<Image>();
            if (img != null)
            {
                if (isOn  && onSprite  != null) img.sprite = onSprite;
                if (!isOn && offSprite != null) img.sprite = offSprite;
                if (onSprite == null && offSprite == null)
                    img.color = isOn ? onColor : offColor;
            }
        }

        // ── Tint button background green (ON) or red (OFF) ───────────────────
        var bgImg = btn.GetComponent<Image>();
        if (bgImg != null && bgImg.sprite == null)
        {
            // Panel-style button: tint background
            bgImg.color = isOn
                ? new Color(0.1f, 0.4f, 0.15f, 0.92f)   // dark green = ON
                : new Color(0.4f, 0.1f, 0.1f, 0.92f);    // dark red   = OFF
        }

        // ── Update label text ─────────────────────────────────────────────────
        if (label != null)
        {
            label.text = $"{labelName}: {(isOn ? "<color=#33FF66>ON</color>" : "<color=#FF4444>OFF</color>")}";
        }

        // ── Also update state indicator strip if present ──────────────────────
        var indTF = btn.transform.Find("StateIndicator");
        if (indTF != null)
        {
            var indImg = indTF.GetComponent<Image>();
            if (indImg != null)
                indImg.color = isOn ? new Color(0.2f, 0.9f, 0.4f) : new Color(0.9f, 0.2f, 0.2f);
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
