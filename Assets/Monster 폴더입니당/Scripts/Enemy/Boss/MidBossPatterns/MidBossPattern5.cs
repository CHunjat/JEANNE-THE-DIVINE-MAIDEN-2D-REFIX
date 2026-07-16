using System.Collections;
using UnityEngine;

// =====================================================
// MidBossPattern5.cs
// 클리어링 - 넉백 거리 제한 및 히트박스 처리 수정
// =====================================================
public class MidBossPattern5 : BossPatternBase
{
    [Header("클리어링 설정 (기획자 조절)")]
    [SerializeField] private float clearingRange = 1.8f;
    [SerializeField] private float knockbackDistance = 10f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float hitboxActiveDuration = 0.5f;

    private GameObject clearingHitbox;
    private Animator visualAnimator;
    private GameObject targetPlayer;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();

        MidBoss parent = GetComponent<MidBoss>();
        if (parent != null) clearingHitbox = parent.hitBox_Clearing;
        if (clearingHitbox != null) clearingHitbox.SetActive(false);

        cooldown = 5f;
        priority = 1;
        distanceType = DistanceType.Close;
    }

    public override bool IsUsable()
    {
        if (!base.IsUsable()) return false;
        targetPlayer = GameObject.FindWithTag("Player");

        if (targetPlayer != null)
        {
            float distX = Mathf.Abs(transform.position.x - targetPlayer.transform.position.x);
            if (distX <= clearingRange) return true;
        }
        return false;
    }

    protected override void OnExecute()
    {
        if (visualAnimator != null) visualAnimator.SetTrigger("doClearing");
    }

    public void AnimEvent_ApplyClearing()
    {
        if (clearingHitbox != null)
        {
            StartCoroutine(ReactivateHitboxRoutine(clearingHitbox, hitboxActiveDuration));
        }

        if (targetPlayer == null) return;

        float xDiff = targetPlayer.transform.position.x - transform.position.x;

        // 클리어링 시점에 플레이어가 이미 범위를 벗어났으면 넉백 생략
        if (Mathf.Abs(xDiff) > clearingRange * 1.5f) return;

        float dirX;

        if (Mathf.Abs(xDiff) < 0.05f)
        {
            dirX = transform.localScale.x > 0 ? 1f : -1f;
        }
        else
        {
            dirX = Mathf.Sign(xDiff);
        }

        float knockbackSpeed = knockbackDistance / Mathf.Max(knockbackDuration, 0.01f);

        StartCoroutine(PushPlayerXOnlyRoutine(targetPlayer.GetComponent<Rigidbody2D>(), dirX, knockbackSpeed, knockbackDuration));
    }

    private IEnumerator PushPlayerXOnlyRoutine(Rigidbody2D playerRb, float dirX, float speedX, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (playerRb == null) yield break;

            float currentY = Mathf.Min(0f, playerRb.linearVelocity.y);
            playerRb.linearVelocity = new Vector2(dirX * speedX, currentY);

            Vector2 targetPos = playerRb.position + new Vector2(dirX * speedX * Time.fixedDeltaTime, 0f);
            playerRb.MovePosition(targetPos);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
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