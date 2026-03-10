using UnityEngine;

/// <summary>
/// A modular powerup script. Uses an enum to determine what effect to apply
/// to the player upon collision.
/// </summary>

public class PowerupPickup : MonoBehaviour
{
    // The dropdown list of possible powerups
    public enum PowerupType { Health, UnlimitedAmmo, MassiveDamage, GoldenGun }

    [Header("Powerup Settings")]
    [SerializeField] private PowerupType _type;

    [Header("Specific Settings")]
    [SerializeField] private float _healthAmount = 25f;  // Only used if type is Health
    [SerializeField] private float _buffDuration = 5f;   // How long timed buffs last

    private void OnEnable()
    {
        // Powerups disappear if left alone
        Invoke("Despawn", 15f);
    }

    private void OnDisable()
    {
        // Cancel invokes when disabled to prevent memory leaks
        CancelInvoke("Despawn");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the player can pick this up
        if (other.CompareTag("Player"))
        {
            ApplyPowerup(other.gameObject);

            AudioManager.Instance.PlayPowerupPickupSound();

            Despawn();
        }
    }

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
                // Build later
                break;

            case PowerupType.GoldenGun:
                // Build later
                break;
        }
    }

    private void Despawn()
    {
        if (PowerupPool.Instance != null)
        {
            PowerupPool.Instance.ReturnPowerup(gameObject);
        }
        else
        {
            Destroy(gameObject);  // Fallback
        }
    }
}
