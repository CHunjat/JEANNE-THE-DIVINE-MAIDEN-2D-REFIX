using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern4.cs
// 거미 보스 1페이즈 패턴 4 - 점프 공격
//
// [기획 문서 기준]
// - 공중으로 도약 후 UI에서 사라짐 (오브젝트 SetActive(false))
// - 일정 시간 후 플레이어 위치를 타겟팅해서 낙하 공격
// - 캐릭터 점프 상태에서 피격 가능, 대시로 회피 불가
// - 피격/가드 시 넉백 + 충격 상태이상 (일정 시간 조작 불가)
// - 패리 성공 시 조작 불가 시간 없음 (프리딜 타임)
//   → 충격 상태이상은 Player 담당자에게 요청할 것
//
// [동작 순서]
// 1. 보스 오브젝트 SetActive(false) → 화면에서 사라짐
// 2. airTime 동안 대기
// 3. 플레이어 현재 위치로 이동 후 SetActive(true) → 다시 나타남
// 4. 착지 히트박스 활성화 → 데미지 판정
//
// [히트박스 세팅 방법]
// 1. MidBoss_Spider 아래 자식으로 "Hitbox_Landing" 오브젝트 만들기
// 2. Hitbox_Landing에 CircleCollider2D (Is Trigger 체크, 반경 넓게 설정)
// 3. Hitbox_Landing에 EnemyHitbox 스크립트 붙이기
// 4. 이 스크립트의 landingHitbox 필드에 Hitbox_Landing 드래그
// =====================================================
public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float airTime = 2f;                    // 공중에 있는 시간 (초)
    [SerializeField] private float hitboxActiveDuration = 0.4f;     // 착지 히트박스 유지 시간 (초)

    [Header("히트박스 연결 - 인스펙터에서 Hitbox_Landing 오브젝트를 드래그해서 넣을 것")]
    [SerializeField] private GameObject landingHitbox;  // 착지 판정 히트박스

    private MidBoss owner;
    private bool isJumping = false;

    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 8f;  // 임시 쿨타임 - 기획 확정 후 수정할 것
        visualAnimator = GetComponentInChildren<Animator>();
        owner = GetComponent<MidBoss>();

        if (landingHitbox != null)
            landingHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (isJumping) return;
        Debug.Log("[MidBossPattern4] 점프 공격 시전!");
        if (visualAnimator != null) visualAnimator.Play("jump attack up");
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;

        // 보스 오브젝트 숨기기 (문서 기준: UI에서 사라짐)
        // Visual 자식 오브젝트만 끄는 방식 - Collider나 스크립트는 유지
        Transform visual = transform.Find("Visual");
        if (visual != null)
            visual.gameObject.SetActive(false);
        else
            Debug.LogWarning("[MidBossPattern4] Visual 자식 오브젝트를 찾지 못함. MidBoss_Spider 아래 Visual 오브젝트가 있는지 확인할 것.");

        Debug.Log("[MidBossPattern4] 보스 화면에서 사라짐. 공중 대기 중...");

        yield return new WaitForSeconds(airTime);

        // 플레이어 현재 위치로 이동
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            transform.position = playerObj.transform.position;

        // 보스 다시 보이기
        if (visual != null)
            visual.gameObject.SetActive(true);

        // 착지 애니메이션 재생
        if (visualAnimator != null)
            visualAnimator.Play("jump attack land");

        Debug.Log("[MidBossPattern4] 착지! 히트박스 활성화.");

        // 착지 히트박스 활성화
        if (landingHitbox != null)
        {
            landingHitbox.SetActive(true);
            yield return new WaitForSeconds(hitboxActiveDuration);
            landingHitbox.SetActive(false);
        }

        isJumping = false;
    }
}