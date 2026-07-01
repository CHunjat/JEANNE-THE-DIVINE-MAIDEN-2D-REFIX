using UnityEngine;
using System.Collections;

// =====================================================
// FinalBossPattern5.cs
// 데몬 누나 2페이즈 패턴 3 - 손톱과 에너지 볼트
//
// [기획 문서 기준]
// - 손톱 베기 공격 2회 시전
// - 손톱 베기 1세트 공격 후 플레이어 시선 기준 후방 10m, 5m 위치에 분신 소환
// - 분신은 소환 시점의 플레이어 위치로 에너지 볼트 공격 2회 시전 후 소멸
// - 분신은 피격 판정 없음, 일반 대쉬(스프린트 x)로 통과 가능
// =====================================================
public class FinalBossPattern5 : FinalBossPatternBase
{
    [Header("손톱+에너지 볼트 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float hitboxActiveDuration = 0.15f;
    [SerializeField] private float shortDelay = 0.2f;
    [SerializeField] private float longDelay = 0.5f;
    [SerializeField] private float clone1Offset = 10f;   // 분신1 후방 거리 (10m)
    [SerializeField] private float clone2Offset = 5f;    // 분신2 후방 거리 (5m)
    [SerializeField] private float boltSpeed = 10f;
    [SerializeField] private float boltRange = 30f;
    [SerializeField] private float boltFireInterval = 0.4f;  // 볼트 2회 발사 간격

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject leftClawHitbox;
    [SerializeField] private GameObject rightClawHitbox;

    [Header("프리팹 연결")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private GameObject energyBoltPrefab;

    private void Awake()
    {
        cooldown = 6f;

        if (leftClawHitbox != null) leftClawHitbox.SetActive(false);
        if (rightClawHitbox != null) rightClawHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("[FinalBossPattern5] 손톱과 에너지 볼트 시전!");
        StartCoroutine(ClawAndBoltRoutine());
    }

    private IEnumerator ClawAndBoltRoutine()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) yield break;

        // 손톱 베기 1세트 (왼손)
        yield return StartCoroutine(ClawSet(leftClawHitbox));

        // 1세트 후 분신 소환 - 소환 시점의 플레이어 위치 기록
        Vector2 playerPosAtSpawn = playerObj.transform.position;
        Vector2 dirFromBossToPlayer = ((Vector2)(playerObj.transform.position - transform.position)).normalized;

        // 플레이어 후방 위치에 분신 소환
        Vector2 clone1Pos = (Vector2)playerObj.transform.position + dirFromBossToPlayer * clone1Offset;
        Vector2 clone2Pos = (Vector2)playerObj.transform.position + dirFromBossToPlayer * clone2Offset;

        if (clonePrefab != null)
        {
            // 분신들이 소환 시점의 플레이어 위치로 에너지 볼트 발사
            StartCoroutine(CloneBoltRoutine(clone1Pos, playerPosAtSpawn));
            StartCoroutine(CloneBoltRoutine(clone2Pos, playerPosAtSpawn));
        }

        yield return new WaitForSeconds(longDelay);

        // 손톱 베기 2세트 (오른손)
        yield return StartCoroutine(ClawSet(rightClawHitbox));
    }

    private IEnumerator ClawSet(GameObject hitbox)
    {
        if (hitbox != null) hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveDuration);
        if (hitbox != null) hitbox.SetActive(false);

        yield return new WaitForSeconds(shortDelay);

        if (hitbox != null) hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveDuration);
        if (hitbox != null) hitbox.SetActive(false);
    }

    private IEnumerator CloneBoltRoutine(Vector2 clonePos, Vector2 targetPos)
    {
        if (clonePrefab == null || energyBoltPrefab == null) yield break;

        GameObject clone = Instantiate(clonePrefab, clonePos, Quaternion.identity);
        Vector2 fireDir = (targetPos - clonePos).normalized;

        // 에너지 볼트 2회 발사
        FireBolt(clonePos, fireDir);
        yield return new WaitForSeconds(boltFireInterval);
        FireBolt(clonePos, fireDir);

        // 분신 소멸
        Destroy(clone, boltFireInterval + 0.5f);
        Debug.Log("[FinalBossPattern5] 분신 에너지 볼트 2회 발사 후 소멸.");
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