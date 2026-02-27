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

    [Header("Combat Stats")]
    [SerializeField] private float _fireRate;
    [SerializeField] private float _damage;

    [Header("References")]
    [SerializeField] private GameObject _projectilePrefab;

    // The Public Getters
    public string WeaponName => _weaponName;
    public Sprite WeaponIcon => _weaponIcon;
    public int CheddarCost => _cheddarCost;
    public float FireRate => _fireRate;
    public float Damage => _damage;
    public GameObject ProjectilePrefab => _projectilePrefab;
}
