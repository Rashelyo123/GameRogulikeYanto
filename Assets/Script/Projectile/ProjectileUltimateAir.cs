using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileUltimateAir : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float shakeAmount = 0.4f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {

            collision.GetComponent<Enemy>().TakeDamage(damage);

        }
    }

    public void OnEnableCollider()
    {
        GetComponent<Collider2D>().enabled = true;
    }
}
