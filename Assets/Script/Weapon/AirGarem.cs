using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirGarem : MonoBehaviour
{
    public float attackInterval = 1f;
    public float damage = 10f;
    public float attackRange = 2f;
    public float attackAngle = 60f; // For damage cone
    public float slashOffset = 0.3f; // Distance from origin to spawn slash
    public LayerMask enemyLayer;
    public Transform attackOrigin;        // Child transform as slash origin
    public GameObject airPrefarbs;     // Prefab for slash visual
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= attackInterval)
        {
            timer = 0f;
            PerformSliceAttack();
        }


    }

    void PerformSliceAttack()
    {
        // Get mouse world position
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        // Calculate direction to mouse, then snap to 8 directions
        Vector2 rawDir = (mousePos - attackOrigin.position).normalized;
        Vector2 dir8 = Get8Direction(rawDir);

        // Set angle from direction
        float angle = Mathf.Atan2(dir8.y, dir8.x) * Mathf.Rad2Deg;

        // Calculate slash position based on direction
        Vector3 slashPosition = attackOrigin.position + (Vector3)(dir8 * slashOffset);

        // Spawn slash at calculated position, rotated towards mouse direction
        if (airPrefarbs)
        {
            GameObject vfx = Instantiate(airPrefarbs, slashPosition, Quaternion.Euler(0f, 0f, angle), attackOrigin);
            airProjectile projectile = vfx.GetComponentInChildren<airProjectile>();
            if (projectile != null)
            {
                projectile.SetDamage((int)damage);
            }
            Destroy(vfx, 0.5f);
        }


    }

    Vector2 Get8Direction(Vector2 inputDir)
    {
        Vector2[] directions = new Vector2[]
        {
            new Vector2(1, 0),    // Right
            new Vector2(1, 1),    // Up-Right
            new Vector2(0, 1),    // Up
            new Vector2(-1, 1),   // Up-Left
            new Vector2(-1, 0),   // Left
            new Vector2(-1, -1),  // Down-Left
            new Vector2(0, -1),   // Down
            new Vector2(1, -1),   // Down-Right
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

    void OnDrawGizmosSelected()
    {
        if (attackOrigin)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackOrigin.position, attackRange);

            // Draw slash offset visualization
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackOrigin.position, slashOffset);
        }
    }
}
