using UnityEngine;
using System.Collections;

// =====================================================
// FinalBossPattern6.cs
// 데몬 누나 2페이즈 패턴 4 - 필살 패턴 (눈나 나 죽어)
//
// [기획 문서 기준]
// - 시전 시 사라졌다가 플레이어 시선 기준:
//   전방 30m, 4m 공중에서 본체 등장
//   전방 40m에 분신1 등장
//   후방 10m에 분신2 등장
// - 본체: 플레이어 방향으로 40m 범위 손 낙하 공격
//   (캐릭터 시선 방향 관계없이 가드/패링 가능)
// - 분신1: 에너지 볼트 공격 시전
// - 분신2: 등장 위치 ~ 본체 기준 5m 위치까지 플레이어를 추격하며 손톱 공격
//   (이동 속도, 공격 간격 조절 필요)
//
// [히트박스 세팅 방법]
// - "Hitbox_HandDrop": 손 낙하 공격 판정 (긴 범위의 BoxCollider2D 권장)
// - 분신1: 에너지 볼트 프리팹 재사용
// - 분신2: FinalBossChaseClone 스크립트를 붙인 별도 프리팹
// =====================================================
public class FinalBossPattern6 : FinalBossPatternBase
{
    [Header("필살 패턴 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float teleportForward = 30f;       // 본체 순간이동 전방 거리
    [SerializeField] private float teleportHeight = 4f;         // 본체 공중 높이
    [SerializeField] private float clone1ForwardOffset = 40f;   // 분신1 전방 거리
    [SerializeField] private float clone2BackOffset = 10f;      // 분신2 후방 거리
    [SerializeField] private float handDropWidth = 40f;         // 손 낙하 공격 범위 (가로)
    [SerializeField] private float handDropDuration = 0.5f;     // 낙하 히트박스 유지 시간
    [SerializeField] private float boltSpeed = 10f;
    [SerializeField] private float boltRange = 30f;

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject handDropHitbox;   // 손 낙하 히트박스

    [Header("프리팹 연결")]
    [SerializeField] private GameObject energyBoltPrefab;     // 에너지 볼트
    [SerializeField] private GameObject clone1Prefab;         // 분신1 (에너지 볼트용)
    [SerializeField] private GameObject chaseClone2Prefab;    // 분신2 (추격 손톱용)

    private bool isExecuting = false;

    private void Awake()
    {
        cooldown = 30f;  // 필살 패턴 - 긴 쿨타임 설정, 기획 확정 후 수정할 것

        if (handDropHitbox != null)
            handDropHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        Debug.Log("[FinalBossPattern6] 필살 패턴 시전! (눈나 나 죽어)");
        StartCoroutine(UltimateRoutine());
    }

    private IEnumerator UltimateRoutine()
    {
        isExecuting = true;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) { isExecuting = false; yield break; }

        // 본체, 분신1, 분신2 위치 계산
        Vector2 playerPos = playerObj.transform.position;
        Vector2 dirFromPlayerToBoss = ((Vector2)(transform.position - playerObj.transform.position)).normalized;
        Vector2 forwardDir = -dirFromPlayerToBoss;  // 플레이어 → 보스 방향이 전방

        // 본체 순간이동: 전방 30m, 4m 공중
        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        Vector2 bossNewPos = playerPos + forwardDir * teleportForward;
        bossNewPos.y += teleportHeight;
        transform.position = bossNewPos;

        if (visual != null) visual.gameObject.SetActive(true);

        // 분신1 소환: 전방 40m
        Vector2 clone1Pos = playerPos + forwardDir * clone1ForwardOffset;
        GameObject clone1 = null;
        if (clone1Prefab != null)
            clone1 = Instantiate(clone1Prefab, clone1Pos, Quaternion.identity);

        // 분신2 소환: 후방 10m
        Vector2 clone2Pos = playerPos - forwardDir * clone2BackOffset;
        GameObject clone2 = null;
        if (chaseClone2Prefab != null)
        {
            clone2 = Instantiate(chaseClone2Prefab, clone2Pos, Quaternion.identity);
            // 분신2에 추격 목표 설정
            FinalBossChaseClone chaseScript = clone2.GetComponent<FinalBossChaseClone>();
            if (chaseScript != null)
                chaseScript.Initialize(playerObj.transform, transform);
        }

        yield return new WaitForSeconds(0.5f);  // 등장 연출 대기

        // 동시에 세 개의 공격 시작
        // 본체: 손 낙하 공격
        StartCoroutine(HandDropRoutine(forwardDir));

        // 분신1: 에너지 볼트
        if (clone1 != null)
        {
            Vector2 boltDir = ((Vector2)playerObj.transform.position - clone1Pos).normalized;
            FireBolt(clone1Pos, boltDir);
            Destroy(clone1, 2f);
        }

        // 분신2는 FinalBossChaseClone 스크립트에서 자동으로 추격 및 공격함

        yield return new WaitForSeconds(handDropDuration + 0.5f);

        // 분신2 소멸 (본체 기준 5m 이내 도달 시 소멸 - FinalBossChaseClone에서 처리)

        isExecuting = false;
    }

    private IEnumerator HandDropRoutine(Vector2 forwardDir)
    {
        if (handDropHitbox == null) yield break;

        // 히트박스 크기를 handDropWidth에 맞게 조정 (인스펙터에서 BoxCollider2D 크기 설정 필요)
        handDropHitbox.SetActive(true);
        Debug.Log($"[FinalBossPattern6] 손 낙하 공격! 범위: {handDropWidth}m");
        yield return new WaitForSeconds(handDropDuration);
        handDropHitbox.SetActive(false);
    }

    private void FireBolt(Vector2 origin, Vector2 dir)
    {
        if (energyBoltPrefab == null) return;

        GameObject bolt = Instantiate(energyBoltPrefab, origin, Quaternion.identity);
        EnergyBoltProjectile projectile = bolt.GetComponent<EnergyBoltProjectile>();
        if (projectile != null)
            projectile.Initialize(dir, boltSpeed, boltRange);
    }
}