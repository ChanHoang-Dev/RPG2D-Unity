using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSessionManager : MonoBehaviour
{
    public static LevelSessionManager Instance { get; private set; }

    [SerializeField] private List<LevelData> allLevels = new List<LevelData>();
    public LevelData currentLevel { get; private set; }
    public Difficulty currentDifficulty { get; private set; }
    public IReadOnlyList<LevelData> AllLevels => allLevels;

    public static event Action<LevelData> OnLevelStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (allLevels == null || allLevels.Count == 0)
        {
            Debug.LogError("[LevelSession] allLevels đang rỗng! Kéo các LevelData asset vào Inspector.");
            return;
        }

        if (LevelSaveManager.HasSaveFile())
            LevelSaveManager.Load(allLevels);
    }

    public LevelData GetLevelById(string id) => allLevels.FirstOrDefault(l => l.levelId == id);
    public LevelData GetLevelByIndex(int index) => allLevels.FirstOrDefault(l => l.levelIndex == index);

    public void SetCurrentSession(LevelData level, Difficulty difficulty)
    {
        currentLevel = level;
        currentDifficulty = difficulty;
    }
    /// <summary>
    /// Gọi khi người chơi hoàn thành 1 level với số sao đạt được.
    /// Cập nhật starsEarned (chỉ tăng, không giảm nếu chơi lại điểm thấp hơn)
    /// và mở khóa level kế tiếp nếu đạt tối thiểu 1 sao.
    /// </summary>
    public void CompleteLevel(string levelId, int starsResult)
    {
        LevelData level = GetLevelById(levelId);
        if (level == null)
        {
            Debug.LogWarning($"[LevelSession] Không tìm thấy level id '{levelId}'.");
            return;
        }

        if (starsResult > level.starsEarned)
            level.starsEarned = starsResult;

        if (starsResult >= 1)
        {
            LevelData nextLevel = GetLevelByIndex(level.levelIndex + 1);
            if (nextLevel != null && !nextLevel.isUnlocked)
            {
                nextLevel.isUnlocked = true;
                OnLevelStateChanged?.Invoke(nextLevel);
            }
        }

        LevelSaveManager.Save(allLevels);
        OnLevelStateChanged?.Invoke(level);
    }
    public void CompleteCurrentLevel()
    {
        if (currentLevel == null)
        {
            Debug.LogWarning("[LevelSession] Chưa có CurrentLevel, không thể hoàn thành.");
            return;
        }
        CompleteLevel(currentLevel.levelId, (int)currentDifficulty);
    }

    public void SaveGame() => LevelSaveManager.Save(allLevels);

    private void OnApplicationPause(bool isPaused)
    {
        if (isPaused) SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}