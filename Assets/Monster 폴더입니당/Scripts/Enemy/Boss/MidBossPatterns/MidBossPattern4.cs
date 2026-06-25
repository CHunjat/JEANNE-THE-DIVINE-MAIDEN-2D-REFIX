using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern4.cs (전체 교체본)
// 거미 보스 1페이즈 패턴 4 - 점프 공격
// =====================================================
public class MidBossPattern4 : BossPatternBase
{
    [Header("점프 공격 설정")]
    [SerializeField] private float trackTime = 2.7f;
    [SerializeField] private float dropDelay = 0.3f;
    [SerializeField] private float hitboxActiveDuration = 1.0f;

    [Header("히트박스 연결")]
    [SerializeField] private GameObject landingHitbox;

    private MidBoss owner;
    private bool isJumping = false;
    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 8f;
        visualAnimator = GetComponentInChildren<Animator>();
        owner = GetComponent<MidBoss>();

        if (landingHitbox != null) landingHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (isJumping) return;
        Debug.Log("[MidBossPattern4] 점프 공격 시작! doJump 방아쇠 격발");

        // 점프 트리거 격발
        if (visualAnimator != null)
            visualAnimator.SetTrigger("doJump");

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

        // 착지 트리거 격발
        if (visualAnimator != null)
            visualAnimator.SetTrigger("doLand");

        if (landingHitbox != null)
        {
            landingHitbox.SetActive(true);
            yield return new WaitForSeconds(hitboxActiveDuration);
            landingHitbox.SetActive(false);
        }

        isJumping = false;
    }
}