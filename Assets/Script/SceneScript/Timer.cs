using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    [Header("计时设置")]
    public float duration = 10f;       // 倒计时总时长
    public bool isCountdown = true;    // true=倒计时, false=正向计时
    public bool autoStart = false;     // 是否自动开始

    public static bool hasStarted = false;

    [Header("计时状态 (只读)")]
    public float currentTime = 0f;     // 当前时间
    public bool isRunning = false;     // 是否正在计时

    [Header("计时结束事件")]
    public UnityEvent onTimerEnd;
    public UnityEvent onTimerEnd2;

    public static bool stolenkey = false;
    public static bool TimerFinished = false;
    public ItemPickup key;
    private GameObject Son;
    public static bool escPaused = false;

    // 单例防止重复
    public static Timer instance;

    void Awake()
    {
        if (instance == null)
    {
        instance = this;
        DontDestroyOnLoad(gameObject);  // 关键
    }
    else
    {
        Destroy(gameObject);
    }
    }

    void Start()
    {
        key = FindKey();
        FindPlayer();

        if (isCountdown)
            currentTime = duration;
        else
            currentTime = 0f;

        if (autoStart)
            StartTimer();

        // 监听场景切换，重新找 Player
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        Son = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        FindPlayer();
        key = FindKey();
        if(!isRunning || escPaused){
            return;
        }
        if (isRunning && !stolenkey && InventoryManager.Instance.HasItem("大门钥匙"))
        {
            
            stolenkey = true;
            Debug.Log("偷到了钥匙"+stolenkey);
        }
            //Debug.Log(currentTime);
        if (isCountdown)
            {
                currentTime -= Time.deltaTime;
                if (currentTime <= 0f)
                {
                    currentTime = 0f;
                    isRunning = false;
                    TVplaying.isplaying = false;
                    
                    TurnTV.tvon = false;
                    TimerFinished = true;
                }
            }
            else
            {
                currentTime += Time.deltaTime;
                if (currentTime >= duration)
                {
                    isRunning = false;
                    // if (Son != null && Son.transform.position.y <= -246.0019f)
                    // {
                    //     onTimerEnd?.Invoke();
                    // }
                    // else
                    // {
                    //     onTimerEnd2?.Invoke();
                    // }
                    TurnTV.tvon = false;
                    TimerFinished = true;
                }
            }


        if(NPCconditionChat.isChatting){
                PauseTimer();
            }
    }

    // 开始计时
    public void StartTimer()
    {   ResetTimer();
        Debug.Log("开始计时");
        isRunning = true;
        hasStarted = true;
    }

    // 暂停计时
    public void PauseTimer()
    {
        Debug.Log("暂停");
        isRunning = false;
    }

    // 重置计时
    public void ResetTimer()
    {
        Debug.Log("重置");
        isRunning = false;
        TimerFinished = false;
        hasStarted = false;
        currentTime = isCountdown ? duration : 0f;
    }

    ItemPickup FindKey()
    {
        ItemPickup[] allItems = FindObjectsOfType<ItemPickup>();
        foreach (var item in allItems)
        {
            if (item.name == "key")   // 注意大小写必须完全一致
            {
                return item;
            }
        }
        return null;
    }
}
