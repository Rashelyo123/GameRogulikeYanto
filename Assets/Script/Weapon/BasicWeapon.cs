using UnityEngine;

public class BasicWeapon : BaseWeapon
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    protected override void Start()
    {
        base.Start();

        if (firePoint == null)
        {
            GameObject firePointObj = new GameObject("FirePoint");
            firePointObj.transform.SetParent(transform);
            firePointObj.transform.localPosition = Vector3.zero;
            firePoint = firePointObj.transform;
            Debug.LogWarning("FirePoint was null, auto-created at (0,0,0) relative to weapon.");
        }

        if (projectilePrefab == null)
        {
            Debug.LogWarning("ProjectilePrefab not assigned in BasicWeapon!");
        }
    }

    protected override void PerformAttack()
    {
        GameObject target = FindNearestEnemy();
        if (target != null)
        {
            Vector2 direction = (target.transform.position - firePoint.position).normalized;
            FireProjectile(direction);
        }
    }

    protected virtual void FireProjectile(Vector2 direction)
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("Cannot fire: projectilePrefab is null!");
            return;
        }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Hitung damage dengan critical chance
        float finalDamage = damage * (Random.value < criticalChance ? 2f : 1f);

        // Initialize projectile
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(direction, finalDamage);
        }
        else
        {
            Debug.LogWarning("Projectile component not found on instantiated projectile!");
        }

        // Rotate projectile
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}