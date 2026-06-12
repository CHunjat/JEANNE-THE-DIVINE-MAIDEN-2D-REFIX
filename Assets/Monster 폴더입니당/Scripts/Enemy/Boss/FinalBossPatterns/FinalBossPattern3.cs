using UnityEngine;
using System.Collections;

// =====================================================
// FinalBossPattern3.cs
// 데몬 누나 2페이즈 패턴 1 - 강화 손톱 베기
//
// [기획 문서 기준]
// - 손톱 베기를 2번 시전
// - 첫 번째 손톱 베기의 1세트 공격 직후 플레이어 시선 기준 후방에 분신 소환
// - 분신은 피격 판정 없음
// - 손톱 베기 1회 공격 후 소멸
// - 일반 대쉬(스프린트 x)로 통과 가능
//
// [분신 처리]
// 분신은 피격 판정이 없는 별도 오브젝트임.
// 분신 오브젝트에 EnemyHitbox만 붙이고 Collider는 붙이지 않음.
// 분신 자체에는 FinalBossClone 스크립트를 붙여서 손톱 베기 1회 후 소멸하게 처리.
//
// [히트박스 세팅 방법]
// - 본체 히트박스: FinalBossPattern1의 leftClawHitbox, rightClawHitbox 재사용
// - 분신 프리팹: "FinalBossClone" 프리팹 별도 제작 (아래 FinalBossClone.cs 참고)
// =====================================================
public class FinalBossPattern3 : FinalBossPatternBase
{
    [Header("강화 손톱 베기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float hitboxActiveDuration = 0.15f;
    [SerializeField] private float shortDelay = 0.2f;
    [SerializeField] private float longDelay = 0.5f;
    [SerializeField] private float cloneSpawnDelay = 0.1f;  // 1세트 후 분신 소환 딜레이

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject leftClawHitbox;
    [SerializeField] private GameObject rightClawHitbox;

    [Header("분신 설정")]
    [SerializeField] private GameObject clonePrefab;  // 분신 프리팹
    [SerializeField] private float cloneOffset = 3f;  // 플레이어 후방 소환 거리

    private void Awake()
    {
        cooldown = 5f;  // 임시 쿨타임 - 기획 확정 후 수정할 것

        if (leftClawHitbox != null) leftClawHitbox.SetActive(false);
        if (rightClawHitbox != null) rightClawHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("[FinalBossPattern3] 강화 손톱 베기 시전!");
        StartCoroutine(EnhancedClawRoutine());
    }

    private IEnumerator EnhancedClawRoutine()
    {
        // 1번 손톱 베기 (1세트)
        yield return StartCoroutine(ClawSet(leftClawHitbox));

        // 1세트 후 분신 소환
        yield return new WaitForSeconds(cloneSpawnDelay);
        SpawnClone();

        yield return new WaitForSeconds(longDelay);

        // 2번 손톱 베기 (2세트)
        yield return StartCoroutine(ClawSet(rightClawHitbox));
    }

    // 손톱 베기 한 세트 (2타)
    private IEnumerator ClawSet(GameObject hitbox)
    {
        // 1타
        if (hitbox != null) hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveDuration);
        if (hitbox != null) hitbox.SetActive(false);

        yield return new WaitForSeconds(shortDelay);

        // 2타
        if (hitbox != null) hitbox.SetActive(true);
        yield return new WaitForSeconds(hitboxActiveDuration);
        if (hitbox != null) hitbox.SetActive(false);
    }

    // 플레이어 시선 기준 후방에 분신 소환
    private void SpawnClone()
    {
        if (clonePrefab == null)
        {
            Debug.LogWarning("[FinalBossPattern3] clonePrefab이 연결되지 않음. 인스펙터에서 프리팹을 넣을 것.");
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        // 플레이어 시선 기준 후방 위치 계산
        // 플레이어가 보스를 향하고 있으므로 플레이어 뒤쪽 = 보스 반대 방향
        Vector2 dirFromBossToPlayer = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
        Vector2 clonePos = (Vector2)playerObj.transform.position + dirFromBossToPlayer * cloneOffset;

        GameObject clone = Instantiate(clonePrefab, clonePos, Quaternion.identity);
        Debug.Log("[FinalBossPattern3] 분신 소환됨.");
    }
}