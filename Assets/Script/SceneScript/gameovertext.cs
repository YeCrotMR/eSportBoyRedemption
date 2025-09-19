using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameovertext : MonoBehaviour
{
    public Text myText;
    public float delay = 1f;         // 等待时间
    public float fadeDuration = 1f;  // 淡入持续时间

    void Start()
    {
        if (myText != null)
        {
            // 初始透明
            Color c = myText.color;
            c.a = 0f;
            myText.color = c;

            StartCoroutine(FadeInAfterDelay());
        }
    }

    IEnumerator FadeInAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        Color c = myText.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = alpha;
            myText.color = c;
            yield return null;
        }

        // 最终确保完全不透明
        c.a = 1f;
        myText.color = c;
    }
}
