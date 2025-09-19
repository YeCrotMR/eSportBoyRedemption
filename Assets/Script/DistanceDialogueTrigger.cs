using UnityEngine;
using System.Collections.Generic;

public class DistanceDialogueTrigger : MonoBehaviour
{
    [Header("对话数据")]
    public DialogueLine[] npcDialogueLines;
    public string dialogueID = "";

    [Header("触发方式")]
    public bool startAutomaticallyOnEnter = true; // 进入触发器就开始
    public bool requireKeyToStart = false;        // 是否需要按键才能开始
    public KeyCode interactKey = KeyCode.E;       // 需要按的键

    private bool playerInRange = false;
    private bool startedThisStay = false;   // 本次玩家停留期间是否已启动过
    private DialogueSystem dialogueSystem;
    public UniversalExpressionEvaluator evaluator;
    readonly Dictionary<string, object> vars = new();

    private void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
    }

    private void Update()
    {
        vars["homework"] = GlobalTimer.ElapsedTime < TimerThresholdActivator.triggerTime && PlayerMovementMonitor.awake == false;
        
        if (!playerInRange || dialogueSystem == null) return;

        // 同一轮停留期间只触发一次
        if (startedThisStay) return;

        // 仅当当前没有其他对话在进行时才允许开始
        if (DialogueSystem.isInDialogue) return;

        // 触发条件：自动进入 或 需要按键且按下
        bool shouldStart = startAutomaticallyOnEnter
                           || (requireKeyToStart && Input.GetKeyDown(interactKey));

        // if (shouldStart)
        // {
        //     // 设置 ID 与“只触发一次”选项，让 DialogueSystem 自己去判断是否已触发过
        //     dialogueSystem.dialogueID = dialogueID;
        //     dialogueSystem.triggerOnlyOnce = true;

        //     dialogueSystem.SetDialogue(npcDialogueLines);
        //     dialogueSystem.StartDialogue();

        //     startedThisStay = true; // 标记已触发，直到离开触发器
        // }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // 如果选择“进入即触发”，可以在这里就尝试一次（避免等到下一帧）
            if (startAutomaticallyOnEnter && dialogueSystem != null && !DialogueSystem.isInDialogue && !startedThisStay && evaluator == null)
            {
                dialogueSystem.dialogueID = dialogueID;
                dialogueSystem.triggerOnlyOnce = true;
                dialogueSystem.SetDialogue(npcDialogueLines);
                dialogueSystem.StartDialogue();
                startedThisStay = true;
            }else if(startAutomaticallyOnEnter && dialogueSystem != null && !DialogueSystem.isInDialogue && !startedThisStay && evaluator != null  && evaluator.EvaluateBool(vars)){
                dialogueSystem.dialogueID = dialogueID;
                dialogueSystem.triggerOnlyOnce = true;
                dialogueSystem.SetDialogue(npcDialogueLines);
                dialogueSystem.StartDialogue();
                startedThisStay = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            // 重置停留标记，下一次进入时才允许再次触发
            startedThisStay = false;
        }
    }
}
