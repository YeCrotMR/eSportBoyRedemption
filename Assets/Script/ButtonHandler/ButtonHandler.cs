using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonHandler : MonoBehaviour
{
    public Button myButton;
    public Text text;
    private Coroutine hintCoroutine;
    public float showDuration = 2f;  // 提示停留时间
    public float fadeDuration = 0.5f;// 淡入淡出时间
    public CanvasGroup hintCanvas;

    void Start()
    {
        // 给按钮绑定一个统一的监听函数
        myButton.onClick.AddListener(OnButtonClick);
    }


    void OnButtonClick()
    {
        // 根据条件执行不同函数
        if (TurnTV.tvon==true && PasswordSystem.correct == false)   // 举例：某个全局条件
        {
            PhoneUIManager.Instance?.OpenAppPage(1);
        }
        else if(TurnTV.tvon== false)
        {
            ShowHint("该设备未开机");
        }
        else if(PasswordSystem.correct == true){
            
        }
    }
    private void ShowHint(string message)
    {
        text.text = message;

        // 如果有上一个协程，先停掉
        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
        }
        hintCoroutine = StartCoroutine(HintRoutine());
    }

    public void cleartext()
    {
        text.text = "";
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
