using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCcontinueMover1 : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] public Transform[] movePoints;
    [SerializeField] public float[] moveDistances;
    [SerializeField] public float[] moveSpeeds;

    [Header("可选参数")]
    [SerializeField] public bool hideAtStart = true;
    [SerializeField] public bool disappearAtEnd = true;
    [SerializeField] public float moveDelay = 0f;

    [Header("碰撞绕边设置")]
    [Tooltip("会阻挡 NPC 的层。若留空，将自动从物理碰撞矩阵推断。")]
    [SerializeField] private LayerMask collisionMask;
    [Tooltip("皮肤宽度，避免贴边误判。")]
    [SerializeField] private float skin = 0.01f;
    [Tooltip("卡住时做一个很小的垂直于行进方向的挤边比例（相对本帧步长）。0~0.5")]
    [Range(0f, 0.5f)]
    [SerializeField] private float edgeNudgeRatio = 0.2f;

    public bool hasFinishedMoving = false;
    public string lastscenename = "";

    private int currentStage = 0;
    public bool isMoving = false;
    private bool isTriggered = false;
    private float delayTimer = 0f;

    private Rigidbody2D rb;

    // Cast 过滤器与缓存
    private ContactFilter2D moveFilter;
    private RaycastHit2D[] hitBuffer = new RaycastHit2D[8];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("需要在NPC上挂载 Rigidbody2D，碰撞才能正常工作。");
        }
        else
        {
            // 运动学刚体移动更稳定；若需要被推开、受力，可设为 Dynamic
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            // 如需：rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // 若用户未在 Inspector 指定 collisionMask，则按物理矩阵自动推断
            if (collisionMask.value == 0)
            {
                collisionMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
                // 一般不把自己这层当障碍
                collisionMask &= ~(1 << gameObject.layer);
            }

            moveFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = collisionMask,
                useTriggers = false // 触发器不当做阻挡。如需避让触发器，将其设为 true
            };
        }
    }

    void Start()
    {
        if (movePoints.Length != moveSpeeds.Length)
        {
            Debug.LogWarning("movePoints 和 moveSpeeds 长度不一致，将使用最短长度。");
        }

        if (moveDistances.Length != movePoints.Length)
        {
            Debug.LogWarning("moveDistances 长度与 movePoints 不一致，将补齐为相同长度。");
            System.Array.Resize(ref moveDistances, movePoints.Length);
            for (int i = 0; i < moveDistances.Length; i++)
            {
                if (moveDistances[i] == 0f)
                    moveDistances[i] = 0.05f;
            }
        }

        SetVisible(!hideAtStart);
    }

    void Update()
    {
        if (!isTriggered && PlayerMovementMonitor.awake && doorTeleport.previousSceneName == lastscenename && gameover2.momisrunning == true)
        {
            isTriggered = true;
            delayTimer = moveDelay;
        }

        if (isTriggered && !isMoving)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                isMoving = true;
                SetVisible(true); // 开始移动前启用可见与碰撞
            }
        }
    }

    void FixedUpdate()
    {
        if (!isMoving || currentStage >= movePoints.Length) return;

        Vector2 target = (Vector2)movePoints[currentStage].position;
        float speed = moveSpeeds[currentStage];

        // 当前与目标的向量
        Vector2 currentPos = rb ? rb.position : (Vector2)transform.position;
        Vector2 toTarget = target - currentPos;

        // 本帧最大可移动距离
        float step = speed * Time.fixedDeltaTime;
        if (step <= 0f)
            return;

        // 期望位移（不超过 step）
        Vector2 desiredDelta = toTarget;
        float dist = desiredDelta.magnitude;
        if (dist > step)
            desiredDelta = desiredDelta * (step / dist);

        // 计算“自动绕边”后实际可走的位移
        Vector2 allowedDelta = ComputeAvoidedDelta(desiredDelta);

        Vector2 nextPos = currentPos + allowedDelta;

        // 推进刚体位置
        if (rb)
            rb.MovePosition(nextPos);
        else
            transform.position = nextPos; // 兜底，不推荐无刚体

        // 抵达判定：用计划的 nextPos 与目标距离
        if (Vector2.Distance(nextPos, target) < moveDistances[currentStage])
        {
            currentStage++;
            if (currentStage >= movePoints.Length)
            {
                isMoving = false;
                hasFinishedMoving = true;
                if (disappearAtEnd)
                    SetVisible(false);
            }
        }
    }

    // 计算在碰撞约束下，本帧允许的位移（包含自动贴边滑动与微调）
    private Vector2 ComputeAvoidedDelta(Vector2 delta)
    {
        if (delta == Vector2.zero || rb == null)
            return Vector2.zero == delta ? delta : Vector2.zero;

        // 1) 整步可走
        if (CanMove(delta))
            return delta;

        // 2) 主轴/副轴（选择分量更大的轴为主轴）
        bool xIsPrimary = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);
        Vector2 primary = xIsPrimary ? new Vector2(delta.x, 0f) : new Vector2(0f, delta.y);
        Vector2 secondary = xIsPrimary ? new Vector2(0f, delta.y) : new Vector2(delta.x, 0f);

        if (primary != Vector2.zero && CanMove(primary))
            return primary;

        if (secondary != Vector2.zero && CanMove(secondary))
            return secondary;

        // 3) 垂直微调“挤边”
        if (edgeNudgeRatio > 0f)
        {
            Vector2 perpDir = new Vector2(-delta.y, delta.x).normalized;
            Vector2 nudge = perpDir * (delta.magnitude * edgeNudgeRatio);
            if (nudge != Vector2.zero && CanMove(nudge))
                return nudge;

            nudge = -nudge;
            if (nudge != Vector2.zero && CanMove(nudge))
                return nudge;
        }

        // 4) 无法前进
        return Vector2.zero;
    }

    // 基于 Rigidbody2D.Cast 的扫掠检测：这一步是否会撞到 collisionMask
    private bool CanMove(Vector2 delta)
    {
        float distance = delta.magnitude;
        if (distance < 0.0001f) return true;

        Vector2 dir = delta.normalized;
        int hitCount = rb.Cast(dir, moveFilter, hitBuffer, distance + skin);
        return hitCount == 0;
    }

    void SetVisible(bool visible)
    {
        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
            renderer.enabled = visible;

        foreach (var collider in GetComponentsInChildren<Collider2D>(true))
            collider.enabled = visible;
    }
}
