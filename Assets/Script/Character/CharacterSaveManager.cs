using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    public string characterId;
    public bool isUnlocked;
    public int totalExp; // level is derived from this
}

[System.Serializable]
public class GameSaveData
{
    public List<CharacterSaveData> characters = new List<CharacterSaveData>();
    public string lastSelectedCharacterId;
}

public static class CharacterSaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "character_save.json");

    public static void Save(List<CharacterData> allCharacters, string lastSelectedCharacterId)
    {
        var saveData = new GameSaveData
        {
            lastSelectedCharacterId = lastSelectedCharacterId
        };

        foreach (var c in allCharacters)
        {
            if (string.IsNullOrEmpty(c.characterId))
            {
                Debug.LogWarning($"[Save] Nhân vật '{c.characterName}' chưa gán characterId -> bỏ qua khi lưu.");
                continue;
            }

            saveData.characters.Add(new CharacterSaveData
            {
                characterId = c.characterId,
                isUnlocked = c.isUnlocked,
                totalExp = c.playerStatsRuntime.TotalExp,
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[Save] Đã lưu dữ liệu tại: {SavePath}");
    }

    /// <summary>
    /// Reads the JSON file and applies it to the CharacterData list.
    /// Old saves (with hpLevel/atkLevel...) still load: unknown fields are ignored and totalExp defaults to 0 (level 1).
    /// </summary>
    /// <returns>characterId of the last selected character, or null if there is no save.</returns>
    public static string Load(List<CharacterData> allCharacters)
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[Save] Chưa có file save -> dùng dữ liệu mặc định.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        Dictionary<string, CharacterData> lookup = new Dictionary<string, CharacterData>();
        foreach (var c in allCharacters)
        {
            if (!string.IsNullOrEmpty(c.characterId))
                lookup[c.characterId] = c;
        }

        foreach (var saved in saveData.characters)
        {
            if (!lookup.TryGetValue(saved.characterId, out CharacterData target))
            {
                Debug.LogWarning($"[Save] characterId '{saved.characterId}' trong file save không khớp nhân vật nào hiện có -> bỏ qua.");
                continue;
            }

            target.isUnlocked = saved.isUnlocked;
            target.playerStatsRuntime.SetTotalExp(saved.totalExp);
            target.RefreshLevelCache();
        }

        Debug.Log("[Save] Đã load dữ liệu thành công.");
        return saveData.lastSelectedCharacterId;
    }

    public static bool HasSaveFile() => File.Exists(SavePath);
}