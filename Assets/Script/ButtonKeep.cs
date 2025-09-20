using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonKeep : MonoBehaviour
{
    public int parameterToSend;

    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn == null || GameManager.Instance == null)
        {
            Debug.LogWarning("按钮绑定失败");
            return;
        }
        Debug.Log("内射所有人");
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => GameManager.Instance.LoadGame(parameterToSend));
    }
}


