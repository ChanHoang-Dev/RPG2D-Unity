using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LevelSaveEntry
{
    public string levelId;
    public bool isUnlocked;
    public int starsEarned;
}

[System.Serializable]
public class LevelSaveData
{
    public List<LevelSaveEntry> levels = new List<LevelSaveEntry>();
}

public static class LevelSaveManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "level_save.json");

    public static void Save(List<LevelData> allLevels)
    {
        var saveData = new LevelSaveData();

        foreach (var lvl in allLevels)
        {
            if (string.IsNullOrEmpty(lvl.levelId))
            {
                Debug.LogWarning($"[LevelSave] Level '{lvl.levelName}' chưa gán levelId -> bỏ qua khi lưu.");
                continue;
            }

            saveData.levels.Add(new LevelSaveEntry
            {
                levelId = lvl.levelId,
                isUnlocked = lvl.isUnlocked,
                starsEarned = lvl.starsEarned
            });
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[LevelSave] Đã lưu tại: {SavePath}");
    }

    public static void Load(List<LevelData> allLevels)
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("[LevelSave] Chưa có file save -> dùng dữ liệu mặc định trên asset.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        LevelSaveData saveData = JsonUtility.FromJson<LevelSaveData>(json);

        Dictionary<string, LevelData> lookup = new Dictionary<string, LevelData>();
        foreach (var lvl in allLevels)
        {
            if (!string.IsNullOrEmpty(lvl.levelId))
                lookup[lvl.levelId] = lvl;
        }

        foreach (var saved in saveData.levels)
        {
            if (!lookup.TryGetValue(saved.levelId, out LevelData target))
            {
                Debug.LogWarning($"[LevelSave] levelId '{saved.levelId}' trong save không khớp level nào hiện có -> bỏ qua.");
                continue;
            }

            target.isUnlocked = saved.isUnlocked;
            target.starsEarned = saved.starsEarned;
        }

        Debug.Log("[LevelSave] Đã load dữ liệu thành công.");
    }

    public static bool HasSaveFile() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[LevelSave] Đã xóa file save.");
        }
    }
}