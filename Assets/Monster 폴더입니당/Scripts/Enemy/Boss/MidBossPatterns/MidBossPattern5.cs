using UnityEngine;

// =====================================================
// MidBossPattern5.cs
// 거미 보스 1페이즈 패턴 5 - 클리어링
//
// [기획 문서 기준]
// - 시전 조건: 플레이어가 몬스터 크기 내부에 있을 때 시전
// - 위, 아래로 이펙트를 발생시키며 플레이어를 밀어냄
// - 데미지 없음
// - 캐릭터가 피격/가드/패리 시 보스 중심 기준 좌/우 중
//   거리 값이 발생한 방향으로 10m 넉백
// - 플레이어 위치와 보스 중심이 완벽히 일치 시 전방으로 10m 넉백
//
// [넉백 처리]
// 플레이어의 Rigidbody2D에 직접 velocity를 줘서 밀어냄.
// 몬스터가 플레이어를 밀어내는 것이므로 몬스터 쪽에서 처리함.
//
// [히트박스 세팅 방법]
// 1. MidBoss_Spider 아래 자식으로 "Hitbox_Clearing" 오브젝트 만들기
// 2. Hitbox_Clearing에 CircleCollider2D (Is Trigger 체크)
//    반경을 보스 크기에 맞게 설정
//    EnemyHitbox는 붙이지 않음 (데미지 없음, 이 스크립트에서 직접 처리)
// 3. 이 스크립트의 clearingHitbox 필드에 드래그해서 넣기
// =====================================================
public class MidBossPattern5 : BossPatternBase
{
    [Header("클리어링 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float clearingRange = 3f;           // 클리어링 발동 범위 (보스 크기 기준)
    [SerializeField] private float knockbackDistance = 10f;      // 넉백 거리 (문서 기준: 10m)
    [SerializeField] private float knockbackDuration = 0.3f;     // 넉백 지속 시간 (초)
    [SerializeField] private float hitboxActiveDuration = 0.5f;  // 히트박스 유지 시간 (초)

    [Header("히트박스 연결 - 인스펙터에서 Hitbox_Clearing 오브젝트를 드래그해서 넣을 것")]
    [SerializeField] private GameObject clearingHitbox;

    private Animator visualAnimator;

    private void Awake()
    {
        cooldown = 5f;  // 임시 쿨타임 - 기획 확정 후 수정할 것
        visualAnimator = GetComponentInChildren<Animator>();

        if (clearingHitbox != null)
            clearingHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        // 시전 조건 체크: 플레이어가 보스 크기 내부에 있을 때만 시전
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;

        float dist = Vector2.Distance(transform.position, playerObj.transform.position);
        if (dist > clearingRange)
        {
            Debug.Log("[MidBossPattern5] 클리어링 조건 미충족 - 플레이어가 범위 밖에 있음.");
            return;
        }

        Debug.Log("[MidBossPattern5] 클리어링 시전!");
        if (visualAnimator != null) visualAnimator.Play("buff/attack 4");

        // 히트박스 활성화
        if (clearingHitbox != null)
        {
            clearingHitbox.SetActive(true);
            Invoke(nameof(DeactivateHitbox), hitboxActiveDuration);
        }

        // 플레이어 넉백 처리 - 몬스터가 플레이어를 밀어냄
        ApplyClearing(playerObj);
    }

    private void ApplyClearing(GameObject playerObj)
    {
        float xDiff = playerObj.transform.position.x - transform.position.x;
        Vector2 knockbackDir;

        // 완벽히 일치 시 전방으로 넉백, 아니면 좌/우로 넉백
        if (Mathf.Abs(xDiff) < 0.01f)
            knockbackDir = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
        else
            knockbackDir = xDiff > 0 ? Vector2.right : Vector2.left;

        if (knockbackDir == Vector2.zero)
            knockbackDir = Vector2.right;

        // 플레이어 Rigidbody2D에 직접 속도를 줘서 밀어냄
        Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            float knockbackSpeed = knockbackDistance / knockbackDuration;
            playerRb.linearVelocity = knockbackDir * knockbackSpeed;
            Debug.Log($"[MidBossPattern5] 플레이어 넉백 적용. 방향: {knockbackDir}, 거리: {knockbackDistance}m");

            // knockbackDuration 후 플레이어 속도 초기화
            // Player 담당자 코드와 충돌할 수 있으므로 나중에 협의 후 수정할 것
        }
        else
        {
            Debug.LogWarning("[MidBossPattern5] 플레이어에게 Rigidbody2D가 없음. Player 오브젝트를 확인할 것.");
        }
    }

    private void DeactivateHitbox()
    {
        if (clearingHitbox != null)
            clearingHitbox.SetActive(false);
    }
}