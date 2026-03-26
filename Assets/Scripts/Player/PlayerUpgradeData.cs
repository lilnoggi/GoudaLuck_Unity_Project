using UnityEngine;

/// <summary>
/// This script...
/// </summary>

public enum UpgradeType { MaxHealth, MoveSpeed, UltCooldown, DashCooldown, Armour }

[CreateAssetMenu(fileName = "NewPlayerUpgrade", menuName = "Gouda Luck/Player Upgrade")]
public class PlayerUpgradeData : ScriptableObject
{
    [Header("UI Info")]
    [SerializeField] private string _upgradeName;
    [TextArea][SerializeField] private string _upgradeDescription;
    [SerializeField] private Sprite _upgradeIcon;

    [Header("Stat Logic")]
    [SerializeField] private UpgradeType _upgradeType;
    [SerializeField] private float _upgradeValue;  // Amount to + or - from the player's stats
}
