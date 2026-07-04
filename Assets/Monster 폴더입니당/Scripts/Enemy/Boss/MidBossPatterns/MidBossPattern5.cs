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

        // 기획서 반영 (무한루프 방지용 내부 쿨타임 5초 유지)
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

        float xDiff = targetPlayer.transform.position.x - transform.position.x;
        Vector2 knockbackDir = Mathf.Abs(xDiff) < 0.01f
            ? ((Vector2)(targetPlayer.transform.position - transform.position)).normalized
            : (xDiff > 0 ? Vector2.right : Vector2.left);

        if (knockbackDir == Vector2.zero) knockbackDir = Vector2.right;

        Rigidbody2D playerRb = targetPlayer.GetComponent<Rigidbody2D>();
        if (playerRb != null)
            playerRb.linearVelocity = knockbackDir * (knockbackDistance / knockbackDuration);
    }

    private void DeactivateClearing() { if (clearingHitbox != null) clearingHitbox.SetActive(false); }
}