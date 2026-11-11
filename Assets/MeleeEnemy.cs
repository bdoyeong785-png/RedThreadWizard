using UnityEngine;

public class MeleeEnemy : EnemyBase
{
    public float moveSpeed = 3f;
    public float detectRange = 8f;

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 감지 범위 내에 들어오면 플레이어를 향해 이동
        if (distanceToPlayer <= detectRange)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += new Vector3(direction.x, 0f, 0f) * moveSpeed * Time.deltaTime;
        }
    }
}