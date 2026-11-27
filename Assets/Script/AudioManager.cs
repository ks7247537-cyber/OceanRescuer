using UnityEngine;

/// <summary>
/// Audio Manager — BGM + SFX with singleton pattern.
/// DontDestroyOnLoad — persists across all scenes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<AudioManager>();
                if (_instance == null)
                {
                    var go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    [Header("SFX Clips")]
    public AudioClip pipeRotateClip;
    public AudioClip winClip;
    public AudioClip failClip;
    public AudioClip buttonClickClip;
    public AudioClip waterFlowClip;

    private bool _musicEnabled = true;
    private bool _sfxEnabled   = true;
    private float _lastSfxTime = 0f;
    private AudioClip _lastSfxClip = null;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop        = true;
        bgmSource.playOnAwake = false;
        sfxSource.loop        = false;
        sfxSource.playOnAwake = false;

        _musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        _sfxEnabled   = PlayerPrefs.GetInt("SFXEnabled",   1) == 1;
        bgmSource.volume = _musicEnabled ? PlayerPrefs.GetFloat("MusicVolume", 0.7f) : 0f;
        sfxSource.volume = _sfxEnabled   ? PlayerPrefs.GetFloat("SFXVolume",   1.0f) : 0f;

        EnsureClipsLoaded();
    }

    /// <summary>
    /// Guarantees that all 7 required audio clips are loaded into memory from Resources/Audio.
    /// Safely handles missing clips without throwing exceptions.
    /// </summary>
    public void EnsureClipsLoaded()
    {
        if (menuMusic == null) menuMusic = Resources.Load<AudioClip>("Audio/bgm_menu");
        if (gameplayMusic == null) gameplayMusic = Resources.Load<AudioClip>("Audio/bgm_gameplay");
        if (pipeRotateClip == null) pipeRotateClip = Resources.Load<AudioClip>("Audio/sfx_pipe_rotate");
        if (winClip == null) winClip = Resources.Load<AudioClip>("Audio/sfx_win");
        if (failClip == null) failClip = Resources.Load<AudioClip>("Audio/sfx_fail");
        if (buttonClickClip == null) buttonClickClip = Resources.Load<AudioClip>("Audio/sfx_button_click");
        if (waterFlowClip == null) waterFlowClip = Resources.Load<AudioClip>("Audio/sfx_water_flow");
    }

    public void PlayBGM(AudioClip clip)
    {
        EnsureClipsLoaded();
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        if (_musicEnabled) bgmSource.Play();
    }

    public void PlayMenuMusic()     => PlayBGM(menuMusic);
    public void PlayGameplayMusic() => PlayBGM(gameplayMusic);
    public void StopBGM()           => bgmSource.Stop();

    // Overload accepting clip name string
    public void PlayBGM(string clipName)
    {
        EnsureClipsLoaded();
        if (clipName == "MainMenuBGM" || clipName == "LevelSelectBGM")  PlayBGM(menuMusic);
        else if (clipName == "GameplayBGM") PlayBGM(gameplayMusic);
    }

    public void PlaySFX(AudioClip clip)
    {
        EnsureClipsLoaded();
        if (clip == null || !_sfxEnabled) return;

        // Prevent identical SFX retrigger within 50ms to stop machine-gun audio stutter
        if (clip == _lastSfxClip && (Time.unscaledTime - _lastSfxTime) < 0.05f) return;
        _lastSfxClip = clip;
        _lastSfxTime = Time.unscaledTime;

        sfxSource.PlayOneShot(clip);
    }

    public void PlayPipeRotate()   => PlaySFX(pipeRotateClip);
    public void PlayWin()          => PlaySFX(winClip);
    public void PlayFail()         => PlaySFX(failClip);
    public void PlayButtonClick()  => PlaySFX(buttonClickClip);
    public void PlayWaterFlow()    => PlaySFX(waterFlowClip);
    public void PlayRockSmash()    => PlaySFX(buttonClickClip != null ? buttonClickClip : pipeRotateClip);
    public void PlayShock()        => PlaySFX(waterFlowClip != null ? waterFlowClip : pipeRotateClip);

    public void SetMusicVolume(float v) { bgmSource.volume = v; PlayerPrefs.SetFloat("MusicVolume", v); PlayerPrefs.Save(); }
    public void SetSFXVolume(float v)   { sfxSource.volume = v; PlayerPrefs.SetFloat("SFXVolume",   v); PlayerPrefs.Save(); }

    public void SetMusicEnabled(bool enable)
    {
        _musicEnabled = enable;
        bgmSource.volume = _musicEnabled ? PlayerPrefs.GetFloat("MusicVolume", 0.7f) : 0f;
        if (_musicEnabled && !bgmSource.isPlaying && bgmSource.clip != null) bgmSource.Play();
        else if (!_musicEnabled) bgmSource.Pause();
        PlayerPrefs.SetInt("MusicEnabled", _musicEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetSFXEnabled(bool enable)
    {
        _sfxEnabled = enable;
        sfxSource.volume = _sfxEnabled ? PlayerPrefs.GetFloat("SFXVolume", 1.0f) : 0f;
        PlayerPrefs.SetInt("SFXEnabled", _sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleMusic()
    {
        SetMusicEnabled(!_musicEnabled);
    }

    public void ToggleSFX()
    {
        SetSFXEnabled(!_sfxEnabled);
    }

    public bool IsMusicEnabled => _musicEnabled;
    public bool IsSFXEnabled   => _sfxEnabled;
}
