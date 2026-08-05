using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 보스 메인 상태 및 패턴 제어하는 메인 스크립트
// 루트 모션 끄고 추격 상태에서 미끄러지는 현상 완벽 방지함
public class MidBoss : EnemyFSM
{
    [Header("페이즈 설정")]
    [SerializeField] private int currentPhase = 1;
    [SerializeField] private float phase2Threshold = 0.5f;
    private bool isPhaseChanging = false;

    // 사운드 스크립트 등 외부에서 현재 페이즈를 안전하게 읽어갈 수 있도록 추가
    public int CurrentPhase => currentPhase;

    [Header("공격 쿨타임 및 헛스윙 설정")]
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private float basicAttackLockDuration = 0.8f;
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

    // 페이즈별로 쓸 패턴들 모아두는 리스트
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
    public GameObject webFirePoint;

    [Header("입 위치 수동 보정 (완결판)")]
    public Vector2 mouthRightPos = new Vector2(0.34f, 2.58f);
    public Vector2 mouthLeftPos = new Vector2(-3.5f, 2.58f);

    [Header("방향 전환 및 오프셋 설정")]
    [SerializeField] private float chaseOffset = 5f;
    [SerializeField] private float overlapDistance = 2f;

    [Header("방향 전환 - 와이퍼 현상 방지")]
    [SerializeField] private float flipDeadzoneX = 0.3f;

    [Header("뒤쪽 클리어링 발동 설정")]
    [SerializeField] private float behindClearingRange = 4f;

    private bool isDeadProcessed = false;
    private Dictionary<GameObject, float> hitboxBaseX = new Dictionary<GameObject, float>();

    protected override void Awake()
    {
        base.Awake();

        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) animator.applyRootMotion = false;

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

        if (webFirePoint != null)
        {
            Vector3 webPos = webFirePoint.transform.localPosition;
            webPos.x = facingLeft ? mouthLeftPos.x : mouthRightPos.x;
            webPos.y = facingLeft ? mouthLeftPos.y : mouthRightPos.y;
            webFirePoint.transform.localPosition = webPos;
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

        if (BGMManager.Instance != null) BGMManager.Instance.SetBattlePhase(2);

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

        float diffX = Mathf.Abs(player.position.x - transform.position.x);
        if (diffX < flipDeadzoneX) return;

        FlipTowardsPlayer();
    }

    private void EnterAttackState()
    {
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        ChangeState(EnemyState.Attack);
    }

    private bool IsPlayerBehind()
    {
        if (player == null || spriteRenderer == null) return false;

        bool facingLeft = spriteRenderer.flipX;
        float diffX = player.position.x - transform.position.x;

        return facingLeft ? (diffX > 0f) : (diffX < 0f);
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

        if (!IsAnyPatternBusy())
        {
            FlipIfGroundLevel();
        }

        if (IsAnyPatternBusy() || Time.time < attackAnimationLockTime)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // [추가됨] 플레이어가 노란색 원(원거리) 밖으로 나가면 즉시 압박하도록 쿨타임 강제 단축
        if (GetCurrentDistanceType() == BossPatternBase.DistanceType.Far)
        {
            if (nextAttackTime > Time.time + 0.3f)
            {
                nextAttackTime = Time.time + 0.3f;
            }
        }

        if (animator != null) animator.SetBool("isMoving", true);

        if (!IsAnyPatternBusy() && Time.time >= attackAnimationLockTime
            && IsPlayerBehind() && GetDistanceToPlayer() <= behindClearingRange
            && clearingPattern != null && clearingPattern.IsUsable())
        {
            EnterAttackState();
            return;
        }

        if (!IsAnyPatternBusy() && Time.time >= attackAnimationLockTime && GetDistanceToPlayer() <= overlapDistance && clearingPattern != null && clearingPattern.IsUsable())
        {
            EnterAttackState();
            return;
        }

        if (Time.time >= nextAttackTime && Time.time >= attackAnimationLockTime)
        {
            List<BossPatternBase> currentList = (currentPhase == 1) ? phase1Patterns : phase2Patterns;
            foreach (var p in currentList)
            {
                if (p.canUseInChase && p.IsUsable())
                {
                    EnterAttackState();
                    return;
                }
            }
        }

        if (GetDistanceToPlayer() <= attackRange && Time.time >= attackAnimationLockTime)
        {
            EnterAttackState();
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

        if (!IsAnyPatternBusy())
        {
            FlipIfGroundLevel();
        }

        if (IsAnyPatternBusy() || Time.time < attackAnimationLockTime)
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            // [핵심 수정 부분] 
            // 사거리 안에 있더라도 쿨타임 중이면 멍때리지(return) 말고 무조건 Chase로 넘겨서 스텝을 밟게 만듦
            ChangeState(EnemyState.Chase);
            return;
        }

        bool overlapClose = GetDistanceToPlayer() <= overlapDistance;
        bool behindClose = IsPlayerBehind() && GetDistanceToPlayer() <= behindClearingRange;
        if (clearingPattern != null && clearingPattern.IsUsable() && (overlapClose || behindClose))
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

        if (animator != null) { animator.SetBool("isMoving", false); animator.SetBool("isAttacking", true); }

        selectedPattern.Execute();

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

        if (BGMManager.Instance != null) BGMManager.Instance.ExitBattle();
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