using UnityEngine;

/// <summary>
/// A data container for the weapons.
/// The CreateAssetMenu attribute allows you to right-click in the Unity Project folder
/// and create a new Weapon exactly like creating a new Material or Prefab
/// Fully encapsulated to prevent accidental data overwrites at runtime.
/// </summary>

[CreateAssetMenu(fileName = "New Weapon", menuName = "Gouda Luck/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("UI Info")]
    [SerializeField] private string _weaponName;
    [SerializeField] private Sprite _weaponIcon;  // For the shop menu
    [SerializeField] private int _cheddarCost;    // How much it costs to unlock

    [Header("Base Combat Stats")]
    [SerializeField] private float _fireRate;
    [SerializeField] private float _damage;

    [Header("Upgrade Stats")]
    [SerializeField] private int _maxUpgradeLevel = 3;
    [SerializeField] private int _upgradeCost = 50;
    [SerializeField] private float _damageIncreasePerLevel = 5f;
    [SerializeField] private float _fireRateDecreasePerLevel = 0.05f;  // Subtract from fire rate to shoot faster

    [Header("Visuals")]
    [SerializeField] private GameObject _weaponModelPrefab;  // The 3D gun model

    [Header("References")]
    [SerializeField] private GameObject _projectilePrefab;

    // The Public Getters
    public string WeaponName => _weaponName;
    public Sprite WeaponIcon => _weaponIcon;
    public int CheddarCost => _cheddarCost;
    public float FireRate => _fireRate;
    public float Damage => _damage;
    public int MaxUpgradeLevel => _maxUpgradeLevel;
    public int UpgradeCost => _upgradeCost;
    public float DamageIncreasePerLevel => _damageIncreasePerLevel;
    public float FireRateDecreasePerLevel => _fireRateDecreasePerLevel;
    public GameObject ProjectilePrefab => _projectilePrefab;
    public GameObject WeaponModelPrefab => _weaponModelPrefab;
}
