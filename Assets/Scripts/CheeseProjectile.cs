using UnityEngine;

/// <summary>
/// Handles the movement and collision logic for the projectiles.
/// Fully upgraded to use Object Pooling
/// </summary>

public class CheeseProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 2f;

    [Header("VFX")]
    [SerializeField] private GameObject _imapctParticles;  // VFX Particle System
    
    private float _currentDamage;

    private string _shooterTag;  // Remember who fired this bullet

    private void OnEnable()
    {
        // Use OnEnable instead of Start() because the pool will turn this object on and off multiple times
        Invoke("Deactivate", _lifeTime);  // SAFETY NET: recycle after a few seconds
    }

    private void OnDisable()
    {
        CancelInvoke("Deactivate");  // Clean up the timer if it hits a wall early
    }

    private void Update()
    {
        // Move the bullet straight forward along its local Z-axis
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    // The WeaponSystem calls this the exact moment the bullet is spawned
    public void Setup(string tagOfShooter, float damageAmount)
    {
        _shooterTag = tagOfShooter;
        _currentDamage = damageAmount;  // Store the weapon's damage
    }

    private void Deactivate()
    {
        // Send it back to the pool instead of destroying it
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);  // Fallback just in case
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore collision of whoever fired it (Player ignores Player, Enemy ignores Enemy)
        if (other.CompareTag(_shooterTag)) return;

        // If it hits an enemy OR hits the player
        if (other.CompareTag("Enemy") && _shooterTag == "Player" || other.CompareTag("Player") && _shooterTag == "Enemy")
        {
            // Grab the HealthSystem off the bean just hit
            HealthSystem targetHealth = other.GetComponent<HealthSystem>();

            if (targetHealth != null)
            {
                // Take damage
                targetHealth.TakeDamage(_currentDamage);
            }
        }

        // Cheese Particles!
        if (_imapctParticles != null)
        {
            Instantiate(_imapctParticles, transform.position, Quaternion.identity);
        }

        // Recycle the bullet when it hits anything else
        Deactivate();
    }
}
