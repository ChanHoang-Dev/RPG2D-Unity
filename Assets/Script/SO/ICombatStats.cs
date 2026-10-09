public interface ICombatStats
{
    int Attack { get; }
    int Defense { get; }
    float CritChance { get; }
    float CritMultiplier { get; }
    float DamageReduction { get; }   
    float DamageRollPercentMin { get; }
    float DamageRollPercentMax { get; }
    float ArmorPenetration { get; }
    float TrueDamageChance { get; }
}