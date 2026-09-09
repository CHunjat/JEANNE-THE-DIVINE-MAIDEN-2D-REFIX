using UnityEngine;

// 앞다리 휘두르기 패턴
public class MidBossPattern2 : BossPatternBase
{
    protected override void Awake()
    {
        base.Awake();

        cooldown = 7f;
        priority = 4;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashPhase2");
    }
}