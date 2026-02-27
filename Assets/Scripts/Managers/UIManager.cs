using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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

    [Header("Game Over Screen")]
    [SerializeField] private GameObject _gameOverPanel;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateScore(int newScore)
    {
        if (_scoreText != null)
        {
            _scoreText.text = "Cheddar Points: " + newScore;
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

    // Turns on the Game Over screen
    public void ShowGameOver()
    {
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
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
}
