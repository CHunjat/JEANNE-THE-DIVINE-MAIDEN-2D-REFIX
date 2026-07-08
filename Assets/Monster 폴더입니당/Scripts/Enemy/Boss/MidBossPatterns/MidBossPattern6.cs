using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern6.cs
// 강화 앞발 찍기 2연찍기 + 뒷발 찌르기 (무조건 3연타)
// 조건부 폐기 - Triple Attack은 항상 3타 전부 나감
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
    [Tooltip("이 시간이 지나도 패턴이 끝나지 않으면 강제로 isExecuting을 false로 되돌림.")]
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

        // 애니메이터에 실제로 존재하는 파라미터 이름(doTriple)로 트리거
        // 조건부 없이 항상 3연타(앞발 2번 + 뒷발 찌르기) 전체가 재생됨
        if (visualAnimator != null) visualAnimator.SetTrigger("doTriple");
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

    // 애니메이션 이벤트: 앞발 찍기 (클립에 이미 2번 찍혀있음, 그대로 사용)
    public void AnimEvent_DoubleStamp()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateStamp), stampHitboxDuration);
        }
    }

    // 애니메이션 이벤트: 뒷발 찌르기 (조건 없이 무조건 실행 + 패턴 종료 처리)
    public void AnimEvent_BackKickHit()
    {
        if (backKickHitbox != null)
        {
            backKickHitbox.SetActive(true);
            Invoke(nameof(DeactivateBackKick), backKickHitboxDuration);
        }

        // 뒷발 찌르기가 패턴의 마지막 동작이므로 여기서 바로 종료 처리
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

    private IEnumerator FailsafeRoutine()
    {
        yield return new WaitForSeconds(maxExecutionTime);

        if (isExecuting)
        {
            Debug.LogWarning($"[{gameObject.name}] MidBossPattern6 안전장치 발동! " +
                              $"{maxExecutionTime}초 안에 AnimEvent_BackKickHit이 호출되지 않음. " +
                              "Animator 클립에 해당 이벤트가 제대로 연결되어 있는지 확인 필요.");
            isExecuting = false;
        }
        failsafeCoroutine = null;
    }

    private void DeactivateStamp() { if (stampHitbox != null) stampHitbox.SetActive(false); }
    private void DeactivateBackKick() { if (backKickHitbox != null) backKickHitbox.SetActive(false); }
}