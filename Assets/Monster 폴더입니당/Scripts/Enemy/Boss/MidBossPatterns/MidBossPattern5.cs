using UnityEngine;

// =====================================================
// MidBossPattern5.cs (안티 캠핑 몸체 겹침 방지 로직 장착 완)
// =====================================================
public class MidBossPattern5 : BossPatternBase
{
    [Header("클리어링 설정 (기획자 조절)")]
    [SerializeField] private float clearingRange = 3f;
    [SerializeField] private float knockbackDistance = 10f;
    [SerializeField] private float knockbackDuration = 0.3f;
    [SerializeField] private float hitboxActiveDuration = 0.5f;

    private GameObject clearingHitbox; // 인스펙터 슬롯 삭제함.
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
    }

    // [기획 반영] 플레이어가 거미 몸체 중간에 비비적거리면 패턴 쿨타임 무시하고 무조건 발동되게 가로챔!
    public override bool IsUsable()
    {
        targetPlayer = GameObject.FindWithTag("Player");
        if (targetPlayer != null)
        {
            float dist = Vector2.Distance(transform.position, targetPlayer.transform.position);
            // 거미 몸 한가운데(거리 1.8 이내)로 파고들었다면? 쿨타임 검사 패스하고 무조건 참(true) 리턴함.
            if (dist <= 1.8f) return true;
        }

        return base.IsUsable(); // 멀리 있을 땐 정상적으로 쿨타임 체크함.
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