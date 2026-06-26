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
    // 현재 보스가 몇 페이즈인지 저장하는 변수임. (기본값 1)
    [SerializeField] private int currentPhase = 1;
    // 2페이즈로 넘어갈 체력 비율임. (0.5 = 최대 체력의 50%)
    [SerializeField] private float phase2Threshold = 0.5f;
    // 지금 2페이즈로 변신(포효 등) 중인지 체크하는 변수임. true일 땐 무적이고 안 움직임.
    private bool isPhaseChanging = false;

    [Header("보스 공격 딜레이")]
    // 패턴들이 기관총처럼 한꺼번에 나가는 걸 막기 위해, '다음 번 공격이 가능한 시간'을 기록해 두는 변수임.
    private float nextAttackTime = 0f;

    [Header("피격 피드백 (경직 면역)")]
    // 보스의 2D 이미지를 화면에 그려주는 컴포넌트임. 색깔을 바꾸기 위해 필요함.
    [SerializeField] private SpriteRenderer spriteRenderer;
    // 맞았을 때 잠깐 입을 '하얀색 옷(마테리얼)'임. 인스펙터에서 GUI/Text Shader를 넣은 마테리얼을 연결해야 함.
    [SerializeField] private Material flashMaterial;
    // 피격이 끝나면 다시 원래 색깔로 돌아오기 위해, 게임 시작 시점의 원래 옷을 기억해 둘 변수임.
    private Material originalMaterial;
    // 하얗게 번쩍이는 타이머(코루틴)가 여러 개 겹쳐서 버그 나는 걸 막기 위해, 현재 실행 중인 타이머를 추적하는 변수임.
    private Coroutine flashCoroutine;

    // 1페이즈 때 쓸 패턴들을 모아둘 장바구니임.
    private List<BossPatternBase> phase1Patterns = new List<BossPatternBase>();
    // 2페이즈 때 쓸 패턴들을 모아둘 장바구니임.
    private List<BossPatternBase> phase2Patterns = new List<BossPatternBase>();

    [Header("Hit Box 연결 (인스펙터에서 할당)")]
    // 인스펙터에서 빈칸에 끌어다 넣을 타격 판정 박스들임.
    public GameObject hitBox_Stamp;
    public GameObject hitBox_Landing;
    public GameObject hitBox_Clearing;
    public GameObject hitBox_Slash;
    public GameObject hitBox_BackKick;

    protected override void Awake()
    {
        base.Awake(); // 부모 클래스(EnemyFSM)의 초기화 코드를 먼저 실행함.

        // 보스 오브젝트에 붙어있는 패턴(1~8번) 스크립트들을 싹 다 배열로 긁어옴.
        BossPatternBase[] allPatterns = GetComponents<BossPatternBase>();

        // 긁어온 스크립트들의 '이름'을 확인해서 1페이즈용, 2페이즈용 장바구니에 알아서 나눠 담음.
        foreach (var p in allPatterns)
        {
            string patternName = p.GetType().Name; // 스크립트 이름 추출

            // 이름이 6, 7, 8번이면 2페이즈 가방에 넣음.
            if (patternName == "MidBossPattern6" || patternName == "MidBossPattern7" || patternName == "MidBossPattern8")
            {
                phase2Patterns.Add(p);
            }
            // 그 외(1~5번)는 1페이즈 가방에 넣음.
            else
            {
                phase1Patterns.Add(p);
            }
        }

        // 게임이 시작될 때 보스의 원래 마테리얼(옷)을 originalMaterial 변수에 저장해 둠.
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }

        // 시작할 때 혹시라도 켜져 있는 판정 박스들 싹 다 끄고 시작함.
        AnimEvent_DisableAllHitBox();
    }

    // 플레이어한테 맞아서 피가 깎일 때 실행되는 함수임.
    public override void TakeDamage(float amount)
    {
        // 2페이즈로 변신하는 연출 중이거나 이미 죽었으면 데미지를 안 받고 함수를 종료함.
        if (isPhaseChanging || GetCurrentState() == EnemyState.Dead) return;

        // 부모 클래스의 코드를 실행해서 실제로 체력을 깎음.
        base.TakeDamage(amount);

        // [피격 피드백 로직] 보스는 경직 면역이므로 맞았을 때 상태가 바뀌지 않고 하얗게 번쩍이기만 함.
        if (spriteRenderer != null && flashMaterial != null)
        {
            // 만약 이미 번쩍이고 있는 중(연타로 맞음)이라면, 꼬이지 않게 기존 타이머를 강제로 끔.
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            // 새 타이머(FlashRoutine)를 켜서 하얗게 만듦.
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        // 피가 깎였으니 "혹시 50% 밑으로 떨어져서 2페이즈 갈 때가 됐나?" 검사함.
        CheckPhaseTransition();
    }

    // 부모 클래스(EnemyBase)의 Die()에서 Destroy(gameObject)를 곧바로 실행하는 것을 막기 위해 오버라이드함.
    protected override void Die()
    {
        // 부모의 즉시 삭제 로직을 완전히 씹어버리고, FSM 상태를 사망(Dead)으로 안전하게 변경함.
        ChangeState(EnemyState.Dead);
    }

    // 0.1초 동안 몸을 하얗게 바꿨다가 원래대로 되돌리는 타이머(코루틴) 함수임.
    private IEnumerator FlashRoutine()
    {
        // 보스 옷을 하얀색(flashMaterial)으로 갈아입힘.
        spriteRenderer.material = flashMaterial;
        // 0.1초 동안 그 상태로 대기함.
        yield return new WaitForSeconds(0.1f);
        // 시간이 지나면 게임 시작할 때 기억해둔 원래 옷(originalMaterial)으로 다시 갈아입힘.
        spriteRenderer.material = originalMaterial;
    }

    // 2페이즈로 넘어갈지 조건을 검사하는 함수임.
    private void CheckPhaseTransition()
    {
        // 현재 1페이즈고, 피가 50% 이하라면 실행됨.
        if (currentPhase == 1 && currentHp <= maxHp * phase2Threshold)
        {
            currentPhase = 2;             // 2페이즈로 올림.
            isPhaseChanging = true;       // 변신 중이라고 표시함 (이때 데미지 안 들어감).
            Debug.Log("[MidBoss] 2페이즈 돌입!");

            // 2초 동안 포효하는 연출을 위해 대기했다가, EndPhaseTransition 함수를 실행함.
            Invoke(nameof(EndPhaseTransition), 2f);
        }
    }

    // 2초간의 변신 연출이 끝나면 실행되는 함수임.
    private void EndPhaseTransition()
    {
        isPhaseChanging = false;         // 변신 끝났다고 표시함.
        ChangeState(EnemyState.Chase);   // 멍때리지 말고 바로 플레이어를 쫓아가게 상태를 바꿈.
    }

    // [상태] 대기: 플레이어가 멀리 있을 때 멍때리는 상태임.
    protected override void OnIdle()
    {
        // 걷거나 공격하는 애니메이션을 끔.
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", false);
        }

        // 플레이어가 감지 범위(detectRange) 안에 들어오면 추격 상태로 바꿈.
        if (GetDistanceToPlayer() <= detectRange)
            ChangeState(EnemyState.Chase);
    }

    // [상태] 추격: 플레이어가 감지 범위엔 들어왔으나 때리기엔 멀 때 다가가는 상태임.
    protected override void OnChase()
    {
        // 변신 중일 땐 안 쫓아감.
        if (isPhaseChanging) return;

        // 걷는 애니메이션을 켬.
        if (animator != null)
            animator.SetBool("isMoving", true);

        // 쫓아가다가 플레이어가 공격 사거리(attackRange) 안에 들어오면 공격 상태로 바꿈.
        if (GetDistanceToPlayer() <= attackRange)
        {
            ChangeState(EnemyState.Attack);
            return;
        }

        // 플레이어가 있는 쪽으로 고개를 돌림 (좌우 반전).
        FlipTowardsPlayer();

        // X축(좌우)으로만 이동 속도를 주고, Y축은 냅둬서 보스가 바닥을 파고들거나 하늘로 날아가는 걸 막음.
        if (player != null)
        {
            float moveDirX = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(moveDirX * moveSpeed, rb.linearVelocity.y);
        }
    }

    // [상태] 공격: 플레이어가 때릴 수 있는 사거리 안에 있을 때의 상태임.
    protected override void OnAttack()
    {
        // 변신 중일 땐 안 때림.
        if (isPhaseChanging) return;

        // 공격하려고 폼 잡았는데 플레이어가 얍삽하게 사거리 밖으로 도망가면 다시 추격 상태로 바꿈.
        if (GetDistanceToPlayer() > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }

        // 플레이어가 보스 뒤로 구르기 등으로 넘어갔을 경우를 대비해 고개를 돌려줌.
        FlipTowardsPlayer();

        // [중요] 아직 다음 공격을 할 타이밍(3.5초 쿨타임)이 안 됐으면, 그냥 제자리에서 이동만 멈추고 쉼.
        if (Time.time < nextAttackTime)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        // 때릴 타이밍이 됐으니 공격 애니메이션을 켬.
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetBool("isAttacking", true);
        }

        // 공격하는 동안 얼음 위처럼 미끄러지지 않게 이동 속도를 0으로 브레이크 욺.
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        // 현재 페이즈에 맞는 패턴 가방을 엶.
        List<BossPatternBase> currentPatterns = (currentPhase == 1) ? phase1Patterns : phase2Patterns;

        // 당장 쓸 수 있는(각자 쿨타임이 다 찬) 패턴들만 모아둘 임시 장바구니를 만듦.
        List<BossPatternBase> readyPatterns = new List<BossPatternBase>();

        // 패턴 가방을 뒤져서 쓸 수 있는 애들만 임시 장바구니에 담음.
        foreach (var pattern in currentPatterns)
        {
            if (pattern.IsUsable())
            {
                readyPatterns.Add(pattern);
            }
        }

        // 쓸 수 있는 스킬이 하나라도 있다면?
        if (readyPatterns.Count > 0)
        {
            // 주사위를 굴려서 그중 랜덤으로 하나를 뽑아서 시전함. (1번만 편식하는 버그 방지)
            int randomIdx = Random.Range(0, readyPatterns.Count);
            readyPatterns[randomIdx].Execute();

            // 스킬을 하나 썼으니, 다음번 공격은 3.5초 뒤에 하라고 타이머를 세팅해 줌.
            nextAttackTime = Time.time + 3.5f;
        }
    }

    // [상태] 피격
    protected override void OnHit()
    {
    }

    // [상태] 사망: 피가 0이 됐을 때 상태임.
    protected override void OnDead()
    {
        // 1. 죽은 시체가 바닥에서 밀려다니지 않게 이동 속도를 완전히 0으로 고정.
        rb.linearVelocity = Vector2.zero;

        // 유니티 중력을 0으로 만들어서 죽을 때 시체가 땅 밑으로 추락하는 현상 방지
        rb.gravityScale = 0f;

        // 2. 시체에 플레이어가 걸려 넘어지거나 비비적거리는 걸 막기 위해 충돌체(Collider)를 끔.
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null)
            coll.enabled = false;

        // 3. 죽는 애니메이션을 틀어줌.
        if (animator != null)
            animator.SetBool("isDead", true);

        Debug.Log("[MidBoss] 컷! 보스 처치 완료!");
    }

    // ========================================================
    // 애니메이션 이벤트 핀에서 실행할 함수들 모음.
    // ========================================================
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