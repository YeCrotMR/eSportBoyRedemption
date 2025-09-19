using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(NPCId))]
public class NPCcontinueMovernormal : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] public Transform[] movePoints;
    [SerializeField] float[]     moveDistances;
    [SerializeField] float[]     moveSpeeds;

    [Header("可选参数")]
    [SerializeField] bool hideAtStart    = true;
    [SerializeField] bool disappearAtEnd = true;
    [SerializeField] float moveDelay     = 0f;

    [Header("碰撞绕边设置")]
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float     skin           = 0.01f;
    [Range(0,0.5f)] [SerializeField] float edgeNudgeRatio = 0.2f;

    [Header("触发与标识")]
    [SerializeField] string lastSceneName = "";
    [SerializeField] NPCId  idProvider;

    // 运行时状态
    public bool hasFinishedMoving = false;
    
    public int  currentStage      = 0;
    public bool isMoving          = false;
    public bool isTriggered       = false;
    public int moveint = 0;
    public static bool momroomisTriggered = false;
    float delayTimer = 0f;
    private bool isPlayerInRange = false;

    public UniversalExpressionEvaluator evaluator;
    readonly Dictionary<string, object> vars = new();
    public NPCcontinueMovernormal copy;

    Rigidbody2D     rb;
    ContactFilter2D moveFilter;
    RaycastHit2D[]  hitBuffer = new RaycastHit2D[8];

    // 统一用 scene.name 作为 key
    string SceneKey => gameObject.scene.name;
    public int CurrentStage => currentStage;
    public bool IsVisible => GetComponentInChildren<SpriteRenderer>(true)?.enabled ?? true;

    // 标记本次是否从内存还原成功
    bool restored = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (idProvider == null) idProvider = GetComponent<NPCId>();

        if (idProvider == null || string.IsNullOrEmpty(idProvider.Id))
        {
            Debug.LogWarning($"[NPC] {name} 缺少 NPCId 或 Id 为空，无法保存/读取位置");
        }

        // 设置碰撞过滤
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (collisionMask.value == 0)
                collisionMask = Physics2D.GetLayerCollisionMask(gameObject.layer) & ~(1 << gameObject.layer);

            moveFilter = new ContactFilter2D {
                useLayerMask = true,
                layerMask    = collisionMask,
                useTriggers  = false
            };
        }

        RestoreFromMemoryIfAny();

        // 只有没有还原时才应用默认可见性，避免覆盖存档
        if (!restored && !hasFinishedMoving)
            SetVisible(!hideAtStart);
    }

    private void Start()
    {
        // 长度检查
        if (movePoints.Length != moveSpeeds.Length)
            Debug.LogWarning("movePoints 和 moveSpeeds 长度不一致，将使用最短长度。");
        if (moveDistances.Length != movePoints.Length)
        {
            System.Array.Resize(ref moveDistances, movePoints.Length);
            for (int i = 0; i < moveDistances.Length; i++)
                if (moveDistances[i] == 0f)
                    moveDistances[i] = 0.05f;
        }
    }

    private void Update()
    {
        // 让 open mover 启动时只看门是否关闭
        vars["open"] = isPlayerInRange && Input.GetKeyDown(KeyCode.E) 
                    && !hasFinishedMoving && !isTriggered;

        // 让 close mover 启动时只看门是否已经开了
        vars["copy"] = isPlayerInRange && Input.GetKeyDown(KeyCode.E) 
               && copy?.hasFinishedMoving == true && !isTriggered;


        // 场景跳转触发
        if (!isTriggered && doorTeleport.previousSceneName == lastSceneName && moveint == 0 && evaluator == null)
        {
            moveint = 1;
            isTriggered = true;
            momroomisTriggered = true;
            delayTimer = moveDelay;
        }else if(!isTriggered && evaluator != null && evaluator.EvaluateBool(vars) && moveint == 0 ){
            moveint = 1; 
            isTriggered = true;
            delayTimer = moveDelay;
        }

        // 延迟后开始移动
        if (isTriggered && !isMoving && !hasFinishedMoving)
        {
            delayTimer -= Time.deltaTime;
            if (delayTimer <= 0f)
            {
                isMoving = true;
                SetVisible(true);
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isMoving || currentStage >= movePoints.Length) return;

        Vector2 target     = movePoints[currentStage].position;
        float   speed      = moveSpeeds[currentStage];
        Vector2 currentPos = rb ? rb.position : (Vector2)transform.position;
        Vector2 toTarget   = target - currentPos;

        float step = speed * Time.fixedDeltaTime;
        if (step <= 0f) return;

        Vector2 desiredDelta = toTarget.magnitude > step
                             ? toTarget.normalized * step
                             : toTarget;
        Vector2 allowedDelta = ComputeAvoidedDelta(desiredDelta);
        Vector2 nextPos      = currentPos + allowedDelta;

        if (rb) rb.MovePosition(nextPos);
        else    transform.position = nextPos;

        // 到达当前目标点
        if (Vector2.Distance(nextPos, target) < moveDistances[currentStage])
        {
            currentStage++;
            if (currentStage >= movePoints.Length)
            {
                isMoving          = false;
                hasFinishedMoving = true;
                SaveToMemory();
                if (disappearAtEnd) SetVisible(false);
            }
        }
    }

    // 计算在碰撞约束下，本帧允许的位移（包含自动贴边滑动与微调 + 智能角度扫描）
private Vector2 ComputeAvoidedDelta(Vector2 delta)
{
    if (delta == Vector2.zero || rb == null)
        return Vector2.zero == delta ? delta : Vector2.zero;

    // 1) 直接可走
    if (CanMove(delta))
        return delta;

    // 2) 尝试主轴/副轴（原本逻辑）
    bool xIsPrimary = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y);
    Vector2 primary = xIsPrimary ? new Vector2(delta.x, 0f) : new Vector2(0f, delta.y);
    Vector2 secondary = xIsPrimary ? new Vector2(0f, delta.y) : new Vector2(delta.x, 0f);

    if (primary != Vector2.zero && CanMove(primary))
        return primary;

    if (secondary != Vector2.zero && CanMove(secondary))
        return secondary;

    // 3) 尝试角度扫描：逐步偏转目标方向，寻找可行路径
    float angleStep = 15f;   // 每次旋转角度
    int maxChecks = 12;      // 扫描 12 次（±180°）
    Vector2 dir = delta.normalized;
    float mag = delta.magnitude;

    for (int i = 1; i <= maxChecks; i++)
    {
        float angle = angleStep * i;

        // 左偏
        Vector2 leftDir = Quaternion.Euler(0, 0, angle) * dir;
        if (CanMove(leftDir * mag))
            return leftDir * mag;

        // 右偏
        Vector2 rightDir = Quaternion.Euler(0, 0, -angle) * dir;
        if (CanMove(rightDir * mag))
            return rightDir * mag;
    }

    // 4) 原本的 edgeNudge 挤边逻辑
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

    // 5) 完全走不动
    return Vector2.zero;
}


   private bool CanMove(Vector2 delta)
{
    float distance = delta.magnitude;
    if (distance < 0.0001f) return true;

    Vector2 dir = delta.normalized;
    int hitCount = rb.Cast(dir, moveFilter, hitBuffer, distance + skin);

    // 排除带有 "Player" 标签的物体
    for (int i = 0; i < hitCount; i++)
    {
        if (hitBuffer[i].collider.CompareTag("Player"))
        {
            // 发现玩家物体，继续检测
            continue;
        }

        // 如果有其他物体，返回 false，表示无法继续移动
        return false;
    }

    // 如果没有其他物体，允许移动
    return true;
}

    public void SetVisible(bool visible)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = visible;
        foreach (var c in GetComponentsInChildren<Collider2D>(true))
            c.enabled = visible;
    }

    private void OnDisable() => SaveToMemory();
    private void OnDestroy() => SaveToMemory();

    private void SaveToMemory()
    {
        if (NPCStateManager.Instance == null || idProvider == null) return;

        var s = new NPCStateManager.NPCState
        {
            position  = transform.position,
            stage     = currentStage,
            finished  = hasFinishedMoving,
            visible   = IsVisible,
            triggered = isTriggered
        };
        NPCStateManager.Instance.SaveState(SceneKey, idProvider.Id, s);
        // Debug.Log($"[NPC] SaveToMemory key={SceneKey}|{idProvider.Id} pos={s.position}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }

    private void RestoreFromMemoryIfAny()
    {
        if (NPCStateManager.Instance == null || idProvider == null) return;

        if (NPCStateManager.Instance.TryGetState(SceneKey, idProvider.Id, out var s))
        {
            if (rb != null)
            {
                rb.position        = s.position;
                rb.velocity        = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            else
            {
                transform.position = s.position;
            }

            currentStage          = Mathf.Clamp(s.stage, 0, movePoints.Length);
            hasFinishedMoving     = s.finished;
            isMoving              = false;
            isTriggered           = s.triggered;
            momroomisTriggered    = s.triggered;
            moveint               = s.moveint;
            SetVisible(s.visible);
            restored = true;

            Debug.Log($"[NPC] Restored {idProvider.Id} @ {s.position}, stage={s.stage}, finished={s.finished}, triggered={s.triggered}");
        }
        else
        {
            // Debug.Log($"[NPC] No saved state for key={SceneKey}|{idProvider.Id}");
        }
    }
}
