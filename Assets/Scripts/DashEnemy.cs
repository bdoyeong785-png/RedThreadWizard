using UnityEngine;

public class DashEnemy : EnemyBase
{
    public float normalSpeed = 1.5f;
    public float dashSpeed = 8f;
    public float dashRange = 5f;
    public float dashCooldown = 3f;

    private bool isDashing = false;
    private float lastDashTime = -999f;
    private Vector3 dashDirection;
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (isDashing)
        {
            // 강습 진행 중
            transform.position += dashDirection * dashSpeed * Time.deltaTime;
        }
        else
        {
            // 사거리 진입 및 쿨타임 완료 시 강습 시작
            if (distance <= dashRange && Time.time >= lastDashTime + dashCooldown)
            {
                StartDash();
            }
            else
            {
                // 일반 이동
                Vector3 dir = (playerTransform.position - transform.position).normalized;
                transform.position += new Vector3(dir.x, 0f, 0f) * normalSpeed * Time.deltaTime;
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        dashDirection = (playerTransform.position - transform.position).normalized;

        if (spriteRenderer != null) spriteRenderer.color = Color.yellow; // 강습 예고 (노란색)

        Invoke(nameof(StopDash), 0.6f); // 0.6초간 강습 유지
    }

    void StopDash()
    {
        isDashing = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }
}