using UnityEngine;

public class NPCMover : MonoBehaviour
{
    public DialogueSystem dialogueSystem;
    [SerializeField] public int index; // Now editable from the Inspector
    public Transform target;
    public float speed = 2f;

    private bool shouldMove = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D npcCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        npcCollider = GetComponent<Collider2D>();

        spriteRenderer.enabled = false;  // 隐藏 NPC
        if (npcCollider != null)
            npcCollider.enabled = false;  // 禁用碰撞器，防止交互
    }

    void Update()
    {
        if(dialogueSystem.clickCount == index){
            shouldMove = true;
        }

        if (shouldMove)
        {
            AppearAndMove();
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            if (Vector2.Distance(transform.position, target.position) < 0.1f)
            {
                shouldMove = false;
            }
        }
    }

    public void AppearAndMove()
    {
        spriteRenderer.enabled = true;
        if (npcCollider != null)
            npcCollider.enabled = true;

        shouldMove = true;
    }
}
