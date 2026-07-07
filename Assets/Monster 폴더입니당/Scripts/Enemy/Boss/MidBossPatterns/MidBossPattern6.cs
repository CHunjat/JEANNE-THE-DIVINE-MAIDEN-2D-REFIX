using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern6.cs
// 강화 앞발 찍기 - 중거리, 쿨타임 0초, 우선순위 5
// (isExecuting 영구 고착 방지 안전장치 추가본)
// =====================================================
public class MidBossPattern6 : BossPatternBase
{
    [Header("강화 2연찍기 이동 설정 (기획자 조절)")]
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("강화 찍기 타격 판정 시간 (기획자 조절)")]
    [SerializeField] private float stampHitboxDuration = 0.2f;
    [SerializeField] private float backKickRange = 3f;
    [SerializeField] private float backKickHitboxDuration = 0.3f;

    [Header("안전장치 - 애니메이션 이벤트 누락 시 강제 리셋 (기획자 조절)")]
    [Tooltip("이 시간이 지나도 패턴이 끝나지 않으면 강제로 isExecuting을 false로 되돌림. 애니메이션 이벤트 연결 누락 시 보스가 영구히 멈추는 것을 방지함.")]
    [SerializeField] private float maxExecutionTime = 5f;

    private GameObject stampHitbox;
    private GameObject backKickHitbox;
    private Rigidbody2D rb;
    private Animator visualAnimator;
    private bool isExecuting = false;
    private Coroutine failsafeCoroutine;

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

        if (stampHitbox != null) stampHitbox.SetActive(false);
        if (backKickHitbox != null) backKickHitbox.SetActive(false);

        // 기획서 반영
        cooldown = 0f;
        priority = 5;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        isExecuting = true;

        if (visualAnimator != null) visualAnimator.SetTrigger("doDouble");
        StartCoroutine(MoveRoutine());

        // 안전장치 시작: maxExecutionTime 안에 정상 종료 안 되면 강제 리셋
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
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateStamp), stampHitboxDuration);
        }
    }

    public void AnimEvent_CheckBackKick()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && Vector2.Distance(transform.position, playerObj.transform.position) <= backKickRange)
        {
            if (visualAnimator != null) visualAnimator.SetTrigger("doConditionBackKick");
        }
        else
        {
            EndExecution();
        }
    }

    public void AnimEvent_BackKickHit()
    {
        if (backKickHitbox != null)
        {
            backKickHitbox.SetActive(true);
            Invoke(nameof(DeactivateBackKick), backKickHitboxDuration);
        }
        EndExecution();
    }

    // 패턴 정상 종료 처리 (isExecuting 리셋 + 안전장치 코루틴 정리)를 한 곳으로 모음
    private void EndExecution()
    {
        isExecuting = false;
        if (failsafeCoroutine != null)
        {
            StopCoroutine(failsafeCoroutine);
            failsafeCoroutine = null;
        }
    }

    private IEnumerator FailsafeRoutine()
    {
        yield return new WaitForSeconds(maxExecutionTime);

        // 여기까지 왔다는 건 애니메이션 이벤트(AnimEvent_BackKickHit 등)가
        // 정상적으로 호출되지 않았다는 뜻. 강제로 리셋해서 보스가 영구히 멈추지 않게 함.
        if (isExecuting)
        {
            Debug.LogWarning($"[{gameObject.name}] MidBossPattern6 안전장치 발동! " +
                              $"{maxExecutionTime}초 안에 정상 종료되지 않아 강제 리셋함. " +
                              "Animator의 doConditionBackKick 클립에 AnimEvent_BackKickHit 이벤트가 " +
                              "제대로 연결되어 있는지 확인 필요.");
            isExecuting = false;
        }
        failsafeCoroutine = null;
    }

    private void DeactivateStamp() { if (stampHitbox != null) stampHitbox.SetActive(false); }
    private void DeactivateBackKick() { if (backKickHitbox != null) backKickHitbox.SetActive(false); }
}