using UnityEngine;
using System.Collections;

// 3연타 돌진 패턴
public class MidBossPattern6 : BossPatternBase
{
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 5f;

    [SerializeField] private float stampHitboxDuration = 0.2f;
    [SerializeField] private float backKickHitboxDuration = 0.3f;

    private GameObject stampHitbox;
    private GameObject backKickHitbox;

    protected override void Awake()
    {
        base.Awake();
        MidBoss parent = GetComponent<MidBoss>();

        if (parent != null)
        {
            stampHitbox = parent.hitBox_Stamp;
            backKickHitbox = parent.hitBox_BackKick;
        }

        if (stampHitbox != null) stampHitbox.SetActive(false);
        if (backKickHitbox != null) backKickHitbox.SetActive(false);

        cooldown = 0f;
        priority = 5;
        distanceType = DistanceType.Mid;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doTriple");
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            Vector2 moveDir = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
            float elapsed = 0f;
            float moveDuration = moveDistance / moveSpeed;

            while (elapsed < moveDuration)
            {
                rb.linearVelocity = moveDir * moveSpeed;
                elapsed += Time.deltaTime;
                yield return null;
            }
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void AnimEvent_DoubleStamp()
    {
        if (stampHitbox != null)
        {
            StartCoroutine(SmartHitboxRoutine(stampHitbox, stampHitboxDuration));
        }
    }

    public void AnimEvent_TripleBackKickHit()
    {
        if (backKickHitbox != null)
        {
            StartCoroutine(SmartHitboxRoutine(backKickHitbox, backKickHitboxDuration));
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