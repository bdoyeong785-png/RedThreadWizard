using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("탄환 속성")]
    public float speed = 20f;
    public int damage = 35; // 영창 마법은 근접(10)보다 강력하게 설정!
    public float lifetime = 3f;

    private Vector3 moveVector;
    private bool isFired = false;

    public void Init(Vector2 direction)
    {
        moveVector = new Vector3(direction.x, 0f, 0f);

        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        isFired = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isFired)
        {
            transform.position += moveVector * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            return;

        // 1. 일반 몬스터 타격
        EnemyBase enemy = collision.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 2. 보스 타격 (보스는 스턴 면역이므로 데미지만 적용)
        BossController boss = collision.GetComponent<BossController>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 3. 바닥/벽 충돌
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}