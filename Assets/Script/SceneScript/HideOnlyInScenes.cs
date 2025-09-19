using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class HideOnlyInScenes : MonoBehaviour
{
    [Tooltip("这些场景中会隐藏该物体，其它场景中显示")]
    public string[] scenesToHideIn;

    private CanvasGroup cg;

    void Awake()
    {
        //DontDestroyOnLoad(gameObject);
        cg = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyVisibility(SceneManager.GetActiveScene().name);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyVisibility(scene.name);
    }

    private void ApplyVisibility(string currentScene)
    {
        bool shouldHide = false;

        foreach (var sceneName in scenesToHideIn)
        {
            if (currentScene == sceneName)
            {
                shouldHide = true;
                break;
            }
        }

        // 控制 CanvasGroup 透明与交互
        if (cg != null)
        {
            cg.alpha = shouldHide ? 0 : 1;
            cg.blocksRaycasts = !shouldHide;
            cg.interactable = !shouldHide;
        }
    }
}
