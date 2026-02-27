using UnityEngine;
using UnityEngine.UI;
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
}
