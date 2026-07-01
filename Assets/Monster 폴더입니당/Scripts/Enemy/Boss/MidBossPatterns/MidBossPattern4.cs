using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern4.cs (20초 쿨타임 기획 자동화 완료)
// =====================================================
public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정 (기획자 조절)")]
    [SerializeField] private float jumpPrepareTime = 0.5f;      // 솟구치기 전 도약 폼 잡는 시간임.
    [SerializeField] private float trackTime = 2.7f;            // 공중에서 유도탄처럼 따라다니는 시간임.
    [SerializeField] private float dropDelay = 0.3f;            // 떨어지기 전 회피 유예 시간임.
    [SerializeField] private float hitboxActiveDuration = 1.0f; // 충격파 유지 시간임.

    private GameObject landingHitbox; // 인스펙터 슬롯 삭제함.
    private MidBoss owner;
    private bool isJumping = false;
    private Animator visualAnimator;
    private float originalGroundY;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = GetComponent<MidBoss>();

        // 부모한테서 랜딩 히트박스 주소 자동 양도받음.
        if (owner != null)
        {
            landingHitbox = owner.hitBox_Landing;
        }

        if (landingHitbox != null) landingHitbox.SetActive(false);
        originalGroundY = transform.position.y;

        // [기획 반영] 점프 공격은 무조건 20초에 한 번씩만 쓰도록 초기 쿨타임 박아버림.
        cooldown = 20f;
    }

    protected override void OnExecute()
    {
        if (isJumping) return;
        if (visualAnimator != null) visualAnimator.SetTrigger("doJump");
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        isJumping = true;
        yield return new WaitForSeconds(jumpPrepareTime);

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        GameObject playerObj = GameObject.FindWithTag("Player");
        float timer = 0f;

        while (timer < trackTime)
        {
            if (playerObj != null)
                transform.position = new Vector2(playerObj.transform.position.x, transform.position.y);
            timer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(dropDelay);

        transform.position = new Vector2(transform.position.x, originalGroundY);

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