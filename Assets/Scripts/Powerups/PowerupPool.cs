using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A serialised data container allowing designers to configure pool sizes
/// for different powerups directly within the Unity Inspector.
/// </summary>
[System.Serializable]
public struct PowerupPoolType
{
    [Tooltip("The specific powerup prefab to be pooled (e.g., Health_Pickup).")]
    public GameObject powerupPrefab;
    [Tooltip("The exact number of instances to pre-allocate into memory upon scene load.")]
    public int PoolSize;
}

/// <summary>
/// A centralised memory management system for loot drops.
/// Pre-allocates powerups to prevent frame drops and material/shader loading lag
/// that occurs when dynamically instantiating complex visual effects during combat.
/// </summary>
public class PowerupPool : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static PowerupPool Instance { get; private set; }

    [Header("Pool Configuration")]
    [Tooltip("List of all powerup types and their respective memory budgets.")]
    [SerializeField] private PowerupPoolType[] _powerupsToPool;

    // --- MEMORY ARCHITECTURE ---
    // A dictionary mapping a string key (Prefab Name) to a Queue (The inactive object pool).
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

    // ==============================================================================================================

    private void Awake()
    {
        // Enforce Signleton instance
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // --- PRE-WARMING THE MEMORY HEAP ---
        foreach (PowerupPoolType pType in _powerupsToPool)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < pType.PoolSize; i++)
            {
                // Instantiate as a child of this manager to keep scene hierarchy clean
                GameObject obj = Instantiate(pType.powerupPrefab, transform);

                // Strip the default "(Clone)" suffix so the dictionary key matches exactly
                obj.name = pType.powerupPrefab.name; 

                obj.SetActive(false);
                pool.Enqueue(obj);
            }

            // Register the newly populated queue into the global dictionary
            _poolDictionary.Add(pType.powerupPrefab.name, pool);
        }
    }

    /// <summary>
    /// Retrieves a pre-allocated powerup from the requested pool.
    /// Enforces strict memory limits by returning null if the pool is exhausted.
    /// </summary>
    public GameObject GetPowerup(GameObject prefab, Vector3 position)
    {
        string key = prefab.name;

        // Verify the requested pool exists and has available inactive entities
        if (_poolDictionary.ContainsKey(key) && _poolDictionary[key].Count > 0)
        {
            GameObject obj = _poolDictionary[key].Dequeue();

            // Apply aptial positioning before activation
            obj.transform.position = position;
            obj.SetActive(true);

            return obj;
        }

        // DEFENSIVE PROGRAMMING: Enforce memory budget
        Debug.LogWarning($"[PowerupPool] Memory budget exceeded! {key} pool is empty! Consider increasing the pool size.");
        return null;
    }

    /// <summary>
    /// Deactivates a collected or expired powerup and recycles it back into its specific queue.
    /// </summary>
    public void ReturnPowerup(GameObject obj)
    {
        // Suspend logic and rendering
        obj.SetActive(false);

        // Utilise the cleaned object name to find the correct dictionary "bucket"
        if (_poolDictionary.ContainsKey(obj.name))
        {
            _poolDictionary[obj.name].Enqueue(obj);
        }
        else
        {
            // Safety fallback in case an untracked object is accidentally sent to the pool
            Debug.LogError($"[PowerupPool] Attempted to return an unregistered object: {obj.name}");
            Destroy(obj);
        }
    }
}
