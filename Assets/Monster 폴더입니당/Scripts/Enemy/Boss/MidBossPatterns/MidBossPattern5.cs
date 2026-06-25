using UnityEngine;

// =====================================================
// MidBossPattern5.cs (애니메이션 이벤트 적용 완료)
// =====================================================
public class MidBossPattern5 : BossPatternBase
{
    [Header("클리어링 설정 (기획자 조절)")]
    [SerializeField] private float clearingRange = 3f;
    [SerializeField] private float knockbackDistance = 10f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float hitboxActiveDuration = 0.5f;

    [Header("히트박스 연결")]
    [SerializeField] private GameObject clearingHitbox;

    private Animator visualAnimator;
    private GameObject targetPlayer;

    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        if (clearingHitbox != null) clearingHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        targetPlayer = GameObject.FindWithTag("Player");
        if (targetPlayer == null) return;

        float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
        if (dist > clearingRange) return;

        if (visualAnimator != null) visualAnimator.SetTrigger("doClearing");
    }

    // [애니메이션 이벤트 연동용 함수]
    // 보스가 기합을 빡! 뿜어내는 타격 프레임에 "AnimEvent_ApplyClearing" 적어 넣음.
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