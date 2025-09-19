using UnityEngine;

public class NPCcontinueMovephone : MonoBehaviour
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
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float skin = 0.01f;
    [Range(0f, 0.5f)] [SerializeField] private float edgeNudgeRatio = 0.2f;

    [Header("触发与标识")]
    [SerializeField] public string lastscenename = "";
    [SerializeField] private NPCId idProvider;

    public bool hasFinishedMoving = false;

    public int currentStage = 0;
    public bool isMoving = false;
    public bool isTriggered = false; // 非 static
    public static bool momroomisTriggered = false;
    public float delayTimer = 0f;

    public Rigidbody2D rb;
    public ContactFilter2D moveFilter;
    public RaycastHit2D[] hitBuffer = new RaycastHit2D[8];

    public string SceneKey => gameObject.scene.path; // 用 path 更稳
    public int CurrentStage => currentStage;
    
    public bool IsVisible
    {
        get
        {
            var sr = GetComponentInChildren<SpriteRenderer>(true);
            return sr == null ? true : sr.enabled;
        }
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (idProvider == null) idProvider = GetComponent<NPCId>();

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            if (collisionMask.value == 0)
            {
                collisionMask = Physics2D.GetLayerCollisionMask(gameObject.layer);
                collisionMask &= ~(1 << gameObject.layer);
            }
            moveFilter = new ContactFilter2D { useLayerMask = true, layerMask = collisionMask, useTriggers = false };
        }

        // 先尝试恢复（尽量早，避免被其他逻辑覆盖）
        RestoreFromMemoryIfAny();

        // 初始可见性（若已完成移动就按结果显示）
        if (!hasFinishedMoving)
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
                if (moveDistances[i] == 0f) moveDistances[i] = 0.05f;
        }
    }

    private void Update()
    {
        if (InventoryManager.Instance.HasItem("iphone16 pro max") ||InventoryManager.Instance.HasItem("大门钥匙"))
        {
            isTriggered = true;
            momroomisTriggered = true;
            delayTimer = moveDelay;
        }

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

        Vector2 target = (Vector2)movePoints[currentStage].position;
        float speed = moveSpeeds[currentStage];

        Vector2 currentPos = rb ? rb.position : (Vector2)transform.position;
        Vector2 toTarget = target - currentPos;

        float step = speed * Time.fixedDeltaTime;
        if (step <= 0f) return;

        Vector2 desiredDelta = toTarget;
        float dist = desiredDelta.magnitude;
        if (dist > step) desiredDelta *= (step / dist);

        Vector2 allowedDelta = ComputeAvoidedDelta(desiredDelta);
        Vector2 nextPos = currentPos + allowedDelta;

        if (rb) rb.MovePosition(nextPos);
        else transform.position = nextPos;

        if (Vector2.Distance(nextPos, target) < moveDistances[currentStage])
        {
            currentStage++;
            if (currentStage >= movePoints.Length)
            {
                isMoving = false;
                hasFinishedMoving = true;
                SaveToMemory(); // 结束时保存
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
        float d = delta.magnitude;
        if (d < 0.0001f) return true;
        Vector2 dir = delta.normalized;
        int hitCount = rb.Cast(dir, moveFilter, hitBuffer, d + skin);
        return hitCount == 0;
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in GetComponentsInChildren<SpriteRenderer>(true)) r.enabled = visible;
        foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.enabled = visible;
    }

    private void OnDisable() => SaveToMemory();
    private void OnDestroy() => SaveToMemory();

    private void SaveToMemory()
    {
        if (NPCStateManager.Instance == null || idProvider == null) return;

        var s = new NPCStateManager.NPCState
        {
            position = transform.position,
            stage = currentStage,
            finished = hasFinishedMoving,
            visible = GetComponentInChildren<SpriteRenderer>(true)?.enabled ?? true
        };
        NPCStateManager.Instance.SaveState(SceneKey, idProvider.Id, s);
    }

    private void RestoreFromMemoryIfAny()
    {
        if (NPCStateManager.Instance == null || idProvider == null) return;

        if (NPCStateManager.Instance.TryGetState(SceneKey, idProvider.Id, out var s))
        {
            transform.position = s.position;
            currentStage = Mathf.Clamp(s.stage, 0, movePoints.Length);
            hasFinishedMoving = s.finished;

            isMoving = false;
            isTriggered = hasFinishedMoving; // 已完成则不再触发移动
            momroomisTriggered = hasFinishedMoving;
            SetVisible(s.visible);

            Debug.Log($"[NPCcontinueMovernormal] Restored {idProvider.Id} at {s.position}, stage={s.stage}, finished={s.finished}");
        }
        else
        {
            Debug.Log($"[NPCcontinueMovernormal] No saved state for {idProvider?.Id} in {SceneKey}");
        }
    }
}