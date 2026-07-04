using UnityEngine;
// =====================================================
// MidBossPattern7.cs
// 강화 앞다리 휘두르기 - 중거리, 쿨타임 7초, 우선순위 4
// =====================================================
public class MidBossPattern7 : BossPatternBase
{
    [Header("2연 휘두르기 설정 (기획자 조절)")]
    [SerializeField] private float slashHitboxDuration = 0.25f;
    [SerializeField] private float returnHitboxDuration = 0.25f;
    [SerializeField] private float conditionStampRange = 2f;

    [Header("특수 히트박스 연결 (얘만 슬롯 유지함)")]
    [SerializeField] private GameObject returnHitbox;

    private GameObject slashHitbox;
    private GameObject stampHitbox;
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

        // 기획서 반영
        cooldown = 7f;
        priority = 4;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashDouble");
    }

    public void AnimEvent_Slash1()
    {
        if (slashHitbox != null)
        {
            slashHitbox.SetActive(true);
            Invoke(nameof(DeactivateSlash), slashHitboxDuration);
        }
    }

    public void AnimEvent_SlashReturn()
    {
        if (returnHitbox != null)
        {
            returnHitbox.SetActive(true);
            Invoke(nameof(DeactivateReturn), returnHitboxDuration);
        }
    }

    public void AnimEvent_CheckConditionStamp()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && Vector2.Distance(transform.position, playerObj.transform.position) <= conditionStampRange)
        {
            if (visualAnimator != null) visualAnimator.SetTrigger("doSlashTriple");
        }
    }

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