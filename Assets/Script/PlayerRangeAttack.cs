using UnityEngine;
[RequireComponent(typeof(PlayerMovement))]
public class PlayerRangeAttack : MonoBehaviour
{
    [Header("SpearThrow")]
    [SerializeField] private GameObject spearPrefab;
    [SerializeField] private Transform spearSpawnPoint;
    [SerializeField] private float spearThrowForce = 10f;

    private PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    /// <summary>Gọi qua Animation Event tại frame ném lao trong clip Attack của Huntress.</summary>
    public void SpawnSpear()
    {
        if (spearPrefab == null || spearSpawnPoint == null) return;

        GameObject spear = Instantiate(spearPrefab, spearSpawnPoint.position, Quaternion.identity);
        Rigidbody2D spearRb = spear.GetComponent<Rigidbody2D>();
        int facingDirection = movement.FacingDirection;

        if (spearRb != null)
        {
            spearRb.linearVelocity = new Vector2(facingDirection * spearThrowForce, 0f);
        }

        Vector3 spearScale = spear.transform.localScale;
        spearScale.x = Mathf.Abs(spearScale.x) * facingDirection; // đảm bảo lao quay đúng hướng bay
        spear.transform.localScale = spearScale;
    }
}