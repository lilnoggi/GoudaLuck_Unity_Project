using System.Collections;
using UnityEngine;

/// <summary>
/// A centralised controller managing the core gameplay loop and progression pacing.
/// Utilises asynchronous Coroutines to handle staggered enemy spawning and integrates
/// with the EnemyPool to strictly enforce memory limits during runtime.
/// </summary>
public class WaveManager : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    [Tooltip("Array of empty GameObjects dictating valid NavMesh spawn locations.")]
    [SerializeField] private Transform[] _spawnPoints; 
    [SerializeField] private float _timeBetweenSpawns = 1.5f;
    [SerializeField] private int _enemiesPerWave = 5;

    [Header("Enemy Types")]
    [SerializeField] private GameObject _basicCatPrefab;
    [SerializeField] private GameObject _tankCatPrefab;
    [SerializeField] private GameObject _kittyTankPrefab;
    [Tooltip("The wave number where Tank cats are put into the spawn pool. The default is 3.")]
    [SerializeField] private int _waveToSpawnTanks = 3;
    [Tooltip("The wave number where Kitt Tanks are put into the spawn pool. Default is 5.")]
    [SerializeField] private int _waveToSpawnKittyTanks = 5;

    // --- PROGRESSION STATE ---
    [SerializeField] private int _currentWave = 1;
    private int _enemiesAlive = 0;

    // --- ASYNCHRONOUS TRACKING ---
    // Acts as a lock to prevent the wave from ending while enemies are still spawning
    private bool _isSpawning = false; 

    private void Awake()
    {
        // Enforce Singleton Pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Initiate the primary game loop
        StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// A decoupled listener method triggered by the EnemyDeathHandler.
    /// Evaluates wave completion logic every time an entity is removed from the board.
    /// </summary>
    public void EnemyDefeated()
    {
        _enemiesAlive--;

        // Only transition to the UI phase if all enemies are dead AND the spawner has finished its queue
        if (_enemiesAlive <= 0 && !_isSpawning)
        {
            EndWaveSequence();
        }
    }

    /// <summary>
    /// Determines which presentation layer (Shop vs Upgrades) to display based on the current wave number.
    /// </summary>
    private void EndWaveSequence()
    {
        // PROGRESSION MATHS: Trigger the roguelite upgrade screen every 3rd wave
        if (_currentWave % 3 == 0)
        {
            if (PlayerUpgradeManager.Instance != null)
            {
                PlayerUpgradeManager.Instance.ShowUpgradeScreen();
            }
        }
        else
        {
            // Otherwise, go to the standard Shop
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowShop();
            }
        }
    }

    /// <summary>
    /// Increments difficulty modifiers and restarts the asynchronous spawn loop.
    /// </summary>
    public void StartNextWave()
    {
        _currentWave++;
        _enemiesPerWave += 2;  // Apply linear difficulty scaling

        StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// An asynchronous loop that staggers enemy instantiation to prevent CPU spikes
    /// and manages probabilistic enemy selection.
    /// </summary>
    private IEnumerator SpawnWave()
    {
        // Lock the progression state
        _isSpawning = true;

        // Tell UIManager Wave has changed
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWave(_currentWave);
        }

        // PACING: Provide the player a brief grace period before combat resumes
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < _enemiesPerWave; i++)
        {
            // Select a random spatial node
            Transform randomSpawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

            // Decide WHICH cat to spawn
            GameObject enemyToSpawn = _basicCatPrefab;  // Default fallback

            // --- PROBABILISTIC SPAWNING LOGIC ---
            // Evaluates the rarest/hardest enemies first
            if (_currentWave >= _waveToSpawnKittyTanks && Random.value <= 0.15f)
            {
                enemyToSpawn = _kittyTankPrefab;
            }
            // If the Kitty Tank, roll for regular tank
            else if (_currentWave >= _waveToSpawnTanks && Random.value <= 0.2f)
            { 
                enemyToSpawn = _tankCatPrefab;
            }

            // --- OBJECT POOLING INJECTION ---
            // Request the specific entity from memory rather than instantiating it dynamically
            GameObject spawnedCat = EnemyPool.Instance.GetEnemy(enemyToSpawn, randomSpawnPoint.position, randomSpawnPoint.rotation);

            // SAFETY CHECK: Only increment the tracking variable if the Object Pool successfully provided an entity
            if (spawnedCat != null)
            {
                _enemiesAlive++;
            }

            // Yield execution to stagger spawns
            yield return new WaitForSeconds(_timeBetweenSpawns);
        }

        // Release the progression lock
        _isSpawning = false;

        // --- EDGE CASE SAFETY NET ---
        // If the player defeated the final enemy during the exact frame it spawned,
        // the EnemyDefeated() lock check might have failed. Re-evaluate here.
        if (_enemiesAlive <= 0)
        {
            EndWaveSequence();
        }
    }
}
