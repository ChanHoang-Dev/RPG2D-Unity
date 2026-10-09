using System.Collections;
using UnityEngine;

public class PortalSpawn : MonoBehaviour
{
    [Header("Portal Spawn Settings")]
    [SerializeField] private GameObject[] enemyPrefab; // Reference to the enemy prefab

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 5f; // Time interval between spawns
    [SerializeField] private float spawnIntervalDecrease = 0.5f; // Amount to decrease the spawn interval after each spawn
    [SerializeField] private float minSpawnInterval = 1f; // Minimum spawn interval

    [Header("Limits")]
    [SerializeField] private int maxEnemies = 2; // Maximum number of enemies allowed in the scene
    private int currentEnemyCount = 0; // Current number of enemies in the scene
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(SpawnCoroutine());
    }

    // Update is called once per frame
    void Update()
    {

    }
    private IEnumerator SpawnCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
                // Decrease the spawn interval but not below the minimum
                spawnInterval = Mathf.Max(spawnInterval - spawnIntervalDecrease, minSpawnInterval);
            }
        }
    }
    private void SpawnEnemy()
    {
        // Randomly select an enemy prefab to spawn
        int randomIndex = Random.Range(0, enemyPrefab.Length);
        GameObject enemyToSpawn = enemyPrefab[randomIndex];

        // Instantiate the enemy at the portal's position and rotation
        GameObject enemy = Instantiate(enemyToSpawn, gameObject.transform.position, Quaternion.identity);
        currentEnemyCount++; // Increment the current enemy count

        EnemyDeathNotifier deathNotifier = enemy.GetComponent<EnemyDeathNotifier>();
        if (deathNotifier != null)
        {
            deathNotifier.OnEnemyDestroyed += () =>
            {
                currentEnemyCount--; // Decrement the current enemy count when the enemy is destroyed
            };
        }
    }
}
