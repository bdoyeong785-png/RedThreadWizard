using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("HP / MP 설정")]
    public float maxHp = 100f;
    public float currentHp;
    public float maxMp = 100f;
    public float currentMp;

    [Header("마법 영창 (X키 - 누르는 동안 이동 불가)")]
    public float castRequiredTime = 1.0f;   // 영창 완료까지 필요한 시간
    public float magicMpCost = 20f;         // 마법 1회 발사 시 MP 소모량
    public GameObject magicBulletPrefab;    // 발사할 마법 탄환 프리팹 (선택)
    public Transform firePoint;             // 마법이 나갈 지팡이 끝 위치 (선택)
    public float bulletSpeed = 18f;
    public float currentCastTime = 0f;
    public bool isCasting = false;          // 현재 영창 중인지 여부

    [Header("보호막 (Shift 키 - 누르는 동안 이동 불가)")]
    public GameObject shieldObject;
    public float shieldMaxHp = 50f;         // 보호막 내구도
    public float currentShieldHp;
    public float shieldMpCostPerSec = 15f;  // 초당 마나 소모량
    public float shieldBreakCooldown = 3f;  // 보호막이 깨졌을 때 재사용 대기시간
    public bool isShieldActive = false;
    private bool isShieldBroken = false;    // 보호막 파손 여부

    [Header("피격 시 긴급 탈출 이속 증가")]
    public float hitSpeedMultiplier = 1.4f; // 피격 시 40% 빨라짐
    public float hitSpeedDuration = 1.5f;   // 지속 시간
    public bool isHitSpeedBoosted = false;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        currentHp = maxHp;
        currentMp = maxMp;
        currentShieldHp = shieldMaxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (shieldObject != null)
            shieldObject.SetActive(false);
    }

    void Update()
    {
        HandleShield();
        HandleCasting();
    }

    // 1. Shift 보호막 처리
    void HandleShield()
    {
        if (isCasting) return; // 영창 중에는 보호막 사용 불가

        if (Input.GetKey(KeyCode.LeftShift) && currentMp > 0 && !isShieldBroken)
        {
            ActivateShield(true);
            UseMp(shieldMpCostPerSec * Time.deltaTime);

            if (currentMp <= 0)
            {
                ActivateShield(false);
            }
        }
        else
        {
            ActivateShield(false);
        }
    }

    // 2. X키 마법 영창 및 마우스 조준 발사 처리
    void HandleCasting()
    {
        if (isShieldActive) return; // 보호막 전개 중에는 영창 불가

        // X키를 누르고 있고 마나가 충분할 때 영창 진행
        if (Input.GetKey(KeyCode.X) && currentMp >= magicMpCost)
        {
            isCasting = true;
            currentCastTime += Time.deltaTime;

            // 영창 진행도에 따라 가면/캐릭터가 붉게 차오르는 임시 연출
            float progress = Mathf.Clamp01(currentCastTime / castRequiredTime);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.35f), progress);
            }
        }
        // X키를 뗐을 때 영창 완료 여부 판정
        else if (Input.GetKeyUp(KeyCode.X) && isCasting)
        {
            if (currentCastTime >= castRequiredTime && UseMp(magicMpCost))
            {
                FireMagicTowardsMouse();
            }
            else
            {
                Debug.Log("[영창 취소] 영창 시간이 부족합니다.");
            }

            isCasting = false;
            currentCastTime = 0f;
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }
    }

    // 마우스 커서 방향을 계산해 마법 발사
    void FireMagicTowardsMouse()
    {
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 shootDir = (mouseWorldPos - spawnPos).normalized;

        Debug.Log($"★ [마법 발사!] 조준 방향: {shootDir} / 남은 MP: {currentMp}");

        if (magicBulletPrefab != null)
        {
            GameObject bullet = Instantiate(magicBulletPrefab, spawnPos, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            if (bulletRb != null)
            {
                bulletRb.velocity = shootDir * bulletSpeed;
            }
        }
    }

    void ActivateShield(bool active)
    {
        isShieldActive = active;
        if (shieldObject != null)
        {
            shieldObject.SetActive(active);
        }
    }

    public bool UseMp(float amount)
    {
        if (currentMp >= amount)
        {
            currentMp -= amount;
            return true;
        }
        return false;
    }

    public void RestoreMp(float amount)
    {
        currentMp = Mathf.Min(currentMp + amount, maxMp);
        Debug.Log($"[마나 회복] 현재 MP: {currentMp} / {maxMp}");
    }

    // 데미지 입을 때 (보호막 활성 여부 분기)
    public void TakeDamage(float damage)
    {
        // 1. 보호막이 켜져 있으면 보호막 내구도가 대신 깎임
        if (isShieldActive)
        {
            currentShieldHp -= damage;
            Debug.Log($"[보호막 방어] 남은 보호막 내구도: {currentShieldHp}");

            if (currentShieldHp <= 0)
            {
                StartCoroutine(BreakShieldRoutine());
            }
            return;
        }

        // 2. 보호막이 없으면 본체 HP 감소
        currentHp -= damage;
        Debug.Log($"[플레이어 피격] 남은 HP: {currentHp}");

        if (currentHp <= 0)
        {
            Debug.Log("플레이어 사망!");
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(HitFlashAndSpeedRoutine());
        }
    }

    private IEnumerator BreakShieldRoutine()
    {
        Debug.LogWarning("★ [경고] 보호막이 깨졌습니다! 3초간 보호막을 사용할 수 없습니다.");
        isShieldBroken = true;
        ActivateShield(false);

        yield return new WaitForSeconds(shieldBreakCooldown);

        currentShieldHp = shieldMaxHp;
        isShieldBroken = false;
        Debug.Log("★ [복구] 보호막 내구도가 회복되었습니다.");
    }

    // 피격 깜빡임 + 기획서 반영(피격 시 잠시 이속 증가)
    private IEnumerator HitFlashAndSpeedRoutine()
    {
        isHitSpeedBoosted = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;
        }

        yield return new WaitForSeconds(Mathf.Max(0f, hitSpeedDuration - 0.2f));
        isHitSpeedBoosted = false;
    }

    // 마나 구슬 획득 판정
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ManaOrb orb = collision.GetComponent<ManaOrb>();
        if (orb != null || collision.name.Contains("ManaOrb") || collision.CompareTag("ManaOrb"))
        {
            float restoreAmount = orb != null ? orb.restoreMpAmount : 25f;
            RestoreMp(restoreAmount);
            Destroy(collision.gameObject);
            Debug.Log("★ 마나 구슬 획득 완료!");
        }
    }
}