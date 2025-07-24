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
    private SpriteRenderer spriteRenderer;
    private bool isDying = false;

    // Object Pooling Support
    private EnemySpawner spawner;
    private GameObject originalPrefab;
    private bool isPooled = false;
    private Coroutine distanceCheckCoroutine;

    // Movement without Rigidbody
    private Vector2 currentVelocity = Vector2.zero;

    #region Initialization
    void Start()
    {
        InitializeEnemy();
    }

    public void InitializeForPooling(EnemySpawner enemySpawner, GameObject prefab)
    {
        spawner = enemySpawner;
        originalPrefab = prefab;
        isPooled = true;

        InitializeEnemy();
    }

    void InitializeEnemy()
    {
        // Reset state
        isDying = false;
        currentVelocity = Vector2.zero;

        // Setup enemy data
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
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // Reset sprite
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        }

        // Re-enable collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true; // Always use trigger for performance
        }

        // Find player
        FindPlayer();

        // Start distance checking
        if (distanceCheckCoroutine != null)
            StopCoroutine(distanceCheckCoroutine);
        distanceCheckCoroutine = StartCoroutine(CheckDistanceFromPlayer());
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    #endregion

    #region Movement (No Rigidbody)
    void Update()
    {
        if (!isDying)
        {
            MoveTowardsPlayer();
        }
        else
        {
            // Apply knockback velocity during death
            ApplyVelocity();
        }
    }

    void MoveTowardsPlayer()
    {
        if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            currentVelocity = direction * moveSpeed;

            // Apply movement
            ApplyVelocity();

            // Flip sprite berdasarkan direction
            if (spriteRenderer != null)
                spriteRenderer.flipX = direction.x > 0;
        }
    }

    void ApplyVelocity()
    {
        // Move using transform (no physics)
        transform.position += (Vector3)currentVelocity * Time.deltaTime;
    }
    #endregion

    #region Damage & Death
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

        // Stop distance checking
        if (distanceCheckCoroutine != null)
        {
            StopCoroutine(distanceCheckCoroutine);
            distanceCheckCoroutine = null;
        }

        // Update UI
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.OnEnemyKilled();
        }

        // Spawn XP orb
        SpawnXPOrb();

        // Handle death based on pooling status
        if (isPooled)
        {
            StartCoroutine(DeathAnimationPooledNoRB());
        }
        else
        {
            StartCoroutine(DeathAnimationNoRB());
        }
    }

    void SpawnXPOrb()
    {
        if (enemyData != null)
        {
            // Use XP Orb Manager for pooled spawning
            if (XPOrbManager.Instance != null)
            {
                XPOrbManager.Instance.SpawnXPOrb(transform.position, enemyData.xpDropAmount);
            }
            else
            {
                // Fallback: traditional instantiate method
                if (xpOrbPrefab != null)
                {
                    GameObject orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
                    XPOrb orbScript = orb.GetComponent<XPOrb>();
                    if (orbScript != null)
                    {
                        orbScript.xpValue = enemyData.xpDropAmount;
                    }
                }
            }
        }
        else
        {
            // Fallback: use default XP value
            if (XPOrbManager.Instance != null)
            {
                XPOrbManager.Instance.SpawnXPOrb(transform.position, 1f);
            }
            else if (xpOrbPrefab != null)
            {
                Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
            }
        }
    }
    #endregion

    #region Death Animations (No Rigidbody)
    IEnumerator DeathAnimationNoRB()
    {
        // Original death animation for non-pooled enemies (no RB)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Vector2 knockbackDirection = GetKnockbackDirection();
        Vector2 startPosition = transform.position;

        // Set knockback velocity
        currentVelocity = knockbackDirection * knockbackForce;

        float timer = 0f;
        Color originalColor = spriteRenderer.color;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeTime;

            // Fade alpha
            float alpha = Mathf.Lerp(1f, 0f, progress);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // Reduce knockback velocity over time
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, Time.deltaTime * 3f);

            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator DeathAnimationPooledNoRB()
    {
        // Modified death animation for pooled enemies (no RB)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Vector2 knockbackDirection = GetKnockbackDirection();
        Vector2 startPosition = transform.position;

        // Set knockback velocity
        currentVelocity = knockbackDirection * knockbackForce;

        float timer = 0f;
        Color originalColor = spriteRenderer.color;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeTime;

            // Fade alpha
            float alpha = Mathf.Lerp(1f, 0f, progress);
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            // Reduce knockback velocity over time
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, Time.deltaTime * 3f);

            yield return null;
        }

        // Reset velocity before returning to pool
        currentVelocity = Vector2.zero;

        // Return to pool instead of destroying
        ReturnToPool();
    }

    Vector2 GetKnockbackDirection()
    {
        Vector2 knockbackDirection = Vector2.zero;
        if (player != null)
        {
            knockbackDirection = (transform.position - player.position).normalized;
            knockbackDirection += new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.2f, 0.2f));
            knockbackDirection = knockbackDirection.normalized;
        }
        else
        {
            knockbackDirection = new Vector2(Random.Range(-1f, 1f), 0f).normalized;
        }
        return knockbackDirection;
    }
    #endregion

    #region Visual Effects
    IEnumerator FlashRed()
    {
        if (spriteRenderer != null && !isDying)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            if (spriteRenderer != null) // Check if still exists
                spriteRenderer.color = original;
        }
    }
    #endregion

    #region Cleanup & Pooling
    IEnumerator CheckDistanceFromPlayer()
    {
        while (!isDying)
        {
            yield return new WaitForSeconds(2f);

            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.position);

                if (distance > maxDistanceFromPlayer)
                {
                    if (isPooled)
                    {
                        ReturnToPool();
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                    break;
                }
            }
        }
    }

    void ReturnToPool()
    {
        // Stop all coroutines
        StopAllCoroutines();

        // Reset state
        isDying = false;
        currentVelocity = Vector2.zero;

        if (spawner != null && originalPrefab != null)
        {
            spawner.ReturnEnemyToPool(gameObject, originalPrefab);
        }
        else
        {
            // Fallback: destroy if can't return to pool
            Destroy(gameObject);
        }
    }

    public void ForceReturnToPool()
    {
        ReturnToPool();
    }
    #endregion

    #region Collision
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            playerHealth?.TakeDamage(damage);
        }
    }
    #endregion

    #region Unity Events
    void OnDisable()
    {
        // Clean up when object is disabled
        if (distanceCheckCoroutine != null)
        {
            StopCoroutine(distanceCheckCoroutine);
            distanceCheckCoroutine = null;
        }
        currentVelocity = Vector2.zero;
    }

    void OnDestroy()
    {
        // Clean up when object is destroyed
        if (distanceCheckCoroutine != null)
        {
            StopCoroutine(distanceCheckCoroutine);
            distanceCheckCoroutine = null;
        }
    }
    #endregion
}