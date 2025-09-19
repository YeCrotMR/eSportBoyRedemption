using UnityEngine;

public class WorldItem : MonoBehaviour
{
    [Tooltip("唯一ID，编辑器里手动赋值，或代码生成")]
    public string itemID;

    public Item itemData;  // 对应的物品信息（ScriptableObject）
}