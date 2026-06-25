using UnityEngine;

// =====================================================
// MidBossPattern3.cs (애니메이션 이벤트 적용 완료)
// =====================================================
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정 (기획자 조절)")]
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;      // 구속 상태이상 지속 시간임.

    [Header("히트박스 연결")]
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
    // 거미가 입에서 침을 뱉어내는 딱 그 프레임에 "AnimEvent_SpitWeb" 적어 넣음.
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