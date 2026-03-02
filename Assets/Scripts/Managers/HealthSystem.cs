using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A modular health component that can be attatched to the Player,
/// Enemies, or even breakable props.
/// </summary>

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    [Header("Local UI (Enemy)")]
    [SerializeField] private Slider _localHealthSlider;  // Drag the enemy's world space slider here

    // Instead of Start() use OnEnable() to reset health every time it spawns from the pool
    private void OnEnable()
    {
        // Everyone starts with full health
        _currentHealth = _maxHealth;

        UpdateHealthUI();  // Update UI immediately when spawned
    }

    // The CheeseProjectile will call this method when it hits
    public void TakeDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;

        // Clamp health so it doesn't go into negative numbers
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        // Tell UIManager to update the health slider
        UpdateHealthUI();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    // --- HELPER METHOD ---
    private void UpdateHealthUI()
    {
        // Update the Global screen UI if this is the player
        if (gameObject.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(_currentHealth, _maxHealth);
        }

        // Update the local floating UI if this object has one (the enemies)
        if (_localHealthSlider != null)
        {
            _localHealthSlider.maxValue = _maxHealth;
            _localHealthSlider.value = _currentHealth;
        }
    }

    private void Die()
    {
        if (gameObject.CompareTag("Player"))
        {
            // Tell the GameManager the player is dead
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            // Add Cheddar Points to the GameManager here
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(10);

                // Tell WaveManager when a cat died
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.EnemyDefeated();
                }

                // Return to pool instead of destroying
                if (EnemyPool.Instance != null)
                {
                    EnemyPool.Instance.ReturnEnemy(gameObject);
                }
                else
                {
                    // FALLBACK :Remove the dead cat
                    Destroy(gameObject);
                }
            }
        }
    }
}
