using UnityEngine;

/// <summary>
/// A centralised, immutable data container for weapon configurations.
/// Utilises the ScriptableObject architecture to enable Data-Driven Design.
/// Fully encapsulated to prevent accidental data mutation at runtime, ensuring 
/// safe access across multiple interchangeable WeaponSystem components.
/// </summary>

[CreateAssetMenu(fileName = "New Weapon", menuName = "Gouda Luck/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("UI Info")]
    [Tooltip("The display name shown in the Shop and HUD.")]
    [SerializeField] private string _weaponName;
    [Tooltip("The 2D Sprite rendered in the Shop and UI panels.")]
    [SerializeField] private Sprite _weaponIcon;
    [Tooltip("The initial cost in Cheddar Points.")]
    [SerializeField] private int _cheddarCost;    // How much it costs to unlock

    [Header("Base Combat Stats")]
    [Tooltip("The minimum delay (in seconds) between each shot.")]
    [SerializeField] private float _fireRate;
    [Tooltip("The base damage inflicted per projectile.")]
    [SerializeField] private float _damage;
    [Tooltip("The number of shots fired before requiring a reload.")]
    [SerializeField] private int _magSize;
    [Tooltip("The duration (in seconds) it takes to complete a reload cycle.")]
    [SerializeField] private float _reloadTime;

    // --- SHOTGUN SPREAD SETTINGS ---
    [Header("Shotgun Settings")]
    [Tooltip("The number of individual bullets instantiated per trigger pull. Default is 1.")]
    [SerializeField] private int _projectilesPerShot = 1;  // Default to 1 bullet
    [Tooltip("The total arc (in degrees) across which the projectiles will be evenly disperesed.")]
    [SerializeField] private float _spreadAngle = 0f;      // Default to straight ahead

    [Header("Upgrade Stats")]
    [Tooltip("The maximum number of times *this* weapon can be upgraded in the Shop.")]
    [SerializeField] private int _maxUpgradeLevel = 3;
    [Tooltip("The cost to purchase a single upgrade tier.")]
    [SerializeField] private int _upgradeCost = 50;
    [Tooltip("The flat amount of damage added per upgrade level.")]
    [SerializeField] private float _damageIncreasePerLevel = 5f;
    [Tooltip("The amount subtracted from the fire rate per level (Lower FireRate = Faster Shooting).")]
    [SerializeField] private float _fireRateDecreasePerLevel = 0.05f;  // Subtract from fire rate to shoot faster

    [Header("Visuals")]
    [SerializeField] private GameObject _weaponModelPrefab;  // The 3D gun model

    [Header("References")]
    [SerializeField] private GameObject _projectilePrefab;

    // ==========================================================================================
    // ========================== --- ENCAPSULATED PUBLIC GETTERS ---  ==========================
    // ==========================================================================================
    // ===== Provides read-only access to external scripts, preserving ==========================
    // ===== the integrity of the base ScriptableObject data.          ==========================
    // ==========================================================================================
    public string WeaponName => _weaponName;
    public Sprite WeaponIcon => _weaponIcon;
    public int CheddarCost => _cheddarCost;
    public float FireRate => _fireRate;
    public float Damage => _damage;
    public int MagSize => _magSize;
    public float ReloadTime => _reloadTime;
    public int ProjectilesPerShot => _projectilesPerShot;
    public float SpreadAngle => _spreadAngle;
    public int MaxUpgradeLevel => _maxUpgradeLevel;
    public int UpgradeCost => _upgradeCost;
    public float DamageIncreasePerLevel => _damageIncreasePerLevel;
    public float FireRateDecreasePerLevel => _fireRateDecreasePerLevel;
    public GameObject ProjectilePrefab => _projectilePrefab;
    public GameObject WeaponModelPrefab => _weaponModelPrefab;
}
