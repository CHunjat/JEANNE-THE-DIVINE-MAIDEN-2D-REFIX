using UnityEngine;
using System.Collections;

// =====================================================
// BossPatternBase.cs (마스터 뼈대 스크립트)
// 미끄러짐 방지 + 무한 대기 락 해제(안전장치) 완벽 통합 버전
// =====================================================
public abstract class BossPatternBase : MonoBehaviour
{
    public enum DistanceType
    {
        Close,   // 근거리: 0~5m
        Mid,     // 중거리: 5~10m
        Far,     // 원거리: 10~20m
        Any      // 거리 무관
    }

    [Header("패턴 기본 설정 (기획자 조절)")]
    [SerializeField] protected float cooldown = 3f;
    [SerializeField] public int priority = 3;
    [SerializeField] public DistanceType distanceType = DistanceType.Mid;

    [Header("추격 중 사용 여부")]
    [SerializeField] public bool canUseInChase = false;

    [Header("안전장치 (공통)")]
    [SerializeField] protected float maxExecutionTime = 5f;

    private float lastUsedTime = -999f;

    // 상태 제어용 공통 변수 (자식 스크립트에서 더 이상 선언할 필요 없음)
    protected bool isExecuting = false;
    public virtual bool IsBusy => isExecuting;

    protected Animator visualAnimator;
    protected Rigidbody2D rb;
    protected Coroutine failsafeCoroutine;

    // 공통 컴포넌트 초기화
    protected virtual void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public virtual bool IsUsable()
    {
        return Time.time >= lastUsedTime + cooldown;
    }

    // 메인 AI가 호출하는 실행 함수
    public void Execute()
    {
        lastUsedTime = Time.time;

        if (isExecuting) return;
        isExecuting = true;

        // 공통 로직: 공격 시작 시 관성을 0으로 만들어 미끄러짐 1차 방지
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 공통 로직: 안전장치 타이머 시작
        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        failsafeCoroutine = StartCoroutine(FailsafeRoutine());

        // 자식 클래스에서 작성한 실제 공격 로직(애니메이션 재생 등) 실행
        OnExecute();
    }

    // 자식 클래스에서 반드시 구현해야 하는 알맹이 함수
    protected abstract void OnExecute();

    // =====================================================
    // 유니티 애니메이션 끝자락(핀)에서 공통으로 호출받을 함수
    // =====================================================
    public virtual void AnimEvent_Finish()
    {
        if (visualAnimator != null && gameObject.activeInHierarchy)
        {
            // 애니메이터가 다음 상태로 완전히 넘어가기 전까지 발을 묶어둠
            StartCoroutine(WaitForAnimatorTransition());
        }
        else
        {
            EndExecution();
        }
    }

    // 트랜지션 중 미끄러짐(문워크) 완벽 방지 루틴
    protected IEnumerator WaitForAnimatorTransition()
    {
        AnimatorStateInfo stateInfo = visualAnimator.GetCurrentAnimatorStateInfo(0);
        int attackStateHash = stateInfo.shortNameHash;

        // 공격 애니메이션 상태를 완전히 벗어날 때까지 대기
        while (visualAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == attackStateHash)
        {
            // 남은 프레임 동안 X축 속도를 강제로 0 고정
            if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            yield return null;
        }

        EndExecution();
    }

    // 패턴 완전 종료 및 락 해제
    public virtual void EndExecution()
    {
        isExecuting = false;
        if (failsafeCoroutine != null)
        {
            StopCoroutine(failsafeCoroutine);
            failsafeCoroutine = null;
        }
    }

    // 애니메이션 핀이 스킵되었을 때를 대비한 공통 안전장치
    protected IEnumerator FailsafeRoutine()
    {
        yield return new WaitForSeconds(maxExecutionTime);

        if (isExecuting)
        {
            Debug.LogWarning($"[{GetType().Name}] 핀(AnimEvent_Finish) 스킵됨! 강제로 AI 락 해제.");
            isExecuting = false;
        }
        failsafeCoroutine = null;
    }
}