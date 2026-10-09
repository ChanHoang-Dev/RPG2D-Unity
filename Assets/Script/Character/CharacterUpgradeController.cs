using UnityEngine;
using System.Collections;
using System;

public class CharacterUpgradeController : MonoBehaviour
{
    [Header("UI hiển thị")]
    public StatsPanelUI statsPanelUI;
    public UpgradePreviewUi upgradePreviewUI;
    [Header("Hiệu ứng mở panel")]
    public RectTransform panelRectTransform;
    public float slideDuration = 1f;
    public AnimationCurve slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CharacterData selectedCharacter;
    private StatType? previewStatType;
    private Coroutine slideCoroutine;
    private float fullWidth;
    private float ShownOffSetMax;

    public static event System.Action<CharacterData> OnCharacterLevelChanged;
    public void Awake()
    {
        if (panelRectTransform == null) return;

        fullWidth = panelRectTransform.rect.width;
        ShownOffSetMax = panelRectTransform.offsetMax.x;
        Debug.Log($"[CharacterUpgrade] AWAKE -> rect.width={panelRectTransform.rect.width}, offsetMax.x={panelRectTransform.offsetMax.x}, offsetMin.x={panelRectTransform.offsetMin.x}");

        SetOffSetMaxX(ShownOffSetMax - fullWidth);
        gameObject.SetActive(false);
    }

    public void SelectCharacter(CharacterData data)
    {
        selectedCharacter = data;

        if (selectedCharacter != null && selectedCharacter.statsSO == null)
        {
            Debug.LogWarning($"[CharacterUpgrade] Nhân vật '{selectedCharacter.characterName}' chưa gán StatsSO -> không thể tính stat.");
        }
        previewStatType = null;
        if (upgradePreviewUI != null)
        {
            upgradePreviewUI.gameObject.SetActive(false);
        }
        SyncCharacterLevel();

        RefreshUI();
        ShowPanel();
    }
    public void ShowPanel()
    {
        gameObject.SetActive(true);
        Debug.Log($"[CharacterUpgrade] SHOW -> fullWidth={fullWidth}, current offsetMax.x={panelRectTransform.offsetMax.x}, target={ShownOffSetMax}");
        StopSlideIfRunning();
        SetOffSetMaxX(ShownOffSetMax - fullWidth);
        slideCoroutine = StartCoroutine(SlideOffSetMax(panelRectTransform.offsetMax.x, ShownOffSetMax));
    }
    public void HidePanel()
    {
        if (!gameObject.activeSelf) return;
        StopSlideIfRunning();
        slideCoroutine = StartCoroutine(SlideOffSetMax(panelRectTransform.offsetMax.x, ShownOffSetMax - fullWidth,
                                                    onComplete: () =>
                                                    {
                                                        gameObject.SetActive(false);
                                                    }));
    }
    private void StopSlideIfRunning()
    {
        if (slideCoroutine == null) return;

        try
        {
            StopCoroutine(slideCoroutine);
        }
        catch (System.Exception)
        {
            // Nếu Coroutine đã kết thúc hoặc bị dừng, StopCoroutine sẽ ném lỗi. Bắt lỗi để tránh crash.
        }
        finally
        {
            slideCoroutine = null;
        }
    }

    private void StopCoroutine(object slideRoutine)
    {
        throw new NotImplementedException();
    }

