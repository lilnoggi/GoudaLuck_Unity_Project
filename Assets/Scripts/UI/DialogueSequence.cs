using UnityEngine;

/// <summary>
/// A single, encapsulated line of dialogue within a broader narrative sequence.
/// Kept serialiseable to expose its fields directly in the Unity Inspector.
/// </summary>
[System.Serializable] 
public struct DialogueLine
{
    [Tooltip("The name of the character speaking this line (e.g., 'Chef Brie'). Sensed by the UI Manager to trigger camera cuts.")]
    public string SpeakerName;
    [Tooltip("The actual narrative text to be displayed via the asynchronous typewriter effect.")]
    [TextArea(3, 5)] // Expands the input box in the Inspector for multi-line editing
    public string Text;
}

/// <summary>
/// A Data-Driven container for narrative sequences.
/// Decouples dialogue data from game logic, allowing designers to easily
/// create, store, and edit conversations directly within the Unity Editor
/// without altering any code.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Gouda Luck/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Tooltip("The sequential array of dialogue lines that make up this specific conversation or cutscene.")]
    public DialogueLine[] Lines;
}
