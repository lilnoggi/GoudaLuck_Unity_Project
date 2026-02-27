using UnityEngine;
using TMPro;

/// <summary>
/// Handles the shop transactions, tracking ownership of weapons,
/// and updating the shop UI to reflect what the player has unlocked.
/// </summary>

public class ShopManager : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] private WeaponData _cheddarData;  // To swap back to starter gun
    [SerializeField] private WeaponData _mozzaData;    // Drop the Mozza-MP5 card here

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _mozzaUnlockText;  // The amount the mozza-mp5 costs
    [SerializeField] private GameObject _mozzaLockOverlay;      // Reference to the dark overlay panel

    // --- STATE TRACKING ---
    // The player always starts with the Cheddar-19, so they don't need to buy it
    private bool _ownsMozza = false;

    // ========================================================================
    
    // --- CHEDDAR-19 LOGIC ---

    // The Cheddar-19 Button calls this
    public void OnCheddarButtonClicked()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;
        WeaponSystem weapon = player.GetComponent<WeaponSystem>();
        if (weapon == null) return;

        // Always own the starter gun
        weapon.EquipWeapon(_cheddarData);
    }

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

    // ========================================================================

    // --- MOZZA-MP5 LOGIC ---

    // The "Buy Mozza-MP5" Button calls this
    public void OnBuyMozzaClicked()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;

        WeaponSystem weapon = player.GetComponent<WeaponSystem>();
        if (weapon == null) return;

        // SCENARIO 1: Already owns it - equip it
        if (_ownsMozza)
        {
            weapon.EquipWeapon(_mozzaData);
        }
        // SCENARIO 2: Doesn't own it yet
        else
        {
            // Try to spend the cost listed right on the data card
            if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_mozzaData.CheddarCost))
            {
                // Mark it as owned
                _ownsMozza = true;

                // Equip the new gun
                weapon.EquipWeapon(_mozzaData);
                Debug.Log("Successfully purchased the Mozza-MP5!");

                // Change the button text from "Buy (250) to "Equip"
                if (_mozzaUnlockText != null)
                {
                    _mozzaUnlockText.text = "Equip";
                }

                // Turn off the dark overlay so it looks unlocked
                if (_mozzaLockOverlay != null)
                {
                    _mozzaLockOverlay.SetActive(false);
                }

                Debug.Log("Successfully purchased and equipped the Mozza-MP5");
            }
            else
            {
                Debug.Log("Not enough Cheddar Points!");
            }
        }
    }

    // ========================================================================

    // --- WAVE LOGIC ---

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
