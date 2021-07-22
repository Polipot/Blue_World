using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class aDialogue : ScriptableObject
{
    public List<string> DialogueNames;

    [TextArea(10,30)]
    public List<string> DialogueLines;
}
