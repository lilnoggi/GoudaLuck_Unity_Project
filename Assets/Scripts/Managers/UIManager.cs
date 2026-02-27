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

    [Header("Shop Screen")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _upgradeButton;  // For controller focus

    [Header("Game Over Screen")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private GameObject _restartButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateScore(int newScore)
    {
        if (_scoreText != null)
        {
            _scoreText.text = newScore.ToString();
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
            _healthSlider.value = currentHealth;
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

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");

        Application.Quit();
    }
}
