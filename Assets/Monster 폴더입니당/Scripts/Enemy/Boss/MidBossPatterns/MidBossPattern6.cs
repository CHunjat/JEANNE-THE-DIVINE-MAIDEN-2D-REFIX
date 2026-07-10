using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern6.cs
// 강화 앞발 찍기 2연찍기 + 뒷발 찌르기 (무조건 3연타)
// (그로기로 인한 정상 중단 시 안전장치 경고 스킵하도록 수정)
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
    private EnemyGroggy groggy; // [추가됨] 그로기 상태 확인용
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

        groggy = GetComponent<EnemyGroggy>(); // [추가됨]

        if (stampHitbox != null) stampHitbox.SetActive(false);
        if (backKickHitbox != null) backKickHitbox.SetActive(false);

        cooldown = 0f;
        priority = 5;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        isExecuting = true;

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

    // [수정됨] 그로기로 인한 정상적인 중단이면 경고 없이 조용히 리셋
    private IEnumerator FailsafeRoutine()
    {
        yield return new WaitForSeconds(maxExecutionTime);

        if (isExecuting)
        {
            bool wasInterruptedByGroggy = (groggy != null && groggy.IsGroggy);

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