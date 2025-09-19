using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea(2, 5)]
    public string description;

    [Header("使用设置")]
    public bool canUse;             // 是否能被使用
    public string useMessage;       // 使用时的提示（比如“恢复了10点生命”）
}
