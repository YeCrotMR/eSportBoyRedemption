using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color pressedColor = Color.red;
    public float transitionDuration = 0.2f;

    private Graphic uiText;
    private Coroutine colorCoroutine;

    private void Awake()
    {
        uiText = GetComponent<Text>(); // UGUI Text
        if (uiText == null)
            uiText = GetComponent<TMP_Text>(); // TextMeshPro

        if (uiText != null)
            uiText.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ChangeColor(hoverColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ChangeColor(normalColor);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ChangeColor(pressedColor);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 如果仍在悬停，恢复 hoverColor，否则恢复 normalColor
        if (RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition))
            ChangeColor(hoverColor);
        else
            ChangeColor(normalColor);
    }

    void ChangeColor(Color targetColor)
    {
        if (colorCoroutine != null)
            StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(AnimateColor(targetColor));
    }

    IEnumerator AnimateColor(Color targetColor)
    {
        Color startColor = uiText.color;
        float time = 0f;

        while (time < transitionDuration)
        {
            uiText.color = Color.Lerp(startColor, targetColor, time / transitionDuration);
            time += Time.unscaledDeltaTime; // 用 unscaled 时间，防止 Time.timeScale = 0 时暂停
            yield return null;
        }

        uiText.color = targetColor;
    }
}
