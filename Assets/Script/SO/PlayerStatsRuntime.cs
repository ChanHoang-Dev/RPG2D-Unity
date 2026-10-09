using UnityEngine;

[System.Serializable]
public class PlayerStatsRuntime
{
    // Level nâng cấp cho từng chỉ số, 0 = min, upgradeMaxLevel = max
    public int hpLevel;
    public int atkLevel;
    public int defLevel;
    public int critLevel;
    public int armorPenLevel;
    public int trueDamageLevel;
    public int dmgReduceLevel;

    public const int UPGRADE_MAX_LEVEL = 10;
    public const int STAT_COUNT = 7;
    public const int MAX_TOTAL_LEVEL_CHARACTER = UPGRADE_MAX_LEVEL * STAT_COUNT;

    private static float Lerp(float min, float max, int level)
        => Mathf.Lerp(min, max, (float)level / UPGRADE_MAX_LEVEL);
    public int GetMaxHealth(PlayerStatsSO so) => GetMaxHealth(so, hpLevel);
    public int GetAttack(PlayerStatsSO so) => GetAttack(so, atkLevel);
    public int GetDefense(PlayerStatsSO so) => GetDefense(so, defLevel);
    public float GetCritChance(PlayerStatsSO so) => GetCritChance(so, critLevel);
    public float GetCritMultiplier(PlayerStatsSO so) => GetCritMultiplier(so, critLevel);
    public float GetArmorPen(PlayerStatsSO so) => GetArmorPen(so, armorPenLevel);
    public float GetTrueDamageChance(PlayerStatsSO so) => GetTrueDamageChance(so, trueDamageLevel);
    public float GetDamageReduction(PlayerStatsSO so) => GetDamageReduction(so, dmgReduceLevel);

    public int GetMaxHealth(PlayerStatsSO so, int level) => Mathf.RoundToInt(Lerp(so.hpMin, so.hpMax, level));
    public int GetAttack(PlayerStatsSO so, int level) => Mathf.RoundToInt(Lerp(so.atkMin, so.atkMax, level));
    public int GetDefense(PlayerStatsSO so, int level) => Mathf.RoundToInt(Lerp(so.defMin, so.defMax, level));
    public float GetCritChance(PlayerStatsSO so, int level) => Lerp(so.critChanceMin, so.critChanceMax, level);
    public float GetCritMultiplier(PlayerStatsSO so, int level) => Lerp(so.critMultiplierMin, so.critMultiplierMax, level);
    public float GetArmorPen(PlayerStatsSO so, int level) => Lerp(so.armorPenMin, so.armorPenMax, level);
    public float GetTrueDamageChance(PlayerStatsSO so, int level) => Lerp(so.trueDamageChanceMin, so.trueDamageChanceMax, level);
    public float GetDamageReduction(PlayerStatsSO so, int level) => Lerp(so.dmgReduceMin, so.dmgReduceMax, level);
    public int GetTotalLevel()
    {
        return hpLevel + atkLevel + defLevel + critLevel + armorPenLevel + trueDamageLevel + dmgReduceLevel;
    }
    public int GetTotalLevelCharacter(int maxCharacterLevel = MAX_TOTAL_LEVEL_CHARACTER + 1)
    {
        int totalLevel = GetTotalLevel();
        float t = (float)totalLevel / MAX_TOTAL_LEVEL_CHARACTER;
        return Mathf.RoundToInt(t * (maxCharacterLevel - 1)) + 1;
    }
    public void UpgradeStat(StatType type)
    {
        switch (type)
        {
            case StatType.HP: hpLevel = Mathf.Min(hpLevel + 1, UPGRADE_MAX_LEVEL); break;
            case StatType.Attack: atkLevel = Mathf.Min(atkLevel + 1, UPGRADE_MAX_LEVEL); break;
            case StatType.Defense: defLevel = Mathf.Min(defLevel + 1, UPGRADE_MAX_LEVEL); break;
            case StatType.Crit: critLevel = Mathf.Min(critLevel + 1, UPGRADE_MAX_LEVEL); break;
            case StatType.ArmorPenetration: armorPenLevel = Mathf.Min(armorPenLevel + 1, UPGRADE_MAX_LEVEL); break;
            case StatType.TrueDamage: trueDamageLevel = Mathf.Min(trueDamageLevel + 1, UPGRADE_MAX_LEVEL); break;
            case StatType.DamageReduction: dmgReduceLevel = Mathf.Min(dmgReduceLevel + 1, UPGRADE_MAX_LEVEL); break;
        }
    }
    public void ResetStats(StatType type)
    {
        switch (type)
        {
            case StatType.HP: hpLevel = 0; break;
            case StatType.Attack: atkLevel = 0; break;
            case StatType.Defense: defLevel = 0; break;
            case StatType.Crit: critLevel = 0; break;
            case StatType.ArmorPenetration: armorPenLevel = 0; break;
            case StatType.TrueDamage: trueDamageLevel = 0; break;
            case StatType.DamageReduction: dmgReduceLevel = 0; break;
        }
    }
    #region [Dev/Test]
    public void ResetAllStats()
    {
        hpLevel = 0;
        atkLevel = 0;
        defLevel = 0;
        critLevel = 0;
        armorPenLevel = 0;
        trueDamageLevel = 0;
        dmgReduceLevel = 0;
    }
    #endregion
}

public enum StatType { HP, Attack, Defense, Crit, DamageReduction, ArmorPenetration, TrueDamage }