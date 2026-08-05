using UnityEngine;

// 거미줄 뱉기 패턴 (에임봇 및 궤도 꼬임 완벽 방지)
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정")]
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;
    [SerializeField] private float playerYOffset = 0f;

    [SerializeField] private GameObject webPrefab;
    [SerializeField] private Transform webSpawnPoint;

    private Transform owner;
    private bool hasFiredThisTurn = false;

    // 발사할 '각도'가 아니라, 플레이어의 '목표 좌표'를 락온(Lock)하는 변수
    private Vector3 lockedTargetPos;
    private bool isTargetLocked = false;

    protected override void Awake()
    {
        base.Awake();
        owner = transform;

        cooldown = 6f;
        priority = 3;
        distanceType = DistanceType.Far;
        canUseInChase = true;
    }

    protected override void OnExecute()
    {
        hasFiredThisTurn = false;

        // 패턴 시작 시, 플레이어의 '현재 위치(좌표)'를 락온! (에임봇 방지)
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            lockedTargetPos = playerObj.transform.position + new Vector3(0, playerYOffset, 0);
            isTargetLocked = true;
        }
        else
        {
            isTargetLocked = false;
        }

        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
    }

    public void AnimEvent_SpitWeb()
    {
        if (!isExecuting || hasFiredThisTurn) return;
        hasFiredThisTurn = true;
        if (webPrefab == null) return;

        // 핀을 밟은 현재 시점의 발사구 위치
        Vector3 spawnPos = webSpawnPoint != null ? webSpawnPoint.position : owner.position;

        // 보스가 현재 바라보고 있는 정면 방향 구하기
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        bool isFacingLeft = (sr != null && sr.flipX);
        float forwardX = isFacingLeft ? -1f : 1f;

        Vector2 finalDir;

        if (isTargetLocked)
        {
            // 발사구에서 락온된 타겟 좌표를 향하는 방향 계산 (궤도 꼬임 해결)
            Vector2 dirToTarget = ((Vector2)(lockedTargetPos - spawnPos)).normalized;

            // [핵심] 만약 플레이어가 폼 잡는 동안 등 뒤로 넘어갔다면? (X 방향 부호가 다르면)
            if (Mathf.Sign(dirToTarget.x) != Mathf.Sign(forwardX))
            {
                // 등 뒤(엉덩이)로 어색하게 쏘지 않고, 원래 바라보던 정면으로 헛스윙 발사!
                finalDir = new Vector2(forwardX, 0f);
            }
            else
            {
                finalDir = dirToTarget;
            }
        }
        else
        {
            // 타겟을 못 찾았으면 그냥 정면 발사
            finalDir = new Vector2(forwardX, 0f);
        }

        GameObject web = Instantiate(webPrefab, spawnPos, Quaternion.identity);

        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
        if (webScript != null)
        {
            webScript.Initialize(finalDir, webSpeed, webRange, bindDuration);
        }
    }

    public override void EndExecution()
    {
        base.EndExecution();
        hasFiredThisTurn = false;
    }
}