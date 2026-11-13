
using UnityEngine;
using System.Collections;

public class Goblin : EnemyFSM
{
    [Header("플레이어")]
    [SerializeField] private Transform playerTarget;

    [Header("공격 설정")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float damageRatio = 1f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float attackHitHeight = 4f;

    [Range(0f, 1f)]
    [SerializeField] private float attackHitTiming = 0.5f;

    [Header("공격 애니메이션")]
    [SerializeField] private AnimationClip attack1Clip;
    [SerializeField] private AnimationClip attack2Clip;

    [Header("피격 설정")]
    [SerializeField] private float knockbackForce = 2f;
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private AnimationClip hitClip;

    [Header("사망 애니메이션")]
    [SerializeField] private AnimationClip deathClip;

    [Header("방향 설정")]
    [SerializeField] private bool spriteFacesRightByDefault = true;

    [Header("디버그")]
    [SerializeField] private bool showDebugLog = false;

    private SpriteRenderer spriteRenderer;

    private float lastAttackTime = -999f;
    private float knockbackEndTime;
    private float knockbackDirection;

    private bool isAttacking;
    private bool isTakingHit;
    private bool isDead;
    private bool useAttack2Next;

    private Coroutine attackCoroutine;
    private Coroutine hitCoroutine;

    // 초기 설정
    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb == null || animator == null)
        {
            Debug.LogError("[Goblin] Rigidbody2D 또는 Animator가 없습니다.", this);
            enabled = false;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (playerTarget != null)
            player = playerTarget;
    }

    // 플레이어 탐색
    private bool HasPlayer()
    {
        if (playerTarget != null)
        {
            player = playerTarget;
            return true;
        }

        if (player == null)
            FindPlayer();

        return player != null;
    }

    // 플레이어 사망 확인
    private bool IsPlayerDead()
    {
        if (!HasPlayer())
            return false;

        PlayerStats stats = player.GetComponentInParent<PlayerStats>();

        return stats != null && stats.currentHp <= 0f;
    }

    // 플레이어와 X축 거리 계산
    private float GetHorizontalDistance()
    {
        if (!HasPlayer())
            return Mathf.Infinity;

        return Mathf.Abs(player.position.x - transform.position.x);
    }

    // 플레이어 방향 계산
    private float GetHorizontalDirection()
    {
        if (!HasPlayer())
            return 0f;

        float difference = player.position.x - transform.position.x;

        if (Mathf.Abs(difference) < 0.01f)
            return 0f;

        return Mathf.Sign(difference);
    }

    // 상태 업데이트
    protected override void Update()
    {
        if (isDead)
            return;

        HasPlayer();

        // 플레이어 사망 시 공격 및 추격 중단
        if (IsPlayerDead())
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }

            isAttacking = false;

            if (GetCurrentState() != EnemyState.Idle)
                ChangeState(EnemyState.Idle);

            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            animator.ResetTrigger("doAttack1");
            animator.ResetTrigger("doAttack2");
            animator.SetBool("isMoving", false);

