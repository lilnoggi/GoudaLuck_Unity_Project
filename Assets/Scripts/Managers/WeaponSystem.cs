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

    [Header("Model Swapping")]
    [SerializeField] private Transform _weaponHolder;  // Where the gun physically sits on the player
    private GameObject _spawnedGunModel;               // Remembers which 3D model the player is currently holding

    private float _nextFireTime = 0f;

    // --- RUNTIME STATS ---
    private int _currentUpgradeLevel = 0;
    private float _currentDamage;
    private float _currentFireRate;

    private void Start()
    {
        if (_currentWeapon != null)
        {
            EquipWeapon(_currentWeapon);
        }
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        _currentWeapon = newWeapon;

        // Reset the runtime stats back to the base stats on the card
        _currentUpgradeLevel = 0;
        _currentDamage = _currentWeapon.Damage;
        _currentFireRate = _currentWeapon.FireRate;

        // --- SWAP THE 3D MODEL ---
        // Destroy the old gun if holding one
        if (_spawnedGunModel != null)
        {
            Destroy(_spawnedGunModel);
        }

        // Spawn the new gun
        if (_currentWeapon.WeaponModelPrefab != null && _weaponHolder != null)
        {
            // Tell Unity to put the gun in the holder, zero it's position, BUT keep its original prefab rotation and scale
            _spawnedGunModel =  Instantiate(_currentWeapon.WeaponModelPrefab, _weaponHolder, false);
        }

        Debug.Log(gameObject.name + " equipped the " + _currentWeapon.WeaponName + "!");

        // Add logic here to swap the 3D model of the gun
    }

    // The Shop UI will call this method when you click the Upgrade button
    public void BuyUpgrade()
    {
        if (_currentUpgradeLevel < _currentWeapon.MaxUpgradeLevel)
        {
            _currentUpgradeLevel++;

            // Apply the maths
            _currentDamage += _currentWeapon.DamageIncreasePerLevel;
            _currentFireRate -= _currentWeapon.FireRateDecreasePerLevel;

            // Clamp fire rate so it never hits 0 or goes negative (WOULD BREAK THE GAME)
            _currentFireRate = Mathf.Max(_currentFireRate, 0.05f);

            Debug.Log($"Weapon Upgraded to Level {_currentUpgradeLevel}! Damage: {_currentDamage}, FireRate: {_currentFireRate}");
        }
        else
        {
            Debug.Log("Weapon is already at max level.");
        }
    }

    // PlayerController or CatAI_Controller can call this method
    public void FireWeapon()
    {
        // SAFETY NET: Don't try to shoot if player doesn't have a weapon equipped
        if (_currentWeapon == null || _firePoint == null) return;

        // COOLDOWN CHECK: Use _currentFireRate instead of the card's base FireRate
        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _currentFireRate;

            // Ask the ProjectilePool for a bullet!
            GameObject obj = ProjectilePool.Instance.GetProjectile(_currentWeapon.ProjectilePrefab, _firePoint.position, _firePoint.rotation);

            // Tell the bullet who fired it
            CheeseProjectile projectileScript = obj.GetComponent<CheeseProjectile>();
            if (projectileScript != null)
            {
                // Pass the tag of the GameObject holding this WeaponSystem (Player or Enemy)
                projectileScript.Setup(gameObject.tag, _currentDamage);
            }

            // SFX can go here later
        }
    }

    // Getter for the UI to check the level
    public int GetUpgradeLevel() => _currentUpgradeLevel;
}