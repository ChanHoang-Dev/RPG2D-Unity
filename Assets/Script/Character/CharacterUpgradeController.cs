using UnityEngine;
using System.Collections;

/// <summary>
/// Stats panel controller. Stats no longer come from manual upgrades: they follow the character's level,
/// and the level comes from EXP (see CharacterProgression). This class only shows the panel and refreshes it.
/// </summary>
public class CharacterUpgradeController : MonoBehaviour
{
    [Header("UI hiển thị")]
    public StatsPanelUI statsPanelUI;

    [Header("Hiệu ứng mở panel")]
    public RectTransform panelRectTransform;
    public float slideDuration = 1f;
    public AnimationCurve slideEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CharacterData selectedCharacter;
    private Coroutine slideCoroutine;
    private float fullWidth;
    private float ShownOffSetMax;

    public void Awake()
    {
        if (panelRectTransform == null) return;

        fullWidth = panelRectTransform.rect.width;
        ShownOffSetMax = panelRectTransform.offsetMax.x;
        Debug.Log($"[CharacterUpgrade] AWAKE -> rect.width={panelRectTransform.rect.width}, offsetMax.x={panelRectTransform.offsetMax.x}, offsetMin.x={panelRectTransform.offsetMin.x}");

        SetOffSetMaxX(ShownOffSetMax - fullWidth);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        CharacterProgression.OnCharacterLevelChanged += OnCharacterLevelChanged;
    }

    private void OnDisable()
    {
        CharacterProgression.OnCharacterLevelChanged -= OnCharacterLevelChanged;
    }

    // Refresh the panel live if the character being shown levels up.
    private void OnCharacterLevelChanged(CharacterData data, int oldLevel, int newLevel)
    {
        if (data != null && data == selectedCharacter)
            RefreshUI();
    }

    public void SelectCharacter(CharacterData data)
    {
        selectedCharacter = data;

        if (selectedCharacter != null && selectedCharacter.statsSO == null)
        {
            Debug.LogWarning($"[CharacterUpgrade] Nhân vật '{selectedCharacter.characterName}' chưa gán StatsSO -> không thể tính stat.");
        }

        if (selectedCharacter != null)
            selectedCharacter.RefreshLevelCache();

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
            onComplete: () => { gameObject.SetActive(false); }));
    }

    private void StopSlideIfRunning()
    {
        if (slideCoroutine == null) return;

        // NOTE: the old private StopCoroutine(object) override that threw NotImplementedException was removed.
        // It hid MonoBehaviour.StopCoroutine, so the slide coroutine was never actually stopped.
        StopCoroutine(slideCoroutine);
        slideCoroutine = null;
    }

    private IEnumerator SlideOffSetMax(float from, float to, System.Action onComplete = null)
    {
        float elapsed = 0f;
        SetOffSetMaxX(from);
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled so it still works while paused
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

    private void RefreshUI()
    {
        if (statsPanelUI == null || selectedCharacter == null || selectedCharacter.statsSO == null) return;
        statsPanelUI.DisplayStats(selectedCharacter, selectedCharacter.statsSO);
    }

    #region [Dev/Test]
    // Hook these to temporary UI buttons (a Button OnClick can pass an int).
    public void Dev_AddExp(int amount)
    {
        if (selectedCharacter == null) return;
        CharacterProgression.GrantExperience(selectedCharacter, amount);
        GameSessionManager.Instance.SaveGame();
        RefreshUI();
    }

    public void Dev_ResetProgress()
    {
        if (selectedCharacter == null) return;
        selectedCharacter.playerStatsRuntime.ResetProgress();
        selectedCharacter.RefreshLevelCache();
        GameSessionManager.Instance.SaveGame();
        RefreshUI();
    }
    #endregion
}