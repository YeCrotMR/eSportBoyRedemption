using UnityEngine;
using System.Collections;

public class esc : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private CanvasGroup canvasGroup;

    private bool isPaused = false;
    public static esc Instance;

    public float fadeDuration = 0.5f; // 透明度渐变时间（秒）

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        canvasGroup = pauseMenuUI.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = pauseMenuUI.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeDuration));
        Time.timeScale = 0f;
        isPaused = true;
    }

    void ResumeGame()
    {
        StartCoroutine(FadeOutAndDisable());
        Time.timeScale = 1f;
        isPaused = false;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        cg.interactable = to > 0.5f;
        cg.blocksRaycasts = to > 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // 用 unscaledDeltaTime 以便暂停时仍能过渡
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }

    IEnumerator FadeOutAndDisable()
    {
        yield return FadeCanvasGroup(canvasGroup, 1f, 0f, fadeDuration);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        pauseMenuUI.SetActive(false);
    }
}
