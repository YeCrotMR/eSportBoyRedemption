using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject dialogueBox;
    public Text dialogueText;
    public Image characterPortrait;

    [Header("对话内容")]
    public DialogueLine[] dialogueLines;

    [Header("选项相关")]
    public GameObject choicePanel;
    public GameObject choiceButtonPrefab;

    [Header("设置")]
    public float typingSpeed = 0.05f;
    public bool autoStart = true;
    public bool dialogueFinished = false;
    public static bool isInDialogue = false;

    [Header("对话触发设置")]
    public bool triggerOnlyOnce = false;
    public string dialogueID = "";

    public int currentLine = 0;
    public bool isTyping = false;
    public bool canClickNext = false;
    public static DialogueSystem Instance;
    private PlayerMovement player;
    public int clickCount = 0;

    // 统一从 GameManager 拿当前存档（最小改动核心）
    private GameSaveData CurrentSave => GameManager.Instance != null ? GameManager.Instance.currentSaveData : null;

    void Awake()
    {
        Instance = this;
        dialogueBox.SetActive(false);
        choicePanel.SetActive(false);
        player = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>();
    }

    public void SetDialogue(DialogueLine[] newLines)
    {
        dialogueLines = newLines;
    }

    void Start()
    {
        dialogueBox.SetActive(false);

        if (autoStart)
        {
            // 可选但强烈建议：若正在读档，等读档完成再开始，避免误判“只触发一次”
            if (GameManager.Instance != null && GameManager.Instance.isLoadingSave)
                StartCoroutine(StartWhenLoaded());
            else
                StartDialogue();
        }
    }

    private IEnumerator StartWhenLoaded()
    {
        yield return new WaitUntil(() => GameManager.Instance != null && !GameManager.Instance.isLoadingSave);
        StartDialogue();
    }

    void Update()
    {
        if (UIManager.isUIMode) return;

        if (dialogueBox.activeSelf && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && canClickNext)
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = dialogueLines[currentLine].text;
                SetIconPreserveHeight(characterPortrait, dialogueLines[currentLine].portrait);
                isTyping = false;
                canClickNext = true;
            }
            else
            {
                clickCount++;
                currentLine++;
                if (currentLine < dialogueLines.Length)
                {
                    StartCoroutine(TypeLine(dialogueLines[currentLine]));
                }
                else
                {
                    EndDialogue();
                }
            }
        }
    }

    public void StartDialogue()
    {
        if (triggerOnlyOnce && !string.IsNullOrEmpty(dialogueID))
        {
            var list = CurrentSave?.triggeredDialogueIDs;
            if (list != null && list.Contains(dialogueID))
            {
                Debug.Log($"对话 {dialogueID} 已触发过，此次跳过。");
                return;
            }
        }

        isInDialogue = true;
        currentLine = 0;
        clickCount = 0;
        dialogueFinished = false;
        dialogueBox.SetActive(true);
        player.canMove = false;

        // ✅ 新增：对话开始时，停止走路音效
        var audio = player.GetComponent<AudioSource>();
        if (audio != null && audio.isPlaying)
            audio.Stop();

        InventoryManager.Instance?.SetInventoryVisible(false);

        if (dialogueLines != null)
        {
            foreach (var line in dialogueLines)
            {
                if (line.hasChoices && line.choices != null)
                {
                    foreach (var choice in line.choices)
                        choice.wasChosen = false;
                }
            }
            StartCoroutine(TypeLine(dialogueLines[currentLine]));
        }
    }


    IEnumerator TypeLine(DialogueLine line)
    {
        isTyping = true;
        canClickNext = false;
        dialogueText.text = "";
        SetIconPreserveHeight(characterPortrait, line.portrait);

        foreach (char c in line.text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        string displayText = string.IsNullOrEmpty(line.speakerName) ? line.text : $"<b>[{line.speakerName}]</b>：{line.text}";
        DialogueLogManager.Instance?.AddEntry(displayText);

        if (line.hasChoices && line.choices != null && line.choices.Length > 0)
        {
            ShowChoices(line.choices);
        }
        else
        {
            canClickNext = true;
        }
    }

    void EndDialogue()
    {
        isInDialogue = false;
        dialogueBox.SetActive(false);
        dialogueText.text = "";
        characterPortrait.sprite = null;
        player.canMove = true;
        dialogueFinished = true;

        InventoryManager.Instance?.SetInventoryVisible(true);

        // 最小改动：把“只触发一次”的记录写到 GameManager.currentSaveData
        if (triggerOnlyOnce && !string.IsNullOrEmpty(dialogueID))
        {
            var save = CurrentSave;
            if (save != null)
            {
                if (save.triggeredDialogueIDs == null)
                    save.triggeredDialogueIDs = new List<string>();
                if (!save.triggeredDialogueIDs.Contains(dialogueID))
                    save.triggeredDialogueIDs.Add(dialogueID);
            }
        }
    }

    void ShowChoices(DialogueChoice[] choices)
    {
        canClickNext = false;
        choicePanel.SetActive(true);

        foreach (Transform child in choicePanel.transform)
            Destroy(child.gameObject);

        foreach (DialogueChoice choice in choices)
        {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choicePanel.transform);

            var image = btnObj.GetComponent<Image>();
            var button = btnObj.GetComponent<Button>();
            if (image != null) image.enabled = true;
            if (button != null) button.enabled = true;

            Text btnText = btnObj.GetComponentInChildren<Text>();
            if (btnText != null)
            {
                btnText.enabled = true;
                btnText.text = choice.choiceText;

                Shadow shadow = btnText.GetComponent<Shadow>();
                if (shadow != null) shadow.enabled = true;
                }
            button.onClick.AddListener(() =>
            {
                choice.wasChosen = true;
                choicePanel.SetActive(false);
                SetDialogue(choice.nextDialogue);
                StartDialogue();
            });
        }
    }

    private void SetIconPreserveHeight(Image image, Sprite sprite)
    {
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;

        if (sprite == null)
        {
            image.rectTransform.sizeDelta = new Vector2(0, image.rectTransform.sizeDelta.y);
            return;
        }

        float fixedHeight = image.rectTransform.sizeDelta.y;
        float aspectRatio = (float)sprite.texture.width / sprite.texture.height;
        float newWidth = fixedHeight * aspectRatio;

        Vector2 size = image.rectTransform.sizeDelta;
        size.x = newWidth;
        image.rectTransform.sizeDelta = size;
    }

    public void ResetDialogueTrigger()
    {
        // 如果你用的就是 GameManager.currentSaveData，可在这里也清除
        if (!string.IsNullOrEmpty(dialogueID))
        {
            var save = CurrentSave;
            if (save?.triggeredDialogueIDs != null)
            {
                save.triggeredDialogueIDs.Remove(dialogueID);
                Debug.Log($"已清除对话触发记录: {dialogueID}");
            }
        }
    }

    public void SkipDialogue()
    {
        if (!dialogueBox.activeSelf || dialogueLines == null || dialogueLines.Length == 0)
            return;

        StopAllCoroutines();

        for (int i = currentLine; i < dialogueLines.Length; i++)
        {
            var line = dialogueLines[i];
            string displayText = string.IsNullOrEmpty(line.speakerName)
                ? line.text
                : $"<b>[{line.speakerName}]</b>：{line.text}";
            DialogueLogManager.Instance?.AddEntry(displayText);
        }

        dialogueText.text = dialogueLines[dialogueLines.Length - 1].text;
        SetIconPreserveHeight(characterPortrait, dialogueLines[dialogueLines.Length - 1].portrait);

        currentLine = dialogueLines.Length;
        EndDialogue();
    }

    public void SkipTyping()
    {
        if (!isTyping || currentLine >= dialogueLines.Length) return;

        StopAllCoroutines();
        isTyping = false;

        DialogueLine line = dialogueLines[currentLine];
        string displayText = string.IsNullOrEmpty(line.speakerName) ? line.text : $"<b>[{line.speakerName}]</b>：{line.text}";
        dialogueText.text = displayText;
        DialogueLogManager.Instance?.AddEntry(displayText);

        canClickNext = true;
    }

    public void ProceedToNextLine()
    {
        if (isTyping) return;

        currentLine++;
        if (currentLine < dialogueLines.Length)
            StartCoroutine(TypeLine(dialogueLines[currentLine]));
        else
            EndDialogue();
    }
}
