using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Collider2D))] // 建议这个碰撞体做“交互范围”，勾选 isTrigger
public class DoorToggle2D : MonoBehaviour
{
    [Header("门位置（两个空物体）")]
    public Transform closedPoint;   // 关门位置
    public Transform openPoint;     // 开门位置

    [Header("交互")]
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;
    public bool requirePlayerInRange = true;

    [Header("移动")]
    [Tooltip("完成一次开/关的时间（秒）")]
    public float moveDuration = 0.6f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("如果门上有 Rigidbody2D，优先用 MovePosition 平滑推动")]
    public bool useRigidbodyIfAvailable = true;
    [Tooltip("允许在移动过程中再次按键，直接反向")]
    public bool allowRetoggleWhileMoving = true;

    [Header("阻塞检测（可选）")]
    public bool preventCloseIfBlocked = true;
    public LayerMask blockMask;            // 可能阻挡关门的层（比如 Player / NPC）
    public Vector2 closeCheckSize = new(0.8f, 1.8f);
    public float   closeCheckAngle = 0f;   // 旋转角度（通常 0）
    public bool autoSizeFromCollider = true;

    [Header("初始状态")]
    public bool openOnStart = false;

    [Header("保存（可选）")]
    public bool persistWithPlayerPrefs = false;
    public string prefsKey = "door.default";

    // —— 运行时 —— //
    bool isBusy;
    bool playerInRange;
    Coroutine moveCo;
    Rigidbody2D rb;
    Collider2D[] selfCols;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        selfCols = GetComponentsInChildren<Collider2D>(true);

        if (autoSizeFromCollider)
        {
            var c = GetComponentsInChildren<Collider2D>(true)
                        .FirstOrDefault(cc => !cc.isTrigger);
            if (c != null)
            {
                var b = c.bounds;
                closeCheckSize = b.size;
            }
        }

        if (persistWithPlayerPrefs && PlayerPrefs.HasKey(prefsKey))
            openOnStart = PlayerPrefs.GetInt(prefsKey, 0) == 1;

        // 放到初始位置
        Vector3 startPos = (openOnStart ? openPoint : closedPoint).position;
        if (rb && useRigidbodyIfAvailable) rb.position = startPos;
        else transform.position = startPos;

        isOpen = openOnStart;
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (!requirePlayerInRange || playerInRange)
                TryToggle();
        }
    }

    public void TryToggle()
    {
        // 正在动且不允许中断
        if (isBusy && !allowRetoggleWhileMoving) return;

        bool wantOpen = !isOpen;

        // 要关门时，做阻塞检查（避免夹人）
        if (!wantOpen && preventCloseIfBlocked && IsCloseBlocked())
            return;

        // 可以打断当前移动，直接反向
        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(MoveDoor(wantOpen));
    }

    IEnumerator MoveDoor(bool toOpen)
    {
        isBusy = true;

        Vector3 from = (rb && useRigidbodyIfAvailable) ? (Vector3)rb.position : transform.position;
        Vector3 to   = (toOpen ? openPoint : closedPoint).position;

        float t = 0f;
        while (t < 1f)
        {
            // 若在移动中又被要求反向，外部会 StopCoroutine 并开启新协程
            t += Time.deltaTime / Mathf.Max(0.0001f, moveDuration);
            float k = ease.Evaluate(Mathf.Clamp01(t));
            Vector3 pos = Vector3.LerpUnclamped(from, to, k);

            if (rb && useRigidbodyIfAvailable)
                rb.MovePosition(pos);
            else
                transform.position = pos;

            yield return null;
        }

        // 归位一次
        if (rb && useRigidbodyIfAvailable) rb.MovePosition(to);
        else transform.position = to;

        isOpen = toOpen;
        isBusy = false;
        moveCo = null;

        if (persistWithPlayerPrefs)
        {
            PlayerPrefs.SetInt(prefsKey, isOpen ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    bool IsCloseBlocked()
    {
        // 在“关门目标位置”做一个 OverlapBox，若命中除自身以外的碰撞体，则视为阻塞
        Vector2 center = (Vector2)closedPoint.position;
        var hits = Physics2D.OverlapBoxAll(center, closeCheckSize, closeCheckAngle, blockMask);
        foreach (var h in hits)
        {
            if (h == null) continue;
            if (selfCols.Contains(h)) continue;     // 忽略自己的碰撞体
            return true;
        }
        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            playerInRange = false;
    }

    // —— 可视化 —— //
    void OnDrawGizmosSelected()
    {
        if (closedPoint && openPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(closedPoint.position, openPoint.position);
            Gizmos.DrawSphere(closedPoint.position, 0.06f);
            Gizmos.DrawSphere(openPoint.position, 0.06f);
        }

        if (closedPoint)
        {
            Gizmos.color = Color.cyan;
            Vector3 c = closedPoint.position;
            Vector3 s = new Vector3(closeCheckSize.x, closeCheckSize.y, 0.01f);
            Matrix4x4 m = Matrix4x4.TRS(c, Quaternion.Euler(0, 0, closeCheckAngle), Vector3.one);
            Gizmos.matrix = m;
            Gizmos.DrawWireCube(Vector3.zero, s);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }

    [Header("唯一 ID（必须不同场景中唯一）")]
    public string doorID;

    private bool isOpen;

    public bool IsOpen => isOpen;


    public void Toggle()
    {
        SetOpen(!isOpen);
    }

        public void SetOpen(bool open)
    {
        isOpen = open;
        Vector3 targetPos = (open ? openPoint.position : closedPoint.position);

        if (rb && useRigidbodyIfAvailable)
            rb.position = targetPos;
        else
            transform.position = targetPos;
    }

}
