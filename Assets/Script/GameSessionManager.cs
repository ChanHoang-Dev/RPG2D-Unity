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
        string lastSelectedCharacterId = CharacterSaveManager.HasSaveFile()
                        ? CharacterSaveManager.Load(allCharacters)
                        : null;
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
    // Ghi nhận kinh nghiệm sau mỗi stage
    public void AwardStageExperience(int expEarned)
    {
        CharacterProgression.DistributeStageExperience(allCharacters, SelectedCharacter, expEarned);
        SaveGame();
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