using UnityEngine;

public class SamuraiStealth : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode castKey = KeyCode.K;

    [Header("Chiêu")]
    [SerializeField] private float stealthDuration = 10f;
    [SerializeField] private float cooldown = 10f;
    [Tooltip("Tick: thời gian hồi chiêu chỉ bắt đầu đếm SAU KHI hết tàn hình. " +
             "Bỏ tick: đếm từ lúc bấm K (khi đó Cooldown <= Stealth Duration sẽ cho tàn hình liên tục).")]
    [SerializeField] private bool cooldownStartsAfterStealth = true;

    [Header("Hiển thị")]
    [Tooltip("Độ trong suốt của Samurai khi tàn hình (1 = hiện rõ, 0 = biến mất hoàn toàn).")]
    [SerializeField, Range(0f, 1f)] private float stealthAlpha = 0.35f;
    [Tooltip("Số giây cuối nhấp nháy để người chơi biết sắp hết tàn hình.")]
    [SerializeField] private float warnBlinkTime = 2f;
    [SerializeField] private float blinkSpeed = 12f;

    private PlayerHealth health;
    private SpriteRenderer[] renderers;
    private float[] originalAlpha;

    private bool isInvisible;
    private float stealthEndTime;
    private float nextCastTime;

    /// <summary>Quái đọc biến này để biết có được đuổi/tấn công Player không.</summary>
    public bool IsInvisible => isInvisible;
    public float StealthRemaining => isInvisible ? Mathf.Max(0f, stealthEndTime - Time.time) : 0f;
    public float CooldownRemaining => isInvisible ? cooldown : Mathf.Max(0f, nextCastTime - Time.time);

    /// <summary>true = vừa bắt đầu tàn hình, false = vừa hết. Dùng cho UI/âm thanh nếu cần.</summary>
    public event System.Action<bool> OnStealthChanged;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();

        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        originalAlpha = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalAlpha[i] = renderers[i].color.a;
    }

    private void Update()
    {
        if (health != null && health.IsDead)
        {
            if (isInvisible) EndStealth();
            return;
        }

        if (isInvisible)
        {
            if (Time.time >= stealthEndTime)
            {
                EndStealth();
                return;
            }
            UpdateVisual();
            return;
        }

        if (Input.GetKeyDown(castKey) && Time.time >= nextCastTime)
            BeginStealth();
    }

    private void BeginStealth()
    {
        isInvisible = true;
        stealthEndTime = Time.time + stealthDuration;

        if (!cooldownStartsAfterStealth)
            nextCastTime = Time.time + cooldown;

        SetAlphaMultiplier(stealthAlpha);
        OnStealthChanged?.Invoke(true);
    }

    private void EndStealth()
    {
        if (!isInvisible) return;

        isInvisible = false;

        if (cooldownStartsAfterStealth)
            nextCastTime = Time.time + cooldown;

        SetAlphaMultiplier(1f);
        OnStealthChanged?.Invoke(false);
    }

    private void UpdateVisual()
    {
        float alpha = stealthAlpha;

        // Những giây cuối: nhấp nháy cảnh báo sắp lộ diện
        if (StealthRemaining <= warnBlinkTime)
        {
            float wave = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;
            alpha = Mathf.Lerp(stealthAlpha, 0.9f, wave);
        }

        SetAlphaMultiplier(alpha);
    }

    private void SetAlphaMultiplier(float multiplier)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            Color c = renderers[i].color;
            c.a = originalAlpha[i] * multiplier;
            renderers[i].color = c;
        }
    }

    // Tắt/huỷ object giữa lúc đang tàn hình thì trả lại màu gốc
    private void OnDisable()
    {
        if (isInvisible) EndStealth();
    }
}