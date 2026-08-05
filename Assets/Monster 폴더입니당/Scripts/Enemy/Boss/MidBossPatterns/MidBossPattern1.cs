using UnityEngine;
using System.Collections;

// 앞발 찍기 패턴
public class MidBossPattern1 : BossPatternBase
{
    [SerializeField] private float hitboxActiveDuration = 0.2f;

    private GameObject stampHitbox;
    private Coroutine hitboxCoroutine;

    protected override void Awake()
    {
        base.Awake();
        MidBoss parent = GetComponent<MidBoss>();

        if (parent != null) stampHitbox = parent.hitBox_Stamp;
        if (stampHitbox != null) stampHitbox.SetActive(false);

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
            if (hitboxCoroutine != null) StopCoroutine(hitboxCoroutine);
            hitboxCoroutine = StartCoroutine(HitboxRoutine(stampHitbox, hitboxActiveDuration));
        }
    }

    private IEnumerator HitboxRoutine(GameObject hitbox, float duration)
    {
        hitbox.SetActive(false);
        hitbox.SetActive(true);

        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }
}