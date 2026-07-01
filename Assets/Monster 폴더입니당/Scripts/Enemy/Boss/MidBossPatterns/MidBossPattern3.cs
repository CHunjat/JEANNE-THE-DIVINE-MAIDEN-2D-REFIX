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

    [Header("발사체 프리팹 연결 (인스펙터에서 MidBossWebProjectile 프리팹 드래그)")]
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
    // Attack Spit 클립의 거미줄 뱉는 타이밍 프레임에 이 함수 핀으로 꽂을 것
    public void AnimEvent_SpitWeb()
    {
        if (webPrefab == null)
        {
            Debug.LogWarning("[MidBossPattern3] webPrefab이 연결되지 않았습니다. 인스펙터에서 프리팹을 연결하세요.");
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir = playerObj != null
            ? ((Vector2)(playerObj.transform.position - owner.position)).normalized
            : Vector2.right;

        GameObject web = Instantiate(webPrefab, owner.position, Quaternion.identity);
        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
        if (webScript != null)
            webScript.Initialize(dir, webSpeed, webRange, bindDuration);

        Debug.Log("<color=cyan>[MidBossPattern3] 거미줄 발사!</color>");
    }
}