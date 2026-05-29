using UnityEngine;

// =====================================================
// PatrolMonster.cs
// 순찰 몬스터 스크립트임.
// 지정된 지점들 사이를 왕복 순찰하다가, 플레이어를 감지하면 추격함.
// 기획 확정 후 순찰 경로를 인스펙터에서 지정할 것.
//
// [사용 방법]
// 인스펙터의 "순찰 지점 목록"에 빈 오브젝트(웨이포인트)를 드래그해서 넣으면
// 해당 지점들을 순서대로 순찰함.
// =====================================================
public class PatrolMonster : EnemyFSM
{
    [Header("순찰 설정")]
    [SerializeField] private Transform[] patrolPoints;  // 순찰할 지점 목록 (인스펙터에서 지정할 것)
    [SerializeField] private float waypointStopDistance = 0.3f;  // 이 거리 이내에 도착하면 다음 지점으로 이동

    [Header("공격 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float attackCooldown = 2f;   // 공격 쿨타임 (초 단위)
    [SerializeField] private float damageRatio = 1.0f;    // 공격력 반영 비율
    [SerializeField] private LayerMask playerLayer;       // Player 레이어 (인스펙터에서 반드시 설정할 것)

    [Header("넉백 설정 - 기획 확정 후 수정 또는 삭제할 것")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private int currentPatrolIndex = 0;  // 현재 향하고 있는 순찰 지점 번호
    private float lastAttackTime = -999f;

    protected override void Awake()
    {
        // 임시 수치 - 기획 확정 후 수정할 것
        maxHp = 100f;
        moveSpeed = 2f;      // 순찰 몬스터는 조금 느리게 설정
        attackDamage = 10f;
        detectRange = 5f;
        attackRange = 1.5f;

        base.Awake();
        SetCollisionWithPlayer(false);
    }

    // 대기 상태 - 순찰 지점이 있으면 순찰하고, 플레이어를 감지하면 추격함
    protected override void OnIdle()
    {
        if (GetDistanceToPlayer() <= detectRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 순찰 지점이 없으면 그냥 제자리에 있음
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // 현재 목표 지점으로 이동
        Transform target = patrolPoints[currentPatrolIndex];
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        // 목표 지점에 도착하면 다음 지점으로 넘어감
        if (Vector2.Distance(transform.position, target.position) <= waypointStopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;  // 마지막 지점 도달 시 처음으로 돌아감
        }
    }

    // 추격 상태
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

    // 공격 상태
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

    protected override void OnHit() { }

    protected override void OnDead()
    {
        rb.linearVelocity = Vector2.zero;
        // 사망 연출 - 기획 확정 후 채울 것
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        if (currentHp > 0)
            ApplyKnockback();
    }

    private void PerformAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float damage = attackDamage * damageRatio;
                playerHealth.TakeDamage(damage);
                Debug.Log($"[{gameObject.name}] 플레이어에게 {damage} 데미지 적용함.");
            }
        }
    }

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

    // 씬 뷰에서 순찰 경로를 시각적으로 보여줌 (파란 선)
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (patrolPoints == null || patrolPoints.Length < 2) return;

        Gizmos.color = Color.blue;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);

            int next = (i + 1) % patrolPoints.Length;
            if (patrolPoints[next] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
        }
    }
}