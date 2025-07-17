using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public UnityEngine.UI.Slider healthBar;

    private bool isInvincible;

    private float currentHealth;
    private UIManager uiManager;

    // Event untuk UI
    public Action<float> OnHealthChanged; // Kirim rasio currentHealth/maxHealth

    void Start()
    {
        currentHealth = maxHealth;
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("UIManager not found in scene!");
        }
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateHealthUI();
        Vector3 textPosition = transform.position + Vector3.up * 1.5f;
        FloatingText.Create(damage.ToString(), textPosition, Color.red);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount; // Heal sejumlah yang sama
        UpdateHealthUI();
    }

    // Tambahan untuk PlayerStats
    public void SetMaxHealth(float value)
    {
        maxHealth = Mathf.Max(0f, value);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }

    public void SetCurrentHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        UpdateHealthUI();
    }
    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    void UpdateHealthUI()
    {
        float healthRatio = maxHealth > 0 ? currentHealth / maxHealth : 0;
        if (healthBar != null)
        {
            healthBar.value = healthRatio;
        }
        OnHealthChanged?.Invoke(healthRatio);
    }

    void Die()
    {
        if (uiManager != null)
        {
            uiManager.OnPlayerDied();
        }
        else
        {
            Debug.LogWarning("Cannot trigger OnPlayerDied: UIManager is null!");
        }
        Time.timeScale = 0f;
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}