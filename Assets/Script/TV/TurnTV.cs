using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnTV : MonoBehaviour
{
    public DialogueLine[] npcDialogueLines;             
    public string dialogueID = "";                       
    private bool playerInRange = false;
    public float delayTimer = 0f;

    private DialogueSystem dialogueSystem;
    public bool hasTriggered = false;
    public static bool tvon = false;
    public AudioSource turnon;
    private bool hasplayed = false;

    [Header("TV Sprite")]
    public Sprite tvOffSprite;        // 电视关的贴图
    public Sprite tvOnSprite;         // 电视开的贴图
    public Sprite tvPlayingSprite;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化为关闭贴图
        if (spriteRenderer != null && tvOffSprite != null)
            spriteRenderer.sprite = tvOffSprite;

        if(tvon==true){
            if (spriteRenderer != null && tvOnSprite != null)
            {
                spriteRenderer.sprite = tvOnSprite;
            }
        }else{
            if (spriteRenderer != null && tvOnSprite != null)
            {
                spriteRenderer.sprite = tvOffSprite;
            }
        }
        
        if(TVplaying.isplaying){
            if (spriteRenderer != null && tvOnSprite != null)
            {
                spriteRenderer.sprite = tvPlayingSprite;
            }
        }
    }

    void Update()
    {
        // 玩家在范围内并按下 E 且拥有物品
        if (playerInRange && InventoryManager.Instance.HasItem("iphone16 pro max") && Input.GetKeyDown(KeyCode.E) && !tvon && !InventoryManager.Instance.HasItem("大门钥匙"))
        {
            tvon = true;

            // 切换贴图
            if (spriteRenderer != null && tvOnSprite != null)
            {
                spriteRenderer.sprite = tvOnSprite;
            }

            if(hasplayed == false){
                turnon.Play();
                hasplayed = true;
            }
            // 触发对话
            if (dialogueSystem != null && hasTriggered == false)
            {
                // 可选：使用 delayTimer 延迟触发
                if (delayTimer > 0f)
                {
                    StartCoroutine(StartDialogueWithDelay(delayTimer));
                }
                else
                {
                    StartDialogue();
                }
            }
        }

        if(tvon==true){
            if (spriteRenderer != null && tvOnSprite != null)
            {
                spriteRenderer.sprite = tvOnSprite;
            }
        }else{
            if (spriteRenderer != null && tvOnSprite != null)
            {
                spriteRenderer.sprite = tvOffSprite;
            }
        }

        if(TVplaying.isplaying){
            if (spriteRenderer != null && tvOnSprite != null)
            {
                spriteRenderer.sprite = tvPlayingSprite;
            }
        }
    }

    private void StartDialogue()
    {
        if (!DialogueSystem.isInDialogue)
        {
            dialogueSystem.dialogueID = dialogueID;
            dialogueSystem.triggerOnlyOnce = true;
            dialogueSystem.SetDialogue(npcDialogueLines);
            dialogueSystem.StartDialogue();
            hasTriggered = true;
        }
    }

    private IEnumerator StartDialogueWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartDialogue();
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

            // 如果希望离开范围时电视关闭：
            // tvon = false;
            // if(spriteRenderer != null && tvOffSprite != null)
            //     spriteRenderer.sprite = tvOffSprite;
        }
    }

    public void TurnOff()
    {
        tvon = false;
        hasTriggered = false;
        hasplayed = false;
        TVplaying.isplaying = false;
    }
}
