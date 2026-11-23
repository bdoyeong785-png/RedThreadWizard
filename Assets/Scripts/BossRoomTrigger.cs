using UnityEngine;

public class BossRoomTrigger : MonoBehaviour
{
    public Transform bossRoomCenter;    // 보스방 중앙 위치
    public GameObject exitBlockerWall;  // 보스방 양쪽 입구/출구 막는 투명 벽 묶음
    public BossController boss;         // 보스 오브젝트 연결
    private bool triggered = false;

    void Start()
    {
        if (exitBlockerWall != null) exitBlockerWall.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!triggered && collision.CompareTag("Player"))
        {
            triggered = true;
            if (exitBlockerWall != null) exitBlockerWall.SetActive(true);

            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null && bossRoomCenter != null)
                cam.EnterBossRoom(bossRoomCenter.position);

            if (boss != null)
                boss.StartBossBattle();
        }
    }
}