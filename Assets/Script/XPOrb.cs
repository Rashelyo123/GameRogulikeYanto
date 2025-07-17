using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Script untuk XP Orb yang drop dari enemy
public class XPOrb : MonoBehaviour
{
    [Header("XP Settings")]
    public float xpValue = 1f;
    public float attractDistance = 3f;
    public float attractSpeed = 8f;
    public float floatSpeed = 1f;
    public float floatHeight = 0.5f;

    private Transform player;
    private bool isAttracted = false;
    private Vector3 startPosition;
    private Rigidbody2D rb;

    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        startPosition = transform.position;

        // Auto destroy after some time
        Destroy(gameObject, 30f);
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isAttracted && distanceToPlayer <= attractDistance)
        {
            isAttracted = true;
        }

        if (isAttracted)
        {
            // Move towards player
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * attractSpeed;
        }
        else
        {
            // Float up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
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

            Destroy(gameObject);
        }
    }
}