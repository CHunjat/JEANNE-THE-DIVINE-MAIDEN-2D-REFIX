using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern7.cs
// 강화 앞다리 휘두르기 - 중거리, 쿨타임 7초, 우선순위 4
// (isExecuting 재진입 방지 + 안전장치 추가본)
// =====================================================
public class MidBossPattern7 : BossPatternBase
{
    [Header("2연 휘두르기 설정 (기획자 조절)")]
    [SerializeField] private float slashHitboxDuration = 0.25f;
    [SerializeField] private float returnHitboxDuration = 0.25f;
    [SerializeField] private float conditionStampRange = 2f;

    [Header("특수 히트박스 연결 (얘만 슬롯 유지함)")]
    [SerializeField] private GameObject returnHitbox;

    [Header("안전장치 - 애니메이션 이벤트 누락 시 강제 리셋 (기획자 조절)")]
    [Tooltip("이 시간이 지나도 패턴이 끝나지 않으면 강제로 isExecuting을 false로 되돌림.")]
    [SerializeField] private float maxExecutionTime = 5f;

    private GameObject slashHitbox;
    private GameObject stampHitbox;
    private Animator visualAnimator;
    private bool isExecuting = false;
    private Coroutine failsafeCoroutine;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null)
        {
            slashHitbox = parent.hitBox_Slash;
            stampHitbox = parent.hitBox_Stamp;
        }

        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (returnHitbox != null) returnHitbox.SetActive(false);
        if (stampHitbox != null) stampHitbox.SetActive(false);

        // 기획서 반영
        cooldown = 7f;
        priority = 4;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        isExecuting = true;

        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashDouble");

        // 안전장치 시작: maxExecutionTime 안에 정상 종료 안 되면 강제 리셋
        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        failsafeCoroutine = StartCoroutine(FailsafeRoutine());
    }

    public void AnimEvent_Slash1()
    {
        if (slashHitbox != null)
        {
            slashHitbox.SetActive(true);
            Invoke(nameof(DeactivateSlash), slashHitboxDuration);
        }
    }

    public void AnimEvent_SlashReturn()
    {
        if (returnHitbox != null)
        {
            returnHitbox.SetActive(true);
            Invoke(nameof(DeactivateReturn), returnHitboxDuration);
        }
    }

    public void AnimEvent_CheckConditionStamp()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && Vector2.Distance(transform.position, playerObj.transform.position) <= conditionStampRange)
        {
            if (visualAnimator != null) visualAnimator.SetTrigger("doSlashTriple");
            // isExecuting은 아직 안 끝남. AnimEvent_ConditionStampHit에서 최종 종료 처리.
        }
        else
        {
            // 조건 불충족 시 여기서 패턴 종료 (앞다리 휘두르기만 하고 끝)
            EndExecution();
        }
    }

    public void AnimEvent_ConditionStampHit()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateStamp), 0.2f);
        }

        EndExecution();
    }

    private void EndExecution()
    {
        isExecuting = false;
        if (failsafeCoroutine != null)
        {
            StopCoroutine(failsafeCoroutine);
            failsafeCoroutine = null;
        }
    }

    private IEnumerator FailsafeRoutine()
    {
        yield return new WaitForSeconds(maxExecutionTime);

        if (isExecuting)
        {
            Debug.LogWarning($"[{gameObject.name}] MidBossPattern7 안전장치 발동! " +
                              $"{maxExecutionTime}초 안에 정상 종료되지 않아 강제 리셋함. " +
                              "Animator의 doSlashTriple 관련 상태/전환이 제대로 연결되어 있는지 확인 필요.");
            isExecuting = false;
        }
        failsafeCoroutine = null;
    }

    private void DeactivateSlash() { if (slashHitbox != null) slashHitbox.SetActive(false); }
    private void DeactivateReturn() { if (returnHitbox != null) returnHitbox.SetActive(false); }
    private void DeactivateStamp() { if (stampHitbox != null) stampHitbox.SetActive(false); }
}