using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PowerupPoolType
{
    public GameObject powerupPrefab;
    public int PoolSize;
}

/// <summary>
/// Pre-allocates powerups to prevent frame drops and material loading lag.
/// </summary>

public class PowerupPool : MonoBehaviour
{
    public static PowerupPool Instance;

    [SerializeField] private PowerupPoolType[] _powerupsToPool;

    private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Pre-warm the pool to load materials into memory immediately
        foreach (PowerupPoolType pType in _powerupsToPool)
        {
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < pType.PoolSize; i++)
            {
                GameObject obj = Instantiate(pType.powerupPrefab, transform);
                obj.name = pType.powerupPrefab.name;  // Keep name clean so dictiontary key works
                obj.SetActive(false);
                pool.Enqueue(obj);
            }

            _poolDictionary.Add(pType.powerupPrefab.name, pool);
        }
    }

    public GameObject GetPowerup(GameObject prefab, Vector3 position)
    {
        string key = prefab.name;

        if (_poolDictionary.ContainsKey(key) && _poolDictionary[key].Count > 0)
        {
            GameObject obj = _poolDictionary[key].Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning($"Powerup pool for {key} is empty!");
        return null;
    }

    public void ReturnPowerup(GameObject obj)
    {
        obj.SetActive(false);
        if (_poolDictionary.ContainsKey(obj.name))
        {
            _poolDictionary[obj.name].Enqueue(obj);
        }
    }
}
