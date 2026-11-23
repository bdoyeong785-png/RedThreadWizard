using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상 (Player)")]
    public Transform target;

    [Header("시야 구도 (진행 방향인 우측이 넓게 보이도록 X: +3)")]
    public Vector3 offset = new Vector3(3.0f, 1.2f, -10f);
    public float smoothSpeed = 6f;

    [Header("카메라 이동 한계선 (맵 끝에서 카메라가 허공 비추는 것 방지)")]
    public bool useBounds = false;
    public float minX = -5f;
    public float maxX = 100f;
    public float minY = -2f;
    public float maxY = 15f;

    [Header("보스전 고정 모드")]
    public bool isBossFight = false;
    public Vector3 bossRoomCenter;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition;

        if (isBossFight)
        {
            // 보스전: 보스방 중앙에 카메라 고정
            desiredPosition = new Vector3(bossRoomCenter.x, bossRoomCenter.y, -10f);
        }
        else
        {
            // 일반 진행: 플레이어 추적 + 우측 시야 확보
            desiredPosition = target.position + offset;

            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    public void EnterBossRoom(Vector3 centerPos)
    {
        bossRoomCenter = centerPos;
        isBossFight = true;
    }
}