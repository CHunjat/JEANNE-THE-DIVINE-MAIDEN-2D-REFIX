using UnityEngine;
using System.Collections;

// =====================================================
// MidBossPattern6.cs
// 거미 보스 2페이즈 패턴 1 - 강화 앞발 찍기
// (2연찍기 + 조건부 뒷발 찌르기)
//
// [기획 문서 기준]
// - 몬스터 시선 기준 전방으로 이동하며 앞발 찍기 2회 시전
// - 앞발 찍기 2회 후 뒷발 찌르기 사정거리 안에 플레이어가 있으면 뒷발 찌르기 시전
// - 몬스터가 앞으로 이동하기 때문에 캐릭터가 맞춰서 밀려나도록 처리 필요
//   (몬스터 몸체 Collider로 처리 - SetCollisionWithPlayer(true) 상태이므로 자동 적용)
//
// [히트박스 세팅 방법]
// MidBoss_Spider 아래 자식으로 아래 오브젝트 만들기:
// - "Hitbox_Stamp2"    : 앞발 찍기 2연타 판정 (Hitbox_Stamp 재사용 가능)
// - "Hitbox_BackKick"  : 뒷발 찌르기 판정 (보스 뒤쪽에 배치)
// 각각 CircleCollider2D (Is Trigger 체크) + EnemyHitbox 붙이고 꺼두기.
// 이 스크립트의 stampHitbox, backKickHitbox 필드에 드래그해서 넣기.
// =====================================================
public class MidBossPattern6 : BossPatternBase
{
    [Header("2연찍기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float moveDistance = 3f;          // 전방 이동 거리
    [SerializeField] private float moveSpeed = 5f;             // 이동 속도
    [SerializeField] private float stampPreDelay = 0.3f;       // 찍기 선딜레이
    [SerializeField] private float stampHitboxDuration = 0.2f; // 찍기 히트박스 유지 시간
    [SerializeField] private float stampInterval = 0.5f;       // 1타 → 2타 간격
    [SerializeField] private float backKickRange = 3f;         // 뒷발 찌르기 사정거리
    [SerializeField] private float backKickHitboxDuration = 0.3f;

    [Header("히트박스 연결 - 인스펙터에서 드래그해서 넣을 것")]
    [SerializeField] private GameObject stampHitbox;     // 앞발 찍기 히트박스
    [SerializeField] private GameObject backKickHitbox;  // 뒷발 찌르기 히트박스

    private Rigidbody2D rb;
    private Animator visualAnimator;
    private bool isExecuting = false;

    private void Awake()
    {
        cooldown = 6f;  // 임시 쿨타임 - 기획 확정 후 수정할 것
        visualAnimator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (stampHitbox != null) stampHitbox.SetActive(false);
        if (backKickHitbox != null) backKickHitbox.SetActive(false);
    }

    protected override void OnExecute()
    {
        if (isExecuting) return;
        Debug.Log("[MidBossPattern6] 강화 앞발 찍기 시전!");
        if (visualAnimator != null) visualAnimator.Play("DoubleAttack");
        StartCoroutine(StampComboRoutine());
    }

    private IEnumerator StampComboRoutine()
    {
        isExecuting = true;

        // 전방으로 이동
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            Vector2 moveDir = ((Vector2)(playerObj.transform.position - transform.position)).normalized;
            float elapsed = 0f;
            float moveDuration = moveDistance / moveSpeed;

            while (elapsed < moveDuration)
            {
                rb.linearVelocity = moveDir * moveSpeed;
                elapsed += Time.deltaTime;
                yield return null;
            }
            rb.linearVelocity = Vector2.zero;
        }

        // 1번 찍기
        yield return new WaitForSeconds(stampPreDelay);
        ActivateHitbox(stampHitbox);
        yield return new WaitForSeconds(stampHitboxDuration);
        DeactivateHitbox(stampHitbox);

        yield return new WaitForSeconds(stampInterval);

        // 2번 찍기
        ActivateHitbox(stampHitbox);
        yield return new WaitForSeconds(stampHitboxDuration);
        DeactivateHitbox(stampHitbox);

        // 조건부 뒷발 찌르기 - 플레이어가 뒷발 사정거리 안에 있을 때만 시전
        if (playerObj != null)
        {
            float dist = Vector2.Distance(transform.position, playerObj.transform.position);
            if (dist <= backKickRange)
            {
                Debug.Log("[MidBossPattern6] 조건 충족 - 뒷발 찌르기 시전!");
                if (visualAnimator != null) visualAnimator.Play("DoubleAttack");
                ActivateHitbox(backKickHitbox);
                yield return new WaitForSeconds(backKickHitboxDuration);
                DeactivateHitbox(backKickHitbox);
            }
        }

        isExecuting = false;
    }

    private void ActivateHitbox(GameObject hitbox)
    {
        if (hitbox != null) hitbox.SetActive(true);
    }

    private void DeactivateHitbox(GameObject hitbox)
    {
        if (hitbox != null) hitbox.SetActive(false);
    }
}