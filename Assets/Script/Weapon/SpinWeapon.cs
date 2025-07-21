using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAttackWeapon : BaseWeapon
{
    [Header("Spin Settings")]
    public GameObject spinEffectPrefab;
    public float spinDuration = 1f;

    [Header("Orbit Animation")]
    public Transform player;
    public float orbitRadius = 2f;
    public float orbitSpeed = 360f;
    public int weaponCount = 1;

    [Header("Visual Effects")]
    public bool showTrail = true;
    public Color trailColor = Color.white;
    public float trailWidth = 0.1f;

    [Header("Upgrade Settings")]
    public int level = 1;
    public int maxLevel = 5;
    public float damagePerLevel = 1f;
    public float radiusPerLevel = 0.3f;
    public float durationPerLevel = 0.2f;
    public int weaponCountPerLevel = 1;

    private bool isSpinning = false;
    private List<GameObject> orbitingWeapons = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    protected override void PerformAttack()
    {
        if (!isSpinning && player != null)
        {
            StartCoroutine(PerformSpinAttack());
        }
    }

    protected virtual IEnumerator PerformSpinAttack()
    {
        isSpinning = true;

        CreateOrbitingWeapons();
        StartCoroutine(OrbitAnimation());
        StartCoroutine(ContinuousDamage());

        yield return new WaitForSeconds(GetSpinDuration());

        DestroyOrbitingWeapons();
        isSpinning = false;
    }

    void CreateOrbitingWeapons()
    {
        for (int i = 0; i < GetWeaponCount(); i++)
        {
            GameObject weapon = spinEffectPrefab != null
                ? Instantiate(spinEffectPrefab, player.position, Quaternion.identity)
                : CreateSimpleWeapon();

            if (showTrail) AddTrailEffect(weapon);

            if (weapon.GetComponent<Collider2D>() == null)
            {
                CircleCollider2D col = weapon.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
            }

            WeaponDamager damager = weapon.AddComponent<WeaponDamager>();
            damager.damage = GetDamage();
            damager.knockbackForce = 8f;

            orbitingWeapons.Add(weapon);
        }
    }

    GameObject CreateSimpleWeapon()
    {
        GameObject weapon = new GameObject("OrbitingWeapon");
        SpriteRenderer sr = weapon.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>()?.sprite;
        sr.color = Color.white;
        return weapon;
    }

    void AddTrailEffect(GameObject weapon)
    {
        TrailRenderer trail = weapon.AddComponent<TrailRenderer>();
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = trailColor;
        trail.startWidth = trailWidth;
        trail.endWidth = 0f;
        trail.time = 0.3f;
        trail.minVertexDistance = 0.1f;
    }

    IEnumerator OrbitAnimation()
    {
        float timer = 0f;

        while (timer < GetSpinDuration() && player != null)
        {
            timer += Time.deltaTime;
            for (int i = 0; i < orbitingWeapons.Count; i++)
            {
                if (orbitingWeapons[i] == null) continue;

                float baseAngle = (timer * orbitSpeed) % 360f;
                float weaponAngle = baseAngle + (i * (360f / GetWeaponCount()));
                Vector3 offset = new Vector3(
                    Mathf.Cos(weaponAngle * Mathf.Deg2Rad) * GetOrbitRadius(),
                    Mathf.Sin(weaponAngle * Mathf.Deg2Rad) * GetOrbitRadius(),
                    0
                );

                orbitingWeapons[i].transform.position = player.position + offset;
                orbitingWeapons[i].transform.rotation = Quaternion.Euler(0, 0, weaponAngle + 90f);
            }

            yield return null;
        }
    }

    IEnumerator ContinuousDamage()
    {
        HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

        while (isSpinning)
        {
            foreach (GameObject weapon in orbitingWeapons)
            {
                if (weapon == null) continue;

                Collider2D[] enemies = Physics2D.OverlapCircleAll(weapon.transform.position, 0.5f);

                foreach (var enemy in enemies)
                {
                    if (enemy.CompareTag("Enemy") && !damagedEnemies.Contains(enemy.gameObject))
                    {
                        DamageEnemy(enemy.gameObject, GetDamage());

                        Vector2 knockbackDir = (enemy.transform.position - player.position).normalized;
                        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
                        if (rb != null)
                        {
                            rb.AddForce(knockbackDir * 8f, ForceMode2D.Impulse);
                        }

                        damagedEnemies.Add(enemy.gameObject);
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    void DestroyOrbitingWeapons()
    {
        foreach (GameObject weapon in orbitingWeapons)
        {
            if (weapon != null) Destroy(weapon);
        }
        orbitingWeapons.Clear();
    }

    // ----------- Upgrade Methods -------------

    public void UpgradeWeapon()
    {
        if (level >= maxLevel)
        {
            Debug.Log("SpinWeapon already max level!");
            return;
        }

        level++;
        Debug.Log($"SpinWeapon upgraded to level {level}");
    }

    private float GetDamage() => damage + (level - 1) * damagePerLevel;
    private float GetOrbitRadius() => orbitRadius + (level - 1) * radiusPerLevel;
    private float GetSpinDuration() => spinDuration + (level - 1) * durationPerLevel;
    private int GetWeaponCount() => weaponCount + (level - 1) * weaponCountPerLevel;

    void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, GetOrbitRadius());
        }
    }
}

// Helper component for weapon damage
public class WeaponDamager : MonoBehaviour
{
    public float damage = 1f;
    public float knockbackForce = 5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Apply damage (you might need to adjust this based on your damage system)
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Apply knockback
            Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}