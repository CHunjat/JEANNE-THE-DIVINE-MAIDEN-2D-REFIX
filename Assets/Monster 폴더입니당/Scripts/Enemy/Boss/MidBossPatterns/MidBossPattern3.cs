using UnityEngine;

// =====================================================
// MidBossPattern3.cs
// 거미 보스 1페이즈 패턴 3 - 거미줄 뱉기
//
// [기획 문서 기준]
// - 플레이어 방향으로 거미줄을 발사하여 공격함
// - 뱉기 전 모션 속도 조절 필요 (애니메이션 작업 시 협력)
// - 거미줄 피격 시 캐릭터 구속 상태이상 발생
// - 캐릭터 약공 버튼 연타 시 구속 시간 단축 (1회당 0.1초 단축)
//   → 구속 해제 로직은 Player 담당자에게 요청할 것
//
// [히트박스 세팅 방법 - 발사체 히트박스]
// 1. Project 창에서 빈 오브젝트로 "WebProjectile" 프리팹 만들기
// 2. WebProjectile에 CircleCollider2D 붙이고 Is Trigger 체크
// 3. WebProjectile에 EnemyHitbox 스크립트 붙이기
//    - Destroy On Hit: 체크 (거미줄은 맞으면 사라짐)
// 4. WebProjectile에 MidBossWebProjectile 스크립트 붙이기
// 5. 이 스크립트의 webPrefab 필드에 WebProjectile 프리팹 드래그
// =====================================================
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float preDelay = 0.6f;        // 선딜레이 (초) - 플레이어가 대비할 수 있도록
    [SerializeField] private float webSpeed = 6f;          // 거미줄 이동 속도
    [SerializeField] private float webRange = 12f;         // 거미줄 최대 사거리
    [SerializeField] private float bindDuration = 3f;      // 구속 상태이상 지속 시간 (초) - 기획 확정 후 수정

    [Header("히트박스 연결 - 인스펙터에서 WebProjectile 프리팹을 드래그해서 넣을 것")]
    [SerializeField] private GameObject webPrefab;         // 거미줄 발사체 프리팹

    private Transform owner;

    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 6f;  // 임시 쿨타임 - 기획 확정 후 수정할 것
        visualAnimator = GetComponentInChildren<Animator>();
        owner = transform;
    }

    protected override void OnExecute()
    {
        Debug.Log("[MidBossPattern3] 거미줄 뱉기 시전!");
        if (visualAnimator != null) visualAnimator.Play("Web");
        Invoke(nameof(FireWeb), preDelay);
    }

    private void FireWeb()
    {
        if (webPrefab == null)
        {
            Debug.LogWarning("[MidBossPattern3] webPrefab이 연결되지 않음. 인스펙터에서 프리팹을 넣을 것.");
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir = playerObj != null
            ? ((Vector2)(playerObj.transform.position - owner.position)).normalized
            : Vector2.right;

        GameObject web = Instantiate(webPrefab, owner.position, Quaternion.identity);
        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();

        if (webScript != null)
            webScript.Initialize(dir, webSpeed, webRange, bindDuration);
    }
}