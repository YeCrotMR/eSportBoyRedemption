using UnityEngine;

public class PlayerMovementMonitor : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public float triggerTime = 1f;
    public float decayRate = 1f;

    private float moveTimer = 0f;
    private Rigidbody2D rb;
    private Animator anim;

    public Timer timer;

    public AudioSource audioSourceA; // A音效 AudioSource
    public AudioSource audioSourceB; // B音效 AudioSource（一次性）

    public static bool awake = false;
    private bool bSoundPlayed = false;

    void Start()
    {
        timer = FindObjectOfType<Timer>();
        rb = playerMovement.GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // 确保A音效开始播放
        if (audioSourceA != null && !audioSourceA.isPlaying)
        {
            audioSourceA.loop = true;
            audioSourceA.Play();
        }
    }

    void LateUpdate()
    {
        timer = FindObjectOfType<Timer>();
        Debug.Log("awake:"+awake);
        Debug.Log("isRunning:"+timer?.isRunning);
        Debug.Log("Timer.hasStarted:"+Timer.hasStarted);
        Debug.Log("Timer.TimerFinished:"+Timer.TimerFinished);
        Debug.Log("TVplaying:"+TVplaying.isplaying);
        

        if(EquipmentManager.IsEquipped("臭袜子")){
            triggerTime = 1f;
        }else{
            triggerTime =.1f;
        }
        if(timer?.isRunning == false && playerMovement.transform.position.y > -246f){
            if (playerMovement != null && rb.velocity.magnitude > 0.1f)
            {
                moveTimer += Time.deltaTime;
            }
            else
            {
                moveTimer -= Time.deltaTime * decayRate;
            }
        
            moveTimer = Mathf.Clamp(moveTimer, 0f, triggerTime);
        }
        //Debug.Log($"[Monitor] Rigidbody 当前速度: {rb.velocity}");
        //Debug.Log($"[Monitor] 移动计时器： {moveTimer:F2}s");
        //|| TVplaying.isplaying
        

        if ( (!awake && moveTimer >= triggerTime) || (Timer.stolenkey == false && InventoryManager.Instance.HasItem("大门钥匙")) )
        {
            Debug.Log("Timer:" + (InventoryManager.Instance.HasItem("大门钥匙") && Timer.stolenkey == false));
            Debug.Log("stolenkey:" + Timer.stolenkey);
            awake = true;

            // 停止 A 音效
            
        }

        if(awake){
            if (audioSourceA != null && audioSourceA.isPlaying)
            {
                audioSourceA.Stop();
            }

            // 播放一次 B 音效
            if (audioSourceB != null && !bSoundPlayed)
            {
                audioSourceB.Play();
                bSoundPlayed = true;
            }
        }

        if(TVplaying.isplaying){
            if (audioSourceA != null && audioSourceA.isPlaying)
            {
                audioSourceA.Stop();
            }

            // 播放一次 B 音效
            if (audioSourceB != null && !bSoundPlayed)
            {
                audioSourceB.Play();
                bSoundPlayed = true;
            }
        }
    }
    
    public void sleep(){
        //awake = false;
        bSoundPlayed = false;
        TextPopup.hasShown = false;
        Debug.Log("睡了");
    }
}
