using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("생성할 몬스터 프리팹 (여기에 몬스터 넣기)")]
    public GameObject[] monsterPrefabs;

    [Header("생성 주기 및 마릿수 설정")]
    public float spawnInterval = 4f;         // 몇 초마다 1마리씩 나올지
    public int maxMonsterCount = 5;          // 동시에 맵에 살아있을 최대 마릿수
    public int totalSpawnLimit = 0;          // 0이면 계속 생성, 숫자를 쓰면 총 그만큼만 생성

    [Header("플레이어 감지 거리")]
    public float activationDistance = 18f;   // 플레이어가 이 거리 안에 와야 작동 시작

    private Transform player;
    private float spawnTimer = 0f;
    private int spawnedTotal = 0;
    private List<GameObject> activeMonsters = new List<GameObject>();

    void Start()
    {
        // 씬에서 Player 태그를 가진 주인공 자동 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        // 이미 죽어서 삭제된 몬스터는 목록에서 자동 정리
        activeMonsters.RemoveAll(m => m == null);

        // 총 생성 제한에 도달했으면 멈춤
        if (totalSpawnLimit > 0 && spawnedTotal >= totalSpawnLimit)
            return;

        // 현재 필드에 몬스터가 최대 마릿수만큼 있으면 대기
        if (activeMonsters.Count >= maxMonsterCount)
            return;

        // 플레이어가 스포너 근처에 있을 때만 소환
        if (player != null && Vector2.Distance(transform.position, player.position) > activationDistance)
            return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnMonster();
            spawnTimer = 0f;
        }
    }

    void SpawnMonster()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
        {
            Debug.LogWarning("[스포너] 생성할 몬스터 프리팹이 연결되지 않았습니다!");
            return;
        }

        // 등록된 몬스터 종류 중 랜덤 1마리 선택
        int randomIndex = Random.Range(0, monsterPrefabs.Length);
        GameObject selectedPrefab = monsterPrefabs[randomIndex];

        if (selectedPrefab != null)
        {
            GameObject newMonster = Instantiate(selectedPrefab, transform.position, Quaternion.identity);
            activeMonsters.Add(newMonster);
            spawnedTotal++;
        }
    }

    // 에디터 화면(Scene)에서 스포너 감지 범위를 초록색 원으로 표시
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, activationDistance);
    }
}