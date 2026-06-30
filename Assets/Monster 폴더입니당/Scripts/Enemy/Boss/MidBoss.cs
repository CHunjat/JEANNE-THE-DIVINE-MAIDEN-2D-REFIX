using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// =====================================================
// MidBoss.cs
// 중간 보스(거미)의 핵심 AI 제어 스크립트임.
// =====================================================
public class MidBoss : EnemyFSM
{
    [Header("페이즈 설정")]
    [SerializeField] private int currentPhase = 1;
    [SerializeField] private float phase2Threshold = 0.5f;
    private bool isPhaseChanging = false;

    [Header("보스 공격 딜레이")]
    private float nextAttackTime = 0f;

    [Header("피격 피드백 (경직 면역)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Material flashMaterial;
    private Material originalMaterial;
    private Coroutine flashCoroutine;

    private List<BossPatternBase> phase1Patterns = new List<BossPatternBase>();
    private List<BossPatternBase> phase2Patterns = new List<BossPatternBase>();

    // 클리어링(Pattern5) 전용 변수 - 긴급 발동을 위해 따로 뺌
    private MidBossPattern5 clearingPattern;

    // [추가됨] 그로기 컴포넌트 참조
    private EnemyGroggy groggy;

    [Header("Hit Box 연결 (인스펙터에서 할당)")]
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
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, playerCollider, true);
            }
        }

        BossPatternBase[] allPatterns = GetComponents<BossPatternBase>();

        // 5번(클리어링)은 긴급 가로채기용이므로 찾아서 따로 빼둠
        clearingPattern = GetComponent<MidBossPattern5>();

        foreach (var p in allPatterns)
        {
            string patternName = p.GetType().Name;

            if (patternName == "MidBossPattern5") continue; // 5번은 랜덤 뽑기 가방에 안 넣음

            if (patternName == "MidBossPattern6" || patternName == "MidBossPattern7" || patternName == "MidBossPattern8")
            {
                phase2Patterns.Add(p);
            }
            else
            {
                phase1Patterns.Add(p);
            }
        }

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }

        // [추가됨] 그로기 컴포넌트 가져오기 (없어도 동작은 함 - null 체크로 방어)
        groggy = GetComponent<EnemyGroggy>();

        AnimEvent_DisableAllHitBox();
    }

    public override void TakeDamage(float amount)
    {
        if (isPhaseChanging || GetCurrentState() == EnemyState.Dead) return;

        // [추가됨] 그로기 중이면 데미지 배율 적용
        float finalDamage = amount;
        if (groggy != null)
        {
            finalDamage = amount * groggy.GetDamageMultiplier();
        }

        base.TakeDamage(finalDamage);

        // [추가됨] 그로기 게이지 누적은 원본 데미지(amount) 기준으로
        if (groggy != null)
        {
            groggy.AddGauge(amount);
        }

        if (spriteRenderer != null && flashMaterial != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        CheckPhaseTransition();
    }

    protected override void Die()
    {
        ChangeState(EnemyState.Dead);
    }

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
            // [추가됨] 그로기 중이면 페이즈 전환을 그로기 끝날 때까지 보류
            if (groggy != null && groggy.IsGroggy)
            {
                groggy.RequestPendingPhaseTransition();
                return;
            }

            StartPhaseTransition();
        }
    }

    // [추가됨] 페이즈 전환 시작 로직을 별도 함수로 분리 (그로기 보류 후 재호출 가능하도록)
    private void StartPhaseTransition()
    {
        currentPhase = 2;
        isPhaseChanging = true;
        Debug.Log("[MidBoss] 2페이즈 돌입!");
        Invoke(nameof(EndPhaseTransition), 2f);
    }

    // [추가됨] EnemyGroggy가 SendMessage로 호출하는 콜백 - 그로기 끝났는데 페이즈 전환이 보류돼 있었을 때
    private void OnGroggyEndedPhaseTransition()
    {
        StartPhaseTransition();
    }

    private void EndPhaseTransition()
    {
        isPhaseChanging = false;
        ChangeState(EnemyState.Chase);
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

        if (animator != null)
            animator.SetBool("isMoving", true);

        if (GetDistanceToPlayer() <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        FlipTowardsPlayer();

        if (player != null)
        {
            float moveDirX = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(moveDirX * moveSpeed, rb.linearVelocity.y);
        }
    }

    protected override void OnAttack()
    {
        if (isPhaseChanging) return;

        // [추가됨] 그로기 중이면 공격 로직 전체 스킵 (무방비 상태)
        if (groggy != null && groggy.IsGroggy) return;

        // ========================================================
        // 1. 긴급 가로채기 (무한루프 차단 및 클리어링 즉시 발동)
        // ========================================================
        if (clearingPattern != null && clearingPattern.IsUsable())
        {
            FlipTowardsPlayer();
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            if (animator != null)
            {
                animator.SetBool("isMoving", false);
                animator.SetBool("isAttacking", true);
            }

            Debug.Log("<color=red>[MidBoss] 몸체 겹침 감지! 클리어링 정상 발동!</color>");
            clearingPattern.Execute();

            // 클리어링 모션 동안만 대기하고 다시 기본 공격 사이클로 복귀 (1.5초)
            nextAttackTime = Time.time + 1.5f;
            return;
        }

        // ========================================================
        // 2. 일반 공격 로직
        // ========================================================
        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        FlipTowardsPlayer();

        // 3.5초 쿨타임 대기 확인
        if (Time.time < nextAttackTime)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", true);
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        List<BossPatternBase> currentPatterns = (currentPhase == 1) ? phase1Patterns : phase2Patterns;
        List<BossPatternBase> readyPatterns = new List<BossPatternBase>();

        foreach (var pattern in currentPatterns)
        {
            if (pattern.IsUsable())
            {
                readyPatterns.Add(pattern);
            }
        }

        if (readyPatterns.Count > 0)
        {
            int randomIdx = Random.Range(0, readyPatterns.Count);
            Debug.Log($"<color=cyan>[MidBoss] 일반 패턴 발동: {readyPatterns[randomIdx].GetType().Name}</color>");
            readyPatterns[randomIdx].Execute();
            nextAttackTime = Time.time + 3.5f;
        }
    }

    protected override void OnHit() { }

    // [추가됨] 그로기 상태 - 이동만 멈추고 나머지는 완전 무방비
    protected override void OnGroggy()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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

        Debug.Log("[MidBoss] 컷! 보스 처치 완료!");
    }

    // ========================================================
    // 애니메이션 이벤트 핀
    // ========================================================
    public void AnimEvent_Slash1()
    {
        if (hitBox_Slash) { hitBox_Slash.SetActive(true); Invoke(nameof(DeactivateSlash), 0.2f); }
    }
    private void DeactivateSlash() { if (hitBox_Slash) hitBox_Slash.SetActive(false); }

    public void AnimEvent_Stamp()
    {
        if (hitBox_Stamp) { hitBox_Stamp.SetActive(true); Invoke(nameof(DeactivateStamp), 0.2f); }
    }
    private void DeactivateStamp() { if (hitBox_Stamp) hitBox_Stamp.SetActive(false); }

    public void AnimEvent_BackKickHit()
    {
        if (hitBox_BackKick) { hitBox_BackKick.SetActive(true); Invoke(nameof(DeactivateBackKick), 0.2f); }
    }
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