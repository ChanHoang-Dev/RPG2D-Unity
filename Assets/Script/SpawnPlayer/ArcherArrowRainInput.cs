using UnityEngine;

/// <summary>
/// Gắn lên Archer (prefab Player). Bấm K để tung chiêu Mưa Tên.
/// Chiêu được sinh từ prefab ArrowRainRoot, đặt tại vị trí Player lúc bấm K rồi đứng yên.
/// </summary>
public class ArcherArrowRainInput : MonoBehaviour
{
    [Header("Chiêu")]
    [Tooltip("Prefab ArrowRainRoot (chứa PS_RainArrow + WarningZone + script ArrowRainSkill).")]
    [SerializeField] private GameObject arrowRainPrefab;
    [SerializeField] private KeyCode castKey = KeyCode.K;
    [SerializeField] private float cooldown = 8f;

    [Header("Vị trí mưa")]
    [Tooltip("Lệch ngang so với Player (đơn vị Unity).")]
    [SerializeField] private float offsetX = 0f;
    [Tooltip("Chiều cao điểm sinh hạt so với Player (đơn vị Unity).")]
    [SerializeField] private float spawnHeight = 10f;

    [Header("Vùng cảnh báo đỏ")]
    [Tooltip("Tick: vùng đỏ nằm ở dưới đất (ngang chân Player) thay vì ở điểm sinh hạt trên cao.")]
    [SerializeField] private bool warningAtGround = true;
    [Tooltip("Độ cao vùng đỏ so với vị trí Player. Số âm = thấp hơn Player (ngang mặt đất).")]
    [SerializeField] private float warningOffsetY = -0.5f;

    private ArrowRainSkill arrowRain;
    private Transform rainRoot;
    private CombatEntity combat;
    private Health health;
    private float nextCastTime;

    private void Awake()
    {
        combat = GetComponent<CombatEntity>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        if (arrowRainPrefab == null)
        {
            Debug.LogWarning("ArcherArrowRainInput: chưa gán Arrow Rain Prefab.");
            return;
        }

        // Sinh một bản duy nhất, dùng lại cho mọi lần tung chiêu
        GameObject go = Instantiate(arrowRainPrefab);
        rainRoot = go.transform;
        arrowRain = go.GetComponentInChildren<ArrowRainSkill>();

        if (arrowRain == null)
            Debug.LogWarning("ArcherArrowRainInput: prefab không có script ArrowRainSkill.");
    }

    private void OnDestroy()
    {
        // Player biến mất: dọn chiêu sau vài giây để các mũi tên đang rơi kịp rơi nốt
        if (rainRoot != null)
            Destroy(rainRoot.gameObject, 10f);
    }

    private void Update()
    {
        if (arrowRain == null) return;
        if (!Input.GetKeyDown(castKey)) return;

        if (Time.time < nextCastTime) return;     // đang hồi chiêu
        if (arrowRain.IsCasting) return;          // chiêu đang chạy
        if (health != null && health.IsDead()) return;

        CastArrowRain();
    }

    private void CastArrowRain()
    {
        // Đặt chiêu tại vị trí Player ngay lúc bấm, sau đó chiêu đứng yên
        Vector3 pos = transform.position;
        pos.x += offsetX;
        pos.y += spawnHeight;
        rainRoot.position = pos;

        if (warningAtGround)
            arrowRain.SetWarningWorldY(transform.position.y + warningOffsetY);

        ICombatStats stats = combat != null ? combat.GetStats() : null;
        arrowRain.Cast(stats);

        nextCastTime = Time.time + cooldown;
    }
}