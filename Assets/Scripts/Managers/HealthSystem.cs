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

    // --- DASHING I-FRAME TRACKING ---
    private bool _isInvincible;

    [Header("Loot Drops (Enemy Only)")]
    [SerializeField] private GameObject[] _powerupPrefabs;  // Array of possible drops
    [SerializeField] private float _dropChance = 0.1f;      // 10% chance to drop

    // Instead of Start() use OnEnable() to reset health every time it spawns from the pool
    private void OnEnable()
    {
        // Everyone starts with full health
        _currentHealth = _maxHealth;

        UpdateHealthUI();  // Update UI immediately when spawned
    }

    // Method to turn invincibility on / off
    public void SetInvincible(bool state)
    {
        _isInvincible = state;
    }

    // The CheeseProjectile will call this method when it hits
    public void TakeDamage(float damageAmount)
    {
        // If invisible, ignore the rest of the method
        if (_isInvincible) return;

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

    // Called in PowerupPickup.cs
    public void Heal(float healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        UpdateHealthUI();
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

                // --- LOOT DROP LOGIC ---
                if (_powerupPrefabs != null && _powerupPrefabs.Length > 0)
                {
                    // Roll and random number between 0.0 and 1.0
                    if (Random.value <= _dropChance)
                    {
                        // Pick a random powerup from the array
                        GameObject drop = _powerupPrefabs[Random.Range(0, _powerupPrefabs.Length)];

                        // Spawn it slightly above the ground so it doesn't clip
                        Vector3 dropPos = transform.position + new Vector3(0f, 0.5f, 0f);
                        Instantiate(drop, dropPos, Quaternion.identity);
                    }
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
