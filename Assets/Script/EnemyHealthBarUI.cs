using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("Nguồn máu")]
    [SerializeField] private Health enemyHealth;

    [Header("UI References")]
    [SerializeField] private Image delayFillImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject healthBarContainer;
    [Header("Delay Bar Setting")]
    [SerializeField] private float delayBeforeCatchUp = 0.4f;
    [SerializeField] private float catchUpSpeed = 2f;

    [Header("Enemy Only")]
    [SerializeField] private bool pollEveryFrameForEnemy = true;

    private int lastKnownEnemyHealth = -1;
    private bool hasTakenDamage = false;
    private float targetFillAmount;
    private float delayTimer;
    void OnEnable()
    {
        hasTakenDamage = false;
        if (healthBarContainer != null)
            healthBarContainer.SetActive(false);
        if (enemyHealth != null)
        {
            lastKnownEnemyHealth = enemyHealth.CurrentHealth;
            targetFillAmount = enemyHealth.MaxHealth > 0
                        ? (float)enemyHealth.CurrentHealth / enemyHealth.MaxHealth
                        : 1f;
            if (fillImage != null) fillImage.fillAmount = targetFillAmount;
            if (delayFillImage != null) delayFillImage.fillAmount = targetFillAmount;
        }
    }
    void Start()
    {
        hasTakenDamage = false;
        if (healthBarContainer != null)
            healthBarContainer.SetActive(false);

        if (enemyHealth != null)
        {
            lastKnownEnemyHealth = enemyHealth.CurrentHealth;
            targetFillAmount = enemyHealth.MaxHealth > 0
                ? (float)enemyHealth.CurrentHealth / enemyHealth.MaxHealth
                : 1f;
            if (fillImage != null) fillImage.fillAmount = targetFillAmount;
            if (delayFillImage != null) delayFillImage.fillAmount = targetFillAmount;
        }
    }
    void Update()
    {
        if (enemyHealth != null && pollEveryFrameForEnemy)
        {
            int current = enemyHealth.CurrentHealth;
            if (current <= 0)
            {
                if (healthBarContainer != null)
                {
                    healthBarContainer.SetActive(false);
                }
                return;
            }
            if (current != lastKnownEnemyHealth)
            {
                bool tookDamage = lastKnownEnemyHealth > 0 && current < lastKnownEnemyHealth;
                lastKnownEnemyHealth = current;
                if (tookDamage && !hasTakenDamage)
                {
                    hasTakenDamage = true;
                    healthBarContainer.SetActive(true);
                }
                HandleHealthChanged(current, enemyHealth.MaxHealth);
            }
        }
        UpdateDelayBar();
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (max <= 0) return;

        targetFillAmount = Mathf.Clamp01((float)current / max);
        if (fillImage != null)
            fillImage.fillAmount = targetFillAmount;
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