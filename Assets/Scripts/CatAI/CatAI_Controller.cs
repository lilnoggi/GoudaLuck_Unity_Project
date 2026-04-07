using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// === CAT AI CONTROLLER **REFACTORED** ===
/// <summary>
/// A component-based Finite State Machine (FSM) dictating Enemy AI behaviour.
/// By centralising the state evaluation within a single controller, this architecture
/// eliminates the "race conditions" found in legacy code and ensures absolute 
/// authority over movement and combat execution.
/// </summary>

public class CatAI_Controller : MonoBehaviour
{
    // --- FSM STATE DEFINITIONS ---
    public enum State { Chase, Attack }

    [Header("FSM Settings")]
    [Tooltip("The active behavioural state of the AI.")]
    [SerializeField] private State _currentState = State.Chase;
    [Tooltip("The distance at which the AI stops chasing and begins firing.")]
    [SerializeField] private float _attackRange = 8f;
    [Tooltip("The maximum distance the AI will track the player before disengaging.")]
    [SerializeField] private float _chaseRange = 15f;

    [Header("Combat Events")]
    [Tooltip("Triggered when the AI is in Attack Range. Drag WeaponSystem or MeleeSystem here!")]
    [SerializeField] private UnityEvent _onAttackTriggered;

    // --- COMPONENT DEPENDENCIES ---
    private NavMeshAgent _agent;
    private Transform _playerTarget;

    private void Awake()
    {
        // Cache component references during initialisation to avoid expensive GetComponent calls during gameplay
        _agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        // OBJECT POOLING SUPPORT: Reset the brain to its default state
        // every time it is pulled from the inactive queue.
        _currentState = State.Chase;

        // If the agent is placed on the NavMesh, unlock its movement
        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
        }
    }

    private void Start()
    {
        // Cache the player reference once at the start of the object's lifecycle
        // GameObject.Find is an expensive API call, so executing it here prevents per-frame CPU drain.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTarget = player.transform;
        }
    }

    private void Update()
    {
        // DEFENSIVE PROGRAMMING: Prevent NullReferenceExceptions if the player is destroyed
        if (_playerTarget == null) return; 

        // --- THE FSM BRAIN ---
        // This switch statement mathematically guarantees the AI evaluates and
        // executes exactly one state per frame, preventing logic conflicts.
        switch (_currentState)
        {
            case State.Chase:
                HandleChaseState();
                break;
            case State.Attack:
                HandleAttackState();
                break;
        }
    }

    /// <summary>
    /// Evaluates movement logic and handles the transition into the Attack state.
    /// </summary>
    private void HandleChaseState()
    {
        // LOGIC: Command the NavMeshAgent to track the player's current position
        _agent.isStopped = false;
        _agent.SetDestination(_playerTarget.position);

        // TRANSITION: Evaluate distance to determine if combat should commence
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTarget.position);
        if (distanceToPlayer <=  _attackRange)
        {
            _currentState = State.Attack;
        }
    }

    /// <summary>
    /// Stops movement, locks rotation to the target, and triggers the decoupled WeaponSystem.
    /// </summary>
    private void HandleAttackState()
    {
        // LOGIC: Stop pathfinding to establish a stable firing stance.
        _agent.isStopped = true;

        // Smoothly interpolate rotation to face the player's current position
        Vector3 directionToPlayer = (_playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;  // Keep the cat from tilting up/down
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 10f);

        // FIRE! (Demonstrates the Single Responsibility Principle: The AI only commands
        // while the WeaponSystem component autonomously calculates ammo, spread, and cooldowns).
        if (_onAttackTriggered != null)
        {
            _onAttackTriggered?.Invoke();
        }

        // TRANSITION: If the player successfully creates distance, revert to the Chase state
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTarget.position);
        if (distanceToPlayer > _attackRange)
        {
            _currentState = State.Chase;
        }
    }

    // === EDITOR DEBUGGING ===
    /// <summary>
    /// Draws visual boundary spheres in the Unity Editor to assist with level design and AI balancing.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _chaseRange);
    }
}
