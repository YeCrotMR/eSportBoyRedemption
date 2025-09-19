using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutline : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propBlock;

    [Header("描边设置")]
    public Color outlineColor = Color.yellow;
    [Range(0f, 100f)] public float outlineThickness = 5f; // 目标厚度
    public float transitionSpeed = 10f; // 过渡速度

    private float currentThickness = 0f;
    private float targetThickness = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propBlock = new MaterialPropertyBlock();
        ApplyOutline();
    }

    void Update()
    {
        // 平滑过渡 thickness
        if (Mathf.Abs(currentThickness - targetThickness) > 0.01f)
        {
            currentThickness = Mathf.Lerp(currentThickness, targetThickness, Time.deltaTime * transitionSpeed);
            ApplyOutline();
        }
    }

    private void ApplyOutline()
    {
        spriteRenderer.GetPropertyBlock(propBlock);

        if (currentThickness > 0.01f)
        {
            propBlock.SetColor("_OutlineColor", outlineColor);
            propBlock.SetFloat("_Thickness", currentThickness);
        }
        else
        {
            propBlock.SetFloat("_Thickness", 0f);
        }

        spriteRenderer.SetPropertyBlock(propBlock);
    }

    /// <summary>启用描边（渐变到目标厚度）</summary>
    public void EnableOutline()
    {
        targetThickness = outlineThickness;
    }

    /// <summary>禁用描边（渐变到 0）</summary>
    public void DisableOutline()
    {
        targetThickness = 0f;
    }
}
