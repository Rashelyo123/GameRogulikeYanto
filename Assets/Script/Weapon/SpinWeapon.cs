using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinAttackWeapon : BaseWeapon
{
    [Header("Spin Settings")]
    public GameObject spinEffectPrefab;
    public float spinDuration = 1f;

    [Header("Orbit Animation")]
    public Transform player; // Reference to player
    public float orbitRadius = 2f;
    public float orbitSpeed = 360f; // degrees per second
    public int weaponCount = 1; // How many weapons orbit around player

    [Header("Visual Effects")]
    public bool showTrail = true;
    public Color trailColor = Color.white;
    public float trailWidth = 0.1f;

    private bool isSpinning = false;
    private List<GameObject> orbitingWeapons = new List<GameObject>();

    void Start()
    {
        // Auto-assign player if not set
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    protected override void PerformAttack()
    {
        // Make sure player is assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (!isSpinning && player != null)
        {
            StartCoroutine(PerformSpinAttack());
        }
        else if (player == null)
        {
            Debug.LogWarning("SpinAttackWeapon: Player reference is null! Make sure Player GameObject has 'Player' tag.");
        }
    }

    protected virtual IEnumerator PerformSpinAttack()
    {
        isSpinning = true;

        // Create orbiting weapons
        CreateOrbitingWeapons();

        // Start orbit animation
        StartCoroutine(OrbitAnimation());

        // Continuously damage enemies during spin
        StartCoroutine(ContinuousDamage());

        // Wait for spin duration
        yield return new WaitForSeconds(spinDuration);

        // Clean up
        DestroyOrbitingWeapons();
        isSpinning = false;
    }

    void CreateOrbitingWeapons()
    {
        if (player == null)
        {
            Debug.LogError("SpinAttackWeapon: Player reference is null in CreateOrbitingWeapons!");
            return;
        }

        for (int i = 0; i < weaponCount; i++)
        {
            GameObject weapon;

            // Use spinEffectPrefab if available, otherwise create simple weapon
            if (spinEffectPrefab != null)
            {
                weapon = Instantiate(spinEffectPrefab, player.position, Quaternion.identity);
            }
            else
            {
                // Create simple weapon representation
                weapon = CreateSimpleWeapon();
            }

            // Add trail effect
            if (showTrail)
            {
                AddTrailEffect(weapon);
            }

            // Add collider for damage detection
            if (weapon.GetComponent<Collider2D>() == null)
            {
                CircleCollider2D collider = weapon.AddComponent<CircleCollider2D>();
                collider.isTrigger = true;
                collider.radius = 0.5f;
            }

            // Add weapon component for damage
            WeaponDamager damager = weapon.AddComponent<WeaponDamager>();
            damager.damage = damage;
            damager.knockbackForce = 8f;

            orbitingWeapons.Add(weapon);
        }
    }

    GameObject CreateSimpleWeapon()
    {
        GameObject weapon = new GameObject("OrbitingWeapon");

        // Add sprite renderer
        SpriteRenderer sr = weapon.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>()?.sprite; // Use same sprite as main weapon
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

        while (timer < spinDuration && player != null)
        {
            timer += Time.deltaTime;

            for (int i = 0; i < orbitingWeapons.Count; i++)
            {
                if (orbitingWeapons[i] != null)
                {
                    // Calculate angle for this weapon
                    float baseAngle = (timer * orbitSpeed) % 360f;
                    float weaponAngle = baseAngle + (i * (360f / weaponCount));

                    // Calculate position
                    Vector3 offset = new Vector3(
                        Mathf.Cos(weaponAngle * Mathf.Deg2Rad) * orbitRadius,
                        Mathf.Sin(weaponAngle * Mathf.Deg2Rad) * orbitRadius,
                        0
                    );

                    orbitingWeapons[i].transform.position = player.position + offset;

                    // Rotate weapon to face movement direction
                    orbitingWeapons[i].transform.rotation = Quaternion.Euler(0, 0, weaponAngle + 90f);
                }
            }

            yield return null;
        }
    }

    IEnumerator ContinuousDamage()
    {
        HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

        while (isSpinning)
        {
            // Check for enemies in range of each orbiting weapon
            foreach (GameObject weapon in orbitingWeapons)
            {
                if (weapon != null)
                {
                    Collider2D[] enemies = Physics2D.OverlapCircleAll(weapon.transform.position, 0.5f);

                    foreach (var enemy in enemies)
                    {
                        if (enemy.CompareTag("Enemy") && !damagedEnemies.Contains(enemy.gameObject))
                        {
                            DamageEnemy(enemy.gameObject, damage);

                            Vector2 knockbackDir = (enemy.transform.position - player.position).normalized;
                            // ApplyKnockback(enemy.gameObject, knockbackDir, 8f);

                            damagedEnemies.Add(enemy.gameObject);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
        }
    }

    void DestroyOrbitingWeapons()
    {
        foreach (GameObject weapon in orbitingWeapons)
        {
            if (weapon != null)
            {
                Destroy(weapon);
            }
        }
        orbitingWeapons.Clear();
    }

    void OnDrawGizmosSelected()
    {
        // Auto-assign player if not set (for editor)
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player != null)
        {
            // Draw orbit radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(player.position, orbitRadius);
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