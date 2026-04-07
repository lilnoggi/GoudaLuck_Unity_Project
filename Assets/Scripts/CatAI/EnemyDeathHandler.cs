using UnityEngine;

/// <summary>
/// A decoupled listener script utilising the Observer Pattern (C# Events).
/// By subscribing to the HealthSystem's OnDeath event, this script handles
/// scoring, wave management, and loot instantiation without cluttering the core logic.
/// This prevents the "God Object" anti-pattern and adheres strictly to the Single Responsibility Principle.
/// </summary>

public class EnemyDeathHandler : MonoBehaviour
{
    [Header("Loot Drops")]
    [Tooltip("Array of possible item drops, each with a specific weight/chance.")]
    [SerializeField] private LootDrop[] _lootTable;
    [Tooltip("The overarching probability (0.0 to 1.0) that ANY item will drop upon death.")]
    [SerializeField] private float _masterDropChance = 0.1f;

    // --- COMPONENT DEPENDENCIES ---
    private HealthSystem _healthSystem;

    // ================================================================================================

    private void Awake()
    {
        // Cache the depdendency
        _healthSystem = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        // OBSERVER PATTERN: "Subscribe" to the publisher's event when this object is active.
        // This allows this script to passively listen for the death state.
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        // DEFENSIVE PROGRAMMING: Always "Unsubscribe" when deactivated or destroyed.
        // Failing to do this in Event-Driven architectures causes severe memory leaks.
        if ( _healthSystem != null )
        {
            _healthSystem.OnDeath -= HandleDeath;
        }
    }

    /// <summary>
    /// Executes independently when the OnDeath event is invoked by the HealthSystem.
    /// </summary>
    private void HandleDeath()
    {
        // --- GLOBAL STATE MANAGEMENT ---
        // Safely interact with Singletons to update game progression
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(10);
        }
        
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.EnemyDefeated();
        }

        // --- LOOT DROP LOGIC (Weighted Random Selection) ---
        if (_lootTable != null && _lootTable.Length > 0)
        {
            // First, determine if a drop occurs at all based on the master probability
            if (Random.value <= _masterDropChance)
            {
                // Calculate the total combined drop chance (weight) of all items in the table
                float totalWeight = 0f;
                foreach (LootDrop drop in _lootTable)
                {
                    totalWeight += drop.DropChance;
                }

                // Generate a random threshold value between 0 and the total weight
                float randomRoll = Random.Range(0f, totalWeight);
                float cumulativeWeight = 0f;

                // Iterate through the items to find which one encompasses the random threshold
                foreach (LootDrop drop in _lootTable)
                {
                    // Accumulate the weights step-by-step
                    cumulativeWeight += drop.DropChance;

                    // If the threshold falls within this item's accumulated range, select it
                    if (randomRoll <= cumulativeWeight)
                    {
                        // Calculate a safe spawn point slitghly above the NavMesh to prevent clipping
                        Vector3 dropPos = transform.position + new Vector3(0f, 0.5f, 0f);
                        
                        // MEMORY OPTIMISATION: Request the item from the Object Pool to bypass Garbage Collection
                        if (PowerupPool.Instance != null)
                        {
                            PowerupPool.Instance.GetPowerup(drop.Prefab, dropPos);
                        }
                        else
                        {
                            // Fallback logic for testing environments where the Pool might not exist
                            Instantiate(drop.Prefab, dropPos, Quaternion.identity);  // Fallback
                        }

                        // Exit the loop immediately to prevent spawning multiple items from a single roll
                        break;
                    }
                }
            }
        }

        // --- OBJECT POOLING: DEACTIVATION ---
        // Return the defeated enemy to the pool instead of destroying it
        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.ReturnEnemy(gameObject);
        }
        else
        {
            Destroy(gameObject);  // Fallback
        }
    }
}
