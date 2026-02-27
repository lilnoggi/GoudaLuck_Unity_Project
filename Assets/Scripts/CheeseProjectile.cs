using UnityEngine;

/// <summary>
/// Handles the movement and collision logic for the projectiles.
/// Fully upgraded to use Object Pooling
/// </summary>

public class CheeseProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 2f;
    [SerializeField] private float _damage = 10f;  // Pass this to the enemy later

    private void OnEnable()
    {
        // Use OnEnable instead of Start() because the pool will turn this object on and off multiple times
        Invoke("Deactivate", _lifeTime);  // SAFETY NET: recycle after a few seconds
    }

    private void OnDisable()
    {
        CancelInvoke("Deactivate");  // Clean up the timer if it hits a wall early
    }

    private void Update()
    {
        // Move the bullet straight forward along its local Z-axis
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void Deactivate()
    {
        // Send it back to the pool instead of destroying it
        if (ProjectilePool.Instance != null)
        {
            ProjectilePool.Instance.ReturnProjectile(gameObject);
        }
        else
        {
            Destroy(gameObject);  // Fallback just in case
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore collision with the player who fired it
        if (other.CompareTag("Player")) return;

        // If it hits an enemy, eventually call their TakeDamage() method
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Hit an Enemy!");
        }

        // Recycle the bullet when it hits anything else
        Deactivate();
    }
}
