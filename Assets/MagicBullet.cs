using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    [Header("탄환 속성")]
    public float speed = 20f;
    public int damage = 20;
    public float lifetime = 3f;

    private Vector3 moveVector;
    private bool isFired = false;

    public void Init(Vector2 direction)
    {
        moveVector = new Vector3(direction.x, 0f, 0f);

        // 왼쪽으로 쏠 때는 탄환 이미지도 반대로 뒤집어줌
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        isFired = true;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isFired)
        {
            // 플레이어가 바라보는 정면 방향으로 이동
            transform.position += moveVector * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.gameObject.layer == LayerMask.NameToLayer("PlayerBullet"))
            return;

        if (collision.CompareTag("Enemy"))
        {
            Debug.Log("적 타격!");
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}