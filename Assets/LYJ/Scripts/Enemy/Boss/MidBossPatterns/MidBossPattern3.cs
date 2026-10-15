using UnityEngine;

public class MidBossPattern3 : BossPatternBase
{
    [Header("Railgun Settings")]
    [SerializeField] private float webSpeed = 75f;
    [SerializeField] private float webRange = 30f;
    [SerializeField] private float bindDuration = 3f;
    [SerializeField] private float playerYOffset = 0f;

    [SerializeField] private GameObject webPrefab;
    [SerializeField] private Transform webSpawnPoint;

    private Transform owner;
    private bool hasFiredThisTurn = false;

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
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
    }

    public void AnimEvent_SpitWeb()
    {
        if (!isExecuting || hasFiredThisTurn) return;
        hasFiredThisTurn = true;

        if (webPrefab == null) return;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        float bossY = webSpawnPoint != null ? webSpawnPoint.position.y : owner.position.y;
        float playerY = playerObj.transform.position.y + playerYOffset;
        float yDiff = Mathf.Abs(playerY - bossY);

        if (yDiff < 6.0f)
        {
            Vector3 spawnPos = webSpawnPoint != null ? webSpawnPoint.position : owner.position;

            // 플레이어 쪽 무조건 조준
            float xDiff = playerObj.transform.position.x - owner.position.x;
            Vector2 finalDir = new Vector2(xDiff < 0 ? -1f : 1f, 0f);

            // 강제 0도(Quaternion.identity) 삭제하고, 프리팹 원본 각도(Y=90) 그대로 살려서 발사
            GameObject web = Instantiate(webPrefab, spawnPos, webPrefab.transform.rotation);

            MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
            if (webScript != null)
            {
                webScript.Initialize(finalDir, webSpeed, webRange, bindDuration);
            }
        }
    }

    public override void EndExecution()
    {
        base.EndExecution();
        hasFiredThisTurn = false;
    }
}