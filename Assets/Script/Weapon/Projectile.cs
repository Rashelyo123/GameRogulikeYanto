using UnityEngine;
using System.Collections.Generic;

// Script untuk projectile
public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float damage = 1f;
    public float lifetime = 3f;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        // Destroy projectile setelah lifetime
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        // Move projectile
        rb.velocity = direction * speed;
    }

    public void Initialize(Vector2 shootDirection, float projectileDamage)
    {
        direction = shootDirection.normalized;
        damage = projectileDamage;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check jika hit enemy
        if (other.CompareTag("Enemy"))
        {
            // Deal damage ke enemy (akan kita buat nanti)
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // Destroy projectile
            Destroy(gameObject);
        }
    }
}
