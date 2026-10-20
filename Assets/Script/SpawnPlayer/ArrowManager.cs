using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class ArrowManager : MonoBehaviour
{
    [Header("Fallback")]
    [Tooltip("Chỉ dùng khi không tính được từ stat (quái không có CombatEntity).")]
    [SerializeField] private int fallbackDamage = 1;

    private ICombatStats attackerStats;
    private LayerMask targetLayer;
    private LayerMask obstacleLayer;
    private bool hasHit;

    /// <param name="attacker">Stat của người bắn (chụp lúc bắn, nên vẫn đúng nếu Player chết khi mũi tên còn đang bay).</param>
    /// <param name="targetLayers">Layer của quái (cùng mask enemyLayer của PlayerRangeAttack).</param>
    /// <param name="obstacleLayers">Layer chặn mũi tên (Ground). Để 0 nếu không cần.</param>
    /// <param name="lifetime">Tự huỷ sau số giây này nếu không trúng gì.</param>
    public void Init(ICombatStats attacker, LayerMask targetLayers, LayerMask obstacleLayers, float lifetime)
    {
        attackerStats = attacker;
        targetLayer = targetLayers;
        obstacleLayer = obstacleLayers;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        int otherBit = 1 << other.gameObject.layer;

        // Chạm vật cản (đất / tường) -> mũi tên biến mất
        if ((obstacleLayer.value & otherBit) != 0)
        {
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        if ((targetLayer.value & otherBit) == 0) return;

        Health targetHealth = other.GetComponentInParent<Health>();
        if (targetHealth == null) return;

        // Xác quái chưa kịp biến mất: cho mũi tên bay xuyên qua, không "ăn" mũi tên
        if (targetHealth.IsDead()) return;

        hasHit = true;

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

        Destroy(gameObject);
    }
}