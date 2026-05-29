using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// =====================================================
// MidBoss.cs
// 중간 보스 (자이언트 스파이더) 스크립트임.
//
// [기획 문서 기준 - 자이언트 스파이더]
// - 컨셉: 엇박과 연타
// - 1페이즈 패턴: 앞발 찍기 / 앞발 휘두르기 / 거미줄 뱉기 / 점프 공격
// - 2페이즈 패턴: 거미줄 뱉기 / 거미줄+점프 / 2연찍기+조건부 뒷발 / 앞발 2연휘두르기+조건부 찍기
// - 체력 70% 이하 시 2페이즈로 전환 (특수 패턴 시전 후 전환)
// - 플레이어 공격에 경직 없음
// - UI에 HP 바 표시됨
// - 패링 성공 시 일정 시간 통과 가능 상태
//
// [패턴 연결 방법]
// 이 오브젝트의 자식으로 MidBossPattern1~5 오브젝트를 만들고
// 해당 스크립트를 붙이면 Awake에서 자동으로 인식함.
// =====================================================
public class MidBoss : EnemyFSM
{
    [Header("페이즈 설정")]
    [SerializeField] private int currentPhase = 1;            // 현재 페이즈 (인스펙터에서 실시간 확인 가능)
    [SerializeField] private float phase2HpRatio = 0.7f;      // 이 비율 이하로 HP가 떨어지면 2페이즈 전환 (기본값: 70%)

    [Header("패링 대쉬 통과 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float parryPassDuration = 1.5f;  // 패링 성공 후 통과 가능 시간 (초 단위)
    private bool isParryPassActive = false;                    // 현재 통과 가능 상태인지 여부

    private bool isPhaseChanging = false;  // 페이즈 전환 중인지 여부 (전환 중에는 행동 중지)

    // 페이즈별 패턴 목록 - 자식 오브젝트에서 자동으로 수집됨
    private List<BossPatternBase> phase1Patterns = new List<BossPatternBase>();
    private List<BossPatternBase> phase2Patterns = new List<BossPatternBase>();

    protected override void Awake()
    {
        // 임시 수치 - 기획 확정 후 수정할 것
        maxHp = 500f;        // 체력
        moveSpeed = 2.5f;    // 이동 속도
        attackDamage = 20f;  // 기본 공격력
        detectRange = 8f;    // 감지 범위
        attackRange = 2f;    // 공격 범위

        base.Awake();
        SetCollisionWithPlayer(true);  // 보스는 기본적으로 플레이어와 충돌함

        // 자식 오브젝트에 붙어있는 패턴 스크립트를 자동으로 수집함
        // Phase1Pattern 태그가 붙은 오브젝트는 1페이즈, Phase2Pattern은 2페이즈로 분류
        // (지금은 모두 1페이즈로 임시 처리 - 기획 확정 후 분리할 것)
        BossPatternBase[] allPatterns = GetComponents<BossPatternBase>();
        foreach (var p in allPatterns)
            phase1Patterns.Add(p);

        Debug.Log($"[MidBoss] 패턴 {phase1Patterns.Count}개 로드됨.");
    }

    // 피격 처리 - 부모의 체력 감소 후 페이즈 전환 체크
    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        CheckPhaseChange();
    }

    // 대기 상태
    protected override void OnIdle()
    {
        if (GetDistanceToPlayer() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    // 추격 상태
    protected override void OnChase()
    {
        if (isPhaseChanging) return;  // 페이즈 전환 중에는 움직이지 않음

        if (GetDistanceToPlayer() <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        rb.linearVelocity = GetDirectionToPlayer() * moveSpeed;
    }

    // 공격 상태 - 사용 가능한 패턴 중 하나를 실행함
    protected override void OnAttack()
    {
        if (isPhaseChanging) return;

        rb.linearVelocity = Vector2.zero;

        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 현재 페이즈의 패턴 목록에서 쿨타임이 끝난 패턴을 실행함
        List<BossPatternBase> currentPatterns = (currentPhase == 1) ? phase1Patterns : phase2Patterns;
        foreach (var pattern in currentPatterns)
        {
            if (pattern.IsUsable())
            {
                pattern.Execute();
                break;  // 한 번에 하나씩만 실행
            }
        }
    }

    // 피격 상태 - 보스는 경직 없음 (문서 기준)
    protected override void OnHit()
    {
        // 경직 면역 - 별도 처리 없음
        // 그로기 수치 관련 처리 - 기획 확정 후 채울 것
    }

    // 사망 상태
    protected override void OnDead()
    {
        rb.linearVelocity = Vector2.zero;
        // 사망 연출 - 기획 확정 후 채울 것
    }

    // 페이즈 전환 조건 체크 - TakeDamage 때마다 호출됨
    private void CheckPhaseChange()
    {
        if (currentPhase == 1 && currentHp <= maxHp * phase2HpRatio && !isPhaseChanging)
        {
            Debug.Log("[MidBoss] 체력 70% 이하! 2페이즈 전환 시작.");
            StartCoroutine(PhaseChangeRoutine());
        }
    }

    // 2페이즈 전환 연출 코루틴
    private IEnumerator PhaseChangeRoutine()
    {
        isPhaseChanging = true;
        rb.linearVelocity = Vector2.zero;

        Debug.Log("[MidBoss] 페이즈 전환 특수 패턴 시전 중...");
        // 특수 패턴 시전 - 기획 확정 후 채울 것
        // 중간 보스는 이속/공속 증가 없음 (문서 기준)
        yield return new WaitForSeconds(1f);  // 임시 대기 시간 (특수 패턴으로 교체할 것)

        currentPhase = 2;
        Debug.Log("[MidBoss] 2페이즈 전환 완료.");

        isPhaseChanging = false;
        ChangeState(EnemyState.Chase);
    }

    // 플레이어가 패링 성공 시 플레이어 스크립트에서 호출할 함수
    // 호출 방법: midbossObject.GetComponent<MidBoss>().OnParrySuccess();
    public void OnParrySuccess()
    {
        if (!isParryPassActive)
        {
            Debug.Log("[MidBoss] 패링 성공! 일시적으로 통과 가능 상태.");
            StartCoroutine(ParryPassRoutine());
        }
    }

    // 패링 후 일정 시간 동안 통과 가능하게 처리
    private IEnumerator ParryPassRoutine()
    {
        isParryPassActive = true;
        SetCollisionWithPlayer(false);  // 충돌 OFF
        yield return new WaitForSeconds(parryPassDuration);
        SetCollisionWithPlayer(true);   // 충돌 ON
        isParryPassActive = false;
        Debug.Log("[MidBoss] 패링 통과 시간 종료. 충돌 복구됨.");
    }
}