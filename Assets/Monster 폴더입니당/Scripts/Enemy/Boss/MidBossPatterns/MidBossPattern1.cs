using UnityEngine;

// =====================================================
// MidBossPattern1.cs
// 거미 보스 1페이즈 패턴 1 - 앞발 찍기 (전체 수정본)
// =====================================================
public class MidBossPattern1 : BossPatternBase
{
    [Header("앞발 찍기 설정")]
    [SerializeField] private float preDelay = 0.4f;           // 선딜레이 (앞발 들어올리는 시간)
    [SerializeField] private float hitboxActiveDuration = 0.2f;  // 히트박스 유지 시간

    [Header("히트박스 연결")]
    [SerializeField] private GameObject stampHitbox;

    private Animator visualAnimator;

    private void Awake()
    {
        // 상속받은 기본 쿨타임 세팅
        cooldown = 3f;

        // 자식 오브젝트에 붙어있는 Animator 컴포넌트 안전하게 자동 긁어오기
        visualAnimator = GetComponentInChildren<Animator>();

        // 게임 시작 시 히트박스는 기본적으로 꺼둠
        if (stampHitbox != null)
            stampHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        Debug.Log("[MidBossPattern1] 앞발 찍기 시전! doAttack1 트리거 발동");

        // [업계 표준 연동] 구식 Play()를 제거하고, 우리가 애니메이터에 만든 트리거를 정확히 저격해서 신호를 보냄
        if (visualAnimator != null)
        {
            visualAnimator.SetTrigger("doAttack1");
        }

        // 지정된 선딜레이(preDelay) 시간이 지난 후에 실제 데미지를 주는 히트박스를 켬
        Invoke(nameof(ActivateHitbox), preDelay);
    }

    private void ActivateHitbox()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(true);
            // 유지 시간이 지나면 히트박스를 다시 끔
            Invoke(nameof(DeactivateHitbox), hitboxActiveDuration);
        }
    }

    private void DeactivateHitbox()
    {
        if (stampHitbox != null)
            stampHitbox.SetActive(false);
    }
}