using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class TextPopup3 : MonoBehaviour
{
    // [SerializeField] private PlayerMovementMonitor playerMovementMonitor;

    public float appearDuration = 0.5f;    
    public float stayDuration = 2f;        

    private Vector3 originalScale;
    private CanvasGroup canvasGroup;

    public static bool hasShown = false;
    private Coroutine currentCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        canvasGroup = GetComponent<CanvasGroup>();

        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        if (!hasShown && InventoryManager.Instance.HasItem("iphone16 pro max"))
        {
            ShowText();
            hasShown = true;
        }
    }

    public void ShowText()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        currentCoroutine = StartCoroutine(ShowTextRoutine());
    }

    private IEnumerator ShowTextRoutine()
    {
        float time = 0f;
        while (time < appearDuration)
        {
            float t = time / appearDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(stayDuration);

        time = 0f;
        while (time < appearDuration)
        {
            float t = time / appearDuration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;

        currentCoroutine = null;
    }
}
