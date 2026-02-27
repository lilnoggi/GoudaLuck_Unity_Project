using UnityEngine;

/// <summary>
/// A modular health component that can be attatched to the Player,
/// Enemies, or even breakable props.
/// </summary>

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    void Start()
    {
        // Everyone starts with full health
        _currentHealth = _maxHealth;
    }

    // The CheeseProjectile will call this method when it hits
    public void TakeDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage! Health remaining: " + _currentHealth);

        // Clamp health so it doesn't go into negative numbers
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " has been defeated!");

        if (gameObject.CompareTag("Player"))
        {
            // Trigger the Game Over screen here
            Debug.Log("GAME OVER! The cats win.");
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            // Add Cheddar Points to the GameManager here
            // GameManager.Instance.AddScore(10);

            // Remove the dead cat
            Destroy(gameObject);
        }
    }
}
