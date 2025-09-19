using UnityEngine;

public class NPCconditionChat2 : MonoBehaviour
{
    public DialogueLine[] npcDialogueLines;             // 这个 NPC 的对话
    public NPCcontinueMover npccontinueMover;
    public NPCcontinueMover1 npccontinueMover1;
    public NPCcontinueMovernormal npccontinueMover2;

    public DialogueSystem dialogueSystem;
    private bool dialogueTriggered = false;
    public bool finished = false;
    public bool triggerOnce = false;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void Update()
    {   
        if (npccontinueMover1.hasFinishedMoving && !dialogueTriggered && doorTeleport.isOpening == false)
        {
            if (dialogueSystem != null)
            {
                dialogueSystem.triggerOnlyOnce = triggerOnce;
                dialogueSystem.SetDialogue(npcDialogueLines); // 设置对应的对话内容
                dialogueSystem.StartDialogue();
                dialogueTriggered = true;
            }
        }


        if (dialogueTriggered && dialogueSystem.dialogueFinished)
        {  
            //Debug.Log("NPC对话完成，可以触发后续事件");
            finished = true;
        }
    }
}
