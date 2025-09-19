using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AutoImageScaler : MonoBehaviour
{
    public float maxWidth = 300f;   // 最大宽度
    public float maxHeight = 400f;  // 最大高度

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
    }

    public void ApplyScale()
    {
        if (img.sprite == null) return;

        float spriteWidth = img.sprite.rect.width;
        float spriteHeight = img.sprite.rect.height;

        float widthRatio = maxWidth / spriteWidth;
        float heightRatio = maxHeight / spriteHeight;

        float scaleRatio = Mathf.Min(widthRatio, heightRatio);

        float finalWidth = spriteWidth * scaleRatio;
        float finalHeight = spriteHeight * scaleRatio;

        img.rectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
    }
}
