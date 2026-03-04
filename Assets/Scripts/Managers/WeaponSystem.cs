using System.Collections;
using System.Collections.Generic;
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

    // --- AMMO STATS ---
    private int _currentAmmo;
    private bool _isReloading = false;
    private bool _hasUnlimitedAmmo = false;  // Unlimited Ammo Powerup

    // --- ARMOURY MEMORY ---
    // This tracks the upgrade level of every gun the player owns
    private Dictionary<string, int> _weaponUpgradeLevels = new Dictionary<string, int>();

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

        // --- MEMORY CHECK ---
        if (!_weaponUpgradeLevels.ContainsKey(_currentWeapon.WeaponName))
        {
            _weaponUpgradeLevels[_currentWeapon.WeaponName] = 0;
        }

        // Pull the saved upgrade level from the dictionary
        _currentUpgradeLevel = _weaponUpgradeLevels[_currentWeapon.WeaponName];

        // Re-calculate the damage and fire rate based on saved level
        _currentDamage = _currentWeapon.Damage + (_currentUpgradeLevel * _currentWeapon.DamageIncreasePerLevel);

        _currentFireRate = _currentWeapon.FireRate - (_currentUpgradeLevel * _currentWeapon.FireRateDecreasePerLevel);
        _currentFireRate = Mathf.Max(_currentFireRate, 0.05f);  // Keep the clamp

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

        _currentAmmo = _currentWeapon.MagSize;
        _isReloading = false;

        // If this weapon is on the Player, update the UI
        if (gameObject.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWeaponUI(_currentWeapon.WeaponIcon);
            UIManager.Instance.UpdateAmmo(_currentAmmo, _currentWeapon.MagSize);
        }
    }

    // The Shop UI will call this method when you click the Upgrade button
    public void BuyUpgrade()
    {
        if (_currentUpgradeLevel < _currentWeapon.MaxUpgradeLevel)
        {
            _currentUpgradeLevel++;

            // Save the new level back into the dictionary
            _weaponUpgradeLevels[_currentWeapon.WeaponName] = _currentUpgradeLevel;

            // Apply the maths
            _currentDamage += _currentWeapon.DamageIncreasePerLevel;
            _currentFireRate -= _currentWeapon.FireRateDecreasePerLevel;
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
        // SAFETY NET: Don't try to shoot if player doesn't have a weapon equipped OR currently reloading
        if (_currentWeapon == null || _firePoint == null || _isReloading) return;

        // AUTO-RELAOD: If click shoot but 0 ammo, trigger a reload instead
        if (_currentAmmo <= 0 && !_hasUnlimitedAmmo)
        {
            Reload();
            return;
        }

        // COOLDOWN CHECK: Use _currentFireRate instead of the card's base FireRate
        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _currentFireRate;

            // Only consume a bullet IF has limited ammo
            if (!_hasUnlimitedAmmo)
            {
                _currentAmmo--;  // Consume 1 bullet
            }
            
            // Ask the ProjectilePool for a bullet!
            GameObject obj = ProjectilePool.Instance.GetProjectile(_currentWeapon.ProjectilePrefab, _firePoint.position, _firePoint.rotation);

            // Tell the bullet who fired it
            CheeseProjectile projectileScript = obj.GetComponent<CheeseProjectile>();
            if (projectileScript != null)
            {
                // Pass the tag of the GameObject holding this WeaponSystem (Player or Enemy)
                projectileScript.Setup(gameObject.tag, _currentDamage);
            }

            // Update the UI after firing BUT only if INFINITE is not being displayed
            if (gameObject.CompareTag("Player") && UIManager.Instance != null && !_hasUnlimitedAmmo)
            {
                UIManager.Instance.UpdateAmmo(_currentAmmo, _currentWeapon.MagSize);
            }

            // SFX can go here later

        }
    }

    // === RELOAD WEAPON ===
    public void Reload()
    {
        // Don't reload if already reloading, or if the mag is already full
        if (_isReloading || _currentAmmo == _currentWeapon.MagSize) return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;

        if (gameObject.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmoText("RELOADING...");
        }

        // Wait for the duration specified in the data card
        yield return new WaitForSeconds(_currentWeapon.ReloadTime);

        _currentAmmo = _currentWeapon.MagSize;
        _isReloading = false;

        if (gameObject.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmo(_currentAmmo, _currentWeapon.MagSize);
        }
    }

    // Getter for the UI to check the level
    public int GetUpgradeLevel() => _currentUpgradeLevel;

    // Allows the Shop to check a weapon's level without equipping it first
    public int GetSpecificWeaponLevel(string weaponName)
    {
        if (_weaponUpgradeLevels.ContainsKey(weaponName))
        {
            return _weaponUpgradeLevels[weaponName];
        }
        return 0;  // If not in the dictionary, it is Level 0
    }

    // --- POWERUP LOGIC ---
    public void ActivateUnlimitedAmmo(float duration)
    {
        StartCoroutine(UnlimitedAmmoRoutine(duration));
    }

    private IEnumerator UnlimitedAmmoRoutine(float duration)
    {
        _hasUnlimitedAmmo = true;

        // Update the UI
        if (gameObject.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmoText("INFINITE");
        }

        // Wait for the powerup to expire
        yield return new WaitForSeconds(duration);

        _hasUnlimitedAmmo = false;

        // Reset the UI back to normal numbers
        if (gameObject.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateAmmo(_currentAmmo, _currentWeapon.MagSize);
        }
    }
}