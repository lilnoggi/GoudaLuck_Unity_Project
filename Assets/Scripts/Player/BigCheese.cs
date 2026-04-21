using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Controls the player's Ultimate Ability ("The Big Cheese").
/// Handles Area of Effect (AoE) physics calculations, delegates damage to the HealthSystem,
/// and triggers Cinemachine impulses to enhance visual "Game Feel" upon impact.
/// </summary>
public class BigCheese : MonoBehaviour
{
    [Header("Impact Settings")]
    [Tooltip("The flat damage amount applied to all entities caught within the blast radius.")]
    [SerializeField] private float _damage = 250f;
    [Tooltip("The spatial radius of the AoE crush zone.")]
    [SerializeField] private float _blastRadius = 5f;

    // --- STATE TRACKING ---
    private bool _hasLanded = false;

    // --- COMPONENT CACHING ---
    private CinemachineImpulseSource _impulseSource;

    // ==============================================================================================================

    private void Awake()
    {
        // Cache the Impulse Source component upon instantiation
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // DEFENSIVE PROGRAMMING: Prevent recursive explosions if the physics engine
        // registers multiple collision points (bouncing) on the same frame.
        if (_hasLanded) return;
        _hasLanded = true;

        // --- AoE BLAST LOGIC ---
        // Project a mathematical sphere into the physics engine and cache all intersecting colliders
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _blastRadius);

        foreach (Collider col in hitColliders)
        {
            // Evaluate tags to ensure the player is immune to their own blast
            if (col.CompareTag("Enemy"))
            {
                // Decoupled damage execution via the target's HealthSystem
                HealthSystem health = col.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(_damage);
                }
            }
        }

        // --- UX / GAME FEEL ---
        // Fire the Cinemachine Impulse to send a shockwave to the Virtual Camera Listener
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse();
        }

        // MEMORY MANAGEMENT: Destroy the GameObject shortly after the impact calculations
        //are complete to keep the scene hierarchy clean.
        Destroy(gameObject, 0.5f);
    }

    // =====================================================================================
    // ============================= --- EDITOR DEBUGGING ---  =============================
    // =====================================================================================
    // ===== Draws visual boundary spheres in the Unity Editor =============================
    // ===== to assist with balancing the blast radius.        =============================
    // =====================================================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _blastRadius);
    }
}
