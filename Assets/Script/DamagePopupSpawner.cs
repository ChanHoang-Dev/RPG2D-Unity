using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("Prefab có component DamagePopup + TextMeshPro (world space)")]
    [SerializeField] private DamagePopup popupPrefab;

    [Header("Spawn Position")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float randomOffsetRange = 0.3f; // tránh số đè lên nhau khi trúng liên tục

    private PlayerHealth playerHealth;
    private Health enemyHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        enemyHealth = GetComponent<Health>();
    }

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.OnDamageTaken += SpawnPopup;
        if (enemyHealth != null) enemyHealth.OnDamageTaken += SpawnPopup;
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnDamageTaken -= SpawnPopup;
        if (enemyHealth != null) enemyHealth.OnDamageTaken -= SpawnPopup;
    }

    private void SpawnPopup(DamageResult result)
    {
        if (popupPrefab == null) return;

        Vector3 randomOffset = new Vector3(Random.Range(-randomOffsetRange, randomOffsetRange), 0f, 0f);
        Vector3 spawnPos = transform.position + spawnOffset + randomOffset;

        DamagePopup popup = Instantiate(popupPrefab, spawnPos, Quaternion.identity);
        popup.Setup(result);
    }
}