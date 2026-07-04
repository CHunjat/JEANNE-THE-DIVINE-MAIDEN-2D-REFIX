using UnityEngine;
using System.Collections;

// =====================================================
// FinalBossPattern1.cs
// 데몬 누나 1페이즈 패턴 1 - 손톱 베기
//
// [기획 문서 기준]
// - 바라보는 방향으로 4개의 손톱을 휘둘러 공격함
// - 1,2번째 공격: 왼손 한 세트 (짧은 딜레이)
// - 3,4번째 공격: 오른손 한 세트 (짧은 딜레이)
// - 1세트 → 2세트 전환 시 조금 더 긴 딜레이
// - 딜레이 프레임은 애니메이션 작업 시 조정 필요
//
// [히트박스 세팅 방법]
// 1. FinalBoss_Demon 아래 자식으로 "Hitbox_Claw_L", "Hitbox_Claw_R" 만들기
// 2. 각각 Collider2D (Is Trigger 체크) + EnemyHitbox 스크립트 붙이기
// 3. 이 스크립트의 leftClawHitbox, rightClawHitbox 필드에 드래그해서 넣기
// 4. 씬 뷰에서 히트박스 위치를 각 손톱 위치에 맞게 조정
// =====================================================
public class FinalBossPattern1 : FinalBossPatternBase
{
    [Header("손톱 베기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float hitboxActiveDuration = 0.15f;  // 히트박스 유지 시간 (초)
    [SerializeField] private float shortDelay = 0.2f;             // 같은 세트 내 딜레이 (초)
    [SerializeField] private float longDelay = 0.5f;              // 세트 전환 딜레이 (초)

    [Header("히트박스 연결 - 인스펙터에서 각 히트박스 오브젝트를 드래그해서 넣을 것")]
    [SerializeField] private GameObject leftClawHitbox;   // 왼손 히트박스 (1,2번째 공격)
    [SerializeField] private GameObject rightClawHitbox;  // 오른손 히트박스 (3,4번째 공격)

    private void Awake()
    {
        cooldown = 3f;  // 임시 쿨타임 - 기획 확정 후 수정할 것

        if (leftClawHitbox != null) leftClawHitbox.SetActive(false);
        if (rightClawHitbox != null) rightClawHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("[FinalBossPattern1] 손톱 베기 시전!");
        StartCoroutine(ClawComboRoutine());
    }

    private IEnumerator ClawComboRoutine()
    {
        // 1번 공격 - 왼손
        ActivateHitbox(leftClawHitbox);
        yield return new WaitForSeconds(hitboxActiveDuration);
        DeactivateHitbox(leftClawHitbox);

        yield return new WaitForSeconds(shortDelay);

        // 2번 공격 - 왼손
        ActivateHitbox(leftClawHitbox);
        yield return new WaitForSeconds(hitboxActiveDuration);
        DeactivateHitbox(leftClawHitbox);

        yield return new WaitForSeconds(longDelay);  // 세트 전환 딜레이

        // 3번 공격 - 오른손
        ActivateHitbox(rightClawHitbox);
        yield return new WaitForSeconds(hitboxActiveDuration);
        DeactivateHitbox(rightClawHitbox);

        yield return new WaitForSeconds(shortDelay);

        // 4번 공격 - 오른손
        ActivateHitbox(rightClawHitbox);
        yield return new WaitForSeconds(hitboxActiveDuration);
        DeactivateHitbox(rightClawHitbox);
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