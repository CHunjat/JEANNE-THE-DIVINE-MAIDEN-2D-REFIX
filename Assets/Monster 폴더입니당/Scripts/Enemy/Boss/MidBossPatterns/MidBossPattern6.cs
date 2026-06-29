using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern6.cs (인스펙터 슬롯 2개 전면 자동화 완료)
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

    private GameObject stampHitbox;    // 인스펙터 슬롯 삭제함.
    private GameObject backKickHitbox; // 인스펙터 슬롯 삭제함.

    private Rigidbody2D rb;
    private Animator visualAnimator;
    private bool isExecuting = false;

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
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        if (visualAnimator != null) visualAnimator.SetTrigger("doDouble");
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        isExecuting = true;
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

    // [애니메이션 이벤트 연동용 함수 1]
    public void AnimEvent_DoubleStamp()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateStamp), stampHitboxDuration);
        }
    }

    // [애니메이션 이벤트 연동용 함수 2]
    public void AnimEvent_CheckBackKick()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && Vector2.Distance(transform.position, playerObj.transform.position) <= backKickRange)
        {
            // [수정] doDouble 중복 트리거 대신 조건부 뒷발차기 전용 트리거를 쏴서 모션 끊김 방지함!
            if (visualAnimator != null) visualAnimator.SetTrigger("doConditionBackKick");
        }
        else
        {
            isExecuting = false;
        }
    }

    // [애니메이션 이벤트 연동용 함수 3]
    public void AnimEvent_BackKickHit()
    {
        if (backKickHitbox != null)
        {
            backKickHitbox.SetActive(true);
            Invoke(nameof(DeactivateBackKick), backKickHitboxDuration);
        }
        isExecuting = false;
    }

    private void DeactivateStamp() { if (stampHitbox != null) stampHitbox.SetActive(false); }
    private void DeactivateBackKick() { if (backKickHitbox != null) backKickHitbox.SetActive(false); }
}