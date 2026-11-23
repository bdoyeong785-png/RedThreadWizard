using UnityEngine;

public class ManaOrb : MonoBehaviour
{
    public float restoreMpAmount = 25f; // 회복 MP 양
    public float floatSpeed = 1.5f;     // 둥둥 뜨는 속도
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 위아래로 살짝 둥둥 떠있는 이펙트
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed * 3f) * 0.15f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 몸체나 플레이어 콜라이더에 닿았을 때
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.RestoreMp(restoreMpAmount);
                Debug.Log($"[마나 구슬] MP +{restoreMpAmount} 회복 완료!");
            }
            Destroy(gameObject);
        }
    }
}