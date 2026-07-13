using UnityEngine;
using System.Collections;
// =============================================================
// MidBossPattern4.cs (공중 부양 버그 완벽 해결본)
// =============================================================
public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정")]
    [SerializeField] private float trackTime = 2.7f;
    [SerializeField] private float dropDelay = 0.3f;
    [SerializeField] private float hitboxActiveDuration = 1.0f;
    private GameObject landingHitbox;
    private MidBoss owner;

    private bool isJumping = false;
    public override bool IsBusy => isJumping;

    private Animator visualAnimator;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = GetComponent<MidBoss>();
        if (owner != null) landingHitbox = owner.hitBox_Landing;
        if (landingHitbox != null) landingHitbox.SetActive(false);

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

        Transform hurtbox = transform.Find("Hurtbox_Body");
        if (hurtbox != null) hurtbox.gameObject.SetActive(false);

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

        // 강제 텔레포트 하던 originalGroundY 로직 삭제. 
        // 콜라이더와 중력이 켜지면 알아서 자연스럽게 바닥으로 떨어짐.

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
        if (!isJumping) return;

        if (landingHitbox != null)
        {
            StartCoroutine(ReactivateHitboxRoutine(landingHitbox, hitboxActiveDuration));
        }

        isJumping = false;
    }

    private IEnumerator ReactivateHitboxRoutine(GameObject hitbox, float duration)
    {
        hitbox.SetActive(false);
        yield return null;
        hitbox.SetActive(true);
        yield return new WaitForSeconds(duration);
        hitbox.SetActive(false);
    }
}