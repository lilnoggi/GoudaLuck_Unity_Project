using System.Collections;
using UnityEngine;

/// <summary>
/// Handles spawning waves of enemies using the EnemyPool.
/// </summary>

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private Transform[] _spawnPoints;  // Drag empty GameObject here as spawn locations
    [SerializeField] private float _timeBetweenSpawns = 1.5f;
    [SerializeField] private int _enemiesPerWave = 5;

    private int _currentWave = 1;
    private int _enemiesAlive = 0;

    private void Awake()
    {
        // Set up the Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Start the first wave
        StartCoroutine(SpawnWave());
    }

    // The HealthSystem will call this method every time a cat is defeated
    public void EnemyDefeated()
    {
        _enemiesAlive--;

        // If all enemies in this wave are dead, open the shop.
        if (_enemiesAlive <= 0)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowShop();
            }
        }
    }

    // The "Next Wave" button in the shop will call this
    public void StartNextWave()
    {
        _currentWave++;
        _enemiesPerWave += 2;  // Increase difficulty

        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        Debug.Log("--- WAVE " + _currentWave + " STARTING! ---");

        // Tell UIManager Wave has changed
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWave(_currentWave);
        }

        // Wait a few seconds before spawning so the player can breath
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < _enemiesPerWave; i++)
        {
            // Pick a random spawn point from the array
            Transform randomSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

            // Ask the pool for a cat
            GameObject spawnedCat = EnemyPool.Instance.GetEnemy(randomSpawnPoint.position, randomSpawnPoint.rotation);

            // SAFETY CHECK: Only count the enemy if the pool wasn't empty
            if (spawnedCat != null)
            {
                _enemiesAlive++;
            }

            // Wait a moment before spawning the next one
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }
    }
}
