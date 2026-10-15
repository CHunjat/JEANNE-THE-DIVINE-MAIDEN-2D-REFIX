using UnityEngine;
using System.Collections;

public class MidBossPattern8 : BossPatternBase
{
    [SerializeField] private float clearingRange = 3f;
    [SerializeField] private float knockbackDistance = 10f;
    [SerializeField] private float clearingDuration = 0.5f;

    [SerializeField] private float webSpeed = 75f;
    [SerializeField] private float webRange = 50f;
    [SerializeField] private float bindDuration = 3f;

    [SerializeField] private float airTime = 1.8f;
    [SerializeField] private float landingHitboxDuration = 0.4f;

    [SerializeField] private GameObject webPrefab;
    [SerializeField] private Transform mouthSpawnPoint;

    [SerializeField] private float playerYOffset = -5f;
    [SerializeField] private float lockBeforeLandTime = 0.6f;

    private GameObject clearingHitbox;
    private GameObject landingHitbox;
    private bool hasFiredWeb = false;
    private float startY;

    protected override void Awake()
    {
        base.Awake();

        maxExecutionTime = 8f;

        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null)
        {
            clearingHitbox = parent.hitBox_Clearing;
            landingHitbox = parent.hitBox_Landing;
        }

        if (clearingHitbox != null) clearingHitbox.SetActive(false);
        if (landingHitbox != null) landingHitbox.SetActive(false);

        cooldown = 20f;
        priority = 2;
        distanceType = DistanceType.Any;
        canUseInChase = true;
    }

    protected override void OnExecute()
    {
        hasFiredWeb = false;
        startY = transform.position.y;
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
    }

    public void AnimEvent_UltClearing()
    {
        ApplyClearing();
        if (clearingHitbox != null)
        {
            StartCoroutine(SmartHitboxRoutine(clearingHitbox, clearingDuration));
        }
    }

    public void AnimEvent_SpitWeb()
    {
        if (!isExecuting) return;

        if (hasFiredWeb) return;
        hasFiredWeb = true;

        if (webPrefab != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            bool isFacingLeft = (sr != null && sr.flipX);

            Vector3 spawnPos = mouthSpawnPoint != null ? mouthSpawnPoint.position : transform.position;

            // 레일건 규격에 맞춰 X축 직선 발사로 고정
            Vector2 dir = new Vector2(isFacingLeft ? -1f : 1f, 0f);

            GameObject web = Instantiate(webPrefab, spawnPos, Quaternion.identity);
            MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
            if (webScript != null) webScript.Initialize(dir, webSpeed, webRange, bindDuration);
        }

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

        StartCoroutine(UltAirRoutine());
    }

    private IEnumerator UltAirRoutine()
    {
        float waitBeforeLock = Mathf.Max(0f, airTime - lockBeforeLandTime);
        yield return new WaitForSeconds(waitBeforeLock);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            transform.position = new Vector2(playerObj.transform.position.x, startY);
        }

        yield return new WaitForSeconds(lockBeforeLandTime);

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
            StartCoroutine(SmartHitboxRoutine(landingHitbox, landingHitboxDuration));
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

    public override void EndExecution()
    {
        base.EndExecution();
        StopAllCoroutines();

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(true);

        Transform hurtbox = transform.Find("Hurtbox_Body");
        if (hurtbox != null) hurtbox.gameObject.SetActive(true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    private void ApplyClearing()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null && Vector2.Distance(transform.position, playerObj.transform.position) <= clearingRange)
        {
            float xDiff = playerObj.transform.position.x - transform.position.x;
            Vector2 knockbackDir = xDiff > 0 ? Vector2.right : Vector2.left;
            Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
            if (playerRb != null) playerRb.linearVelocity = knockbackDir * (knockbackDistance / 0.3f);
        }
    }
}