using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// This script...
/// </summary>

public class IntroDialogueManager : MonoBehaviour
{
    public static IntroDialogueManager Instance {get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _speakerNameText;
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private GameObject _dialoguePanel;

    [Header("Cameras")]
    [SerializeField] private GameObject _camChefBrie;
    [SerializeField] private GameObject _camPlayer;

    [Header("Data")]
    [SerializeField] private DialogueSequence _introSequence;

    private int _currentLineIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Start the dialogue
        _dialoguePanel.SetActive(true);
        DisplayNextLine(_currentLineIndex);
    }

    private void Update()
    {
        // Use the Input System's quick-check commands for E and Gamepad A
        bool pressedE = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool pressedAdvance = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (pressedE || pressedAdvance)
        {
            _currentLineIndex++;

            if (_currentLineIndex < _introSequence.Lines.Length)
            {
                DisplayNextLine(_currentLineIndex);
            }
            else
            {
                EndCutscene();
            }
        }
    }

    // --- TIMELINE SIGNAL CALLS THIS ---
    public void DisplayNextLine(int index)
    {
        // Get the name of who is talking
        string speaker = _introSequence.Lines[index].SpeakerName;

        // Update the UI and text
        _dialoguePanel.SetActive(true);
        _dialogueText.text = _introSequence.Lines[_currentLineIndex].Text;

        // --- CAMERA CUT LOGIC ---
        if (speaker == "Chef Brie")
        {
            _camChefBrie.SetActive(true);
            _camPlayer.SetActive(false);
        }
        else // Otherwise the player is talking
        {
            _camPlayer.SetActive(true);
            _camChefBrie.SetActive(false);
        }
    }

    public void EndCutscene()
    {
        SceneManager.LoadScene("Restaurant");
    }
}
