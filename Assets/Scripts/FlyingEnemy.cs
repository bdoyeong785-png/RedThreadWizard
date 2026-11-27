using UnityEngine;

public class FlyingEnemy : EnemyBase
{
    [Header("공중 이동 설정")]
    public float moveSpeed = 2.5f;
    public float detectRange = 12f;
    public float hoverHeight = 2.5f; // 플레이어 머리 위 띄울 높이

    [Header("원거리 공격 설정")]
    public GameObject enemyBulletPrefab; // ★ 적 총알 프리팹 연결
    public Transform firePoint;          // ★ 총알이 나갈 입/지팡이 위치 (비워두면 몸 중심에서 발사)
    public float attackRange = 8f;       // 총알을 쏘기 시작하는 거리
    public float fireCooldown = 2.5f;    // 발사 간격(초)

    private float nextFireTime = 0f;

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= detectRange)
        {
            // 플레이어 머리 위쪽 목표 지점으로 이동
            Vector3 targetPos = playerTransform.position + Vector3.up * hoverHeight;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            // 플레이어 방향 바라보기 (좌우 반전)
            float dirX = playerTransform.position.x - transform.position.x;
            if (Mathf.Abs(dirX) > 0.05f)
            {
                float sign = Mathf.Sign(dirX);
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * sign, transform.localScale.y, transform.localScale.z);
            }

            // 공격 사거리 안이고 쿨타임이 찼으면 총알 발사
            if (distance <= attackRange && Time.time >= nextFireTime)
            {
                ShootAtPlayer();
                nextFireTime = Time.time + fireCooldown;
            }
        }
    }

    void ShootAtPlayer()
    {
        if (enemyBulletPrefab == null)
        {
            Debug.LogWarning("[FlyingEnemy] EnemyBulletPrefab이 연결되지 않았습니다!");
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 shootDir = (playerTransform.position - spawnPos).normalized;

        GameObject bulletObj = Instantiate(enemyBulletPrefab, spawnPos, Quaternion.identity);
        EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();
        if (bullet != null)
        {
            bullet.Init(shootDir);
        }
    }
}