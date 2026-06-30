using UnityEngine;

// =====================================================
// MidBossPattern5.cs (무한루프 버그 완벽 수정)
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
        if (parent != null)
        {
            clearingHitbox = parent.hitBox_Clearing;
        }

        if (clearingHitbox != null) clearingHitbox.SetActive(false);

        // [핵심 해결 1] 인스펙터 값 무시하고 강제로 5초 쿨타임 박아버림 (무한루프 방지)
        cooldown = 5f;
    }

    public override bool IsUsable()
    {
        // [핵심 해결 2] 쿨타임이 아직 덜 돌았으면 거리 검사도 안 하고 칼같이 컷!
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
        {
            float knockbackSpeed = knockbackDistance / knockbackDuration;
            playerRb.linearVelocity = knockbackDir * knockbackSpeed;
        }
    }

    private void DeactivateClearing() { if (clearingHitbox != null) clearingHitbox.SetActive(false); }
}