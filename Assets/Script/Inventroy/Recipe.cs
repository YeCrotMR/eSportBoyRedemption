using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;

    [Header("合成材料")]
    public List<Item> requiredItems; // 可以重复，比如需要2个“木头”

    [Header("合成结果")]
    public Item resultItem;
}
