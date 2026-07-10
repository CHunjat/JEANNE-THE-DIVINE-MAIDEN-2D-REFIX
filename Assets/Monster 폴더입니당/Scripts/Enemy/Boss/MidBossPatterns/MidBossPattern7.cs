using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern7.cs
// 강화 슬래시 (Slash Triple Attack - 통짜 3연타 버전)
// (그로기로 인한 정상 중단 시 안전장치 경고 스킵하도록 수정)
// =====================================================
public class MidBossPattern7 : BossPatternBase
{
    [Header("3연 휘두르기 설정 (기획자 조절)")]
    [SerializeField] private float slashHitboxDuration = 0.25f;
    [SerializeField] private float returnHitboxDuration = 0.25f;
    [SerializeField] private float stampHitboxDuration = 0.2f;

    [Header("특수 히트박스 연결 (얘만 슬롯 유지함)")]
    [SerializeField] private GameObject returnHitbox;

    [Header("안전장치 - 애니메이션 이벤트 누락 시 강제 리셋")]
    [SerializeField] private float maxExecutionTime = 5f;

    private GameObject slashHitbox;
    private GameObject stampHitbox;
    private Animator visualAnimator;
    private EnemyGroggy groggy; // [추가됨] 그로기 상태 확인용
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

        groggy = GetComponent<EnemyGroggy>(); // [추가됨]

        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (returnHitbox != null) returnHitbox.SetActive(false);
        if (stampHitbox != null) stampHitbox.SetActive(false);

        cooldown = 7f;
        priority = 4;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        isExecuting = true;

        // [핵심] doSlashDouble 버리고, 무조건 3연타 'doSlashTriple' 한방에 발동!
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashTriple");

        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        failsafeCoroutine = StartCoroutine(FailsafeRoutine());
    }

    public void AnimEvent_Slash1()
    {
        if (slashHitbox != null)
        {
            slashHitbox.SetActive(false);
            slashHitbox.SetActive(true);
            Invoke(nameof(DeactivateSlash), slashHitboxDuration);
        }
    }

    public void AnimEvent_SlashReturn()
    {
        if (returnHitbox != null)
        {
            returnHitbox.SetActive(false);
            returnHitbox.SetActive(true);
            Invoke(nameof(DeactivateReturn), returnHitboxDuration);
        }
    }

    // [핵심] 통짜 애니메이션이므로 중간에 거리 잴 필요 없음. 에러 방지용 빈 껍데기!
    public void AnimEvent_CheckConditionStamp()
    {
    }

    public void AnimEvent_ConditionStampHit()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(false);
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateStamp), stampHitboxDuration);
        }
        // 마지막 3타 찍었으니 여기서 패턴 깔끔하게 종료!
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

    // [수정됨] 그로기로 인한 정상적인 중단이면 경고 없이 조용히 리셋
    private IEnumerator FailsafeRoutine()
    {
        yield return new WaitForSeconds(maxExecutionTime);

        if (isExecuting)
        {
            bool wasInterruptedByGroggy = (groggy != null && groggy.IsGroggy);

            if (!wasInterruptedByGroggy)
            {
                Debug.LogWarning($"[{gameObject.name}] 패턴7 안전장치 발동! 5초 초과 리셋. " +
                                  "Animator 클립에 해당 이벤트가 제대로 연결되어 있는지 확인 필요.");
            }
            isExecuting = false;
        }
        failsafeCoroutine = null;
    }

    private void DeactivateSlash() { if (slashHitbox != null) slashHitbox.SetActive(false); }
    private void DeactivateReturn() { if (returnHitbox != null) returnHitbox.SetActive(false); }
    private void DeactivateStamp() { if (stampHitbox != null) stampHitbox.SetActive(false); }
}