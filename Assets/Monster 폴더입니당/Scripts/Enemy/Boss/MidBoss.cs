using UnityEngine;
using System.Collections.Generic;

// =====================================================
// MidBoss.cs
// 중간 보스(거미)의 핵심 AI 및 패턴 제어 클래스
// =====================================================
public class MidBoss : EnemyFSM
{
    [Header("페이즈 설정")]
    [SerializeField] private int currentPhase = 1;
    [SerializeField] private float phase2Threshold = 0.5f; // 체력 50% 이하 시 2페이즈
    private bool isPhaseChanging = false;

    private List<BossPatternBase> phase1Patterns = new List<BossPatternBase>();
    private List<BossPatternBase> phase2Patterns = new List<BossPatternBase>();

    protected override void Awake()
    {
        base.Awake();

        // 몬스터에 붙어있는 모든 패턴 스크립트 가져오기
        BossPatternBase[] allPatterns = GetComponents<BossPatternBase>();

        foreach (var p in allPatterns)
        {
            // [🚨 에러 해결된 부분]
            // 스크립트 파일이 없어도 컴파일 에러가 나지 않도록, 클래스의 '이름(문자열)'으로 2페이즈를 검사함!
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
        if (isPhaseChanging) return;
        base.TakeDamage(amount);
        CheckPhaseTransition();
    }

    private void CheckPhaseTransition()
    {
        if (currentPhase == 1 && currentHp <= maxHp * phase2Threshold)
        {
            currentPhase = 2;
            isPhaseChanging = true;
            Debug.Log("[MidBoss] 2페이즈 돌입!");

            // 2페이즈 전환 시 포효 등의 처리용 임시 딜레이
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

        // 감지 범위 안에 들어오면 추적 시작
        if (GetDistanceToPlayer() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    protected override void OnChase()
    {
        if (isPhaseChanging) return;

        if (animator != null)
            animator.SetBool("isMoving", true);

        // 공격 범위 안에 들어오면 공격 시작
        if (GetDistanceToPlayer() <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // 플레이어 쪽으로 고개 돌리기 (EnemyBase의 함수 호출)
        FlipTowardsPlayer();

        // Y축(위아래)은 무시하고 X축(좌우) 방향만 계산해서 이동 (땅 파고들기 방지)
        if (player != null)
        {
            float moveDirX = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(moveDirX * moveSpeed, rb.linearVelocity.y);
        }
    }

    protected override void OnAttack()
    {
        if (isPhaseChanging) return;

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", true);
        }

        // 공격 중에는 이동 멈춤
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 플레이어가 사거리 밖으로 나가면 다시 추적
        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 플레이어를 바라보면서 공격
        FlipTowardsPlayer();

        // 현재 페이즈에 맞는 패턴 실행
        List<BossPatternBase> currentPatterns = (currentPhase == 1) ? phase1Patterns : phase2Patterns;
        foreach (var pattern in currentPatterns)
        {
            if (pattern.IsUsable())
            {
                pattern.Execute();
                break;
            }
        }
    }

    protected override void OnHit()
    {
        // 피격 모션 처리
    }

    protected override void OnDead()
    {
        rb.linearVelocity = Vector2.zero;
        if (animator != null)
            animator.SetBool("isDead", true);
    }
}