using UnityEngine;

/// <summary>
/// This script...
/// </summary>

public class EnemyDeathHandler : MonoBehaviour
{
    [Header("Loot Drops")]
    [SerializeField] private LootDrop[] _lootTable;
    [SerializeField] private float _masterDropChance = 0.1f;

    private HealthSystem _healthSystem;

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        // "Subscribe" to the health system when spawned
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        // Always "Unsubscribe" when turned off to prevent memory leaks
        if ( _healthSystem != null )
        {
            _healthSystem.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        // Update Game Managers
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
            // Roll a random number to determine if a drop occurs based on the master chance
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
                        // Spawn it slightly above the ground so it doesn't clip
                        Vector3 dropPos = transform.position + new Vector3(0f, 0.5f, 0f);
                        Instantiate(drop.Prefab, dropPos, Quaternion.identity);

                        // Break the loop so it doesn't spawn multiple items
                        break;
                    }
                }
            }
        }

        // Return to Pool
        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.ReturnEnemy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
