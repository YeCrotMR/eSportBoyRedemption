using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform target;

    [Header("边界控制")]
    public Transform minBoundObject;
    public Transform maxBoundObject;
    private Vector2 minPosition;
    private Vector2 maxPosition;

    [Header("缩放设置")]
    public float zoomSpeed = 2f;
    public float minZoom = 3f;
    public float maxZoom = 10f;
    public float initialZoom = 5f;

    [Header("跟随设置")]
    public bool enableSmoothFollow = true;
    public float followSmoothTime = 0.2f;
    private Vector3 velocity = Vector3.zero;

    [Header("像素对齐（可选）")]
    public bool enablePixelPerfect = false;
    public float pixelsPerUnit = 32f;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (!cam.orthographic)
        {
            Debug.LogError("该脚本仅适用于正交摄像机");
        }

        cam.orthographicSize = Mathf.Clamp(initialZoom, minZoom, maxZoom);

        if (minBoundObject && maxBoundObject)
        {
            minPosition = minBoundObject.position;
            maxPosition = maxBoundObject.position;

            Vector3 centerPos = (minPosition + maxPosition) / 2f;
            centerPos.z = transform.position.z;
            transform.position = centerPos;
        }
        else if (target)
        {
            Vector3 initPos = target.position;
            initPos.z = transform.position.z;
            transform.position = initPos;
        }
    }

    void LateUpdate()
    {
        HandleZoomCenteredOnPlayer();
        FollowPlayer();

        if (enablePixelPerfect)
        {
            PixelSnap();
        }
    }

    void HandleZoomCenteredOnPlayer()
    {
        if (!target) return;

        float scroll = 0f;
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.KeypadPlus)) scroll = -1f;    // 放大（正交尺寸减小）
        else if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) scroll = 1f; // 缩小（正交尺寸增大）

        if (Mathf.Approximately(scroll, 0f)) return;

        float oldSize = cam.orthographicSize;
        float newSize = Mathf.Clamp(oldSize + scroll * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        if (Mathf.Approximately(oldSize, newSize)) return;

        Vector3 newCamPos;

        // 修改点：只有当两个维度都能装下边界时才居中，否则跟踪玩家（哪怕只超出一个维度）
        if (IsMapSmallerThanView(newSize))
        {
            newCamPos = (minPosition + maxPosition) / 2f;
            newCamPos.z = transform.position.z;
        }
        else
        {
            // 以玩家为锚点缩放
            Vector3 viewportPos = cam.WorldToViewportPoint(target.position);
            cam.orthographicSize = newSize;
            Vector3 afterZoom = cam.ViewportToWorldPoint(viewportPos);
            Vector3 delta = target.position - afterZoom;
            newCamPos = transform.position + delta;
            newCamPos = ClampPosition(newCamPos, newSize);
        }

        cam.orthographicSize = newSize;
        transform.position = newCamPos;
    }

    void FollowPlayer()
    {
        if (!target) return;

        // 修改点：只有两个维度都能完全放下时才居中，否则跟踪玩家（并按轴夹取）
        if (IsMapSmallerThanView())
        {
            Vector3 center = (minPosition + maxPosition) / 2f;
            center.z = transform.position.z;
            transform.position = center;
        }
        else
        {
            Vector3 targetPos = target.position;
            targetPos.z = transform.position.z;
            targetPos = ClampPosition(targetPos);

            if (enableSmoothFollow)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, followSmoothTime);
            }
            else
            {
                transform.position = targetPos;
            }
        }
    }

    // 按轴分别处理的夹取：能完全装下的轴居中，装不下的轴进行夹取
    Vector3 ClampPosition(Vector3 rawPos, float? overrideZoom = null)
    {
        if (!minBoundObject || !maxBoundObject) return rawPos;

        float zoom = overrideZoom ?? cam.orthographicSize;
        float vertExtent = zoom;
        float horzExtent = zoom * cam.aspect;

        float boundWidth = maxPosition.x - minPosition.x;
        float boundHeight = maxPosition.y - minPosition.y;

        float minX = minPosition.x + horzExtent;
        float maxX = maxPosition.x - horzExtent;
        float minY = minPosition.y + vertExtent;
        float maxY = maxPosition.y - vertExtent;

        // 水平：如果视野宽度 >= 边界宽度，居中X；否则夹取X
        if (horzExtent * 2f >= boundWidth)
        {
            rawPos.x = (minPosition.x + maxPosition.x) * 0.5f;
        }
        else
        {
            rawPos.x = Mathf.Clamp(rawPos.x, minX, maxX);
        }

        // 垂直：如果视野高度 >= 边界高度，居中Y；否则夹取Y
        if (vertExtent * 2f >= boundHeight)
        {
            rawPos.y = (minPosition.y + maxPosition.y) * 0.5f;
        }
        else
        {
            rawPos.y = Mathf.Clamp(rawPos.y, minY, maxY);
        }

        return rawPos;
    }

    // 修改点：只有当两个维度都能完全容纳边界时才返回 true
    bool IsMapSmallerThanView(float? overrideZoom = null)
    {
        if (!minBoundObject || !maxBoundObject) return false;

        float zoom = overrideZoom ?? cam.orthographicSize;
        float vertExtent = zoom;
        float horzExtent = vertExtent * cam.aspect;

        float boundWidth = maxPosition.x - minPosition.x;
        float boundHeight = maxPosition.y - minPosition.y;

        // AND：两个维度都能容纳时才认为“地图小于视野”
        return (horzExtent * 2f >= boundWidth) && (vertExtent * 2f >= boundHeight);
    }

    void PixelSnap()
    {
        float unitsPerPixel = 1f / pixelsPerUnit;
        Vector3 pos = transform.position;
        pos.x = Mathf.Round(pos.x / unitsPerPixel) * unitsPerPixel;
        pos.y = Mathf.Round(pos.y / unitsPerPixel) * unitsPerPixel;
        transform.position = pos;
    }
}
