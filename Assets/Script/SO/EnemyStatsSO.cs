using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Combat/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    [Header("Enemy Stats (cố định)")]
    public int maxHealth = 5000;
    public int attack = 100;
    public int defense = 100;
    [Range(0f, 1f)] public float critChance = 0.1f;
    public float critMultiplier = 1.5f;

    [Header("Damage Roll")]
    public float damageRollPercentMin = 0.1f;
    public float damageRollPercentMax = 0.2f;
}