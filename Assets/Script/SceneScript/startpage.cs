using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class startpage : MonoBehaviour
{
    [Header("基础 UI 面板")]
    public GameObject homepage;
    public GameObject uiBackgroundObject;

    [Header("动画参数")]
    public float fadeDuration = 0.3f;

    private CanvasGroup uiBackground;
    private Stack<CanvasGroup> uiStack = new Stack<CanvasGroup>();
    private bool isHomeInitialized = false;
    private bool isTransitioning = false;

    void Start()
    {
        InitAllPanels();

        if (uiBackgroundObject != null)
        {
            uiBackground = GetCanvasGroup(uiBackgroundObject);
            SetPanelVisible(uiBackground, false);
        }

        ShowHomepageOnStart();
    }

    void Update()
{
    if (isTransitioning) return; // 锁动画期间输入

    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (uiStack.Count > 1)
        {
            PopUI();
        }
    }
}


    void ShowHomepageOnStart()
    {
        if (homepage != null && !isHomeInitialized)
        {
            CanvasGroup homeCG = GetCanvasGroup(homepage);
            homepage.SetActive(true);
            uiStack.Push(homeCG);
            StartCoroutine(FadeIn(homeCG));

            // 显式隐藏背景
            if (uiBackground != null)
                StartCoroutine(FadeOut(uiBackground));

            isHomeInitialized = true;
        }
    }

    void InitAllPanels()
    {
        CanvasGroup[] allCanvasGroups = GetComponentsInChildren<CanvasGroup>(true);
        foreach (var cg in allCanvasGroups)
        {
            SetPanelVisible(cg, false);
        }
    }

    CanvasGroup GetCanvasGroup(GameObject obj)
    {
        var cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();
        return cg;
    }

    void SetPanelVisible(CanvasGroup cg, bool visible)
    {
        cg.alpha = visible ? 1 : 0;
        cg.interactable = visible;
        cg.blocksRaycasts = visible;
        cg.gameObject.SetActive(visible);
    }

    public void PushUI(GameObject panel)
{
    if (panel == null) return;

    CanvasGroup top = uiStack.Count > 0 ? uiStack.Peek() : null;

    // 顶部面板淡出
    if (top != null)
    {
        StartCoroutine(FadeOut(top));
    }

    CanvasGroup next = GetCanvasGroup(panel);
    panel.SetActive(true);
    StartCoroutine(FadeIn(next));
    uiStack.Push(next);

    // 背景控制逻辑
    if (uiBackground != null)
    {
        bool isFromHome = top != null && top.gameObject == homepage;
        bool isToHome = panel == homepage;

        if (isFromHome && !isToHome) // 从主页进入其他页面
        {
            StartCoroutine(FadeIn(uiBackground));
        }
        else if (!isFromHome && !isToHome)
        {
            // 其他情况不做动画，只保证背景是激活的
            SetPanelVisible(uiBackground, true);
        }
    }
}

    public void PopUI()
    {
        if (uiStack.Count > 1)
        {
            CanvasGroup top = uiStack.Pop();
            StartCoroutine(FadeOut(top));

            CanvasGroup previous = uiStack.Peek();
            StartCoroutine(FadeIn(previous));

            // 背景控制逻辑
            if (uiBackground != null)
            {
                bool isToHome = previous.gameObject == homepage;
                bool isFromHome = top.gameObject == homepage;

                if (isToHome && !isFromHome) // 从其他页面回主页
                {
                    StartCoroutine(FadeOut(uiBackground));
                }
                else if (!isToHome && !isFromHome)
                {
                    // 其他情况不做动画，只保证背景是激活的
                    SetPanelVisible(uiBackground, true);
                }
            }
        }
    }


    IEnumerator FadeIn(CanvasGroup cg)
{
    isTransitioning = true;

    float t = 0f;
    cg.gameObject.SetActive(true);
    cg.interactable = true;
    cg.blocksRaycasts = true;

    while (t < fadeDuration)
    {
        t += Time.unscaledDeltaTime;
        cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
        yield return null;
    }

    cg.alpha = 1f;
    isTransitioning = false;
}

IEnumerator FadeOut(CanvasGroup cg)
{
    isTransitioning = true;

    float t = 0f;
    cg.interactable = false;
    cg.blocksRaycasts = false;

    while (t < fadeDuration)
    {
        t += Time.unscaledDeltaTime;
        cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
        yield return null;
    }

    cg.alpha = 0f;
    cg.gameObject.SetActive(false);
    isTransitioning = false;
}

}
