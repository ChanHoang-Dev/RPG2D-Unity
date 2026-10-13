using System;
using UnityEngine;

/// <summary>
/// Runtime progression of one character. Level is DERIVED from totalExp (single source of truth).
/// Every stat = Lerp(min, max, (level-1)/(MaxLevel-1)) + milestone bonus.
/// Level 1 -> SO min values, Level 50 -> SO max values.
/// </summary>
[Serializable]
public class PlayerStatsRuntime
{
    [SerializeField] private int totalExp;

    // Needed so milestone buffs can differ per character. Set by CharacterData.OnEnable / OnValidate.
    [NonSerialized] private CharacterData owner;

    public void Bind(CharacterData characterOwner) => owner = characterOwner;

    // ---- Level / EXP ----
    public int TotalExp => totalExp;
    public int Level => CharacterProgression.LevelFromTotalExp(totalExp);
    public bool IsMaxLevel => Level >= CharacterProgression.MaxLevel;
    public int ExpIntoCurrentLevel => IsMaxLevel ? 0 : totalExp - CharacterProgression.TotalExpForLevel(Level);
    public int ExpNeededForNextLevel => IsMaxLevel ? 0 : CharacterProgression.ExpToNextLevel(Level);
    public float LevelProgress01 => IsMaxLevel ? 1f : (float)ExpIntoCurrentLevel / Mathf.Max(1, ExpNeededForNextLevel);

    public void SetTotalExp(int value)
    {
        totalExp = Mathf.Clamp(value, 0, CharacterProgression.MaxTotalExp);
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        long sum = (long)totalExp + amount;
        totalExp = (int)Math.Min(sum, CharacterProgression.MaxTotalExp);
    }

    public void ResetProgress() => totalExp = 0;

    // ---- Helpers ----
    private static float T(int level) => Mathf.InverseLerp(1, CharacterProgression.MaxLevel, level);
    private StatBonus Bonus(int level) => CharacterProgression.GetTotalMilestoneBonus(owner, level);

    // ---- Stats (current level) ----
    public int GetMaxHealth(PlayerStatsSO so) => GetMaxHealth(so, Level);
    public int GetAttack(PlayerStatsSO so) => GetAttack(so, Level);
    public int GetDefense(PlayerStatsSO so) => GetDefense(so, Level);
    public float GetCritChance(PlayerStatsSO so) => GetCritChance(so, Level);
    public float GetCritMultiplier(PlayerStatsSO so) => GetCritMultiplier(so, Level);
    public float GetDamageReduction(PlayerStatsSO so) => GetDamageReduction(so, Level);
    public float GetArmorPen(PlayerStatsSO so) => GetArmorPen(so, Level);
    public float GetTrueDamageChance(PlayerStatsSO so) => GetTrueDamageChance(so, Level);
    public float GetAttackSpeed(PlayerStatsSO so) => GetAttackSpeed(so, Level);

    // ---- Stats (explicit level, handy for UI previews) ----
    public int GetMaxHealth(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.hpMin, so.hpMax, T(level));
        return Mathf.RoundToInt(v * (1f + Bonus(level).hpPercent));
    }

    public int GetAttack(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.atkMin, so.atkMax, T(level));
        return Mathf.RoundToInt(v * (1f + Bonus(level).atkPercent));
    }

    public int GetDefense(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.defMin, so.defMax, T(level));
        return Mathf.RoundToInt(v * (1f + Bonus(level).defPercent));
    }

    public float GetCritChance(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.critChanceMin, so.critChanceMax, T(level)) + Bonus(level).critChanceFlat;
        return Mathf.Clamp01(v);
    }

    public float GetCritMultiplier(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.critMultiplierMin, so.critMultiplierMax, T(level)) + Bonus(level).critMultiplierFlat;
        return Mathf.Max(1f, v);
    }

    public float GetDamageReduction(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.dmgReduceMin, so.dmgReduceMax, T(level)) + Bonus(level).dmgReduceFlat;
        return Mathf.Clamp01(v);
    }

    public float GetArmorPen(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.armorPenMin, so.armorPenMax, T(level)) + Bonus(level).armorPenFlat;
        return Mathf.Clamp01(v);
    }

    public float GetTrueDamageChance(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.trueDamageChanceMin, so.trueDamageChanceMax, T(level)) + Bonus(level).trueDamageFlat;
        return Mathf.Clamp01(v);
    }

    /// <summary>Attack speed multiplier (1 = base speed).</summary>
    public float GetAttackSpeed(PlayerStatsSO so, int level)
    {
        float v = Mathf.Lerp(so.attackSpeedMin, so.attackSpeedMax, T(level));
        return Mathf.Max(0.1f, v * (1f + Bonus(level).attackSpeedPercent));
    }
}