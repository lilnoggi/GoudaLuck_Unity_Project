using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

/// <summary>
/// A modular component handling firing logic, ammo management, and projectile instantiation.
/// Driven by encapsulated ScriptableObjects (WeaponData) to allow for data-driven design.
/// Adheres to Component-Based Architecture by functioning identically whether attatched to
/// a Player Controller or an Enemy AI Brain.
/// </summary>

public class WeaponSystem : MonoBehaviour
{
    [Header("Current Weapon")]
    [Tooltip("The ScriptableObject containing the base stats for the equipped weapon. Drag a WeaponData card here! (Cheddar-19 is the default)")]
    [SerializeField] private WeaponData _currentWeapon;
    [Tooltip("The fallback transform for projectile instantiation (useful for enemies without distinct gun models).")]
    [SerializeField] private Transform _defaultFirePoint;
    private Transform _activeFirePoint;  // The actual point the bullet comes from

    [Header("Model Swapping")]
    [Tooltip("The parent transform where the 3D gun model will be instantiated.")]
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
    // Utilises a Dictionary for 0(1) time complexity lookups of weapon upgrade levels.
    private Dictionary<string, int> _weaponUpgradeLevels = new Dictionary<string, int>();

    // --- WEAPON INVENTORY ---
    // A dynamic List tracking unlocked weapons to allow cycling via player input.
    private List<WeaponData> _unlockedWeapons = new List<WeaponData>();
    private int _currentWeaponIndex = 0;

    private void Start()
    {
        // Establish the baseline fire point to prevent NullReferenceExceptions
        _activeFirePoint = _defaultFirePoint;

        if (_currentWeapon != null)
        {
            EquipWeapon(_currentWeapon);
        }
    }

    /// <summary>
    /// Swaps the active weapon, recalculates dynamic stats based on saved upgrade levels,
    /// and handles the instantiation of the 3D visual model.
    /// </summary>
    public void EquipWeapon(WeaponData newWeapon)
    {
        _currentWeapon = newWeapon;

        // --- MEMORY CHECK ---
        // Ensure the weapon exists in the upgrade dictionary to prevent KeyNotFound exceptions
        if (!_weaponUpgradeLevels.ContainsKey(_currentWeapon.WeaponName))
        {
            _weaponUpgradeLevels[_currentWeapon.WeaponName] = 0;
        }

        // --- INVENTORY CHECK ---
        if (!_unlockedWeapons.Contains(newWeapon))
        {
            _unlockedWeapons.Add(newWeapon);
        }

        // Update the index to keep track of where we are in the list
        _currentWeaponIndex = _unlockedWeapons.IndexOf(newWeapon);

        // Pull the saved upgrade level from the dictionary
        _currentUpgradeLevel = _weaponUpgradeLevels[_currentWeapon.WeaponName];

        // Re-calculate the damage and fire rate based on saved level
        _currentDamage = _currentWeapon.Damage + (_currentUpgradeLevel * _currentWeapon.DamageIncreasePerLevel);
        _currentFireRate = _currentWeapon.FireRate - (_currentUpgradeLevel * _currentWeapon.FireRateDecreasePerLevel);

        // Clamp the fire rate to prevent negative cooldownds
        _currentFireRate = Mathf.Max(_currentFireRate, 0.05f); 

        // --- VISUAL MODEL SWAPPING ---
        // Clean up legacy geometry before allocating new memory
        if (_spawnedGunModel != null)
        {
            Destroy(_spawnedGunModel);
        }

        // Spawn the new gun & assume the default fire point is being used
        _activeFirePoint = _defaultFirePoint;

        if (_currentWeapon.WeaponModelPrefab != null && _weaponHolder != null)
        {
            // Instantiate as a child of the weapon holder, preserving local prefab transforms
            _spawnedGunModel =  Instantiate(_currentWeapon.WeaponModelPrefab, _weaponHolder, false);

            // Dynamically search for a child transform named "Barrel" to use as the new fire point
            Transform gunBarrel = _spawnedGunModel.transform.Find("Barrel");
            if (gunBarrel != null)
            {
                // If a specific barrel exists, override the default fire point
                _activeFirePoint = gunBarrel;
            }
        }

        _currentAmmo = _currentWeapon.MagSize;
        _isReloading = false;

        // Decoupled UI Update: Only update the UI if this component is attatched to the Player.
        if (gameObject.CompareTag("Player") && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateWeaponUI(_currentWeapon.WeaponIcon);
            UIManager.Instance.UpdateAmmo(_currentAmmo, _currentWeapon.MagSize);
        }
    }

