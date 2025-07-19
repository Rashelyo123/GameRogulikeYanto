using UnityEngine;

public class airProjectile : MonoBehaviour
{
    [SerializeField] private int damage;

    public void SetDamage(int newDamage)
    {
        damage = newDamage;
        Debug.Log("Damage set to: " + damage);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(damage);
        }
    }
}