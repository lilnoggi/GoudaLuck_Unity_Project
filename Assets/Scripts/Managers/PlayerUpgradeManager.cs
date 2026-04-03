using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages the randomised player upgrade screen, spawns the UI cards,
/// and applies the selected stats to the player.
/// </summary>

public class PlayerUpgradeManager : MonoBehaviour
{
    public static PlayerUpgradeManager Instance { get; private set; }

    [Header("Upgrade Data")]
    [SerializeField] private PlayerUpgradeData[] _allPossibleUpgrades;  // Drag the ScriptableObjects here!

    [Header("UI References")]
    [SerializeField] private GameObject _upgradePanel;       // The main panel
    [SerializeField] private Transform _cardContainer;       // The HorizontalLayoutGroup that holds the cards
    [SerializeField] private GameObject _upgradeCardPrefab;  // The Master Card Prefab

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- TRIGGER THE UPGRADE SCREEN ---
    // The WaveManager will call this instead of UIManager
    public void ShowUpgradeScreen()
    {
        _upgradePanel.SetActive(true);
        Time.timeScale = 0;  // Pause game

        // Clear any old cards from the previous upgrade screen
        foreach (Transform child in _cardContainer)
        {
            Destroy(child.gameObject);
        }

        // Pick 3 random UNIQUE upgrades
        List<PlayerUpgradeData> pickedUpgrades = GetRandomUpgrades(3);

        GameObject firstCard = null;  // Save this for Controller Support

        // Spawn the cards and use the data
        foreach (PlayerUpgradeData data in pickedUpgrades)
        {
            GameObject newCard = Instantiate(_upgradeCardPrefab, _cardContainer);

            // Tell the card to set up its text and image
            newCard.GetComponentInChildren<UpgradeCardUI>().SetupCard(data);

            if (firstCard == null)
            {
                firstCard = newCard;
            }

            // Controller Support
            EventSystem.current.SetSelectedGameObject(null);
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null && !player.IsUsingMouse && firstCard != null)
            {
                // Force the controller to highlight the far-left card
                EventSystem.current.SetSelectedGameObject(firstCard);
            }
        }
    }

    // --- APPLY UPGRADE ---
    // The UpgradeCardUI script calls this when the player clicks a button
    public void SelectUpgrade(PlayerUpgradeData selectedData)
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        HealthSystem health = player.GetComponent<HealthSystem>();

        // Switch statement based on the type of upgrade
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

        // UI sound here
        AudioManager.Instance.PlayPowerupPickupSound();

        // Close the upgrade screen
        _upgradePanel.SetActive(false);

        // Immediately open the Shop
        UIManager.Instance.ShowShop();
    }

    // --- HELPER METHOD: PICK 3 UNIQUE CARDS ---
    private List<PlayerUpgradeData> GetRandomUpgrades(int count)
    {
        List<PlayerUpgradeData> pool = new List<PlayerUpgradeData>(_allPossibleUpgrades);
        List<PlayerUpgradeData> chosen = new List<PlayerUpgradeData>();

        // Loop until 3 cards have been chosen (or until no more options)
        while (chosen.Count < count && pool.Count > 0)
        {
            int randomIndex = Random.Range(0, pool.Count);
            chosen.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);  // Remove so it can't be picked twice
        }

        return chosen;
    }
}
