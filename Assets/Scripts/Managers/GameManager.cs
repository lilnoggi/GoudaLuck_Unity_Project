using UnityEngine;

/// <summary>
/// Manages global game states such as the player's 'Cheddar Points' economy and win/loss conditions.
/// Implements the Singleton design pattern to provide a globally accessible point of reference
/// without requiring tangled, hardcoded inspector references across the codebase.
/// </summary>

public class GameManager : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [Tooltip("The player's current currency (Cheddar Points), used for purchasing weapon upgrades.")]
    [SerializeField] private int _score = 0;

    // Encapsulated public getter prevents external scripts from modifying the score directly without using Add/Spend methods
    public int Score => _score;

    private void Awake()
    {
        // --- SINGLETON PATTERN SETUP ---
        // Enforce strict global access and prevent duplicate managers during scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Increases the player's currency. Designed to be triggered by decoupled event listeners
    /// (e.g., when an enemy is defeated) to maintain separation of concerns and avoid tight coupling between game systems.
    /// </summary>
    public void AddScore(int amount)
    {
        _score += amount;

        // Decoupled UI Update: The GameManager pushes data to the UIManager rather than the UI polling for it
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateScore(_score);
        }
    }

    /// <summary>
    /// Evaluates transaction logic for the Shop.
    /// Returns true if the player has enough points to make the purchase, deducts the amount from the score,
    /// and updates the UI. Returns false if the player cannot afford the purchase, leaving the score unchanged.
    /// </summary>
    public bool SpendPoints(int amount)
    {
        if (_score >= amount)
        {
            // Execute the transaction
            _score -= amount;

            // Update the UI safely
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateScore(_score);
            }
            
            // Purchase successful
            // Return true regardless of UI state so the transaction succeeds logically
            return true; 
        }

        // Transaction failed due to insufficient funds
        return false;
    }

    /// <summary>
    /// Stops the gameplay loop and triggers the loss state presentation layer.
    /// </summary>
    public void TriggerGameOver()
    {
        // Prompt the UI Manager to display the Game Over screen
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowGameOver();
        }

        // Pause the game physics so nothing else can move or shoot
        Time.timeScale = 0f;
    }
}
