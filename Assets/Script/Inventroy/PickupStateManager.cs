using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PickupStateManager : MonoBehaviour
{
    public static PickupStateManager Instance;

    void Awake()
    {
        // 保证单例
        Instance = this;
        // 不使用 DontDestroyOnLoad，场景切换时允许重置
    }

    /// <summary>
    /// 统一刷新场景中所有 ItemPickup 显示状态
    /// 根据存档中的 inventoryItemNames 判断哪些物品玩家已有
    /// </summary>
    public void RefreshWorldItems()
    {
        var currentData = GameManager.Instance?.currentSaveData;
        if (currentData == null || currentData.inventoryItemNames == null) return;

        // 使用 HashSet 加速查找
        var invSet = new HashSet<string>(currentData.inventoryItemNames);

        var pickups = GameObject.FindObjectsOfType<ItemPickup>(true); // true 找到隐藏的项
        foreach (var pickup in pickups)
        {
            if (pickup == null || pickup.item == null) continue;

            if (invSet.Contains(pickup.item.itemName))
            {
                // 存档里玩家已有该物品 => 隐藏场景中的该物品
                pickup.gameObject.SetActive(false);
                pickup.hasPickedUp = true;
            }
            else
            {
                // 玩家物品栏没有该物品 => 显示到场景
                pickup.gameObject.SetActive(true);
                pickup.hasPickedUp = false;

                // 可选：重置 outline / 其他显示效果
                var outline = pickup.GetComponent<SpriteOutline>();
                if (outline != null) outline.DisableOutline();
            }
        }
    }
}
