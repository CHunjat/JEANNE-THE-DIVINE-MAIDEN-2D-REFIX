using UnityEngine;

// =====================================================
// MidBossPattern3.cs (전체 교체본)
// 거미 보스 1페이즈 패턴 3 - 거미줄 뱉기
// =====================================================
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정")]
    [SerializeField] private float preDelay = 0.6f;
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;

    [Header("히트박스 연결")]
    [SerializeField] private GameObject webPrefab;

    private Transform owner;
    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 6f;
        visualAnimator = GetComponentInChildren<Animator>();
        owner = transform;
    }

    protected override void OnExecute()
    {
        Debug.Log("[MidBossPattern3] 거미줄 뱉기 시전! doSpit 방아쇠 격발");
        // 아까 Web 모션 삭제했으므로 뱉기 모션인 doSpit으로 통일 격발
        if (visualAnimator != null)
            visualAnimator.SetTrigger("doSpit");

        Invoke(nameof(FireWeb), preDelay);
    }

    private void FireWeb()
    {
        if (webPrefab == null) return;

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir = playerObj != null
            ? ((Vector2)(playerObj.transform.position - owner.position)).normalized
            : Vector2.right;

        GameObject web = Instantiate(webPrefab, owner.position, Quaternion.identity);
        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();

        if (webScript != null)
            webScript.Initialize(dir, webSpeed, webRange, bindDuration);
    }
}