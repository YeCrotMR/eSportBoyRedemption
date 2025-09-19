using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 依赖：
// - DoorStateManager（用于跨场景/跨存档保存每扇门的锁定/解锁状态）
// - GameManager 在保存/读取时需导出/导入 DoorStateManager 的 doorStates 列表
//   （参见我之前给你的 Save/Load 改动示例）

public class doorTeleport : MonoBehaviour
{
    private AudioSource teleportSound;
    public AudioSource locked;
    public AudioSource unlocked;
    private bool levelCompleted = false;
    private Animator anim;

    [Header("Teleport")]
    public float xPos;
    public float yPos;
    public int index;

    private PlayerMovement playerMovement;
    public static string previousSceneName = "";
    public string doorID;

    [Header("Unlock Dialogue (optional)")]
    public DialogueLine[] doorDialogue;
    public string dialogueID = "";
    private bool dialogueTriggered = false;
    private bool waitingForUnlock = false;

    private bool playerInRange = false;
    private GameObject player;
    public static bool isOpening = false;

    bool hasCard;
    bool hasTowel;

    // 兼容旧逻辑的全局标记：仅用于 momroom，表示已经开锁过（跨场景/存档时建议改用 DoorStateManager）
    public static bool momunHasunlocked = false;

    // 解锁流程状态
    [SerializeField] private bool isUnlocking = false; // 正在播放开锁动画
    [SerializeField] private bool isUnlocked = false;  // 已完成开锁（需二次按键才开门）

    // 等待开锁动画结束的方式
    [SerializeField] private float unlockAnimDuration = 0.8f; // 若不填状态名，则用这个时长
    [SerializeField] private string unlockAnimStateName = "";  // 可选：Animator中开锁动画状态名

    // 运行时锁定（每帧计算）
    public bool doorLocked = false;

    // 持久锁定（从 DoorStateManager 恢复/写入）
    private bool persistentLocked = false;
    private string sceneName;

    private void Start()
    {
        isOpening = false;
        teleportSound = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        playerMovement = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
        isUnlocking = false;
        isUnlocked = false;

        sceneName = SceneManager.GetActiveScene().name;

        // 从 DoorStateManager 恢复当前门的持久状态（如果没有就用默认 false/false）
        var s = DoorStateManager.Instance != null
            ? DoorStateManager.Instance.Get(sceneName, doorID)
            : default;

        bool isMom = doorID == "momroom";

        // 已解锁优先；兼容旧的 momunHasunlocked 标记
        isUnlocked = s.isUnlocked || (isMom && momunHasunlocked);

        // 持久锁，若已解锁则不再视为锁住
        persistentLocked = s.isLocked && !isUnlocked;

        // 如果是 momroom 并且曾经解锁过，禁用再次对话/解锁流
        if (isMom && isUnlocked)
        {
            waitingForUnlock = false;
            dialogueTriggered = true;
            if (anim != null) anim.SetInteger("door", 0);
        }
    }

    private void Update()
    {
        if(InventoryManager.Instance != null){
            hasCard = InventoryManager.Instance.HasItem("学生卡");
            hasTowel = InventoryManager.Instance.HasItem("擦脚巾");
        }
        bool hasBoth = hasCard && hasTowel;
        bool pressInteract = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space);

        bool isMom = doorID == "momroom";
        bool isBig = doorID == "bigdoor";
        bool timerLockActive = GlobalTimer.ElapsedTime >= TimerThresholdActivator.triggerTime;

        // 先处理：如果已经触发了解锁对话，并且对话结束了，执行开锁收尾
        if (isMom && timerLockActive && hasBoth && playerInRange
            && !isUnlocked && !isUnlocking && waitingForUnlock
            && DialogueSystem.Instance != null && DialogueSystem.Instance.dialogueFinished)
        {
            StartCoroutine(UnlockDoorRoutine());
            momunHasunlocked = true; // 兼容旧标记
            waitingForUnlock = false;
            return; // 本帧到此为止
        }

        // 对话中不处理交互输入（但上面的“开锁收尾”仍能执行）
        if (DialogueSystem.isInDialogue) return;

        // 计算锁门（每帧）
        doorLocked = false;

        // bigdoor 永远上锁
        if (isBig)
        {
            if(!InventoryManager.Instance.HasItem("大门钥匙")){
            doorLocked = true;}else{
                doorLocked = false;
            }
        }

