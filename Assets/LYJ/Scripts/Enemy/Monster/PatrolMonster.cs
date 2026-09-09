using UnityEngine;

// =====================================================
// PatrolMonster.cs
// =====================================================
public class PatrolMonster : EnemyFSM
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waypointStopDistance = 0.3f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float damageRatio = 1.0f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;

    private int currentPatrolIndex = 0;
    private float lastAttackTime = -999f;

    protected override void Awake()
    {
        maxHp = 100f;
        moveSpeed = 2f;
        attackDamage = 10f;
        detectRange = 5f;
        attackRange = 1.5f;
        base.Awake();
        SetCollisionWithPlayer(false);
    }

    protected override void OnIdle()
    {
        if (GetDistanceToPlayer() <= detectRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        Transform target = patrolPoints[currentPatrolIndex];
        Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
        if (Vector2.Distance(transform.position, target.position) <= waypointStopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

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
    protected override void OnDead() { rb.linearVelocity = Vector2.zero; }

    public override void TakeDamage(float amount, float groggyDamage = 0f)
    {
        base.TakeDamage(amount, groggyDamage);
        if (currentHp > 0)
            ApplyKnockback();
    }

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