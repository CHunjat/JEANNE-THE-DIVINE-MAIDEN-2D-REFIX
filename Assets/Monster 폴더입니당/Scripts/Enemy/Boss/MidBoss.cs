using UnityEngine;
using System.Collections.Generic;

// =====================================================
// MidBoss.cs
// 중간 보스(거미)의 핵심 AI 스크립트.
// FSM(상태 머신)을 기반으로 행동 상태를 제어함.
// =====================================================
public class MidBoss : EnemyFSM
{
    [Header("페이즈 설정")]
    [SerializeField] private int currentPhase = 1;
    [SerializeField] private float phase2Threshold = 0.5f; // 2페이즈 전환 체력 비율 (0.5 = 50%)
    private bool isPhaseChanging = false; // 페이즈 전환 상태 체크

    [Header("보스 공격 딜레이")]
    private float nextAttackTime = 0f; // 다음 공격 가능 시간. OnAttack() 내부에서 딜레이 조절 시 사용함.

    private List<BossPatternBase> phase1Patterns = new List<BossPatternBase>();
    private List<BossPatternBase> phase2Patterns = new List<BossPatternBase>();

    protected override void Awake()
    {
        base.Awake();

        // 보스에 부착된 모든 패턴 스크립트를 가져옴.
        BossPatternBase[] allPatterns = GetComponents<BossPatternBase>();

        // 스크립트 이름 기준으로 1, 2페이즈 패턴을 자동 분류함.
        foreach (var p in allPatterns)
        {
            string patternName = p.GetType().Name;
            if (patternName == "MidBossPattern6" || patternName == "MidBossPattern7" || patternName == "MidBossPattern8")
            {
                phase2Patterns.Add(p);
            }
            else
            {
                phase1Patterns.Add(p);
            }
        }
    }

    public override void TakeDamage(float amount)
    {
        // 페이즈 전환 중에는 데미지 무시함.
        if (isPhaseChanging) return;

        base.TakeDamage(amount);
        CheckPhaseTransition();
    }

    private void CheckPhaseTransition()
    {
        // 1페이즈 상태에서 체력이 임계점 이하로 떨어지면 2페이즈로 전환함.
        if (currentPhase == 1 && currentHp <= maxHp * phase2Threshold)
        {
            currentPhase = 2;
            isPhaseChanging = true;
            Debug.Log("[MidBoss] 2페이즈 돌입!");

            // 2페이즈 전환 시 연출을 위한 대기 시간.
            Invoke(nameof(EndPhaseTransition), 2f);
        }
    }

    private void EndPhaseTransition()
    {
        isPhaseChanging = false;
        ChangeState(EnemyState.Chase);
    }

    protected override void OnIdle()
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", false);
        }

        // 감지 범위 안에 들어오면 추격 상태로 전환함.
        if (GetDistanceToPlayer() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    protected override void OnChase()
    {
        // 페이즈 전환 중에는 이동하지 않음.
        if (isPhaseChanging) return;

        if (animator != null)
            animator.SetBool("isMoving", true);

        // 공격 사거리 안에 들어오면 공격 상태로 전환함.
        if (GetDistanceToPlayer() <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        FlipTowardsPlayer();

        // 플레이어 방향으로 이동함 (Y축은 무시하여 비정상적인 상하 이동 방지).
        if (player != null)
        {
            float moveDirX = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(moveDirX * moveSpeed, rb.linearVelocity.y);
        }
    }

    protected override void OnAttack()
    {
        if (isPhaseChanging) return;

        // 다음 공격 타이밍 전까지 이동을 멈추고 대기함 (다중 패턴 동시 실행 버그 방지).
        if (Time.time < nextAttackTime)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", true);
        }

        // 공격 중 이동 멈춤.
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 플레이어가 사거리 밖으로 나가면 추격 상태로 복귀함.
        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        FlipTowardsPlayer();

        List<BossPatternBase> currentPatterns = (currentPhase == 1) ? phase1Patterns : phase2Patterns;

        foreach (var pattern in currentPatterns)
        {
            // 쿨타임이 지난 사용 가능한 패턴을 찾아서 실행함.
            if (pattern.IsUsable())
            {
                pattern.Execute();

                // 패턴 실행 후 다음 공격까지의 딜레이 타임 설정 (난이도에 맞춰 숫자 조절).
                nextAttackTime = Time.time + 3.5f;

                break;
            }
        }
    }

    protected override void OnHit()
    {
        // 피격 모션 및 셰이더 처리용.
    }

    protected override void OnDead()
    {
        // 사망 시 물리 이동을 멈추고 애니메이션 재생함.
        rb.linearVelocity = Vector2.zero;
        if (animator != null)
            animator.SetBool("isDead", true);
    }
}