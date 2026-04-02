using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attatched to the blank Master Card prefab.
/// Reads the chosen Upgrade ScriptableObject and visually updates iteself
/// </summary>

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Image _iconImage;

    // Store the data so the card remembers what it is holding
    private PlayerUpgradeData _storedData;

    // The Upgrade Manager will call this method and pass in the data
    public void SetupCard(PlayerUpgradeData data)
    {
        _storedData = data;

        // Update the visual UI
        _nameText.text = data.UpgradeName;
        _descriptionText.text = data.UpgradeDescription;
        _iconImage.sprite = data.UpgradeIcon;
    }

    // Link this to the Button's "On-Click" Event in Inspector
    public void OnCardClicked()
    {
        // Tell the Upgrade Manager that this specific card was picekd
        if (PlayerUpgradeManager.Instance != null)
        {
            PlayerUpgradeManager.Instance.SelectUpgrade(_storedData);
        }
    }
}
