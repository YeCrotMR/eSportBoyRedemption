using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex; // 存储该物品槽的索引
    private InventoryManager inventoryManager; // 引用 InventoryManager，修改为私有
    public float expandedClickArea = 10f; // 控制点击范围扩大多少像素，默认扩展 10 像素

    private RectTransform rectTransform; // 用于获取物品槽的矩形区域

    void Start()
    {
        // 自动找到 InventoryManager 实例
        inventoryManager = FindObjectOfType<InventoryManager>();

        // 获取物品槽的 RectTransform
        rectTransform = GetComponent<RectTransform>();
    }

    // 点击事件处理方法
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryManager != null && IsClickWithinExpandedArea(eventData))
        {
            inventoryManager.SetSelectedIndex(slotIndex); // 更新选中的物品槽
        }
    }

    // 判断点击是否在扩展范围内
    private bool IsClickWithinExpandedArea(PointerEventData eventData)
    {
        // 将世界坐标转换为物品槽的局部坐标系
        Vector2 localPosition = rectTransform.InverseTransformPoint(eventData.position);

        // 物品槽的尺寸（宽度和高度）
        Vector2 slotSize = rectTransform.rect.size;

        // 判断点击是否在物品槽区域及其扩展区域内
        // 扩展的范围就是在原有的宽度和高度基础上，加上 `expandedClickArea`
        float expandedWidth = slotSize.x / 2 + expandedClickArea;
        float expandedHeight = slotSize.y / 2 + expandedClickArea;

        // 计算点击点到物品槽中心的距离
        if (Mathf.Abs(localPosition.x) <= expandedWidth && Mathf.Abs(localPosition.y) <= expandedHeight)
        {
            return true; // 点击在扩展区域内
        }

        return false; // 点击在扩展区域外
    }
}
