using UnityEngine;

// =====================================================
// MidBossPattern2.cs (전체 교체본)
// 거미 보스 1페이즈 패턴 2 - 앞 다리 휘두르기 (검기 발사)
// =====================================================
public class MidBossPattern2 : BossPatternBase
{
    [Header("앞발 휘두르기 설정")]
    [SerializeField] private float preDelay = 0.4f;              // 선딜레이 (초)
    [SerializeField] private float slashSpeed = 8f;              // 이펙트 이동 속도
    [SerializeField] private float slashRange = 6f;              // 이펙트 최대 사거리

    [Header("히트박스 연결")]
    [SerializeField] private GameObject slashEffectPrefab;       // 발사할 이펙트 프리팹

    private Transform owner;
    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 4f;
        visualAnimator = GetComponentInChildren<Animator>();
        owner = transform;
    }

    protected override void OnExecute()
    {
        Debug.Log("[MidBossPattern2] 앞발 휘두르기 시전! doSlashPhase2 방아쇠 격발");
        // [업계 표준 트리거 적용]
        if (visualAnimator != null)
            visualAnimator.SetTrigger("doSlashPhase2");

        Invoke(nameof(FireSlash), preDelay);
    }

    private void FireSlash()
    {
        if (slashEffectPrefab == null)
        {
            Debug.LogWarning("[MidBossPattern2] slashEffectPrefab이 연결되지 않음.");
            return;
        }

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