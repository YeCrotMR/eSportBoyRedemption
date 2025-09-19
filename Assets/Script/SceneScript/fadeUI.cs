using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class fadeUI : MonoBehaviour
{
    public float duration = 1f; // 淡入/淡出时间
    public bool fadeInOnStart = true; // 是否开始就淡入

    private SpriteRenderer spriteRenderer;
    private Graphic uiGraphic; // Image 或 Text

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiGraphic = GetComponent<Graphic>();

        if (fadeInOnStart)
        {
            SetAlpha(0f);
            StartCoroutine(FadeTo(1f, duration));
        }
    }

    public void SetAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
        else if (uiGraphic != null)
        {
            Color c = uiGraphic.color;
            c.a = alpha;
            uiGraphic.color = c;
        }
    }

    public IEnumerator FadeTo(float targetAlpha, float time)
    {
        float startAlpha = (spriteRenderer != null) ? spriteRenderer.color.a : uiGraphic.color.a;
        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / time);
            SetAlpha(newAlpha);
            yield return null;
        }

        SetAlpha(targetAlpha); // 保证最终值精确
    }

    // 外部调用淡入淡出
    public void FadeIn() => StartCoroutine(FadeTo(1f, duration));
    public void FadeOut() => StartCoroutine(FadeTo(0f, duration));
}
