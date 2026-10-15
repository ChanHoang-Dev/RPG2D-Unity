using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyStats", menuName = "Combat/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject
{
    public const int MaxLevel = 50;
 
    [Header("Enemy Stats (chỉ số ở LEVEL 1)")]
    public int maxHealth = 5000;
    public int attack = 100;
    public int defense = 100;
    [Range(0f, 1f)] public float critChance = 0.1f;
    public float critMultiplier = 1.5f;
    [Header("Tăng trưởng theo level")]
    [Tooltip("Mỗi level tăng thêm bao nhiêu % so với chỉ số gốc. 0.12 = +12%/level (level 50 ≈ x6.9).")]
    public float hpGrowthPerLevel = 0.12f;
    [Tooltip("0.15 = +15%/level (level 50 ≈ x8.4)")]
    public float atkGrowthPerLevel = 0.15f;
    [Tooltip("0.08 = +8%/level (level 50 ≈ x4.9)")]
    public float defGrowthPerLevel = 0.08f;
 
    [Header("EXP khi bị tiêu diệt")]
    [Tooltip("EXP cơ bản của loại quái này ở level 1. Mỗi loại quái chỉnh khác nhau (boss / quái mạnh nhập cao hơn).")]
    public int baseExp = 20;
    [Tooltip("Mỗi level của quái tăng thêm bao nhiêu % EXP. 0 = EXP không đổi theo level.")]
    public float expGrowthPerLevel = 0.1f;

    [Header("Damage Roll")]
    public float damageRollPercentMin = 0.1f;
    public float damageRollPercentMax = 0.2f;
    private static float Scale(float growthPerLevel, int level)
    {
        level = Mathf.Clamp(level, 1, MaxLevel);
        return 1f + growthPerLevel * (level - 1);
    }
 
    public int GetMaxHealth(int level) => Mathf.RoundToInt(maxHealth * Scale(hpGrowthPerLevel, level));
    public int GetAttack(int level) => Mathf.RoundToInt(attack * Scale(atkGrowthPerLevel, level));
    public int GetDefense(int level) => Mathf.RoundToInt(defense * Scale(defGrowthPerLevel, level));
    public int GetExpReward(int level) => Mathf.RoundToInt(baseExp * Scale(expGrowthPerLevel, level));
}