using UnityEngine;

/// <summary>
/// This script...
/// </summary>

public class PlayerDeathHandler : MonoBehaviour
{
    private HealthSystem _healthSystem;

    private void Awake()
    {
        _healthSystem = GetComponent<HealthSystem>();
    }

    private void OnEnable()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerGameOver();
        }
    }
}
