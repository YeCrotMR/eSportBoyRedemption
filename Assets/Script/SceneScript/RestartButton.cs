using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInButton : MonoBehaviour
{
    public float delay = 1f;         // 延迟时间
    public float fadeDuration = 1f;  // 淡入时间

    private Image buttonImage;
    private Text buttonText;

    void Start()
    {
        // 获取按钮的背景和文字组件
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<Text>();

        // 初始设置为透明
        SetAlpha(0f);

        // 开始协程淡入
        StartCoroutine(FadeInAfterDelay());
    }

    void SetAlpha(float alpha)
    {
        if (buttonImage != null)
        {
            Color c = buttonImage.color;
            c.a = alpha;
            buttonImage.color = c;
        }

        if (buttonText != null)
        {
            Color c = buttonText.color;
            c.a = alpha;
            buttonText.color = c;
        }
    }

    IEnumerator FadeInAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(1f); // 最终设为完全不透明
    }
}
