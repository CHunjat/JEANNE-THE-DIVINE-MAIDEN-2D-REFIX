using UnityEngine;
using System.Collections;

// =====================================================
// FinalBossPattern4.cs
// 데몬 누나 2페이즈 패턴 2 - 강화 에너지 볼트
//
// [기획 문서 기준]
// - 시전 시 사라졌다가 플레이어 시선 기준 전방 30m에서 등장
// - 일정 간격으로 본체 전방에 분신 생성 후 에너지 볼트 2회 공격
// - 분신1 - 분신2 - 본체 순서로 위치 후 순차적으로 에너지 볼트 공격
// - 3개의 에너지 볼트를 연타로 공격하도록 소환 시간 조절 필요
// - 분신은 피격 판정 없음, 에너지 볼트 2회 후 소멸
// - 일반 대쉬(스프린트 x)로 통과 가능
// =====================================================
public class FinalBossPattern4 : FinalBossPatternBase
{
    [Header("강화 에너지 볼트 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float teleportRange = 30f;      // 순간이동 거리
    [SerializeField] private float cloneSpacing = 5f;        // 분신 간격
    [SerializeField] private float boltSpeed = 12f;          // 볼트 속도 (강화됨)
    [SerializeField] private float boltRange = 30f;          // 사거리
    [SerializeField] private float fireInterval = 0.3f;      // 분신1 → 분신2 → 본체 발사 간격

    [Header("프리팹 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject energyBoltPrefab;  // 에너지 볼트 프리팹
    [SerializeField] private GameObject clonePrefab;       // 분신 프리팹

    private bool isExecuting = false;

    private void Awake()
    {
        cooldown = 7f;  // 임시 쿨타임 - 기획 확정 후 수정할 것
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        Debug.Log("[FinalBossPattern4] 강화 에너지 볼트 시전!");
        StartCoroutine(EnhancedBoltRoutine());
    }

    private IEnumerator EnhancedBoltRoutine()
    {
        isExecuting = true;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) { isExecuting = false; yield break; }

        // 본체 순간이동 - 플레이어 시선 기준 전방 30m
        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        Vector2 dirToPlayer = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
        // 플레이어 앞쪽(플레이어가 보스를 향하면 플레이어 뒤쪽)으로 이동
        transform.position = (Vector2)playerObj.transform.position + dirToPlayer * teleportRange;

        if (visual != null) visual.gameObject.SetActive(true);
        Debug.Log("[FinalBossPattern4] 순간이동 완료.");

        // 분신 소환 - 본체 전방에 순서대로
        Vector2 fireDir = -dirToPlayer;  // 플레이어 방향
        Vector2 clone1Pos = (Vector2)transform.position + fireDir * cloneSpacing;
        Vector2 clone2Pos = (Vector2)transform.position + fireDir * (cloneSpacing * 2f);

        GameObject clone1 = null, clone2 = null;
        if (clonePrefab != null)
        {
            clone1 = Instantiate(clonePrefab, clone1Pos, Quaternion.identity);
            clone2 = Instantiate(clonePrefab, clone2Pos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.2f);

        // 분신1 → 분신2 → 본체 순서로 에너지 볼트 발사
        FireBolt(clone1Pos, fireDir);
        yield return new WaitForSeconds(fireInterval);

        FireBolt(clone2Pos, fireDir);
        yield return new WaitForSeconds(fireInterval);

        FireBolt(transform.position, fireDir);

        // 분신 소멸
        if (clone1 != null) Destroy(clone1, 0.5f);
        if (clone2 != null) Destroy(clone2, 0.5f);

        isExecuting = false;
    }

    private void FireBolt(Vector2 origin, Vector2 dir)
    {
        if (energyBoltPrefab == null) return;

        GameObject bolt = Instantiate(energyBoltPrefab, origin, Quaternion.identity);
        EnergyBoltProjectile projectile = bolt.GetComponent<EnergyBoltProjectile>();
        if (projectile != null)
            projectile.Initialize(dir, boltSpeed, boltRange);

        Debug.Log($"[FinalBossPattern4] 에너지 볼트 발사! 위치: {origin}");
    }
}