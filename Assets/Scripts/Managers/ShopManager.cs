using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private WeaponData _shotgunData;  // Drop the Shotgun data card here

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _mozzaUnlockText;  // The amount the mozza-mp5 costs
    [SerializeField] private GameObject _mozzaLockOverlay;      // Reference to the dark overlay panel
    [SerializeField] private TextMeshProUGUI _shotgunUnlockText;
    [SerializeField] private GameObject _shotgunLockOverlay;

    [Header("Upgrade Dots")]
    [SerializeField] private Image[] _cheddarDots;
    [SerializeField] private Image[] _mozzaDots;
    [SerializeField] private Image[] _shotgunDots;
    [SerializeField] private Color _filledColour = Color.yellow;
    [SerializeField] private Color _emptyColour = Color.black;

    // --- STATE TRACKING ---
    // The player always starts with the Cheddar-19, so they don't need to buy it
    private bool _ownsMozza = false;
    private bool _ownsShotgun = false;

    // ========================================================================

    // --- UI Refresh ---
    // This runs automatically every single time the Shop Panel is turned on
    private void OnEnable()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;
        WeaponSystem weapon = player.GetComponent<WeaponSystem>();
        if (weapon == null) return;

        // Peek at the filing cabinet and colour the dots
        UpdateDotsUI(_cheddarDots, weapon.GetSpecificWeaponLevel(_cheddarData.WeaponName));
        UpdateDotsUI(_mozzaDots, weapon.GetSpecificWeaponLevel(_mozzaData.WeaponName));
        UpdateDotsUI(_shotgunDots, weapon.GetSpecificWeaponLevel(_shotgunData.WeaponName));
    }

    private void UpdateDotsUI(Image[] dotsArray, int currentLevel)
    {
        for (int i = 0; i < dotsArray.Length; i++)
        {
            // If the dot's slot number is lower than the level, make it yellow
            if (i < currentLevel) dotsArray[i].color = _filledColour;
            else dotsArray[i].color = _emptyColour;
        }
    }

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
    public void OnUpgradeCheddarClicked()
    {
        // Force the player to equip the Cheddar-19 first so the maths apply to the right gun
        OnCheddarButtonClicked();

        WeaponSystem weapon = FindFirstObjectByType<PlayerController>().GetComponent<WeaponSystem>();
        if (weapon.GetUpgradeLevel() >= _cheddarData.MaxUpgradeLevel) return;

        // Try to spend points using the cost on the data card
        if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_cheddarData.UpgradeCost))
        {
            weapon.BuyUpgrade();
            UpdateDotsUI(_cheddarDots, weapon.GetUpgradeLevel());
        }
        else
        {
            // Debug.Log("Not enough cheddar points!");
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
               // Debug.Log("Successfully purchased the Mozza-MP5!");

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

                // Debug.Log("Successfully purchased and equipped the Mozza-MP5");
            }
            else
            {
                // Debug.Log("Not enough Cheddar Points!");
            }
        }
    }

    public void OnUpgradeMozzaClicked()
    {
        // Don't allow upgrades if the gun is not bought yet
        if (!_ownsMozza) return;

        // Force equip the Mozza first
        OnBuyMozzaClicked();

        WeaponSystem weapon = FindFirstObjectByType<PlayerController>().GetComponent<WeaponSystem>();
        if (weapon.GetUpgradeLevel() >= _mozzaData.MaxUpgradeLevel) return;

        if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_mozzaData.UpgradeCost))
        {
            weapon.BuyUpgrade();
            UpdateDotsUI(_mozzaDots, weapon.GetUpgradeLevel());
        }
    }

    // ========================================================================

    // --- SHOTGUN LOGIC ---

    public void OnBuyShotgunClicked()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null) return;
        WeaponSystem weapon = player.GetComponent<WeaponSystem>();

        if (_ownsShotgun)
        {
            weapon.EquipWeapon(_shotgunData);
        }
        else
        {
            if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_shotgunData.CheddarCost))
            {
                _ownsShotgun = true;
                weapon.EquipWeapon(_shotgunData);
                if (_shotgunUnlockText != null)
                {
                    _shotgunUnlockText.text = "Equip";
                }

                if (_shotgunLockOverlay != null)
                {
                    _shotgunLockOverlay.SetActive(false);
                }
            }
        }
    }

    public void OnUpgradeShotgunClicked()
    {
        if (!_ownsShotgun) return;

        OnBuyShotgunClicked();  // Force equip

        WeaponSystem weapon = FindFirstObjectByType<PlayerController>().GetComponent<WeaponSystem>();

        if (weapon.GetUpgradeLevel() >= _shotgunData.MaxUpgradeLevel) return;

        if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_shotgunData.UpgradeCost))
        {
            weapon.BuyUpgrade();
            UpdateDotsUI(_shotgunDots, weapon.GetUpgradeLevel());
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
