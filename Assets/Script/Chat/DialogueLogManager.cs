using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueLogManager : MonoBehaviour
{
    public static DialogueLogManager Instance;

    [Header("UI 引用")]
    public GameObject logPanel;             // 整个记录面板（带ScrollRect）
    public ScrollRect scrollRect;           // ScrollRect组件
    public Text logText;                    // 显示对话内容的Text组件

    private List<string> dialogueHistory = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (logPanel != null)
            logPanel.SetActive(false);
    }

    public void ToggleLogPanel()
    {
        if (logPanel != null)
            logPanel.SetActive(!logPanel.activeSelf);
    }

    public void AddEntry(string entry)
{
    if (dialogueHistory.Count > 0 && dialogueHistory[dialogueHistory.Count - 1] == entry)
    {
        // 跳过重复的连续对话
        return;
    }

    dialogueHistory.Add(entry);

    if (logText != null)
    {
        logText.text = string.Join("\n", dialogueHistory);
    }
}

    public void ClearLog()
    {
        dialogueHistory.Clear();
        if (logText != null)
            logText.text = "";
    }
}
