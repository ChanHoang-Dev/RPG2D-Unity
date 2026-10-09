using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WeaponHitbox : MonoBehaviour
{
    [Header("Attacker")]
    [Tooltip("CombatEntity của Player/Enemy đang sở hữu vũ khí này. " +
             "Nếu để trống, sẽ tự tìm trên object cha lúc Awake.")]
    [SerializeField] private CombatEntity ownerCombat;

    [Header("Damage Settings (fallback)")]
    [Tooltip("Chỉ dùng khi ownerCombat null (object test nhanh, không qua hệ thống stat).")]
    [SerializeField] private int fallbackDamage = 1;
    [SerializeField] private bool damageEnabled = true; // Có thể tắt damage khi test animation, hoặc khi muốn vũ khí chỉ để "vẽ" mà không gây sát thương

    [Header("Target Filter")]
    [Tooltip("Chọn LAYER của phe đối phương, không phải Tag. " +
             "Hitbox thuộc ENEMY -> chọn layer Player. Hitbox thuộc PLAYER -> chọn layer Enemy " +
             "(tick chọn layer Enemy là đủ, không cần liệt kê từng loại quái Goblin/Skeleton/Bandit...).")]
    [SerializeField] private LayerMask targetLayer;

    private Collider2D weaponCollider;

    void Awake()
    {
        weaponCollider = GetComponent<Collider2D>();
        weaponCollider.isTrigger = true;

        // Mặc định TẮT hitbox, chỉ bật đúng frame chém qua Animation Event
        weaponCollider.enabled = false;

        if (ownerCombat == null)
            ownerCombat = GetComponentInParent<CombatEntity>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!damageEnabled) return; // Nếu đang tắt damage, không làm gì cả
        if (!IsInTargetLayer(other.gameObject)) return;

        // Trường hợp target là PLAYER
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            ApplyDamage(playerHealth, other);
            return;
        }

        // Trường hợp target là ENEMY
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth == null)
            targetHealth = other.GetComponentInParent<Health>(); // Phòng trường hợp Collider nằm trên child

        if (targetHealth != null)
        {
            ApplyDamage(targetHealth, other);
        }
    }

    private void ApplyDamage(PlayerHealth playerHealth, Collider2D targetCollider)
    {
        CombatEntity targetCombat = targetCollider.GetComponentInParent<CombatEntity>();
        if (ownerCombat != null && targetCombat != null)
        {
            DamageResult result = ownerCombat.DealDamageTo(targetCombat);
            playerHealth.TakeDamage(result);
        }
        else
        {
            playerHealth.TakeDamage(fallbackDamage);
        }
    }

    private void ApplyDamage(Health targetHealth, Collider2D targetCollider)
    {
        CombatEntity targetCombat = targetCollider.GetComponentInParent<CombatEntity>();
        if (ownerCombat != null && targetCombat != null)
        {
            DamageResult result = ownerCombat.DealDamageTo(targetCombat);
            targetHealth.TakeDamage(result);
        }
        else
        {
            targetHealth.TakeDamage(fallbackDamage);
        }
    }

    public void EnableHitbox()
    {
        weaponCollider.enabled = true;
    }

    public void DisableHitbox()
    {
        weaponCollider.enabled = false;
    }

    // So bit trực tiếp trên layer của object, không cần liệt kê tên từng loại quái/Player như tag array cũ.
    // Thêm quái loại mới -> chỉ cần gán đúng layer Enemy cho nó, không phải sửa lại WeaponHitbox nào cả.
    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}