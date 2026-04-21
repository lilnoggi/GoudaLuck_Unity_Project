using UnityEngine;
using UnityEngine.SceneManagement;  // Required to change levels
using UnityEngine.EventSystems;     // Required for Controller support

/// <summary>
/// Manages the primary entry point of the application.
/// Ensures native gamepad support for the Steam Deck by forcing UI focus,
/// and handles safe scene transitions into the core gameplay loop.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Controller Support")]
    [Tooltip("The default UI button to highlight when the scene loads, ensuring gamepad accessibility without a mouse.")]
    [SerializeField] private GameObject _playButton;

    // =========================================================================================================================

    private void Start()
    {
        // --- STEAM DECK / GAMEPAD SUPPORT ---
        // By forcefully setting the selected GameObject in the EventSystem,
        // it is guaranteed that the player can navigate the UI immediately using a D-Pad or joystick 
        // without needing to click with a mouse first.
        if (EventSystem.current != null && _playButton != null)
        {
            // Clear any existing selection to ensure the new selection is registered correctly
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_playButton);
        }
    }

    /// <summary>
    /// Executes the transition from the Main Menu into the narrative intro.
    /// Designed to be triggered via Unity UI Button OnClick events.
    /// </summary>
    public void PlayGame()
    {
        // Provide immediate audio feedback
        AudioManager.Instance.PlaySelectButtonSound();
        
        // DEFENSIVE PROGRAMMING: Reset the global time scale.
        // If the player quit to the main menu while the game was paused, the time scale
        // would remian at 0f, causing the newly loaded scene to be completely frozen.
        Time.timeScale = 1f;

        // Load the narrative presentation layer
        SceneManager.LoadScene("IntroCutsceneScene");
    }

    /// <summary>
    /// Safely terminates the application.
    /// </summary>
    public void QuitGame()
    {
        AudioManager.Instance.PlaySelectButtonSound();

        // Ignored in built .exe, but useful for debugging in the editor
        Debug.Log("Quitting the game...");

        // Send the termination signal to the OS.
        // NOTE: In the Unity Editor, this will not stop play mode. Instead, it will log the quit action.
        Application.Quit();
    }
}
