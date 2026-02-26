using UnityEngine;

/// <summary>
/// Handles the movement and collision logic for the projectiles.
/// Currently uses Destroy() for prototyping, but will be upgrading to Object Pooling!
/// </summary>

public class CheeseProjectile : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _lifeTime = 2f;
    [SerializeField] private float _damage = 10f;  // Pass this to the enemy later

    void Start()
    {
        // SAFETY NET: Destroy the bullet after a few seconds so it doesn't fly into infinity
        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        // Move the bullet straight forward along its local Z-axis
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
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

        // Destroy the bullet when it hits anything else (like a wall or enemy)
        // Change to gameObject.SetActive(false) when the Object Pool is built
        Destroy(gameObject);
    }
}
