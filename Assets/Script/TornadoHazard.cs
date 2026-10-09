using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TornadoHazard : MonoBehaviour
{
    [Header("Attacker")]
    [Tooltip("CombatEntity của EnemyFly đã spawn ra tornado này. Set lúc Instantiate qua Init().")]
    private CombatEntity ownerCombat;

    [Header("Damage Settings")]
    [Tooltip("Hệ số nhân sát thương so với 1 đòn đánh gốc của enemy cha.")]
    [SerializeField] private float damageMultiplier = 2f;
    [Tooltip("Chỉ dùng khi ownerCombat null.")]
    [SerializeField] private int fallbackDamage = 1;
    [SerializeField] private bool damageEnabled = true;

    [Header("Target Filter")]
    [Tooltip("Layer Player. Tornado CHỈ gây damage lên Player, bỏ qua Enemy.")]
    [SerializeField] private LayerMask targetLayer;

    [Header("Movement")]
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float lifetime = 8f;

    [Header("Damage Tick")]
    [Tooltip("Khoảng thời gian giữa 2 lần gây damage khi player còn đứng trong vùng lốc.")]
    [SerializeField] private float tickInterval = 0.2f;

    private Collider2D tornadoCollider;
    private Vector2 moveDirection = Vector2.right;
    private float lastDamageTime = -999f;

    void Awake()
    {
        tornadoCollider = GetComponent<Collider2D>();
        tornadoCollider.isTrigger = true;
    }

    void Start()
    {
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    public void Init(CombatEntity owner, Vector2 direction)
    {
        ownerCombat = owner;
        moveDirection = direction.normalized;

        if (moveDirection.x < 0f)
        {
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
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
        if (!damageEnabled) return;
        if (!IsInTargetLayer(other.gameObject)) return;
        if (Time.time - lastDamageTime < tickInterval) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null) playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            lastDamageTime = Time.time;
            ApplyDamage(playerHealth, other);
        }
    }

    private void ApplyDamage(PlayerHealth playerHealth, Collider2D targetCollider)
    {
        CombatEntity targetCombat = targetCollider.GetComponentInParent<CombatEntity>();
        if (ownerCombat != null && targetCombat != null)
        {
            DamageResult result = ownerCombat.DealDamageTo(targetCombat);

            if (!result.isMiss)
            {
                result.amount = Mathf.Max(1, Mathf.RoundToInt(result.amount * damageMultiplier));
            }

            playerHealth.TakeDamage(result);
        }
        else
        {
            playerHealth.TakeDamage(Mathf.RoundToInt(fallbackDamage * damageMultiplier));
        }
    }

    private bool IsInTargetLayer(GameObject obj)
    {
        return (targetLayer.value & (1 << obj.layer)) != 0;
    }
}