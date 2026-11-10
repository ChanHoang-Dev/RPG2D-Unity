using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Vòng lửa quanh nhân vật: mỗi enemy đứng trong vùng va chạm nhận damage 1 lần mỗi tickInterval giây.
/// Gắn lên object Ring (con của Player), kèm 1 CircleCollider2D.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FireRingDamage : MonoBehaviour
{
    [Header("Attacker")]
    [Tooltip("CombatEntity của Player. Để trống sẽ tự tìm trên object cha lúc Awake.")]
    [SerializeField] private CombatEntity ownerCombat;

    [Header("Damage Settings")]
    [Tooltip("Khoảng thời gian giữa 2 lần gây damage lên CÙNG một enemy.")]
    [SerializeField] private float tickInterval = 0.5f;
    [Tooltip("Hệ số nhân sát thương so với 1 đòn đánh gốc của Player (vòng lửa thường yếu hơn đòn thường).")]
    [SerializeField] private float damageMultiplier = 1f;
    [Tooltip("Tắt = vòng lửa không bao giờ bị Miss (roll lại nếu trượt).")]
    [SerializeField] private bool allowMiss = false;
    [Tooltip("Chỉ dùng khi ownerCombat null.")]
    [SerializeField] private int fallbackDamage = 1;
    [SerializeField] private bool damageEnabled = true;

    [Header("Target Filter")]
    [Tooltip("Mặc định Everything. Script tự bỏ qua Player và mọi object không có Health.")]
    [SerializeField] private LayerMask targetLayer = ~0;

    [Header("Ring Shape (tùy chọn)")]
    [Tooltip("0 = cả hình đĩa đều ăn damage. > 0 = enemy nằm hoàn toàn trong lỗ giữa (bán kính này, đơn vị local " +
             "giống radius của CircleCollider2D) thì không ăn damage, chỉ ăn khi chạm phần vành.")]
    [SerializeField] private float innerRadius = 0f;

    private readonly Dictionary<Health, float> nextTickTime = new Dictionary<Health, float>();
    private readonly List<Health> purgeBuffer = new List<Health>();

    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        if (ownerCombat == null)
            ownerCombat = GetComponentInParent<CombatEntity>();

        // OnTriggerStay2D không được gọi khi CẢ HAI Rigidbody2D đều đã "ngủ".
        // Cho body của Player không bao giờ ngủ để vòng lửa luôn tick đều.
        Rigidbody2D rb = GetComponentInParent<Rigidbody2D>();
        if (rb != null) rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    void OnDisable()
    {
        nextTickTime.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        // Debug.Log($"Ring chạm: {other.name} | layer={LayerMask.LayerToName(other.gameObject.layer)} | trigger={other.isTrigger}");
        if (!damageEnabled) return;
        if (other.isTrigger) return; // bỏ qua trigger của enemy (vùng detect, hitbox vũ khí...)
        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.GetComponentInParent<PlayerHealth>() != null) return;

        Health health = other.GetComponentInParent<Health>();
        if (health == null || health.IsDead()) return;
        if (!IsOutsideInnerHole(other)) return;

        float now = Time.time;
        if (nextTickTime.TryGetValue(health, out float next) && now < next) return;

        if (nextTickTime.Count > 32) Purge(now);
        nextTickTime[health] = now + tickInterval;

        ApplyDamage(health, other);
    }

    private void ApplyDamage(Health target, Collider2D targetCollider)
    {
        CombatEntity targetCombat = targetCollider.GetComponentInParent<CombatEntity>();

        if (ownerCombat != null && targetCombat != null)
        {
            DamageResult result = ownerCombat.DealDamageTo(targetCombat);

            if (!allowMiss)
            {
                for (int i = 0; i < 5 && result.isMiss; i++)
                    result = ownerCombat.DealDamageTo(targetCombat);

                if (result.isMiss)
                    result = new DamageResult { amount = Mathf.Max(1, fallbackDamage) };
            }

            if (!result.isMiss && !Mathf.Approximately(damageMultiplier, 1f))
                result.amount = Mathf.Max(1, Mathf.RoundToInt(result.amount * damageMultiplier));

            target.TakeDamage(result); // đi qua OnDamageTaken nên popup số damage vẫn hiện
        }
        else
        {
            target.TakeDamage(Mathf.Max(1, Mathf.RoundToInt(fallbackDamage * damageMultiplier)));
        }
    }

    // Enemy có điểm xa tâm nhất vẫn nằm trong lỗ giữa => coi như không chạm vành.
    private bool IsOutsideInnerHole(Collider2D other)
    {
        if (innerRadius <= 0f) return true;

        Vector2 c = transform.position;
        Bounds b = other.bounds;
        float dx = Mathf.Max(Mathf.Abs(b.min.x - c.x), Mathf.Abs(b.max.x - c.x));
        float dy = Mathf.Max(Mathf.Abs(b.min.y - c.y), Mathf.Abs(b.max.y - c.y));
        float farthest = new Vector2(dx, dy).magnitude;

        return farthest > innerRadius * WorldScale();
    }

    private float WorldScale()
    {
        Vector3 s = transform.lossyScale;
        return Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
    }

    // Dọn entry của enemy đã bị Destroy hoặc đã rời vùng từ lâu.
    private void Purge(float now)
    {
        purgeBuffer.Clear();
        foreach (var kv in nextTickTime)
            if (kv.Key == null || kv.Value < now - 5f)
                purgeBuffer.Add(kv.Key);

        foreach (var key in purgeBuffer)
            nextTickTime.Remove(key);
    }

    void OnDrawGizmosSelected()
    {
        if (innerRadius <= 0f) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, innerRadius * WorldScale());
    }
}