using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
   // private CapsuleCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;
    private AudioSource walkSoundEffect;

    public bool canMove = true;

    private float dirX = 0f;
    private float dirY = 0f;
    private Vector2 lastMoveDir = Vector2.down;

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintBonus = 3f;

    private enum MovementState { idle, walkNorth, walkEast, walkSouth, walkWest, rlidle, upidle }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //coll = GetComponent<CapsuleCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        walkSoundEffect = GetComponent<AudioSource>();

        if (TeleportInfo.useTargetPosition)
        {
            transform.position = TeleportInfo.targetPosition;
            canMove = TeleportInfo.shouldEnableMovement;
            TeleportInfo.useTargetPosition = false;
            TeleportInfo.shouldEnableMovement = false;
        }
    }

    private void Update()
    {

        if (UIManager.isUIMode)
        {
            return; // 阻止移动输入
        }

        if (!canMove)
        {
            dirX = 0;
            dirY = 0;
            anim.speed = 0f;
            return;
        }

        dirX = Input.GetAxisRaw("Horizontal");
        dirY = Input.GetAxisRaw("Vertical");

        UpdateAnimationState();

        Vector2 inputDir = new Vector2(dirX, dirY);
        if (inputDir != Vector2.zero)
        {
            if (!walkSoundEffect.isPlaying)
            {
                walkSoundEffect.Play();
            }
            lastMoveDir = inputDir;
        }
        else
        {
            walkSoundEffect.Stop();
        }
        
        if(EquipmentManager.IsEquipped("臭袜子")){
            walkSoundEffect.Stop();
        }

    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direction = new Vector2(dirX, dirY).normalized;

        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed += sprintBonus;
        }

        rb.velocity = direction * currentSpeed;

        // 动画速度控制
        anim.speed = currentSpeed > moveSpeed ? 1.5f : 1f;
    }

    private void UpdateAnimationState()
    {
        MovementState state;

        if (dirX > 0f)
        {
            sprite.flipX = false;
            state = MovementState.walkEast;
        }
        else if (dirX < 0f)
        {
            sprite.flipX = false;
            state = MovementState.walkWest;
        }
        else if (dirY > 0f)
        {
            sprite.flipX = false;
            state = MovementState.walkNorth;
        }
        else if (dirY < 0f)
        {
            sprite.flipX = false;
            state = MovementState.walkSouth;
        }
        else
        {
            if (lastMoveDir.x > 0f)
            {
                sprite.flipX = true;
                state = MovementState.rlidle;
            }
            else if (lastMoveDir.x < 0f)
            {
                sprite.flipX = false;
                state = MovementState.rlidle;
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

    public void SetLastMoveDir(Vector2 dir)
    {
        lastMoveDir = dir;
    }

    public Vector2 LastMoveDir => lastMoveDir;
}