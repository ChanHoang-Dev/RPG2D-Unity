using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image delayFillImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [Header("Delay Bar Setting")]
    [SerializeField] private float delayBeforeCatchUp = 0.4f ;
    [SerializeField] private float catchUpSpeed = 2f;

    private PlayerHealth playerHealth;
    private float targetFillAmount;
    private float delayTimer;

    void OnEnable()
    {
        TryBindPlayer();
    }

    void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    void Update()
    {
        // Phòng trường hợp Player spawn muộn hơn UI, hoặc bị respawn (instance mới)
        if (playerHealth == null)
        {
            TryBindPlayer();
        }
        UpdateDelayBar();
    }

    private void TryBindPlayer()
    {
        if (playerHealth != null) return;

        PlayerHealth found = FindFirstObjectByType<PlayerHealth>();
        if (found == null) return;

        playerHealth = found;
        playerHealth.OnHealthChanged += HandleHealthChanged;
        HandleHealthChanged(playerHealth.CurrentHealth, playerHealth.maxHealth);

        if(delayFillImage != null)
        {
            delayFillImage.fillAmount = targetFillAmount;
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (fillImage == null || max <= 0) return;
        targetFillAmount = Mathf.Clamp01((float)current / max);
        if (fillImage != null)
            fillImage.fillAmount = targetFillAmount;
        if (healthText != null)
            healthText.text = $"{current} / {max}";
        delayTimer = delayBeforeCatchUp;
    }
    private void UpdateDelayBar()
    {
        if (delayFillImage == null) return;
        if (Mathf.Approximately(delayFillImage.fillAmount, targetFillAmount)) return;

        if (delayTimer > 0f)
        {
            delayTimer -= Time.deltaTime;
            return;
        }

        delayFillImage.fillAmount = Mathf.MoveTowards(
            delayFillImage.fillAmount,
            targetFillAmount,
            catchUpSpeed * Time.deltaTime
        );
    }
}