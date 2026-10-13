using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class CharacterPreviewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator previewSourceAnimator;
    [SerializeField] private SpriteRenderer previewSourceRenderer;
    [SerializeField] private Image previewImage;

    [Header("Auto-select khi vào scene")]
    public Transform characterListContent;

    [Header("Run Animation Fix")]
    [SerializeField] private string isRunningParameterName = "IsRunning";
    [SerializeField] private string isGroundedParameterName = "IsGrounded";

    private int isRunningParamHash;
    private bool hasIsRunningParam;
    private int isGroundedParamHash;
    private bool hasIsGroundedParam;
    private int currentIndex = 0;
    private Coroutine sequenceRoutine;
    private Sprite lastSyncedSprite;
    private List<string> animSequence = new List<string>();

    [Header("Information Character")]
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI characterLevelText;
    [SerializeField] private StarUI starUI;

    [Header("Action Button dùng chung (ActionSelectChar) — đại diện cho nhân vật đang Preview")]
    [SerializeField] private Button actionSelectCharButton;
    [SerializeField] private TextMeshProUGUI actionSelectCharText;
    [SerializeField] private Image actionSelectCharImage;
    [SerializeField] private Color colorLocked = Color.gray;
    [SerializeField] private Color colorSelect = new Color(0.2f, 0.5f, 1f);
    [SerializeField] private Color colorSelected = new Color(0.2f, 0.8f, 0.3f);
    [Header("Panel Confirm Unlock")]
    [SerializeField] private GameObject panelConfirmUnlock;
    [SerializeField] private Button buttonConfirmUnlock;
    [SerializeField] private Button buttonCancelUnlock;


    private CharacterData currentCharacterData;

    private void Start()
    {
        AutoSelectFirstUnlocked();
        previewImage.transform.localScale = new Vector3(5, 5, 1);
        panelConfirmUnlock.SetActive(false);

        if (actionSelectCharButton != null)
        {
            actionSelectCharButton.onClick.RemoveAllListeners();
            actionSelectCharButton.onClick.AddListener(OnActionSelectCharClicked);
        }
        if (buttonConfirmUnlock != null)
        {
            buttonConfirmUnlock.onClick.RemoveAllListeners();
            buttonConfirmUnlock.onClick.AddListener(OnConfirmUnlockClicked);
        }
        if (buttonCancelUnlock != null)
        {
            buttonCancelUnlock.onClick.RemoveAllListeners();
            buttonCancelUnlock.onClick.AddListener(OnCancelUnlockClicked);
        }
    }

    private void Update()
    {
        SyncSpriteToImage();
    }

    private void SyncSpriteToImage()
    {
        if (previewSourceRenderer == null || previewImage == null) return;

        Sprite currentSprite = previewSourceRenderer.sprite;
        if (currentSprite != lastSyncedSprite)
        {
            previewImage.sprite = currentSprite;
            previewImage.SetNativeSize();
            Vector2 pivotNormalized = new Vector2(
            currentSprite.pivot.x / currentSprite.rect.width,
            currentSprite.pivot.y / currentSprite.rect.height
        );
            previewImage.rectTransform.pivot = pivotNormalized;
            lastSyncedSprite = currentSprite;
        }
    }

    private void AutoSelectFirstUnlocked()
    {
        if (characterListContent == null)
        {
            Debug.LogWarning("[CharacterPreview] Chưa gán 'Character List Content' -> không thể tự chọn nhân vật đầu tiên.");
            return;
        }

        CharacterSlotUI[] slots = characterListContent.GetComponentsInChildren<CharacterSlotUI>(true);

        CharacterSlotUI target = null;
        foreach (var slot in slots)
        {
            if (slot.characterData != null && slot.characterData.isUnlocked)
            {
                target = slot;
                break;
            }
        }
        if (target == null && slots.Length > 0)
        {
            target = slots[0];
        }

        if (target != null)
        {
            ShowCharacter(target.characterData);
        }
    }

    public void ShowCharacter(CharacterData data)
    {
        if (previewSourceAnimator == null)
        {
            Debug.LogError("[CharacterPreview] previewSourceAnimator chưa được gán!");
            return;
        }
        if (previewSourceRenderer == null)
        {
            Debug.LogError("[CharacterPreview] previewSourceRenderer chưa được gán!");
            return;
        }
        if (previewImage == null)
        {
            Debug.LogError("[CharacterPreview] previewImage chưa được gán!");
            return;
        }
        if (data == null)
        {
            Debug.LogError("[CharacterPreview] CharacterData truyền vào đang null!");
            return;
        }
        if (data.animatorController == null)
        {
            Debug.LogWarning($"[CharacterPreview] Nhân vật '{data.characterName}' chưa gán animatorController trong Inspector.");
        }

        previewSourceAnimator.runtimeAnimatorController = data.animatorController;

        CacheRunParameters();

        previewImage.color = data.isUnlocked ? Color.white : Color.black;
        lastSyncedSprite = null;
        if (characterNameText != null) characterNameText.text = data.characterName;
        currentCharacterData = data;
        if (characterLevelText != null) characterLevelText.text = $"Level{data.level}";
        if (starUI != null) starUI.SetRating(data.powerRating);

        // Đồng bộ nút ActionSelectChar theo đúng nhân vật đang xem
        RefreshActionSelectChar(data);

        BuildAnimSequence(data);

        if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
        currentIndex = 0;
        sequenceRoutine = StartCoroutine(PlaySequence());
    }

    private void RefreshActionSelectChar(CharacterData data)
    {
        if (data == null || actionSelectCharButton == null) return;

        bool isUnlocked = data.isUnlocked;
        bool isSelected = GameSessionManager.Instance != null
            && GameSessionManager.Instance.SelectedCharacter != null
            && GameSessionManager.Instance.SelectedCharacter.characterId == data.characterId;

        if (!isUnlocked)
        {
            if (actionSelectCharText != null) actionSelectCharText.text = "Unlock";
            if (actionSelectCharImage != null) actionSelectCharImage.color = colorLocked;
            actionSelectCharButton.interactable = true;
        }
        else if (isSelected)
        {
            if (actionSelectCharText != null) actionSelectCharText.text = "Selected";
            if (actionSelectCharImage != null) actionSelectCharImage.color = colorSelected;
            actionSelectCharButton.interactable = false;
        }
        else
        {
            if (actionSelectCharText != null) actionSelectCharText.text = "Select";
            if (actionSelectCharImage != null) actionSelectCharImage.color = colorSelect;
            actionSelectCharButton.interactable = true;
        }
    }

    private void OnActionSelectCharClicked()
    {
        if (currentCharacterData == null) return;

        if (!currentCharacterData.isUnlocked)
        {
            if (CharacterUnlockRules.CanUnlock(currentCharacterData))
                panelConfirmUnlock.SetActive(true);
            else
                Debug.LogWarning($"[CharacterPreview] Nhân vật '{currentCharacterData.characterName}' chưa mở khóa và không đủ điều kiện mở khóa.");
        }
    }
    private void OnConfirmUnlockClicked()
    {
        if (currentCharacterData == null) return;

        GameSessionManager.Instance.RequestUnlock(currentCharacterData);
        previewImage.color = Color.white;
        panelConfirmUnlock.SetActive(false);
    }
    private void OnCancelUnlockClicked()
    {
        panelConfirmUnlock.SetActive(false);
    }

    private void CacheRunParameters()
    {
        hasIsRunningParam = false;
        hasIsGroundedParam = false;

        if (!string.IsNullOrEmpty(isRunningParameterName))
        {
            isRunningParamHash = Animator.StringToHash(isRunningParameterName);
        }
        if (!string.IsNullOrEmpty(isGroundedParameterName))
        {
            isGroundedParamHash = Animator.StringToHash(isGroundedParameterName);
        }

        foreach (var p in previewSourceAnimator.parameters)
        {
            if (p.type != AnimatorControllerParameterType.Bool) continue;

            if (p.name == isRunningParameterName) hasIsRunningParam = true;
            if (p.name == isGroundedParameterName) hasIsGroundedParam = true;
        }
    }

    private void BuildAnimSequence(CharacterData data)
    {
        animSequence.Clear();
        animSequence.Add("Knight_White-Idle");
        animSequence.Add("Knight_White-Run");
        animSequence.Add("Knight_White-Jump");

        for (int i = 1; i <= data.attackCount; i++)
        {
            animSequence.Add($"Knight_White-Attack{i}");
        }
        animSequence.Add("Knight_White-Hurt");
        animSequence.Add("Knight_White-Death");
    }

    private void ApplyParametersForState(string stateName)
    {
        bool isRun = stateName.Contains("Run");
        bool isJump = stateName.Contains("Jump");

        if (hasIsGroundedParam)
        {
            bool groundedValue = isJump ? false : true;
            previewSourceAnimator.SetBool(isGroundedParamHash, groundedValue);
        }

        if (hasIsRunningParam)
        {
            previewSourceAnimator.SetBool(isRunningParamHash, isRun);
        }
    }

    private IEnumerator PlaySequence()
    {
        yield return null;
        if (previewSourceAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("[CharacterPreview] runtimeAnimatorController đang null -> dừng PlaySequence.");
            yield break;
        }
        if (animSequence.Count == 0)
        {
            Debug.LogError("[CharacterPreview] animSequence rỗng -> dừng PlaySequence.");
            yield break;
        }

        int consecutiveMisses = 0;

        while (true)
        {
            string stateName = animSequence[currentIndex];

            if (!HasState(stateName))
            {
                Debug.LogWarning($"[CharacterPreview] Không tìm thấy state '{stateName}'.");
                currentIndex = (currentIndex + 1) % animSequence.Count;
                consecutiveMisses++;

                if (consecutiveMisses >= animSequence.Count)
                {
                    Debug.LogError("[CharacterPreview] Không có state nào khớp. Dừng PlaySequence.");
                    yield break;
                }

                yield return null;
                continue;
            }
            consecutiveMisses = 0;

            ApplyParametersForState(stateName);

            previewSourceAnimator.Play(stateName, 0, 0f);

            yield return null;
            AnimatorStateInfo info = previewSourceAnimator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(info.length);

            currentIndex = (currentIndex + 1) % animSequence.Count;
        }
    }

    private bool HasState(string stateName)
    {
        int hash = Animator.StringToHash(stateName);
        int layerCount = previewSourceAnimator.layerCount;
        for (int layer = 0; layer < layerCount; layer++)
        {
            if (previewSourceAnimator.HasState(layer, hash))
            {
                return true;
            }
        }
        return false;
    }

    private void OnEnable()
    {
        CharacterProgression.OnCharacterLevelChanged += UpdateCharacterLevelText;
        GameSessionManager.OnCharacterChanged += OnGameStateChanged;
        GameSessionManager.OnCharacterUnlocked += OnGameStateChanged;
    }

    private void OnGameStateChanged(CharacterData data)
    {
        RefreshActionSelectChar(currentCharacterData);
    }

    private void UpdateCharacterLevelText(CharacterData data, int oldLevel, int newLevel)
    {
        if (currentCharacterData != null && data == currentCharacterData)
        {
            if (characterLevelText != null)
            {
                characterLevelText.text = $"Level{data.level}";
            }
        }
    }

    private void OnDisable()
    {
        CharacterProgression.OnCharacterLevelChanged -= UpdateCharacterLevelText;
        GameSessionManager.OnCharacterChanged -= OnGameStateChanged;
        GameSessionManager.OnCharacterUnlocked -= OnGameStateChanged;
        if (sequenceRoutine != null) StopCoroutine(sequenceRoutine);
    }

}