using System.Collections;
using UnityEngine;

/// <summary>
/// Handles spawning waves of enemies using the EnemyPool.
/// </summary>

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private Transform[] _spawnPoints;  // Drag empty GameObject here as spawn locations
    [SerializeField] private float _timeBetweenSpawns = 1.5f;
    [SerializeField] private int _enemiesPerWave = 5;

    private int _currentWave = 1;

    void Start()
    {
        // Start the first wave
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        Debug.Log("--- WAVE " + _currentWave + " STARTING! ---");

        for (int i = 0; i < _enemiesPerWave; i++)
        {
            // Pick a random spawn point from the array
            Transform randomSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

            // Ask the pool for a cat
            EnemyPool.Instance.GetEnemy(randomSpawnPoint.position, randomSpawnPoint.rotation);

            // Wait a moment before spawning the next one
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }
    }
}
