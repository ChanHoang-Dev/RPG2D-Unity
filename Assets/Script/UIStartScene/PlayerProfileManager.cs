using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerProfileData
{
    public int selectedAvatarIndex = -1; 
}

public static class PlayerProfileManager
{
    private static readonly string SavePath = Path.Combine(Application.persistentDataPath, "player_profile.json");

    private static PlayerProfileData cachedData;

    public static void SaveAvatarIndex(int avatarIndex)
    {
        var data = GetData();
        data.selectedAvatarIndex = avatarIndex;
        WriteData(data);
    }

    public static int LoadAvatarIndex()
    {
        return GetData().selectedAvatarIndex;
    }

    private static PlayerProfileData GetData()
    {
        if (cachedData != null) return cachedData;

        if (!File.Exists(SavePath))
        {
            cachedData = new PlayerProfileData();
            return cachedData;
        }

        string json = File.ReadAllText(SavePath);
        cachedData = JsonUtility.FromJson<PlayerProfileData>(json);
        return cachedData;
    }

    private static void WriteData(PlayerProfileData data)
    {
        cachedData = data;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[PlayerProfile] Đã lưu profile tại: {SavePath}");
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            cachedData = null;
            Debug.Log("[PlayerProfile] Đã xóa file profile.");
        }
    }

    public static bool HasSaveFile() => File.Exists(SavePath);
}