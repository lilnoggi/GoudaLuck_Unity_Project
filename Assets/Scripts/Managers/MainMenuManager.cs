using UnityEngine;
using UnityEngine.SceneManagement;  // Required to change levels
using UnityEngine.EventSystems;     // Required for Controller support

public class MainMenuManager : MonoBehaviour
{
    [Header("Controller Support")]
    [SerializeField] private GameObject _playButton;

    private void Start()
    {
        // --- STEAM DECK / GAMEPAD SUPPORT ---
        // As soon as the menu loads, force the controller to highlight the play button
        if (EventSystem.current != null && _playButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_playButton);
        }
    }

    // The Play Button will call this
    public void PlayGame()
    {
        AudioManager.Instance.PlaySelectButtonSound();
        // Un-pause time just in case the player quit to the menu while the game was paused
        Time.timeScale = 1f;

        SceneManager.LoadScene("Restaurant");
    }

    // The Quit Button will call this
    public void QuitGame()
    {
        AudioManager.Instance.PlaySelectButtonSound();
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}
