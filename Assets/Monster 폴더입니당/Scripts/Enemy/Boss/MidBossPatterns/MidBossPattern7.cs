using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern7.cs
// 거미 보스 2페이즈 패턴 2 - 앞 다리 휘두르기
//
// [기획 문서 기준]
// - 앞 다리 휘두르기 공격
// - 휘두른 다리를 회수하며 공격
// - 회수 후 앞발 찍기 범위 내 플레이어가 있을 때 앞발 찍기 추가 시전
//
// [히트박스 세팅 방법]
// MidBoss_Spider 아래 자식으로 아래 오브젝트 만들기:
// - "Hitbox_Slash2"  : 휘두르기 판정 (Hitbox_Stamp 재사용 가능)
// - "Hitbox_Return"  : 다리 회수 시 판정 (휘두르기와 반대 방향)
// - "Hitbox_Stamp"   : 조건부 앞발 찍기 (기존 Hitbox_Stamp 재사용)
// 각각 CircleCollider2D (Is Trigger 체크) + EnemyHitbox 붙이고 꺼두기.
// =====================================================
public class MidBossPattern7 : BossPatternBase
{
    [Header("2연 휘두르기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float slashHitboxDuration = 0.25f;   // 휘두르기 히트박스 유지 시간
    [SerializeField] private float returnHitboxDuration = 0.25f;  // 회수 히트박스 유지 시간
    [SerializeField] private float slashInterval = 0.4f;          // 휘두르기 → 회수 간격
    [SerializeField] private float conditionStampRange = 2f;      // 조건부 찍기 발동 범위

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject slashHitbox;   // 앞다리 휘두르기 히트박스
    [SerializeField] private GameObject returnHitbox;  // 다리 회수 히트박스
    [SerializeField] private GameObject stampHitbox;   // 조건부 앞발 찍기 히트박스 (Hitbox_Stamp 재사용)

    private Animator visualAnimator;
    private bool isExecuting = false;

    private void Awake()
    {
        cooldown = 5f;  // 임시 쿨타임 - 기획 확정 후 수정할 것
        visualAnimator = GetComponentInChildren<Animator>();

        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (returnHitbox != null) returnHitbox.SetActive(false);
        if (stampHitbox != null) stampHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        Debug.Log("[MidBossPattern7] 앞다리 휘두르기 시전!");
        if (visualAnimator != null) visualAnimator.Play("Slash Double Attack");
        StartCoroutine(SlashComboRoutine());
    }

    private IEnumerator SlashComboRoutine()
    {
        isExecuting = true;

        // 휘두르기
        ActivateHitbox(slashHitbox);
        yield return new WaitForSeconds(slashHitboxDuration);
        DeactivateHitbox(slashHitbox);

        yield return new WaitForSeconds(slashInterval);

        // 다리 회수 (회수하며 공격)
        ActivateHitbox(returnHitbox);
        yield return new WaitForSeconds(returnHitboxDuration);
        DeactivateHitbox(returnHitbox);

        // 조건부 앞발 찍기 - 범위 안에 플레이어 있을 때만 시전
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            float dist = Vector2.Distance(transform.position, playerObj.transform.position);
            if (dist <= conditionStampRange)
            {
                Debug.Log("[MidBossPattern7] 조건 충족 - 앞발 찍기 추가 시전!");
                if (visualAnimator != null) visualAnimator.Play("Slash Double Attack");
                ActivateHitbox(stampHitbox);
                yield return new WaitForSeconds(0.2f);
                DeactivateHitbox(stampHitbox);
            }
        }

        isExecuting = false;
    }

    private void ActivateHitbox(GameObject hitbox)
    {
        if (hitbox != null) hitbox.SetActive(true);
    }

    private void DeactivateHitbox(GameObject hitbox)
    {
        if (hitbox != null) hitbox.SetActive(false);
    }
}