using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("보스 기본 스탯 (일반 몬스터의 2~3배 체급)")]
    public string bossName = "붉은 실의 수호자";
    public float maxHp = 500f;
    public float currentHp;
    public float contactDamage = 20f;
    public bool isImmuneToStun = true; // 보스는 스턴 면역

    [Header("플레이어 자동 감지 (거리 안으로 오면 바로 전투 시작!)")]
    public float detectRange = 14f;

    [Header("전투 패턴 설정")]
    public float patternInterval = 2.0f;   // 패턴 사이 휴식 시간 (영창 타이밍)
    public float dashSpeed = 16f;          // 강습 돌진 속도
    public GameObject bossBulletPrefab;    // 보스가 쏘는 탄막 프리팹
    public Transform firePoint;            // 탄막 발사 위치 (비워두면 보스 몸에서 발사)
    public float bulletSpeed = 10f;

    [Header("클리어 시 열릴 벽")]
    public GameObject exitBlockerWall;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isBattleStarted = false;
    private bool isActing = false;

    void Awake()
    {
        currentHp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            // 혹시 Player 태그 설정을 깜빡했어도 PlayerCombat 스크립트로 자동 탐색!
            PlayerCombat combat = FindObjectOfType<PlayerCombat>();
            if (combat != null) player = combat.transform;
        }
    }

    void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // 트리거를 안 밟았더라도 플레이어가 detectRange 안으로 들어오면 자동으로 전투 시작!
        if (!isBattleStarted)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= detectRange)
            {
                StartBossBattle();
            }
        }
    }

    public void StartBossBattle()
    {
        if (isBattleStarted) return;
        isBattleStarted = true;
        Debug.Log($"★ [{bossName}] 전투 시작! 패턴 루프를 가동합니다.");
        StartCoroutine(BossPatternLoop());
    }

    private IEnumerator BossPatternLoop()
    {
        yield return new WaitForSeconds(0.8f);

        while (currentHp > 0 && player != null)
        {
            if (!isActing)
            {
                int randomPattern = Random.Range(0, 3);
                switch (randomPattern)
                {
                    case 0:
                        Debug.Log("[보스 패턴 1] 부채꼴 탄막 발사!");
                        yield return StartCoroutine(Pattern_SpreadShot());
                        break;
                    case 1:
                        Debug.Log("[보스 패턴 2] 고속 돌진!");
                        yield return StartCoroutine(Pattern_ChargeDash());
                        break;
                    case 2:
                        Debug.Log("[보스 패턴 3] 점프 내려찍기!");
                        yield return StartCoroutine(Pattern_JumpSlam());
                        break;
                }
            }

            yield return new WaitForSeconds(patternInterval);
        }
    }

    // [패턴 1] 원거리 부채꼴 탄막 3발
    private IEnumerator Pattern_SpreadShot()
    {
        isActing = true;
        LookAtPlayer();

        if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f);
        yield return new WaitForSeconds(0.5f);
        if (sr != null) sr.color = Color.white;

        if (bossBulletPrefab == null)
        {
            Debug.LogWarning("[보스 경고] Boss Bullet Prefab 칸이 비어 있어서 탄막이 안 나갑니다!");
            isActing = false;
            yield break;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector2 baseDir = (player.position - spawnPos).normalized;

        float[] angles = { -15f, 0f, 15f };
        foreach (float angle in angles)
        {
            Vector2 dir = Quaternion.Euler(0, 0, angle) * baseDir;
            GameObject bullet = Instantiate(bossBulletPrefab, spawnPos, Quaternion.identity);
            Rigidbody2D bRb = bullet.GetComponent<Rigidbody2D>();
            if (bRb != null) bRb.velocity = dir * bulletSpeed;
        }

        isActing = false;
    }

    // [패턴 2] 강습형 고속 돌진
    private IEnumerator Pattern_ChargeDash()
    {
        isActing = true;
        LookAtPlayer();

        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        if (rb != null) rb.velocity = new Vector2(-dirX * 3f, 0f);
        yield return new WaitForSeconds(0.5f);

        float timer = 0f;
        while (timer < 0.65f)
        {
            if (rb != null) rb.velocity = new Vector2(dirX * dashSpeed, rb.velocity.y);
            timer += Time.deltaTime;
            yield return null;
        }

        if (rb != null) rb.velocity = new Vector2(0f, rb.velocity.y);
        isActing = false;
    }

    // [패턴 3] 도약 후 내려찍기
    private IEnumerator Pattern_JumpSlam()
    {
        isActing = true;
        LookAtPlayer();

        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        if (rb != null) rb.velocity = new Vector2(dirX * 7f, 14f);
        yield return new WaitForSeconds(0.8f);

        if (rb != null) rb.velocity = new Vector2(0f, -18f);
        yield return new WaitForSeconds(0.4f);

        isActing = false;
    }

    void LookAtPlayer()
    {
        if (player == null) return;
        float dirX = Mathf.Sign(player.position.x - transform.position.x);
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dirX, transform.localScale.y, transform.localScale.z);
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log($"[보스 피격] {bossName} 남은 HP: {currentHp} / {maxHp}");
        StartCoroutine(DamageFlash());

        if (currentHp <= 0) Die();
    }

    private IEnumerator DamageFlash()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
    }

    void Die()
    {
        Debug.Log($"★ [{bossName}] 처치 완료!");
        if (exitBlockerWall != null) exitBlockerWall.SetActive(false);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats stats = collision.gameObject.GetComponent<PlayerStats>();
            if (stats != null) stats.TakeDamage(contactDamage);
        }
    }
}