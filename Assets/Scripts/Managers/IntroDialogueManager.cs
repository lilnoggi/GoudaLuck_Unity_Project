using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// This script manages the intro cutscene, handling camera cuts, typewriter text animation,
/// and player input to advance the dialogue.
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

    [Header("Animation")]
    [SerializeField] private float _typingSpeed = 0.04f;  // Time between each letter

    private int _currentLineIndex = 0;

    // Coroutine tracking variables
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;

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
            // If the text is currently animating, interrupt it and show the full dialogue
            if (_isTyping)
            {
                if (_typingCoroutine != null)
                {
                    StopCoroutine(_typingCoroutine);
                }
                    // Hard-set the text to the full string
                    _dialogueText.text = _introSequence.Lines[_currentLineIndex].Text;
                    _isTyping = false;
            }
            else  // If the text is fully typed out, move to the next line
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
    }


    public void DisplayNextLine(int index)
    {
        // Get the data
        string speaker = _introSequence.Lines[index].SpeakerName;
        string fullSentence = _introSequence.Lines[index].Text;

        // Update the UI and text
        _dialoguePanel.SetActive(true);
        _speakerNameText.text = speaker;

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

        // --- TYPEWRITER LOGIC ---
        // Stop the previous coroutine just in case
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        _typingCoroutine = StartCoroutine(TypeSentence(fullSentence));
    }

    // Typewriter coroutine
    private IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        _dialogueText.text = "";

        // Loop through the string and add one letter at a time
        foreach (char letter in sentence.ToCharArray())
        {
            _dialogueText.text += letter;

            yield return new WaitForSeconds(_typingSpeed);
        }

        _isTyping = false;  // Finished typing!
    }

    public void EndCutscene()
    {
        SceneManager.LoadScene("Restaurant");
    }
}