    /// <summary>
    /// Applies progressive mathematical upgrades to the current weapon and stores the state.
    /// </summary>
    public void BuyUpgrade()
    {
        if (_currentUpgradeLevel < _currentWeapon.MaxUpgradeLevel)
        {
            _currentUpgradeLevel++;

            // Save the new weapon level back into the dictionary
            _weaponUpgradeLevels[_currentWeapon.WeaponName] = _currentUpgradeLevel;

            // Apply the maths
            _currentDamage += _currentWeapon.DamageIncreasePerLevel;
            _currentFireRate -= _currentWeapon.FireRateDecreasePerLevel;
            _currentFireRate = Mathf.Max(_currentFireRate, 0.05f);
        }
    }

    /// <summary>
    /// The primary public execution method. Can be invoked by UnityEvents,
    /// Player Input Systems, or AI Controllers.
    /// </summary>
    public void FireWeapon()
    {
        // SAFETY NET: Validate state before attempting to calculate firing logic
        if (_currentWeapon == null || _activeFirePoint == null || _isReloading) return;

        // AUTO-RELAOD: Automatically trigger the reload coroutine if the magazine is empty
        if (_currentAmmo <= 0 && !_hasUnlimitedAmmo)
        {
            Reload();
            return;
        }

        // COOLDOWN LOGIC: Restricts execution based on calculated fire rate
        if (Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + _currentFireRate;

            // Only consume a bullet IF has limited ammo
            // NOTE: A shotgun uses 1 "ammo" per pull of the trigger, even if it first 5 pellets
            if (!_hasUnlimitedAmmo)
            {
                _currentAmmo--;  // Consume 1 bullet
            }

            // --- PROJECTILE INSTANTIATION & SPREAD MATHDS ---
            for (int i = 0; i < _currentWeapon.ProjectilesPerShot; i++)
            {
                // Calculate the maths for an even spread
                float spread = 0f;

                // If firing multiple projectiles (e.g., Shotgun), calculate a uniform spread arc
                if (_currentWeapon.ProjectilesPerShot > 1)
                {
                    // This calculation spaces the bullets out evenly across the Spread Angle
                    float fraction = (float)i / (_currentWeapon.ProjectilesPerShot - 1);
                    // Lerp calculates the exact angle for each pellet across the total SpreadAngle
                    spread = Mathf.Lerp(-_currentWeapon.SpreadAngle / 2f, _currentWeapon.SpreadAngle / 2f, fraction);
                }

                // Apply the calculated Euler angle to the base rotation using Quaternion multiplication
                Quaternion bulletRotation = _activeFirePoint.rotation * Quaternion.Euler(0f, spread, 0f);

                // MEMORY OPTIMISATION: Retrieve a projectile from the Object Pool instead of instantiating a new one to reduce GC overhead and improve performance
                GameObject obj = ProjectilePool.Instance.GetProjectile(_currentWeapon.ProjectilePrefab, _activeFirePoint.position, bulletRotation);

                // Inject dynamic data (Damage and Origin Tag) into the spawned projectile
                CheeseProjectile projectileScript = obj.GetComponent<CheeseProjectile>();
                if (projectileScript != null)
                {
                    // Pass the tag of the GameObject holding this WeaponSystem (Player or Enemy)
                    projectileScript.Setup(gameObject.tag, _currentDamage);
                }
            }

            // Update the UI after firing BUT only if INFINITE is not being displayed
            if (gameObject.CompareTag("Player") && UIManager.Instance != null && !_hasUnlimitedAmmo)
            {
                UIManager.Instance.UpdateAmmo(_currentAmmo, _currentWeapon.MagSize);
            }

            AudioManager.Instance.PlayShootSound();
        }
    }

    // === AMMO MANAGEMENT ===
    public void Reload()
    {
        // Don't reload if already reloading, or if the mag is already full
        if (_isReloading || _currentAmmo == _currentWeapon.MagSize) return;

        StartCoroutine(ReloadRoutine());
    }

    /// <summary>
    /// Asynchronous coroutine handling reload delays without blocking the main thread.
    /// </summary>
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

    // --- DATA GETTERS ---
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

    // --- INVENTORY CYCLING ---
    public void CycleWeaponForward()
    {
        // Don't try to swap if player only has 1 gun
        if (_unlockedWeapons.Count <= 1 || _isReloading) return;

        _currentWeaponIndex++;

        // If we go past the end of the list, loop back to the first gun (0)
        if(_currentWeaponIndex >= _unlockedWeapons.Count)
        {
            _currentWeaponIndex = 0;
        }

        EquipWeapon(_unlockedWeapons[_currentWeaponIndex]);
    }

    public void CycleWeaponBackward()
    {
        // Don't try to swap if player only has 1 gun
        if (_unlockedWeapons.Count <= 1 || _isReloading) return;

        _currentWeaponIndex--;

        // If we go below 0, loop back to the very last gun in the list
        if (_currentWeaponIndex < 0)
        {
            _currentWeaponIndex = _unlockedWeapons.Count - 1;
        }

        EquipWeapon(_unlockedWeapons[_currentWeaponIndex]);
    }
}