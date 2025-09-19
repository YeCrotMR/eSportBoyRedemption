using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string text;                     // 对话文本
    public Sprite portrait;                 // 对应立绘图像

    public bool hasChoices = false;
    public DialogueChoice[] choices;
    public string speakerName;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public DialogueLine[] nextDialogue;
    [HideInInspector] public bool wasChosen = false;
}





