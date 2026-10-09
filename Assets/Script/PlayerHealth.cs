using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerCombat))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Máu tối đa của Player")]
    public int maxHealth = 2000;
    [Tooltip("Máu hiện tại (chỉ để xem trong Inspector lúc runtime, đừng chỉnh tay)")]
    [SerializeField] private int currentHealth;

    [Header("Combat Stats")]
    [SerializeField] private CombatEntity combatEntity; // NEW: lấy maxHealth từ PlayerStatsSO + Runtime

    [Header("Hurt")]
    [Tooltip("Thời lượng khoá hành động khi bị đánh trúng (giây)")]
    public float hurtLockDuration = 0.3f;

    [Header("Death")]
    public float deathLockDuration = 999f;

    private PlayerController controller;
    private PlayerCombat combat;
    private Animator animator;
    private Rigidbody2D rb;

    private bool isDead = false;

    public System.Action<int, int> OnHealthChanged;
    public System.Action OnDied;

    // NEW: bắn cho damage popup UI biết amount/isCrit/isMiss để hiện đúng ("Miss", số to nếu crit...)
    public System.Action<DamageResult> OnDamageTaken;

    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();

        if (combatEntity == null)
            combatEntity = GetComponent<CombatEntity>();

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
                Debug.LogWarning($"PlayerHealth.cs: maxHealth ({maxHealth}) không khớp với CombatEntity ({entityMaxHealth}). " +
                                 $"Sửa lại hoặc bỏ field maxHealth để lấy từ CombatEntity.", this);
            }
        }
    }

    void Start()
    {
        animator = controller.Animator;
        rb = controller.Rb;
        if (combatEntity != null)
            maxHealth = combatEntity.GetMaxHealth(); // lấy theo upgrade level hiện tại

        currentHealth = maxHealth;
        ValidateMaxHealth();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>Đường cũ, vẫn giữ để tương thích chỗ nào đang gọi trực tiếp int</summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            OnHitByEnemy();
        }
    }

    /// <summary>NEW: nhận DamageResult từ CombatCalculator, xử lý cả Miss</summary>
    public void TakeDamage(DamageResult result)
    {
        if (isDead) return;

        OnDamageTaken?.Invoke(result); // popup UI tự đọc isMiss để hiện "Miss"

        if (result.isMiss) return; // Player né được, không trừ máu, không trigger Hurt

        TakeDamage(result.amount);
    }

    void OnHitByEnemy()
    {
        combat.CancelCombo();

        animator.ResetTrigger("Hurt");
        animator.SetTrigger("Hurt");
        controller.StartActionLock(hurtLockDuration);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        combat.CancelCombo();
        controller.LockActionsPermanently();

        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;

        OnDied?.Invoke();
    }
}