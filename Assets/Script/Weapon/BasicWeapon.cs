using UnityEngine;

public class BasicWeapon : BaseWeapon
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Upgrade Settings")]
    public int weaponLevel = 1;
    public int maxLevel = 5;

    public float[] damagePerLevel = { 10, 15, 20, 25, 30 };
    public float[] critChancePerLevel = { 0.1f, 0.15f, 0.2f, 0.25f, 0.3f };
    public float[] cooldownPerLevel = { 1f, 0.9f, 0.8f, 0.7f, 0.6f };
    public int[] projectileCountPerLevel = { 1, 1, 2, 2, 3 };

    private int currentProjectileCount = 1;

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

        ApplyUpgradeStats(); // Apply starting stats
    }

    public bool UpgradeWeapon()
    {
        if (weaponLevel >= maxLevel)
        {
            Debug.Log("Weapon is already at max level!");
            return false;
        }

        weaponLevel++;
        ApplyUpgradeStats();

        Debug.Log($"Weapon upgraded to level {weaponLevel}!");
        return true;
    }

    private void ApplyUpgradeStats()
    {
        damage = damagePerLevel[weaponLevel - 1];
        criticalChance = critChancePerLevel[weaponLevel - 1];
        attackCooldown = cooldownPerLevel[weaponLevel - 1];
        currentProjectileCount = projectileCountPerLevel[weaponLevel - 1];
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

        int count = currentProjectileCount;
        float spreadAngle = 10f; // Total spread angle in degrees

        for (int i = 0; i < count; i++)
        {
            // Angle offset per projectile
            float angleOffset = (-(count - 1) / 2f + i) * spreadAngle;
            Vector2 spreadDir = Quaternion.Euler(0, 0, angleOffset) * direction;

            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            float finalDamage = damage * (Random.value < criticalChance ? 2f : 1f);

            Projectile projectileScript = projectile.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(spreadDir.normalized, finalDamage);
            }
            else
            {
                Debug.LogWarning("Projectile script not found!");
            }

            float angle = Mathf.Atan2(spreadDir.y, spreadDir.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }
}
