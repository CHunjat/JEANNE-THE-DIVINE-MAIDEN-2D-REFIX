using UnityEngine;

// =====================================================
// MidBossPattern7.cs (트리거 무한루프 버그 수정 완료본)
// =====================================================
public class MidBossPattern7 : BossPatternBase
{
    [Header("2연 휘두르기 설정 (기획자 조절)")]
    [SerializeField] private float slashHitboxDuration = 0.25f;
    [SerializeField] private float returnHitboxDuration = 0.25f;
    [SerializeField] private float conditionStampRange = 2f;

    [Header("특수 히트박스 연결 (얘만 슬롯 유지함)")]
    [SerializeField] private GameObject returnHitbox;

    private GameObject slashHitbox; // 인스펙터 슬롯 삭제함.
    private GameObject stampHitbox; // 인스펙터 슬롯 삭제함.
    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();

        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null)
        {
            slashHitbox = parent.hitBox_Slash;
            stampHitbox = parent.hitBox_Stamp;
        }

        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (returnHitbox != null) returnHitbox.SetActive(false);
        if (stampHitbox != null) stampHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashDouble");
    }

    // [애니메이션 이벤트 연동용 함수 1]
    public void AnimEvent_Slash1()
    {
        if (slashHitbox != null)
        {
            slashHitbox.SetActive(true);
            Invoke(nameof(DeactivateSlash), slashHitboxDuration);
        }
    }

    // [애니메이션 이벤트 연동용 함수 2]
    public void AnimEvent_SlashReturn()
    {
        if (returnHitbox != null)
        {
            returnHitbox.SetActive(true);
            Invoke(nameof(DeactivateReturn), returnHitboxDuration);
        }
    }

    // [애니메이션 이벤트 연동용 함수 3]
    public void AnimEvent_CheckConditionStamp()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && Vector2.Distance(transform.position, playerObj.transform.position) <= conditionStampRange)
        {
            // [핵심 수정] 무한루프 돌던 doSlashDouble 대신 애니메이터에 있는 3타 찍기 트리거를 쏨!
            if (visualAnimator != null) visualAnimator.SetTrigger("doSlashTriple");
        }
    }

    // [애니메이션 이벤트 연동용 함수 4]
    public void AnimEvent_ConditionStampHit()
    {
        if (stampHitbox != null)
        {
            stampHitbox.SetActive(true);
            Invoke(nameof(DeactivateStamp), 0.2f);
        }
    }

    private void DeactivateSlash() { if (slashHitbox != null) slashHitbox.SetActive(false); }
    private void DeactivateReturn() { if (returnHitbox != null) returnHitbox.SetActive(false); }
    private void DeactivateStamp() { if (stampHitbox != null) stampHitbox.SetActive(false); }
}