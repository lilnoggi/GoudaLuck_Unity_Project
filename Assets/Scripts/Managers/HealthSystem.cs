using System;  // Required for Actions
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A highly modular component governing health, damage calculations, and invincibility frames.
/// Utilises the Observer Pattern (C# Actions) to decouple death logic, ensuring this script
/// strictly adheres to the Single Responsibility Principle. The HealthSystem "shouts" when an entity dies, allowing
/// other scripts (e.g., EnemyController, PlayerController) to "listen" and react accordingly without tight coupling.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Core Stats")]
    [Tooltip("The maximum health capacity for this entity.")]
    [SerializeField] private float _maxHealth = 100f;  // Default of 100, can be overridden in Inspector
    private float _currentHealth;

    [Header("Local UI (Enemy)")]
    [Tooltip("Optional: A World Space Canvas slider attatched to enemies or props.")]
    [SerializeField] private Slider _localHealthSlider;  // Drag the enemy's world space slider here

    // --- STATE TRACKING ---
    private bool _isInvincible;
    private float _armourResistance = 0;  // 0 = take full damage, 0.10 = take 10% less damage

    // --- OBSERVER PATTERN ---
    // A public event that external scripts (Listeners) can subscribe to without hard coupling.
    public event Action OnDeath;

    // =================================================================================================================

    /// <summary>
    /// OBJECT POOLING SUPPORT: Use OnEnable instead of Start so health resets
    /// every time the entity is pulled from the inactive queue.
    /// </summary>
    private void OnEnable()
    {
        // Everyone starts with full health
        _currentHealth = _maxHealth;

        UpdateHealthUI();  // Update UI immediately when spawned
    }

    /// <summary>
    /// Toggles invincibility frames (i-frames). 
    /// Called in PlayerController in the Coroutine DashRoutine().
    /// </summary>
    public void SetInvincible(bool state)
    {
        _isInvincible = state;
    }

    /// <summary>
    /// Calculates incoming damage, applies armour reductions, and evaluates death states.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        // DEFENSIVE PROGRAMMING: Bypass calculations if currently invincible
        if (_isInvincible) return;

        // Apply armour reduction mathematics
        _currentHealth -= damageAmount * (1f - _armourResistance);

        // --- AUDIO FEEDBACK ---
        if (gameObject.CompareTag("Player"))
        {
            AudioManager.Instance.PlayPlayerDamageSound();
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            // Otherwise, play the meow
            AudioManager.Instance.PlayMeowSound();
        }

        // Clamp to prevent negative health values and ensure UI consistency
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        // Tell UIManager to update the health slider
        UpdateHealthUI();

        if (_currentHealth <= 0)
        {
            // OBSERVER PATTERN: Invoke the event to notify all subsribed listeners.
            // The null-conditional operator (?.) ensures no errors are thrown if zero scripts are listening.
            OnDeath?.Invoke();
        }
    }



    /// <summary>
    /// Restores health, usually triggered by external powerup interactions.
    /// </summary>
    public void Heal(float healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        UpdateHealthUI();
    }

    // --- PRESENTATION LAYER ---
    private void UpdateHealthUI()
    {
        // Global UI Update (Player Only)
        if (gameObject.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealth(_currentHealth, _maxHealth);
        }

        // Local UI Update (Enemies / Props)
        if (_localHealthSlider != null)
        {
            _localHealthSlider.maxValue = _maxHealth;
            _localHealthSlider.value = _currentHealth;
        }
    }

    // --- UPGRADE MODIFIERS ---
    public void IncreaseMaxHealth(float amount)
    {
        _maxHealth += amount;
        _currentHealth += amount;  // Automatically heal the player by the newly added amount
        UpdateHealthUI();
    }

    public void AddArmour(float amount)
    {
        // MATHEMATICAL SAFETY: Clamp at 0.9f (90% reduction) to prevent mathematical invincibility
        // or accidental healing if resistance exceeds 1.0f.
        _armourResistance = Mathf.Clamp(_armourResistance + amount, 0f, 0.9f);
    }
}

// ===========================================================================================================

/// <summary>
/// A custom data structure to hold a powerup prefab and its spawn chance.
/// Kept serialisable to allow configuration via the Unity Inspector.
/// </summary>
[System.Serializable]
public struct LootDrop
{
    [Tooltip("The powerup object to be instantiated upon death.")]
    public GameObject Prefab;

    [Tooltip("The weighted probability of this item dropping relative to other items in the loot table. Must be between 0 and 100.")]
    [Range(0f, 100f)]
    public float DropChance;
}
