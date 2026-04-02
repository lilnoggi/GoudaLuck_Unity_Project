using UnityEngine;

/// <summary>
/// This script...
/// </summary>

[System.Serializable]
public struct DialogueLine
{
    public string SpeakerName;
    [TextArea(3, 5)] public string Text;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Gouda Luck/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public DialogueLine[] Lines;
}
