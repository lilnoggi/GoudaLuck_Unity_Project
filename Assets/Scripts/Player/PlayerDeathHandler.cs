using UnityEngine;

/// <summary>
/// A dedicated listener script utilising the Observer Pattern to handle the player's death state.
/// By decoupling this logic from the core HealthSystem, we prevent the "GodObject" anti-pattern
/// and ensure the player's death triggers the global Game Over sequence cleanly.
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    // --- COMPONENT DEPENDENCIES ---
    private HealthSystem _healthSystem;

    private void Awake()
    {
        // Cache the dependency during initialisation
        _healthSystem = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        // OBSERVER PATTERN: Subscribe to the publisher's event
        // This allows the script to passively listen for the death state without a per-frame Update loop.
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        // DEFENSIVE PROGRAMMING: Always unsubscribe when disabled or destroyed.
        // Failing to do so causes severe memory leaks in Event-Driven architectures.
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath -= HandleDeath;
        }
    }

    /// <summary>
    /// Executes independently when the OnDeath event is invoked by the player's HealthSystem.
    /// </summary>
    private void HandleDeath()
    {
        // --- GLOBAL STATE MANAGEMENT ---
        // Safely notify the global GameManager to stop the gameplay loop and display the presentation layer
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
