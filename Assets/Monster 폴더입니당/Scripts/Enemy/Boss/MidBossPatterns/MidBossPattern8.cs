using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern8.cs (2중 착지 버그 완벽 해결본)
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
    [SerializeField] private float boundDamageMultiplier = 2f;

    [SerializeField] private GameObject webPrefab;

    private GameObject clearingHitbox;
    private GameObject landingHitbox;
    private Animator visualAnimator;
    private bool isExecuting = false;

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
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
    }

    public void AnimEvent_UltClearing()
    {
        ApplyClearing();
        if (clearingHitbox != null)
        {
            clearingHitbox.SetActive(true);
            Invoke(nameof(DeactivateClearing), clearingDuration);
        }
    }

    public void AnimEvent_UltWeb()
    {
        if (webPrefab != null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            bool isFacingLeft = (sr != null && sr.flipX);
            GameObject playerObj = GameObject.FindWithTag("Player");
            Vector2 dir = playerObj != null ? ((Vector2)(playerObj.transform.position - transform.position)).normalized : new Vector2(isFacingLeft ? -1f : 1f, 0f);

            GameObject web = Instantiate(webPrefab, transform.position, Quaternion.identity);
            MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
            if (webScript != null) webScript.Initialize(dir, webSpeed, webRange, bindDuration);
        }

        if (visualAnimator != null) visualAnimator.SetTrigger("doJump");
    }

    public void AnimEvent_JumpAir()
    {
        // 핵심 : 지금 2페이즈 필살기를 쓴 상태가 아니라면 4번 스킬이 뛴 거니까 신호 무시!
        if (!isExecuting) return;

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        StartCoroutine(UltAirRoutine());
    }

    private IEnumerator UltAirRoutine()
    {
        yield return new WaitForSeconds(airTime);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) transform.position = playerObj.transform.position;

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(true);
        if (visualAnimator != null) visualAnimator.SetTrigger("doLand");
    }

    public void AnimEvent_UltLandImpact()
    {
        if (landingHitbox != null)
        {
            landingHitbox.SetActive(true);
            Invoke(nameof(DeactivateLanding), landingHitboxDuration);
        }
        isExecuting = false;
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

    private void DeactivateClearing() { if (clearingHitbox != null) clearingHitbox.SetActive(false); }
    private void DeactivateLanding() { if (landingHitbox != null) landingHitbox.SetActive(false); }
}