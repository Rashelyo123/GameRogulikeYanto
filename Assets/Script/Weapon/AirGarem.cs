using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirGarem : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackInterval = 1f;
    public float damage = 2f;
    public float attackRange = 2f;
    public float attackAngle = 60f;
    public float slashOffset = 0.3f;
    public LayerMask enemyLayer;
    [SerializeField] private PlayerMana manaSystem;

    [Header("Spawn References")]
    public Transform attackOrigin;
    public GameObject airPrefarbs;
    [SerializeField] private GameObject airGaremUltimatePrefarb;

    [Header("Upgrade Settings")]
    public int level = 1;
    public int maxLevel = 5;
    public float criticalChance = 0f; // 0 to 1
    public float sizeMultiplier = 1f; // size scale of slash
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackSound;

    private float timer = 0f;
    void Start()
    {
        manaSystem = FindAnyObjectByType<PlayerMana>();
    }
    void Update()
    {


        if (Input.GetMouseButtonDown(1) && manaSystem.IsManaFull())
        {
            UseUltimate();
            manaSystem.ResetMana();
        }

        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            timer = 0f;
            PerformSliceAttack();
        }
    }

    public void UseUltimate()
    {
        GameObject ultimateInstance = Instantiate(airGaremUltimatePrefarb, attackOrigin.position, Quaternion.identity, attackOrigin);
        manaSystem.ResetMana();
        Destroy(ultimateInstance, 3f); // destroy yang di scene, bukan prefab
    }


    void PerformSliceAttack()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector2 rawDir = (mousePos - attackOrigin.position).normalized;
        Vector2 dir8 = Get8Direction(rawDir);

        float angle = Mathf.Atan2(dir8.y, dir8.x) * Mathf.Rad2Deg;
        Vector3 slashPosition = attackOrigin.position + (Vector3)(dir8 * slashOffset);

        if (airPrefarbs)
        {
            GameObject vfx = Instantiate(airPrefarbs, slashPosition, Quaternion.Euler(0f, 0f, angle), attackOrigin);
            if (audioSource && attackSound)
            {
                audioSource.PlayOneShot(attackSound);
            }

            // Set size
            vfx.transform.localScale *= sizeMultiplier;

            // Handle critical damage
            float finalDamage = damage;
            if (Random.value < criticalChance)
            {
                finalDamage *= 2f; // Critical hit deals double damage
                Debug.Log("CRITICAL HIT!");
            }

            airProjectile projectile = vfx.GetComponentInChildren<airProjectile>();
            if (projectile != null)
            {
                projectile.SetDamage((int)finalDamage);
            }

            Destroy(vfx, 0.5f);
        }
    }

    Vector2 Get8Direction(Vector2 inputDir)
    {
        Vector2[] directions = new Vector2[]
        {
            new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
            new Vector2(-1, 1), new Vector2(-1, 0), new Vector2(-1, -1),
            new Vector2(0, -1), new Vector2(1, -1)
        };

        float maxDot = -Mathf.Infinity;
        Vector2 bestMatch = Vector2.right;

        foreach (Vector2 dir in directions)
        {
            float dot = Vector2.Dot(inputDir.normalized, dir.normalized);
            if (dot > maxDot)
            {
                maxDot = dot;
                bestMatch = dir;
            }
        }

        return bestMatch.normalized;
    }

    // Upgrade logic
    public void UpgradeLevel()
    {
        if (level >= maxLevel) return;

        level++;

        // Increase stats based on level
        damage += 2f;
        criticalChance += 0.1f; // 10% per level
        sizeMultiplier += 0.2f;

        Debug.Log($"Upgraded to Level {level} | Damage: {damage} | Crit: {criticalChance * 100}% | Size: x{sizeMultiplier}");
    }

    void OnDrawGizmosSelected()
    {
        if (attackOrigin)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackOrigin.position, slashOffset);
        }
    }
}
