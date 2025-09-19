using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Collider2D))]
public class DoorToggleOil : MonoBehaviour
{
    [Header("门位置（两个空物体）")]
    public Transform closedPoint;
    public Transform openPoint;

    [Header("交互")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    public bool requirePlayerInRange = true;

    [Header("移动")]
    public float moveDuration = 0.6f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useRigidbodyIfAvailable = true;
    public bool allowRetoggleWhileMoving = true;

    [Header("贴图切换")]
    public SpriteRenderer doorRenderer;
    public Sprite oiledSprite;

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip talkClip;
    public AudioClip doorClip;

    [Header("阻塞检测（可选）")]
    public bool preventCloseIfBlocked = true;
    public LayerMask blockMask;
    public Vector2 closeCheckSize = new(0.8f, 1.8f);
    public float closeCheckAngle = 0f;
    public bool autoSizeFromCollider = true;

    [Header("唯一 ID（场景中唯一）")]
    public string doorID;

    // —— 运行时 —— //
    public bool isOpen;
    public bool hasChangedSprite;
    private bool playerInRange;
    public bool isBusy;
    private Coroutine moveCo;
    private Rigidbody2D rb;
    private Collider2D[] selfCols;

    public int interactStep = 0;
    private bool restoredFromSave = false;
    private DialogueSystem dialogueSystem;
    public DialogueLine[] npcDialogueLines; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        selfCols = GetComponentsInChildren<Collider2D>(true);

        if (autoSizeFromCollider)
        {
            var c = selfCols.FirstOrDefault(cc => !cc.isTrigger);
            if (c != null) closeCheckSize = c.bounds.size;
        }

        // —— 只读取状态字段，不移动门位置 —— //
        if (!string.IsNullOrEmpty(doorID) && GlassStateManager.Instance != null)
        {
            if (GlassStateManager.Instance.TryGetGlassState(SceneManager.GetActiveScene().name, doorID, out var entry))
            {
                isOpen = entry.isOpen;        // 只设置字段
                hasChangedSprite = entry.isOiled;
                restoredFromSave = true;

                // 立刻更新贴图
                ApplySpriteOnly();
            }
        }
    }

    void Start()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        if (openPoint == null || closedPoint == null)
        {
            Debug.LogWarning($"[{name}] openPoint 或 closedPoint 未分配，无法恢复门位置。");
            ApplySpriteOnly();
            return;
        }

