using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern6.cs (애니메이션 이벤트 적용 완료)
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

    [Header("히트박스 연결")]
    [SerializeField] private GameObject stampHitbox;
    [SerializeField] private GameObject backKickHitbox;

    private Rigidbody2D rb;
    private Animator visualAnimator;
    private bool isExecuting = false;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

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
    // 1타 찍는 프레임과 2타 찍는 프레임 두 곳 모두에 "AnimEvent_DoubleStamp" 꽂음.
    public void AnimEvent_DoubleStamp()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateStamp), stampHitboxDuration);
        }
    }

    // [애니메이션 이벤트 연동용 함수 2]
    // 애니메이션이 완전히 끝나기 직전(또는 2타 종료 직후) 프레임에 "AnimEvent_CheckBackKick" 꽂음.
    // 사거리 내에 적이 있으면 알아서 모션 연계 방아쇠를 당김.
    public void AnimEvent_CheckBackKick()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && Vector2.Distance(transform.position, playerObj.transform.position) <= backKickRange)
        {
            if (visualAnimator != null) visualAnimator.SetTrigger("doDouble");
        }
        else
        {
            isExecuting = false; // 연계 안 하면 패턴 종료함.
        }
    }

    // [애니메이션 이벤트 연동용 함수 3]
    // 뒷발 찌르기 모션의 타격 프레임에 "AnimEvent_BackKickHit" 꽂음.
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