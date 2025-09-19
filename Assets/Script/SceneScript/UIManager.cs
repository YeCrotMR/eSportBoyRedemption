using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("基础 UI 面板")]
    public GameObject homepage;
    public GameObject uiBackgroundObject;

    [Header("手动添加控制的 UI 面板（包括主页、设置、存档、读取等）")]
    public List<GameObject> managedPanels = new List<GameObject>();

    [Header("动画参数")]
    public float fadeDuration = 0.3f;

    private CanvasGroup uiBackground;
    private Stack<CanvasGroup> uiStack = new Stack<CanvasGroup>();

    public static bool isUIMode = false; // 全局静态，供其他脚本访问
    private bool isTransitioning = false;

    void Start()
    {
        InitAllPanels();

        if (homepage != null)
        {
            CanvasGroup homepageCG = GetCanvasGroup(homepage);
            SetPanelVisible(homepageCG, false);
        }

        if (uiBackgroundObject != null)
        {
            uiBackground = GetCanvasGroup(uiBackgroundObject);
            SetPanelVisible(uiBackground, false);
        }

        // 开始时不显示任何 UI，等待 ESC 弹出 homepage
    }


    void Update()
{
    if (isTransitioning) return;

    if (Input.GetKeyDown(KeyCode.Escape)
        && SceneManager.GetActiveScene().name != "start"
        && SceneManager.GetActiveScene().name != "gameover")
    {
        if (uiStack.Count == 0)
        {
            PushUI(homepage);
        }
        else
        {
            PopUI();
        }
    }

    if (isUIMode)
    {
        ApplyMuteSetting(true); // 打开 UI 时静音
        Timer.escPaused = true;
    }else{
        ApplyMuteSetting(false);
        Timer.escPaused = false;
    }
}

    void InitAllPanels()
    {
        foreach (var panel in managedPanels)
        {
            if (panel == null) continue;
            CanvasGroup cg = GetCanvasGroup(panel);
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

        if (uiStack.Count == 0 && uiBackground != null)
        {
            StartCoroutine(FadeIn(uiBackground));
        }

        if (uiStack.Count > 0)
        {
            CanvasGroup top = uiStack.Peek();
            StartCoroutine(FadeOut(top));
        }

        CanvasGroup next = GetCanvasGroup(panel);
        panel.SetActive(true);
        StartCoroutine(FadeIn(next));
        uiStack.Push(next);

        isUIMode = true; // 打开 UI 时禁用 WASD

        if (panel == homepage && Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
        }

        

    }


    public void PopUI()
    {
        if (uiStack.Count > 0)
        {
            CanvasGroup top = uiStack.Pop();
            StartCoroutine(FadeOut(top));
        }

        if (uiStack.Count > 0)
        {
            CanvasGroup previous = uiStack.Peek();
            StartCoroutine(FadeIn(previous));
        }
        else
{
    if (uiBackground != null)
    {
        StartCoroutine(FadeOut(uiBackground));
    }

    Time.timeScale = 1f;
    isUIMode = false;
}


        
    }

        public void CloseAllUI()
    {
        StopAllCoroutines();

        while (uiStack.Count > 0)
        {
            CanvasGroup cg = uiStack.Pop();
            SetPanelVisible(cg, false);
        }

        if (uiBackground != null)
        {
            SetPanelVisible(uiBackground, false);
        }

        Time.timeScale = 1f;
        isUIMode = false;
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

public void ApplyMuteSetting(bool isMuted)
{
    AudioListener.volume = isMuted ? 0f : 1f;

    foreach (AudioSource source in FindObjectsOfType<AudioSource>())
    {
        source.mute = isMuted;
        //Debug.Log("声音状态：" + (isMuted ? "静音" : "开启"));
    }
}

}