using UnityEngine;

// =====================================================
// EliteMonster.cs
// 엘리트 몬스터 스크립트임.
// 일반 몬스터보다 강하며, 공격 패턴이 3개임.
// 플레이어 공격에 경직이 없음 (문서 기준).
// 일반 대쉬로 통과 가능함.
//
// [기획 문서 기준]
// - 같은 맵 일반 몬스터 대비 10~30배 HP
// - 경직 면역
// - 특수 기믹 또는 특수 스킬 보유 (기획 확정 후 채울 것)
// =====================================================
public class EliteMonster : EnemyFSM
{
    [Header("넉백 설정 - 기획 확정 후 수정 또는 삭제할 것")]
    [SerializeField] private float knockbackForce = 2f;       // 넉백 강도 (엘리트는 일반보다 덜 밀림)
    [SerializeField] private float knockbackDuration = 0.15f; // 넉백 지속 시간 (초 단위)

    [Header("공격 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float attackCooldown = 2.5f;     // 공격 쿨타임 (초 단위)
    [SerializeField] private float damageRatio = 1.0f;        // 공격력 반영 비율 (1.0 = 100%)
    [SerializeField] private LayerMask playerLayer;           // Player 레이어 (인스펙터에서 반드시 설정할 것)

    private float lastAttackTime = -999f;  // 마지막 공격 시각

    protected override void Awake()
    {
        // 임시 수치 - 기획 확정 후 수정할 것 (일반 몬스터보다 높게 설정)
        maxHp = 300f;        // 체력 (일반 몬스터 대비 3배, 기획 확정 후 10~30배로 조정)
        moveSpeed = 2.5f;    // 이동 속도
        attackDamage = 25f;  // 기본 공격력
        detectRange = 6f;    // 감지 범위
        attackRange = 1.5f;  // 공격 범위

        base.Awake();
        SetCollisionWithPlayer(false);  // 일반 대쉬로 통과 가능하게 설정
    }

    // 대기 상태 - 매 프레임 실행됨
    protected override void OnIdle()
    {
        if (GetDistanceToPlayer() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    // 추격 상태 - 매 프레임 실행됨
    protected override void OnChase()
    {
        if (GetDistanceToPlayer() <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (GetDistanceToPlayer() > detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        rb.linearVelocity = GetDirectionToPlayer() * moveSpeed;
    }

    // 공격 상태 - 매 프레임 실행됨
    protected override void OnAttack()
    {
        rb.linearVelocity = Vector2.zero;

        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    // 피격 상태 - 경직 면역이므로 넉백만 적용함
    protected override void OnHit()
    {
        // 경직 면역 - 별도 경직 처리 없음 (문서 기준)
    }

    // 사망 상태
    protected override void OnDead()
    {
        rb.linearVelocity = Vector2.zero;
        // 사망 연출 - 기획 확정 후 채울 것
    }

    // 피격 처리 오버라이드 - 부모 인수에 맞춰 변경 완료!
    public override void TakeDamage(float amount, float groggyDamage = 0f)
    {
        base.TakeDamage(amount, groggyDamage);
        if (currentHp > 0)
            ApplyKnockback();
    }

    // 실제 공격 수행
    private void PerformAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit != null)
        {
            PlayerStats playerStats = hit.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                float damage = attackDamage * damageRatio;
                playerStats.TakeDamage(damage);
                Debug.Log($"[{gameObject.name}] 플레이어에게 {damage} 데미지 적용함.");
            }
        }
    }

    // 넉백 적용
    private void ApplyKnockback()
    {
        Vector2 knockbackDir = -GetDirectionToPlayer();
        rb.linearVelocity = knockbackDir * knockbackForce;
        Invoke(nameof(StopKnockback), knockbackDuration);
    }

    private void StopKnockback()
    {
        rb.linearVelocity = Vector2.zero;
        ChangeState(EnemyState.Chase);
    }
}