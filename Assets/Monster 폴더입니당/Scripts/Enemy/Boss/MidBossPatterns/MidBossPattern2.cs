using UnityEngine;

// =====================================================
// MidBossPattern2.cs (애니메이션 이벤트 적용 완료)
// =====================================================
public class MidBossPattern2 : BossPatternBase
{
    [Header("검기 발사 설정 (기획자 조절)")]
    [SerializeField] private float slashSpeed = 8f;              // 검기 이동 속도임.
    [SerializeField] private float slashRange = 6f;              // 검기 최대 사거리임.

    [Header("히트박스 연결")]
    [SerializeField] private GameObject slashEffectPrefab;

    private Transform owner;
    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = transform;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashPhase2");
    }

    // [애니메이션 이벤트 연동용 함수]
    // 거미가 팔을 휙 뻗는 프레임에 "AnimEvent_FireSlash" 적어 넣으면 됨.
    public void AnimEvent_FireSlash()
    {
        if (slashEffectPrefab == null) return;

        GameObject slash = Instantiate(slashEffectPrefab, owner.position, Quaternion.identity);
        SlashProjectile projectile = slash.GetComponent<SlashProjectile>();

        if (projectile != null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            Vector2 dir = playerObj != null
                ? ((Vector2)(playerObj.transform.position - owner.position)).normalized
                : Vector2.right;

            projectile.Initialize(dir, slashSpeed, slashRange);
        }
    }
}