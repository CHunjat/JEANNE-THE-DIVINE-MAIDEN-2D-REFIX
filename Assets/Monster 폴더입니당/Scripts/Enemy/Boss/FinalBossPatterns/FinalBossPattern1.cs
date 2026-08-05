using UnityEngine;
using System.Collections;

// FinalBossPattern1.cs
// 데몬 누나 1페이즈 패턴 1 - 손톱 베기
//
// 기획 문서 기준
// - 바라보는 방향으로 4개의 손톱을 휘둘러 공격함
// - 1,2번째 공격: 왼손 한 세트 (짧은 딜레이)
// - 3,4번째 공격: 오른손 한 세트 (짧은 딜레이)
// - 1세트에서 2세트 전환 시 조금 더 긴 딜레이
public class FinalBossPattern1 : FinalBossPatternBase
{
    [Header("손톱 베기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float hitboxActiveDuration = 0.15f;
    [SerializeField] private float shortDelay = 0.2f;
    [SerializeField] private float longDelay = 0.5f;

    [Header("히트박스 연결 - 인스펙터에서 각 히트박스 오브젝트를 드래그해서 넣을 것")]
    [SerializeField] private GameObject leftClawHitbox;
    [SerializeField] private GameObject rightClawHitbox;

    protected override void Awake()
    {
        base.Awake();
        cooldown = 3f;

        if (leftClawHitbox != null) leftClawHitbox.SetActive(false);
        if (rightClawHitbox != null) rightClawHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("FinalBossPattern1 손톱 베기 시전!");
        StartCoroutine(ClawComboRoutine());
    }

    private IEnumerator ClawComboRoutine()
    {
        ActivateHitbox(leftClawHitbox);
        yield return new WaitForSeconds(hitboxActiveDuration);
        DeactivateHitbox(leftClawHitbox);

        yield return new WaitForSeconds(shortDelay);

        ActivateHitbox(leftClawHitbox);
        yield return new WaitForSeconds(hitboxActiveDuration);
        DeactivateHitbox(leftClawHitbox);

        yield return new WaitForSeconds(longDelay);

        ActivateHitbox(rightClawHitbox);
        yield return new WaitForSeconds(hitboxActiveDuration);
        DeactivateHitbox(rightClawHitbox);

        yield return new WaitForSeconds(shortDelay);

        ActivateHitbox(rightClawHitbox);
        yield return new WaitForSeconds(hitboxActiveDuration);
        DeactivateHitbox(rightClawHitbox);

        EndExecution();
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