using UnityEngine;
// =====================================================
// MidBossPattern2.cs
// 앞다리 휘두르기 - 중거리, 쿨타임 7초, 우선순위 4
// =====================================================
public class MidBossPattern2 : BossPatternBase
{
    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();

        // 기획서 반영
        cooldown = 7f;
        priority = 4;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashPhase2");
    }
}