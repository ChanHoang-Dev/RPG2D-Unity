using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class LoreonInvincible : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode castKey = KeyCode.K;

    [Header("Chiêu")]
    [SerializeField] private float activeDuration = 10f;
    [SerializeField] private float cooldown = 10f;

    [Header("Hiển thị")]
    [SerializeField] private Color invincibleColor = new Color(0.6f, 0.25f, 0.9f, 1f);
    [Tooltip("Tick: tô màu cả object con (vd. FireRing). Mặc định chỉ tô sprite của chính nhân vật.")]
    [SerializeField] private bool tintChildren = false;

    private PlayerHealth health;
    private Animator animator;
    private SpriteRenderer[] renderers;
    private Color[] originalColors;

    private bool isActive;
    private float endTime;
    private float nextCastTime;

    public bool IsActive => isActive;
    public float ActiveRemaining => isActive ? Mathf.Max(0f, endTime - Time.time) : 0f;
    public float CooldownRemaining => isActive ? cooldown : Mathf.Max(0f, nextCastTime - Time.time);

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();

        renderers = tintChildren
            ? GetComponentsInChildren<SpriteRenderer>(true)
            : GetComponents<SpriteRenderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    private void Update()
    {
        if (health.IsDead)
        {
            if (isActive) End();
            return;
        }

        if (isActive)
        {
            if (Time.time >= endTime) End();
            return;
        }

        if (Input.GetKeyDown(castKey) && Time.time >= nextCastTime)
            Begin();
    }

    private void Begin()
    {
        isActive = true;
        endTime = Time.time + activeDuration;

        health.IsInvincible = true;
        if (animator != null) animator.ResetTrigger("Hurt");

        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = invincibleColor;
    }

    private void End()
    {
        if (!isActive) return;
        isActive = false;
        nextCastTime = Time.time + cooldown; // cooldown tính sau khi hết active

        health.IsInvincible = false;

        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) renderers[i].color = originalColors[i];
    }

    private void OnDisable()
    {
        if (isActive) End();
    }
}