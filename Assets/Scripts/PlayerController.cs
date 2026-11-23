using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프")]
    public float moveSpeed = 8f;
    public float jumpForce = 13f;

    [Header("구르기 (C키 - 적 통과 & 슬로우)")]
    public float rollSpeed = 16f;
    public float rollDuration = 0.25f;
    public float rollCooldown = 1.2f;
    public float slowDuration = 0.6f;     // 구르기 후 슬로우 지속시간
    public float slowMultiplier = 0.5f;   // 슬로우 시 속도 배율 (50%)

    [Header("지면 감지")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerStats playerStats;

    private float moveInput;
    private bool isGrounded;
    private bool isRolling = false;
    private bool canRoll = true;
    private bool isSlowed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        playerStats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        // 1. 보호막(Shift) 또는 영창(X) 중에는 이동, 점프, 구르기 모두 잠금 (제자리 고정)
        if (playerStats != null && (playerStats.isShieldActive || playerStats.isCasting))
        {
            moveInput = 0f;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (isRolling) return; // 구르기 중에는 조작 잠금

        // 2. 좌우 이동 입력 (A/D 또는 화살표)
        moveInput = Input.GetAxisRaw("Horizontal");

        // 3. 바닥 착지 체크
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 4. 점프 (Space)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 5. 구르기 / 대시 (C키)
        if (Input.GetKeyDown(KeyCode.C) && canRoll)
        {
            StartCoroutine(RollRoutine());
        }
    }

    void FixedUpdate()
    {
        // 보호막이나 영창 중이면 물리 이동도 완전히 정지
        if (playerStats != null && (playerStats.isShieldActive || playerStats.isCasting))
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (isRolling) return;

        // 속도 계산 (구르기 후 슬로우 vs 피격 후 이속 증가)
        float currentSpeed = moveSpeed;
        if (isSlowed)
            currentSpeed *= slowMultiplier;
        else if (playerStats != null && playerStats.isHitSpeedBoosted)
            currentSpeed *= playerStats.hitSpeedMultiplier;

        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        // 바라보는 방향에 따라 좌우 반전
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // 구르기 코루틴 (적 레이어 통과 무적 + 구르기 후 슬로우)
    private IEnumerator RollRoutine()
    {
        isRolling = true;
        canRoll = false;

        // 입력 방향이 없으면 현재 바라보는 방향으로 대시
        float rollDir = moveInput != 0 ? Mathf.Sign(moveInput) : Mathf.Sign(transform.localScale.x);

        // 1. Player와 Enemy 레이어 간 물리 충돌을 꺼서 적을 통과
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }

        Color originColor = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.4f);

        // 2. 대시 이동 실행 (오타 수정 완료)
        float timer = 0f;
        while (timer < rollDuration)
        {
            rb.velocity = new Vector2(rollDir * rollSpeed, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        // 3. 무적 해제 및 적 충돌 복구
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }
        if (sr != null) sr.color = originColor;
        isRolling = false;

        // 4. 구르기 직후 슬로우 디버프 발동
        isSlowed = true;
        Debug.Log("구르기 종료! 잠시 슬로우 상태이상 적용");
        yield return new WaitForSeconds(slowDuration);
        isSlowed = false;

        // 5. 남은 쿨타임 대기
        yield return new WaitForSeconds(Mathf.Max(0f, rollCooldown - slowDuration));
        canRoll = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}