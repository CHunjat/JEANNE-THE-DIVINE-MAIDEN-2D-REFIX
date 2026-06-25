using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// =====================================================
// MidBoss.cs
// 중간 보스(거미)의 핵심 AI 제어 스크립트임.
// EnemyFSM(상태 머신)을 상속받아 행동을 제어함.
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

    [Header("Hit Box 연결 (인스펙터에서 할당)")]
    public GameObject hitBox_Stamp;
    public GameObject hitBox_Landing;
    public GameObject hitBox_Clearing;
    public GameObject hitBox_Slash;
    public GameObject hitBox_BackKick;

    protected override void Awake()
    {
        base.Awake();

        BossPatternBase[] allPatterns = GetComponents<BossPatternBase>();

        foreach (var p in allPatterns)
        {
            string patternName = p.GetType().Name;

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

        AnimEvent_DisableAllHitBox();
    }

    public override void TakeDamage(float amount)
    {
        if (isPhaseChanging || GetCurrentState() == EnemyState.Dead) return;

        base.TakeDamage(amount);

        if (spriteRenderer != null && flashMaterial != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // 체력이 0 이하가 되면 강제로 사망 상태로 전환
        if (currentHp <= 0)
        {
            ChangeState(EnemyState.Dead);
            return;
        }

        CheckPhaseTransition();
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
            currentPhase = 2;
            isPhaseChanging = true;
            Debug.Log("[MidBoss] 2페이즈 돌입!");

            Invoke(nameof(EndPhaseTransition), 2f);
        }
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

        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        FlipTowardsPlayer();

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
            readyPatterns[randomIdx].Execute();

            nextAttackTime = Time.time + 3.5f;
        }
    }

    protected override void OnHit()
    {
    }

    protected override void OnDead()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null)
            coll.enabled = false;

        if (animator != null)
            animator.SetBool("isDead", true);

        Debug.Log("[MidBoss] 컷! 보스 처치 완료!");
    }

    public void AnimEvent_Slash1()
    {
        if (hitBox_Slash) hitBox_Slash.SetActive(true);
    }

    public void AnimEvent_Stamp()
    {
        if (hitBox_Stamp) hitBox_Stamp.SetActive(true);
    }

    public void AnimEvent_BackKickHit()
    {
        if (hitBox_BackKick) hitBox_BackKick.SetActive(true);
    }

    public void AnimEvent_DisableAllHitBox()
    {
        if (hitBox_Stamp) hitBox_Stamp.SetActive(false);
        if (hitBox_Landing) hitBox_Landing.SetActive(false);
        if (hitBox_Clearing) hitBox_Clearing.SetActive(false);
        if (hitBox_Slash) hitBox_Slash.SetActive(false);
        if (hitBox_BackKick) hitBox_BackKick.SetActive(false);
    }

    public void AnimEvent_Die()
    {
        Destroy(gameObject);
    }
}