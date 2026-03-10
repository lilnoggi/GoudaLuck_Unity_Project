using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;  // Required for Controller UI Navigation
using TMPro;

/// <summary>
/// Manages the Heads Up Display (HUD) and updates the screen when
/// the player takes damage or score points.
/// </summary>

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _waveText;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Image _dashWheelImage;
    [SerializeField] private Image _ultFillImage;

    [Header("Smooth UI Fill")]
    [SerializeField] private float _fillSpeed = 5f;  // How fast the bar catches up
    private float _targetHealth;

    [Header("Weapon UI")]
    [SerializeField] private TextMeshProUGUI _ammoText;
    [SerializeField] private Image _weaponIconDisplay;

    [Header("Shop Screen")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _upgradeButton;  // For controller focus
    [SerializeField] private TextMeshProUGUI _shopScoreText;

    [Header("Pause Screen")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _resumeButton;  // For controller focus
    private bool _isPaused = false;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _restartButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Update()
    {
        // Find the player once to save performance
        PlayerController player = null;

        // Only update the wheel if it is actually assigned in the Inspector
        if(_dashWheelImage != null || _ultFillImage != null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player != null)
        {
            // Update Dash Wheel
            if (_dashWheelImage != null)
            {
                _dashWheelImage.fillAmount = player.DashCooldownRatio;
            }

            // Update Ultimate Slider
            if (_ultFillImage != null)
            {
                // The slider goes from 0 to 1
                _ultFillImage.fillAmount = Mathf.Lerp(_ultFillImage.fillAmount, player.UltChargeRatio, Time.deltaTime * _fillSpeed);
            }
        }

        // --- SMOOTH HEALTH BAR ---
        if (_healthSlider != null)
        {
            // Smoothly move the visual slider value towards the target health
            _healthSlider.value = Mathf.Lerp(_healthSlider.value, _targetHealth, Time.deltaTime * _fillSpeed);
        }
    }

    public void TogglePause()
    {
        // SAFETY: Don't pause if the Shop or Game Over screens are currently open
        if ((_shopPanel != null && _shopPanel.activeSelf) || (_gameOverPanel != null && _gameOverPanel.activeSelf)) return;

        _isPaused = !_isPaused;  // Flip the true/false switch

        if (_pausePanel != null)
        {
            _pausePanel.SetActive(_isPaused);
        }

        if (_isPaused)
        {
            Time.timeScale = 0;  // Freeze the game

            // Controller Support
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && !player.IsUsingMouse)
            {
                EventSystem.current.SetSelectedGameObject(_resumeButton);
            } 
        }
        else
        {
            Time.timeScale = 1f;  // Unfreeze game

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && !player.IsUsingMouse)
            {
                Cursor.visible = false;
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
            _targetHealth = currentHealth;  // Tell the UI what number to chase
        }
    }

    // === SHOP LOGIC ===
    public void ShowShop()
    {
        if (_shopPanel != null)
        {
            _shopPanel.SetActive(true);
            Time.timeScale = 0f;  // Pause the game

            // Controller support
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && !player.IsUsingMouse)
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
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && !player.IsUsingMouse)
            {
                Cursor.visible = true;
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
            // Force the cursor to be visible in case they want to use the mouse to click
            Cursor.visible = true;

            // Clear whatever the Event System might have been looking at
            EventSystem.current.SetSelectedGameObject(null);

            // Find the Player in the scene and check what device they are using
            PlayerController player = FindFirstObjectByType<PlayerController>();

            // If we found the player, AND they are NOT using the mouse
            if (player != null && !player.IsUsingMouse)
            {
                // Force the controller to focus directly onto the Restart Button
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
