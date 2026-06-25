using UnityEngine;

// =====================================================
// MidBossPattern1.cs (애니메이션 이벤트 적용 완료)
// =====================================================
public class MidBossPattern1 : BossPatternBase
{
    [Header("앞발 찍기 설정 (기획자 조절)")]
    [SerializeField] private float hitboxActiveDuration = 0.2f;  // 타격 판정이 켜져 있는 시간임.

    [Header("히트박스 연결")]
    [SerializeField] private GameObject stampHitbox;

    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        if (stampHitbox != null) stampHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doAttack1");
    }

    // [애니메이션 이벤트 연동용 함수]
    // Animation 창에서 Event String에 "AnimEvent_Stamp" 라고 적으면 이 프레임에 판정이 켜짐.
    public void AnimEvent_Stamp()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateHitbox), hitboxActiveDuration);
        }
    }

    private void DeactivateHitbox()
    {
        if (stampHitbox != null) stampHitbox.SetActive(false);
    }
}   