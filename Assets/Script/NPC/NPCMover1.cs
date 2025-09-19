using UnityEngine;

public class NPCMover1 : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    [SerializeField] public int index; // 对话计数触发值
    public Transform target;
    public float speed = 2f;

    private bool shouldMove = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D npcCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        npcCollider = GetComponent<Collider2D>();

        // 初始隐藏
        spriteRenderer.enabled = false;
        if (npcCollider != null)
            npcCollider.enabled = false;
    }

    void Update()
    {
        // 条件满足，开始移动并出现
        if (dialogueSystem.clickCount == index)
        {
            shouldMove = true;
            Appear(); // 只出现一次
        }

        // 移动逻辑
        if (shouldMove)
        {
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) < 0.1f)
            {
                shouldMove = false;
                Disappear(); // 到达终点后消失
            }
        }
    }

    // 出现（显示图像 + 启用碰撞）
        void Appear()
    {
        // 启用所有 SpriteRenderer
        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = true;
        }

        // 启用所有 Collider2D
        foreach (var collider in GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = true;
        }
    }

    void Disappear()
    {
        // 禁用所有 SpriteRenderer
        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = false;
        }

        // 禁用所有 Collider2D
        foreach (var collider in GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = false;
        }
    }

}
