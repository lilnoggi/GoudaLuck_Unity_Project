using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pre-allocates a pool of projectiles to prevent Garbage Collection spikes.
/// </summary>

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;  // Singleton reference

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
    public GameObject GetProjectile(Vector3 position, Quaternion rotation)
    {
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();  // Take it out the queue
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);               // Turn it on
            return obj;
        }

        // FAILSAFE: If run out of bullets, make a new one
        return Instantiate(_projectilePrefab, position, rotation);
    }

    // The CheeseProjectile will call this to put itself back in the pool
    public void ReturnProjectile(GameObject obj)
    {
        obj.SetActive(false);  // Turn it off
        _pool.Enqueue(obj);    // Put it back in line
    }
}
