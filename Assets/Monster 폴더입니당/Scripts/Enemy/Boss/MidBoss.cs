using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// =====================================================
// MidBoss.cs
// 보스 메인 상태 및 패턴 제어 (정통 액션 헛스윙 유지 로직 적용)
// [수정] 와이퍼 현상(플립 발작) 방지를 높이 제한 대신 "플립 쿨타임 + 데드존" 방식으로 교체
//        -> 기존 높이 제한(flipHeightThreshold)이 문워크 버그의 원인이었음
// [추가] 플레이어가 뒤에 있을 때 클리어링 패턴 우선 발동 (IsPlayerBehind)
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
    [SerializeField] private float chaseOffset = 5f;
    [SerializeField] private float overlapDistance = 2f;

    [Header("방향 전환 - 와이퍼 현상 방지 (쿨타임 방식)")]
    [Tooltip("플립(방향전환)이 성공한 뒤, 다시 플립을 시도할 수 있을 때까지의 최소 시간. 플레이어가 머리 위로 빠르게 지나가도 좌우로 파르르 떠는 현상을 막아줌.")]
    [SerializeField] private float minFlipInterval = 0.35f;
    [Tooltip("플레이어와의 X축 거리가 이 값보다 작으면 플립을 시도하지 않음. 거의 정중앙(머리 위)에 있을 때 좌우 판정이 흔들리는 것을 막아줌.")]
    [SerializeField] private float flipDeadzoneX = 0.3f;
    private float nextFlipAllowedTime = 0f;

    [Header("뒤쪽 클리어링 발동 설정")]
    [Tooltip("플레이어가 등 뒤에 있을 때, 이 거리 이내라면 거리/우선순위 상관없이 클리어링을 우선 발동함")]
    [SerializeField] private float behindClearingRange = 4f;

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

        // 실제로 방향이 바뀐 순간에만 쿨타임을 다시 걸기
        // (FlipTowardsPlayer는 방향이 실제로 바뀔 때만 이 훅을 호출하므로,
        // 여기서 타이머를 세팅하면 "성공한 플립" 기준으로 정확히 쿨타임이 걸림)
        nextFlipAllowedTime = Time.time + minFlipInterval;
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

    // 수정 사항 : 높이 제한 대신 쿨타임 + 데드존 방식으로 와이퍼 현상 방지
    private void FlipIfGroundLevel()
    {
        if (player == null) return;

        // 아직 쿨타임 중이면 플립 시도 자체를 하지 않음 (와이퍼 방지 핵심)
        if (Time.time < nextFlipAllowedTime) return;

        // 플레이어가 거의 정중앙(머리 위)에 있으면 좌우 판정이 흔들리니 무시
        float diffX = Mathf.Abs(player.position.x - transform.position.x);
        if (diffX < flipDeadzoneX) return;

        // 높이는 더 이상 체크하지 않음 -> 이동 방향과 얼굴 방향이 항상 일치하게 됨 (문워크 해결)
        FlipTowardsPlayer();
    }

    // 추가 사항 : Attack 상태로 전환하는 모든 지점에서 이걸 호출하면,
    // 다음 프레임 OnAttack()이 실행되길 기다리지 않고 그 즉시 속도를 0으로 고정함.
    // -> Chase에서 Attack으로 넘어가는 그 순간 한 프레임 동안 관성으로 밀리던 현상 해결.
    private void EnterAttackState()
    {
        if (rb != null) rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        ChangeState(EnemyState.Attack);
    }

    // 추가 사항 : 플레이어가 스파이더의 등 뒤에 있는지 판정
    private bool IsPlayerBehind()
    {
        if (player == null || spriteRenderer == null) return false;

        bool facingLeft = spriteRenderer.flipX;
        float diffX = player.position.x - transform.position.x;

        // facingLeft == true 이면 왼쪽을 보고 있는 것.
        // 이때 플레이어가 오른쪽(diffX > 0)에 있으면 등 뒤에 있는 것.
        // facingLeft == false(오른쪽을 보고 있음)이면 플레이어가 왼쪽(diffX < 0)에 있을 때 등 뒤.
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
        FlipIfGroundLevel();
        if (animator != null) animator.SetBool("isMoving", true);

        // 추가 사항 : 플레이어가 등 뒤에 있고, 클리어링 사용 가능하면 거리 / 쿨타임 상관없이 최우선 발동
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

        // 4. 클리어링 패턴 실행 (등 뒤에 있을 때도 사용 가능하도록 조건 확장)
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