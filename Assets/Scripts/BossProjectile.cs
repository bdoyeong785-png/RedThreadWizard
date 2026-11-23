using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float damage = 15f;
    public float lifeTime = 4f;

    void Start() => Destroy(gameObject, lifeTime);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null) stats.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}