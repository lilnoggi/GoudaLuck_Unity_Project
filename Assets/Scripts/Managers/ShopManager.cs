using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A dedicated Controller script for the Shop presentaiton layer.
/// Manages transaction logic, tracks weapon ownership state, and dynamically updates
/// the UI to reflect the player's current progression.
/// </summary>

public class ShopManager : MonoBehaviour
{
    [Header("Weapon Data")]
    [Tooltip("The ScriptableObject for the default starting weapon, the Cheddar-19.")]
    [SerializeField] private WeaponData _cheddarData; 
    [Tooltip("The ScriptableObject for the SMG weapon variant, the Mozza-MP5.")]
    [SerializeField] private WeaponData _mozzaData;  
    [Tooltip("The ScriptableObject for the Shotgun weapon variant, the Shotgun Swiss.")]
    [SerializeField] private WeaponData _shotgunData; 

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _mozzaUnlockText;  // The amount the mozza-mp5 costs
    [SerializeField] private GameObject _mozzaLockOverlay;      // Reference to the dark overlay panel
    [SerializeField] private TextMeshProUGUI _shotgunUnlockText;
    [SerializeField] private GameObject _shotgunLockOverlay;

    [Header("Upgrade Dots")]
    [Tooltip("Array of Image components representing the upgrade pip UI for each weapon.")]
    [SerializeField] private Image[] _cheddarDots;
    [SerializeField] private Image[] _mozzaDots;
    [SerializeField] private Image[] _shotgunDots;
    [SerializeField] private Color _filledColour = Color.yellow;
    [SerializeField] private Color _emptyColour = Color.black;

    // --- STATE TRACKING ---
    // The player inherently owns the Cheddar-19, so it does not require an unlock flag
    private bool _ownsMozza = false;
    private bool _ownsShotgun = false;

    // --- COMPONENT CACHING ---
    // Cached to avoid expensive FindFirstObjectByType calls every time a button is clicked
    private WeaponSystem _cachedPlayerWeapon;

    // =================================================================================================================

    // --- UI LIFECYCLE ---
    private void OnEnable()
    {
        // Cache the player reference once when the UI panel is activated
        if (_cachedPlayerWeapon == null)
        {
            PlayerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
            if (player != null)
            {
                _cachedPlayerWeapon = player.GetComponent<WeaponSystem>();
            }
        }

        // Defensive check
        if (_cachedPlayerWeapon == null) return;

        // Synchronise the visual UI dots with the mathematical upgrade levels stored in the WeaponSystem
        UpdateDotsUI(_cheddarDots, _cachedPlayerWeapon.GetSpecificWeaponLevel(_cheddarData.WeaponName));
        UpdateDotsUI(_mozzaDots, _cachedPlayerWeapon.GetSpecificWeaponLevel(_mozzaData.WeaponName));
        UpdateDotsUI(_shotgunDots, _cachedPlayerWeapon.GetSpecificWeaponLevel(_shotgunData.WeaponName));
    }

    /// <summary>
    /// Dynamically colours an array of UI images based on the current mathematical upgrade level.
    /// </summary>
    private void UpdateDotsUI(Image[] dotsArray, int currentLevel)
    {
        for (int i = 0; i < dotsArray.Length; i++)
        {
            // If the array index is lower than the player's level, mark it as purchased
            if (i < currentLevel) dotsArray[i].color = _filledColour;
            else dotsArray[i].color = _emptyColour;
        }
    }

    // ==============================================================================================================

    // --- CHEDDAR-19 LOGIC ---
    // The Cheddar-19 Button calls this
    public void OnCheddarButtonClicked()
    {
        if (_cachedPlayerWeapon == null) return;

        AudioManager.Instance.PlaySelectButtonSound();

        // The starter weapon is always available to equip
        _cachedPlayerWeapon.EquipWeapon(_cheddarData);
    }

