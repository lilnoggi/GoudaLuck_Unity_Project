using System;  // Required for Actions
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

    // --- ACTIONS ---
    // Any other scripts can "listen" to this event
    public event Action OnDeath;

    private float _armourResistance = 0;  // 0 = take full damage, 0.10 = take 10% less damage

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

        // Implement armour upgrade logic
        _currentHealth -= damageAmount * (1f - _armourResistance);

        // Only play the player damage sound IF the player took damage
        if (gameObject.CompareTag("Player"))
        {
            AudioManager.Instance.PlayPlayerDamageSound();
        }
        else if (gameObject.CompareTag("Enemy"))
        {
            // Otherwise, play the meow
            AudioManager.Instance.PlayMeowSound();
        }

        // Clamp health so it doesn't go into negative numbers
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

        // Tell UIManager to update the health slider
        UpdateHealthUI();

        if (_currentHealth <= 0)
        {
            // "Shout" to other script that an event is happening
            // The ? means "Only shout if something is listening"
            OnDeath?.Invoke();
        }
    }



    // Called in PowerupPickup.cs
    public void Heal(float healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        UpdateHealthUI();
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

    // --- UPGRADE HELPER'S ---
    public void IncreaseMaxHealth(float amount)
    {
        _maxHealth += amount;
        _currentHealth += amount;
        UpdateHealthUI();
    }

    public void AddArmour(float amount)
    {
        _armourResistance += amount;
    }
}

/// <summary>
/// A custom data structure to hold a powerup prefab and its spawn chance.
/// </summary>

[System.Serializable]
public struct LootDrop
{
    public GameObject Prefab;

    // This creates a slider in the Inspector from 0 - 100
    [Range(0f, 100f)]
    public float DropChance;
}
