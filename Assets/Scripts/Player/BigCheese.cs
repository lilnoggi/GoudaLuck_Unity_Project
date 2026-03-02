using UnityEngine;

/// <summary>
/// This script...
/// </summary>

public class BigCheese : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float _damage = 250f;     // Massive damage
    [SerializeField] private float _blastRadius = 5f;  // How big the crush zone is

    private bool _hasLanded = false;

    private void OnCollisionEnter(Collision collision)
    {
        // Prevent it from exploding multiple times if it bounces
        if (_hasLanded) return;
        _hasLanded = true;

        // --- AOE BLAST LOGIC ---
        // Draw an invisible mathmatical sphere, and grab everything touching it
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _blastRadius);

        foreach (Collider col in hitColliders)
        {
            // If it's an enemy, completely crush them
            if (col.CompareTag("Enemy"))
            {
                HealthSystem health = col.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(_damage);
                }
            }
        }

        Debug.Log("The Big Cheese has landed!");

        // Destroy the cheese wheel shortly after it hits the ground
        Destroy(gameObject, 0.5f);
    }

    // --- DEBUGGIN ---
    // Draws a red sphere in the editor so you can see how big the blast is
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _blastRadius);
    }
}
