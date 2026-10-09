using UnityEngine;
using UnityEngine.UI;

public class UpgradeButtonBinder : MonoBehaviour
{
    [Header("Tham chiếu controller")]
    public CharacterUpgradeController controller;

    [Header("7 nút xanh lá, đúng thứ tự stat")]
    public Button hpButton;
    public Button atkButton;
    public Button defButton;
    public Button critButton;
    public Button armorPenButton;
    public Button trueDamageButton;
    public Button dmgReduceButton;

    [Header("Nút xác nhận nâng cấp trong panel preview")]
    public Button confirmUpgradeButton;

    [Header("Nút reset tất cả stat (chỉ dùng dev/test]")]
    public Button resetAllStatButton;

    private void Start()
    {
        if (controller == null)
        {
            Debug.LogError("[UpgradeButtonBinder] Chưa gán controller.");
            return;
        }

        BindStatButton(hpButton, StatType.HP);
        BindStatButton(atkButton, StatType.Attack);
        BindStatButton(defButton, StatType.Defense);
        BindStatButton(critButton, StatType.Crit);
        BindStatButton(armorPenButton, StatType.ArmorPenetration);
        BindStatButton(trueDamageButton, StatType.TrueDamage);
        BindStatButton(dmgReduceButton, StatType.DamageReduction);

        if (confirmUpgradeButton != null)
            confirmUpgradeButton.onClick.AddListener(controller.ConfirmUpgrade);

        if (resetAllStatButton != null)
            resetAllStatButton.onClick.AddListener(controller.ResetAllStat);
        
    }

    private void BindStatButton(Button button, StatType type)
    {
        if (button == null) return;
        // dùng biến local "type" trong closure -> mỗi nút giữ đúng StatType của nó
        button.onClick.AddListener(() => controller.SelectStatForPreview(type));
    }
}