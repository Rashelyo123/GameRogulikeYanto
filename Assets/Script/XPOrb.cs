using UnityEngine;
using System.Collections;

// Script untuk XP Orb yang drop dari enemy dengan Object Pooling
public class XPOrb : MonoBehaviour
{
    [Header("XP Settings")]
    public float xpValue = 1f;
    public float attractDistance = 3f;
    public float attractSpeed = 8f;
    public float floatSpeed = 1f;
    public float floatHeight = 0.5f;
    public float lifeTime = 30f;

    private Transform player;
    private bool isAttracted = false;
    private Vector3 startPosition;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // Object Pooling
    private XPOrbManager orbManager;
    private bool isPooled = false;
    private Coroutine lifeTimeCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            rb.sleepMode = RigidbodySleepMode2D.StartAsleep;
        }
    }

    public void Initialize(XPOrbManager manager, float xpAmount, Vector3 spawnPosition)
    {
        orbManager = manager;
        isPooled = true;
        xpValue = xpAmount;

        // Reset state
        transform.position = spawnPosition;
        startPosition = spawnPosition;
        isAttracted = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.WakeUp();
        }

        // Reset sprite
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
        }

        // Find player
        FindPlayer();

        // Start lifetime countdown
        if (lifeTimeCoroutine != null)
            StopCoroutine(lifeTimeCoroutine);
        lifeTimeCoroutine = StartCoroutine(LifeTimeCountdown());
    }

    void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isAttracted && distanceToPlayer <= attractDistance)
        {
            isAttracted = true;
        }

        if (isAttracted)
        {
            // Move towards player
            Vector2 direction = (player.position - transform.position).normalized;
            if (rb != null)
            {
                rb.velocity = direction * attractSpeed;
            }
            else
            {
                // Fallback movement without rigidbody
                transform.position += (Vector3)direction * attractSpeed * Time.deltaTime;
            }
        }
        else
        {
            // Float up and down
            if (rb != null)
                rb.velocity = Vector2.zero;

            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    IEnumerator LifeTimeCountdown()
    {
        yield return new WaitForSeconds(lifeTime);

        // Return to pool instead of destroy
        ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Give XP to player
            ExperienceManager expManager = other.GetComponent<ExperienceManager>();
            if (expManager != null)
            {
                expManager.GainXP(xpValue);
            }

            // Return to pool instead of destroy
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        // Stop lifetime coroutine
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
            lifeTimeCoroutine = null;
        }

        // Reset state
        isAttracted = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.Sleep();
        }

        if (isPooled && orbManager != null)
        {
            orbManager.ReturnOrbToPool(gameObject);
        }
        else
        {
            // Fallback for non-pooled orbs
            Destroy(gameObject);
        }
    }

    public void ForceReturn()
    {
        ReturnToPool();
    }

    void OnDisable()
    {
        // Clean up when disabled
        if (lifeTimeCoroutine != null)
        {
            StopCoroutine(lifeTimeCoroutine);
            lifeTimeCoroutine = null;
        }
    }
}