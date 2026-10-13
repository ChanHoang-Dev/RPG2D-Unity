using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    [Header("Combat Stats")]
    [SerializeField] private CombatEntity combatEntity; // NEW: lấy maxHealth từ EnemyStatsSO nếu có gắn

    [Header("References")]
    [SerializeField] private Animator animator;

    private bool isDead = false;

    private const string IS_DEAD_BOOL = "IsDead";
    private const string HURT_TRIGGER = "Hurt";

    private int dieCallCount = 0;

    // Cache để tránh GetComponent mỗi lần nhận damage
    private EnemyProperties cachedEnemyProps;
    private FlyingEnemyProperties cachedFlyingEnemy;

    // NEW: bắn khi nhận damage để UI popup số lên màn hình biết crit/miss
    public System.Action<DamageResult> OnDamageTaken;

    void Awake()
    {
        if (combatEntity == null)
            combatEntity = GetComponent<CombatEntity>();

        // Nếu có CombatEntity (EnemyStatsSO) thì lấy maxHealth từ đó, không thì dùng field cũ
        if (combatEntity != null)
            maxHealth = combatEntity.GetMaxHealth();

        currentHealth = maxHealth;
        ValidateMaxHealth();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        cachedEnemyProps = GetComponent<EnemyProperties>();
        cachedFlyingEnemy = GetComponent<FlyingEnemyProperties>();
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateMaxHealth();
    }
#endif
    private void ValidateMaxHealth()
    {
        CombatEntity entity = GetComponent<CombatEntity>();
        if (entity != null)
        {
            int entityMaxHealth = entity.GetMaxHealth();
            if (entityMaxHealth > 0 && maxHealth != entityMaxHealth)
            {
                Debug.LogWarning($"Health.cs: maxHealth ({maxHealth}) không khớp với CombatEntity ({entityMaxHealth}). " +
                                 $"Sửa lại hoặc bỏ field maxHealth để lấy từ CombatEntity.", this);
            }
        }
    }
    void Start()
    {
        if (combatEntity != null)
            maxHealth = combatEntity.GetMaxHealth();

        currentHealth = maxHealth;
        ValidateMaxHealth();
    }

    /// <summary>Gọi hàm này từ WeaponHitbox khi bị trúng đòn (đường cũ, vẫn giữ để tương thích)</summary>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (cachedEnemyProps != null) cachedEnemyProps.CancelAttack();
            if (cachedFlyingEnemy != null) cachedFlyingEnemy.CancelAttack();

            if (animator != null)
            {
                animator.ResetTrigger(HURT_TRIGGER);
                animator.SetTrigger(HURT_TRIGGER);
            }
        }
    }
    public void TakeDamage(DamageResult result)
    {
        if (isDead) return;

        // Enemy không có Miss, nhưng vẫn phòng hờ nếu logic đổi sau này
        if (result.isMiss) return;

        OnDamageTaken?.Invoke(result);
        TakeDamage(result.amount); // amount có thể là -1, currentHealth vẫn trừ bình thường
    }

    private void Die()
    {
        dieCallCount++;
        isDead = true;

        if (animator != null)
        {
            animator.SetBool(IS_DEAD_BOOL, true);
        }
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        EnemyProperties chase = GetComponent<EnemyProperties>();
        if (chase != null) chase.enabled = false;
        FlyingEnemyProperties flyingEnemy = GetComponent<FlyingEnemyProperties>();
        if(flyingEnemy != null) flyingEnemy.enabled = false;

        Destroy(gameObject, 5f);
    }

    public bool IsDead()
    {
        return isDead;
    }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
}