    // The Cheddar-19 Upgrade Button calls this
    public void OnUpgradeCheddarClicked()
    {
        if (_cachedPlayerWeapon == null) return;

        // Force the player to equip the Cheddar-19 first so the modifiers apply to the correct instance
        OnCheddarButtonClicked();

        // Validate max level bounds
        if (_cachedPlayerWeapon.GetUpgradeLevel() >= _cheddarData.MaxUpgradeLevel) return;

        // Transaction logic
        if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_cheddarData.UpgradeCost))
        {
            AudioManager.Instance.PlayUpgradeWeaponSound();
            _cachedPlayerWeapon.BuyUpgrade();
            UpdateDotsUI(_cheddarDots, _cachedPlayerWeapon.GetUpgradeLevel());
        }
        else
        {
            AudioManager.Instance.PlayPurchaseFailedSound();
        }
    }

    // ==============================================================================================================

    // --- MOZZA-MP5 LOGIC ---
    // The "Buy Mozza-MP5" Button calls this
    public void OnBuyMozzaClicked()
    {
        if (_cachedPlayerWeapon == null) return;

        // SCENARIO 1: Weapon already unlocked. Equip it.
        if (_ownsMozza)
        {
            AudioManager.Instance.PlaySelectButtonSound();
            _cachedPlayerWeapon.EquipWeapon(_mozzaData);
        }
        // SCENARIO 2: Initial Purchase
        else
        {
            // Try to spend the cost listed right on the data card
            if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_mozzaData.CheddarCost))
            {
                AudioManager.Instance.PlayPurchaseGunSound();
                
                // Set unlock flag
                _ownsMozza = true;

                // Equip immediately upon purhcase
                _cachedPlayerWeapon.EquipWeapon(_mozzaData);

                // Update Presentation Layer (Change price tag to "Equip" and remove dark overlay)
                if (_mozzaUnlockText != null)
                {
                    _mozzaUnlockText.text = "Equip";
                }

                // Turn off the dark overlay so it looks unlocked
                if (_mozzaLockOverlay != null)
                {
                    _mozzaLockOverlay.SetActive(false);
                }
            }
            else
            {
                // Purchase failed - not enough Cheddar Points
                AudioManager.Instance.PlayPurchaseFailedSound();
            }
        }
    }

    // Mozza Upgrade Button calls this
    public void OnUpgradeMozzaClicked()
    {
        // Guard Clause: Prevent upgrading a weapon the player does not own
        if (!_ownsMozza || _cachedPlayerWeapon == null) return;

        // Force equip to ensure the correct modifier application
        OnBuyMozzaClicked();

        if (_cachedPlayerWeapon.GetUpgradeLevel() >= _mozzaData.MaxUpgradeLevel) return;

        if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_mozzaData.UpgradeCost))
        {
            AudioManager.Instance.PlayUpgradeWeaponSound();
            _cachedPlayerWeapon.BuyUpgrade();
            UpdateDotsUI(_mozzaDots, _cachedPlayerWeapon.GetUpgradeLevel());
        }
        else
        {
            AudioManager.Instance.PlayPurchaseFailedSound();
        }
    }

    // ==============================================================================================================

    // --- SHOTGUN LOGIC ---
    public void OnBuyShotgunClicked()
    {
        if (_cachedPlayerWeapon == null) return;

        // Weapon already unlocked
        if (_ownsShotgun)
        {
            AudioManager.Instance.PlaySelectButtonSound();
            _cachedPlayerWeapon.EquipWeapon(_shotgunData);
        }
        // Initial Purchase
        else
        {
            if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_shotgunData.CheddarCost))
            {
                AudioManager.Instance.PlayPurchaseGunSound();

                _ownsShotgun = true;
                _cachedPlayerWeapon.EquipWeapon(_shotgunData);

                // Update Presentation Layer
                if (_shotgunUnlockText != null)
                {
                    _shotgunUnlockText.text = "Equip";
                }

                if (_shotgunLockOverlay != null)
                {
                    _shotgunLockOverlay.SetActive(false);
                }
            }
            else
            {
                AudioManager.Instance.PlayPurchaseFailedSound();
            }
        }
    }

    public void OnUpgradeShotgunClicked()
    {
        if (!_ownsShotgun || _cachedPlayerWeapon == null) return;

        OnBuyShotgunClicked();  // Force equip

        if (_cachedPlayerWeapon.GetUpgradeLevel() >= _shotgunData.MaxUpgradeLevel) return;

        if (GameManager.Instance != null && GameManager.Instance.SpendPoints(_shotgunData.UpgradeCost))
        {
            AudioManager.Instance.PlayUpgradeWeaponSound();
            _cachedPlayerWeapon.BuyUpgrade();
            UpdateDotsUI(_shotgunDots, _cachedPlayerWeapon.GetUpgradeLevel());
        }
        else
        {
            AudioManager.Instance.PlayPurchaseFailedSound();
        }
    }

    // ==============================================================================================================

    // --- WAVE LOGIC ---
    // The Next Wave Button calls this
    public void OnNextWaveClicked()
    {
        AudioManager.Instance.PlaySelectButtonSound();

        // Dismiss the Shop UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideShop();
        }

        // Trigger the core gameplay loop to resume
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.StartNextWave();
        }
    }
}
