using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pre-allocates a pool of projectiles to prevent Garbage Collection spikes.
/// Upgraded to a Dictionary Pool to support multiple ammo types dynamically.
/// </summary>

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;  // Singleton reference

    // A dictionary allows for multiple pools
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        // Set up Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
            obj = Instantiate(prefab, transform);  // By adding 'transform' here, the bullet becomes a child of the Pool
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
