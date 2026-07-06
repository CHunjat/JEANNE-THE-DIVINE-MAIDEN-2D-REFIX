using UnityEngine;
using System.Collections;
// =====================================================
// MidBossPattern4.cs (2중 착지 버그 완벽 해결본)
// =====================================================
public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정")]
    [SerializeField] private float trackTime = 2.7f;
    [SerializeField] private float dropDelay = 0.3f;
    [SerializeField] private float hitboxActiveDuration = 1.0f;
    private GameObject landingHitbox;
    private MidBoss owner;
    private bool isJumping = false;
    private Animator visualAnimator;
    private float originalGroundY;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = GetComponent<MidBoss>();
        if (owner != null) landingHitbox = owner.hitBox_Landing;
        if (landingHitbox != null) landingHitbox.SetActive(false);
        originalGroundY = transform.position.y;
        cooldown = 20f;
        priority = 2;
        distanceType = DistanceType.Any;
        canUseInChase = true;
    }

    protected override void OnExecute()
    {
        if (isJumping) return;
        isJumping = true;
        if (visualAnimator != null) visualAnimator.SetTrigger("doJump");
    }

    public void AnimEvent_JumpAir()
    {
        if (!isJumping) return;
        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(false);

        // 콜라이더 끄기
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        StartCoroutine(TrackAndDropRoutine());
    }

    private IEnumerator TrackAndDropRoutine()
    {
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

        Transform visual = transform.Find("Visual");
        if (visual != null) visual.gameObject.SetActive(true);

        // 콜라이더 켜기
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        if (visualAnimator != null) visualAnimator.SetTrigger("doLand");
        isJumping = false;
    }

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