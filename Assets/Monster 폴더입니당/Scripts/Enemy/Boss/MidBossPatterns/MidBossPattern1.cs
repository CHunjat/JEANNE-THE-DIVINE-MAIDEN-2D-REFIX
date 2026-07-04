using UnityEngine;
// =====================================================
// MidBossPattern1.cs
// 앞발 찍기 - 중거리, 쿨타임 0초, 우선순위 5
// =====================================================
public class MidBossPattern1 : BossPatternBase
{
    [Header("앞발 찍기 설정 (기획자 조절)")]
    [SerializeField] private float hitboxActiveDuration = 0.2f;

    private GameObject stampHitbox;
    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();

        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null) stampHitbox = parent.hitBox_Stamp;
        if (stampHitbox != null) stampHitbox.SetActive(false);

        // 기획서 반영
        cooldown = 0f;
        priority = 5;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doAttack1");
    }

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