using UnityEngine;

public class EnemyProperties : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private WeaponHitbox weaponHitbox;
    [SerializeField] private Health health;
    [SerializeField] private SkullDetach skullDetach;
    private Rigidbody2D rb;
    private Collider2D col;

    private Transform player;
    private const string ATTACK_TRIGGER = "Attack";

    [Header("Attack Settings")]
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackLockDuration = 0.6f;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    [Header("Hurt Settings")]
    [SerializeField] private float hurtLockDuration = 0.4f;
    private bool isHurt = false;
    [SerializeField] private bool IsNightBorne = false;
    private float baseScaleX = 1f;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private LayerMask groundLayer;          // Nền đất (để kiểm tra đang đứng)
    [SerializeField] private LayerMask obstacleLayer;        // Vật cản/tường (có thể trùng groundLayer)
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float obstacleCheckDistance = 0.3f;  // Khoảng tia vươn ra khỏi mép collider
    [SerializeField] private float lowCheckHeight = 0.15f;   // Độ cao tia thấp, tính từ đáy collider
    [SerializeField] private float maxJumpableHeight = 1.5f; // Vật cản cao hơn mức này thì không nhảy
    [Header("Jump To Player (bục cao)")]
    [SerializeField] private bool jumpTowardHigherPlayer = true;
    [SerializeField] private float playerHeightThreshold = 0.8f; // Player phải cao hơn quái tối thiểu bấy nhiêu
    private float lastJumpTime = -999f;
    private float moveDirX = 1f; // hướng di chuyển thực tế (+1 phải, -1 trái)

    void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (weaponHitbox == null)
            weaponHitbox = GetComponentInChildren<WeaponHitbox>();

        if (health == null)
            health = GetComponent<Health>();

        if (skullDetach == null)
            skullDetach = GetComponentInChildren<SkullDetach>(true);
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (health != null && health.IsDead()) return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (isAttacking || isHurt)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        FacePlayer();

        if (distance > stopDistance)
        {
            ChasePlayer();
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            TryAttack();
        }
    }

    // ===================== ATTACK / HURT =====================

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        PlayAttack();
    }

    public void PlayAttack()
    {
        if (animator != null)
        {
            animator.ResetTrigger(ATTACK_TRIGGER);
            animator.SetTrigger(ATTACK_TRIGGER);
        }

        isAttacking = true;
        CancelInvoke(nameof(EndAttackLock));
        Invoke(nameof(EndAttackLock), attackLockDuration);
    }

    private void EndAttackLock()
    {
        isAttacking = false;
    }

    public void CancelAttack()
    {
        isAttacking = false;
        CancelInvoke(nameof(EndAttackLock));

        if (animator != null)
            animator.ResetTrigger(ATTACK_TRIGGER);

        isHurt = true;
        CancelInvoke(nameof(EndHurtLock));
        Invoke(nameof(EndHurtLock), hurtLockDuration);
    }

    private void EndHurtLock()
    {
        isHurt = false;
    }

    public void EnableWeaponHitbox()
    {
        if (weaponHitbox != null)
            weaponHitbox.EnableHitbox();
    }

    public void DisableWeaponHitbox()
    {
        if (weaponHitbox != null)
            weaponHitbox.DisableHitbox();
    }

    public void DetachSkull()
    {
        if (skullDetach == null) return;
        bool isFacingRight = transform.localScale.x < 0;
        skullDetach.Trigger(isFacingRight);
    }

    // ===================== MOVEMENT =====================

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        if (Mathf.Abs(direction.x) < 0.01f) return;
        float facing = direction.x > 0
            ? (IsNightBorne ? 1f : -1f)
            : (IsNightBorne ? -1f : 1f);
        transform.localScale = new Vector3(baseScaleX * facing, transform.localScale.y, transform.localScale.z);
    }

    private void ChasePlayer()
    {
        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) > 0.01f)
            moveDirX = dx > 0 ? 1f : -1f;

        rb.linearVelocity = new Vector2(moveDirX * moveSpeed, rb.linearVelocity.y);

        TryJump();
    }

    // ===================== JUMP =====================

    private bool IsGrounded()
    {
        if (col == null) return true;

        Bounds b = col.bounds;
        Vector2 origin = new Vector2(b.center.x, b.min.y);
        Vector2 size = new Vector2(b.size.x * 0.9f, groundCheckDistance);

        return Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDistance, groundLayer).collider != null;
    }

    private void TryJump()
    {
        if (col == null) return;
        if (Time.time - lastJumpTime < jumpCooldown) return;
        if (!IsGrounded()) return;

        if (ShouldJumpObstacle() || ShouldJumpToHigherPlayer())
            Jump();
    }

    /// <summary>
    /// Bắn 2 tia ngang từ mép collider phía trước: tia thấp (gần chân) và tia cao (ở mức nhảy tối đa).
    /// Tia thấp chạm + tia cao không chạm -> vật cản đủ thấp để nhảy qua.
    /// Dùng collider.bounds nên không bị ảnh hưởng khi localScale.x đảo dấu.
    /// </summary>
    private bool ShouldJumpObstacle()
    {
        Bounds b = col.bounds;
        float frontX = b.center.x + moveDirX * b.extents.x;
        Vector2 dir = new Vector2(moveDirX, 0f);

        Vector2 lowOrigin  = new Vector2(frontX, b.min.y + lowCheckHeight);
        Vector2 highOrigin = new Vector2(frontX, b.min.y + maxJumpableHeight);

        bool lowHit  = Physics2D.Raycast(lowOrigin,  dir, obstacleCheckDistance, obstacleLayer);
        bool highHit = Physics2D.Raycast(highOrigin, dir, obstacleCheckDistance, obstacleLayer);

        return lowHit && !highHit;
    }

    /// <summary>
    /// Player đứng cao hơn quái đáng kể và phía trước quái có vật cản -> nhảy để leo lên bục.
    /// Vẫn cần tia thấp chạm vật cản để tránh nhảy lung tung khi player chỉ đang nhảy trên không.
    /// </summary>
    private bool ShouldJumpToHigherPlayer()
    {
        if (!jumpTowardHigherPlayer || player == null) return false;

        float heightDiff = player.position.y - transform.position.y;
        if (heightDiff < playerHeightThreshold) return false;

        Bounds b = col.bounds;
        float frontX = b.center.x + moveDirX * b.extents.x;
        Vector2 dir = new Vector2(moveDirX, 0f);
        Vector2 lowOrigin = new Vector2(frontX, b.min.y + lowCheckHeight);

        return Physics2D.Raycast(lowOrigin, dir, obstacleCheckDistance, obstacleLayer);
    }

    private void Jump()
    {
        lastJumpTime = Time.time;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D c = col != null ? col : GetComponent<Collider2D>();
        if (c == null) return;

        Bounds b = c.bounds;
        float frontX = b.center.x + moveDirX * b.extents.x;
        Vector3 dir = new Vector3(moveDirX, 0f, 0f);

        // Tia thấp (đỏ)
        Gizmos.color = Color.red;
        Vector3 low = new Vector3(frontX, b.min.y + lowCheckHeight, 0f);
        Gizmos.DrawLine(low, low + dir * obstacleCheckDistance);

        // Tia cao (vàng)
        Gizmos.color = Color.yellow;
        Vector3 high = new Vector3(frontX, b.min.y + maxJumpableHeight, 0f);
        Gizmos.DrawLine(high, high + dir * obstacleCheckDistance);

        // Vùng kiểm tra chạm đất (xanh lá)
        Gizmos.color = Color.green;
        Vector3 groundPos = new Vector3(b.center.x, b.min.y - groundCheckDistance * 0.5f, 0f);
        Gizmos.DrawWireCube(groundPos, new Vector3(b.size.x * 0.9f, groundCheckDistance, 0f));
    }
}