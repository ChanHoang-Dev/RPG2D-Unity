using UnityEngine;

[CreateAssetMenu(fileName = "New Level Data", menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Identity")]
    public string levelId;
    public string levelName;
    public int levelIndex; 

    [Header("Runtime State")]
    public bool isUnlocked;
    [Range(0, 3)]
    public int starsEarned;
    [Header("Enemy Level (quái sinh ra random trong khoảng này)")]
    [Min(1)] public int enemyLevelMin = 1;
    [Min(1)] public int enemyLevelMax = 1;
    [Header("Scene")]
    public string sceneNameToLoad;

    public bool IsDifficultyUnLocked(Difficulty difficulty)
    {
        if(!isUnlocked) return false;
        return (int)difficulty <= starsEarned +1;
    }
    public Difficulty HighestUnLockedDifficulty()
    {
        int d =Mathf.Clamp(starsEarned + 1, 1, 3);
        return (Difficulty)d;
    }
}