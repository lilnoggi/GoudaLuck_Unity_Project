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

    // A public getter so the shop can check the score
    public int Score => _score;

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
        // Debug.Log("Cat defeated! Cheddar Points: " + _score);

        // Tell the UI manager score has changed
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(_score);
        }
    }

    // The shop will call this when you click "Upgrade"
    public bool SpendPoints(int amount)
    {
        if (_score >= amount)
        {
            _score -= amount;

            // Update the UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore(_score);
                return true;  // Purchase successful
            }
        }
        return false;  // Not enough points
    }

    // Called by the HealthSystem when the player dies
    public void TriggerGameOver()
    {
        // Debug.Log("=== GAME OVER === The felines have taken over.");

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        // Pause the game physics so nothing else can move or shoot
        Time.timeScale = 0f;

        // UI comes here later
    }
}
