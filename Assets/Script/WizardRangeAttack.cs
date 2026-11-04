using UnityEngine;

/// <summary>
/// Tấn công tầm xa cho Wizard: bắn ra chiêu "charge".
/// Gắn script này lên Player (Wizard).
/// </summary>
public class WizardRangeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;                 // Animator của Wizard
    [SerializeField] private Transform firePoint;               // Vị trí chiêu xuất hiện (đầu gậy)
    [SerializeField] private ChargePrefabs projectilePrefab; // Prefab chiêu charge

    [Header("Attack Settings")]
    [SerializeField] private KeyCode attackKey = KeyCode.J;
    [SerializeField] private float attackCooldown = 0.6f;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float projectileLifeTime = 3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Animation")]
    [SerializeField] private string attackTrigger = "Attack";
    [Tooltip("Bật nếu muốn chiêu chỉ xuất hiện đúng frame vung gậy (gọi SpawnProjectile từ Animation Event). " +
             "Tắt thì chiêu bắn ngay khi bấm phím.")]
    [SerializeField] private bool useAnimationEvent = false;

    private CombatEntity combatEntity;
    private float nextAttackTime;
    private Vector2 pendingDirection = Vector2.right;

    private void Awake()
    {
        combatEntity = GetComponent<CombatEntity>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime)
        {
            Attack();
        }
    }

    private void Attack()
    {
        nextAttackTime = Time.time + attackCooldown;
        pendingDirection = GetFacingDirection();

        if (animator != null)
            animator.SetTrigger(attackTrigger);

        if (!useAnimationEvent)
            SpawnProjectile();
    }

    /// <summary>
    /// Có thể gọi từ Animation Event ở frame vung gậy của animation attack.
    /// </summary>
    public void SpawnProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        ChargePrefabs projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        // Chụp stat của Wizard tại thời điểm bắn (damage tính theo Attack/Crit... của nhân vật)
        ICombatStats attackerStats = combatEntity != null ? combatEntity.GetStats() : null;
        projectile.Init(attackerStats, pendingDirection, projectileSpeed, projectileLifeTime, enemyLayer);
    }

    // TODO: đổi cho giống cách Archer xác định hướng (nếu Archer dùng hướng di chuyển / chuột thì copy sang đây)
    private Vector2 GetFacingDirection()
    {
        return transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
    }
}