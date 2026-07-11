using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern6.cs
// 강화 앞발 찍기 2연찍기 + 뒷발 찌르기 (무조건 3연타)
// (그로기 판정 타이밍 경합 제거: Update로 실시간 감시)
// =====================================================
public class MidBossPattern6 : BossPatternBase
{
    [Header("강화 2연찍기 이동 설정 (기획자 조절)")]
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("강화 찍기 타격 판정 시간 (기획자 조절)")]
    [SerializeField] private float stampHitboxDuration = 0.2f;
    [SerializeField] private float backKickHitboxDuration = 0.3f;

    [Header("안전장치 - 애니메이션 이벤트 누락 시 강제 리셋 (기획자 조절)")]
    [SerializeField] private float maxExecutionTime = 5f;

    private GameObject stampHitbox;
    private GameObject backKickHitbox;
    private Rigidbody2D rb;
    private Animator visualAnimator;
    private EnemyGroggy groggy;
    private bool isExecuting = false;
    private bool wasInterruptedByGroggy = false; // [추가됨] 실행 도중 그로기 발생 여부를 실시간 기록
    private Coroutine failsafeCoroutine;

    public override bool IsBusy => isExecuting;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null)
        {
            stampHitbox = parent.hitBox_Stamp;
            backKickHitbox = parent.hitBox_BackKick;
        }

        groggy = GetComponent<EnemyGroggy>();

        if (stampHitbox != null) stampHitbox.SetActive(false);
        if (backKickHitbox != null) backKickHitbox.SetActive(false);

        cooldown = 0f;
        priority = 5;
        distanceType = DistanceType.Mid;
    }

    // [추가됨] 실행 중일 때만 매 프레임 그로기 진입 여부를 감시 (타이밍 경합 방지)
    private void Update()
    {
        if (isExecuting && groggy != null && groggy.IsGroggy)
        {
            wasInterruptedByGroggy = true;
        }
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        isExecuting = true;
        wasInterruptedByGroggy = false; // [추가됨] 새 사이클 시작 시 초기화

        if (visualAnimator != null) visualAnimator.SetTrigger("doTriple");
        StartCoroutine(MoveRoutine());

        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        failsafeCoroutine = StartCoroutine(FailsafeRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            Vector2 moveDir = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
            float elapsed = 0f;
            float moveDuration = moveDistance / moveSpeed;
            while (elapsed < moveDuration)
            {
                rb.linearVelocity = moveDir * moveSpeed;
                elapsed += Time.deltaTime;
                yield return null;
            }
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void AnimEvent_DoubleStamp()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(false);
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateStamp), stampHitboxDuration);
        }
    }

    public void AnimEvent_TripleBackKickHit()
    {
        if (backKickHitbox != null)
        {
            backKickHitbox.SetActive(false);
            backKickHitbox.SetActive(true);
            Invoke(nameof(DeactivateBackKick), backKickHitboxDuration);
        }

        EndExecution();
    }

    private void EndExecution()
    {
        isExecuting = false;
        if (failsafeCoroutine != null)
        {
            StopCoroutine(failsafeCoroutine);
            failsafeCoroutine = null;
        }
    }

    // [수정됨] 5초 "후"에 그로기 여부를 확인하는 대신, Update에서 실시간 기록해둔 값을 사용
    private IEnumerator FailsafeRoutine()
    {
        yield return new WaitForSeconds(maxExecutionTime);

        if (isExecuting)
        {
            if (!wasInterruptedByGroggy)
            {
                Debug.LogWarning($"[{gameObject.name}] MidBossPattern6 안전장치 발동! " +
                                  $"{maxExecutionTime}초 안에 AnimEvent_TripleBackKickHit이 호출되지 않음. " +
                                  "Animator 클립에 해당 이벤트가 제대로 연결되어 있는지 확인 필요.");
            }
            isExecuting = false;
        }
        failsafeCoroutine = null;
    }

    private void DeactivateStamp() { if (stampHitbox != null) stampHitbox.SetActive(false); }
    private void DeactivateBackKick() { if (backKickHitbox != null) backKickHitbox.SetActive(false); }
}