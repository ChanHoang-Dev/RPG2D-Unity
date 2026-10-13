using UnityEngine;

public class CombatEntity : MonoBehaviour
{
    public enum EntityKind { Player, Enemy }
    [SerializeField] private EntityKind kind;

    [Header("Player mode")]
    [SerializeField] private PlayerStatsSO fallbackPlayerSO;

    [Header("Enemy mode")]
    [SerializeField] private EnemyStatsSO enemySO;

    private CharacterData playerCharacter;
    private PlayerStatsSO playerSO;
    private bool isInitialized = false;

    public PlayerStatsRuntime PlayerRuntime => playerCharacter != null ? playerCharacter.playerStatsRuntime : null;

    private void Awake()
    {
        isInitialized = true;
        if (kind == EntityKind.Player)
        {
            InitPlayerFromSession();
            LogPlayerStats();
        }
    }
    private void Start()
    {
        if( kind == EntityKind.Enemy)
        {
            LogEnemyStats();
        }
    }
    private void LogPlayerStats()
    {
        if (playerCharacter == null || playerSO == null)
        {
            Debug.LogWarning($"[CombatEntity] Không có playerCharacter hoặc playerSO để log stats.");
            return;
        }

        var stats = GetStats();
        Debug.Log($"[CombatEntity] Player '{playerCharacter.characterName}' (Lv.{playerCharacter.level}) stats: " +
               $"HP={GetMaxHealth()}, Attack={stats.Attack}, Defense={stats.Defense}, CritChance={stats.CritChance}, " +
               $"CritMultiplier={stats.CritMultiplier}, DamageReduction={stats.DamageReduction}, " +
               $"DamageRollPercentMin={stats.DamageRollPercentMin}, DamageRollPercentMax={stats.DamageRollPercentMax}, " +
               $"ArmorPenetration={stats.ArmorPenetration}, TrueDamageChance={stats.TrueDamageChance}");
    }
    private void LogEnemyStats()
    {
        if (enemySO == null)
        {
            Debug.LogWarning($"[CombatEntity] {name} chưa gán enemySO.");
            return;
        }

        var s = GetStats();
        var difficulty = LevelSessionManager.Instance != null
            ? LevelSessionManager.Instance.currentDifficulty
            : Difficulty.Easy;

        Debug.Log($"[Enemy Spawn] {name.Replace("(Clone)", "")} | Độ khó={difficulty} | " +
                  $"HP={GetMaxHealth()} (gốc {enemySO.maxHealth}) | " +
                  $"ATK={s.Attack} (gốc {enemySO.attack}) | " +
                  $"DEF={s.Defense} (gốc {enemySO.defense}) | " +
                  $"Crit={s.CritChance:P0} x{s.CritMultiplier}");
    }

    private void InitPlayerFromSession()
    {
        if (GameSessionManager.Instance != null && GameSessionManager.Instance.SelectedCharacter != null)
        {
            playerCharacter = GameSessionManager.Instance.SelectedCharacter;
            playerSO = playerCharacter.statsSO != null ? playerCharacter.statsSO : fallbackPlayerSO;
            Debug.Log($"[CombatEntity] SelectedCharacter='{playerCharacter.characterName}' " +
                 $"(asset instance ID={playerCharacter.GetInstanceID()}), " +
                 $"statsSO={(playerCharacter.statsSO != null ? playerCharacter.statsSO.name : "NULL")}, " +
                 $"dùng playerSO={(playerSO != null ? playerSO.name : "NULL")} " +
                 $"(hpMin trong SO này={(playerSO != null ? playerSO.hpMin.ToString() : "N/A")})");

            if (playerCharacter.statsSO == null)
                Debug.LogWarning($"[CombatEntity] '{playerCharacter.characterName}' chưa gán statsSO -> dùng fallback.");
        }
        else
        {
            Debug.LogWarning("[CombatEntity] Không tìm thấy GameSessionManager.SelectedCharacter -> dùng fallback rỗng để test.");
            playerCharacter = new CharacterData { characterName = "TestPlayer", isUnlocked = true };
            playerSO = fallbackPlayerSO;
        }

        if (playerSO == null)
        {
            Debug.LogError($"[CombatEntity] playerSO đang NULL cho '{playerCharacter.characterName}' trên '{gameObject.name}'! " +
                            $"Cần gán 'statsSO' trong CharacterData, hoặc 'fallbackPlayerSO' trên prefab.");
        }
    }

    public ICombatStats GetStats()
    {
        if (kind == EntityKind.Player)
            return new PlayerCombatStatsAdapter(playerSO, playerCharacter.playerStatsRuntime);
        var m = DifficultyScaling.Current();
        return new EnemyCombatStatsAdapter(enemySO, m.attack, m.defense);
    }

    public DamageResult DealDamageTo(CombatEntity target)
    {
        return CombatCalculator.CalculateDamage(GetStats(), target.GetStats());
    }

    // SỬA: hàm duy nhất, tự bảo vệ mọi trường hợp gọi sớm/gọi lúc Edit Mode
    public int GetMaxHealth()
    {
        if (!isInitialized)
        {
            // Chưa Awake() (Edit Mode, hoặc gọi quá sớm trước khi Unity khởi tạo component)
            return 0;
        }

        if (kind == EntityKind.Player)
        {
            if (playerCharacter == null || playerCharacter.playerStatsRuntime == null || playerSO == null)
                return 0;

            return playerCharacter.playerStatsRuntime.GetMaxHealth(playerSO);
        }
        if (enemySO == null) return 0;
        return Mathf.RoundToInt(enemySO.maxHealth * DifficultyScaling.Current().health);
    }

    private readonly struct PlayerCombatStatsAdapter : ICombatStats
    {
        private readonly PlayerStatsSO so;
        private readonly PlayerStatsRuntime rt;
        public PlayerCombatStatsAdapter(PlayerStatsSO so, PlayerStatsRuntime rt) { this.so = so; this.rt = rt; }

        public int Attack => rt.GetAttack(so);
        public int Defense => rt.GetDefense(so);
        public float CritChance => rt.GetCritChance(so);
        public float CritMultiplier => rt.GetCritMultiplier(so);
        public float DamageReduction => rt.GetDamageReduction(so);
        public float DamageRollPercentMin => so.damageRollPercentMin;
        public float DamageRollPercentMax => so.damageRollPercentMax;
        public float ArmorPenetration => rt.GetArmorPen(so);
        public float TrueDamageChance => rt.GetTrueDamageChance(so);
        public float AttackSpeed => rt.GetAttackSpeed(so);
    }

    private readonly struct EnemyCombatStatsAdapter : ICombatStats
    {
        private readonly EnemyStatsSO so;
        private readonly float attMul;
        private readonly float defMul;
        public EnemyCombatStatsAdapter(EnemyStatsSO so, float attMul, float defMul)
        {
            this.so = so;
            this.attMul = attMul;
            this.defMul = defMul;
        }

        public int Attack => Mathf.RoundToInt(so.attack * attMul);
        public int Defense => Mathf.RoundToInt(so.defense * defMul);
        public float CritChance => so.critChance;
        public float CritMultiplier => so.critMultiplier;
        public float DamageReduction => 0f;
        public float DamageRollPercentMin => so.damageRollPercentMin;
        public float DamageRollPercentMax => so.damageRollPercentMax;
        public float ArmorPenetration => 0f;
        public float TrueDamageChance => 0f;
        public float AttackSpeed => 1f;
    }
}