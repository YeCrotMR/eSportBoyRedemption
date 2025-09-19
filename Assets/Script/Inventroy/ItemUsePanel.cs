using UnityEngine;
using UnityEngine.UI;

public class ItemUsePanel : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject panelRoot;
    public Image itemImage;
    public Button useButton;     // 使用按钮
    public Button closeButton;

    private Item currentItem;
    private bool isVisible = false;

    void Start()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (useButton != null)
            useButton.onClick.AddListener(OnUseItem);

        if (closeButton != null)
            closeButton.onClick.AddListener(HidePanel);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isVisible) HidePanel();
            else
            {
                // 从 InventoryManager 取当前物品
                var inv = InventoryManager.Instance;
                if (inv != null && inv.selectedIndex >= 0 && inv.selectedIndex < inv.inventory.Count)
                {
                    ShowPanel(inv.inventory[inv.selectedIndex]);
                }
            }
        }
    }

    public void ShowPanel(Item item)
    {
        if (panelRoot == null) return;

        panelRoot.SetActive(true);
        isVisible = true;

        currentItem = item;

        if (itemImage != null)
            itemImage.sprite = item.icon;

        // ✅ 如果物品不可使用，隐藏“使用”按钮
        if (useButton != null)
            useButton.gameObject.SetActive(item.canUse);
    }

    public void HidePanel()
    {
        if (panelRoot == null) return;

        panelRoot.SetActive(false);
        isVisible = false;
        currentItem = null;
    }

    private void OnUseItem()
    {
        if (currentItem == null) return;

        if (currentItem.canUse)
        {
            Debug.Log("使用物品: " + currentItem.itemName);
            if (!string.IsNullOrEmpty(currentItem.useMessage))
                InventoryManager.Instance.ShowPopup(currentItem.useMessage);

            // 示例：用完就删除
            InventoryManager.Instance.RemoveItemByName(currentItem.itemName);

            HidePanel();
        }
    }
}
