using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// A persistent Singleton managing all global audio state across scene transitions.
/// Centralises AudioMixer controls, handles logarithmic volume scaling, and provides
/// globally accessible helper methods to decouple audio instantiation from gameplay scripts.
/// </summary>

public class AudioManager : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
   public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("The dedicated source for looping background music.")]
    [SerializeField] private AudioSource _musicSource;
    [Tooltip("The dedicated source for overlapping, one-shot sound effects.")]
    [SerializeField] private AudioSource _sfxSource;

    [Header("Music Tracks")]
    [Tooltip("The default track to play when this manager initialises.")]
    public AudioClip BackgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip _playerDamageSound;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _catMeowSound;
    [SerializeField] private AudioClip _powerupSound;
    [SerializeField] private AudioClip _dashSound;

    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip _purchaseGun;
    [SerializeField] private AudioClip _purchaseFailed;
    [SerializeField] private AudioClip _hoverButton;
    [SerializeField] private AudioClip _selectButton;
    [SerializeField] private AudioClip _upgradeWeapon;

    [Header("Mixer")]
    [Tooltip("The master AudioMixer dicating the project's audio routing.")]
    [SerializeField] private AudioMixer _audioMixer;

    private void Awake()
    {
        // --- SINGLETON PATTERN ---
        // Enforce strict global access and prevent duplicate managers during scene loads
        if (Instance == null)
        {
            Instance = this;
            // Persist this object across the Main Menu and Gameplay scenes
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialise the presentation layer's audio
        if (BackgroundMusic != null)
        {
            PlayMusic(BackgroundMusic);
        }

        // --- PERSISTING DATA LOADING ---
        // Retrieve saved user preferences from the local disk.
        // Defaults to 0.5f (50% volume) if no save data exists.
        float savedMusic = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVol", 0.5f);

        SetMusicVolume(savedMusic);
        SetSFXVolume(savedSFX);
    }

    // --- MUSIC PIPELINE ---
    /// <summary>
    /// Swaps the active background track. Bypasses execution if the requested track is already playing.
    /// </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (_musicSource.clip == clip) return;

        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    // --- SFX PIPELINE ---
    /// <summary>
    /// Executes a sound effect without interrupting currently playing SFX.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            // PlayOneShot allows multiple sounds (e.g., rapid gunfire) to overlap safely.
            _sfxSource.PlayOneShot(clip, volume);
        }
    }

    // --- HELPER METHODS ---
    // These encapsulated lambda methods allow external scripts to trigger audio
    // without needing direct references to the AudioClip data.
    public void PlayShootSound() => PlaySFX(_shootSound);
    public void PlayMeowSound() => PlaySFX(_catMeowSound);
    public void PlayPlayerDamageSound() => PlaySFX(_playerDamageSound);
    public void PlayPowerupPickupSound() => PlaySFX(_powerupSound);
    public void PlayDashSound() => PlaySFX(_dashSound);
    public void PlayPurchaseGunSound() => PlaySFX(_purchaseGun);
    public void PlayPurchaseFailedSound() => PlaySFX(_purchaseFailed);
    public void PlayHoverButtonSound() => PlaySFX(_hoverButton);
    public void PlaySelectButtonSound() => PlaySFX(_selectButton);
    public void PlayUpgradeWeaponSound() => PlaySFX(_upgradeWeapon);

    // --- SETTINGS LOGIC ---
    /// <summary>
    /// AUDIO MATHEMATICS: The Unity Inspector operates on a logarithmic Decibel scale (-80dB to 0dB),
    /// while UI sliders operate on a lienar scale (0.0001 to 1).
    /// // The Base-10 logarithm of the slider must be calculated to create a natural-sounding volume curve.
    /// </summary>
    public void SetMusicVolume(float sliderValue)
    {
        // Convert linear UI slider (0.0001 to 1) to logarithmic decibles (-80dB to 0dB)
        _audioMixer.SetFloat("MusicVol", Mathf.Log10(sliderValue) * 20f);

        // Save the setting to the local disk
        PlayerPrefs.SetFloat("MusicVol", sliderValue);
    }

    public void SetSFXVolume(float sliderValue)
    {
        _audioMixer.SetFloat("SFXVol", Mathf.Log10(sliderValue) * 20f);
        PlayerPrefs.SetFloat("SFXVol", sliderValue);
    }
}
