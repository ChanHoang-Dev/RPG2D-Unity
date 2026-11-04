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

    [Header("Wall Check (BoxCast ngang, dùng chung cho mọi char)")]
    [Tooltip("Bật: chạm tường/vật cản theo hướng đang đi thì khoá di chuyển hướng đó và chuyển sang Idle")]
    [SerializeField] private bool enableWallCheck = true;

    [Tooltip("Layer coi là vật cản. Để Nothing thì dùng groundLayer của PlayerController (layer Ground)")]
    [SerializeField] private LayerMask obstacleLayer;

    [Tooltip("Khoảng cách dò ra phía ngoài mép collider (đơn vị world)")]
    [SerializeField] private float wallCheckDistance = 0.05f;

    [Tooltip("Chiều cao vùng dò so với chiều cao collider. Nhỏ hơn 1 để không quét trúng sàn dưới chân / trần trên đầu. " +
             "Khi nhảy lên cao hơn mép tường thì vùng dò hết chạm tường -> di chuyển lại được.")]
    [Range(0.5f, 1f)] [SerializeField] private float wallCheckHeightRatio = 0.9f;

    [Tooltip("Bật để in Console mỗi khi trạng thái 'kẹt tường' đổi (cho biết do cast hay do contact). Test xong thì tắt.")]
    [SerializeField] private bool debugWallCheck = false;

    [Header("Archer Jump Animation (CHỈ tick cho Archer)")]
    [Tooltip("Bật cho Archer: animation Jump chạy frame 1-6 lúc bay lên, dừng ở frame 6 tại đỉnh, " +
             "chạy tiếp frame 7-12 lúc rơi, chạm đất thì kết thúc. Các char khác để tắt. Mọi char đều nhảy bằng Space.")]
    [SerializeField] private bool IsArcher = false;

    [Tooltip("Tên STATE Jump trong Animator Controller GỐC (Archer dùng override của Knight_White nên là Knight_White-Jump)")]
    [SerializeField] private string archerJumpStateName = "Knight_White-Jump";

    [Tooltip("normalizedTime nằm GIỮA frame 6 của clip Archer_Jump. " +
             "Frame 6 trải từ 0.4167 đến 0.5417 (frame 7 bắt đầu ở 0.5417).")]
    [Range(0f, 1f)] [SerializeField] private float archerApexNormalizedTime = 0.48f;

    [Tooltip("|vận tốc Y| nhỏ hơn mức này được coi là đang ở đỉnh nhảy")]
    [SerializeField] private float archerApexVelocityThreshold = 0.5f;

    private PlayerController controller;
    private PlayerCombat combat;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private readonly RaycastHit2D[] wallHits = new RaycastHit2D[4];
    private readonly ContactPoint2D[] wallContacts = new ContactPoint2D[16];
    private bool lastBlockedDebug;

    // --- trạng thái animation Jump của Archer ---
    private bool archerJumping;
    private bool archerEnteredJumpState;
    private bool archerLeftGround;
    private float archerJumpTimer;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();
        bodyCollider = GetComponent<Collider2D>();
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

        if (IsArcher && archerJumping)
            HandleArcherJumpAnimation();
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
            else
            {
                StopIfPushingIntoWall(); // đang bị khoá trên không mà quán tính đẩy vào tường
            }
            return;
        }

        bool holdingA = Input.GetKey(KeyCode.A);
        bool holdingD = Input.GetKey(KeyCode.D);

        float dir = holdingA && !holdingD ? -1f :
                    holdingD && !holdingA ? 1f : 0f;

        // Phía trước (theo hướng đang bấm) có vật cản không?
        bool blocked = dir != 0f && IsBlockedHorizontally(dir);

        // Chỉ chạy anim Run khi thực sự đang di chuyển; kẹt tường thì về Idle
        animator.SetBool("IsRunning", dir != 0f && !blocked);

        if (dir != 0f)
        {
            // Vẫn quay mặt theo hướng bấm (kể cả khi kẹt tường)
            if (IsBandit == false)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(dir) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
            else
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(-dir) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            Vector2 velocity = rb.linearVelocity;
            if (blocked)
            {
                // Khoá di chuyển về hướng đó: không đẩy vào tường (cũng tránh bị "dính" tường khi đang nhảy)
                velocity.x = 0;
            }
            else
            {
                float speed = runSpeed;
                if (!controller.IsGrounded) speed *= airControlMultiplier;
                velocity.x = dir * speed;
            }
            rb.linearVelocity = velocity;
        }
        else if (controller.IsGrounded)
        {
            // Trên đất mà không giữ phím thì dừng hẳn
            Vector2 velocity = rb.linearVelocity;
            velocity.x = 0;
            rb.linearVelocity = velocity;
        }
        else
        {
            // Trên không, không giữ phím: quán tính vẫn giữ, nhưng không được đẩy vào tường
            StopIfPushingIntoWall();
        }
    }

    // ====================== WALL CHECK ======================
    //
    // Dùng 2 cách dò độc lập, cách nào thấy tường là coi như bị chặn:
    //  1) CastBlocked:    BoxCast MỎNG bắt đầu từ TÂM thân, bắn ngang ra ngoài mép collider.
    //                     (Không bắt đầu từ mép collider: collider Ground là Outlines/edge mỏng, hộp dò bắt đầu
    //                      sát/lún qua cạnh thì Unity thường không báo hit.)
    //  2) ContactBlocked: đọc chính các điểm tiếp xúc physics đang đẩy nhân vật (cùng nguồn với việc nhân vật bị chặn thật).

    LayerMask GetObstacleMask()
    {
        return obstacleLayer.value != 0 ? obstacleLayer : controller.groundLayer;
    }

    bool IsBlockedHorizontally(float dir)
    {
        if (!enableWallCheck || bodyCollider == null || dir == 0f) return false;

        bool byCast = CastBlocked(dir);
        bool byContact = !byCast && ContactBlocked(dir);
        bool blocked = byCast || byContact;

        if (debugWallCheck && blocked != lastBlockedDebug)
        {
            lastBlockedDebug = blocked;
            Debug.Log($"[WallCheck] {name}: blocked={blocked} dir={dir} (cast={byCast}, contact={byContact})", this);
        }
        return blocked;
    }

    bool CastBlocked(float dir)
    {
        Bounds b = bodyCollider.bounds;
        float sign = Mathf.Sign(dir);

        // Hộp mỏng cao ~90% thân, bắt đầu ở tâm (nằm trong collider), bắn ra tới mép + wallCheckDistance
        Vector2 size = new Vector2(Mathf.Min(0.05f, b.size.x), b.size.y * wallCheckHeightRatio);
        float distance = b.extents.x + wallCheckDistance;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.layerMask = GetObstacleMask();
        filter.useTriggers = false;

        int count = Physics2D.BoxCast(b.center, size, 0f, new Vector2(sign, 0f), filter, wallHits, distance);

        for (int i = 0; i < count; i++)
        {
            if (wallHits[i].collider == bodyCollider) continue;
            // Chỉ tính là tường khi mặt va gần thẳng đứng (sàn/dốc thoải không chặn)
            if (Mathf.Abs(wallHits[i].normal.y) < 0.5f) return true;
        }
        return false;
    }

    bool ContactBlocked(float dir)
    {
        int count = rb.GetContacts(wallContacts);
        if (count == 0) return false;

        LayerMask mask = GetObstacleMask();
        float centerX = bodyCollider.bounds.center.x;
        float sign = Mathf.Sign(dir);

        for (int i = 0; i < count; i++)
        {
            ContactPoint2D c = wallContacts[i];

            // Một trong hai collider là của mình (layer Player), cái còn lại phải thuộc layer vật cản
            bool otherIsObstacle =
                (c.collider != null && (mask.value & (1 << c.collider.gameObject.layer)) != 0) ||
                (c.otherCollider != null && (mask.value & (1 << c.otherCollider.gameObject.layer)) != 0);
            if (!otherIsObstacle) continue;

            // Mặt va thẳng đứng (không phải sàn/trần) và nằm ở phía đang di chuyển
            bool vertical = Mathf.Abs(c.normal.x) > 0.5f;
            bool onThatSide = (c.point.x - centerX) * sign > 0f;
            if (vertical && onThatSide) return true;
        }
        return false;
    }

    void StopIfPushingIntoWall()
    {
        float vx = rb.linearVelocity.x;
        if (Mathf.Abs(vx) > 0.01f && IsBlockedHorizontally(vx))
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        Collider2D col = bodyCollider != null ? bodyCollider : GetComponent<Collider2D>();
        if (col == null) return;

        Bounds b = col.bounds;
        float reach = b.extents.x + wallCheckDistance;
        Vector3 size = new Vector3(Mathf.Min(0.05f, b.size.x), b.size.y * wallCheckHeightRatio, 0.01f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(b.center + Vector3.left * reach, size);
        Gizmos.DrawLine(b.center, b.center + Vector3.left * reach);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(b.center + Vector3.right * reach, size);
        Gizmos.DrawLine(b.center, b.center + Vector3.right * reach);
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && controller.IsGrounded && !controller.IsActionLocked)
        {
            animator.SetBool("IsRunning", false);
            animator.ResetTrigger("Jump"); 
            animator.SetTrigger("Jump");
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (IsArcher) BeginArcherJump();
        }
    }

    void BeginArcherJump()
    {
        archerJumping = true;
        archerEnteredJumpState = false;
        archerLeftGround = false;
        archerJumpTimer = 0f;
    }

    void EndArcherJump(bool restoreSpeed)
    {
        archerJumping = false;
        archerEnteredJumpState = false;
        archerLeftGround = false;
        if (restoreSpeed) animator.speed = 1f;
    }

    // Đưa animation tới normalizedTime `target` rồi giữ nguyên (speed = 0)
    void FreezeJumpAt(float currentTime, float target)
    {
        if (Mathf.Abs(currentTime - target) > 0.005f)
            animator.Play(archerJumpStateName, 0, target);
        animator.speed = 0f;
    }

    void HandleArcherJumpAnimation()
    {
        bool combatIsAttacking = combat != null && combat.IsAttacking;

        // Có hành động khác chen vào (đánh, bị đánh, chết...) -> nhả quyền điều khiển animation.
        // PlayerCombat/PlayerHealth tự đặt animator.speed của mình nên không được ghi đè lên.
        if (controller.IsDead || controller.IsActionLocked || combatIsAttacking)
        {
            EndArcherJump(!combatIsAttacking);
            return;
        }

        archerJumpTimer += Time.deltaTime;

        bool grounded = controller.IsGrounded;
        if (!grounded) archerLeftGround = true;

        float vy = rb.linearVelocity.y;

        // 5) CHẠM ĐẤT -> kết thúc Jump (trả speed về 1 để Animator chuyển Jump -> Idle/Run theo IsGrounded).
        // Phải từng rời đất trước, vì IsGrounded chỉ thành false sau 1 bước physics.
        if (archerLeftGround && grounded && vy <= archerApexVelocityThreshold)
        {
            EndArcherJump(true);
            return;
        }

        // Phòng hờ: trigger Jump không kích hoạt / không rời đất được
        if (!archerLeftGround && archerJumpTimer > 0.3f)
        {
            EndArcherJump(true);
            return;
        }

        // Animator còn đang blend sang state Jump -> chờ
        if (animator.IsInTransition(0)) return;

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (!info.IsName(archerJumpStateName))
        {
            // Đã từng vào Jump mà giờ đang ở state khác -> thôi điều khiển
            if (archerEnteredJumpState) EndArcherJump(true);
            return; // chưa vào Jump thì chờ tiếp (Idle/Run -> Jump cần IsGrounded = false)
        }

        archerEnteredJumpState = true;
        float t = info.normalizedTime;

        if (vy > archerApexVelocityThreshold)
        {
            // 2) ĐANG BAY LÊN: chạy bình thường (frame 1 -> 6), không để vượt qua frame 6
            if (t >= archerApexNormalizedTime) FreezeJumpAt(t, archerApexNormalizedTime);
            else animator.speed = 1f;
        }
        else if (vy >= -archerApexVelocityThreshold)
        {
            // 3) ĐỈNH: tạm dừng ở frame 6
            FreezeJumpAt(t, archerApexNormalizedTime);
        }
        else
        {
            // 4) ĐANG RƠI: chạy tiếp frame 7 -> 12
            if (t < archerApexNormalizedTime)
            {
                // Đỉnh tới sớm hơn animation: nhảy tới frame 6 rồi chạy tiếp
                animator.Play(archerJumpStateName, 0, archerApexNormalizedTime);
                animator.speed = 1f;
            }
            else
            {
                animator.speed = 1f;
            }
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