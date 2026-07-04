using UnityEngine;

// =====================================================
// FinalBossPattern2.cs
// 데몬 누나 1페이즈 패턴 2 - 에너지 볼트
//
// [기획 문서 기준]
// - 바라보는 방향으로 에너지 구체 발사
// - 시전 속도 상승, 구체 이동 속도 증가, 사정 거리 30m
//
// [히트박스 세팅 방법 - 발사체 히트박스]
// 1. Project 창에서 빈 오브젝트로 "EnergyBolt" 프리팹 만들기
// 2. EnergyBolt에 CircleCollider2D (Is Trigger 체크)
// 3. EnergyBolt에 EnemyHitbox 스크립트 붙이기
//    - Destroy On Hit: 체크 (볼트는 맞으면 사라짐)
// 4. EnergyBolt에 EnergyBoltProjectile 스크립트 붙이기
// 5. 이 스크립트의 energyBoltPrefab 필드에 EnergyBolt 프리팹 드래그
// =====================================================
public class FinalBossPattern2 : FinalBossPatternBase
{
    [Header("에너지 볼트 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float preDelay = 0.3f;       // 선딜레이 (초)
    [SerializeField] private float boltSpeed = 10f;       // 구체 이동 속도
    [SerializeField] private float boltRange = 30f;       // 사정 거리 (문서 기준: 30m)

    [Header("히트박스 연결 - 인스펙터에서 EnergyBolt 프리팹을 드래그해서 넣을 것")]
    [SerializeField] private GameObject energyBoltPrefab;

    private Transform owner;

    private void Awake()
    {
        cooldown = 4f;  // 임시 쿨타임 - 기획 확정 후 수정할 것
        owner = transform;
    }

    protected override void OnExecute()
    {
        Debug.Log("[FinalBossPattern2] 에너지 볼트 시전!");
        Invoke(nameof(FireBolt), preDelay);
    }

    private void FireBolt()
    {
        if (energyBoltPrefab == null)
        {
            Debug.LogWarning("[FinalBossPattern2] energyBoltPrefab이 연결되지 않음. 인스펙터에서 프리팹을 넣을 것.");
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir = playerObj != null
            ? ((Vector2)(playerObj.transform.position - owner.position)).normalized
            : Vector2.right;

        GameObject bolt = Instantiate(energyBoltPrefab, owner.position, Quaternion.identity);
        EnergyBoltProjectile projectile = bolt.GetComponent<EnergyBoltProjectile>();

        if (projectile != null)
            projectile.Initialize(dir, boltSpeed, boltRange);
    }
}