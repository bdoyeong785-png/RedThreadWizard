using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("적 탄환 속성")]
    public float speed = 8f;
    public float damage = 15f;
    public float lifetime = 4f;

    private Vector3 moveDirection;
    private bool isFired = false;

    public void Init(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // 날아가는 각도에 맞춰 총알 이미지 회전
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        isFired = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isFired)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 같은 적이나 적 총알끼리는 무시
        if (collision.CompareTag("Enemy") || collision.GetComponent<EnemyBullet>() != null)
            return;

        // 플레이어(또는 보호막 상태인 플레이어) 타격
        if (collision.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // 바닥이나 벽에 닿으면 소멸
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}