using UnityEngine;

// =====================================================
// MidBossPattern3.cs (발사체 프리팹 슬롯 유지 완료)
// =====================================================
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정 (기획자 조절)")]
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;      // 구속 상태이상 지속 시간임.

    [Header("발사체 프리팹 연결 (이건 새로 소환하는 거라 유지함)")]
    [SerializeField] private GameObject webPrefab;

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

    // [애니메이션 이벤트 연동용 함수]
    public void AnimEvent_SpitWeb()
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