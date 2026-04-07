using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles the movement, collision logic, and lifecycle of standard projectiles.
/// Fully integrated with the ProjectilePool to prevent Garbage Collection spikes,
/// and uses type-safe Coroutines for lifecycle memory management.
/// </summary>
public class CheeseProjectile : MonoBehaviour
{
    [Header("Flight Settings")]
    [Tooltip("The forward velocity of the projectiles in units per second.")]
    [SerializeField] private float _speed = 20f;
    [Tooltip("The maximum time in seconds before the projectile is automatically recycled.")]
    [SerializeField] private float _lifeTime = 2f;

    [Header("VFX")]
    [Tooltip("The particle system instantiated upon impact.")]
    [SerializeField] private GameObject _imapctParticles;  // VFX Particle System
    
    // --- STATE TRACKING ---
    private float _currentDamage;

    // Stores the tag of the entity that fired it to prevent friendly fire
    private string _shooterTag;  // Remember who fired this bullet

    private Coroutine _lifeTimerCoroutine;

    // ==============================================================================================================

    private void OnEnable()
    {
        // ARCHITECTURE FIX: Use type-safe Coroutines instead of string-based Invokes.
        // Start the safety net timer so the bullet recycles if it flies off the map.
        _lifeTimerCoroutine = StartCoroutine(LifeTimerRoutine());
    }

    private void OnDisable()
    {
        // DEFENSIVE PROGRAMMING: Clean up the timer if the projectile hits a target early
        // to prevent null reference errors or memory leaks in the pool.
        if (_lifeTimerCoroutine != null)
        {
            StopCoroutine(_lifeTimerCoroutine);
        }
    }

    private void Update()
    {
        // Move the bullet straight forward along its local Z-axis
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    /// <summary>
    /// Inserts damage data and ownership tags into the projectile immediately
    /// after it is pulled from the Object Pool
    /// </summary>
    public void Setup(string tagOfShooter, float damageAmount)
    {
        _shooterTag = tagOfShooter;
        _currentDamage = damageAmount; 
    }

    /// <summary>
    /// Asynchronous timer ensuring missed projectiles are safely recycled
    /// </summary>
    private IEnumerator LifeTimerRoutine()
    {
        yield return new WaitForSeconds(_lifeTime);
        Deactivate();
    }

    /// <summary>
    /// Safely returns the object to the memory pool instead of destroying it.
    /// </summary>
    private void Deactivate()
    {
        // Send it back to the pool instead of destroying it
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);  // Fallback for isolated testing
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // DEFENSIVE PROGRAMMING: Ignore collisions with the entity that fired it (No friendly fire)
        if (other.CompareTag(_shooterTag)) return;

        // Evaluate valid hit states (Player hitting Enemy OR Enemy hitting Player)
        if (other.CompareTag("Enemy") && _shooterTag == "Player" || other.CompareTag("Player") && _shooterTag == "Enemy")
        {
            // Delegate damage execution strictly to the target's HealthSystem
            HealthSystem targetHealth = other.GetComponent<HealthSystem>();

            if (targetHealth != null)
            {
                targetHealth.TakeDamage(_currentDamage);
            }
        }

        // --- VISUAL FEEDBACK ---
        if (_imapctParticles != null)
        {
            // Pool these later on
            Instantiate(_imapctParticles, transform.position, Quaternion.identity);
        }

        // Recycle the bullet when it hits anything else
        Deactivate();
    }
}
