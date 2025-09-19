using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex; // 槽位编号
    public TextMeshProUGUI labelText;
    public Button button;

    public Action<int> onClick;

    public void SetState(bool hasSave)
    {
        labelText.text = hasSave ? $"存档 {slotIndex}" : "无存档";
    }
    
    // public void OnSlotClick()
    // {
    //     onClick?.Invoke(slotIndex);
    // }
    
    // private void Awake()
    // {
    //     button.onClick.AddListener(() =>
    //     {
    //         onClick?.Invoke(slotIndex);
    //     });
    // }
}
