using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern7.cs
// 강화 슬래시 (Slash Triple Attack - 통짜 3연타 버전)
// (MidBoss.cs와 함수명 충돌 방지: AnimEvent_Stamp -> AnimEvent_TripleStampHit)
// =====================================================
public class MidBossPattern7 : BossPatternBase
{
    [Header("3연 휘두르기 설정")]
    [SerializeField] private float slashHitboxDuration = 0.25f;
    [SerializeField] private float returnHitboxDuration = 0.25f;
    [SerializeField] private float stampHitboxDuration = 0.2f;
    [Header("특수 히트박스 연결")]
    [SerializeField] private GameObject returnHitbox;
    [Header("안전장치")]
    [SerializeField] private float maxExecutionTime = 5f;
    private GameObject slashHitbox;
    private GameObject stampHitbox;
    private Animator visualAnimator;
    private EnemyGroggy groggy;
    private bool isExecuting = false;
    private Coroutine failsafeCoroutine;
    public override bool IsBusy => isExecuting;
    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null)
        {
            slashHitbox = parent.hitBox_Slash;
            stampHitbox = parent.hitBox_Stamp;
        }
        groggy = GetComponent<EnemyGroggy>();
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
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashTriple");
        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        failsafeCoroutine = StartCoroutine(FailsafeRoutine());
    }
    public void AnimEvent_Slash1()
    {
        if (slashHitbox != null)
        {
            StartCoroutine(ReactivateHitboxRoutine(slashHitbox, slashHitboxDuration));
        }
    }
    public void AnimEvent_SlashReturn()
    {
        if (returnHitbox != null)
        {
            StartCoroutine(ReactivateHitboxRoutine(returnHitbox, returnHitboxDuration));
        }
    }
    public void AnimEvent_CheckConditionStamp() { }

    // [변경됨] AnimEvent_Stamp -> AnimEvent_TripleStampHit
    // MidBoss.cs에 이미 있는 AnimEvent_Stamp와 이름이 겹쳐서 충돌하던 문제 수정
    public void AnimEvent_TripleStampHit()
    {
        if (stampHitbox != null)
        {
            StartCoroutine(ReactivateHitboxRoutine(stampHitbox, stampHitboxDuration));
        }
        // 3타 이후 일어나는 애니메이션 대기 후 패턴 종료
        Invoke(nameof(EndExecution), stampHitboxDuration + 0.6f);
    }

    // 물리엔진 판정 리셋을 위한 1프레임 대기 처리
    private IEnumerator ReactivateHitboxRoutine(GameObject hitbox, float duration)
    {
        hitbox.SetActive(false);
        yield return null;
        hitbox.SetActive(true);
        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
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
            isExecuting = false;
        }
        failsafeCoroutine = null;
    }
}