        // momroom 锁定逻辑
        if (isMom)
        {
            if (timerLockActive)
            {
                if (!isUnlocked)
                {
                    // 达阈值但未解锁：
                    // 1) 物品不足 -> 锁住（并持久化记录为锁定）
                    if (!hasBoth)
                    {
                        doorLocked = true;
                        if (!persistentLocked)
                        {
                            persistentLocked = true;
                            DoorStateManager.Instance?.SetLocked(sceneName, doorID, true);
                        }
                    }
                    else
                    {
                        // 2) 物品齐了 -> 仍视为锁住，需先触发解锁对话/动画
                        doorLocked = true;
                    }
                }
                else
                {
                    // 已解锁 -> 不锁
                    doorLocked = false;
                }
            }
            else
            {
                // 阈值之前：不锁（可直接开门）
                doorLocked = false;
            }
        }

        // 叠加“持久锁”（除非已解锁）
        if (!isUnlocked)
            doorLocked = doorLocked || persistentLocked;

        // momroom：达阈值且物品齐全且未解锁 -> 首次按键触发“解锁对话”
        if (isMom && timerLockActive && hasBoth && playerInRange && pressInteract && !isUnlocked && !isUnlocking)
        {
            if (!dialogueTriggered && DialogueSystem.Instance != null)
            {
                dialogueTriggered = true;

                DialogueSystem.Instance.dialogueID = dialogueID; // 设置唯一ID
                DialogueSystem.Instance.triggerOnlyOnce = true;   // 只触发一次
                DialogueSystem.Instance.SetDialogue(doorDialogue);
                DialogueSystem.Instance.StartDialogue();

                waitingForUnlock = true;
            }
            return; // 本帧不再处理后续逻辑（避免立刻进入锁门音效/开门）
        }

        // 锁住时按键 -> 播放锁门音效
        if (doorLocked && playerInRange && pressInteract)
        {
            if (locked != null) locked.Play();
            return;
        }

        // 正常开门逻辑
        bool canOpen = !doorLocked && playerInRange && !levelCompleted && pressInteract && !isUnlocking;

        // momroom 在阈值之后必须先完成解锁，才允许开门
        if (isMom && timerLockActive)
            canOpen &= isUnlocked;

        if (canOpen)
        {
            isOpening = true;
            levelCompleted = true;
            if (playerMovement != null) playerMovement.canMove = false;

            TeleportInfo.targetPosition = new Vector3(xPos, yPos, 0f);
            TeleportInfo.useTargetPosition = true;
            TeleportInfo.shouldEnableMovement = true;

            if (anim != null)
                anim.SetInteger("door", 1);

            if (teleportSound != null)
                teleportSound.Play();

            Invoke(nameof(CompleteLevel), 0.466f);
        }
    }

    private IEnumerator UnlockDoorRoutine()
    {
        isUnlocking = true;

        // 播放开锁动画与音效
        if (anim != null) anim.SetInteger("door", 3);
        if (unlocked != null) unlocked.Play();

        // 等待开锁动画播放完成
        if (anim != null && !string.IsNullOrEmpty(unlockAnimStateName))
        {
            int layer = 0;
            float safetyTimeout = 5f;
            float t = 0f;

            // 等待进入指定状态
            while (t < safetyTimeout)
            {
                var st = anim.GetCurrentAnimatorStateInfo(layer);
                if (st.IsName(unlockAnimStateName)) break;
                t += Time.deltaTime;
                yield return null;
            }

            // 等待该状态播放结束
            bool inState = true;
            while (inState)
            {
                var st = anim.GetCurrentAnimatorStateInfo(layer);
                inState = st.IsName(unlockAnimStateName) && st.normalizedTime < 1f;
                yield return null;
            }
        }
        else
        {
            // 没填状态名就按时长等待
            yield return new WaitForSeconds(unlockAnimDuration);
        }

        // 回到 idle
        if (anim != null) anim.SetInteger("door", 0);

        // 标记已解锁：下一次按键才真正开门
        isUnlocking = false;
        isUnlocked = true;
        doorLocked = false;

        // 同步到 DoorStateManager（持久）
        persistentLocked = false;
        DoorStateManager.Instance?.SetUnlocked(sceneName, doorID, true);
        DoorStateManager.Instance?.SetLocked(sceneName, doorID, false);

        // 兼容旧标记（仅 momroom 使用）
        if (doorID == "momroom") momunHasunlocked = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
            player = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
        }
    }

    private void CompleteLevel()
    {
        previousSceneName = SceneManager.GetActiveScene().name;
        FadeController.Instance.FadeAndLoadScene(SceneManager.GetActiveScene().buildIndex + index);
    }
}
