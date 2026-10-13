using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Combat/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("HP")]
    public int hpMin = 2000;
    public int hpMax = 20000;

    [Header("Attack")]
    public int atkMin = 100;
    public int atkMax = 1000;

    [Header("Defense")]
    public int defMin = 100;
    public int defMax = 500;

    [Header("Crit")]
    [Range(0f, 1f)] public float critChanceMin = 0.05f;
    [Range(0f, 1f)] public float critChanceMax = 0.35f;
    public float critMultiplierMin = 1.3f;
    public float critMultiplierMax = 2.0f;

    [Header("Damage Reduction")]
    [Range(0f, 1f)] public float dmgReduceMin = 0f;
    [Range(0f, 1f)] public float dmgReduceMax = 0.3f;
    
    [Header("Armor Penetration")] 
    [Tooltip("% giáp của đối thủ bị vô hiệu hoá trước khi tính công thức Armor")]
    [Range(0f, 1f)] public float armorPenMin = 0f;
    [Range(0f, 1f)] public float armorPenMax = 0.4f;

    [Header("True Damage")] 
    [Tooltip("Tỉ lệ xuất hiện đòn sát thương chuẩn — bỏ qua Armor/Giảm ST, không thể crit")]
    [Range(0f, 1f)] public float trueDamageChanceMin = 0f;
    [Range(0f, 1f)] public float trueDamageChanceMax = 0.15f;
    [Header("Attack Speed")]
    [Tooltip("Attack speed multiplier (1 = base speed). Level 1 uses Min, level 50 uses Max.")]
    public float attackSpeedMin = 1.0f;
    public float attackSpeedMax = 1.5f;

    [Header("Damage Roll")]
    [Tooltip("Random % dao động quanh dame gốc, vd 0.1 = ±10%")]
    public float damageRollPercentMin = 0.1f;
    public float damageRollPercentMax = 0.2f;
}