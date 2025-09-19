using UnityEngine;
using System.Collections.Generic;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public string dialogueID;
    //public string uniqueID; // 手动在Inspector中赋值，保证唯一性
    public bool hasPickedUp;

    [Header("可选：是否拾取前先触发对话")]
    public bool hasDialogue = true;
    public bool haspickedDialogue = false;
    public float delayTimer = 0f;

    public DialogueLine[] pickupDialogue;
    public DialogueLine[] hasPickupedDialogue;

    public bool hasUnlocked = true;
    private SpriteOutline outline;

    public bool isPlayerInRange = false;
    private bool dialogueTriggered = false;
    public bool dialogueFinished = false;
    private bool PickupeddialogueTriggered = false;
    private bool waitingForPickup = false;

    public UniversalExpressionEvaluator evaluator;

    readonly Dictionary<string, object> vars = new();

    private void Start()
{
    outline = GetComponent<SpriteOutline>();

    // 如果当前运行时已经在背包里（玩家在当前会话已拥有），直接隐藏
    if ((InventoryManager.Instance != null && InventoryManager.Instance.HasItem(item.itemName)) || EquipmentManager.IsEquipped(item.itemName))
    {
        hasPickedUp = true;
        gameObject.SetActive(false);
        return;
    }

    // 如果读档数据里已包含该物品名，也应隐藏
    var currentData = GameManager.Instance?.currentSaveData;
    if (currentData != null && currentData.inventoryItemNames != null && currentData.inventoryItemNames.Contains(item.itemName))
    {
        hasPickedUp = true;
        gameObject.SetActive(false);
        return;
    }

    // 原来的逻辑（保持 UI Outline 初始状态）
    if (outline != null)
        outline.DisableOutline();
}

   private void Update()
{
        vars["lockedtowel"] =  InventoryManager.Instance.HasItem("学生卡");
        vars["noawake"] = PlayerMovementMonitor.awake == false;

if ((InventoryManager.Instance != null && InventoryManager.Instance.HasItem(item.itemName)) || EquipmentManager.IsEquipped(item.itemName))
    {
        hasPickedUp = true;
        gameObject.SetActive(false);
        return;
    }

    if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && evaluator == null)
    {
        if (hasDialogue)
        {
            if (!dialogueTriggered)
            {
                

                DialogueSystem.Instance.dialogueID = dialogueID;      // 设置唯一ID
                DialogueSystem.Instance.triggerOnlyOnce = false;       // 只触发一次

                DialogueSystem.Instance.SetDialogue(pickupDialogue);
                DialogueSystem.Instance.StartDialogue();

                waitingForPickup = true;
                dialogueTriggered = true;
            }
        }
        else
        {
            PickUpItem();
        }

    }else if(isPlayerInRange && Input.GetKeyDown(KeyCode.E) &&  evaluator !=null && evaluator.EvaluateBool(vars)){
        if (hasDialogue)
        {
            if (!dialogueTriggered)
            {
                DialogueSystem.Instance.dialogueID = dialogueID;      // 设置唯一ID
                DialogueSystem.Instance.triggerOnlyOnce = false;       // 只触发一次

                DialogueSystem.Instance.SetDialogue(pickupDialogue);
                DialogueSystem.Instance.StartDialogue();

                waitingForPickup = true;
                dialogueTriggered = true;
            }
        }else
        {
            PickUpItem();
        }
    }

    if(haspickedDialogue && hasPickedUp == true){
            delayTimer -= Time.deltaTime;
            if(delayTimer <= 0f){
                if (!PickupeddialogueTriggered)
                {
                    DialogueSystem.Instance.dialogueID = dialogueID;      // 设置唯一ID
                    DialogueSystem.Instance.triggerOnlyOnce = false;       // 只触发一次

                    DialogueSystem.Instance.SetDialogue(hasPickupedDialogue);
                    DialogueSystem.Instance.StartDialogue();
                    PickupeddialogueTriggered = true;
                }
            }
        }

    if (hasDialogue && waitingForPickup && DialogueSystem.Instance.dialogueFinished)
    {
        dialogueFinished = true;
        PickUpItem();
    }
}

    private void PickUpItem()
{
    // 加入背包
    InventoryManager.Instance.AddItem(item);
    hasPickedUp = true;

    // 不再写入 pickedItems（彻底弃用 pickedUpItemIDs）
    // if (!PickupStateManager.Instance.pickedItems.Contains(uniqueID))
    // {
    //     PickupStateManager.Instance.pickedItems.Add(uniqueID);
    // }

    gameObject.SetActive(false);
}

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (outline != null) outline.EnableOutline();
        }
    }

    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (outline != null) outline.DisableOutline();
        }
    }
}
