using UnityEngine;

// =====================================================
// NormalMonster.cs
// 일반 몬스터 스크립트임.
// 플레이어를 감지하면 추격하고, 공격 범위에 들어오면 공격함.
// 피격 시 넉백(뒤로 밀려남)이 발생함.
// 일반 대쉬로 항상 통과 가능함 (SetCollisionWithPlayer(false)).
//
// [기획 문서 기준]
// - 경직 없음 (보스/엘리트에만 해당)
// - 기본 공격 패턴 보유
// =====================================================
public class NormalMonster : EnemyFSM
{
    [Header("넉백 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float knockbackForce = 3f;       // 넉백 강도 (숫자가 클수록 더 많이 밀려남)
    [SerializeField] private float knockbackDuration = 0.2f;  // 넉백 지속 시간 (초 단위)

    [Header("공격 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float attackCooldown = 2f;       // 공격 쿨타임 (초 단위)
    [SerializeField] private float damageRatio = 1.0f;        // 공격력 반영 비율 (1.0 = 100%)
    [SerializeField] private LayerMask playerLayer;           // Player 레이어 (인스펙터에서 반드시 설정할 것)

    private float lastAttackTime = -999f;  // 마지막 공격 시각

    protected override void Awake()
    {
        // 임시 수치 - 기획 확정 후 수정할 것
        maxHp = 100f;        // 체력
        moveSpeed = 3f;      // 이동 속도
        attackDamage = 10f;  // 기본 공격력
        detectRange = 5f;    // 감지 범위
        attackRange = 1.5f;  // 공격 범위

        base.Awake();
        SetCollisionWithPlayer(false);  // 일반 대쉬로 통과 가능하게 설정
    }

    // 대기 상태 - 매 프레임 실행됨
    protected override void OnIdle()
    {
        // 플레이어가 감지 범위 안에 들어오면 추격 시작
        if (GetDistanceToPlayer() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    // 추격 상태 - 매 프레임 실행됨
    protected override void OnChase()
    {
        // 공격 범위 안에 들어오면 공격 상태로 전환
        if (GetDistanceToPlayer() <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // 감지 범위를 벗어나면 대기 상태로 복귀
        if (GetDistanceToPlayer() > detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        // 플레이어 방향으로 이동
        rb.linearVelocity = GetDirectionToPlayer() * moveSpeed;
    }

    // 공격 상태 - 매 프레임 실행됨
    protected override void OnAttack()
    {
        rb.linearVelocity = Vector2.zero;  // 공격 중에는 정지

        // 공격 범위를 벗어나면 다시 추격
        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 쿨타임이 지났으면 공격 실행
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    // 피격 상태 - 매 프레임 실행됨
    protected override void OnHit()
    {
        // 넉백은 TakeDamage에서 호출하므로 여기서는 별도 처리 없음
    }

    // 사망 상태 - 매 프레임 실행됨
    protected override void OnDead()
    {
        rb.linearVelocity = Vector2.zero;
        // 사망 연출 - 기획 확정 후 채울 것 (파티클, 드롭 아이템 등)
    }

    // 피격 처리 오버라이드 - 부모 인수에 맞춰 변경 완료!
    public override void TakeDamage(float amount, float groggyDamage = 0f)
    {
        base.TakeDamage(amount, groggyDamage);  // 체력 감소 및 사망 처리
        if (currentHp > 0)
            ApplyKnockback();     // 살아있을 때만 넉백 적용
    }

    // 실제 공격 수행 - 공격 범위 안의 플레이어에게 데미지를 줌
    private void PerformAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit != null)
        {
            PlayerStats playerHealth = hit.GetComponent<PlayerStats>();
            if (playerHealth != null)
            {
                float damage = attackDamage * damageRatio;  // 공격력 × 반영 비율
                playerHealth.TakeDamage(damage);
                Debug.Log($"[{gameObject.name}] 플레이어에게 {damage} 데미지 적용함.");
            }
        }
    }

    // 넉백 적용 - 플레이어 반대 방향으로 밀려남
    private void ApplyKnockback()
    {
        Vector2 knockbackDir = -GetDirectionToPlayer();  // 플레이어 반대 방향
        rb.linearVelocity = knockbackDir * knockbackForce;
        Invoke(nameof(StopKnockback), knockbackDuration);  // 일정 시간 후 정지
    }

    // 넉백 종료 후 다시 추격 상태로 전환
    private void StopKnockback()
    {
        rb.linearVelocity = Vector2.zero;
        ChangeState(EnemyState.Chase);
    }
}