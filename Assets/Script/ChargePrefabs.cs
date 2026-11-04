using System.Collections;
using UnityEngine;

/// <summary>
/// Chiêu của Wizard: bay bằng animation "charge_fly",
/// khi chạm quái thì chuyển sang "charge_collision" rồi tự hủy.
/// Gắn lên prefab chiêu (cần Rigidbody2D + Collider2D bật Is Trigger + Animator).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ChargePrefabs : MonoBehaviour
{
    [Header("Animation State Names (phải trùng tên state trong Animator)")]
    [SerializeField] private string flyState = "Charge_Fly";
    [SerializeField] private string collisionState = "Charge_Collision";

    [Header("Fallback")]
    [Tooltip("Chỉ dùng khi không tính được từ stat (không có stat người bắn hoặc quái không có CombatEntity).")]
    [SerializeField] private int fallbackDamage = 1;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;

    private Vector2 direction;
    private float speed;
    private ICombatStats attackerStats; // stat của Wizard, chụp lúc bắn
    private LayerMask enemyLayer;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        col.isTrigger = true;
    }

    /// <param name="attacker">Stat của Wizard (chụp lúc bắn, nên vẫn đúng nếu Wizard chết khi chiêu còn đang bay).</param>
    public void Init(ICombatStats attacker, Vector2 dir, float moveSpeed, float lifeTime, LayerMask targetLayer)
    {
        direction = dir.normalized;
        speed = moveSpeed;
        attackerStats = attacker;
        enemyLayer = targetLayer;

        // Lật sprite theo hướng bay
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (direction.x >= 0 ? 1 : -1);
        transform.localScale = scale;

        // Nếu hướng bay chéo / dọc thì xoay theo hướng (bỏ comment nếu cần)
        // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(0, 0, angle);

        PlayState(flyState);
        rb.linearVelocity = direction * speed; // Unity 6. Unity 2022 trở xuống dùng rb.velocity

        // Hết thời gian mà chưa trúng gì thì tự hủy
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        // Chỉ va chạm với layer của quái
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        Health targetHealth = other.GetComponentInParent<Health>();
        if (targetHealth == null) return;

        // Xác quái chưa kịp biến mất: cho chiêu bay xuyên qua
        if (targetHealth.IsDead()) return;

        hasHit = true;

        // Dừng chiêu và tắt collider để không gây damage 2 lần
        rb.linearVelocity = Vector2.zero; // Unity 2022 trở xuống: rb.velocity
        col.enabled = false;

        // Damage tính từ chỉ số của Wizard (giống ArrowManager của Archer)
        CombatEntity targetCombat = other.GetComponentInParent<CombatEntity>();
        if (attackerStats != null && targetCombat != null)
        {
            DamageResult result = CombatCalculator.CalculateDamage(attackerStats, targetCombat.GetStats());
            targetHealth.TakeDamage(result);
        }
        else
        {
            targetHealth.TakeDamage(fallbackDamage);
        }

        StartCoroutine(PlayCollisionAndDestroy());
    }

    private IEnumerator PlayCollisionAndDestroy()
    {
        PlayState(collisionState);

        // Chờ 1 frame để Animator cập nhật state, rồi đợi hết độ dài animation
        yield return null;

        float length = 0.3f; // dự phòng nếu không có Animator
        if (animator != null)
            length = animator.GetCurrentAnimatorStateInfo(0).length;

        yield return new WaitForSeconds(length);
        Destroy(gameObject);
    }

    private void PlayState(string stateName)
    {
        if (animator != null)
            animator.Play(stateName, 0, 0f);
    }
}