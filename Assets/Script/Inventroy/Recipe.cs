using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Recipe")]
public class Recipe : ScriptableObject
{
    public string recipeName;

    [Header("�ϳɲ���")]
    public List<Item> requiredItems; // �����ظ���������Ҫ2����ľͷ��

    [Header("�ϳɽ��")]
    public Item resultItem;
}
