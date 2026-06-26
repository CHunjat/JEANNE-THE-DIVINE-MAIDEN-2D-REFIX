using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern4.cs (도약 증발 & 공중부양 버그 수정 완)
// =====================================================
public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정 (기획자 조절)")]
    [SerializeField] private float jumpPrepareTime = 0.5f;      // [추가] 솟구치기 전 도약 폼 잡는 시간
    [SerializeField] private float trackTime = 2.7f;            // 공중에서 유도탄처럼 따라다니는 시간
    [SerializeField] private float dropDelay = 0.3f;            // 떨어지기 전 회피 유예 시간
    [SerializeField] private float hitboxActiveDuration = 1.0f; // 충격파 유지 시간

    [Header("히트박스 연결")]
    [SerializeField] private GameObject landingHitbox;

    private MidBoss owner;
    private bool isJumping = false;
    private Animator visualAnimator;

    // [추가] 원래 바닥의 Y좌표를 기억해 둘 변수
    private float originalGroundY;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = GetComponent<MidBoss>();
        if (landingHitbox != null) landingHitbox.SetActive(false);

        // 게임 시작 시 보스가 서 있는 진짜 바닥 Y 좌표를 저장 (공중부양 방지)
        originalGroundY = transform.position.y;
    }

    protected override void OnExecute()
    {
        if (isJumping) return;

        // 1. 도약 애니메이션 틀기
        if (visualAnimator != null) visualAnimator.SetTrigger("doJump");

        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;

        // 2. 도약 폼을 잡고 이펙트가 터질 수 있게 잠시 대기 (0초만에 숨는 버그 방지)
        yield return new WaitForSeconds(jumpPrepareTime);

        // 3. 하늘로 솟구침 (Visual 끄기)
        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        GameObject playerObj = GameObject.FindWithTag("Player");
        float timer = 0f;

        // 4. 공중 추적 (X좌표만 따라다님)
        while (timer < trackTime)
        {
            if (playerObj != null)
                transform.position = new Vector2(playerObj.transform.position.x, transform.position.y);
            timer += Time.deltaTime;
            yield return null;
        }

        // 5. 추적 종료 후 쾅 찍기 전 딜레이
        yield return new WaitForSeconds(dropDelay);

        // 6. 바닥으로 쾅! (플레이어 Y가 아니라, 처음에 저장해둔 '진짜 바닥 Y'로 복구)
        transform.position = new Vector2(transform.position.x, originalGroundY);

        // 7. 모습 다시 켜고 착지 애니메이션 재생
        if (visual != null) visual.gameObject.SetActive(true);
        if (visualAnimator != null) visualAnimator.SetTrigger("doLand");

        isJumping = false;
    }

    // [애니메이션 이벤트 연동용 함수]
    public void AnimEvent_LandImpact()
    {
        if (landingHitbox != null)
        {
            landingHitbox.SetActive(true);
            Invoke(nameof(DeactivateLanding), hitboxActiveDuration);
        }
    }

    private void DeactivateLanding() { if (landingHitbox != null) landingHitbox.SetActive(false); }
}