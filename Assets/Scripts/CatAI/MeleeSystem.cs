using UnityEngine;
using UnityEngine.AI;
using Unity.Cinemachine;

/// <summary>
/// This script...
/// </summary>

public class MeleeSystem : MonoBehaviour
{
    public enum State { Chase, Attack }

    [Header("FSM Settings")]
    [SerializeField] private State _currentState = State.Chase;
    [SerializeField] private float _attackRange = 2.5f;
    [SerializeField] private float _chaseRange = 20f;

    [Header("Melee Combat")]
    [SerializeField] private float _meleeDamage = 25f;
    [SerializeField] private float _attackCooldown = 1.5f;
    private float _nextAttackTime;

    private NavMeshAgent _agent;
    private Transform _playerTarget;
    private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        _currentState = State.Chase;

        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.isStopped = false;
        }
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTarget = player.transform;
        }
    }

    private void Update()
    {
        if (_playerTarget == null) return;
        
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
        _agent.isStopped = false;
        _agent.SetDestination(_playerTarget.position);

        if (Vector3.Distance(transform.position, _playerTarget.position) <= _attackRange)
        {
            _currentState = State.Attack;
        }
    }

    private void HandleAttackState()
    {
        _agent.isStopped = true;

        // Rotate to face player
        Vector3 directionToPlayer = (_playerTarget.position - transform.position).normalized;
        directionToPlayer.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(directionToPlayer), Time.deltaTime * 10f);

        // --- MELEE SLAM LOGIC ---
        if (Time.time >= _nextAttackTime)
        {
            SlamAttack();
            _nextAttackTime = Time.time + _attackCooldown;
        }

        if (Vector3.Distance(transform.position, _playerTarget.position) > _attackRange)
        {
            _currentState = State.Chase;
        }
    }

    private void SlamAttack()
    {
        // Hurt the player
        HealthSystem playerHealth = _playerTarget.GetComponent<HealthSystem>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(_meleeDamage);
        }

        // Shake the screen
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse();
        }
    }

    // === DEBUGGIN ===
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
