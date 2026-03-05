using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A struct to allow multiple enemy types in the Inspector
/// </summary>

[System.Serializable]
public struct EnemyPoolType
{
    public GameObject EnemyPrefab;
    public int PoolSize;
}

/// <summary>
/// Pre-allocates a pool of enemies to prevent Garbage Collection spikes.
/// Upgraded to a dictionary to support Basic Cats, Tank Cats, etc...
/// </summary>

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    [SerializeField] private EnemyPoolType[] _enemiesToPool;

    // A Dictionary of Queues
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Pre-warm the pool for EVERY enemy in the list
        foreach (EnemyPoolType enemyType in _enemiesToPool)
        {
            Queue<GameObject> enemyPool = new Queue<GameObject>();

            for (int i = 0; i < enemyType.PoolSize; i++)
            {
                GameObject obj = Instantiate(enemyType.EnemyPrefab, transform);

                // Force the name to match the prefab exactly
                obj.name = enemyType.EnemyPrefab.name;

                obj.SetActive(false);
                enemyPool.Enqueue(obj);
            }

            // Add this specific queue to the dictionary, using the prefab's name as the key
            _poolDictionary.Add(enemyType.EnemyPrefab.name, enemyPool);
        }
    }

    // --- Requires you to state WHICH enemy prefab to use ---
    public GameObject GetEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        if (_poolDictionary.ContainsKey(key) && _poolDictionary[key].Count > 0)
        {
            GameObject obj = _poolDictionary[key].Dequeue();

            // NOTE: Set the position BEFORE turning the enemy on.
            // This prevents the NavMeshAgent from throwing a teleportation error
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        Debug.Log("Enemy pool is empty! Increase the pool size?");
        return null;  // Return null instead of instantiating to force strict memory limits
    }

    public void ReturnEnemy(GameObject obj)
    {
        obj.SetActive(false);

        // Put it back in its specific "bucket" using its name
        if (_poolDictionary.ContainsKey(obj.name))
        {
            _poolDictionary[obj.name].Enqueue(obj);
        }
    }
}
