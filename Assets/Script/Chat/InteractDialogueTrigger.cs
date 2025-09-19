using UnityEngine;
using System.Collections.Generic;

public class InteractDialogueTrigger : MonoBehaviour
{
    public DialogueLine[] npcDialogueLines;             // 这个 NPC 的对话
    public string dialogueID = "";                       // 唯一的对话ID，Inspector填写
    private bool playerInRange = false;
    public float delayTimer = 0f;

    private DialogueSystem dialogueSystem;
    public bool hasTriggered = false;
    public bool finished = false;

    public UniversalExpressionEvaluator evaluator;

    readonly Dictionary<string, object> vars = new();

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    void Update()
    {
        vars["locked"] = GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime && !InventoryManager.Instance.HasItem("学生卡") && playerInRange && (Input.GetKeyDown(KeyCode.E));
        vars["lockedbutcard"] = GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime && InventoryManager.Instance.HasItem("学生卡") && !InventoryManager.Instance.HasItem("擦脚巾") && playerInRange && (Input.GetKeyDown(KeyCode.E));
        vars["lockedtowel"] = !InventoryManager.Instance.HasItem("学生卡") && playerInRange && (Input.GetKeyDown(KeyCode.E));
        vars["nosocket"] = (GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime && InventoryManager.Instance.HasItem("学生卡")) && InventoryManager.Instance.HasItem("擦脚巾") && !EquipmentManager.IsEquipped("臭袜子") && hasTriggered == false && !PlayerMovementMonitor.awake;
        vars["getphone"] = GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime && InventoryManager.Instance.HasItem("iphone16 pro max") && hasTriggered == false;
        vars["turnTV"] = !InventoryManager.Instance.HasItem("iphone16 pro max") && playerInRange && (Input.GetKeyDown(KeyCode.E));
        vars["haskey"] = GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime && InventoryManager.Instance.HasItem("大门钥匙") && hasTriggered == false;

        if (playerInRange && (Input.GetKeyDown(KeyCode.E)) && doorTeleport.isOpening == false && evaluator == null)
        {
            if (dialogueSystem != null)
            {
                // 只在未进行中时触发对话开始，或当前对话行打字完后才响应
                if (!DialogueSystem.isInDialogue)
                {
                    // 先设置唯一ID和只触发一次开关
                    dialogueSystem.dialogueID = dialogueID;
                    dialogueSystem.triggerOnlyOnce = true;

                    dialogueSystem.SetDialogue(npcDialogueLines);
                    dialogueSystem.StartDialogue();
                    hasTriggered = true;
                }
                else if (!dialogueSystem.isTyping && dialogueSystem.canClickNext)
                {
                    finished = true;
                    dialogueSystem.ProceedToNextLine();  // 新增方法，用于外部触发
                }
            }
        }else if(doorTeleport.isOpening == false && evaluator !=null && evaluator.EvaluateBool(vars)){
            if (dialogueSystem != null)
            {
                // 只在未进行中时触发对话开始，或当前对话行打字完后才响应
                delayTimer -= Time.deltaTime;
                if(delayTimer <= 0f){
                    if (!DialogueSystem.isInDialogue)
                    {
                        // 先设置唯一ID和只触发一次开关
                        dialogueSystem.dialogueID = dialogueID;
                        dialogueSystem.triggerOnlyOnce = true;

                        dialogueSystem.SetDialogue(npcDialogueLines);
                        dialogueSystem.StartDialogue();
                        hasTriggered = true;
                    }
                }
                else if (!dialogueSystem.isTyping && dialogueSystem.canClickNext)
                {
                    dialogueSystem.ProceedToNextLine();  // 新增方法，用于外部触发
                    
                }
            }
        }

        if (hasTriggered && dialogueSystem.dialogueFinished)
        {  
            Debug.Log("NPC对话完成，可以触发后续事件");
            finished = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}