using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// A decoupled combat component handling melee damage and visual feedback.
/// Designed to be triggered by an external controller (CatAI_Controller) 
/// to adhere to the Single Responsibility Principle.
/// </summary>

public class MeleeSystem : MonoBehaviour
{
    [Header("Melee Combat Settings")]
    [Tooltip("The amount of damage dealt to the player on a successful melee attack.")]
    [SerializeField] private float _meleeDamage = 25f;
    [Tooltip("The cooldown time between consecutive melee attacks.")]
    [SerializeField] private float _attackCooldown = 1.5f;
    private float _nextAttackTime;

    // --- COMPONENT DEPENDENCIES ---
    private Transform _playerTarget;
    private CinemachineImpulseSource _impulseSource;

    // ================================================================================================

    private void Awake()
    {
        // Cache the impulse source to drive camera shake on impact
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        // Dynamically locate the player at runtime
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTarget = player.transform;
        }
    }

    /// <summary>
    /// The public method to be called by the AI Brain.
    /// Safely handles its own cooldown mathematics asynchronously.
    /// </summary>
    public void TriggerAttack()
    {
        // Asynchronous cooldown check by bypassing InvokeRepeating
        if (Time.time >= _nextAttackTime)
        {
            SlamAttack();
            _nextAttackTime = Time.time + _attackCooldown;
        }
    }

    /// <summary>
    /// Executes the damage calculation and triggers UX feedback (Screen Shake).
    /// </summary>
    private void SlamAttack()
    {
        // Defensive check just in case the player was destroyed
        if (_playerTarget == null) return;
        
        // Decoupled damage execution via the Player's HealthSystem
        HealthSystem playerHealth = _playerTarget.GetComponent<HealthSystem>();    
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(_meleeDamage);
        }
        
        // VISUAL FEEDBACK: Trigger Cinemachine screen shake
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse();
        }
    }
}
