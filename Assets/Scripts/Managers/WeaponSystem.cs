using UnityEngine;

/// <summary>
/// A modular component that handles firing logic, cooldowns, and projectiles spawning.
/// Now powered by Encapsulated ScriptableObjects (WeaponData).
/// This can be attatched to the Player OR an Enemy AI.
/// </summary>

public class WeaponSystem : MonoBehaviour
{
    [Header("Current Weapon")]
    [SerializeField] private WeaponData _currentWeapon;  // Drag Cheddar-19_Data here
    [SerializeField] private Transform _firePoint;

    private float _nextFireTime = 0f;

    public void EquipWeapon(WeaponData newWeapon)
    {
        _currentWeapon = newWeapon;
        Debug.Log(gameObject.name + " equipped the " + _currentWeapon.WeaponName + "!");

        // Add logic here to swap the 3D model of the gun
    }

    // PlayerController or CatAI_Controller can call this method
    public void FireWeapon()
    {
        // SAFETY NET: Don't try to shoot if player doesn't have a weapon equipped
        if (_currentWeapon == null || _firePoint == null) return;

        // COOLDOWN CHECK: Read the FireRate directly from the ScriptableObject
        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _currentWeapon.FireRate;


            // Ask the ProjectilePool for a bullet!
            GameObject obj = ProjectilePool.Instance.GetProjectile(_firePoint.position, _firePoint.rotation);

            // Tell the bullet who fired it
            CheeseProjectile projectileScript = obj.GetComponent<CheeseProjectile>();
            if (projectileScript != null)
            {
                // Pass the tag of the GameObject holding this WeaponSystem (Player or Enemy)
                projectileScript.Setup(gameObject.tag, _currentWeapon.Damage);
            }

            // SFX can go here later
        }
    }
}