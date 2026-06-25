using UnityEngine;

// =====================================================
// MidBossPattern7.cs (애니메이션 이벤트 적용 완료)
// =====================================================
public class MidBossPattern7 : BossPatternBase
{
    [Header("2연 휘두르기 설정 (기획자 조절)")]
    [SerializeField] private float slashHitboxDuration = 0.25f;
    [SerializeField] private float returnHitboxDuration = 0.25f;
    [SerializeField] private float conditionStampRange = 2f;

    [Header("히트박스 연결")]
    [SerializeField] private GameObject slashHitbox;
    [SerializeField] private GameObject returnHitbox;
    [SerializeField] private GameObject stampHitbox;

    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (returnHitbox != null) returnHitbox.SetActive(false);
        if (stampHitbox != null) stampHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashDouble");
    }

    // 1타 휘두르기 프레임에 "AnimEvent_Slash1" 꽂음.
    public void AnimEvent_Slash1()
    {
        if (slashHitbox != null)
        {
            slashHitbox.SetActive(true);
            Invoke(nameof(DeactivateSlash), slashHitboxDuration);
        }
    }

    // 다리 거둬들이는 타격 프레임에 "AnimEvent_SlashReturn" 꽂음.
    public void AnimEvent_SlashReturn()
    {
        if (returnHitbox != null)
        {
            returnHitbox.SetActive(true);
            Invoke(nameof(DeactivateReturn), returnHitboxDuration);
        }
    }

    // 2타 회수 끝나는 타이밍에 "AnimEvent_CheckConditionStamp" 꽂음.
    public void AnimEvent_CheckConditionStamp()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && Vector2.Distance(transform.position, playerObj.transform.position) <= conditionStampRange)
        {
            if (visualAnimator != null) visualAnimator.SetTrigger("doSlashDouble");
        }
    }

    // 연계된 앞발 찍기 타격 프레임에 "AnimEvent_ConditionStampHit" 꽂음.
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