using UnityEngine;

public class NPCconditionChat1 : MonoBehaviour
{
    public DialogueLine[] npcDialogueLines; 
    public DialogueLine[] npcDialogueLines2;             // 这个 NPC 的对话
    public NPCcontinueMovephone npccontinueMoverphone;

    public DialogueSystem dialogueSystem;
    private bool dialogueTriggered = false;
    public bool finished = false;
    public bool triggerOnce = false;
    public string dialogueID = "phone";

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void Update()
    {
        if ((npccontinueMoverphone.hasFinishedMoving) && InventoryManager.Instance.HasItem("iphone16 pro max")&& !dialogueTriggered && doorTeleport.isOpening == false)
        {
            if (dialogueSystem != null)
            {
                dialogueSystem.dialogueID = dialogueID;
                dialogueSystem.triggerOnlyOnce = triggerOnce;
                dialogueSystem.SetDialogue(npcDialogueLines); // 设置对应的对话内容
                dialogueSystem.StartDialogue();
                dialogueTriggered = true;
            }
        }else if((npccontinueMoverphone.hasFinishedMoving) && InventoryManager.Instance.HasItem("大门钥匙")&& !dialogueTriggered && doorTeleport.isOpening == false){
            if (dialogueSystem != null)
            {
                dialogueSystem.dialogueID = dialogueID;
                dialogueSystem.triggerOnlyOnce = triggerOnce;
                dialogueSystem.SetDialogue(npcDialogueLines2); // 设置对应的对话内容
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
