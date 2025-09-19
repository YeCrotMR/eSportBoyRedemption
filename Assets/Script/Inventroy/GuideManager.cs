using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HintData
{
    public string id;         // 提示 ID（如 "move" / "open_inventory"）
    [TextArea] public string message; // 提示文字
}

public class GuideManager : MonoBehaviour
{
    public static GuideManager Instance;

    [Header("UI 引用")]
    public CanvasGroup hintCanvas;   // 用于淡入淡出的 CanvasGroup
    public Text hintText;        // 提示文字

    [Header("提示配置表")]
    public List<HintData> hints = new List<HintData>();

    [Header("参数设置")]
    public float fadeDuration = 0.5f; // 淡入淡出时间
    public float showDuration = 2f;   // 默认显示停留时间

    private Coroutine currentRoutine;
    private Dictionary<string, string> hintDictionary;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (hintCanvas != null)
            hintCanvas.alpha = 0f; // 初始隐藏

        // 初始化字典
        hintDictionary = new Dictionary<string, string>();
        foreach (var h in hints)
        {
            if (!hintDictionary.ContainsKey(h.id))
                hintDictionary.Add(h.id, h.message);
        }
    }

    /// <summary>
    /// 根据 ID 显示提示文字
    /// </summary>
    public void ShowHintByID(string id, float? customDuration = null)
    {
        if (hintDictionary.TryGetValue(id, out string message))
        {
            ShowHint(message, customDuration);
        }
        else
        {
            Debug.LogWarning($"[GuideManager] 未找到提示 ID: {id}");
        }
    }

    /// <summary>
    /// 直接显示指定文字
    /// </summary>
    public void ShowHint(string message, float? customDuration = null)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowHintRoutine(message, customDuration ?? showDuration));
    }

    private IEnumerator ShowHintRoutine(string message, float duration)
    {
        hintText.text = message;

        // 淡入
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            hintCanvas.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        hintCanvas.alpha = 1;

        // 停留
        yield return new WaitForSeconds(duration);

        // 淡出
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            hintCanvas.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        hintCanvas.alpha = 0;
    }
}
