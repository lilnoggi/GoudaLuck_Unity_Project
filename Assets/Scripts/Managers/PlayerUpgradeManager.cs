using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the data-driven roguelite upgrade screen.
/// Dynamically populates UI cards using ScriptableObjects and applies
/// mathematically calculated stat modifiers to the player entity.
/// </summary>

public class PlayerUpgradeManager : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static PlayerUpgradeManager Instance { get; private set; }

    [Header("Upgrade Data")]
    [Tooltip("Array of all possible upgrade ScriptableObjects. The manager will randomly select from this pool.")]
    [SerializeField] private PlayerUpgradeData[] _allPossibleUpgrades;  // Drag the ScriptableObjects here!

    [Header("UI References")]
    [Tooltip("The root UI canvas panel for the upgrade screen.")]
    [SerializeField] private GameObject _upgradePanel;  
    [Tooltip("The Auto Layout Group acting as the spatial container for the cards.")]     
    [SerializeField] private Transform _cardContainer;       
    [Tooltip("The UI prefab designed with Spatial Partitioning (separated layout and visual layers).")]
    [SerializeField] private GameObject _upgradeCardPrefab;  

    // =================================================================================================================

    private void Awake()
    {
        // Enforce the Singleton pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Stops gameplay and generates a randomised selection of unique upgrades.
    /// Triggered globally by the WaveManager upon wave completion.
    /// </summary>
    public void ShowUpgradeScreen()
    {
        _upgradePanel.SetActive(true);

        // Stop physics and gameplay loops (pause the game)
        Time.timeScale = 0;  

        // MEMORY CLEANUP: Destroy legacy cards from the previous wave
        foreach (Transform child in _cardContainer)
        {
            Destroy(child.gameObject);
        }

        // Pick 3 random UNIQUE upgrades
        List<PlayerUpgradeData> pickedUpgrades = GetRandomUpgrades(3);

        GameObject firstCard = null;  // Cached for UI hardware abstraction (Controller Support)

        // --- DYNAMIC UI INSTANTATION ---
        foreach (PlayerUpgradeData data in pickedUpgrades)
        {
            // Instantiate the spatial layout prefab into the Canvas group
            GameObject newCard = Instantiate(_upgradeCardPrefab, _cardContainer);

            // Put the SccriptableObject data into the card's visual layer
            newCard.GetComponentInChildren<UpgradeCardUI>().SetupCard(data);

            // Cache the very first card spawned
            if (firstCard == null)
            {
                firstCard = newCard;
            }
        }

        // --- HARDWARE ABSTRACTION (CONTROLLER SUPPORT) ---
        // Executed once after all cards are instantiated.
        // GameObject.FindGameObjectWithTag is used as a slightly faster alternative to FindObjectsFirstByType.
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController player = playerObj.GetComponent<PlayerController>();

            // If the player is using a gamepad, force UI focus onto the left-most card
            if (player != null && !player.IsUsingMouse && firstCard != null)
            {
                // Force the controller to highlight the far-left card
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(firstCard);
            }
        }
    }

    /// <summary>
    /// Executes the logic for the chosen upgrade.
    /// Called via UnityEvents on the UI Button components.
    /// </summary>
    public void SelectUpgrade(PlayerUpgradeData selectedData)
    {
        // Locate the target entity to apply modifiers
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;  // Defensive check

        PlayerController player = playerObj.GetComponent<PlayerController>();
        HealthSystem health = playerObj.GetComponent<HealthSystem>();

        // Evaluate the enum type defined in the ScriptableObject and apply the correct mathematical modifier
        switch (selectedData.UpgradeType)
        {
            case UpgradeType.MaxHealth:
                health.IncreaseMaxHealth(selectedData.UpgradeValue);
                break;
            case UpgradeType.MoveSpeed:
                player.IncreaseMoveSpeed(selectedData.UpgradeValue);
                break;
            case UpgradeType.DashCooldown:
                player.DecreaseDashCooldown(selectedData.UpgradeValue);
                break;
            case UpgradeType.UltCooldown:
                player.IncreaseUltChargeRate(selectedData.UpgradeValue);
                break;
            case UpgradeType.Armour:
                health.AddArmour(selectedData.UpgradeValue);
                break;
        }

        // Trigger Audio Feedback
        AudioManager.Instance.PlayPowerupPickupSound();

        // Close the presentation layer
        _upgradePanel.SetActive(false);

        // Transition the game state to the Shop phase
        UIManager.Instance.ShowShop();
    }

    /// <summary>
    /// A helper algorithm to extract a specific number of unique items from the global pool.
    /// </summary>
    private List<PlayerUpgradeData> GetRandomUpgrades(int count)
    {
        // Create a temporary clone of the master list so items can be removed
        List<PlayerUpgradeData> pool = new List<PlayerUpgradeData>(_allPossibleUpgrades);
        List<PlayerUpgradeData> chosen = new List<PlayerUpgradeData>();

        // Loop until 3 cards have been chosen (or until no more options)
        while (chosen.Count < count && pool.Count > 0)
        {
            int randomIndex = Random.Range(0, pool.Count);

            // Add the selected item to the output list
            chosen.Add(pool[randomIndex]);

            // Remove the item from the temporary pool to mathematically guarantee uniqueness
            pool.RemoveAt(randomIndex); 
        }

        return chosen;
    }
}
