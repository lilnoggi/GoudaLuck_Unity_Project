using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pre-allocates a pool of enemies to prevent Garbage Collection spikes.
/// </summary>

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private int _poolSize = 15;  // How many cats to load into memory

    private Queue<GameObject> _pool = new Queue<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Pre-warm the pool
        for (int i = 0; i < _poolSize; i++)
        {
            GameObject obj = Instantiate(_enemyPrefab);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public GameObject GetEnemy(Vector3 position, Quaternion rotation)
    {
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();

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
        _pool.Enqueue(obj);
    }
}
