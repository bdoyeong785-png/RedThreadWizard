using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("근접 공격 (Z 키)")]
    public Transform attackPoint;
    public float attackRange = 1.2f;
    public LayerMask enemyLayer;
    public int meleeDamage = 10;
    public float meleeRate = 0.4f;
    private float nextMeleeTime = 0f;

    [Header("마법 영창 & 발사 (X 키)")]
    public GameObject magicBulletPrefab; // ★Project 창의 MagicBullet 프리팹 연결
    public Transform wandTip;          // ★Player 자식 WandTip 연결
    public float castTime = 0.5f;       // 테스트용: 0.5초만 눌러도 완충되도록 설정
    public float mpCost = 15f;

    private bool isCasting = false;
    private float currentCastTimer = 0f;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Z키 근접 공격
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= nextMeleeTime && !isCasting)
        {
            MeleeAttack();
            nextMeleeTime = Time.time + meleeRate;
        }

        // 2. X키 누르기 시작 (영창 시작)
        if (Input.GetKeyDown(KeyCode.X))
        {
            isCasting = true;
            currentCastTimer = 0f;
            Debug.Log("[시스템] 영창 시작!");
        }

        // 3. X키 누르고 있는 동안 (차징)
        if (Input.GetKey(KeyCode.X) && isCasting)
        {
            currentCastTimer += Time.deltaTime;
            float chargeRatio = Mathf.Clamp01(currentCastTimer / castTime);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, Color.cyan, chargeRatio);
            }
        }

        // 4. X키 뗠 때 발사 (조건 체크 보완)
        if (Input.GetKeyUp(KeyCode.X) && isCasting)
        {
            Debug.Log($"[시스템] X키 뗌! 현재 충전 시간: {currentCastTimer:F2}초 / 목표 시간: {castTime:F2}초");

            if (currentCastTimer >= castTime)
            {
                // 캐릭터 바라보는 방향 (오른쪽: 1, 왼쪽: -1)
                Vector2 facingDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
                ShootMagic(facingDir);
            }
            else
            {
                Debug.LogWarning("[경고] 영창 시간이 부족해서 마법이 취소되었습니다! (X키를 조금 더 길게 눌러보세요)");
            }

            // 초기화
            isCasting = false;
            currentCastTimer = 0f;
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
        }
    }

    void MeleeAttack()
    {
        Debug.Log("몽둥이 휘두르기!");

        // attackPoint 위치 기준으로 attackRange 반지름 내의 enemyLayer 오브젝트 감지
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("근접 타격 성공: " + enemy.name);

            // 감지된 적에서 EnemyBase 스크립트를 가져와 데미지 부여!
            EnemyBase enemyComponent = enemy.GetComponent<EnemyBase>();
            if (enemyComponent != null)
            {
                enemyComponent.TakeDamage(meleeDamage);
            }
        }
    }

    void ShootMagic(Vector2 direction)
    {
        PlayerStats stats = GetComponent<PlayerStats>();

        // PlayerStats가 연결되어 있고 마나가 부족할 때
        if (stats != null && !stats.UseMp(mpCost))
        {
            Debug.LogWarning("[경고] 마나가 부족하여 발사할 수 없습니다!");
            return;
        }

        if (magicBulletPrefab == null)
        {
            Debug.LogError("[에러] MagicBulletPrefab이 Inspector에 연결되지 않았습니다!");
            return;
        }

        if (wandTip == null)
        {
            Debug.LogError("[에러] WandTip이 Inspector에 연결되지 않았습니다!");
            return;
        }

        // 총알 생성 후 방향 전달
        GameObject bulletObj = Instantiate(magicBulletPrefab, wandTip.position, Quaternion.identity);
        MagicBullet bullet = bulletObj.GetComponent<MagicBullet>();

        if (bullet != null)
        {
            bullet.Init(direction);
            Debug.Log("[성공] 총알 발사 성공!");
        }
        else
        {
            Debug.LogError("[에러] 프리팹에 MagicBullet 스크립트가 붙어있지 않습니다!");
        }
    }
}