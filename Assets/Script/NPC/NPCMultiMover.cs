using UnityEngine;

public class NPCMultiMover : MonoBehaviour
{
    [Header("引用组件")]
    [SerializeField] public DialogueSystem dialogueSystem;

    [Header("移动设置")]
    [SerializeField] public Transform[] movePoints;     // 目标点序列
    [SerializeField] public int[] triggerIndices;       // 对应的触发 clickCount 值
    [SerializeField] public float[] moveSpeeds;         // 每段移动速度（新增）

    [Header("可选参数")]
    [SerializeField] public bool hideAtStart = true;    // 是否初始隐藏
    [SerializeField] public bool disappearAtEnd = true; // 是否最后一段结束后消失

    public bool hasFinishedMoving = false; // ✅ 是否完成所有移动

    private int currentStage = 0;
    private bool shouldMove = false;
    private bool[] hasMoved;

    void Start()
    {
        if (movePoints.Length != triggerIndices.Length || movePoints.Length != moveSpeeds.Length)
        {
            Debug.LogWarning("NPCMultiMover: movePoints、triggerIndices 和 moveSpeeds 的长度必须一致！");
        }

        hasMoved = new bool[movePoints.Length];

        SetVisible(!hideAtStart);
    }

    void Update()
    {
        if (currentStage < triggerIndices.Length &&
            (dialogueSystem.clickCount == triggerIndices[currentStage]||dialogueSystem.dialogueFinished == true) &&
            !hasMoved[currentStage] && (dialogueSystem.clickCount >= 6))
        {
            shouldMove = true;
            SetVisible(true);
        }

        if (shouldMove)
        {
            Transform target = movePoints[currentStage];
            float moveSpeed = moveSpeeds[currentStage]; // 使用每段速度

            transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, target.position) < 0.05f)
            {
                shouldMove = false;
                hasMoved[currentStage] = true;
                currentStage++;

                // ✅ 达到最后一段目标点
                if (currentStage >= movePoints.Length)
                {
                    if (disappearAtEnd)
                        SetVisible(false);
                    hasFinishedMoving = true;
                }
            }
        }
    }

    // 控制NPC及其子物体显隐                            
    void SetVisible(bool visible)
    {
        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = visible;
        }

        foreach (var collider in GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = visible;
        }
    }
}
