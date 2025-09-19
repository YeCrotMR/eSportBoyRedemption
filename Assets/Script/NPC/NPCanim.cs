using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCanim : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator anim;
    public bool canMove = true;
    private AudioSource walkSoundEffect;

    private Vector2 lastMoveDir = Vector2.down;
    private Vector2 lastPosition;

    public  enum MovementState { idle, walkNorth, walkEast, walkSouth, walkWest, rlidle, upidle }

    private const float velocityThreshold = 0.05f;

    [Header("初始动画设置")]
    public MovementState initialState = MovementState.idle; // 手动设置初始动画
    private bool hasStartedMoving = false; // 标记是否已经开始移动

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // 开启插值

        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        walkSoundEffect = GetComponent<AudioSource>();

        lastPosition = rb.position;

        // 设置初始动画
        SetInitialAnimationState(initialState);
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            anim.SetInteger("state", 0);
            walkSoundEffect.Stop();
            return;
        }

        // 物理帧计算速度
        Vector2 currentPosition = rb.position;
        Vector2 velocity = (currentPosition - lastPosition) / Time.fixedDeltaTime;
        lastPosition = currentPosition;

        if (!hasStartedMoving && velocity.magnitude <= velocityThreshold)
        {
            // NPC 还没开始动，保持初始动画
            return;
        }

        if (velocity.magnitude > velocityThreshold)
        {
            hasStartedMoving = true; // 一旦检测到移动，就切换到正常逻辑
            if (!walkSoundEffect.isPlaying)
                walkSoundEffect.Play();

            lastMoveDir = velocity.normalized;
        }
        else
        {
            walkSoundEffect.Stop();
        }

        UpdateAnimationState(velocity);
    }

    private void SetInitialAnimationState(MovementState state)
    {
        anim.SetInteger("state", (int)state);

        switch (state)
        {
            case MovementState.walkEast:
                sprite.flipX = false;
                break;
            case MovementState.walkWest:
                sprite.flipX = false;
                break;
            case MovementState.rlidle:
                // 根据需求 flipX 自己调
                sprite.flipX = false;
                break;
            default:
                sprite.flipX = false;
                break;
        }
    }

    private void UpdateAnimationState(Vector2 velocity)
    {
        MovementState state = MovementState.idle;

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
                    state = MovementState.walkNorth;
                    sprite.flipX = false;
                }
                else
                {
                    state = MovementState.walkSouth;
                    sprite.flipX = false;
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
                state = MovementState.upidle;
                sprite.flipX = false;
            }
            else
            {
                state = MovementState.idle;
                sprite.flipX = false;
            }
        }

        anim.SetInteger("state", (int)state);
    }
}
