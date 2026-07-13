using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// =====================================================
// MidBoss.cs
// 보스 메인 상태 및 패턴 제어 (정통 액션 헛스윙 유지 로직 적용)
// =====================================================
public class MidBoss : EnemyFSM
{
    [Header("페이즈 설정")]
    [SerializeField] private int currentPhase = 1;
    [SerializeField] private float phase2Threshold = 0.5f;
    private bool isPhaseChanging = false;

    [Header("공격 쿨타임 및 헛스윙 설정")]
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float basicAttackLockDuration = 0.8f; // 기획자가 모션 길이에 맞춰 조절
    private float nextAttackTime = 0f;
    private float attackAnimationLockTime = 0f;

    [Header("거리 범위 설정")]
    [SerializeField] private float closeRangeMax = 5f;
    [SerializeField] private float midRangeMax = 10f;
    [SerializeField] private float farRangeMax = 20f;

    [Header("피격 피드백")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration = 0.05f;
    private Material originalMaterial;
    private Coroutine flashCoroutine;

    private List<BossPatternBase> phase1Patterns = new List<BossPatternBase>();
    private List<BossPatternBase> phase2Patterns = new List<BossPatternBase>();

    private MidBossPattern5 clearingPattern;
    private MidBossPattern3 webPattern;
    private EnemyGroggy groggy;

    [Header("Hit Box 연결")]
    public GameObject hitBox_Stamp;
    public GameObject hitBox_Landing;
    public GameObject hitBox_Clearing;
    public GameObject hitBox_Slash;
    public GameObject hitBox_BackKick;

    [Header("방향 전환 및 오프셋 설정")]
    [SerializeField] private float flipHeightThreshold = 1f;
    [SerializeField] private float chaseOffset = 5f;
    [SerializeField] private float overlapDistance = 2f;

    private bool isDeadProcessed = false;
    private Dictionary<GameObject, float> hitboxBaseX = new Dictionary<GameObject, float>();

    protected override void Awake()
    {
        base.Awake();
        Collider2D myCollider = GetComponent<Collider2D>();
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null && myCollider != null)
        {
            Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
            if (playerCollider != null) Physics2D.IgnoreCollision(myCollider, playerCollider, true);
        }

        BossPatternBase[] allPatterns = GetComponents<BossPatternBase>();
        clearingPattern = GetComponent<MidBossPattern5>();
        webPattern = GetComponent<MidBossPattern3>();

        foreach (var p in allPatterns)
        {
            string name = p.GetType().Name;
            if (name == "MidBossPattern5") continue;
            if (name == "MidBossPattern3")
            {
                phase1Patterns.Add(p);
                phase2Patterns.Add(p);
                continue;
            }

            if (name == "MidBossPattern6" || name == "MidBossPattern7" || name == "MidBossPattern8")
                phase2Patterns.Add(p);
            else
                phase1Patterns.Add(p);
        }

        if (spriteRenderer != null) originalMaterial = spriteRenderer.material;
        groggy = GetComponent<EnemyGroggy>();
        AnimEvent_DisableAllHitBox();
        CacheHitboxPositions();
    }

    private void CacheHitboxPositions()
    {
        GameObject[] hitboxes = { hitBox_Stamp, hitBox_Landing, hitBox_Clearing, hitBox_Slash, hitBox_BackKick };
        foreach (var hb in hitboxes)
        {
            if (hb != null)
                hitboxBaseX[hb] = Mathf.Abs(hb.transform.localPosition.x);
        }
    }

    protected override void OnFacingChanged(bool facingLeft)
    {
        foreach (var kvp in hitboxBaseX)
        {
            GameObject hb = kvp.Key;
            float baseX = kvp.Value;
            Vector3 pos = hb.transform.localPosition;
            pos.x = facingLeft ? -baseX : baseX;
            hb.transform.localPosition = pos;
        }
    }

    private bool IsAnyPatternBusy()
    {
        List<BossPatternBase> currentList = (currentPhase == 1) ? phase1Patterns : phase2Patterns;
        foreach (var p in currentList)
        {
            if (p.IsBusy) return true;
        }
        return false;
    }

    public override void TakeDamage(float amount, float groggyDamage = 0f)
    {
        if (isPhaseChanging || GetCurrentState() == EnemyState.Dead) return;

        float finalDamage = (groggy != null) ? amount * groggy.GetDamageMultiplier() : amount;
        base.TakeDamage(finalDamage, groggyDamage);

        if (groggy != null) groggy.AddGauge(groggyDamage);

        if (spriteRenderer != null && flashMaterial != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        CheckPhaseTransition();
    }

    protected override void Die() { ChangeState(EnemyState.Dead); }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.material = flashMaterial;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.material = originalMaterial;
    }

    private void CheckPhaseTransition()
    {
        if (currentPhase == 1 && currentHp <= maxHp * phase2Threshold)
        {
            if (groggy != null && groggy.IsGroggy)
            {
                groggy.RequestPendingPhaseTransition();
                return;
            }
            StartPhaseTransition();
        }
    }

    private void StartPhaseTransition()
    {
        currentPhase = 2;
        isPhaseChanging = true;
        Invoke(nameof(EndPhaseTransition), 2f);
    }

    private void OnGroggyEndedPhaseTransition() { StartPhaseTransition(); }

    private void EndPhaseTransition()
    {
        isPhaseChanging = false;
        ChangeState(EnemyState.Chase);
    }

