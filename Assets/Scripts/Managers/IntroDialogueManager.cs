using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// A controller script designed to manage narrative presentation and UX pacing.
/// This system decouples dialogue data from the scene by reading from a ScriptableObject,
/// and utilises asynchronous Coroutines and Input System interrupts to give the player
/// complete control over cutscene pacing.
/// </summary>
public class IntroDialogueManager : MonoBehaviour
{
    // --- SINGLETON INSTANCE ---
    public static IntroDialogueManager Instance {get; private set; }

    [Header("UI References")]
    [Tooltip("Text component displaying the current speaker's name.")]
    [SerializeField] private TextMeshProUGUI _speakerNameText;
    [Tooltip("Text component for the typewriter dialogue effect.")]
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [Tooltip("The parent panel containing all dialogue UI elements.")]
    [SerializeField] private GameObject _dialoguePanel;

    [Header("Cameras")]
    [Tooltip("Camera focused on Chef Brie.")]
    [SerializeField] private GameObject _camChefBrie;
    [Tooltip("Camera focused on the Player.")]
    [SerializeField] private GameObject _camPlayer;

    [Header("Data Source")]
    [Tooltip("The ScriptableObject containing the dialogue lines for the intro sequence.")]
    [SerializeField] private DialogueSequence _introSequence;

    [Header("Animation UX")]
    [Tooltip("Delay in seconds between each character rendering.")]
    [SerializeField] private float _typingSpeed = 0.04f;  // Time between each letter


    // --- STATE TRACKING ---
    private int _currentLineIndex = 0;
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;

    // =================================================================================================================

    private void Awake()
    {
        // Enforce the Singleton Instance
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Start the dialogue - initialise the presentation layer
        _dialoguePanel.SetActive(true);
        DisplayNextLine(_currentLineIndex);
    }

    private void Update()
    {
        // --- UX PACING & INPUT INTERRUPTS ---
        // For UI pacing, directly checking the hardware state per-frame is acceptable
        // to ensure immediate, snappy interrupt response.
        bool pressedE = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool pressedAdvance = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;

        if (pressedE || pressedAdvance)
        {
            // If the text is currently animating, interrupt it and show the full dialogue
            if (_isTyping)
            {
                // INTERRUPT: If the player reads faster than the animation,
                // stop the Coroutine and immediately display the full sentence
                if (_typingCoroutine != null)
                {
                    StopCoroutine(_typingCoroutine);
                }
                    
                // Hard-set the text to the full string
                _dialogueText.text = _introSequence.Lines[_currentLineIndex].Text;
                _isTyping = false;
            }
            else  
            {
                // PROGRESS: If the player presses the button after the full text is shown, advance to the next line
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
    }


    public void DisplayNextLine(int index)
    {
        // Get the data from the ScriptableObject
        string speaker = _introSequence.Lines[index].SpeakerName;
        string fullSentence = _introSequence.Lines[index].Text;

        // Update the UI and text
        _dialoguePanel.SetActive(true);
        _speakerNameText.text = speaker;

        // --- CAMERA CUT LOGIC ---
        // Toggling these Virtual Cameras automatically triggers the Cinemachine Brain
        // to execute a smooth, cinematic camera cut
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

        // --- TYPEWRITER LOGIC ---
        // Safely stop any existing Coroutine before starting a new one to prevent overlap
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _typingCoroutine = StartCoroutine(TypeSentence(fullSentence));
    }

    /// <summary>
    /// An IEnumerator that renders text one character at a time, creating a typewriter effect.
    /// Yielding execution prevents the while-loop from locking the main thread.
    /// </summary>
    private IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        _dialogueText.text = "";

        // Iterate through the string array and append characters
        foreach (char letter in sentence.ToCharArray())
        {
            _dialogueText.text += letter;

            // Suspend execution until the typing speed interval has passed
            yield return new WaitForSeconds(_typingSpeed);
        }

        _isTyping = false;  // Flag the animation as complete
    }

    public void EndCutscene()
    {
        // Transition from the narrative intro into the core gameplay loop
        SceneManager.LoadScene("Restaurant");
    }
}
