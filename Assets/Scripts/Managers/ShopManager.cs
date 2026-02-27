using UnityEngine;

/// <summary>
/// 
/// </summary>

public class ShopManager : MonoBehaviour
{
    [Header("New Weapons")]
    [SerializeField] private WeaponData _mozzaData;  // Drop the Mozza-MP5 card here

    // The Upgrade Button calls this
    public void OnUpgradeClicked()
    {
        // Find the Player and their Weapon
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        WeaponSystem weapon = player.GetComponent<WeaponSystem>();
        if (weapon == null) return;

        // Check if they are already max level
        if (weapon.GetUpgradeLevel() >= 3)
        {
            Debug.Log("Weapon is already Max Level!");
            return;
        }

        // Try to spend 50 points (pull from WeaponData later)
        if (GameManager.Instance != null && GameManager.Instance.SpendPoints(50))
        {
            weapon.BuyUpgrade();
            Debug.Log("Upgrade successful");
            
            // SFX go here
        }
        else
        {
            Debug.Log("Not enough cheddar points!");
        }
    }

    // The "Buy Mozza-MP5" Button calls this
    public void OnBuyMozzaClicked()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        WeaponSystem weapon = player.GetComponent<WeaponSystem>();
        if (weapon == null) return;

        // Try to spend the cost listed right on the data card
        if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_mozzaData.CheddarCost))
        {
            // Equip the new gun
            weapon.EquipWeapon(_mozzaData);
            Debug.Log("Successfully purchased the Mozza-MP5!");

            // NOTE: Hide the buy button here so they don't buy it again.
        }
        else
        {
            Debug.Log("Not enough Cheddar Points!");
        }
    }

    // The Next Wave Button calls this
    public void OnNextWaveClicked()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideShop();
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartNextWave();
        }
    }
}
