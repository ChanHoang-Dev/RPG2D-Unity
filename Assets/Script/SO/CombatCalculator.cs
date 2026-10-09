using UnityEngine;

public struct DamageResult
{
    public int amount;     
    public bool isCrit;
    public bool isMiss;    
    public bool isTrueDamage ;
}

public static class CombatCalculator
{
    private const float MISS_CHANCE = 0.3f;

    public static DamageResult CalculateDamage(ICombatStats attacker, ICombatStats defender)
    {
        // Bước 0) Roll Miss trước tiên, độc lập với mọi chỉ số khác
        if (Random.value < MISS_CHANCE)
        {
            return new DamageResult { amount = 0, isCrit = false, isMiss = true };
        }
        bool isTrueDamage = Random.value < attacker.TrueDamageChance;
        // Bước 1) Damage ngẫu nhiên: random % quanh Attack gốc
        float rollPercent = Random.Range(attacker.DamageRollPercentMin, attacker.DamageRollPercentMax);
        float rollSign = Random.value < 0.5f ? -1f : 1f;
        float randomDamage = attacker.Attack + attacker.Attack * rollPercent * rollSign;
        if (isTrueDamage)
        {
            int trueDamageAmount = Mathf.Max(1, Mathf.RoundToInt(randomDamage));//
            return new DamageResult { amount = trueDamageAmount, isCrit = false, isMiss = false, isTrueDamage = true };
        }

        // Bước 2) Roll Critical
        bool isCrit = Random.value < attacker.CritChance;

        // Bước 3) Crit Damage = randomDamage × CritMultiplier (nếu crit)
        float critDamage = isCrit ? randomDamage * attacker.CritMultiplier : randomDamage;

        // Bước 4) ArmorDamage = critDamage² / (critDamage + Armor)
        float effectiveArmor = defender.Defense * (1f - attacker.ArmorPenetration);
        float armorDamage = (critDamage * critDamage) / (critDamage + effectiveArmor);

        // Bước 5) Damage Reduction % (áp cuối cùng)
        float finalDamage = armorDamage * (1f - defender.DamageReduction);

        int finalAmount = Mathf.Max(1, Mathf.RoundToInt(finalDamage));

        return new DamageResult { amount = finalAmount, isCrit = isCrit, isMiss = false, isTrueDamage = false };
    }
}