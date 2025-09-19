using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("按键")]
    public KeyCode equipKey = KeyCode.C;

    [Header("玩家 Animator")]
    public Animator playerAnimator; // 可在 Inspector 指定；为空则自动按 Tag=Player 寻找
    private RuntimeAnimatorController defaultController; // 卸载时恢复

    [Header("装备映射（按物品名）")]
    public List<EquipmentMapping> equipmentMap = new List<EquipmentMapping>();
    // itemName -> AnimatorController 的映射，必要时还能提供 Item 资源用于卸载回背包

    // 当前装备状态
    private bool isEquipped = false;
    private string equippedItemName = null; // 已装备物品名（未装备为 null）
    private Item equippedItemRef = null;    // 从背包取出的原始 Item 引用（读档恢复时可能为 null）

    private void Awake()
    {
        if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);  // 关键
    }
    else
    {
        Destroy(gameObject);
    }

        EnsurePlayerAnimator();
        CaptureDefaultController();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (Input.GetKeyDown(equipKey))
        {
            if (!isEquipped)
                TryEquipFromSelection();
            else
                TryUnequip();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsurePlayerAnimator();

        if (playerAnimator != null)
        {
            // 新场景中玩家的“默认控制器”（用于卸载时恢复）
            defaultController = playerAnimator.runtimeAnimatorController;

            // 若之前处于已装备状态，则重新套用装备控制器
            if (isEquipped && !string.IsNullOrEmpty(equippedItemName))
            {
                if (TryGetControllerForName(equippedItemName, out var ctrl))
                    playerAnimator.runtimeAnimatorController = ctrl;
            }
        }
    }

    private void EnsurePlayerAnimator()
    {
        if (playerAnimator == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerAnimator = player.GetComponent<Animator>();
        }
    }

    private void CaptureDefaultController()
    {
        if (playerAnimator != null)
            defaultController = playerAnimator.runtimeAnimatorController;
    }

    // 从当前选中的物品槽尝试装备
    private void TryEquipFromSelection()
    {
        var inv = InventoryManager.Instance;
        if (inv == null)
        {
            Debug.LogWarning("EquipmentManager: 未找到 InventoryManager.Instance");
            return;
        }

        int idx = inv.selectedIndex;
        if (idx < 0 || idx >= inv.inventory.Count)
        {
            inv.ShowPopup("未选中可装备物品");
            return;
        }

        var item = inv.inventory[idx];
        if (item == null)
        {
            inv.ShowPopup("未选中可装备物品");
            return;
        }

        // 查找映射
        if (!TryGetControllerForName(item.itemName, out var controllerToApply))
        {
            inv.ShowPopup("该物品不可装备");
            return;
        }

        EnsurePlayerAnimator();
        if (playerAnimator == null)
        {
            inv.ShowPopup("未找到玩家 Animator");
            return;
        }

        if (defaultController == null)
            defaultController = playerAnimator.runtimeAnimatorController;

        // 应用控制器
        playerAnimator.runtimeAnimatorController = controllerToApply;

        // 从背包移除这件物品（按索引，避免误删重名）
        equippedItemRef = item;
        equippedItemName = item.itemName;
        isEquipped = true;

        inv.inventory.RemoveAt(idx);
        inv.RefreshUI();
        inv.ShowPopup("已装备：" + equippedItemName);
    }

    // 卸载当前装备并将物品放回背包
    public void TryUnequip()
    {
        if (!isEquipped)
            return;

        var inv = InventoryManager.Instance;
        if (inv == null)
        {
            Debug.LogWarning("EquipmentManager: 未找到 InventoryManager.Instance");
            return;
        }

        // 背包空间检查
        if (inv.inventory.Count >= inv.itemSlots.Length)
        {
            inv.ShowPopup("物品栏已满，无法卸载");
            return;
        }

        EnsurePlayerAnimator();
        if (playerAnimator != null && defaultController != null)
            playerAnimator.runtimeAnimatorController = defaultController;

        // 物品回到背包：优先用原始引用；读档恢复的情况用映射中的 itemAsset
        Item itemToReturn = equippedItemRef;
        if (itemToReturn == null)
        {
            // 尝试用映射中的资源恢复
            if (!TryGetItemAssetForName(equippedItemName, out itemToReturn))
            {
                inv.ShowPopup("未找到物品资源，无法将装备放回背包");
                // 即使没法放回物品，也需要清理装备态并恢复控制器
                isEquipped = false;
                equippedItemName = null;
                equippedItemRef = null;
                return;
            }
        }

        inv.AddItem(itemToReturn);
        inv.ShowPopup("已卸下装备");

        isEquipped = false;
        equippedItemName = null;
        equippedItemRef = null;
    }

    // 按物品名查找对应的 AnimatorController
    private bool TryGetControllerForName(string name, out RuntimeAnimatorController controller)
    {
        foreach (var m in equipmentMap)
        {
            if (m != null && m.controller != null && m.itemName == name)
            {
                controller = m.controller;
                return true;
            }
        }
        controller = null;
        return false;
    }

    // 按物品名查找对应的 Item 资源（用于读档恢复或卸载回包）
    private bool TryGetItemAssetForName(string name, out Item itemAsset)
    {
        foreach (var m in equipmentMap)
        {
            if (m != null && m.itemAsset != null && m.itemName == name)
            {
                itemAsset = m.itemAsset;
                return true;
            }
        }
        itemAsset = null;
        return false;
    }

    // —— 对外静态接口（用于其他脚本/存档系统） ——

    // 1) 简单判断：是否已装备指定名字的物品
    public static bool IsEquipped(string itemName)
    {
        return Instance != null &&
               Instance.isEquipped &&
               Instance.equippedItemName == itemName;
    }

    // 2) 是否装备了任意物品
    public static bool IsEquippedAny()
    {
        return Instance != null && Instance.isEquipped;
    }

    // 3) 获取当前已装备物品名（未装备返回 null）
    public static string GetEquippedName()
    {
        return Instance != null ? Instance.equippedItemName : null;
    }

    // 4) 读档恢复：根据物品名直接恢复装备状态（不会改动背包）
    public static void RestoreEquipFromSave(string itemName)
    {
        if (Instance == null || string.IsNullOrEmpty(itemName))
            return;

        var em = Instance;
        em.EnsurePlayerAnimator();
        if (em.playerAnimator == null)
            return;

        // 新场景或读档时，记录当前默认控制器
        if (em.defaultController == null)
            em.defaultController = em.playerAnimator.runtimeAnimatorController;

        if (em.TryGetControllerForName(itemName, out var ctrl))
        {
            em.playerAnimator.runtimeAnimatorController = ctrl;
            em.isEquipped = true;
            em.equippedItemName = itemName;
            em.equippedItemRef = null; // 读档时通常没有原始引用
        }
        else
        {
            Debug.LogWarning($"EquipmentManager.RestoreEquipFromSave: 未找到物品 '{itemName}' 的控制器映射。");
        }
    }
    public void reset()
    {
        if (!isEquipped)
            return;

        var inv = InventoryManager.Instance;
        if (inv == null)
        {
            Debug.LogWarning("EquipmentManager: 未找到 InventoryManager.Instance");
            return;
        }

        // 背包空间检查
        if (inv.inventory.Count >= inv.itemSlots.Length)
        {
            return;
        }

        EnsurePlayerAnimator();
        if (playerAnimator != null && defaultController != null)
            playerAnimator.runtimeAnimatorController = defaultController;

        // 物品回到背包：优先用原始引用；读档恢复的情况用映射中的 itemAsset
        Item itemToReturn = equippedItemRef;
        if (itemToReturn == null)
        {
            // 尝试用映射中的资源恢复
            if (!TryGetItemAssetForName(equippedItemName, out itemToReturn))
            {
                // 即使没法放回物品，也需要清理装备态并恢复控制器
                isEquipped = false;
                equippedItemName = null;
                equippedItemRef = null;
                return;
            }
        }

        isEquipped = false;
        equippedItemName = null;
        equippedItemRef = null;
    }
}

[System.Serializable]
public class EquipmentMapping
{
    [Tooltip("物品名（需与 Item.itemName 完全一致）")]
    public string itemName;

    [Tooltip("装备后应用的 Animator Controller")]
    public RuntimeAnimatorController controller;

    [Tooltip("该物品的资源引用（ScriptableObject），用于卸载时放回背包或读档恢复")]
    public Item itemAsset;
}
