using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDialogueTrigger : MonoBehaviour
{
    public DialogueLine[] dialogueLines;

    private DialogueSystem dialogueSystem;
    private bool dialogueTriggered = false;
    private bool hasCollectedKey = false;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void Update()
    {
        if (hasCollectedKey && !dialogueTriggered)
        {
            dialogueSystem.SetDialogue(dialogueLines);
            dialogueSystem.StartDialogue();
            dialogueTriggered = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hasCollectedKey = true;
        }
    }
}

