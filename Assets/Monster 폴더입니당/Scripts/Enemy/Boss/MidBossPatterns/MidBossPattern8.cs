using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern8.cs
// =====================================================
public class MidBossPattern8 : BossPatternBase
{
    [Header("필살 패턴 설정")]
    [SerializeField] private float clearingRange = 3f;
    [SerializeField] private float knockbackDistance = 10f;
    [SerializeField] private float clearingDuration = 0.5f;
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;
    [SerializeField] private float airTime = 1.8f;
    [SerializeField] private float landingHitboxDuration = 0.4f;

    [SerializeField] private GameObject webPrefab;

    [Header("거미줄 발사 위치")]
    [SerializeField] private Transform mouthSpawnPoint;

    [Header("안전장치")]
    [SerializeField] private float maxExecutionTime = 8f;

    private GameObject clearingHitbox;
    private GameObject landingHitbox;
    private Animator visualAnimator;
    private bool isExecuting = false;
    private bool hasFiredWeb = false;
    private Coroutine failsafeCoroutine;

    public override bool IsBusy => isExecuting;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
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
        if (isExecuting) return;
        isExecuting = true;
        hasFiredWeb = false;

        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");

        if (failsafeCoroutine != null) StopCoroutine(failsafeCoroutine);
        failsafeCoroutine = StartCoroutine(FailsafeRoutine());
    }

    public void AnimEvent_UltClearing()
    {
        ApplyClearing();
        if (clearingHitbox != null)
        {
            StartCoroutine(ReactivateHitboxRoutine(clearingHitbox, clearingDuration));
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
            GameObject playerObj = GameObject.FindWithTag("Player");

            Vector3 spawnPos = mouthSpawnPoint != null ? mouthSpawnPoint.position : transform.position;

            Vector2 dir = playerObj != null
                ? ((Vector2)(playerObj.transform.position - spawnPos)).normalized
                : new Vector2(isFacingLeft ? -1f : 1f, 0f);

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
        yield return new WaitForSeconds(airTime);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) transform.position = playerObj.transform.position;

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(true);

        Transform hurtbox = transform.Find("Hurtbox_Body");
        if (hurtbox != null) hurtbox.gameObject.SetActive(true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        if (visualAnimator != null) visualAnimator.SetTrigger("doLand");

        // [핵심] 유니티가 0.00초 핀을 씹어먹을 것에 대비해 강제 종료 예약 (착지 + 후딜레이 포함 1.5초 뒤)
        Invoke(nameof(EndExecution), landingHitboxDuration + 1.0f);
    }

    public void AnimEvent_LandImpact()
    {
        if (!isExecuting) return;

        if (landingHitbox != null)
        {
            StartCoroutine(ReactivateHitboxRoutine(landingHitbox, landingHitboxDuration));
        }
    }

    // 물리엔진 판정 리셋을 위한 1프레임 대기 처리 (무적시간 씹힘 방지)
    private IEnumerator ReactivateHitboxRoutine(GameObject hitbox, float duration)
    {
        hitbox.SetActive(false);
        yield return null;
        hitbox.SetActive(true);

        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }

    private void EndExecution()
    {
        isExecuting = false;
        if (failsafeCoroutine != null)
        {
            StopCoroutine(failsafeCoroutine);
            failsafeCoroutine = null;
        }
    }

    private IEnumerator FailsafeRoutine()
    {
        yield return new WaitForSeconds(maxExecutionTime);
        if (isExecuting)
        {
            isExecuting = false;
        }
        failsafeCoroutine = null;
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