using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The presentation layer (View) for an individual Player Uprgade card.
/// Designed to be attatched to the visual child of the dynamically instantiated layout prefab.
/// Reads the assigned ScriptableObject data and updates its own UI components.
/// </summary>
public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("The text component displaying the title of the upgrade.")]
    [SerializeField] private TextMeshProUGUI _nameText;
    [Tooltip("The text component explaining the mechanical effect of the upgrade.")]
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [Tooltip("The image component displaying the upgrade's icon.")]
    [SerializeField] private Image _iconImage;

    // --- STATE TRACKING ---
    // Stores a local reference to the assigned data so it can be passed back to the Manager when clicked.
    private PlayerUpgradeData _storedData;

    // ==============================================================================================================

    /// <summary>
    /// Inserts the data from the chosen ScriptableObject into the visual UI elements.
    /// Called dynamically by the PlayerUpgradeManager during the instantiation phase.
    /// </summary>
    public void SetupCard(PlayerUpgradeData data)
    {
        _storedData = data;

        // Update the visual presentation safely
        if (_nameText != null) _nameText.text = data.UpgradeName;
        if (_descriptionText != null) _descriptionText.text = data.UpgradeDescription;
        if (_iconImage != null) _iconImage.sprite = data.UpgradeIcon;
    }

    /// <summary>
    /// Event listener triggered by the Unity UI Button's OnClick event.
    /// Passes the stored data back to the global Manager to apply the mathematic modifiers.
    /// </summary>
    public void OnCardClicked()
    {
        // Tell the Upgrade Manager that this specific card was picekd
        if (PlayerUpgradeManager.Instance != null)
        {
            PlayerUpgradeManager.Instance.SelectUpgrade(_storedData);
        }
    }
}
