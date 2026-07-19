using UnityEngine;

// =====================================================
// MidBossPattern3.cs 거미줄 뱉기
// 수정: 거미줄이 위로 솟구치지 않도록 Y오프셋 기본값 0으로 변경
// =====================================================
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정")]
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;

    [Tooltip("거미줄이 향할 플레이어의 높이 오프셋 (승천 버그 방지용)")]
    [SerializeField] private float playerYOffset = 0f; // 1.5에서 0으로 변경!

    [SerializeField] private GameObject webPrefab;
    [SerializeField] private Transform webSpawnPoint;

    private Transform owner;
    private Animator visualAnimator;
    private bool isSpitting = false;
    private bool hasFiredThisTurn = false;

    public override bool IsBusy => isSpitting;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = transform;
        cooldown = 6f;
        priority = 3;
        distanceType = DistanceType.Far;
        canUseInChase = true;
    }

    protected override void OnExecute()
    {
        if (isSpitting) return;
        isSpitting = true;
        hasFiredThisTurn = false;
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
        Invoke(nameof(UnlockSpitting), 2.0f);
    }

    private void UnlockSpitting() { isSpitting = false; }

    public void AnimEvent_SpitWeb()
    {
        if (!isSpitting || hasFiredThisTurn) return;
        hasFiredThisTurn = true;
        if (webPrefab == null) return;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        bool isFacingLeft = (sr != null && sr.flipX);

        Vector3 spawnPos;
        if (webSpawnPoint == null)
        {
            Debug.LogError("Web Spawn Point 누락!");
            spawnPos = owner.position;
        }
        else
        {
            spawnPos = webSpawnPoint.position;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir;
        if (playerObj != null)
        {
            Vector3 targetPos = playerObj.transform.position + new Vector3(0, playerYOffset, 0);
            dir = ((Vector2)(targetPos - spawnPos)).normalized;
        }
        else
        {
            dir = new Vector2(isFacingLeft ? -1f : 1f, 0f);
        }

        GameObject web = Instantiate(webPrefab, spawnPos, Quaternion.identity);

        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
        if (webScript != null)
        {
            webScript.Initialize(dir, webSpeed, webRange, bindDuration);
        }
    }

    public void EndExecution()
    {
        isSpitting = false;
        hasFiredThisTurn = false;
        CancelInvoke(nameof(UnlockSpitting));
    }
}