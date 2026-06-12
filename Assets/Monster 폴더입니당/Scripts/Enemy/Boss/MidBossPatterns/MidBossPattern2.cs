using UnityEngine;

// =====================================================
// MidBossPattern2.cs
// 거미 보스 1페이즈 패턴 2 - 앞 다리 휘두르기
//
// [기획 문서 기준]
// - 플레이어 방향으로 앞다리를 휘둘러 공격함
// - 앞다리 들어올릴 때 눈 반짝임으로 앞발 찍기와 구분 가능하게 해야 함
//   (애니메이션 작업 시 협력 필요)
// - 사진처럼 이펙트(검기)가 나가는 방식으로 구현함
//
// [히트박스 세팅 방법 - 이펙트 히트박스]
// 1. Project 창에서 빈 오브젝트로 "SlashEffect" 프리팹 만들기
// 2. SlashEffect에 CircleCollider2D 붙이고 Is Trigger 체크
// 3. SlashEffect에 EnemyHitbox 스크립트 붙이기
//    - Owner Damage: 보스 공격력 수치
//    - Damage Ratio: 이 패턴의 반영 비율
//    - Destroy On Hit: 체크 안 함 (이펙트는 관통)
// 4. SlashEffect에 SlashProjectile 스크립트 붙이기 (아래 별도 파일)
// 5. 이 스크립트의 slashEffectPrefab 필드에 SlashEffect 프리팹 드래그
//
// [나중에 스프라이트 받으면]
// SlashEffect 프리팹에 SpriteRenderer 추가하고 스프라이트 넣으면 됨.
// =====================================================
public class MidBossPattern2 : BossPatternBase
{
    [Header("앞발 휘두르기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float preDelay = 0.4f;              // 선딜레이 (초)
    [SerializeField] private float slashSpeed = 8f;              // 이펙트 이동 속도
    [SerializeField] private float slashRange = 6f;              // 이펙트 최대 사거리

    [Header("히트박스 연결 - 인스펙터에서 SlashEffect 프리팹을 드래그해서 넣을 것")]
    [SerializeField] private GameObject slashEffectPrefab;       // 발사할 이펙트 프리팹

    private Transform owner;

    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 4f;  // 임시 쿨타임 - 기획 확정 후 수정할 것
        visualAnimator = GetComponentInChildren<Animator>();
        owner = transform;
    }

    protected override void OnExecute()
    {
        Debug.Log("[MidBossPattern2] 앞발 휘두르기 시전!");
        if (visualAnimator != null) visualAnimator.Play("Slash Attack_Fase2");
        Invoke(nameof(FireSlash), preDelay);
    }

    private void FireSlash()
    {
        if (slashEffectPrefab == null)
        {
            Debug.LogWarning("[MidBossPattern2] slashEffectPrefab이 연결되지 않음. 인스펙터에서 프리팹을 넣을 것.");
            return;
        }

        // 플레이어 방향으로 이펙트 발사
        GameObject slash = Instantiate(slashEffectPrefab, owner.position, Quaternion.identity);
        SlashProjectile projectile = slash.GetComponent<SlashProjectile>();

        if (projectile != null)
        {
            // 보스가 바라보는 방향 계산
            GameObject playerObj = GameObject.FindWithTag("Player");
            Vector2 dir = playerObj != null
                ? ((Vector2)(playerObj.transform.position - owner.position)).normalized
                : Vector2.right;

            projectile.Initialize(dir, slashSpeed, slashRange);
        }
    }
}