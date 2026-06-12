using UnityEngine;

// =====================================================
// MidBossPattern1.cs
// 거미 보스 1페이즈 패턴 1 - 앞발 찍기
//
// [기획 문서 기준]
// - 현재 위치에서 플레이어 위치로 앞다리를 들어올렸다가 내리찍어 공격함
// - 들어올리는 타이밍과 내리찍는 타이밍 조절이 핵심
// - 타이밍 조절은 애니메이션 작업 시 협력 필요
//
// [애니메이션 연동]
// Animator Controller에 "NormalAttack" 클립이 있어야 함.
// Aseprite 태그 이름과 일치해야 함.
//
// [히트박스 세팅 방법]
// 1. MidBoss_Spider 아래 자식으로 "Hitbox_Stamp" 만들기
// 2. CircleCollider2D (Is Trigger 체크) + EnemyHitbox 붙이기
// 3. 오브젝트 꺼두기
// 4. stampHitbox 필드에 드래그
// =====================================================
public class MidBossPattern1 : BossPatternBase
{
    [Header("앞발 찍기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float preDelay = 0.4f;              // 선딜레이 (앞발 들어올리는 시간)
    [SerializeField] private float hitboxActiveDuration = 0.2f;  // 히트박스 유지 시간

    [Header("히트박스 연결 - 인스펙터에서 Hitbox_Stamp를 드래그해서 넣을 것")]
    [SerializeField] private GameObject stampHitbox;

    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 3f;  // 임시 쿨타임 - 기획 확정 후 수정할 것

        // Visual 자식 오브젝트의 Animator 가져오기
        visualAnimator = GetComponentInChildren<Animator>();

        if (stampHitbox != null)
            stampHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("[MidBossPattern1] 앞발 찍기 시전!");

        // 애니메이션 재생
        // Aseprite 태그 이름이 다르면 여기 이름 수정할 것
        if (visualAnimator != null)
            visualAnimator.Play("Attack 1");

        // 선딜레이 후 히트박스 활성화
        Invoke(nameof(ActivateHitbox), preDelay);
    }

    private void ActivateHitbox()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateHitbox), hitboxActiveDuration);
        }
    }

    private void DeactivateHitbox()
    {
        if (stampHitbox != null)
            stampHitbox.SetActive(false);
    }
}