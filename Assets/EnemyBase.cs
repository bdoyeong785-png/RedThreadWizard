using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("몬스터 기본 능력치")]
    public float maxHp = 50f;
    public float currentHp;
    public float attackDamage = 10f;

    [Header("드랍 아이템")]
    public GameObject manaOrbPrefab; // 마나 구슬 프리팹 (선택)

    protected Transform playerTransform;

    protected virtual void Awake()
    {
        currentHp = maxHp;
    }

    protected virtual void Start()
    {
        // 씬에서 Player 태그를 가진 오브젝트 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHp -= damage;
        Debug.Log($"{gameObject.name} 피격! 남은 HP: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} 처치됨!");

        // 마나 구슬 드랍 (있다면)
        if (manaOrbPrefab != null)
        {
            Instantiate(manaOrbPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // 플레이어와 몸통 충돌 시 데미지 부여
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(attackDamage);
            }
        }
    }
}