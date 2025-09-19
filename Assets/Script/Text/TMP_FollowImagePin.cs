using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TMP_FollowImagePinV2 : MonoBehaviour
{
    [Header("Target (会随窗口变化的图片)")]
    public RectTransform targetImage;

    [Header("位置（归一化0~1，(0,0)=左下，(1,1)=右上）")]
    [Range(0,1)] public Vector2 normalizedPos = new Vector2(0.5f, 0.5f);
    [Tooltip("以**屏幕像素**为单位的偏移（向右为正X，向上为正Y）")]
    public Vector2 pixelOffsetScreen = Vector2.zero;

    [Header("缩放策略")]
    public bool scaleByLocalScale = true; // true = 缩放 transform.localScale；false = 修改 TMP.fontSize
    public enum ScaleSource { Width, Height, Min, Max }
    public ScaleSource scaleSource = ScaleSource.Height;

    [Tooltip("若为空，脚本会在首次运行时自动记录为参考尺寸（屏幕像素）")]
    public Vector2 referenceImageScreenSize = Vector2.zero;

    [Header("当使用字体缩放时")]
    public float baseFontSize = 36f; // 在 referenceImageScreenSize 下的字号
    public float minFontSize = 8f;
    public float maxFontSize = 200f;

    [Header("选项")]
    public bool forceDisableAutoSize = true;
    public bool debug = false;

    RectTransform _rt;
    TextMeshProUGUI _tmp;
    Canvas _rootCanvas;
    Vector3 _baseLocalScale = Vector3.one;
    Vector2 _lastScreenSize = Vector2.zero;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _tmp = GetComponent<TextMeshProUGUI>();
        _rootCanvas = GetComponentInParent<Canvas>();
        _baseLocalScale = _rt.localScale;
        if (_tmp == null) Debug.LogWarning("TMP_FollowImagePinV2: 没找到 TextMeshProUGUI 组件。", this);
        if (forceDisableAutoSize && _tmp != null) _tmp.enableAutoSizing = false;
    }

    void LateUpdate()
    {
        if (targetImage == null) return;
        if (_rootCanvas == null) _rootCanvas = GetComponentInParent<Canvas>();
        if (_rootCanvas == null) return;

        // 确保 UI 布局已更新（必要时）；会有开销，但可帮助避免读取到旧 rect。
        Canvas.ForceUpdateCanvases();

        // 1) 计算图片在屏幕空间的四角（像素）
        Vector3[] corners = new Vector3[4];
        targetImage.GetWorldCorners(corners); // world corners: bl=0, tl=1, tr=2, br=3

        // world -> screen (兼容各种 Canvas render mode)
        Vector2 blScreen = RectTransformUtility.WorldToScreenPoint(_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera, corners[0]);
        Vector2 trScreen = RectTransformUtility.WorldToScreenPoint(_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera, corners[2]);
        Vector2 sizeScreen = trScreen - blScreen;

        if (sizeScreen.x <= 0 || sizeScreen.y <= 0) return;

        // 记录 reference（首次有效时）
        if (referenceImageScreenSize == Vector2.zero)
        {
            referenceImageScreenSize = sizeScreen;
            if (debug) Debug.Log($"[TMP_FollowImagePinV2] recorded referenceImageScreenSize = {referenceImageScreenSize}", this);
        }

        // 2) 目标屏幕点 = blScreen + sizeScreen * normalized + pixelOffsetScreen
        Vector2 targetScreenPoint = blScreen + Vector2.Scale(sizeScreen, normalizedPos) + pixelOffsetScreen;

        // 3) 将屏幕点转换为文本父 Rect 的本地锚点坐标（anchoredPosition）
        RectTransform parentRect = _rt.parent as RectTransform;
        if (parentRect == null)
        {
            // 如果没有父 Rect（极少见），直接设置世界位置
            Vector3 worldPos = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? (Vector3)targetScreenPoint
                : _rootCanvas.worldCamera.ScreenToWorldPoint(new Vector3(targetScreenPoint.x, targetScreenPoint.y, _rootCanvas.planeDistance));
            _rt.position = worldPos;
        }
        else
        {
            Vector2 localPoint;
            bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, targetScreenPoint, _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _rootCanvas.worldCamera, out localPoint);
            if (!ok) return;
            _rt.anchoredPosition = localPoint;
        }

        // 4) 计算缩放比例（基于屏幕尺寸）
        float scale = ComputeScale(sizeScreen, referenceImageScreenSize, scaleSource);

        // 5) 应用缩放（两种策略）
        if (scaleByLocalScale)
        {
            _rt.localScale = _baseLocalScale * scale;
        }
        else
        {
            if (_tmp != null)
            {
                if (forceDisableAutoSize) _tmp.enableAutoSizing = false;
                float targetFont = Mathf.Clamp(baseFontSize * scale, minFontSize, maxFontSize);
                _tmp.fontSize = targetFont;
            }
        }

        // debug
        if (debug)
        {
            if (_lastScreenSize != sizeScreen)
            {
                Debug.Log($"[TMP_FollowImagePinV2] sizeScreen={sizeScreen} ref={referenceImageScreenSize} scale={scale} anchoredPos={_rt.anchoredPosition}", this);
                _lastScreenSize = sizeScreen;
            }
        }
    }

    float ComputeScale(Vector2 cur, Vector2 @ref, ScaleSource src)
    {
        float cw = Mathf.Max(1e-3f, cur.x), ch = Mathf.Max(1e-3f, cur.y);
        float rw = Mathf.Max(1e-3f, @ref.x), rh = Mathf.Max(1e-3f, @ref.y);
        switch (src)
        {
            case ScaleSource.Width: return cw / rw;
            case ScaleSource.Height: return ch / rh;
            case ScaleSource.Min: return Mathf.Min(cw / rw, ch / rh);
            case ScaleSource.Max: return Mathf.Max(cw / rw, ch / rh);
            default: return ch / rh;
        }
    }
}
