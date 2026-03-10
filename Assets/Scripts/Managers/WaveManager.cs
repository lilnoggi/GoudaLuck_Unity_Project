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

    [Header("Enemy Types")]
    [SerializeField] private GameObject _basicCatPrefab;
    [SerializeField] private GameObject _tankCatPrefab;
    [SerializeField] private GameObject _kittyTankPrefab;
    [SerializeField] private int _waveToSpawnTanks = 3;  // Tanks won't appear until wave 3
    [SerializeField] private int _waveToSpawnKittyTanks = 5;

    private int _currentWave = 1;
    private int _enemiesAlive = 0;

    private bool _isSpawning = false;  // Tracks if the coroutine is currently busy

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

        // Only open the shop if done spawning
        if (_enemiesAlive <= 0 && !_isSpawning)
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

        // Lock the shop from opening
        _isSpawning = true;

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

            // Decide WHICH cat to spawn
            GameObject enemyToSpawn = _basicCatPrefab;  // Default

            // If reached the harder waves:
            // Roll for the rarist cat first
            if (_currentWave >= _waveToSpawnKittyTanks && Random.value <= 0.15f)
            {
                    enemyToSpawn = _kittyTankPrefab;
            }
            // If the Kitty Tank, roll for regular tank
            else if (_currentWave >= _waveToSpawnTanks && Random.value <= 0.2f)
            { 
                    enemyToSpawn = _tankCatPrefab;
            }

            // Ask the pool for the specific cat
            GameObject spawnedCat = EnemyPool.Instance.GetEnemy(enemyToSpawn, randomSpawnPoint.position, randomSpawnPoint.rotation);

            // SAFETY CHECK: Only count the enemy if the pool wasn't empty
            if (spawnedCat != null)
            {
                _enemiesAlive++;
            }

            // Wait a moment before spawning the next one
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }

        // Unlock the shop now that all enemies have been spawned
        _isSpawning = false;

        // --- SAFETY CHECK ---
        // Just in case the player killed the absolute last enemy the exact millisecond it spawned
        if (_enemiesAlive <= 0)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowShop();
            }
        }
    }
}
