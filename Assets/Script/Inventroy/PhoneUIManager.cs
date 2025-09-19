using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhoneUIManager : MonoBehaviour
{
    public static PhoneUIManager Instance;

    [Header("UI 引用")]
    public GameObject phoneRoot;       // 整个手机 UI 根节点
    public CanvasGroup phoneCanvas;    // 手机整体 CanvasGroup（如果为空会自动加上）
    public GameObject homePage;        // 桌面页面
    public GameObject[] appPages;      // 各个 App 页面

    [Header("动画设置")]
    public float fadeDuration = 0.3f;     // 页面交叉淡化时长
    public float phoneOpenDuration = 0.35f; // 手机开关动画时长
    [Range(0.6f, 1f)]
    public float phoneOpenScale = 0.9f;   // 打开时从该缩放到 1

    [Header("手机判定")]
    public string requiredPhoneName = "iphone16 pro max";

    private bool isVisible = false;
    private Stack<GameObject> pageHistory = new Stack<GameObject>();
    private Coroutine crossFadeRoutine;
    private Coroutine fadeOutRoutine;
    private Coroutine phoneAnimRoutine;
    private GameObject currentPage;

    void Awake()
    {
        if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);  // 关键
    }
    else
    {
        Destroy(gameObject);
    }

        if (phoneRoot == null)
        {
            Debug.LogError("PhoneUIManager: phoneRoot 未设置！");
            enabled = false;
            return;
        }

        // 确保整体 CanvasGroup 存在
        if (phoneCanvas == null)
        {
            phoneCanvas = phoneRoot.GetComponent<CanvasGroup>();
            if (phoneCanvas == null)
                phoneCanvas = phoneRoot.AddComponent<CanvasGroup>();
        }

        // 初始隐藏
        phoneRoot.SetActive(false);
        phoneCanvas.alpha = 0f;
        phoneCanvas.blocksRaycasts = false;

        // 所有页面加 CanvasGroup
        AddCanvasGroup(homePage);
        if (appPages != null)
        {
            foreach (var page in appPages)
                AddCanvasGroup(page);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            var inv = InventoryManager.Instance;

            if (!isVisible)
            {
                // 打开手机必须选中正确物品
                if (inv != null && inv.selectedIndex >= 0 && inv.selectedIndex < inv.inventory.Count)
                {
                    Item selectedItem = inv.inventory[inv.selectedIndex];
                    if (selectedItem != null && selectedItem.itemName == requiredPhoneName)
                        ShowPhone();
                }
            }
            else
            {
                // 随时关闭
                HidePhone();
            }
        }
    }

    // 确保页面有 CanvasGroup
    void AddCanvasGroup(GameObject go)
    {
        if (go == null) return;
        CanvasGroup cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        go.SetActive(false);
    }

    // ---------------- 手机动画 ----------------
    public void ShowPhone()
    {
        if (phoneRoot == null) return;

        if (phoneAnimRoutine != null)
            StopCoroutine(phoneAnimRoutine);

        isVisible = true;
        phoneAnimRoutine = StartCoroutine(AnimatePhoneIn());

        // 如果有记录，恢复上次页面，否则回主页
        if (currentPage != null)
        {
            SetPageImmediate(currentPage);
        }
        else
        {
            ShowHome();
        }
    }

    public void HidePhone()
    {
        if (phoneRoot == null) return;

        if (crossFadeRoutine != null)
        {
            StopCoroutine(crossFadeRoutine);
            crossFadeRoutine = null;
        }

        if (phoneAnimRoutine != null)
        {
            StopCoroutine(phoneAnimRoutine);
            phoneAnimRoutine = null;
        }

        phoneAnimRoutine = StartCoroutine(AnimatePhoneOut());
    }

    IEnumerator AnimatePhoneIn()
    {
        phoneRoot.SetActive(true);
        phoneCanvas.blocksRaycasts = false;
        phoneCanvas.alpha = 0f;

        Vector3 startScale = Vector3.one * phoneOpenScale;
        Vector3 endScale = Vector3.one;
        phoneRoot.transform.localScale = startScale;

        float elapsed = 0f;
        while (elapsed < phoneOpenDuration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / phoneOpenDuration);
            phoneCanvas.alpha = Mathf.Lerp(0f, 1f, k);
            phoneRoot.transform.localScale = Vector3.Lerp(startScale, endScale, k);
            yield return null;
        }

        phoneCanvas.alpha = 1f;
        phoneRoot.transform.localScale = endScale;
        phoneCanvas.blocksRaycasts = true;
        phoneAnimRoutine = null;
    }

    IEnumerator AnimatePhoneOut()
    {
        phoneCanvas.blocksRaycasts = false;

        Vector3 startScale = phoneRoot.transform.localScale;
        Vector3 endScale = Vector3.one * phoneOpenScale;

        float elapsed = 0f;
        while (elapsed < phoneOpenDuration)
        {
            elapsed += Time.deltaTime;
            float k = Mathf.Clamp01(elapsed / phoneOpenDuration);
            phoneCanvas.alpha = Mathf.Lerp(1f, 0f, k);
            phoneRoot.transform.localScale = Vector3.Lerp(startScale, endScale, k);
            yield return null;
        }

        phoneCanvas.alpha = 0f;
        phoneRoot.SetActive(false);

        isVisible = false;
        phoneAnimRoutine = null;
    }

    // ---------------- 页面切换 ----------------
    public void ShowHome()
    {
        CrossFade(currentPage, homePage);
        pageHistory.Clear();
        pageHistory.Push(homePage);
    }

    public void OpenAppPage(int index)
{
    if (appPages == null || index < 0 || index >= appPages.Length) return;

    GameObject cur = pageHistory.Count > 0 ? pageHistory.Peek() : null;
    GameObject next = appPages[index];
    if (next == null) return;

    // 如果已经在当前页面，则不重复打开/不重复 push
    if (cur == next)
        return;

    CrossFade(cur, next);

    // 只有当栈顶不是 next 时才 push
    if (pageHistory.Count == 0 || pageHistory.Peek() != next)
        pageHistory.Push(next);
}

    public void GoBack()
{
    if (pageHistory.Count <= 1) return;

    GameObject current = pageHistory.Pop();
    GameObject previous = pageHistory.Peek();

    // 停掉正在运行的动画，避免冲突
    if (crossFadeRoutine != null)
    {
        StopCoroutine(crossFadeRoutine);
        crossFadeRoutine = null;
    }
    if (fadeOutRoutine != null)
    {
        StopCoroutine(fadeOutRoutine);
        fadeOutRoutine = null;
    }

    // 🔹 直接使用交叉淡化
    CrossFade(current, previous);

    currentPage = previous;
}



    void CrossFade(GameObject fromPage, GameObject toPage)
{
    if (toPage == null) return;
    if (fromPage == toPage) return;

    // 停掉可能存在的交叉淡入协程
    if (crossFadeRoutine != null)
    {
        StopCoroutine(crossFadeRoutine);
        crossFadeRoutine = null;
    }

    // 停掉可能存在的单页淡出（避免冲突）
    if (fadeOutRoutine != null)
    {
        StopCoroutine(fadeOutRoutine);
        fadeOutRoutine = null;
    }

    crossFadeRoutine = StartCoroutine(CrossFadeRoutine(fromPage, toPage));
    currentPage = toPage;
}

    IEnumerator CrossFadeRoutine(GameObject fromPage, GameObject toPage)
{
    if (toPage == null) yield break;

    CanvasGroup toCg = toPage.GetComponent<CanvasGroup>();
    CanvasGroup fromCg = fromPage != null ? fromPage.GetComponent<CanvasGroup>() : null;

    // 激活并置顶新页面，确保在最前面
    toPage.SetActive(true);
    toPage.transform.SetAsLastSibling();
    if (toCg != null) toCg.alpha = 0f;

    // 确保 fromPage 起始 alpha 为 1（如果存在）
    if (fromPage != null && fromCg != null)
        fromCg.alpha = 1f;

    float t = 0f;
    while (t < fadeDuration)
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / fadeDuration);

        if (toCg != null) toCg.alpha = Mathf.Lerp(0f, 1f, k);
        if (fromCg != null) fromCg.alpha = Mathf.Lerp(1f, 0f, k);

        yield return null;
    }

    if (toCg != null) toCg.alpha = 1f;

    // 关闭旧页面（如果有）
    if (fromPage != null)
    {
        if (fromCg != null) fromCg.alpha = 0f;
        fromPage.SetActive(false);
    }

    crossFadeRoutine = null;
}

    // 🔹 用于手机打开时直接恢复页面，避免闪烁
    void SetPageImmediate(GameObject page)
    {
        if (page == null) return;

        foreach (var p in appPages)
        {
            if (p == null) continue;
            var cg = p.GetComponent<CanvasGroup>();
            if (p == page)
            {
                p.SetActive(true);
                if (cg != null) cg.alpha = 1f;
            }
            else
            {
                p.SetActive(false);
                if (cg != null) cg.alpha = 0f;
            }
        }

        if (homePage != null)
        {
            var cg = homePage.GetComponent<CanvasGroup>();
            if (page == homePage)
            {
                homePage.SetActive(true);
                if (cg != null) cg.alpha = 1f;
            }
            else
            {
                homePage.SetActive(false);
                if (cg != null) cg.alpha = 0f;
            }
        }

        currentPage = page;
    }
    IEnumerator FadeOutAndDeactivate(GameObject page)
{
    if (page == null) yield break;

    CanvasGroup cg = page.GetComponent<CanvasGroup>();
    if (cg == null)
    {
        page.SetActive(false);
        fadeOutRoutine = null;
        yield break;
    }

    float t = 0f;
    while (t < fadeDuration)
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / fadeDuration);

        cg.alpha = Mathf.Lerp(1f, 0f, k);

        yield return null;
    }

    cg.alpha = 0f;
    page.SetActive(false);
    fadeOutRoutine = null;
}

/// <summary>
/// 还原手机页面栈，只保留主页并关闭手机
/// </summary>
public void ResetPhoneStack(bool closePhone = true)
{
    // 停掉正在进行的淡化
    if (crossFadeRoutine != null)
    {
        StopCoroutine(crossFadeRoutine);
        crossFadeRoutine = null;
    }

    // 关闭所有 App 页面
    if (homePage != null)
    {
        foreach (var p in appPages)
        {
            if (p == null) continue;
            var cg = p.GetComponent<CanvasGroup>();
            p.SetActive(false);
            if (cg != null) cg.alpha = 0f;
        }

        // 恢复主页可见
        var homeCg = homePage.GetComponent<CanvasGroup>();
        homePage.SetActive(true);
        if (homeCg != null) homeCg.alpha = 1f;

        // 还原栈
        pageHistory.Clear();
        pageHistory.Push(homePage);

        currentPage = homePage;
    }

    // 🔹 关闭手机（可选）
    if (closePhone)
    {
        HidePhone();
    }
}

}
