using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("HP / MP 설정")]
    public float maxHp = 100f;
    public float currentHp;
    public float maxMp = 100f;
    public float currentMp;

    [Header("보호막 (Shift 키)")]
    public GameObject shieldObject;
    public float shieldMpCostPerSec = 20f;
    public bool isShieldActive = false;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        currentHp = maxHp;
        currentMp = maxMp;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (shieldObject != null)
            shieldObject.SetActive(false);
    }

    void Update()
    {
        // Shift 키 누르는 동안 보호막 동작
        if (Input.GetKey(KeyCode.LeftShift) && currentMp > 0)
        {
            ActivateShield(true);
            UseMp(shieldMpCostPerSec * Time.deltaTime);
        }
        else
        {
            ActivateShield(false);
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

    // 마나 소모 함수
    public bool UseMp(float amount)
    {
        if (currentMp >= amount)
        {
            currentMp -= amount;
            return true;
        }
        return false;
    }

    // 마나 회복 함수
    public void RestoreMp(float amount)
    {
        currentMp = Mathf.Min(currentMp + amount, maxMp);
        Debug.Log($"[플레이어] 마나 회복! 현재 MP: {currentMp} / {maxMp}");
    }

    // 데미지 입을 때 (피격)
    public void TakeDamage(float damage)
    {
        if (isShieldActive)
        {
            UseMp(damage * 0.5f);
            return;
        }

        currentHp -= damage;
        Debug.Log($"[플레이어] 피격! 남은 HP: {currentHp}");

        if (currentHp <= 0)
        {
            Debug.Log("플레이어 사망!");
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(HitFlashRoutine());
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;
        }
    }

    // ★ 마나 구슬 획득 판정 (플레이어가 직접 감지!)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 닿은 오브젝트의 이름이 ManaOrb이거나 ManaOrb 스크립트를 가지고 있다면!
        ManaOrb orb = collision.GetComponent<ManaOrb>();
        if (orb != null || collision.name.Contains("ManaOrb"))
        {
            float restoreAmount = 25f;
            if (orb != null) restoreAmount = orb.restoreMpAmount;

            RestoreMp(restoreAmount);
            Destroy(collision.gameObject); // 구슬 파괴
            Debug.Log("★ [성공] 마나 구슬 획득 완료!");
        }
    }
}