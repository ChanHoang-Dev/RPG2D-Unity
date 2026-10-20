
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerRangeAttack : MonoBehaviour
{
    [Header("Spear Throw")]
    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private Transform spearSpawnPoint;
    [SerializeField] private float spearThrowForce = 10f;

    [Header("Enemy Targeting")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float targetRange = 10f;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    // Tìm Enemy gần nhất ở phía trước
    private Transform FindTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            targetRange,
            enemyLayer
        );

        Transform nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        int facingDirection = movement.FacingDirection;

        foreach (Collider2D enemy in enemies)
        {
            Vector2 enemyPosition = enemy.bounds.center;

            float directionX =
                enemyPosition.x - transform.position.x;

            // Bỏ qua Enemy phía sau
            if (directionX * facingDirection <= 0f)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                enemyPosition
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        return nearestEnemy;
    }

    // Gọi bằng Animation Event
    public void SpawnSpear()
    {
        if (spearPrefab == null || spearSpawnPoint == null)
            return;

        int facingDirection = movement.FacingDirection;

        // Hướng bay mặc định
        Vector2 throwDirection = new Vector2(
            facingDirection, 0f
        );

        Transform target = FindTarget();

        // Nếu tìm thấy Enemy phía trước
        if (target != null)
        {
            Collider2D targetCollider =
                target.GetComponent<Collider2D>();

            Vector2 targetPosition = targetCollider != null
                ? (Vector2)targetCollider.bounds.center
                : (Vector2)target.position;

            throwDirection = (
                targetPosition -
                (Vector2)spearSpawnPoint.position
            ).normalized;
        }

        GameObject spear = Instantiate(
            spearPrefab,
            spearSpawnPoint.position,
            Quaternion.identity
        );

        Rigidbody2D spearRb =
            spear.GetComponent<Rigidbody2D>();

        if (spearRb != null)
        {
            spearRb.linearVelocity =
                throwDirection * spearThrowForce;
        }

        // Xoay Spear theo hướng bay
        float angle = Mathf.Atan2(
            throwDirection.y,
            throwDirection.x
        ) * Mathf.Rad2Deg;

        spear.transform.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            targetRange
        );
    }
}
