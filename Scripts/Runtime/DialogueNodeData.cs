using System;
using ScriptableObjects;
using UnityEngine;

[Serializable]
public class DialogueNodeData
{
    public string nodeGuid;
    public string title;
    public string dialogueText;
    public Character speaker;
    public Vector2 position;

    public DialogueNodeData(string nodeGuid, string title, string dialogueText,Character speaker, Vector2 position)
    {
        this.nodeGuid = nodeGuid;
        this.title = title;
        this.dialogueText = dialogueText;
        this.speaker = speaker;
        this.position = position;
    }

    public DialogueNodeData()
    {

    }
}

