using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bonus granted by a level milestone. Everything at 0 = no effect.
/// Percent fields: 0.1 = +10%. Flat fields are added directly to the stat.
/// </summary>
[Serializable]
public struct StatBonus
{
    public float hpPercent;
    public float atkPercent;
    public float defPercent;
    public float attackSpeedPercent;

    public float critChanceFlat;
    public float critMultiplierFlat;
    public float dmgReduceFlat;
    public float armorPenFlat;
    public float trueDamageFlat;

    public static StatBonus operator +(StatBonus a, StatBonus b)
    {
        return new StatBonus
        {
            hpPercent = a.hpPercent + b.hpPercent,
            atkPercent = a.atkPercent + b.atkPercent,
            defPercent = a.defPercent + b.defPercent,
            attackSpeedPercent = a.attackSpeedPercent + b.attackSpeedPercent,
            critChanceFlat = a.critChanceFlat + b.critChanceFlat,
            critMultiplierFlat = a.critMultiplierFlat + b.critMultiplierFlat,
            dmgReduceFlat = a.dmgReduceFlat + b.dmgReduceFlat,
            armorPenFlat = a.armorPenFlat + b.armorPenFlat,
            trueDamageFlat = a.trueDamageFlat + b.trueDamageFlat,
        };
    }
}

/// <summary>
/// All level / experience rules live here (pure logic, no MonoBehaviour).
/// </summary>
public static class CharacterProgression
{
    public const int MaxLevel = 50;

    /// <summary>Locked or non-selected characters get this fraction of the EXP the active character earns.</summary>
    public const float InactiveExpRatio = 0.4f;

    // ---- EXP curve (placeholder values, tune freely) ----
    // EXP needed to go from level L to L+1 = BaseExp * L ^ ExpExponent
    private const float BaseExp = 100f;
    private const float ExpExponent = 1.2f;

    /// <summary>Levels at which a buff is granted: 5, 10, 15 ... 50.</summary>
    public static readonly int[] MilestoneLevels = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };

    // cumulativeExp[L] = total EXP required to reach level L (cumulativeExp[1] = 0)
    private static readonly int[] cumulativeExp = BuildCumulativeTable();

    public static int MaxTotalExp => cumulativeExp[MaxLevel];

    /// <summary>(character, oldLevel, newLevel)</summary>
    public static event Action<CharacterData, int, int> OnCharacterLevelChanged;

    /// <summary>(character, milestoneLevel) - fired once per milestone crossed. Use for popups / VFX / unlocking skills.</summary>
    public static event Action<CharacterData, int> OnMilestoneReached;

    // ------------------------------------------------------------------
    // EXP curve helpers
    // ------------------------------------------------------------------

    /// <summary>EXP needed to advance from <paramref name="level"/> to level + 1. Returns 0 at max level.</summary>
    public static int ExpToNextLevel(int level)
    {
        if (level >= MaxLevel) return 0;
        level = Mathf.Max(1, level);
        return Mathf.RoundToInt(BaseExp * Mathf.Pow(level, ExpExponent));
    }

    public static int TotalExpForLevel(int level)
    {
        return cumulativeExp[Mathf.Clamp(level, 1, MaxLevel)];
    }

    public static int LevelFromTotalExp(int totalExp)
    {
        for (int l = MaxLevel; l >= 1; l--)
        {
            if (totalExp >= cumulativeExp[l]) return l;
        }
        return 1;
    }

    private static int[] BuildCumulativeTable()
    {
        int[] table = new int[MaxLevel + 1];
        table[0] = 0;
        if (MaxLevel >= 1) table[1] = 0;
        for (int l = 2; l <= MaxLevel; l++)
        {
            table[l] = table[l - 1] + ExpToNextLevel(l - 1);
        }
        return table;
    }

    // ------------------------------------------------------------------
    // Granting EXP
    // ------------------------------------------------------------------

    /// <summary>Adds EXP to one character and fires level / milestone events.</summary>
    public static void GrantExperience(CharacterData character, int amount)
    {
        if (character == null || amount <= 0) return;

        PlayerStatsRuntime rt = character.playerStatsRuntime;
        int oldLevel = rt.Level;

        rt.AddExperience(amount);

        int newLevel = rt.Level;
        character.RefreshLevelCache();

        if (newLevel == oldLevel) return;

        OnCharacterLevelChanged?.Invoke(character, oldLevel, newLevel);

        foreach (int milestone in MilestoneLevels)
        {
            if (milestone > oldLevel && milestone <= newLevel)
            {
                HandleMilestoneReached(character, milestone);
            }
        }
    }

    /// <summary>
    /// Call once when a stage ends. The active character gets the full amount,
    /// every other character (locked or not selected) gets InactiveExpRatio (40%).
    /// </summary>
    public static void DistributeStageExperience(IList<CharacterData> allCharacters, CharacterData activeCharacter, int expEarned)
    {
        if (allCharacters == null || expEarned <= 0) return;

        int inactiveAmount = Mathf.RoundToInt(expEarned * InactiveExpRatio);

        foreach (CharacterData c in allCharacters)
        {
            if (c == null) continue;

            bool isActive = activeCharacter != null
                && (c == activeCharacter
                    || (!string.IsNullOrEmpty(c.characterId) && c.characterId == activeCharacter.characterId));

            GrantExperience(c, isActive ? expEarned : inactiveAmount);
        }
    }

    // ------------------------------------------------------------------
    // Milestone buffs (structure only - fill in later)
    // ------------------------------------------------------------------

    /// <summary>Sum of all milestone buffs a character has earned at <paramref name="level"/>.</summary>
    public static StatBonus GetTotalMilestoneBonus(CharacterData character, int level)
    {
        StatBonus total = default;
        foreach (int milestone in MilestoneLevels)
        {
            if (milestone > level) break;
            total = total + GetMilestoneBuff(character, milestone);
        }
        return total;
    }

    /// <summary>
    /// The stat buff for ONE milestone. Stateless: it is re-evaluated from the character's level,
    /// so it needs no extra save data. Branch on character.characterId if buffs differ per character.
    /// </summary>
    public static StatBonus GetMilestoneBuff(CharacterData character, int milestoneLevel)
    {
        // TODO: define the real buffs.
        switch (milestoneLevel)
        {
            case 5:  return default; // TODO
            case 10: return default; // TODO
            case 15: return default; // TODO
            case 20: return default; // TODO
            case 25: return default; // TODO
            case 30: return default; // TODO
            case 35: return default; // TODO
            case 40: return default; // TODO
            case 45: return default; // TODO
            case 50: return default; // TODO
            default: return default;
        }
    }

    /// <summary>
    /// Hook for NON-stat rewards at a milestone (new skill, skin, popup...). Runs once, right when the level is crossed.
    /// </summary>
    private static void HandleMilestoneReached(CharacterData character, int milestoneLevel)
    {
        // TODO: special rewards (not stat-based) go here.
        OnMilestoneReached?.Invoke(character, milestoneLevel);
    }
}