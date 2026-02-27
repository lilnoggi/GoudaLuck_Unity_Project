using UnityEngine;

/// <summary>
/// 
/// </summary>

public class ShopManager : MonoBehaviour
{
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