    private BossPatternBase.DistanceType GetCurrentDistanceType()
    {
        float dist = GetDistanceToPlayer();
        if (dist <= closeRangeMax) return BossPatternBase.DistanceType.Close;
        if (dist <= midRangeMax) return BossPatternBase.DistanceType.Mid;
        if (dist <= farRangeMax) return BossPatternBase.DistanceType.Far;
        return BossPatternBase.DistanceType.Far;
    }

    private void FlipIfGroundLevel()
    {
        if (player == null) return;
        if (player.position.y <= transform.position.y + flipHeightThreshold)
            FlipTowardsPlayer();
    }

    protected override void OnIdle()
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", false);
        }

        if (GetDistanceToPlayer() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    protected override void OnChase()
    {
        if (isPhaseChanging) return;
        FlipIfGroundLevel();
        if (animator != null) animator.SetBool("isMoving", true);

        if (!IsAnyPatternBusy() && Time.time >= attackAnimationLockTime && GetDistanceToPlayer() <= overlapDistance && clearingPattern != null && clearingPattern.IsUsable())
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (Time.time >= nextAttackTime && Time.time >= attackAnimationLockTime)
        {
            List<BossPatternBase> currentList = (currentPhase == 1) ? phase1Patterns : phase2Patterns;
            foreach (var p in currentList)
            {
                if (p.canUseInChase && p.IsUsable())
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }
            }
        }

        if (GetDistanceToPlayer() <= attackRange && Time.time >= attackAnimationLockTime)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (player != null)
        {
            if (GetDistanceToPlayer() > chaseOffset)
            {
                float moveDirX = Mathf.Sign(player.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(moveDirX * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
    }

    protected override void OnAttack()
    {
        if (isPhaseChanging || (groggy != null && groggy.IsGroggy)) return;

        // 1. 공격 중에는 무조건 발바닥에 본드 칠하기 (스케이트 방지)
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 2. 대형 패턴(6, 7, 8번 등) 진행 중이거나, 기본 공격의 헛스윙 모션이 안 끝났다면 무조건 헛스윙하며 제자리 대기!
        if (IsAnyPatternBusy() || Time.time < attackAnimationLockTime)
        {
            return;
        }

        // 3. 헛스윙 모션이 완전히 끝났고 쿨타임이 도는 중이면, 거리를 재고 그제서야 추격!
        if (Time.time < nextAttackTime)
        {
            if (GetDistanceToPlayer() > attackRange)
            {
                ChangeState(EnemyState.Chase);
            }
            return;
        }

        // 4. 클리어링 패턴 실행
        if (clearingPattern != null && clearingPattern.IsUsable() && GetDistanceToPlayer() <= overlapDistance)
        {
            if (animator != null) { animator.SetBool("isMoving", false); animator.SetBool("isAttacking", true); }
            clearingPattern.Execute();

            attackAnimationLockTime = Time.time + basicAttackLockDuration;
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        List<BossPatternBase> currentPatterns = (currentPhase == 1) ? phase1Patterns : phase2Patterns;
        BossPatternBase.DistanceType currentDist = GetCurrentDistanceType();

        List<BossPatternBase> candidates = new List<BossPatternBase>();
        foreach (var p in currentPatterns)
        {
            if (!p.IsUsable()) continue;
            bool distanceOk = false;
            switch (p.distanceType)
            {
                case BossPatternBase.DistanceType.Any: distanceOk = true; break;
                case BossPatternBase.DistanceType.Close: distanceOk = (currentDist == BossPatternBase.DistanceType.Close); break;
                case BossPatternBase.DistanceType.Mid: distanceOk = (currentDist == BossPatternBase.DistanceType.Close || currentDist == BossPatternBase.DistanceType.Mid); break;
                case BossPatternBase.DistanceType.Far: distanceOk = (currentDist == BossPatternBase.DistanceType.Far); break;
            }
            if (distanceOk) candidates.Add(p);
        }

        if (candidates.Count == 0)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        candidates.Sort((a, b) => a.priority.CompareTo(b.priority));
        int highestPriority = candidates[0].priority;
        List<BossPatternBase> topCandidates = new List<BossPatternBase>();
        foreach (var p in candidates)
        {
            if (p.priority == highestPriority) topCandidates.Add(p);
            else break;
        }

        int randomIdx = Random.Range(0, topCandidates.Count);
        BossPatternBase selectedPattern = topCandidates[randomIdx];

        // 5. 일반 패턴 실행
        if (animator != null) { animator.SetBool("isMoving", false); animator.SetBool("isAttacking", true); }

        selectedPattern.Execute();

        // 패턴이 실행되는 순간, 정해진 시간(basicAttackLockDuration) 동안 절대 못 움직이게 락을 건다!
        attackAnimationLockTime = Time.time + basicAttackLockDuration;
        nextAttackTime = Time.time + attackCooldown;
    }

    protected override void OnHit() { }

    protected override void OnGroggy()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        SendMessage("EndExecution", SendMessageOptions.DontRequireReceiver);
    }

    protected override void OnDead()
    {
        if (isDeadProcessed) return;
        isDeadProcessed = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;
        if (animator != null) animator.SetBool("isDead", true);
    }

    public void AnimEvent_DisableAllHitBox()
    {
        if (hitBox_Stamp) hitBox_Stamp.SetActive(false);
        if (hitBox_Landing) hitBox_Landing.SetActive(false);
        if (hitBox_Clearing) hitBox_Clearing.SetActive(false);
        if (hitBox_Slash) hitBox_Slash.SetActive(false);
        if (hitBox_BackKick) hitBox_BackKick.SetActive(false);
    }
    public void AnimEvent_Die() { Destroy(gameObject); }
}