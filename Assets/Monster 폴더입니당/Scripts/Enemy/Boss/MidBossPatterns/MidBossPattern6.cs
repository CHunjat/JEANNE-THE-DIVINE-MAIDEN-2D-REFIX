using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern6.cs
// 강화 앞발 찍기 - 중거리, 쿨타임 0초, 우선순위 5
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

    private GameObject stampHitbox;
    private GameObject backKickHitbox;
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

        // 기획서 반영
        cooldown = 0f;
        priority = 5;
        distanceType = DistanceType.Mid;
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
            isExecuting = false;
        }
    }

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