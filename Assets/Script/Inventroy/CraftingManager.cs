using UnityEngine;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance;

    [Header("���п����䷽")]
    public List<Recipe> recipes;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool CanCraft(Recipe recipe)
    {
        Dictionary<string, int> required = new Dictionary<string, int>();
        Dictionary<string, int> owned = new Dictionary<string, int>();

        foreach (Item item in recipe.requiredItems)
        {
            if (!required.ContainsKey(item.itemName))
                required[item.itemName] = 0;
            required[item.itemName]++;
        }

        foreach (Item item in InventoryManager.Instance.inventory)
        {
            if (!owned.ContainsKey(item.itemName))
                owned[item.itemName] = 0;
            owned[item.itemName]++;
        }

        foreach (var pair in required)
        {
            if (!owned.ContainsKey(pair.Key) || owned[pair.Key] < pair.Value)
                return false;
        }

        return true;
    }

    public bool Craft(Recipe recipe)
    {
        if (!CanCraft(recipe))
        {
            InventoryManager.Instance.ShowPopup("ȱ�ٲ��ϣ�");
            return false;
        }

        // ɾ������
        Dictionary<string, int> toRemove = new Dictionary<string, int>();
        foreach (Item item in recipe.requiredItems)
        {
            if (!toRemove.ContainsKey(item.itemName))
                toRemove[item.itemName] = 0;
            toRemove[item.itemName]++;
        }

        foreach (var pair in toRemove)
        {
            for (int i = 0; i < pair.Value; i++)
            {
                InventoryManager.Instance.RemoveItemByName(pair.Key);
            }
        }

        // ��Ӻϳ���
        InventoryManager.Instance.AddItem(recipe.resultItem);
        InventoryManager.Instance.ShowPopup("�ϳɳɹ���" + recipe.resultItem.itemName);
        return true;
    }
}