    private IEnumerator SlideOffSetMax(float from, float to, System.Action onComplete = null)
    {
        float elapsed = 0f;
        SetOffSetMaxX(from);
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;// sử dụng unscaledDeltaTime để không bị ảnh hưởng bởi Time.timeScale (ví dụ khi pause)
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float curveValue = slideEase.Evaluate(t);
            SetOffSetMaxX(Mathf.LerpUnclamped(from, to, curveValue));
            yield return null;
        }
        SetOffSetMaxX(to);
        slideCoroutine = null;
        onComplete?.Invoke();
    }
    private void SetOffSetMaxX(float x)
    {
        Vector2 offsetMax = panelRectTransform.offsetMax;
        offsetMax.x = x;
        panelRectTransform.offsetMax = offsetMax;
    }

    // Gọi từ OnClick của các nút xanh lá. Chỉ hiện preview, chưa nâng cấp.
    public void SelectStatForPreview(StatType type)
    {
        if (selectedCharacter == null || selectedCharacter.statsSO == null) return;

        previewStatType = type;
        RefreshUpgradePreview();
    }

    public void ConfirmUpgrade()
    {
        if (selectedCharacter == null || previewStatType == null)
        {
            Debug.LogWarning("[CharacterUpgrade] Chưa chọn stat nào để nâng cấp.");
            return;
        }

        if (!selectedCharacter.isUnlocked)
        {
            Debug.LogWarning($"[CharacterUpgrade] Nhân vật '{selectedCharacter.characterName}' chưa mở khóa, không thể nâng cấp.");
            return;
        }

        if (selectedCharacter.statsSO == null)
        {
            Debug.LogError($"[CharacterUpgrade] Nhân vật '{selectedCharacter.characterName}' chưa gán StatsSO -> không thể nâng cấp.");
            return;
        }

        StatType upgradeType = previewStatType.Value;
        // TODO: trừ vàng/tài nguyên nâng cấp tại đây trước khi gọi UpgradeStat, nếu có hệ thống currency

        selectedCharacter.playerStatsRuntime.UpgradeStat(upgradeType); // sửa: dùng upgradeType, không dùng tham số type nữa
        GameSessionManager.Instance.SaveGame();
        SyncCharacterLevel();
        RefreshUI();
        RefreshUpgradePreview();
    }
    public void SyncCharacterLevel()
    {
        if (selectedCharacter == null || selectedCharacter.statsSO == null) return;
        int oldLevel = selectedCharacter.level;
        selectedCharacter.level = selectedCharacter.playerStatsRuntime.GetTotalLevelCharacter();
        if (selectedCharacter.level != oldLevel)
        {
            OnCharacterLevelChanged?.Invoke(selectedCharacter);
        }
    }
    #region [Dev/Test]
    public void ResetAllStat()
    {
        if (selectedCharacter == null || selectedCharacter.statsSO == null) return;

        selectedCharacter.playerStatsRuntime.ResetAllStats();
        SyncCharacterLevel();
        GameSessionManager.Instance.SaveGame();
        RefreshUI();
        RefreshUpgradePreview();
    }
    #endregion

    private void RefreshUI()
    {
        if (statsPanelUI == null || selectedCharacter == null || selectedCharacter.statsSO == null) return;
        statsPanelUI.DisplayStats(selectedCharacter, selectedCharacter.statsSO);
    }

    private void RefreshUpgradePreview()
    {
        if (upgradePreviewUI == null || selectedCharacter == null
        || selectedCharacter.statsSO == null || previewStatType == null) return;

        var stats = selectedCharacter.playerStatsRuntime;
        var so = selectedCharacter.statsSO;
        int maxLevel = PlayerStatsRuntime.UPGRADE_MAX_LEVEL;

        StatType type = previewStatType.Value;
        string statName = GetStatDisplayName(type);
        int currentLevel = GetLevel(stats, type);
        int nextLevel = Mathf.Min(currentLevel + 1, maxLevel);

        string currentValue = FormalValue(stats, so, type, currentLevel);
        string nextValue = currentLevel >= maxLevel ? "" : FormalValue(stats, so, type, nextLevel); // sửa: so sánh với maxLevel, không phải nextLevel

        // sửa: thiếu dòng gọi ShowPreview -> panel không bao giờ được cập nhật/hiện lên
        upgradePreviewUI.ShowUpgradePreview(statName, currentLevel, maxLevel, currentValue, nextValue);
    }

    private static string GetStatDisplayName(StatType type)
    {
        return type switch
        {
            StatType.HP => "HP",
            StatType.Attack => "Attack",
            StatType.Defense => "Defense",
            StatType.Crit => "Critical Hit",
            StatType.ArmorPenetration => "Armor Penetration",
            StatType.TrueDamage => "True Damage",
            StatType.DamageReduction => "Damage Reduction",
            _ => type.ToString()
        };
    }
    private static int GetLevel(PlayerStatsRuntime stats, StatType type)
    {
        return type switch
        {
            StatType.HP => stats.hpLevel,
            StatType.Attack => stats.atkLevel,
            StatType.Defense => stats.defLevel,
            StatType.Crit => stats.critLevel,
            StatType.ArmorPenetration => stats.armorPenLevel,
            StatType.TrueDamage => stats.trueDamageLevel,
            StatType.DamageReduction => stats.dmgReduceLevel,
            _ => 0
        };
    }
    private static string FormalValue(PlayerStatsRuntime stats, PlayerStatsSO so, StatType type, int level)
    {
        return type switch
        {
            StatType.HP => stats.GetMaxHealth(so, level).ToString(),
            StatType.Attack => stats.GetAttack(so, level).ToString(),
            StatType.Defense => stats.GetDefense(so, level).ToString(),
            StatType.Crit => $"{stats.GetCritChance(so, level) * 100:F1}%",
            StatType.ArmorPenetration => $"{stats.GetArmorPen(so, level) * 100:F1}%",
            StatType.TrueDamage => $"{stats.GetTrueDamageChance(so, level) * 100:F1}%",
            StatType.DamageReduction => $"{stats.GetDamageReduction(so, level) * 100:F1}%",
            _ => ""
        };
    }
}