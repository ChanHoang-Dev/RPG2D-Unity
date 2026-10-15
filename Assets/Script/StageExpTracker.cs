using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gom EXP của MỘT màn chơi: mỗi quái chết cộng EXP (theo loại quái + level quái).
/// Khi màn chơi kết thúc, Commit() nhân tổng với hệ số độ khó (Easy x1, Medium x1.5, Hard x2)
/// rồi chia cho các nhân vật qua GameSessionManager.AwardStageExperience (100% nhân vật đang chọn, 40% còn lại).
/// </summary>
public static class StageExpTracker
{
    public static int BaseExp { get; private set; }
    public static int KillCount { get; private set; }

    private static bool committed;

    /// <summary>(baseExp, killCount) - dùng cho HUD nếu muốn hiện EXP trong lúc chơi.</summary>
    public static event Action<int, int> OnExpChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        Reset();
        // Mỗi lần load scene mới là một phiên mới -> xóa EXP cũ
        SceneManager.sceneLoaded += (scene, mode) => Reset();
    }

    public static void Reset()
    {
        BaseExp = 0;
        KillCount = 0;
        committed = false;
    }

    public static bool RegisterKill(int exp)
    {
        if (committed) return false; // người chơi đã thua / màn đã kết thúc -> không tính nữa
        KillCount++;
        BaseExp += Mathf.Max(0, exp);
        OnExpChanged?.Invoke(BaseExp, KillCount);
        return true;
    }
    public static int GetDisplayExp(int baseExp)
    {
        return Mathf.RoundToInt(baseExp * DifficultyScaling.Current().exp);
    }
    /// <summary>Tổng EXP sau khi nhân hệ số độ khó.</summary>
    public static int GetFinalExp()
    {
        return Mathf.RoundToInt(BaseExp * DifficultyScaling.Current().exp);
    }

    /// <summary>Chốt EXP, chỉ chạy 1 lần mỗi màn. Trả về số EXP của nhân vật đang chọn.</summary>
    public static int Commit()
    {
        if (committed) return 0;
        committed = true;

        int finalExp = GetFinalExp();
        Debug.Log($"[StageExp] Kết thúc màn: {KillCount} quái, EXP gốc={BaseExp}, hệ số độ khó=x{DifficultyScaling.Current().exp} -> nhận {finalExp} EXP.");

        if (finalExp > 0 && GameSessionManager.Instance != null)
            GameSessionManager.Instance.AwardStageExperience(finalExp);

        return finalExp;
    }
}