using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pre-allocates a pool of projectiles to prevent Garbage Collection spikes.
/// </summary>

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;  // Singleton reference

    // A dictionary allows for multiple pools
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private int _poolSize = 30;  // How many bullets to spawn at the start

    // A Queue is a FIFO data structure (First-In, First-Out) - perfect for pooling
    private Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        // Set up Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // "Pre-warm" the pool by spawning inactive cheese wedges
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject obj = Instantiate(_projectilePrefab);
            obj.SetActive(false);  // Turn it off immediately
            _pool.Enqueue(obj);    // Add it to the queue
        }
    }

    // WeaponSystem will call this to grab a bullet
    public GameObject GetProjectile(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string key = prefab.name;

        // If this is the first time asking for this bullet, create a pool
        if (!_poolDictionary.ContainsKey(key))
        {
            _poolDictionary[key] = new Queue<GameObject>();
        }

        GameObject obj;

        // Pull from the pool if there is a spare
        if (_poolDictionary[key].Count > 0)
        {
            obj = _poolDictionary[key].Dequeue();
        }
        else
        {
            // Otherwise, make a new one and name it exactly like they key
            obj = Instantiate(prefab);
            obj.name = key;
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);  // Turn it on
        return obj;
    }

    // The CheeseProjectile will call this to put itself back in the pool
    public void ReturnProjectile(GameObject obj)
    {
        obj.SetActive(false);  // Turn it off

        // SAFETY CHECK: Ensure the dictionary key exists before putting it back
        if (!_poolDictionary.ContainsKey(obj.name))
        {
            _poolDictionary[obj.name] = new Queue<GameObject>();
        }

        _poolDictionary[obj.name].Enqueue(obj);    // Put it back in line
    }
}
