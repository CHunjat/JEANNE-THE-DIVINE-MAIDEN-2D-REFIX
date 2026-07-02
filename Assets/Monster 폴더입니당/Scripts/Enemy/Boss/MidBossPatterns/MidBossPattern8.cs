using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern8.cs (필살 패턴 - 클리어링 → 거미줄 → 점프 낙하)
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

    // [기획] 구속 중인 적이 구속을 제시간에 못 풀면 큰 데미지
    // 병합 후 플레이어 담당자와 협의해서 ApplyBind 연동 시 사용할 것
    [SerializeField] private float boundDamageMultiplier = 2f;

    [Header("발사체 프리팹 연결")]
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
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        isExecuting = true;
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
    }

    // 1단계: 클리어링 (플레이어 밀어내기)
    public void AnimEvent_UltClearing()
    {
        ApplyClearing();
        if (clearingHitbox != null)
        {
            clearingHitbox.SetActive(true);
            Invoke(nameof(DeactivateClearing), clearingDuration);
        }
    }

    // 2단계: 거미줄 발사 후 점프 루틴 시작
    public void AnimEvent_UltWeb()
    {
        if (webPrefab == null) return;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        bool isFacingLeft = (sr != null && sr.flipX);

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir;
        if (playerObj != null)
        {
            dir = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
        }
        else
        {
            dir = new Vector2(isFacingLeft ? -1f : 1f, 0f);
        }

        GameObject web = Instantiate(webPrefab, transform.position, Quaternion.identity);
        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
        if (webScript != null) webScript.Initialize(dir, webSpeed, webRange, bindDuration);

        StartCoroutine(UltJumpRoutine());
    }

    // 3단계: 공중에서 플레이어 위치로 이동 후 낙하
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

    // 4단계: 낙하 충격 히트박스
    public void AnimEvent_UltLandImpact()
    {
        if (landingHitbox != null)
        {
            landingHitbox.SetActive(true);
            Invoke(nameof(DeactivateLanding), landingHitboxDuration);
        }

        // [기획] 구속 중인 플레이어한테 boundDamageMultiplier 배율 추가 데미지
        // 병합 후 플레이어 담당자와 협의해서 구현할 것
        Debug.Log($"<color=red>[Pattern8] 낙하 충격! 구속 중이면 {boundDamageMultiplier}배 데미지 (미구현, 병합 후 처리)</color>");

        isExecuting = false;
    }

    private void ApplyClearing()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;
        if (Vector2.Distance(transform.position, playerObj.transform.position) > clearingRange) return;

        float xDiff = playerObj.transform.position.x - transform.position.x;
        Vector2 knockbackDir = Mathf.Abs(xDiff) < 0.01f
            ? ((Vector2)(playerObj.transform.position - transform.position)).normalized
            : (xDiff > 0 ? Vector2.right : Vector2.left);

        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = knockbackDir * (knockbackDistance / 0.3f);
    }

    private void DeactivateClearing() { if (clearingHitbox != null) clearingHitbox.SetActive(false); }
    private void DeactivateLanding() { if (landingHitbox != null) landingHitbox.SetActive(false); }
}