using UnityEngine;
using System.Collections;

// 슬래시 3연타 패턴임. (단일 히트박스 유지 버전)
public class MidBossPattern7 : BossPatternBase
{
    [SerializeField] private float slashHitboxDuration = 0.25f;
    [SerializeField] private float returnHitboxDuration = 0.25f;
    [SerializeField] private float stampHitboxDuration = 0.2f;

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
        // 형 말대로 분리하지 않고 동일한 slashHitbox를 그대로 다시 사용함.
        if (slashHitbox != null)
        {
            if (currentHitboxCoroutine != null) StopCoroutine(currentHitboxCoroutine);
            currentHitboxCoroutine = StartCoroutine(SmartHitboxRoutine(slashHitbox, returnHitboxDuration));
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
        hitbox.SetActive(true);

        EnemyHitbox hitComp = hitbox.GetComponent<EnemyHitbox>();
        if (hitComp != null)
        {
            hitComp.ResetHitRecord();
        }

        // 공백 프레임(WaitForFixedUpdate)을 완전히 제거함.
        // 대신 콜라이더만 같은 프레임 내에서 즉시 껐다 켜서 물리 판정을 강제로 재활성화함.
        Collider2D col = hitbox.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
            col.enabled = true;
        }

        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }

    public override void EndExecution()
    {
        base.EndExecution();

        if (currentHitboxCoroutine != null)
        {
            StopCoroutine(currentHitboxCoroutine);
            currentHitboxCoroutine = null;
        }

        if (slashHitbox != null) slashHitbox.SetActive(false);
        if (stampHitbox != null) stampHitbox.SetActive(false);
    }
}