using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float fireRate = 1f;
    [SerializeField] protected float range = 5f;
    [SerializeField] protected float criticalChance = 0.1f;

    protected float nextFireTime;
    protected Camera mainCamera;
    protected PlayerController playerController;

    protected virtual void Start()
    {
        nextFireTime = Time.time;
        mainCamera = Camera.main;
        playerController = GetComponentInParent<PlayerController>();

        if (mainCamera == null) Debug.LogWarning("Main Camera not found!");
        if (playerController == null) Debug.LogWarning("PlayerController not found!");
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            PerformAttack();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    protected virtual void PerformAttack()
    {
        // Diimplementasikan di child class
    }

    public virtual void UpgradeDamage(float multiplier)
    {
        damage *= multiplier;
    }

    public virtual void UpgradeFireRate(float multiplier)
    {
        fireRate *= multiplier;
    }

    public virtual void UpgradeRange(float amount)
    {
        range += amount;
    }

    public virtual void SetCriticalChance(float chance)
    {
        criticalChance = Mathf.Clamp(chance, 0f, 1f);
    }

    protected GameObject FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDistance = Mathf.Infinity;
        Vector3 position = transform.position;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(enemy.transform.position, position);
            if (distance < minDistance && distance <= range)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }
        return nearest;
    }

    protected Vector2 GetMouseDirection()
    {
        if (mainCamera == null) return Vector2.zero;
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return (mousePos - transform.position).normalized;
    }

    protected Collider2D[] GetEnemiesInRange(float range)
    {
        return Physics2D.OverlapCircleAll(transform.position, range, LayerMask.GetMask("Enemy"));
    }

    protected void DamageEnemy(GameObject enemy, float damage)
    {
        Enemy enemyHealth = enemy.GetComponent<Enemy>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }
    }

    protected void ApplyKnockback(GameObject enemy, Vector2 direction)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            float knockbackForce = 5f; // Bisa diatur di Inspector kalau mau
            enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
        }
    }
}