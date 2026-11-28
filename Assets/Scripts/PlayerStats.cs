using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private HUD_UI hudUI;

    [Header("HP / MP 설정")]
    public float maxHp = 100f;
    public float currentHp;
    public float maxMp = 100f;
    public float currentMp;

    [Header("마법 영창 (X키 - 누르는 동안 이동 불가)")]
    public float castRequiredTime = 1.0f;   // 영창 완료까지 필요한 시간
    public float magicMpCost = 20f;         // 마법 1회 발사 시 MP 소모량
    public GameObject magicBulletPrefab;    // 발사할 마법 탄환 프리팹
    public Transform firePoint;             // 마법이 나갈 지팡이 끝 위치
    public float bulletSpeed = 18f;
    public float currentCastTime = 0f;
    public bool isCasting = false;          // 현재 영창 중인지 여부

    [Header("보호막 (Ctrl 키 - 누르는 동안 이동 불가)")]
    public GameObject shieldObject;
    public float shieldMaxHp = 50f;         // 보호막 내구도
    public float currentShieldHp;
    public float shieldMpCostPerSec = 15f;  // 초당 마나 소모량
    public float shieldBreakCooldown = 3f;  // 보호막이 깨졌을 때 재사용 대기시간
    public bool isShieldActive = false;
    private bool isShieldBroken = false;    // 보호막 파손 여부

    [Header("보호막 내구도 색상 (파랑 -> 노랑 -> 빨강)")]
    public Color shieldFullColor = new Color(0.2f, 0.6f, 1f, 0.55f);   // 100%: 파란색
    public Color shieldMidColor = new Color(1f, 0.9f, 0.2f, 0.55f);    // 50%: 노란색
    public Color shieldLowColor = new Color(1f, 0.2f, 0.2f, 0.65f);   // 0% 직전: 빨간색

    [Header("피격 시 긴급 탈출 이속 증가")]
    public float hitSpeedMultiplier = 1.4f; // 피격 시 40% 빨라짐
    public float hitSpeedDuration = 1.5f;   // 지속 시간
    public bool isHitSpeedBoosted = false;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer shieldSpriteRenderer; // 보호막 스프라이트 색상 변경용

    void Awake()
    {

        currentHp = maxHp;
        currentMp = maxMp;
        currentShieldHp = shieldMaxHp;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (shieldObject != null)
        {
            shieldSpriteRenderer = shieldObject.GetComponent<SpriteRenderer>();
            shieldObject.SetActive(false);
        }
    }
    
    void Start()
    {
        if (hudUI != null)
        {
            hudUI.UpdateHP(currentHp, maxHp);
            hudUI.UpdateMP(currentMp, maxMp);
        }
    }


    void Update()
    {
        HandleShield();
        HandleCasting();
    }

    // 1. [수정됨] 왼쪽 Ctrl 키 보호막 처리
    void HandleShield()
    {
        if (isCasting) return; // 영창 중에는 보호막 사용 불가

        if (Input.GetKey(KeyCode.LeftControl) && currentMp > 0 && !isShieldBroken)
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

        if (Input.GetKey(KeyCode.X) && currentMp >= magicMpCost)
        {
            isCasting = true;
            currentCastTime += Time.deltaTime;

            float progress = Mathf.Clamp01(currentCastTime / castRequiredTime);
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, new Color(1f, 0.35f, 0.35f), progress);
            }
        }
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
            if (active)
            {
                UpdateShieldColor();
            }
        }
    }

    // 내구도 비율에 따라 파랑 -> 노랑 -> 빨강으로 부드럽게 색상 변경
    void UpdateShieldColor()
    {
        if (shieldSpriteRenderer == null || shieldMaxHp <= 0f) return;

        float ratio = Mathf.Clamp01(currentShieldHp / shieldMaxHp);

        if (ratio > 0.5f)
        {
            // 내구도 100% -> 50% : 파랑에서 노랑으로 점점 변화
            float t = (ratio - 0.5f) * 2f;
            shieldSpriteRenderer.color = Color.Lerp(shieldMidColor, shieldFullColor, t);
        }
        else
        {
            // 내구도 50% -> 0% : 노랑에서 빨강으로 점점 변화
            float t = ratio * 2f;
            shieldSpriteRenderer.color = Color.Lerp(shieldLowColor, shieldMidColor, t);
        }
    }

    public bool UseMp(float amount)
    {
        if (currentMp >= amount)
        {
            currentMp -= amount;
            
            if (hudUI != null) hudUI.UpdateMP(currentMp, maxMp);

            return true;
        }
        return false;
    }

    public void RestoreMp(float amount)
    {
        currentMp = Mathf.Min(currentMp + amount, maxMp);
        if (hudUI != null) hudUI.UpdateMP(currentMp, maxMp);
        Debug.Log($"[마나 회복] 현재 MP: {currentMp} / {maxMp}");
    }

    public void TakeDamage(float damage)
    {
        if (isShieldActive)
        {
            currentShieldHp -= damage;
           
            UpdateShieldColor(); // 맞을 때마다 즉시 색상 갱신
            Debug.Log($"[보호막 방어] 남은 보호막 내구도: {currentShieldHp}");

            if (currentShieldHp <= 0)
            {
                currentShieldHp = 0;
                StartCoroutine(BreakShieldRoutine());
            }
            return;
        }

        currentHp -= damage;
        if (hudUI != null) hudUI.UpdateHP(currentHp, maxHp);
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
        ActivateShield(false); // 깨지면 즉시 오브젝트 비활성화(사라짐)

        yield return new WaitForSeconds(shieldBreakCooldown);

        currentShieldHp = shieldMaxHp;
        isShieldBroken = false;
        UpdateShieldColor(); // 복구 시 다시 파란색으로 초기화
        Debug.Log("★ [복구] 보호막 내구도가 회복되었습니다.");
    }

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