        if (restoredFromSave)
        {
            // 存档存在，恢复门位置和贴图
            RestoreToState(isOpen, hasChangedSprite);
        }
        else
        {
            // 无存档，使用 Inspector 初始值
            ApplyInspectorInitialState();
        }
        
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (!requirePlayerInRange || playerInRange)
                CustomInteract();
        }
    }

    void CustomInteract()
    {
        bool isOilSelected = InventoryManager.Instance.HasItem("食用油");

        if (interactStep == 0)
        {
            if (isOilSelected && !hasChangedSprite)
            {
                SetOiled(true);
                interactStep = 1;
            }
            else
            {
                if (audioSource && talkClip) audioSource.PlayOneShot(talkClip);
                if (!DialogueSystem.isInDialogue)
                {
                    // 先设置唯一ID和只触发一次开关
                    dialogueSystem.triggerOnlyOnce = true;

                    dialogueSystem.SetDialogue(npcDialogueLines);
                    dialogueSystem.StartDialogue();
                }
                else if (!dialogueSystem.isTyping && dialogueSystem.canClickNext)
                {
                    dialogueSystem.ProceedToNextLine();  // 新增方法，用于外部触发
                }
                interactStep = 1;
            }
        }
        else
        {
            TryToggle();

            // 保存状态
            if (!string.IsNullOrEmpty(doorID) && GlassStateManager.Instance != null)
                GlassStateManager.Instance.SaveDoorState(SceneManager.GetActiveScene().name, doorID, isOpen, hasChangedSprite);

            if (!isOilSelected && audioSource && doorClip){
                audioSource.PlayOneShot(doorClip);
                if(!PlayerMovementMonitor.awake){
                    PlayerMovementMonitor.awake = true;
                }
                }
            interactStep = 2;
        }
    }

    public void TryToggle()
    {
        if (isBusy && !allowRetoggleWhileMoving) return;

        bool wantOpen = !isOpen;

        if (!wantOpen && preventCloseIfBlocked && IsCloseBlocked())
            return;

        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(MoveDoor(wantOpen));
    }

    IEnumerator MoveDoor(bool toOpen)
    {
        isBusy = true;

        Vector3 from = rb && useRigidbodyIfAvailable ? rb.position : transform.position;
        Vector3 to = toOpen ? openPoint.position : closedPoint.position;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
            float k = ease.Evaluate(Mathf.Clamp01(t));
            Vector3 pos = Vector3.LerpUnclamped(from, to, k);

            if (rb && useRigidbodyIfAvailable)
                rb.MovePosition(pos);
            else
                transform.position = pos;

            yield return null;
        }

        if (rb && useRigidbodyIfAvailable)
        {
            rb.position = to;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            transform.position = to;
        }

        isOpen = toOpen;
        isBusy = false;
        moveCo = null;

        if (!string.IsNullOrEmpty(doorID) && GlassStateManager.Instance != null)
            GlassStateManager.Instance.SaveDoorState(SceneManager.GetActiveScene().name, doorID, isOpen, hasChangedSprite);
    }

    bool IsCloseBlocked()
    {
        Vector2 center = closedPoint.position;
        var hits = Physics2D.OverlapBoxAll(center, closeCheckSize, closeCheckAngle, blockMask);
        foreach (var h in hits)
        {
            if (h == null) continue;
            if (selfCols.Contains(h)) continue;
            return true;
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) playerInRange = false;
    }

    public void SetOpen(bool open)
    {
        Vector3 targetPos = open ? openPoint.position : closedPoint.position;

        if (rb && useRigidbodyIfAvailable)
        {
            rb.position = targetPos;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            transform.position = targetPos;
        }

        isOpen = open;

        if (!string.IsNullOrEmpty(doorID) && GlassStateManager.Instance != null)
            GlassStateManager.Instance.SaveDoorState(SceneManager.GetActiveScene().name, doorID, isOpen, hasChangedSprite);
    }

    public void SetOiled(bool state)
    {
        hasChangedSprite = state;
        if (doorRenderer && oiledSprite && state) doorRenderer.sprite = oiledSprite;

        if (!string.IsNullOrEmpty(doorID) && GlassStateManager.Instance != null)
            GlassStateManager.Instance.SaveDoorState(SceneManager.GetActiveScene().name, doorID, isOpen, hasChangedSprite);
    }

    public bool IsOpen => isOpen;
    public bool IsOiled => hasChangedSprite;

    public void RestoreToState(bool open, bool oiled)
    {
        Vector3 targetPos = open ? openPoint.position : closedPoint.position;
        if (rb && useRigidbodyIfAvailable)
        {
            rb.position = targetPos;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            transform.position = targetPos;
        }

        isOpen = open;
        hasChangedSprite = oiled;

        ApplySpriteOnly();

        if (moveCo != null) { StopCoroutine(moveCo); moveCo = null; }
        isBusy = false;
    }

    private void ApplyInspectorInitialState()
    {
        Vector3 target = isOpen ? openPoint.position : closedPoint.position;
        if (rb && useRigidbodyIfAvailable)
        {
            rb.position = target;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            transform.position = target;
        }

        ApplySpriteOnly();
        isBusy = false;
    }

    private void ApplySpriteOnly()
    {
        if (doorRenderer == null) return;
        if (hasChangedSprite && oiledSprite != null)
            doorRenderer.sprite = oiledSprite;
    }

    // DoorToggleOil.cs —— add this public method
public void ForceRestoreState(bool open, bool oiled)
{
    // 停掉可能的移动协程
    if (moveCo != null) { StopCoroutine(moveCo); moveCo = null; }

    // 先设置标志，防止其它逻辑干扰
    isBusy = false;
    hasChangedSprite = oiled;
    isOpen = open;

    // 同步贴图
    ApplySpriteOnly();

    // 临时禁用 Animator（若有），以防动画在下一帧覆盖位置
    Animator animator = GetComponent<Animator>();
    bool animatorWasEnabled = false;
    if (animator != null && animator.enabled)
    {
        animatorWasEnabled = true;
        animator.enabled = false;
    }

    // 立刻写入位置
    Vector3 targetPos = open ? (openPoint != null ? openPoint.position : transform.position)
                             : (closedPoint != null ? closedPoint.position : transform.position);

    if (rb && useRigidbodyIfAvailable)
    {
        rb.position = targetPos;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    else
    {
        transform.position = targetPos;
    }

    // 恢复 animator 状态（保守做法）
    if (animatorWasEnabled)
        animator.enabled = true;

    // 保证交互步一致（已上油直接跳过第一步）
    interactStep = hasChangedSprite ? 1 : 0;

    // 标记空闲
    isBusy = false;

}

}
