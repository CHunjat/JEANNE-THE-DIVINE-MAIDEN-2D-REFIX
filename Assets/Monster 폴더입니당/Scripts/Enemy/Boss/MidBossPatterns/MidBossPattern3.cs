using UnityEngine;

// =====================================================
// MidBossPattern3.cs
// =====================================================
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정")]
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;
    [SerializeField] private float playerYOffset = 1.5f;
    [SerializeField] private GameObject webPrefab;
    [SerializeField] private Transform webSpawnPoint;

    private Transform owner;
    private Animator visualAnimator;
    private bool isSpitting = false;
    private bool hasFiredThisTurn = false; // 2연발 발사 절대 차단용 

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
        hasFiredThisTurn = false; // 턴 시작 시 자물쇠 초기화
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
        Invoke(nameof(UnlockSpitting), 2.0f);
    }

    private void UnlockSpitting() { isSpitting = false; }

    public void AnimEvent_SpitWeb()
    {
        // 애니메이션 프레임 이벤트가 2번 들어와도 1발만 쏘고 칼같이 컷!
        if (hasFiredThisTurn) return;
        hasFiredThisTurn = true;

        if (webPrefab == null) return;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        bool isFacingLeft = (sr != null && sr.flipX);

        Vector3 spawnPos = owner.position;
        if (webSpawnPoint != null)
        {
            Vector3 localOffset = webSpawnPoint.localPosition;
            if (isFacingLeft) localOffset.x = -Mathf.Abs(localOffset.x);
            else localOffset.x = Mathf.Abs(localOffset.x);
            spawnPos = owner.position + localOffset;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir;
        if (playerObj != null)
        {
            Vector3 targetPos = playerObj.transform.position + new Vector3(0, playerYOffset, 0);
            dir = ((Vector2)(targetPos - spawnPos)).normalized;
        }
        else dir = new Vector2(isFacingLeft ? -1f : 1f, 0f);

        GameObject web = Instantiate(webPrefab, spawnPos, Quaternion.identity);
        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
        if (webScript != null) webScript.Initialize(dir, webSpeed, webRange, bindDuration);

        Debug.Log($"<color=cyan>[MidBossPattern3] 거미줄 발사 완료!</color>");
    }
}