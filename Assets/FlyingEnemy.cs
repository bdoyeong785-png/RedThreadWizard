using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    public float moveSpeed = 2.5f;
    public float detectRange = 10f;
    public float hoverHeight = 1.5f; // 플레이어 머리 위 살짝 높은 위치 추적

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= detectRange)
        {
            // 플레이어 머리 위쪽 목표 지점 설정
            Vector3 targetPos = playerTransform.position + Vector3.up * hoverHeight;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }
    }
}