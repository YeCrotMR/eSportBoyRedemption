using UnityEngine;

public class GlobalTimer : MonoBehaviour
{
    private static GlobalTimer _instance;
    public static GlobalTimer Instance => _instance;

    // 对外公开的“实时计时时间”（秒）
    public static float ElapsedTime;

    // 是否正在计时
    public static bool IsRunning;

    // 是否使用不受 Time.timeScale 影响的真实时间
    public static bool UseUnscaledTime { get; set; } = false;

    private void Awake()
    {
        // 标准单例守卫：销毁重复实例
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (!IsRunning) return;
        ElapsedTime += UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        //Debug.Log(ElapsedTime);
        // Debug.Log(ElapsedTime); // 调试时再打开，平时建议关闭以免刷屏
    }

    // 手动开始计时；reset=true 表示从 0 重新开始
    public static void StartTimer(bool reset = true)
    {
        EnsureInstance();
        if (reset) ElapsedTime = 0f;
        IsRunning = true;
    }

    // 暂停计时
    public static void StopTimer()
    {
        if (_instance == null) return;
        IsRunning = false;
    }

    // 将计时清零（不自动开始）
    public static void ResetTimer()
    {
        ElapsedTime = 0f;
    }

    // 可选：直接设置计时值
    public static void SetTime(float value)
    {
        ElapsedTime = Mathf.Max(0f, value);
    }

    // 确保有一个常驻实例（优先复用场景中已有的）
    public static void EnsureInstance()
    {
        if (_instance != null) return;

        // 尝试复用场景中已存在的实例（即使它还没 Awake）
        var existing = FindObjectOfType<GlobalTimer>();
        if (existing != null)
        {
            _instance = existing;
            // 由 existing 的 Awake 负责 DontDestroyOnLoad
            return;
        }

        // 场景中没有则创建新的常驻对象
        var go = new GameObject("GlobalTimer");
        _instance = go.AddComponent<GlobalTimer>();
        DontDestroyOnLoad(go);
    }
}
