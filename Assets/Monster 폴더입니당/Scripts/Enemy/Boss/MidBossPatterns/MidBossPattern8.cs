using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern8.cs
// 거미 보스 2페이즈 패턴 3 - 거미줄 뱉고 점프 공격 (필살 패턴)
//
// [기획 문서 기준]
// - 클리어링 → 거미줄 뱉기 → 점프 공격 순서로 패턴 수행
// - 클리어링: 캐릭터 밀어내기
// - 거미줄 뱉기: 캐릭터 구속
// - 점프 공격: 구속 중인 적이 구속을 제시간에 못 풀면 큰 데미지
//   (구속 중인 플레이어에게 데미지 배율 증가)
//
// [히트박스 세팅 방법]
// 기존 히트박스 재사용:
// - Hitbox_Clearing  : 클리어링 (MidBossPattern5의 것 재사용)
// - WebProjectile 프리팹 : 거미줄 (MidBossPattern3의 것 재사용)
// - Hitbox_Landing   : 착지 판정 (MidBossPattern4의 것 재사용)
// =====================================================
public class MidBossPattern8 : BossPatternBase
{
    [Header("필살 패턴 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float clearingRange = 3f;           // 클리어링 발동 범위
    [SerializeField] private float knockbackDistance = 10f;      // 클리어링 넉백 거리
    [SerializeField] private float clearingDuration = 0.5f;      // 클리어링 히트박스 유지 시간
    [SerializeField] private float webPreDelay = 0.6f;           // 거미줄 선딜레이
    [SerializeField] private float webSpeed = 6f;                // 거미줄 속도
    [SerializeField] private float webRange = 12f;               // 거미줄 사거리
    [SerializeField] private float bindDuration = 3f;            // 구속 지속 시간
    [SerializeField] private float airTime = 2f;                 // 점프 공중 대기 시간
    [SerializeField] private float landingHitboxDuration = 0.4f; // 착지 판정 유지 시간
    [SerializeField] private float boundDamageMultiplier = 2f;   // 구속 중 플레이어에게 데미지 배율

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject clearingHitbox;   // 클리어링 히트박스
    [SerializeField] private GameObject landingHitbox;    // 착지 판정 히트박스
    [SerializeField] private GameObject webPrefab;        // 거미줄 프리팹

    private Animator visualAnimator;
    private bool isExecuting = false;

    private void Awake()
    {
        cooldown = 20f;  // 필살 패턴이므로 쿨타임 길게 설정 - 기획 확정 후 수정할 것
        visualAnimator = GetComponentInChildren<Animator>();

        if (clearingHitbox != null) clearingHitbox.SetActive(false);
        if (landingHitbox != null) landingHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        Debug.Log("[MidBossPattern8] 필살 패턴 시전! (클리어링 → 거미줄 → 점프)");
        if (visualAnimator != null) visualAnimator.Play("Attack Spit");
        StartCoroutine(UltimateRoutine());
    }

    private IEnumerator UltimateRoutine()
    {
        isExecuting = true;

        // ① 클리어링 - 플레이어 밀어내기
        Debug.Log("[MidBossPattern8] ① 클리어링!");
        ApplyClearing();
        if (clearingHitbox != null)
        {
            clearingHitbox.SetActive(true);
            yield return new WaitForSeconds(clearingDuration);
            clearingHitbox.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(clearingDuration);
        }

        // ② 거미줄 뱉기 - 구속
        Debug.Log("[MidBossPattern8] ② 거미줄 뱉기!");
        yield return new WaitForSeconds(webPreDelay);
        FireWeb();

        // 거미줄 날아가는 시간 대기
        yield return new WaitForSeconds(webRange / webSpeed);

        // ③ 점프 공격
        Debug.Log("[MidBossPattern8] ③ 점프 공격!");
        yield return StartCoroutine(JumpAttackRoutine());

        isExecuting = false;
    }

    // 클리어링 - 플레이어를 보스 중심 기준 좌/우로 넉백
    private void ApplyClearing()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        float dist = Vector2.Distance(transform.position, playerObj.transform.position);
        if (dist > clearingRange) return;

        float xDiff = playerObj.transform.position.x - transform.position.x;
        Vector2 knockbackDir = Mathf.Abs(xDiff) < 0.01f
            ? ((Vector2)(playerObj.transform.position - transform.position)).normalized
            : (xDiff > 0 ? Vector2.right : Vector2.left);

        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = knockbackDir * (knockbackDistance / 0.3f);
            Debug.Log($"[MidBossPattern8] 클리어링 넉백 적용. 방향: {knockbackDir}");
        }
    }

    // 거미줄 발사
    private void FireWeb()
    {
        if (webPrefab == null)
        {
            Debug.LogWarning("[MidBossPattern8] webPrefab이 연결되지 않음.");
            return;
        }

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir = playerObj != null
            ? ((Vector2)(playerObj.transform.position - transform.position)).normalized
            : Vector2.right;

        GameObject web = Instantiate(webPrefab, transform.position, Quaternion.identity);
        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
        if (webScript != null)
            webScript.Initialize(dir, webSpeed, webRange, bindDuration);
    }

    // 점프 공격 - 구속 중 플레이어에게 데미지 배율 증가
    private IEnumerator JumpAttackRoutine()
    {
        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        yield return new WaitForSeconds(airTime);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            transform.position = playerObj.transform.position;

        if (visual != null) visual.gameObject.SetActive(true);

        // 착지 판정 - 구속 중이면 데미지 배율 증가
        if (landingHitbox != null)
        {
            // EnemyHitbox의 데미지 배율을 구속 여부에 따라 변경
            // 지금은 구속 여부 확인 방법이 없으므로 임시로 로그만 출력
            // Player 담당자가 IsBound 프로퍼티 만들면 여기서 체크할 것
            Debug.Log($"[MidBossPattern8] 착지! 구속 중이면 데미지 {boundDamageMultiplier}배 적용 예정.");

            landingHitbox.SetActive(true);
            yield return new WaitForSeconds(landingHitboxDuration);
            landingHitbox.SetActive(false);
        }
    }
}