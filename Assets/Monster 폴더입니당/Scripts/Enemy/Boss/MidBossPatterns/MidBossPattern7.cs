using UnityEngine;
using System.Collections;

// 슬래시 3연타 패턴
public class MidBossPattern7 : BossPatternBase
{
    [SerializeField] private float slashHitboxDuration = 0.25f;
    [SerializeField] private float returnHitboxDuration = 0.25f;
    [SerializeField] private float stampHitboxDuration = 0.2f;

    [SerializeField] private GameObject returnHitbox;

    private GameObject slashHitbox;
    private GameObject stampHitbox;
    private Coroutine currentHitboxCoroutine;

    protected override void Awake()
    {
        base.Awake();

        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null)
        {
            slashHitbox = parent.hitBox_Slash;
            stampHitbox = parent.hitBox_Stamp;
        }

        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (returnHitbox != null) returnHitbox.SetActive(false);
        if (stampHitbox != null) stampHitbox.SetActive(false);

        cooldown = 7f;
        priority = 4;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doSlashTriple");
    }

    public void AnimEvent_Slash1()
    {
        if (slashHitbox != null)
        {
            if (currentHitboxCoroutine != null) StopCoroutine(currentHitboxCoroutine);
            currentHitboxCoroutine = StartCoroutine(SmartHitboxRoutine(slashHitbox, slashHitboxDuration));
        }
    }

    public void AnimEvent_SlashReturn()
    {
        if (returnHitbox != null)
        {
            if (currentHitboxCoroutine != null) StopCoroutine(currentHitboxCoroutine);
            currentHitboxCoroutine = StartCoroutine(SmartHitboxRoutine(returnHitbox, returnHitboxDuration));
        }
    }

    public void AnimEvent_CheckConditionStamp() { }

    public void AnimEvent_TripleStampHit()
    {
        if (stampHitbox != null)
        {
            if (currentHitboxCoroutine != null) StopCoroutine(currentHitboxCoroutine);
            currentHitboxCoroutine = StartCoroutine(SmartHitboxRoutine(stampHitbox, stampHitboxDuration));
        }
    }

    private IEnumerator SmartHitboxRoutine(GameObject hitbox, float duration)
    {
        EnemyHitbox hitComp = hitbox.GetComponent<EnemyHitbox>();

        if (hitbox.activeSelf && hitComp != null)
        {
            hitComp.ResetHitRecord();
        }
        else
        {
            hitbox.SetActive(true);
        }

        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }
}