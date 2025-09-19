using UnityEngine;
using System.IO;

public class SaveSlotManager : MonoBehaviour
{
    public SaveSlotUI[] slots;
    public GameObject overideui;
    public GameObject savesuceed;
    public GameObject readsuceed;
    public UIManager uiManager;
    
    public bool isSaving = true; // true: 保存模式；false: 读取模式

    private void Start()
    {
        RefreshAllSlots();
    }

    public void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i;
            bool hasSave = SaveSystem.HasSave(index);
            slots[i].slotIndex = index;
            slots[i].SetState(hasSave);
            slots[i].onClick = OnSlotClicked;
        }
    }

    public void OnSlotClicked(int slot)
    {
        // for (int i = 0; i < slots.Length; i++)
        // {
        //     int index = i;
        // }

        bool hasSave = SaveSystem.HasSave(slot);
        if (hasSave)
        {
            uiManager.PushUI(overideui);
        }else{
            uiManager.PushUI(savesuceed);
        }
    }

    public void OnReadSlotClicked(int slot)
    {
        bool hasSave = SaveSystem.HasSave(slot);
        if (hasSave)
        {
            uiManager.PushUI(readsuceed);
        }else{
        }
    }
}
