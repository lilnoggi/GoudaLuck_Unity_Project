using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// A centralised Singleton to handle all music and sound effects
/// </summary>

public class AudioManager : MonoBehaviour
{
   public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Music Tracks")]
    public AudioClip BackgroundMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip _playerDamageSound;
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _catMeowSound;
    [SerializeField] private AudioClip _powerupSound;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Transitioning between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Start the arcade music immediately
        if (BackgroundMusic != null)
        {
            PlayMusic(BackgroundMusic);
        }
    }

    // --- MUSIC ---
    public void PlayMusic(AudioClip clip)
    {
        if (_musicSource.clip == clip) return;  // Don't restart the track if already playing

        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();
    }

    // --- SOUND EFFECTS ---
    // Core method that plays the sound
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip, volume);
        }
    }

    // --- HELPER METHODS FOR OTHER SCRIPTS ---
    public void PlayShootSound() => PlaySFX(_shootSound);
    public void PlayMeowSound() => PlaySFX(_catMeowSound);
    public void PlayPlayerDamageSound() => PlaySFX(_playerDamageSound);
    public void PlayPowerupPickupSound() => PlaySFX(_powerupSound);
}