            return;
        }

        base.Update();
        UpdateAnimator();
    }

    // 이동 처리
    private void FixedUpdate()
    {
        if (rb == null || isDead)
            return;

        float velocityX = 0f;

        if (isTakingHit)
        {
            if (Time.time < knockbackEndTime)
                velocityX = knockbackDirection * knockbackForce;
        }
        else if (!isAttacking && GetCurrentState() == EnemyState.Chase)
        {
            velocityX = GetHorizontalDirection() * moveSpeed;
        }

        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);
    }

    // 대기 상태
    protected override void OnIdle()
    {
        if (!HasPlayer())
            return;

        if (GetHorizontalDistance() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    // 추격 상태
    protected override void OnChase()
    {
        if (!HasPlayer())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        float distance = GetHorizontalDistance();

        if (distance > detectRange)
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        UpdateFacing();

        if (distance <= attackRange)
            ChangeState(EnemyState.Attack);
    }

    // 공격 상태
    protected override void OnAttack()
    {
        if (!HasPlayer())
        {
            ChangeState(EnemyState.Idle);
            return;
        }

        if (isAttacking)
            return;

        if (GetHorizontalDistance() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        UpdateFacing();

        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    // 공격 애니메이션 실행
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        bool useSecondAttack = useAttack2Next;
        useAttack2Next = !useAttack2Next;

        AnimationClip selectedClip = useSecondAttack ? attack2Clip : attack1Clip;
        float duration = selectedClip != null ? selectedClip.length : 0.6f;

        animator.SetBool("isMoving", false);
        animator.ResetTrigger("doAttack1");
        animator.ResetTrigger("doAttack2");

        animator.SetTrigger(useSecondAttack ? "doAttack2" : "doAttack1");

        Log(useSecondAttack ? "Attack2 시작" : "Attack1 시작");

        yield return new WaitForSeconds(duration * attackHitTiming);

        ApplyAttackDamage();

        yield return new WaitForSeconds(duration * (1f - attackHitTiming));

        isAttacking = false;
        attackCoroutine = null;

        if (GetHorizontalDistance() > attackRange)
            ChangeState(EnemyState.Chase);
    }

    // 공격 데미지 처리
    private void ApplyAttackDamage()
    {
        if (isDead || !HasPlayer())
            return;

        float direction = GetHorizontalDirection();

        if (direction == 0f)
        {
            direction = spriteRenderer != null && spriteRenderer.flipX
                ? (spriteFacesRightByDefault ? -1f : 1f)
                : (spriteFacesRightByDefault ? 1f : -1f);
        }

        Vector2 center = new Vector2(
            transform.position.x + direction * attackRange * 0.5f,
            transform.position.y
        );

        Vector2 size = new Vector2(attackRange, attackHitHeight);

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center, size, 0f, playerLayer
        );

        foreach (Collider2D hit in hits)
        {
            PlayerController playerController =
                hit.GetComponentInParent<PlayerController>();

            if (playerController == null)
                continue;

            float damage = attackDamage * damageRatio;

            playerController.EvaluateAttack(damage, transform.position);

            Log("공격 성공! 데미지: " + damage);
            break;
        }
    }

    // 피격 상태
    protected override void OnHit()
    {
    }

    // 피격 처리
    public override void TakeDamage(float amount, float groggyDamage = 0f)
    {
        if (isDead)
            return;

        base.TakeDamage(amount, groggyDamage);

        if (isDead)
            return;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        isAttacking = false;

        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        hitCoroutine = StartCoroutine(HitRoutine());
    }

    // 피격 애니메이션 및 넉백
    private IEnumerator HitRoutine()
    {
        isTakingHit = true;
        ChangeState(EnemyState.Hit);

        knockbackDirection = -GetHorizontalDirection();
        knockbackEndTime = Time.time + knockbackDuration;

        animator.SetBool("isMoving", false);
        animator.ResetTrigger("doAttack1");
        animator.ResetTrigger("doAttack2");
        animator.ResetTrigger("doHit");
        animator.SetTrigger("doHit");

        float duration = hitClip != null ? hitClip.length : 0.3f;

        yield return new WaitForSeconds(duration);

        isTakingHit = false;
        hitCoroutine = null;

        if (GetHorizontalDistance() <= detectRange)
            ChangeState(EnemyState.Chase);
        else
            ChangeState(EnemyState.Idle);
    }

    // 그로기 상태
    protected override void OnGroggy()
    {
        if (animator != null)
            animator.SetBool("isMoving", false);
    }

    // 사망 상태
    protected override void OnDead()
    {
    }

    // 사망 처리
    protected override void Die()
    {
        if (isDead)
            return;

        isDead = true;
        ChangeState(EnemyState.Dead);

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        }

        isAttacking = false;
        isTakingHit = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        animator.ResetTrigger("doAttack1");
        animator.ResetTrigger("doAttack2");
        animator.ResetTrigger("doHit");
        animator.SetBool("isMoving", false);
        animator.SetBool("isDead", true);

        if (col != null)
            col.enabled = false;

        float duration = deathClip != null ? deathClip.length : 0.8f;

        Destroy(gameObject, duration);

        Log("사망");
    }

    // 바라보는 방향 처리
    private void UpdateFacing()
    {
        if (spriteRenderer == null || !HasPlayer())
            return;

        bool facingLeft = player.position.x < transform.position.x;

        spriteRenderer.flipX = spriteFacesRightByDefault
            ? facingLeft
            : !facingLeft;
    }

    // Animator 동기화
    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        bool moving =
            GetCurrentState() == EnemyState.Chase &&
            !isAttacking &&
            !isTakingHit &&
            Mathf.Abs(rb.linearVelocity.x) > 0.01f;

        animator.SetBool("isMoving", moving);
    }

    // 디버그 로그
    private void Log(string message)
    {
        if (showDebugLog)
            Debug.Log("[Goblin] " + message, this);
    }

    // 공격 범위 표시
    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        float direction = 1f;

        if (Application.isPlaying && player != null)
            direction = player.position.x < transform.position.x ? -1f : 1f;

        Gizmos.color = Color.magenta;

        Vector3 center = transform.position +
                         Vector3.right * direction * attackRange * 0.5f;

        Gizmos.DrawWireCube(
            center,
            new Vector3(attackRange, attackHitHeight, 0f)
        );
    }
}
