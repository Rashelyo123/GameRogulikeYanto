using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float maxHealth = 3f;
    public float moveSpeed = 2f;
    public float damage = 1f;
    [SerializeField] private EnemyData enemyData;

    [Space]
    [SerializeField] private GameObject xpOrbPrefab;

    [Header("Cleanup")]
    public float maxDistanceFromPlayer = 20f;

    [Header("Death Animation")]
    public float knockbackForce = 1f;
    public float fadeTime = 0.8f;

    private float currentHealth;
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isDying = false;

    void Start()
    {
        // Pastikan enemyData ada
        if (enemyData != null)
        {
            currentHealth = enemyData.maxHealth;
            maxHealth = enemyData.maxHealth;
            moveSpeed = enemyData.moveSpeed;
            damage = enemyData.damage;
        }
        else
        {
            currentHealth = maxHealth;
        }

        // Initialize components
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        rb.gravityScale = 0f;

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        StartCoroutine(CheckDistanceFromPlayer());
    }

    void FixedUpdate()
    {
        if (!isDying)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;

            // Flip sprite berdasarkan direction
            spriteRenderer.flipX = direction.x > 0;
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDying) return; // Prevent taking damage while dying

        currentHealth -= damageAmount;
        Vector3 textPosition = transform.position + Vector3.up * 1.5f;
        FloatingText.Create(damageAmount.ToString(), textPosition, Color.white);

        // Visual feedback
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDying) return; // Prevent multiple death calls
        isDying = true;

        // Update UI
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.OnEnemyKilled();
        }

        // Spawn XP orb
        if (xpOrbPrefab != null)
        {
            GameObject orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
            XPOrb orbScript = orb.GetComponent<XPOrb>();
            if (orbScript != null && enemyData != null)
            {
                orbScript.xpValue = enemyData.xpDropAmount;
            }
        }

        // Start death animation
        StartCoroutine(DeathAnimation());

        Debug.Log("Enemy died!");
    }

    IEnumerator DeathAnimation()
    {
        // Disable collision
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Knockback away from player (more predictable)
        Vector2 knockbackDirection = Vector2.zero;
        if (player != null)
        {
            // Push away from player
            knockbackDirection = (transform.position - player.position).normalized;
            // Add slight random variation (much smaller)
            knockbackDirection += new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.2f));
            knockbackDirection = knockbackDirection.normalized;
        }
        else
        {
            // Fallback random direction if no player
            knockbackDirection = new Vector2(Random.Range(-1f, 1f), 0f).normalized;
        }

        // Apply knockback force
        rb.velocity = knockbackDirection * knockbackForce;

        // Fade out over time
        float timer = 0f;
        Color originalColor = spriteRenderer.color;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // Gradually reduce knockback
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, Time.deltaTime * 3f);

            yield return null;
        }

        // Destroy the enemy
        Destroy(gameObject);
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer != null && !isDying)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;
        }
    }

    IEnumerator CheckDistanceFromPlayer()
    {
        while (!isDying)
        {
            yield return new WaitForSeconds(2f); // Check every 2 seconds

            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.position);

                // Destroy enemy jika terlalu jauh (optimization)
                if (distance > maxDistanceFromPlayer)
                {
                    Destroy(gameObject);
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return; // Don't damage player while dying

        // Damage player saat collision
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player hit by enemy!");
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            playerHealth?.TakeDamage(damage);
        }
    }
}