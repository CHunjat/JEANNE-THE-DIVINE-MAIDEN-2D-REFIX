using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern4.cs (애니메이션 이벤트 적용 완료)
// =====================================================
public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정 (기획자 조절)")]
    [SerializeField] private float trackTime = 2.7f;            // 공중에서 유도탄처럼 따라다니는 시간임.
    [SerializeField] private float dropDelay = 0.3f;            // 떨어지기 전 회피 유예 시간임.
    [SerializeField] private float hitboxActiveDuration = 1.0f; // 충격파 유지 시간임.

    [Header("히트박스 연결")]
    [SerializeField] private GameObject landingHitbox;

    private MidBoss owner;
    private bool isJumping = false;
    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = GetComponent<MidBoss>();
        if (landingHitbox != null) landingHitbox.SetActive(false);
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

        if (playerObj != null)
            transform.position = new Vector2(transform.position.x, playerObj.transform.position.y);

        if (visual != null) visual.gameObject.SetActive(true);
        if (visualAnimator != null) visualAnimator.SetTrigger("doLand");

        isJumping = false;
    }

    // [애니메이션 이벤트 연동용 함수]
    // doLand 모션 중 바닥에 발이 쾅 닿는 프레임에 "AnimEvent_LandImpact" 적어 넣음.
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