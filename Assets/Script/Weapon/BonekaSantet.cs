using System.Collections.Generic;
using UnityEngine;

public class Weapon_SingleTarget : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float baseAttackRange = 2f;
    public int baseDamage = 1;
    public float baseAttackCooldown = 1f;
    public LayerMask enemyLayer;
    public GameObject hitEffectPrefab;

    [Header("Upgrade Settings")]
    public int level = 1;
    public int maxLevel = 5;
    public float rangeIncreasePerLevel = 0.5f;
    public int damageIncreasePerLevel = 1;
    public float cooldownReductionPerLevel = 0.1f;

    private float lastAttackTime;

    void Update()
    {
        if (Time.time >= lastAttackTime + GetAttackCooldown())
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Attack()
    {
        float currentRange = GetAttackRange();
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(transform.position, currentRange, enemyLayer);
        Debug.Log($"Enemies in range: {enemiesInRange.Length}");

        if (enemiesInRange.Length > 0)
        {
            Collider2D randomEnemy = enemiesInRange[Random.Range(0, enemiesInRange.Length)];
            Enemy enemy = randomEnemy.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(GetDamage());

                // Spawn hit effect
                if (hitEffectPrefab != null)
                {
                    GameObject effect = Instantiate(hitEffectPrefab, enemy.transform.position, Quaternion.identity);
                    Destroy(effect, 0.5f); // auto destroy hit effect after 0.5s
                }
            }
        }
    }

    // --- Upgrade ---
    public void UpgradeWeapon()
    {
        if (level < maxLevel)
        {
            level++;
            Debug.Log($"Weapon upgraded to level {level}!");
        }
        else
        {
            Debug.Log("Weapon is already at max level!");
        }
    }

    // --- Helper Methods for Stats ---
    private float GetAttackRange()
    {
        return baseAttackRange + (level - 1) * rangeIncreasePerLevel;
    }

    private int GetDamage()
    {
        return baseDamage + (level - 1) * damageIncreasePerLevel;
    }

    private float GetAttackCooldown()
    {
        return Mathf.Max(0.1f, baseAttackCooldown - (level - 1) * cooldownReductionPerLevel);
    }

    // Visualize range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, GetAttackRange());
    }
}
