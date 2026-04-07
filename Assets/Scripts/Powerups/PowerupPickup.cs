using System.Collections;
using UnityEngine;

/// <summary>
/// A modular powerup component utilising an enum-based switch to apply specific buffs.
/// Integrates with Object Pooling to manage memory efficiently and uses Coroutines
/// for type-safe despawn timers, strictly adhering to project architecture standards.
/// </summary>
public class PowerupPickup : MonoBehaviour
{
    // The dropdown list of possible powerups
    public enum PowerupType { Health, UnlimitedAmmo, MassiveDamage, GoldenGun }

    [Header("Powerup Settings")]
    [Tooltip("Defines the specific type of buff applied to the player upon pickup.")]
    [SerializeField] private PowerupType _type;

    [Header("Specific Settings")]
    [Tooltip("The amount of health restored (Only used if type is Health)")]
    [SerializeField] private float _healthAmount = 25f;
    [Tooltip("The duration in seconds the buff remains active (For timed buffs).")]
    [SerializeField] private float _buffDuration = 5f;

    // --- STATE TRACKING ---
    private Coroutine _despawnCoroutine;

    private void OnEnable()
    {
        // ARCHITECTURE FIX: Replaced brittle string-based Invoke with a type-safe Coroutine.
        // Powerups will automatically return to the pool if ignored by the player.
        _despawnCoroutine = StartCoroutine(DespawnRoutine(15f));
    }

    private void OnDisable()
    {
        // DEFENSIVE PROGRAMMING: Stop the coroutine to prevent memory leaks
        // when the object is deactivated or returned to the pool early.
        if (_despawnCoroutine != null)
        {
            StopCoroutine(_despawnCoroutine);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Physics filtering: Only the player entity can trigger the buff
        if (other.CompareTag("Player"))
        {
            ApplyPowerup(other.gameObject);

            AudioManager.Instance.PlayPowerupPickupSound();

            Despawn();
        }
    }

    /// <summary>
    /// Evaluates the assigned enum and delegates the mathematical modifier
    /// to the appropriate decoupled system.
    /// </summary>
    private void ApplyPowerup(GameObject player)
    {
        // Check the dropdown menu to know which one to do
        switch (_type)
        {
            case PowerupType.Health:
                
                HealthSystem health = player.GetComponent<HealthSystem>();
                
                if (health != null)
                {
                    health.Heal(_healthAmount);
                }
                
                break;

            case PowerupType.UnlimitedAmmo:
               
                WeaponSystem weapon = player.GetComponent<WeaponSystem>();
                
                if (weapon != null)
                {
                    weapon.ActivateUnlimitedAmmo(_buffDuration);
                }
                
                break;

            case PowerupType.MassiveDamage:
                // Future Expansion
                break;

            case PowerupType.GoldenGun:
                // Future Expansion
                break;
        }
    }

    /// <summary>
    /// Asynchronous timer handling the automatic cleanup of ignored powerups.
    /// </summary>
    private IEnumerator DespawnRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Despawn();
    }

    /// <summary>
    /// Removes the powerup from the active game board, prioritising Object Pooling
    /// over Garbage Collection to maintain stable framerates.
    /// </summary>
    private void Despawn()
    {
        if (PowerupPool.Instance != null)
        {
            PowerupPool.Instance.ReturnPowerup(gameObject);
        }
        else
        {
            Destroy(gameObject);  // Fallback for isolated testing
        }
    }
}
