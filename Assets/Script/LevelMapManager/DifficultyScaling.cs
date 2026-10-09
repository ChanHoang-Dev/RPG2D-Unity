using UnityEngine;

public static class DifficultyScaling
{
    public struct Multiplier
    {
        public float health;
        public float attack;
        public float defense;
    }

    public static Multiplier Get(Difficulty d)
    {
        switch (d)
        {
            case Difficulty.Medium: return new Multiplier { health = 1.5f, attack = 1.2f, defense = 1.2f };
            case Difficulty.Hard:   return new Multiplier { health = 2.0f, attack = 1.5f, defense = 1.5f };
            default:                return new Multiplier { health = 1.0f, attack = 1.0f, defense = 1.0f };
        }
    }

    // Độ khó của phiên chơi hiện tại, mặc định Easy nếu chưa qua menu
    public static Multiplier Current()
    {
        Difficulty d = LevelSessionManager.Instance != null
            ? LevelSessionManager.Instance.currentDifficulty
            : Difficulty.Easy;
        return Get(d);
    }
}