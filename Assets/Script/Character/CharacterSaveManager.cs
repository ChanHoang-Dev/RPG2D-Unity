using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    public string characterId;
    public bool isUnlocked;
    public int hpLevel;
    public int atkLevel;
    public int defLevel;
    public int critLevel;
    public int armorPenLevel;
    public int trueDamageLevel;
    public int dmgReduceLevel;
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
                hpLevel = c.playerStatsRuntime.hpLevel,
                atkLevel = c.playerStatsRuntime.atkLevel,
                defLevel = c.playerStatsRuntime.defLevel,
                critLevel = c.playerStatsRuntime.critLevel,
                armorPenLevel = c.playerStatsRuntime.armorPenLevel,
                trueDamageLevel = c.playerStatsRuntime.trueDamageLevel,
                dmgReduceLevel = c.playerStatsRuntime.dmgReduceLevel,
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[Save] Đã lưu dữ liệu tại: {SavePath}");
    }

    /// <summary>
    /// Đọc file JSON và áp dữ liệu ngược lại vào danh sách CharacterData đang có trong game.
    /// Gọi hàm này SAU khi danh sách CharacterData (asset reference, tên, sprite...) đã được khởi tạo sẵn.
    /// </summary>
    /// <returns>characterId của nhân vật được chọn lần cuối, hoặc null nếu chưa có save.</returns>
    public static string Load(List<CharacterData> allCharacters)
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[Save] Chưa có file save -> dùng dữ liệu mặc định.");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        // Map nhanh id -> CharacterData để tra cứu O(1) thay vì lặp lồng nhau
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
            target.playerStatsRuntime.hpLevel = saved.hpLevel;
            target.playerStatsRuntime.atkLevel = saved.atkLevel;
            target.playerStatsRuntime.defLevel = saved.defLevel;
            target.playerStatsRuntime.critLevel = saved.critLevel;
            target.playerStatsRuntime.armorPenLevel = saved.armorPenLevel;
            target.playerStatsRuntime.trueDamageLevel = saved.trueDamageLevel;
            target.playerStatsRuntime.dmgReduceLevel = saved.dmgReduceLevel;

            // Đồng bộ luôn level hiển thị từ stat vừa load, tránh phải chờ SelectCharacter gọi lại
            if (target.statsSO != null)
                target.level = target.playerStatsRuntime.GetTotalLevelCharacter();
        }

        Debug.Log("[Save] Đã load dữ liệu thành công.");
        return saveData.lastSelectedCharacterId;
    }

    public static bool HasSaveFile() => File.Exists(SavePath);
}