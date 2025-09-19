using UnityEngine;
using System.Collections.Generic;
using System.Collections;


public class NPCconditionChat : MonoBehaviour
{
    public DialogueLine[] npcDialogueLines;             // 这个 NPC 的对话
    public NPCcontinueMover npccontinueMover;
    public NPCcontinueMover npccontinueMover3;
    public NPCcontinueMovernormal npccontinueMover2;
    public NPCcontinueMover1 npccontinueMover1;

    public DialogueSystem dialogueSystem;
    private bool dialogueTriggered = false;
    public bool finished = false;
    public bool triggerOnce = false;
    public static bool isChatting = false;
    public ItemPickup key;
    public UniversalExpressionEvaluator evaluator;
    readonly Dictionary<string, object> vars = new();
    public NPCconditionChat other;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void Update()
    {   
        vars["fuckkey"] = GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime && InventoryManager.Instance.HasItem("大门钥匙") && !TVplaying.isplaying && !Timer.stolenkey;
        vars["normal"] = (npccontinueMover?.hasFinishedMoving == true || npccontinueMover2?.hasFinishedMoving == true || npccontinueMover1?.hasFinishedMoving == true || npccontinueMover3?.hasFinishedMoving == true) && !dialogueTriggered && doorTeleport.isOpening == false;
        
        if ((npccontinueMover?.hasFinishedMoving == true|| npccontinueMover2?.hasFinishedMoving == true || npccontinueMover1?.hasFinishedMoving == true || npccontinueMover3?.hasFinishedMoving == true) && !dialogueTriggered && doorTeleport.isOpening == false && evaluator == null && other?.dialogueTriggered == false)
        {
            if (dialogueSystem != null)
            {
                dialogueSystem.triggerOnlyOnce = triggerOnce;
                dialogueSystem.SetDialogue(npcDialogueLines); // 设置对应的对话内容
                dialogueSystem.StartDialogue();
                isChatting = true;
                dialogueTriggered = true;
                
            }
        }else if(!dialogueTriggered && doorTeleport.isOpening == false && evaluator != null && evaluator.EvaluateBool(vars)){
                if (dialogueSystem != null)
                {
                dialogueSystem.triggerOnlyOnce = triggerOnce;
                dialogueSystem.SetDialogue(npcDialogueLines); // 设置对应的对话内容
                dialogueSystem.StartDialogue();
                isChatting = true;
                dialogueTriggered = true;
                
                }
            }

        if (dialogueTriggered && dialogueSystem.dialogueFinished)
        {  
            //Debug.Log("NPC对话完成，可以触发后续事件");
            finished = true;
            isChatting = false;
        }
    }
}
