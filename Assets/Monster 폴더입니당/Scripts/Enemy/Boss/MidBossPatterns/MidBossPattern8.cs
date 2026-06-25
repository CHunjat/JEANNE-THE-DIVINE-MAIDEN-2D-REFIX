using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern8.cs (애니메이션 이벤트 적용 완료)
// =====================================================
public class MidBossPattern8 : BossPatternBase
{
    [Header("필살 패턴 설정 (기획자 조절)")]
    [SerializeField] private float clearingRange = 3f;
    [SerializeField] private float knockbackDistance = 10f;
    [SerializeField] private float clearingDuration = 0.5f;
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;
    [SerializeField] private float airTime = 2f;
    [SerializeField] private float landingHitboxDuration = 0.4f;
    [SerializeField] private float boundDamageMultiplier = 2f;

    [Header("히트박스 연결")]
    [SerializeField] private GameObject clearingHitbox;
    [SerializeField] private GameObject landingHitbox;
    [SerializeField] private GameObject webPrefab;

    private Animator visualAnimator;
    private bool isExecuting = false;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        if (clearingHitbox != null) clearingHitbox.SetActive(false);
        if (landingHitbox != null) landingHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        isExecuting = true;
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
    }

    // 1단계: 넉백 뿜어내는 모션 프레임에 "AnimEvent_UltClearing" 꽂음.
    public void AnimEvent_UltClearing()
    {
        ApplyClearing();
        if (clearingHitbox != null)
        {
            clearingHitbox.SetActive(true);
            Invoke(nameof(DeactivateClearing), clearingDuration);
        }
    }

    // 2단계: 거미줄 뱉는 모션 프레임에 "AnimEvent_UltWeb" 꽂음.
    public void AnimEvent_UltWeb()
    {
        if (webPrefab == null) return;
        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir = playerObj != null ? ((Vector2)(playerObj.transform.position - transform.position)).normalized : Vector2.right;

        GameObject web = Instantiate(webPrefab, transform.position, Quaternion.identity);
        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
        if (webScript != null) webScript.Initialize(dir, webSpeed, webRange, bindDuration);

        // 거미줄 발사 후 바로 공중 점프 코루틴 진입함.
        StartCoroutine(UltJumpRoutine());
    }

    private IEnumerator UltJumpRoutine()
    {
        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        yield return new WaitForSeconds(airTime);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) transform.position = playerObj.transform.position;

        if (visual != null) visual.gameObject.SetActive(true);
        if (visualAnimator != null) visualAnimator.SetTrigger("doLand");
    }

    // 3단계: doLand 착지 모션 쾅 찍는 프레임에 "AnimEvent_UltLandImpact" 꽂음.
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
        if (playerObj == null || Vector2.Distance(transform.position, playerObj.transform.position) > clearingRange) return;

        float xDiff = playerObj.transform.position.x - transform.position.x;
        Vector2 knockbackDir = Mathf.Abs(xDiff) < 0.01f ? ((Vector2)(playerObj.transform.position - transform.position)).normalized : (xDiff > 0 ? Vector2.right : Vector2.left);

        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null) playerRb.linearVelocity = knockbackDir * (knockbackDistance / 0.3f);
    }

    private void DeactivateClearing() { if (clearingHitbox != null) clearingHitbox.SetActive(false); }
    private void DeactivateLanding() { if (landingHitbox != null) landingHitbox.SetActive(false); }
}