using UnityEngine;

// =====================================================
// MidBossPattern3.cs 거미줄 뱉기 (발사 높이 강제 조절 기능 추가)
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

    [Header("발사 위치 높이 강제 조절 (이미지 쳐짐 해결용)")]
    [SerializeField] private float manualYOffset = 0f; // ★ 인스펙터에서 이 값을 올려서 입까지 맞추면 됨!

    private Transform owner;
    private Animator visualAnimator;
    private bool isSpitting = false;
    private bool hasFiredThisTurn = false;

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

        Vector3 spawnPos = owner.position;

        if (webSpawnPoint != null)
        {
            float offsetX = Mathf.Abs(webSpawnPoint.localPosition.x);

            // X축: 보스가 왼쪽 보면 왼쪽에서, 오른쪽 보면 오른쪽에서 오차 없이 발사
            float finalX = isFacingLeft ? (owner.position.x - offsetX) : (owner.position.x + offsetX);

            // Y축: 입 위치(World Y) + 수동으로 끌어올린 높이
            float finalY = webSpawnPoint.position.y + manualYOffset;

            spawnPos = new Vector3(finalX, finalY, owner.position.z);
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
}