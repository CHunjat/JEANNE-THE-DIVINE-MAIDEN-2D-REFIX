using UnityEngine;

// 엘리트 몬스터의 FSM 최상단 스크립트.
// 직접 타격하지 않고 애니메이터를 통해 패턴 스크립트에 공격을 지시.
public class EliteMonster : EnemyFSM
{
    [Header("범위 중심점 오프셋")]
    [SerializeField] private Vector2 centerOffset;

    [Header("넉백 설정")]
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private float knockbackDuration = 0.15f;

    [Header("공격 설정")]
    [SerializeField] private float attackCooldown = 2.5f;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        SetCollisionWithPlayer(false);
    }

    // 엘리트 몬스터 전용 중심점 좌표를 반환.
    private Vector2 GetEliteCenterPosition()
    {
        return (Vector2)transform.position + centerOffset;
    }

    // 오프셋이 적용된 중심점 기준으로 플레이어와의 거리를 계산.
    private float GetEliteDistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(GetEliteCenterPosition(), player.position);
    }

    // 오프셋이 적용된 중심점 기준으로 플레이어를 향한 방향을 계산.
    private Vector2 GetEliteDirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return ((Vector2)player.position - GetEliteCenterPosition()).normalized;
    }

    protected override void OnIdle()
    {
        animator.SetBool("isWalk", false);
        rb.linearVelocity = Vector2.zero;

        if (GetEliteDistanceToPlayer() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    protected override void OnChase()
    {
        if (GetEliteDistanceToPlayer() <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                ChangeState(EnemyState.Attack);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                animator.SetBool("isWalk", false);
                FlipTowardsPlayer();
            }
            return;
        }

        if (GetEliteDistanceToPlayer() > detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        rb.linearVelocity = GetEliteDirectionToPlayer() * moveSpeed;
        animator.SetBool("isWalk", true);
        FlipTowardsPlayer();
    }

    protected override void OnAttack()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isWalk", false);

        if (!isAttacking)
        {
            isAttacking = true;
            FlipTowardsPlayer();
            animator.SetTrigger("doAttack1");
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
        lastAttackTime = Time.time;
        ChangeState(EnemyState.Idle);
    }

    protected override void OnHit()
    {
        // 경직 면역
    }

    public override void TakeDamage(float amount, float groggyDamage = 0f)
    {
        base.TakeDamage(amount, groggyDamage);
        if (currentHp > 0)
            ApplyKnockback();
    }

    private void ApplyKnockback()
    {
        Vector2 knockbackDir = -GetEliteDirectionToPlayer();
        rb.linearVelocity = knockbackDir * knockbackForce;
        Invoke(nameof(StopKnockback), knockbackDuration);
    }

    private void StopKnockback()
    {
        if (this != null && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    protected override void OnDrawGizmos()
    {
        Vector2 drawPosition = (Vector2)transform.position + centerOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(drawPosition, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(drawPosition, attackRange);
    }

    // 엘리트 몬스터 전용 플립 (오프셋 좌표 기준)
    protected override void FlipTowardsPlayer()
    {
        if (player == null) return;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            // 오프셋이 적용된 중심점을 기준으로 플레이어가 왼쪽에 있는지 확인
            bool shouldFaceLeft = player.position.x < GetEliteCenterPosition().x;

            // 만약 몬스터가 앞을 안 보고 반대로(문워크) 걷는다면 아래 코드를 sr.flipX = !shouldFaceLeft; 로 변경하면 됨
            if (sr.flipX != shouldFaceLeft)
            {
                sr.flipX = shouldFaceLeft;
            }
        }
    }
}