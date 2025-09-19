using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(TilemapRenderer), typeof(Tilemap))]
public class TilemapYAxisSorting : MonoBehaviour
{
    private TilemapRenderer tilemapRenderer;
    private Tilemap tilemap;

    [Header("参考点，用于计算 Y 排序")]
    public Transform referencePoint;

    [Header("透明范围控制点")]
    public Transform topPoint;
    public Transform bottomPoint;

    [Header("透明度设置")]
    [Range(0, 1)] public float hiddenAlpha = 0.5f;
    [Range(0, 10)] public float fadeSpeed = 3f;

    private Transform player;
    private float targetAlpha = 1f;

    void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        tilemap = GetComponent<Tilemap>();

        if (referencePoint == null)
            referencePoint = transform;

        // 自动寻找场景中 Player 标签的物体
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("TilemapYAxisSorting: 场景中找不到 Tag 为 'Player' 的物体");

        if (topPoint == null || bottomPoint == null)
            Debug.LogWarning("TilemapYAxisSorting: 透明范围点未设置，将不会自动半透明");
    }

    void LateUpdate()
    {
        // 根据参考点 Y 值调整 sortingOrder
        tilemapRenderer.sortingOrder = Mathf.RoundToInt(-referencePoint.position.y * 100);

        if (player != null && topPoint != null && bottomPoint != null)
        {
            // 判断主角是否在范围内
            if (player.position.y >= bottomPoint.position.y && player.position.y <= topPoint.position.y)
                targetAlpha = hiddenAlpha;
            else
                targetAlpha = 1f;
        }

        // 平滑过渡透明度
        Color c = tilemap.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
        tilemap.color = c;
    }
}
