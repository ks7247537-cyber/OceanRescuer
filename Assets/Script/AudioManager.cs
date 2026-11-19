using UnityEngine;

/// <summary>
/// Audio Manager — BGM + SFX with singleton pattern.
/// DontDestroyOnLoad — persists across all scenes.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

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
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        if (_musicEnabled) bgmSource.Play();
    }

    public void PlayMenuMusic()     => PlayBGM(menuMusic);
    public void PlayGameplayMusic() => PlayBGM(gameplayMusic);
    public void StopBGM()           => bgmSource.Stop();

    // Overload accepting clip name string (for future Resources.Load support)
    public void PlayBGM(string clipName)
    {
        // Map known names to assigned clips
        if (clipName == "MainMenuBGM" || clipName == "LevelSelectBGM")  PlayBGM(menuMusic);
        else if (clipName == "GameplayBGM") PlayBGM(gameplayMusic);
        // If no clip found, silently ignore (no assets yet)
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || !_sfxEnabled) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayPipeRotate()   => PlaySFX(pipeRotateClip);
    public void PlayWin()          => PlaySFX(winClip);
    public void PlayFail()         => PlaySFX(failClip);
    public void PlayButtonClick()  => PlaySFX(buttonClickClip);

    public void SetMusicVolume(float v) { bgmSource.volume = v; PlayerPrefs.SetFloat("MusicVolume", v); PlayerPrefs.Save(); }
    public void SetSFXVolume(float v)   { sfxSource.volume = v; PlayerPrefs.SetFloat("SFXVolume",   v); PlayerPrefs.Save(); }

    public void ToggleMusic()
    {
        _musicEnabled = !_musicEnabled;
        bgmSource.volume = _musicEnabled ? PlayerPrefs.GetFloat("MusicVolume", 0.7f) : 0f;
        if (_musicEnabled && !bgmSource.isPlaying && bgmSource.clip != null) bgmSource.Play();
        else if (!_musicEnabled) bgmSource.Pause();
        PlayerPrefs.SetInt("MusicEnabled", _musicEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX()
    {
        _sfxEnabled = !_sfxEnabled;
        sfxSource.volume = _sfxEnabled ? PlayerPrefs.GetFloat("SFXVolume", 1.0f) : 0f;
        PlayerPrefs.SetInt("SFXEnabled", _sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool IsMusicEnabled => _musicEnabled;
    public bool IsSFXEnabled   => _sfxEnabled;
}
