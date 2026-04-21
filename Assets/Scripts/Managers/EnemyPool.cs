using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A serialised data container allowing designers to easily configure
/// distinct pool sizes for various enemy archetypes directly within the Unity Inspector.
/// </summary>
[System.Serializable]
public struct EnemyPoolType
{
    [Tooltip("The specific enemy prefab to be pooled (e.g., Cat_Ranged, Cat_Tank).")]
    public GameObject EnemyPrefab;
    [Tooltip("The number of instances to pre-allocate into memory upon scene load.")]
    public int PoolSize;
}

// ===========================================================================================================

/// <summary>
/// A centralised memory management system designed to bypass the C# Garbage Collector.
/// Utilises a Dictionary of Queues to pre-allocate and recycle multiple enemy variants,
/// ensuring zero runtime instantiation spikes during gameplay.
/// </summary>
public class EnemyPool : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static EnemyPool Instance { get; private set; }

    [Header("Pool Configuration")]
    [Tooltip("List of all enemy types and their respective memory budgets.")]
    [SerializeField] private EnemyPoolType[] _enemiesToPool;

    // --- MEMORY ARCHITECTURE ---
    // A Dictionary mapping a string key (Prefab Name) to a Queue (The inactive object pool).
    // This allows for 0(1) time complexity lookups when requesting a specific enemy type.
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

    // ==============================================================================================================

    private void Awake()
    {
        // Enforce Singleton instance
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // --- PRE-WARMING THE MEMORY HEAP ---
        // Instantiate all required entities during the loading screen to ensure
        // 60 FPS gameplay is never interrupted by memory allocation.
        foreach (EnemyPoolType enemyType in _enemiesToPool)
        {
            Queue<GameObject> enemyPool = new Queue<GameObject>();

            for (int i = 0; i < enemyType.PoolSize; i++)
            {
                // Instantiate as a child of this manager to keep the hierarchy organised
                GameObject obj = Instantiate(enemyType.EnemyPrefab, transform);

                // NOTE: Unity appends "(Clone)" to instantiated objects by default.
                // Strip this out so the object's name strictly matches the Dictionary key for easy retrieval.
                obj.name = enemyType.EnemyPrefab.name;

                obj.SetActive(false);
                enemyPool.Enqueue(obj);
            }

            // Register the newly populated queue into the global dictionary
            _poolDictionary.Add(enemyType.EnemyPrefab.name, enemyPool);
        }
    }

    /// <summary>
    /// Retrieves a pre-allocated enemy from the requested pool.
    /// Enforces strict memory limits by returning null if the pool is exhausted.
    /// </summary>
    public GameObject GetEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        // Verify the requested pool exists and has available inactive entities 
        if (_poolDictionary.ContainsKey(key) && _poolDictionary[key].Count > 0)
        {
            GameObject obj = _poolDictionary[key].Dequeue();

            // NOTE: Always apply spatial transforms BEFORE setting the obejct to active.
            // Activating a NavMeshAgent and then moving it causes severe pathing and teleportation errors.
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            obj.SetActive(true);
            return obj;
        }

        // DEFENSIVE PROGRAMMING: Instead of dynamically allocating new memory (which causes GC spikes),
        //enforce strict memory budgets required for handheld hardware.
        Debug.LogWarning($"[EnemyPool] Memory budget exceeded! {key} pool is empty. Consider increasing the pool size in the inspector.");
        return null; 
    }

    /// <summary>
    /// Deactivates a defeated enemy and recycles it back into its specific queue.
    /// </summary>
    public void ReturnEnemy(GameObject obj)
    {
        obj.SetActive(false);

        // Utilise the cleaned object name (set in Awake) to find the correct dictionary "bucket"
        if (_poolDictionary.ContainsKey(obj.name))
        {
            _poolDictionary[obj.name].Enqueue(obj);
        }
        else
        {
            // Safety fallback in case an untracked object is accidentally sent to the pool.
            Debug.LogError($"[EnemyPool] Attempted to return an unregistered object: {obj.name}. This object will be destroyed to prevent memory leaks.");
            Destroy(obj);
        }
    }
}
