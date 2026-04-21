using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A dynamic, Dictionary-based Object Pool handling projectile memory allocation.
/// Prevents severe frame-rate stuttering by recycling GameObjects instead of
/// continously triggering the C# Garbage Collector via Instantiate/Destroy loops.
/// </summary>
public class ProjectilePool : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static ProjectilePool Instance; 

    // --- MEMORY ARCHITECTURE ---
    // A Dictionary mapping a string key (Prefab Name) to a Queue (The inactive object pool).
    // This allows the system to seamlessly support dozens of different ammo types simultaneously.
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

    // =================================================================================================================

    private void Awake()
    {
        // Enforce the Singleton Pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Retrieves a projectile from the pool.
    /// Utilises a dynamic expansion strategy: if the pool is empty, it will instantiate
    /// a new object to prevent the weapon from misfiring.
    /// </summary>
    public GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        // LAZY INITIALISATION: If this ammo type has never been requested before,
        // allocate a new "bucket" for it in the dictionary.
        if (!_poolDictionary.ContainsKey(key))
        {
            _poolDictionary[key] = new Queue<GameObject>();
        }

        GameObject obj;

        // MEMORY RECYCLING: Pull from the pool if an inactive entity is available
        if (_poolDictionary[key].Count > 0)
        {
            obj = _poolDictionary[key].Dequeue();
        }
        else
        {
            // DYNAMIC EXPANSION: Otherwise, allocate new memory and force its name
            // to match the dictionary key (stripping the default "(Clone)" suffix).
            // Instantiating with 'transform' keeps the scene hierarchy organised.
            obj = Instantiate(prefab, transform); 
            obj.name = key;
        }

        // Apply spatial transforms
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // Activate the entity
        obj.SetActive(true);  
        return obj;
    }

    /// <summary>
    /// Deactivates a projectile upon impact or timeout, returning it to the pool for future use.
    /// </summary>
    public void ReturnProjectile(GameObject obj)
    {
        // Suspend the object's logic and rendering
        obj.SetActive(false); 

        // DEFENSIVE PROGRAMMING: Ensure the dictionary "bucket" exists before attemping to enqueue.
        // This prevents KeyNotFound exceptions if an external script tries to return an untracked object.
        if (!_poolDictionary.ContainsKey(obj.name))
        {
            _poolDictionary[obj.name] = new Queue<GameObject>();
        }

        // Return the obejct to the back of the queue
        _poolDictionary[obj.name].Enqueue(obj);
    }
}
