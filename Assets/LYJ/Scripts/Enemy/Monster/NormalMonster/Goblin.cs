
using UnityEngine;
using System.Collections;

// Goblin 전용 일반 몬스터 AI
// EnemyBase → EnemyFSM → Goblin 상속 구조
public class Goblin : EnemyFSM
{
    [Header("공격 설정")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float damageRatio = 1f;
    [SerializeField] private LayerMask playerLayer;

    [Header("공격 애니메이션")]
    [SerializeField] private AnimationClip attack1Clip;
    [SerializeField] private AnimationClip attack2Clip;

    [Range(0f, 1f)]
    [SerializeField] private float attackHitTiming = 0.5f;

    [Header("피격 및 사망 애니메이션")]
    [SerializeField] private AnimationClip hitClip;
    [SerializeField] private AnimationClip deathClip;

    private float lastAttackTime = -999f;

    private bool isAttacking;
    private bool isTakingHit;
    private bool isDead;

    private bool useAttack2Next;

    private Coroutine attackRoutine;
    private Coroutine hitRoutine;

    protected override void Awake()
    {
        base.Awake();

        if (rb == null)
        {
            Debug.LogError(
                "[Goblin] Rigidbody2D가 없습니다.",
                this
            );

            enabled = false;
            return;
        }

        if (animator == null)
        {
            Debug.LogError(
                "[Goblin] Animator가 없습니다.",
                this
            );

            enabled = false;
            return;
        }
    }

    protected override void Update()
    {
        if (isDead)
            return;

        base.Update();

        UpdateAnimation();
    }

    // 대기 상태
    protected override void OnIdle()
    {
        rb.linearVelocity = Vector2.zero;

        if (GetDistanceToPlayer() <= detectRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    // 플레이어 추격
    protected override void OnChase()
    {
        if (GetDistanceToPlayer() > detectRange)
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(EnemyState.Idle);
            return;
        }

        FlipTowardsPlayer();

        if (GetDistanceToPlayer() <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(EnemyState.Attack);
            return;
        }

        Vector2 direction = GetDirectionToPlayer();

        // 지상 몬스터이므로 X축으로 이동
        rb.linearVelocity = new Vector2(
            direction.x * moveSpeed,
            rb.linearVelocity.y
        );
    }

    // 공격 상태
    protected override void OnAttack()
    {
        rb.linearVelocity = Vector2.zero;

        if (isAttacking)
            return;

        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        FlipTowardsPlayer();

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    // 공격 애니메이션 재생 및 타격 처리
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        bool attack2 = useAttack2Next;
        useAttack2Next = !useAttack2Next;

        AnimationClip selectedClip =
            attack2 ? attack2Clip : attack1Clip;

        float duration = selectedClip != null
            ? selectedClip.length
            : 0.6f;

        animator.SetBool("isMoving", false);

        if (attack2)
        {
            animator.SetTrigger("doAttack2");
        }
        else
        {
            animator.SetTrigger("doAttack1");
        }

        // 공격 모션의 지정된 시점까지 대기
        yield return new WaitForSeconds(
            duration * attackHitTiming
        );

        // 실제 데미지 적용
        ApplyAttackDamage();

        // 남은 공격 애니메이션 시간 대기
        yield return new WaitForSeconds(
            duration * (1f - attackHitTiming)
        );

        isAttacking = false;
        attackRoutine = null;

        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }

    // 플레이어에게 데미지 적용
    private void ApplyAttackDamage()
    {
        if (isDead || player == null)
            return;

        // 타격 순간에 공격 범위 재확인
        Collider2D hit = Physics2D.OverlapCircle(
            transform.position,
            attackRange,
            playerLayer
        );

        if (hit == null)
            return;

        PlayerStats playerStats =
            hit.GetComponentInParent<PlayerStats>();

        if (playerStats == null)
            return;

        float finalDamage = attackDamage * damageRatio;

        playerStats.TakeDamage(finalDamage);

        Debug.Log(
            $"[Goblin] 플레이어 공격 성공! 데미지: {finalDamage}"
        );
    }

    // 피격 상태
    protected override void OnHit()
    {
        rb.linearVelocity = Vector2.zero;
    }

    public override void TakeDamage(
        float amount,
        float groggyDamage = 0f)
    {
        if (isDead)
            return;

        base.TakeDamage(amount, groggyDamage);

        // 사망했다면 피격 애니메이션 실행하지 않음
        if (isDead)
            return;

        // 공격 중이었다면 공격 중단
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;

        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        isTakingHit = true;

        ChangeState(EnemyState.Hit);

        rb.linearVelocity = Vector2.zero;

        animator.ResetTrigger("doAttack1");
        animator.ResetTrigger("doAttack2");

        animator.SetBool("isMoving", false);
        animator.SetTrigger("doHit");

        float duration = hitClip != null
            ? hitClip.length
            : 0.3f;

        yield return new WaitForSeconds(duration);

        isTakingHit = false;
        hitRoutine = null;

        if (GetDistanceToPlayer() <= detectRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else
        {
            ChangeState(EnemyState.Idle);
        }
    }

    // 사망 상태
    protected override void OnDead()
    {
        rb.linearVelocity = Vector2.zero;
    }

    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;

        ChangeState(EnemyState.Dead);

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
            hitRoutine = null;
        }

        rb.linearVelocity = Vector2.zero;

        animator.ResetTrigger("doAttack1");
        animator.ResetTrigger("doAttack2");
        animator.ResetTrigger("doHit");

        animator.SetBool("isMoving", false);
        animator.SetBool("isDead", true);

        // 사망 후 충돌 비활성화
        if (col != null)
            col.enabled = false;

        float deathDuration = deathClip != null
            ? deathClip.length
            : 0.8f;

        Destroy(gameObject, deathDuration);

        Debug.Log("[Goblin] 사망 애니메이션 실행");
    }

    // FSM 상태와 Animator 동기화
    private void UpdateAnimation()
    {
        if (animator == null)
            return;

        if (isTakingHit || isAttacking)
        {
            animator.SetBool("isMoving", false);
            return;
        }

        bool moving =
            GetCurrentState() == EnemyState.Chase &&
            Mathf.Abs(rb.linearVelocity.x) > 0.01f;

        animator.SetBool("isMoving", moving);
    }
}
