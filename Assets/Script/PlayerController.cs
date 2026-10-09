using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    [Header("Ground Check")]
    public LayerMask groundLayer;

    private Animator animator;
    private Rigidbody2D rb;

    private PlayerMovement movement;
    private PlayerCombat combat;
    private PlayerHealth health;

    private bool isGrounded = true;
    private int groundContactCount = 0; // đếm số điểm va chạm đang tiếp xúc với Ground, tránh false negative khi đứng giữa 2 platform liền nhau
    private bool isActionLocked = false;

    public Animator Animator => animator;
    public Rigidbody2D Rb => rb;
    public bool IsGrounded => isGrounded;
    public bool IsActionLocked => isActionLocked;
    public bool IsDead => health != null && health.IsDead;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        health = GetComponent<PlayerHealth>();
    }

    void Start()
    {
        animator.SetBool("IsGrounded", isGrounded);
    }

    void Update()
    {
        if (IsDead) return;

        movement.Tick();
        combat.TickInput();
        combat.TickComboTimer();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsGroundLayer(collision.gameObject.layer) && IsLandingOnTop(collision))
        {
            groundContactCount++;
            SetGrounded(true);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (IsGroundLayer(collision.gameObject.layer) && IsLandingOnTop(collision) && !isGrounded)
        {
            groundContactCount++;
            SetGrounded(true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsGroundLayer(collision.gameObject.layer))
        {
            groundContactCount = Mathf.Max(0, groundContactCount - 1);
            if (groundContactCount == 0)
                SetGrounded(false);
        }
    }

    bool IsGroundLayer(int layer)
    {
        return (groundLayer.value & (1 << layer)) != 0;
    }

    bool IsLandingOnTop(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f) return true;
        }
        return false;
    }

    void SetGrounded(bool grounded)
    {
        if (grounded == isGrounded) return;
        isGrounded = grounded;
        if (animator != null)
            animator.SetBool("IsGrounded", grounded);
    }

    /// <summary>
    /// Khoá hành động (Jump/Move) trong `duration` giây. Dùng chung bởi PlayerCombat (attack/hurt)
    /// và PlayerHealth (hurt/death) để tránh 2 nơi tự giữ timer riêng gây lệch trạng thái.
    /// </summary>
    public void StartActionLock(float duration)
    {
        isActionLocked = true;
        CancelInvoke(nameof(EndActionLock));
        Invoke(nameof(EndActionLock), duration);
    }

    /// <summary>Khoá vĩnh viễn, dùng khi chết (không có Invoke tự mở lại).</summary>
    public void LockActionsPermanently()
    {
        isActionLocked = true;
        CancelInvoke(nameof(EndActionLock));
    }

    void EndActionLock()
    {
        isActionLocked = false;
    }
}