using UnityEngine;

/// <summary>
/// Defines the specific mathematical modifier applied to the player's stats.
/// </summary>
public enum UpgradeType { MaxHealth, MoveSpeed, UltCooldown, DashCooldown, Armour }

/// <summary>
/// A centralised, immutable data container for roguelite player upgrades.
/// Utilises the ScriptableObject architecture to enable Data-Driven Design,
/// allowing designers to create and balance new powerups without modifying code.
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerUpgrade", menuName = "Gouda Luck/Player Upgrade")]
public class PlayerUpgradeData : ScriptableObject
{
    [Header("UI Info")]
    [Tooltip("The display name shown on the upgrade card.")]
    [SerializeField] private string _upgradeName;
    [Tooltip("The descriptive text explaining the modifier to the player.")]
    [TextArea][SerializeField] private string _upgradeDescription;
    [Tooltip("The 2D Sprite rendered on the upgrade card.")]
    [SerializeField] private Sprite _upgradeIcon;

    [Header("Stat Logic")]
    [Tooltip("The specific stat category this upgrade modifies.")]
    [SerializeField] private UpgradeType _upgradeType;
    [Tooltip("The numerical value added to (or subtracted from) the player's base stats.")]
    [SerializeField] private float _upgradeValue;  // Amount to + or - from the player's stats

    // ==========================================================================================
    // ========================== --- ENCAPSULATED PUBLIC GETTERS ---  ==========================
    // ==========================================================================================
    // ===== Provides read-only access to external scripts, preserving ==========================
    // ===== the integrity of the base ScriptableObject data.          ==========================
    // ==========================================================================================
    public string UpgradeName => _upgradeName;
    public string UpgradeDescription => _upgradeDescription;
    public Sprite UpgradeIcon => _upgradeIcon;
    public UpgradeType UpgradeType => _upgradeType;
    public float UpgradeValue => _upgradeValue;
}
