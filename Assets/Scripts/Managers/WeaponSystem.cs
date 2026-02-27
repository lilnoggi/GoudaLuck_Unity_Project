using UnityEngine;

/// <summary>
/// A modular component that handles firing logic, cooldowns, and projectiles spawning
/// This can be attatched to the Player OR an Enemy AI.
/// </summary>

public class WeaponSystem : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] private float _fireRate = 0.2f;
    [SerializeField] private float _damage = 10f;

    [Header("References")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;

    private float _nextFireTime = 0f;

    // PlayerController or CatAI_Controller can call this method
    public void FireWeapon()
    {
        // COOLDOWN CHECK: Only fire if enough time has passed
        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _fireRate;

            if (_projectilePrefab != null && _firePoint != null)
            {
                // Ask the ProjectilePool for a bullet!
                GameObject obj = ProjectilePool.Instance.GetProjectile(_firePoint.position, _firePoint.rotation);

                // Tell the bullet who fired it
                CheeseProjectile projectileScript = obj.GetComponent<CheeseProjectile>();
                if (projectileScript != null)
                {
                    // Pass the tag of the GameObject holding this WeaponSystem (Player or Enemy)
                    projectileScript.Setup(gameObject.tag);
                }

                // SFX can go here later
            }
            else
            {
                Debug.LogWarning("Weapon System is missing a projectile Prefab or Fire Point!");
            }
        }
    }
}
