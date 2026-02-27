using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// A centralised Finite State Machine (FSM) for Enemy AI.
/// Eliminates the "race conditions" from legacy code by ensuring only one state runs at a time.
/// </summary>

public class CatAI_Controller : MonoBehaviour
{
    // Define the FSM States
    public enum State { Chase, Attack }

    [Header("FSM Settings")]
    [SerializeField] private State _currentState = State.Chase;
    [SerializeField] private float _attackRange = 8f;
    [SerializeField] private float _chaseRange = 15f;  // How far it will chase before giving up

    private NavMeshAgent _agent;
    private WeaponSystem _weaponSystem;
    private Transform _playerTarget;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _weaponSystem = GetComponent<WeaponSystem>();
    }

    private void Start()
    {
        // Find the player automatically
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTarget = player.transform;
        }
    }

    private void Update()
    {
        if (_playerTarget == null) return;  // Do nothing if the player is dead/missing

        // --- THE FSM BRAIN ---
        // This switch statement ensures the AI can never be chasing and attacking at the same time
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

    private void HandleChaseState()
    {
        // LOGIC: Move towards the player
        _agent.isStopped = false;
        _agent.SetDestination(_playerTarget.position);

        // TRANSITION: If AI gets close enough, switch to Attack state
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTarget.position);
        if (distanceToPlayer <=  _attackRange)
        {
            _currentState = State.Attack;
        }
    }

    private void HandleAttackState()
    {
        // LOGIC: Stop moving, stare down the player, and pull the trigger
        _agent.isStopped = true;

        // Rotate to face the player
        Vector3 directionToPlayer = (_playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;  // Keep the cat from tilting up/down
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 10f);

        // FIRE! (The WeaponSystem handles its own cooldowns, spam safely
        if (_weaponSystem != null)
        {
            _weaponSystem.FireWeapon();
        }

        // TRANSITION: If the player runs away, switch back to Chase state
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTarget.position);
        if (distanceToPlayer > _attackRange)
        {
            _currentState = State.Chase;
        }
    }

    // === DEBUGGING ===
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _chaseRange);
    }
}
