using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PasswordSystem : MonoBehaviour
{
    [Header("密码设置")]
    [Tooltip("密码的最大字符长度")]
    public int maxLength = 6;
    [Tooltip("正确的密码")]
    public string correctPassword = "1234";
    public static bool correct = false;

    [Header("UI 引用")]
    public TMP_Text passwordText;    // 显示输入的密码
    public Text hintText;            // 提示信息
    public CanvasGroup hintCanvas;   // 控制提示的淡入淡出

    [Header("提示设置")]
    public float showDuration = 2f;  // 提示停留时间
    public float fadeDuration = 0.5f;// 淡入淡出时间

    private string currentInput = "";
    private Coroutine hintCoroutine;

    /// <summary>
    /// 按钮调用此方法输入字符
    /// </summary>
    public void InputChar(string c)
    {
        if (currentInput.Length < maxLength)
        {
            currentInput += c;
            UpdatePasswordText();
        }
    }

    /// <summary>
    /// 删除一个字符
    /// </summary>
    public void DeleteChar()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdatePasswordText();
        }
    }

    /// <summary>
    /// 确认输入
    /// </summary>
    public void ConfirmInput()
    {
        bool result = CheckPassword();
        if (result)
        {
            correct = true;
            ShowHint("配对成功");
            PhoneUIManager.Instance.GoBack();
        }
        else
        {
            ShowHint("密码输入错误");
        }
    }

    /// <summary>
    /// 检查密码是否正确
    /// </summary>
    public bool CheckPassword()
    {
        return currentInput == correctPassword;
    }

    /// <summary>
    /// 更新显示的密码文本
    /// </summary>
    private void UpdatePasswordText()
    {
        passwordText.text = currentInput;
        // 如果想显示为 ****：
        // passwordText.text = new string('*', currentInput.Length);
    }

    /// <summary>
    /// 清空输入
    /// </summary>
    public void ClearInput()
    {
        currentInput = "";
        UpdatePasswordText();
        hintText.text = "";
        hintCanvas.alpha = 0;
    }

    /// <summary>
    /// 显示提示并自动淡出
    /// </summary>
    private void ShowHint(string message)
    {
        hintText.text = message;

        // 如果有上一个协程，先停掉
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }
        hintCoroutine = StartCoroutine(HintRoutine());
    }

    /// <summary>
    /// 提示文字的动画协程
    /// </summary>
    private IEnumerator HintRoutine()
    {
        // 淡入
        yield return StartCoroutine(FadeCanvasGroup(hintCanvas, 0f, 1f, fadeDuration));

        // 停留
        yield return new WaitForSeconds(showDuration);

        // 淡出
        yield return StartCoroutine(FadeCanvasGroup(hintCanvas, 1f, 0f, fadeDuration));
    }

    /// <summary>
    /// 淡入淡出动画
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        float elapsed = 0f;
        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
