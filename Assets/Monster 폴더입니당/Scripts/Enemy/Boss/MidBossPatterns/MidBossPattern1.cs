using UnityEngine;

// =====================================================
// MidBossPattern1.cs (인스펙터 슬롯 중복 제거 완료)
// =====================================================
public class MidBossPattern1 : BossPatternBase
{
    [Header("앞발 찍기 설정 (기획자 조절)")]
    [SerializeField] private float hitboxActiveDuration = 0.2f;  // 타격 판정이 켜져 있는 시간임.

    private GameObject stampHitbox; // 인스펙터 슬롯 삭제하고 코드로 부모한테 받아올 변수임.
    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();

        // 부모(MidBoss) 스크립트를 찾아가서 이미 연결해둔 스탬프 히트박스 주소를 자동으로 훔쳐옴.
        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null)
        {
            stampHitbox = parent.hitBox_Stamp;
        }

        if (stampHitbox != null) stampHitbox.SetActive(false);

        // [기획 반영] 할 거 없을 때 쓰는 기본 콤보 공격이라 패턴 자체 쿨타임은 0초로 밀어버림.
        cooldown = 0f;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doAttack1");
    }

    // [애니메이션 이벤트 연동용 함수]
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