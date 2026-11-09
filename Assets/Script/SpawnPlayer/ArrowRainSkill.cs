using System.Collections;
using UnityEngine;

/// <summary>
/// Chiêu Mưa Tên (gắn lên PS_RainArrow):
/// 1. Vùng cảnh báo đỏ nhấp nháy
/// 2. Bắn mưa tên (Particle System)
/// 3. Hạt chạm Ground -> biến mất (do Lifetime Loss của module Collision)
/// 4. Hạt chạm Enemy  -> gây sát thương, hạt biến mất
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ArrowRainSkill : MonoBehaviour
{
    [Header("Thời gian")]
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private float rainTime = 4f;

    [Header("Vùng cảnh báo đỏ")]
    [SerializeField] private SpriteRenderer warningZone;
    [Tooltip("Màu vùng cảnh báo (Alpha sẽ do script điều khiển khi nhấp nháy).")]
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float blinkSpeed = 10f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0.15f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.6f;
    [Tooltip("Tick nếu muốn vùng đỏ vẫn hiện suốt cơn mưa.")]
    [SerializeField] private bool keepWarningDuringRain = false;
    [Tooltip("Tự copy Sorting Layer + Order in Layer từ mưa tên (mưa đã thấy được thì vùng đỏ cũng thấy được).")]
    [SerializeField] private bool matchRainSorting = true;
    [Tooltip("Vùng đỏ cao hơn mưa bao nhiêu Order (âm = nằm sau mưa).")]
    [SerializeField] private int warningOrderOffset = 1;
    [Tooltip("In thông tin vùng đỏ ra Console mỗi lần tung chiêu để tìm lỗi không hiện.")]
    [SerializeField] private bool debugWarning = true;

    [Header("Sát thương")]
    [SerializeField] private LayerMask enemyLayer;
    [Tooltip("Chỉ dùng khi không tính được damage từ stat.")]
    [SerializeField] private int fallbackDamage = 1;

    private ParticleSystem rain;
    private ICombatStats attackerStats;
    private Coroutine routine;
    private readonly System.Collections.Generic.List<ParticleCollisionEvent> events =
        new System.Collections.Generic.List<ParticleCollisionEvent>();

    public bool IsCasting => routine != null;

    private void Awake()
    {
        rain = GetComponent<ParticleSystem>();

        if (matchRainSorting && warningZone != null)
        {
            ParticleSystemRenderer pr = GetComponent<ParticleSystemRenderer>();
            if (pr != null)
            {
                warningZone.sortingLayerID = pr.sortingLayerID;
                warningZone.sortingOrder = pr.sortingOrder + warningOrderOffset;
            }
        }
    }

    private void Start()
    {
        if (warningZone != null) warningZone.gameObject.SetActive(false);
        // Cast(null); // bỏ comment để test ngay khi Play
    }

    // ───────────────────────── Điều khiển chiêu ─────────────────────────

    /// <summary>Tung chiêu. Truyền stat của người tung (Player/Boss), hoặc null để dùng fallbackDamage.</summary>
    public void Cast(ICombatStats attacker)
    {
        if (routine != null) return; // đang tung chiêu thì bỏ qua

        attackerStats = attacker;
        routine = StartCoroutine(CastRoutine());
    }

    /// <summary>Đặt độ cao (toạ độ thế giới) của vùng cảnh báo, ví dụ để nó nằm dưới đất chỗ mưa sắp rơi.</summary>
    public void SetWarningWorldY(float worldY)
    {
        if (warningZone == null) return;
        Vector3 p = warningZone.transform.position;
        p.y = worldY;
        warningZone.transform.position = p;
    }

    private void LogWarningInfo()
    {
        SpriteRenderer sr = warningZone;
        Camera cam = Camera.main;

        string inView = "không có Camera.main";
        string cullOk = "?";
        string camPos = "?";
        if (cam != null)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            inView = GeometryUtility.TestPlanesAABB(planes, sr.bounds) ? "CÓ" : "KHÔNG";
            cullOk = (cam.cullingMask & (1 << sr.gameObject.layer)) != 0 ? "có" : "KHÔNG";
            camPos = cam.transform.position.ToString();
        }

        Debug.Log(
            "[ArrowRain] WarningZone"
            + " | sprite=" + (sr.sprite != null ? sr.sprite.name : "TRỐNG")
            + " | pos=" + sr.transform.position
            + " | lossyScale=" + sr.transform.lossyScale
            + " | kích thước thật=" + sr.bounds.size
            + " | sortingLayer=" + SortingLayer.IDToName(sr.sortingLayerID)
            + " | order=" + sr.sortingOrder
            + " | color=" + sr.color
            + " | material=" + (sr.sharedMaterial != null ? sr.sharedMaterial.name : "null")
            + " | trong khung hình camera: " + inView
            + " | camera vẽ layer này: " + cullOk
            + " | camera pos=" + camPos,
            sr);
    }

    /// <summary>Huỷ chiêu giữa chừng.</summary>
    public void Cancel()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        if (warningZone != null) warningZone.gameObject.SetActive(false);
        rain.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private IEnumerator CastRoutine()
    {
        // 1. Cảnh báo đỏ nhấp nháy
        if (warningZone != null)
        {
            warningZone.gameObject.SetActive(true);
            if (debugWarning) LogWarningInfo();
            Color c = warningColor;
            float t = 0f;
            while (t < warningTime)
            {
                t += Time.deltaTime;
                float wave = (Mathf.Sin(t * blinkSpeed) + 1f) * 0.5f;
                c.a = Mathf.Lerp(minAlpha, maxAlpha, wave);
                warningZone.color = c;
                yield return null;
            }

            if (!keepWarningDuringRain)
                warningZone.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ArrowRainSkill: chưa gán Warning Zone nên không có vùng cảnh báo đỏ.");
            yield return new WaitForSeconds(warningTime);
        }

        // 2. Mưa tên
        rain.Play();
        yield return new WaitForSeconds(rainTime);

        // 3. Dừng sinh hạt mới, các mũi tên đang rơi vẫn rơi nốt
        rain.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (warningZone != null) warningZone.gameObject.SetActive(false);
        routine = null;
    }

    // ───────────────────────── Va chạm ─────────────────────────

    // Chạm Ground: không cần xử lý, hạt tự biến mất nhờ Lifetime Loss = 1.
    private void OnParticleCollision(GameObject other)
    {
        if ((enemyLayer.value & (1 << other.layer)) == 0) return;

        Health targetHealth = other.GetComponentInParent<Health>();
        if (targetHealth == null || targetHealth.IsDead()) return;

        CombatEntity targetCombat = other.GetComponentInParent<CombatEntity>();

        // Mỗi hạt trúng = 1 lần sát thương
        int count = rain.GetCollisionEvents(other, events);
        for (int i = 0; i < count; i++)
        {
            if (targetHealth.IsDead()) break;

            if (attackerStats != null && targetCombat != null)
            {
                DamageResult result = CombatCalculator.CalculateDamage(attackerStats, targetCombat.GetStats());
                targetHealth.TakeDamage(result);
            }
            else
            {
                targetHealth.TakeDamage(fallbackDamage);
            }
        }
    }
}