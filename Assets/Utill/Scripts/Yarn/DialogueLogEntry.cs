using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueLogEntry 
{
    public string CharacterName { get; }
    public string Text { get; }

    public DialogueLogEntry(string characterName, string text)
    {
        CharacterName = characterName;
        Text = text;
    }
}