using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동, 달리기(Shift) 및 점프")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 1.5f; // [신규 추가] Left Shift 누를 때 속도 배율
    public float jumpForce = 10f;         // 화면 천장 밖으로 너무 높게 뜨지 않도록 조정

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
        // 1. 보호막(Ctrl) 또는 영창(X) 중에는 이동, 점프, 구르기 모두 잠금
        if (playerStats != null && (playerStats.isShieldActive || playerStats.isCasting))
        {
            moveInput = 0f;
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (isRolling) return;

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
        if (playerStats != null && (playerStats.isShieldActive || playerStats.isCasting))
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (isRolling) return;

        // [신규 추가] 속도 계산: 슬로우 상태가 아닐 때 Left Shift를 누르면 달리기 적용
        float currentSpeed = moveSpeed;
        if (isSlowed)
        {
            currentSpeed *= slowMultiplier;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(moveInput) > 0.01f)
            {
                currentSpeed *= sprintMultiplier;
            }

            if (playerStats != null && playerStats.isHitSpeedBoosted)
            {
                currentSpeed *= playerStats.hitSpeedMultiplier;
            }
        }

        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        // 바라보는 방향에 따라 좌우 반전 (기존 스케일 크기 유지)
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private IEnumerator RollRoutine()
    {
        isRolling = true;
        canRoll = false;

        float rollDir = moveInput != 0 ? Mathf.Sign(moveInput) : Mathf.Sign(transform.localScale.x);

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }

        Color originColor = sr != null ? sr.color : Color.white;
        if (sr != null) sr.color = new Color(1f, 1f, 1f, 0.4f);

        float timer = 0f;
        while (timer < rollDuration)
        {
            rb.velocity = new Vector2(rollDir * rollSpeed, 0f);
            timer += Time.deltaTime;
            yield return null;
        }

        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }
        if (sr != null) sr.color = originColor;
        isRolling = false;

        isSlowed = true;
        Debug.Log("구르기 종료! 잠시 슬로우 상태이상 적용");
        yield return new WaitForSeconds(slowDuration);
        isSlowed = false;

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