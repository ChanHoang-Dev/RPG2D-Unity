using UnityEngine;

public class FlyingEnemyProperties : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Flight Settings")]
    [SerializeField] private float hoverAmplitude = 0.3f;
    [SerializeField] private float hoverFrequency = 3f;
    [SerializeField] private float minHeightAboveGround = 1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Tornado Attack")]
    [SerializeField] private GameObject tornadoPrefab;
    [SerializeField] private CombatEntity combatEntity;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Health health;
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
    private float baseScaleX = 1f;

    private float hoverOffset;
    [Header("Attack Range Detection")]
    private float lastInRangeTime = -999f;
    private const float RANGE_TOLERANCE = 0.1f;

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

        if (health == null)
            health = GetComponent<Health>();

        if (combatEntity == null)
            combatEntity = GetComponent<CombatEntity>();

        baseScaleX = Mathf.Abs(transform.localScale.x);

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        }

        hoverOffset = Random.Range(0f, 100f);
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
            return;
        }
        FacePlayer();
        bool cooldownReady = Time.time - lastAttackTime >= attackCooldown;

        if (playerInAttackRange && cooldownReady)
        {
            TryAttack();
        }
        else
        {
            ChasePlayer();
        }
    }
    public void NotifyPlayerInRange()
    {
        lastInRangeTime = Time.time;
    }
    private bool playerInAttackRange => Time.time - lastInRangeTime <= RANGE_TOLERANCE;

    private void TryAttack()
    {
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


    public void SpawnTornado()
    {
        if(health != null && health.IsDead()) return;
        if (tornadoPrefab == null) return;

        Vector2 spawnPos = transform.position;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 50f, groundLayer);
        if (hit.collider != null)
        {
            spawnPos.y = hit.point.y / 2;
        }
        spawnPos.x = transform.position.x;

        float facingDir = Mathf.Sign(transform.localScale.x);
        Vector2 direction = new Vector2(facingDir, 0f);

        GameObject tornado = Instantiate(tornadoPrefab, spawnPos, Quaternion.identity);

        TornadoHazard hazard = tornado.GetComponent<TornadoHazard>();
        if (hazard != null)
        {
            hazard.Init(combatEntity, direction);
        }
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
        if (Mathf.Abs(direction.x) < 0.05f) return;
        float facing = direction.x > 0 ? 1f : -1f;
        transform.localScale = new Vector3(baseScaleX * facing, transform.localScale.y, transform.localScale.z);
    }

    private void ChasePlayer()
    {
        Vector2 toPlayer = (Vector2)player.position - rb.position;
        float dist = toPlayer.magnitude;

        // Đã đủ gần thì không lao vào chồng lên Player nữa
        if (dist <= stopDistance) return;

        Vector2 direction = toPlayer / dist;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        float hover = Mathf.Sin((Time.time + hoverOffset) * hoverFrequency) * hoverAmplitude;

        Vector2 moveDir = (direction + perpendicular * hover).normalized;
        Vector2 newPos = rb.position + moveDir * moveSpeed * Time.deltaTime;
        newPos.y = ClampAboveGround(newPos.y);

        rb.MovePosition(newPos);
    }

    private float ClampAboveGround(float targetY)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 50f, groundLayer);

        if (hit.collider != null)
        {
            float minY = hit.point.y + minHeightAboveGround;
            if (targetY < minY)
                return minY;
        }

        return targetY;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 rayStart = transform.position;
        Vector3 rayEnd = rayStart + Vector3.down * 50f;
        Gizmos.DrawLine(rayStart, rayEnd);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 50f, groundLayer);
        if (hit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.1f);

            float minY = hit.point.y + minHeightAboveGround;
            Gizmos.color = Color.yellow;
            Vector3 lineStart = new Vector3(transform.position.x - 1f, minY, 0f);
            Vector3 lineEnd = new Vector3(transform.position.x + 1f, minY, 0f);
            Gizmos.DrawLine(lineStart, lineEnd);
        }

        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);

            Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;

            Vector2 perpendicular = new Vector2(-direction.y, direction.x);
            Vector3 hoverPosA = transform.position + (Vector3)(perpendicular * hoverAmplitude);
            Vector3 hoverPosB = transform.position - (Vector3)(perpendicular * hoverAmplitude);

            Gizmos.color = new Color(1f, 0f, 1f);
            Gizmos.DrawLine(hoverPosA, hoverPosB);
            Gizmos.DrawSphere(hoverPosA, 0.05f);
            Gizmos.DrawSphere(hoverPosB, 0.05f);
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}