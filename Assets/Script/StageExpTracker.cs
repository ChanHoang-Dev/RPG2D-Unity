using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// EXP của MỘT màn chơi: mỗi quái chết nhân NGAY hệ số độ khó (Easy x1, Medium x1.5, Hard x2)
/// rồi cộng LUÔN vào nhân vật qua GameSessionManager.AwardStageExperience
/// (100% nhân vật đang chọn, 40% còn lại) -> lên level là chỉ số tăng ngay trong màn.
/// Commit() chỉ còn khoá màn (không tính EXP nữa) và lưu file.
/// </summary>
public static class StageExpTracker
{
    /// <summary>Tổng EXP của màn, ĐÃ nhân hệ số độ khó của từng lần tiêu diệt.</summary>
    public static int TotalExp { get; private set; }
    public static int KillCount { get; private set; }

    private static bool committed;

    /// <summary>true sau khi Commit() đã chạy: EXP của màn đã được cộng vào nhân vật (HUD không cộng thêm nữa).</summary>
    public static bool IsCommitted => committed;

    /// <summary>(totalExp, killCount) - dùng cho HUD nếu muốn hiện EXP trong lúc chơi.</summary>
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
        TotalExp = 0;
        KillCount = 0;
        committed = false;
    }

    /// <summary>
    /// Ghi nhận một lần tiêu diệt. baseExp là EXP gốc của quái; gainedExp trả về EXP thực nhận
    /// sau khi nhân hệ số độ khó (ví dụ Medium: 20 -> 30). Trả về false nếu màn đã kết thúc.
    /// </summary>
    public static bool RegisterKill(int baseExp, out int gainedExp)
    {
        gainedExp = 0;
        if (committed) return false; // người chơi đã thua / màn đã kết thúc -> không tính nữa

        gainedExp = ApplyDifficulty(baseExp);
        KillCount++;
        TotalExp += gainedExp;

        // Cộng EXP vào nhân vật NGAY (lên level -> chỉ số tăng luôn trong màn)
        if (gainedExp > 0 && GameSessionManager.Instance != null)
            GameSessionManager.Instance.AwardStageExperience(gainedExp);

        OnExpChanged?.Invoke(TotalExp, KillCount);
        return true;
    }

    /// <summary>EXP gốc -> EXP sau hệ số độ khó hiện tại (làm tròn nửa lên, nên 3 x1.5 = 5).</summary>
    public static int ApplyDifficulty(int baseExp)
    {
        float scaled = Mathf.Max(0, baseExp) * DifficultyScaling.Current().exp;
        return Mathf.FloorToInt(scaled + 0.5f);
    }

    /// <summary>Tổng EXP của màn (đã nhân hệ số độ khó từng lần giết).</summary>
    public static int GetFinalExp()
    {
        return TotalExp;
    }

    /// <summary>
    /// Kết thúc màn, chỉ chạy 1 lần: khoá không tính EXP nữa và lưu file
    /// (EXP đã được cộng vào nhân vật từng lần giết rồi). Trả về tổng EXP kiếm được trong màn.
    /// </summary>
    public static int Commit()
    {
        if (committed) return 0;
        committed = true;

        int finalExp = GetFinalExp();
        Debug.Log($"[StageExp] Kết thúc màn: {KillCount} quái, độ khó x{DifficultyScaling.Current().exp} -> kiếm được {finalExp} EXP.");

        if (GameSessionManager.Instance != null)
            GameSessionManager.Instance.SaveGame();

        return finalExp;
    }
}