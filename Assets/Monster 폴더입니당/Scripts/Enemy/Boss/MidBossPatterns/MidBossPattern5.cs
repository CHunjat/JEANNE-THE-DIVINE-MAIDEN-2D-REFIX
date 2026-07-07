using System.Collections;
using UnityEngine;

// =====================================================
// MidBossPattern5.cs
// 클리어링 - 근거리, 쿨타임 0초(내부 5초), 우선순위 1, 2페이즈 관계없이 적용
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
            clearingHitbox.SetActive(true);
            Invoke(nameof(DeactivateClearing), hitboxActiveDuration);
        }

        if (targetPlayer == null) return;

        Rigidbody2D playerRb = targetPlayer.GetComponent<Rigidbody2D>();
        if (playerRb == null) return;

        float xDiff = targetPlayer.transform.position.x - transform.position.x;
        float dirX;

        // 등 위(정수리)에 올라타 있어도 무조건 거미가 바라보는 쪽이나 바깥쪽으로 방향 결정
        if (Mathf.Abs(xDiff) < 0.05f)
        {
            dirX = transform.localScale.x > 0 ? 1f : -1f;
        }
        else
        {
            dirX = Mathf.Sign(xDiff);
        }

        float knockbackSpeed = knockbackDistance / Mathf.Max(knockbackDuration, 0.01f);

        // [100% 완벽 해결 코루틴 시작!]
        StartCoroutine(PushPlayerXOnlyRoutine(playerRb, dirX, knockbackSpeed, knockbackDuration));

        Debug.Log($"[클리어링 100% 확정 넉백 발동] 방향: {dirX}, 속도: {knockbackSpeed}");
    }

    // 플레이어 컨트롤러를 100% 무시하고 옆(X축)으로만 밀어버리는 궁극의 코루틴
    private IEnumerator PushPlayerXOnlyRoutine(Rigidbody2D playerRb, float dirX, float speedX, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (playerRb == null) yield break;

            // 1. 위쪽 Y축 속도 튀는 걸 완벽 차단! (위로는 1픽셀도 안 뜸)
            float currentY = Mathf.Min(0f, playerRb.linearVelocity.y);
            playerRb.linearVelocity = new Vector2(dirX * speedX, currentY);

            // 2.핵심 : 플레이어가 속도를 0으로 지워도 무조건 물리 좌표를 이동시킴!
            Vector2 targetPos = playerRb.position + new Vector2(dirX * speedX * Time.fixedDeltaTime, 0f);
            playerRb.MovePosition(targetPos);

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private void DeactivateClearing()
    {
        if (clearingHitbox != null) clearingHitbox.SetActive(false);
    }
}