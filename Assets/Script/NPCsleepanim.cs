using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NPCsleepanim : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    public bool canMove = true;
    private AudioSource walkSoundEffect;
    //public PlayerMovementMonitor playerMovementMonitor;

    private Vector2 lastMoveDir = Vector2.down;
    private Vector2 lastPosition;

    private bool justWokeUp = false;
    private bool lastAwakeState = false;
    private float wakeIdleTimer = 0f;
    private const float wakeIdleDuration = 0.5f; // 醒来时idle持续时长（秒）

    private const float velocityThreshold = 0.05f;
    private bool hasbacksleep = false;
    public NPCcontinueMover backtosleep;
    public UnityEvent onTimerEnd;
    private enum MovementState
    {
        idle,        // 0
        walkNorth,   // 1
        walkEast,    // 2
        walkSouth,   // 3
        walkWest,    // 4
        rlidle,      // 5
        upidle,      // 6
        sleep        // 7
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        walkSoundEffect = GetComponent<AudioSource>();
        lastPosition = transform.position;
    }

    private void Update()
    {
        bool currentAwake = PlayerMovementMonitor.awake;

        // 检测是否刚醒来
        if (currentAwake && !lastAwakeState)
        {
            justWokeUp = true;
            wakeIdleTimer = wakeIdleDuration;
            anim.SetInteger("state", (int)MovementState.idle); // 播放idle
        }

        Debug.Log("困醒:"+(NPCcontinueMover.doisMoving));

        lastAwakeState = currentAwake;

        if (!canMove)
        {
            walkSoundEffect.Stop();
            return;
        }

        if(doorTeleport.previousSceneName == "living room" && PlayerMovementMonitor.awake == true && gameover2.momisrunning == true){
            gameObject.SetActive(false);
        }

        Vector2 currentPosition = transform.position;
        Vector2 velocity = (currentPosition - lastPosition) / Time.deltaTime;
        lastPosition = currentPosition;

        // 醒来时延迟播放正常动画
        if (justWokeUp)
        {
            wakeIdleTimer -= Time.deltaTime;
            if (wakeIdleTimer > 0f)
            {
                walkSoundEffect.Stop(); // 保证醒来不走动
                return;
            }
            else
            {
                justWokeUp = false;
            }
        }

        UpdateAnimationState(velocity);
        if (velocity.magnitude > velocityThreshold)
        {
            if (!walkSoundEffect.isPlaying)
            {
                walkSoundEffect.Play();
            }

            lastMoveDir = velocity.normalized;
        }
        else
        {
            walkSoundEffect.Stop();
        }

        
    }

    private void UpdateAnimationState(Vector2 velocity)
    {
        MovementState state;

        if (!PlayerMovementMonitor.awake && !TVplaying.isplaying && !NPCcontinueMover.doisMoving)
        {
            Debug.Log("驾崩");
            state = MovementState.sleep;
            anim.SetInteger("state", (int)state);
            return;
        }

        if(backtosleep.hasFinishedMoving && backtosleep != null && !hasbacksleep){
                onTimerEnd?.Invoke();
                hasbacksleep = true;
        }

        if (velocity.magnitude > velocityThreshold)
        {
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            {
                if (velocity.x > 0)
                {
                    state = MovementState.walkEast;
                    sprite.flipX = false;
                }
                else
                {
                    state = MovementState.walkWest;
                    sprite.flipX = false;
                }
            }
            else
            {
                if (velocity.y > 0)
                {
                    sprite.flipX = false;
                    state = MovementState.walkNorth;
                }
                else
                {
                    sprite.flipX = false;
                    state = MovementState.walkSouth;
                }
            }
        }
        else
        {
            if (lastMoveDir.x > 0f)
            {
                state = MovementState.rlidle;
                sprite.flipX = true;
            }
            else if (lastMoveDir.x < 0f)
            {
                state = MovementState.rlidle;
                sprite.flipX = false;
            }
            else if (lastMoveDir.y > 0f)
            {
                sprite.flipX = false;
                state = MovementState.upidle;
            }
            else
            {
                sprite.flipX = false;
                state = MovementState.idle;
            }
        }

        anim.SetInteger("state", (int)state);
    }
}
