using UnityEngine;

/// <summary>
/// Manages global game states such as the player's 'Cheddar Points' score and win/loss conditions.
/// Uses a Singleton design pattern to provide a globally accessible point of reference.
/// </summary>

public class GameManager : MonoBehaviour
{
    // The static Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int _score = 0;
    // [SerializeField] private int _currentWave = 1;

    private void Awake()
    {
        // --- SINGLETON PATTERN SETUP ---
        if (Instance != null && Instance != this)
        {
            // If another GameManager already exists, destroy this duplicate.
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Called by the HealthSystem when a cat dies
    public void AddScore(int amount)
    {
        _score += amount;
        Debug.Log("Cat defeated! Cheddar Points: " + _score);
    }

    // Called by the HealthSystem when the player dies
    public void TriggerGameOver()
    {
        Debug.Log("=== GAME OVER === The felines have taken over.");

        // Pause the game physics so nothing else can move or shoot
        Time.timeScale = 0f;

        // UI comes here later
    }
}
