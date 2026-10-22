using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [SerializeField] private List<CharacterData> allCharacters = new List<CharacterData>();

    public IReadOnlyList<CharacterData> AllCharacters => allCharacters;

    public CharacterData SelectedCharacter { get; private set; }

    private Transform registeredCharacterListContent;

    public static event Action<CharacterData> OnCharacterChanged;
    public static event Action<CharacterData> OnCharacterUnlocked;

    // Có nhân vật nào lên level trong lần cộng EXP vừa rồi không -> lên level thì lưu luôn
    private bool levelChangedFlag;

    private void OnEnable()
    {
        CharacterProgression.OnCharacterLevelChanged += MarkLevelChanged;
    }

    private void OnDisable()
    {
        CharacterProgression.OnCharacterLevelChanged -= MarkLevelChanged;
    }

    private void MarkLevelChanged(CharacterData data, int oldLevel, int newLevel)
    {
        levelChangedFlag = true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (allCharacters == null || allCharacters.Count == 0)
        {
            Debug.LogError("[GameSession] allCharacters đang rỗng! Kéo các CharacterData asset vào Inspector của GameSessionManager.");
            return;
        }
        RestoreSelectionFromSave();
    }

    private void RestoreSelectionFromSave()
    {
        string lastSelectedCharacterId = null;
        if (CharacterSaveManager.HasSaveFile())
        {
            lastSelectedCharacterId = CharacterSaveManager.Load(allCharacters);
        }
        else
        {
            // Không có file save = chơi mới. CharacterData là ScriptableObject nên trong Editor nó GIỮ LẠI EXP
            // của lần Play trước (không tự về 0) -> phải reset tay, nếu không xóa file save vẫn còn level cũ.
            ResetAllProgress();
        }
        CharacterData target = null;
        if (!string.IsNullOrEmpty(lastSelectedCharacterId))
        {
            target = GetCharacterById(lastSelectedCharacterId);
            if (target != null && !target.isUnlocked)
            {
                Debug.LogWarning($"[GameSession] Nhân vật được chọn lần cuối '{target.characterName}' chưa unlock -> bỏ qua.");
                target = null;
            }
        }
        if (target == null)
        {
            target = allCharacters.Count > 0 ? allCharacters[0] : null;
            if (target != null && !target.isUnlocked)
            {
                Debug.LogWarning($"[GameSession] Nhân vật đầu tiên '{target.characterName}' chưa unlock -> bỏ qua.");
            }
        }
        SelectedCharacter = target;
    }
    /// <summary>Đưa EXP / level của mọi nhân vật về 0 (level 1).</summary>
    private void ResetAllProgress()
    {
        foreach (CharacterData c in allCharacters)
        {
            if (c == null || c.playerStatsRuntime == null) continue;
            c.playerStatsRuntime.ResetProgress();
            c.RefreshLevelCache();
        }
        Debug.Log("[GameSession] Không có file save -> reset EXP của mọi nhân vật về level 1.");
    }

    public void RegisterCharacterList(Transform characterListContent)
    {
        registeredCharacterListContent = characterListContent;
        RefreshAllSlots(characterListContent);
    }
    public void RequestUnlock(CharacterData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[GameSession] Không thể unlock nhân vật null.");
            return;
        }
        if (data.isUnlocked)
        {
            Debug.LogWarning($"[GameSession] Nhân vật '{data.characterName}' đã unlock rồi.");
            return;
        }
        if (!CharacterUnlockRules.CanUnlock(data))
        {
            Debug.LogWarning($"[GameSession] Nhân vật '{data.characterName}' chưa đủ điều kiện unlock.");
            return;
        }
        data.isUnlocked = true;
        SaveGame();
        OnCharacterUnlocked?.Invoke(data);
        RefreshAllSlots(registeredCharacterListContent);
    }

    public void SelectCharacter(CharacterData data)
    {
        if (data == null || !data.isUnlocked)
        {
            Debug.LogWarning("[GameSession] Không thể chọn nhân vật null hoặc chưa unlock.");
            return;
        }
        SelectedCharacter = data;
        SaveGame();
        OnCharacterChanged?.Invoke(data);
        RefreshAllSlots(registeredCharacterListContent);
    }

    public void RefreshAllSlots(Transform characterListContent)
    {
        if (characterListContent == null)
        {
            Debug.LogWarning("[GameSession] Không thể refresh slots vì characterListContent null.");
            return;
        }
        CharacterSlotUI[] slots = characterListContent.GetComponentsInChildren<CharacterSlotUI>(true);
        foreach (var slot in slots)
        {
            if (slot.characterData == null)
            {
                Debug.LogWarning($"[GameSession] Slot '{slot.gameObject.name}' có characterData = NULL -> chưa kéo asset vào field này trong Inspector.");
                continue;
            }
            bool IsSelected = SelectedCharacter != null && slot.characterData.characterId == SelectedCharacter.characterId;
            slot.RefreshState(IsSelected);
        }
    }
    // Cộng EXP ngay mỗi lần giết quái. Không ghi file mỗi lần giết: chỉ lưu khi có người lên level,
    // phần EXP lẻ còn lại được lưu lúc Player chết / thoát game (OnApplicationQuit).
    public void AwardStageExperience(int expEarned)
    {
        levelChangedFlag = false;
        CharacterProgression.DistributeStageExperience(allCharacters, SelectedCharacter, expEarned);
        if (levelChangedFlag) SaveGame();
    }
    public CharacterData GetCharacterByName(string name)
    {
        return allCharacters.FirstOrDefault(c => c.characterName == name);
    }
    public CharacterData GetCharacterById(string id)
    {
        return allCharacters.FirstOrDefault(c => c.characterId == id);
    }
    public void SaveGame()
    {
        string lastSelectedCharacterId = SelectedCharacter != null ? SelectedCharacter.characterId : null;
        CharacterSaveManager.Save(allCharacters, lastSelectedCharacterId);
    }
    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            SaveGame();
        }
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}