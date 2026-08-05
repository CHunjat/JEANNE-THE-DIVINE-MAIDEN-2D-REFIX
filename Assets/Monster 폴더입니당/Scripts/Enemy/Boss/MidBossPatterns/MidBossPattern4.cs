using UnityEngine;
using System.Collections;

// 기본 점프 공격 패턴
public class MidBossPattern4 : BossPatternBase
{
    [SerializeField] private float trackTime = 2.7f;
    [SerializeField] private float dropDelay = 0.3f;
    [SerializeField] private float lockBeforeLandTime = 0.6f;

    private GameObject landingHitbox;
    private float startY;

    protected override void Awake()
    {
        base.Awake();
        MidBoss owner = GetComponent<MidBoss>();

        if (owner != null) landingHitbox = owner.hitBox_Landing;
        if (landingHitbox != null) landingHitbox.SetActive(false);

        cooldown = 20f;
        priority = 2;
        distanceType = DistanceType.Any;
        canUseInChase = true;
    }

    protected override void OnExecute()
    {
        startY = transform.position.y;
        if (visualAnimator != null) visualAnimator.SetTrigger("doJump");
    }

    public void AnimEvent_JumpAir()
    {
        if (!isExecuting) return;

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        Transform hurtbox = transform.Find("Hurtbox_Body");
        if (hurtbox != null) hurtbox.gameObject.SetActive(false);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(TrackAndDropRoutine());
    }

    private IEnumerator TrackAndDropRoutine()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        float trackingDuration = Mathf.Max(0f, trackTime - lockBeforeLandTime);
        float timer = 0f;

        while (timer < trackingDuration)
        {
            if (playerObj != null)
                transform.position = new Vector2(playerObj.transform.position.x, startY);
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(lockBeforeLandTime);
        yield return new WaitForSeconds(dropDelay);

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(true);

        Transform hurtbox = transform.Find("Hurtbox_Body");
        if (hurtbox != null) hurtbox.gameObject.SetActive(true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        if (visualAnimator != null) visualAnimator.SetTrigger("doLand");
    }

    public void AnimEvent_LandImpact()
    {
        if (!isExecuting) return;

        if (landingHitbox != null)
        {
            StartCoroutine(SmartHitboxRoutine(landingHitbox));
        }
    }

    private IEnumerator SmartHitboxRoutine(GameObject hitbox)
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

        yield return new WaitForSeconds(0.1f);
        hitbox.SetActive(false);
    }

    public override void EndExecution()
    {
        base.EndExecution();
        StopAllCoroutines();

        if (landingHitbox != null) landingHitbox.SetActive(false);

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(true);

        Transform hurtbox = transform.Find("Hurtbox_Body");
        if (hurtbox != null) hurtbox.gameObject.SetActive(true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }
}