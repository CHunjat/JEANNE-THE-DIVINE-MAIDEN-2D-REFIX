using UnityEngine;
// =====================================================
// MidBossPattern3.cs
// =====================================================
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정 (기획자 조절)")]
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;
    [SerializeField] private float playerYOffset = 1.5f;

    [Header("발사체 프리팹 연결")]
    [SerializeField] private GameObject webPrefab;

    [Header("발사 위치 (거미 입 근처 빈 오브젝트 연결)")]
    [SerializeField] private Transform webSpawnPoint;

    private Transform owner;
    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = transform;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
    }

    public void AnimEvent_SpitWeb()
    {
        if (webPrefab == null)
        {
            Debug.LogWarning("[MidBossPattern3] webPrefab이 연결되지 않았습니다.");
            return;
        }

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        bool isFacingLeft = (sr != null && sr.flipX);

        // 발사 위치: 왼쪽 볼 때 X 반전
        Vector3 spawnPos = owner.position;
        if (webSpawnPoint != null)
        {
            Vector3 localOffset = webSpawnPoint.localPosition;
            if (isFacingLeft) localOffset.x = -localOffset.x;
            spawnPos = owner.position + localOffset;
        }

        // 플레이어 방향 계산
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
            webScript.Initialize(dir, webSpeed, webRange, bindDuration);

        Debug.Log($"<color=cyan>[MidBossPattern3] 거미줄 발사! 방향: {dir} 위치: {spawnPos}</color>");
    }
}