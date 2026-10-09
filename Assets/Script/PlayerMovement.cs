using UnityEngine;
[RequireComponent(typeof(PlayerController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    public float runSpeed = 6f;
    public float airControlMultiplier = 0.8f; 

    [Header("Jump")]
    public float jumpForce = 8f;

    [Tooltip("Nhân vật Bandit bị flip ngược hướng scale so với các nhân vật khác (do sprite gốc quay sẵn)")]
    [SerializeField] private bool IsBandit = false;

    private PlayerController controller;
    private Animator animator;
    private Rigidbody2D rb;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
    }

    void Start()
    {
        animator = controller.Animator;
        rb = controller.Rb;
    }

    public void Tick()
    {
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        if (controller.IsActionLocked)
        {
            if (controller.IsGrounded)
            {
                Vector2 stopVel = rb.linearVelocity;
                stopVel.x = 0;
                rb.linearVelocity = stopVel;
            }
            return;
        }

        bool holdingA = Input.GetKey(KeyCode.A);
        bool holdingD = Input.GetKey(KeyCode.D);
        bool holdingMoveKey = holdingA || holdingD;

        animator.SetBool("IsRunning", holdingMoveKey);

        if (holdingMoveKey)
        {
            float dir = holdingA && !holdingD ? -1f :
                        holdingD && !holdingA ? 1f : 0f;

            float speed = runSpeed;
            if (!controller.IsGrounded) speed *= airControlMultiplier;

            Vector2 velocity = rb.linearVelocity;
            velocity.x = dir * speed;
            rb.linearVelocity = velocity;

            if (dir != 0f && IsBandit == false)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(dir) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            if (dir != 0f && IsBandit == true)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(-dir) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
        else if (controller.IsGrounded)
        {
            // Trên đất mà không giữ phím thì dừng hẳn
            Vector2 velocity = rb.linearVelocity;
            velocity.x = 0;
            rb.linearVelocity = velocity;
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.IsGrounded && !controller.IsActionLocked)
        {
            animator.SetBool("IsRunning", false);
            animator.ResetTrigger("Jump"); // xóa trigger tồn đọng (nếu có) trước khi set lại, tránh delay
            animator.SetTrigger("Jump");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
    public int FacingDirection
    {
        get
        {
            bool facingRightScale = transform.localScale.x > 0;
            // Bandit bị flip ngược scale so với hướng di chuyển thật, nên facing thật ngược lại IsBandit
            bool facingRight = IsBandit ? !facingRightScale : facingRightScale;
            return facingRight ? 1 : -1;
        }
    }
}