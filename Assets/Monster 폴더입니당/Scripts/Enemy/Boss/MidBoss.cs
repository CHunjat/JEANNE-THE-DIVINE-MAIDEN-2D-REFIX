using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// =====================================================
// MidBoss.cs
// =====================================================
public class MidBoss : EnemyFSM
{
    [Header("페이즈 설정")]
    [SerializeField] private int currentPhase = 1;
    [SerializeField] private float phase2Threshold = 0.5f;
    private bool isPhaseChanging = false;

    [Header("공격 쿨타임")]
    [SerializeField] private float attackCooldown = 2.5f;
    private float nextAttackTime = 0f;

    [Header("거리 범위 설정")]
    [SerializeField] private float closeRangeMax = 5f;
    [SerializeField] private float midRangeMax = 10f;
    [SerializeField] private float farRangeMax = 20f;

    [Header("피격 피드백")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material flashMaterial;
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

    private bool isDeadProcessed = false;

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
    }

    public override void TakeDamage(float amount)
    {
        if (isPhaseChanging || GetCurrentState() == EnemyState.Dead) return;

        float finalDamage = (groggy != null) ? amount * groggy.GetDamageMultiplier() : amount;
        base.TakeDamage(finalDamage);

        if (groggy != null) groggy.AddGauge(amount);

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
        yield return new WaitForSeconds(0.1f);
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
        Debug.Log("[MidBoss] 2페이즈 돌입!");
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

        FlipTowardsPlayer(); // 추격 중 항상 플레이어 바라보기 확실히

        if (animator != null) animator.SetBool("isMoving", true);

        // 원거리 공격 쿨타임 돌았으면 즉시 발동
        if (Time.time >= nextAttackTime)
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

        if (GetDistanceToPlayer() <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        if (player != null)
        {
            float moveDirX = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(moveDirX * moveSpeed, rb.linearVelocity.y);
        }
    }

    protected override void OnAttack()
    {
        if (isPhaseChanging || (groggy != null && groggy.IsGroggy)) return;

        FlipTowardsPlayer(); // 공격 판단 중에도 항상 고개 돌려주기

        if (clearingPattern != null && clearingPattern.IsUsable())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (animator != null) { animator.SetBool("isMoving", false); animator.SetBool("isAttacking", true); }
            clearingPattern.Execute();
            nextAttackTime = Time.time + attackCooldown;
            return;
        }

        // 문제 해결 : 공격 쿨타임 중인데 멀리 떨어져 있으면 멍때리지 않고 바로 추격으로 복귀!
        if (Time.time < nextAttackTime)
        {
            if (GetDistanceToPlayer() > attackRange)
            {
                ChangeState(EnemyState.Chase);
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
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

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (animator != null) { animator.SetBool("isMoving", false); animator.SetBool("isAttacking", true); }

        Debug.Log($"<color=cyan>[MidBoss] 패턴 발동: {selectedPattern.GetType().Name}</color>");
        selectedPattern.Execute();
        nextAttackTime = Time.time + attackCooldown;
    }

    protected override void OnHit() { }
    protected override void OnGroggy() { rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); }
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

    public void AnimEvent_Slash1() { if (hitBox_Slash) { hitBox_Slash.SetActive(true); Invoke(nameof(DeactivateSlash), 0.2f); } }
    private void DeactivateSlash() { if (hitBox_Slash) hitBox_Slash.SetActive(false); }
    public void AnimEvent_Stamp() { if (hitBox_Stamp) { hitBox_Stamp.SetActive(true); Invoke(nameof(DeactivateStamp), 0.2f); } }
    private void DeactivateStamp() { if (hitBox_Stamp) hitBox_Stamp.SetActive(false); }
    public void AnimEvent_BackKickHit() { if (hitBox_BackKick) { hitBox_BackKick.SetActive(true); Invoke(nameof(DeactivateBackKick), 0.2f); } }
    private void DeactivateBackKick() { if (hitBox_BackKick) hitBox_BackKick.SetActive(false); }
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