using UnityEngine;

/// <summary>
/// A Data-Driven container for narrative sequences.
/// Decouples dialogue data from game logic, allowing designers to easily
/// create, store, and edit conversations directly within the Unity Editor
/// without altering any code.
/// </summary>

[System.Serializable] // Required to expose this custom struct in the Unity Inspector
public struct DialogueLine
{
    public string SpeakerName;
    [TextArea(3, 5)] // Expands the input box in the Inspector for multi-line editing
    public string Text;
}

// Allowed developers to easily create new dialogue instances from the right-click Project menu
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Gouda Luck/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueLine[] Lines;
}
