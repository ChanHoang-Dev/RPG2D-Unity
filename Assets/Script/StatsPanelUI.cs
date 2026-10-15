using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanelUI : MonoBehaviour
{
    [Header("Text hiển thị (kéo Text hoặc TMP_Text tương ứng)")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI critChanceText;
    public TextMeshProUGUI armorPenText;
    public TextMeshProUGUI trueDamageText;
    public TextMeshProUGUI dmgReduceText;
    public TextMeshProUGUI attackSpeedText;

    public void DisplayStats(CharacterData character, PlayerStatsSO so)
    {
        if (character == null || so == null) return;
        PlayerStatsRuntime stats = character.playerStatsRuntime;

        if (hpText != null) hpText.text = "HP: " + stats.GetMaxHealth(so).ToString();
        if (atkText != null) atkText.text = "ATK: " + stats.GetAttack(so).ToString();
        if (defText != null) defText.text = "DEF: " + stats.GetDefense(so).ToString();
        if (critChanceText != null) critChanceText.text = $"CRIT: {stats.GetCritChance(so) * 100f:0.#}%";
        if (armorPenText != null) armorPenText.text = $"ARMOR PEN: {stats.GetArmorPen(so) * 100f:0.#}%";
        if (trueDamageText != null) trueDamageText.text = $"TRUE DMG: {stats.GetTrueDamageChance(so) * 100f:0.#}%";
        if (dmgReduceText != null) dmgReduceText.text = $"DMG REDUCE: {stats.GetDamageReduction(so) * 100f:0.#}%";
        if (attackSpeedText != null) attackSpeedText.text = $"ATK SPEED: {stats.GetAttackSpeed(so):0.00}x";    
    }   
}