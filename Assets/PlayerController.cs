using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 및 점프")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("구르기 (회피)")]
    public float rollSpeed = 15f;
    public float rollDuration = 0.3f;
    public float rollCooldown = 1f;
    public float slowDuration = 0.5f;
    public float slowMultiplier = 0.5f;

    [Header("지면 감지")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;

    private float moveInput;
    private bool isGrounded;
    private bool isRolling = false;
    private bool canRoll = true;
    private bool isSlowed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        if (isRolling) return; // 구르기 중에는 일반 이동 및 점프 입력 방지

        // 1. A/D 및 화살표 좌우 이동 입력
        moveInput = Input.GetAxisRaw("Horizontal");

        // 2. 바닥 착지 여부 확인
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 3. 점프 (Space)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // 4. 구르기 (C 키)
        if (Input.GetKeyDown(KeyCode.C) && canRoll)
        {
            StartCoroutine(RollRoutine());
        }
    }

    // PlayerController.cs 내 이동 로직 부분
    void FixedUpdate()
    {
        if (isRolling) return;

        float currentSpeed = isSlowed ? moveSpeed * slowMultiplier : moveSpeed;
        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        // 바라보는 방향 전환 (Scale.x 변경)
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // 구르기 코루틴 (무적 + 슬로우 연출)
    private IEnumerator RollRoutine()
    {
        isRolling = true;
        canRoll = false;

        // 진행 방향이 없으면 바라보는 기본 방향 사용
        float rollDirection = moveInput != 0 ? Mathf.Sign(moveInput) : transform.localScale.x;

        // 무적 상태 (적 레이어와의 충돌을 시뮬레이션하기 위해 피격 레이어 비활성화 가능)
        // 코드로 레이어 무적을 적용하는 기본적인 연출:
        Color originalColor = GetComponent<SpriteRenderer>().color;
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f); // 반투명 무적 연출

        float timer = 0f;
        while (timer < rollDuration)
        {
            rb.velocity = new Vector2(rollDirection * rollSpeed, 0f); // 일정한 속도로 구르기
            timer += Time.deltaTime;
            yield return null;
        }

        // 구르기 종료 후 무적 해제
        GetComponent<SpriteRenderer>().color = originalColor;
        isRolling = false;

        // 기획서 반영: 구르기 직후 이동 속도 슬로우 디버프
        isSlowed = true;
        yield return new WaitForSeconds(slowDuration);
        isSlowed = false;

        // 쿨타임 대기
        yield return new WaitForSeconds(rollCooldown - slowDuration);
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