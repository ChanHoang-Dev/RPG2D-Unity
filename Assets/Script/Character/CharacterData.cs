using UnityEngine;
[CreateAssetMenu(fileName = "New Character Data", menuName = "Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public string characterName;
    public int level;
    public bool isUnlocked;
    public RuntimeAnimatorController animatorController;
    public Sprite thumbnail;
    [Header("Prefab")]
    public GameObject characterPrefab;

    [Header("Attack Config")]
    [Range(1, 3)]
    public int attackCount;

    [Header("Power Rating")]
    [Range(0, 5)]
    public float powerRating;
    [Header("Stats")]
    public PlayerStatsRuntime playerStatsRuntime = new PlayerStatsRuntime();
    [Header("Stats")]
    [Tooltip("Chỉ số gốc (min/max) CỦA RIÊNG nhân vật này. Mỗi nhân vật có thể chỉnh khác nhau tùy độ mạnh/yếu.")]
    public PlayerStatsSO statsSO;
    private void OnEnable()
    {
        Setup();
    }
 
    private void OnValidate()
    {
        Setup();
    }
 
    private void Setup()
    {
        if (playerStatsRuntime == null) playerStatsRuntime = new PlayerStatsRuntime();
        playerStatsRuntime.Bind(this);
        RefreshLevelCache();
    }
 
    /// <summary>Re-sync the public 'level' field from EXP. Called after loading / granting EXP.</summary>
    public void RefreshLevelCache()
    {
        if (playerStatsRuntime != null)
            level = playerStatsRuntime.Level;
    }
}
