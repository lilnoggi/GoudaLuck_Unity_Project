using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;  // Required for Controller UI Navigation
using TMPro;

/// <summary>
/// A centralised Presentation Layer controller managing the Heads Up Display (HUD) and menu states.
/// Handles dynamic hardware abstraction (mouse vs. gamepad) and utilises smoothed UI interpolation
/// for high-quality "Game Feel".
/// </summary>

public class UIManager : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Image _dashWheelImage;
    [SerializeField] private Image _ultFillImage;

    [Header("Smooth UI Fill")]
    [Tooltip("The interpolation speed for health and ultimate bars to catch up to their target values.")]
    [SerializeField] private float _fillSpeed = 5f;  // How fast the bar catches up
    private float _targetHealth;

    [Header("Weapon UI")]
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private Image _weaponIconDisplay;

    [Header("Player Upgrades Screen")]
    [SerializeField] private GameObject _playerUpgradesPanel;

    [Header("Shop Screen")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _upgradeButton;  // For controller focus
    [SerializeField] private TextMeshProUGUI _shopScoreText;

    [Header("Pause Screen")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _resumeButton;  // For controller focus
    private bool _isPaused = false;

    [Header("Settings UI")]
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _audioSettingsButton;

    [Header("Audio Settings")]
    [SerializeField] private GameObject _audioSettingsPanel;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Input Controls")]
    [SerializeField] private GameObject _controlsPanel;
    [SerializeField] private GameObject _gamepadPanel;
    [SerializeField] private GameObject _keyboardMousePanel;
    [SerializeField] private GameObject _controlsBackButton;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _restartButton;

    // --- COMPONENT CACHING ---
    // Caching the player reference prevents severe CPU spikes caused by
    // executing FindObject operations inside the Update loop.
    private PlayerController _cachedPlayer;

    // ==============================================================================================================

    private void Awake()
    {
        // Enforce the Singleton Pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Synchronise visual UI sliders with persistent saved data
        if (_musicSlider != null)
        {
            _musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.5f);
        }
    }

    public void Update()
    {
        // MEMORY OPTIMISATION: Attempt to cache the player once, bypassing expensive frame-by-frame searches
        if (_cachedPlayer == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _cachedPlayer = playerObj.GetComponent<PlayerController>();
            }
        }

        if (_cachedPlayer != null)
        {
            // Update Dash Wheel (Direct Ratio
            if (_dashWheelImage != null)
            {
                _dashWheelImage.fillAmount = _cachedPlayer.DashCooldownRatio;
            }

            // Update Ultimate Slider (Smoothed Interpolation)
            if (_ultFillImage != null)
            {
                // The slider goes from 0 to 1
                _ultFillImage.fillAmount = Mathf.Lerp(_ultFillImage.fillAmount, _cachedPlayer.UltChargeRatio, Time.deltaTime * _fillSpeed);
            }
        }

        // --- SMOOTH HEALTH BAR ---
        if (_healthSlider != null)
        {
            // Smoothly interpolate the visual slider value towards the absolute target health
            _healthSlider.value = Mathf.Lerp(_healthSlider.value, _targetHealth, Time.deltaTime * _fillSpeed);
        }
    }

    // ==========================================================================================================
    // ============================== --- MENU TOGGLES & HARDWARE ABSTRACTION ---  ==============================
    // ==========================================================================================================

    public void TogglePause()
    {
        // DEFENSIVE PROGRAMMING: Prevent pause overlaps if critical menus are already open
        if ((_playerUpgradesPanel != null && _playerUpgradesPanel.activeSelf) || 
            (_shopPanel != null && _shopPanel.activeSelf) || 
            (_gameOverPanel != null && _gameOverPanel.activeSelf)) return;

        _isPaused = !_isPaused;  // Flip the true/false switch

        if (_isPaused)
        {
            Time.timeScale = 0;  // Freeze the game

            // Turn on the main pause menu
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(true);
            }

            // --- STEAM DECK / CONTROLLER SUPPORT ---
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse)
            {
                EventSystem.current.SetSelectedGameObject(_resumeButton);
            } 
        }
        else
        {
            Time.timeScale = 1f;  // Resume game

            // Sweep all menus off the screen
            if (_pausePanel != null) _pausePanel.SetActive(false);
            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_audioSettingsPanel != null) _audioSettingsPanel.SetActive(false);
            if (_controlsPanel != null) _controlsPanel.SetActive(false);
            if (_shopPanel != null) _shopPanel.SetActive(false);
            if (_playerUpgradesPanel != null) _playerUpgradesPanel.SetActive(false);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse)
            {
                Cursor.visible = false;
            }
        }
    }

    // --- SETTINGS PANEL ---
    public void OpenSettings()
    {
        if (_settingsPanel != null)
        {
            _pausePanel.SetActive(false);
            _settingsPanel.SetActive(true);

            // --- CONTROLLER SUPPORT ---
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _audioSettingsButton != null)
            {
                EventSystem.current.SetSelectedGameObject(_audioSettingsButton);
            }
        }
    }

    // Return to pause panel
    public void CloseSettingsPanel()
    {
        if (_settingsPanel != null)
        {
            _settingsPanel.SetActive(false);
            _pausePanel.SetActive(true);

            // --- RE-FOCUS THE CONTROLLER ---
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _resumeButton != null)
            {
                // Force the controller to grab the Resume button again
                EventSystem.current.SetSelectedGameObject(_resumeButton);
            }
        }
    }

    // --- AUDIO SETTINGS PANEL ---
    // From settings to audio settings
    public void OpenAudioSettingsPanel()
    {
        if (_audioSettingsPanel != null)
        {
            _settingsPanel.SetActive(false);
            _audioSettingsPanel.SetActive(true);

            // --- CONTROLLER SUPPORT ---
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _musicSlider != null)
            {
                // Force the controller to grab the Music slider first
                EventSystem.current.SetSelectedGameObject(_musicSlider.gameObject);
            }
        }
    }

    // Return to settings panel
    public void CloseAudioSettingsPanel()
    {
        if (_audioSettingsPanel != null)
        {
            _audioSettingsPanel.SetActive(false);
            _settingsPanel.SetActive(true);

            // --- RE-FOCUS THE CONTROLLER ---
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _audioSettingsButton != null)
            {
                // Force the controller to grab the Audiobutton again
                EventSystem.current.SetSelectedGameObject(_audioSettingsButton);
            }
        }
    }

    // --- INPUT CONTROLS PANEL ---
    // From settings to Controls
    public void OpenControlsPanel()
    {
        if (_controlsPanel != null && _gamepadPanel != null)
        {
            _settingsPanel.SetActive(false);
            _controlsPanel.SetActive(true);
            _gamepadPanel.SetActive(true);  // Default

            // --- CONTROLLER SUPPORT ---
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _controlsBackButton != null)
            {
                // Force the controller to grab the Back Button first
                EventSystem.current.SetSelectedGameObject(_controlsBackButton);
            }
        }
    }

    // --- Called by the keyboard button ---
    public void SwitchToKeyboardPanel()
    {
        if (_controlsPanel != null && _keyboardMousePanel != null)
        {
            _gamepadPanel.SetActive(false);
            _controlsPanel.SetActive(true);
            _keyboardMousePanel.SetActive(true);  // Switch to keyboard mouse

            // --- CONTROLLER SUPPORT ---
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _controlsBackButton != null)
            {
                // Force the controller to grab the Back Button first
                EventSystem.current.SetSelectedGameObject(_controlsBackButton);
            }
        }
    }

    // --- Called by the gamepad button
    public void SwitchToGamepadPanel()
    {
        if (_controlsPanel != null && _gamepadPanel != null)
        {
            _keyboardMousePanel.SetActive(false);
            _controlsPanel.SetActive(true);
            _gamepadPanel.SetActive(true);  // Switch back to gamepad

            // --- CONTROLLER SUPPORT ---
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _controlsBackButton != null)
            {
                // Force the controller to grab the Back Button first
                EventSystem.current.SetSelectedGameObject(_controlsBackButton);
            }
        }
    }

    // Return to settings panel
    public void CloseControlsPanel()
    {
        if (_controlsPanel != null)
        {
            _controlsPanel.SetActive(false);
            _settingsPanel.SetActive(true);

            // --- RE-FOCUS THE CONTROLLER ---
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _audioSettingsButton != null)
            {
                // Force the controller to grab the Audiobutton again
                EventSystem.current.SetSelectedGameObject(_audioSettingsButton);
            }
        }
    }

    // Called by the resume button
    public void Resume()
    {
        if (_isPaused)
        {
            AudioManager.Instance.PlaySelectButtonSound();
            TogglePause();
        }
    }

    // =========================================================================================================
    // ====================================== --- HUD DATA INJECTION ---  ======================================
    // =========================================================================================================

    public void UpdateAmmo(int currentAmmo, int maxAmmo)
    {
        if (_ammoText != null) _ammoText.text = currentAmmo + " / " + maxAmmo;
    }

    public void UpdateAmmoText(string message)
    {
        if (_ammoText != null) _ammoText.text = message;
    }

    public void UpdateWeaponUI(Sprite weaponSprite)
    {
        if (_weaponIconDisplay != null)
        {
            _weaponIconDisplay.sprite = weaponSprite;

            // Ensure it has full opacity / is turned on
            _weaponIconDisplay.color = Color.white;
        }
    }

    public void UpdateScore(int newScore)
    {
        if (_scoreText != null)
        {
            _scoreText.text = newScore.ToString();
        }

        if (_shopScoreText != null)
        {
            _shopScoreText.text = newScore.ToString();
        }
    }

    public void UpdateWave(int newWave)
    {
        if (_waveText != null)
        {
            _waveText.text = "Wave: " + newWave;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = maxHealth;
            // Record the target state; the Update loop handles the smooth visual interpolation
            _targetHealth = currentHealth;  
        }
    }

    // =========================================================================================================
    // ====================================== --- PROGRESSION SCREENS---  ======================================
    // =========================================================================================================

    // === SHOP LOGIC ===
    public void ShowShop()
    {
        if (_shopPanel != null)
        {
            _playerUpgradesPanel.SetActive(false);
            _shopPanel.SetActive(true);
            Time.timeScale = 0f;  // Pause the game

            // Controller support
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _upgradeButton != null)
            {
                EventSystem.current.SetSelectedGameObject(_upgradeButton);
            }
        }
    }

    public void HideShop()
    {
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(false);
            Time.timeScale = 1f;  // Unpause the game

            // Hide the cursor if using a gamepad
            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse)
            {
                Cursor.visible = false;
            }
        }
    }

    // === GAME OVER ===
    // Turns on the Game Over screen
    public void ShowGameOver()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);

            // --- STEAM DECK / CONTROLLER INPUT ---
            // Clear whatever the Event System might have been looking at
            EventSystem.current.SetSelectedGameObject(null);

            if (_cachedPlayer != null && !_cachedPlayer.IsUsingMouse && _restartButton != null)
            {
                EventSystem.current.SetSelectedGameObject(_restartButton);
            }
        }
    }

    // The Restart Button will call this method
    public void RestartGame()
    {
        // Unfreeze the game first
        Time.timeScale = 1f;

        AudioManager.Instance.PlaySelectButtonSound();

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        AudioManager.Instance.PlaySelectButtonSound();
        Debug.Log("Quitting the game...");

        Application.Quit();
    }
}
