using UnityEngine;
using System.Collections;

// FinalBossChaseClone.cs
// 데몬 누나 필살 패턴의 분신2 스크립트임.
// 등장 위치에서 본체 기준 5m 위치까지 플레이어를 추격하며 손톱 공격함.
//
// 사용 방법
// FinalBossPattern6에서 chaseClone2Prefab으로 사용함.
// Initialize(playerTransform, bossTransform) 호출 시 자동으로 추격 시작.
//
// 프리팹 구성
// - 이 스크립트 (FinalBossChaseClone)
// - CircleCollider2D (Is Trigger 체크) - 공격 판정용
// - EnemyHitbox 스크립트 - 데미지 처리
// - SpriteRenderer (나중에 스프라이트 받으면 추가)
public class FinalBossChaseClone : MonoBehaviour
{
    [Header("추격 분신 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float chaseSpeed = 4f;
    [SerializeField] private float attackInterval = 0.5f;
    [SerializeField] private float attackHitboxDuration = 0.2f;
    [SerializeField] private float stopDistance = 5f;

    private Transform playerTarget;
    private Transform bossTransform;
    private Collider2D attackCollider;
    private bool isChasing = false;

    private void Awake()
    {
        attackCollider = GetComponent<Collider2D>();
        if (attackCollider != null) attackCollider.enabled = false;
    }

    public void Initialize(Transform player, Transform boss)
    {
        playerTarget = player;
        bossTransform = boss;
        isChasing = true;
        StartCoroutine(ChaseAndAttackRoutine());
    }

    private IEnumerator ChaseAndAttackRoutine()
    {
        while (isChasing)
        {
            if (playerTarget == null || bossTransform == null) break;

            float distToBoss = Vector2.Distance(transform.position, bossTransform.position);
            if (distToBoss <= stopDistance)
            {
                Debug.Log("FinalBossChaseClone 본체 기준 5m 도달. 분신 소멸.");
                Destroy(gameObject);
                yield break;
            }

            Vector2 dir = ((Vector2)(playerTarget.position - transform.position)).normalized;
            transform.position = Vector2.MoveTowards(transform.position, playerTarget.position, chaseSpeed * Time.deltaTime);

            if (attackCollider != null)
            {
                attackCollider.enabled = true;
                yield return new WaitForSeconds(attackHitboxDuration);
                attackCollider.enabled = false;
                yield return new WaitForSeconds(attackInterval - attackHitboxDuration);
            }
            else
            {
                yield return new WaitForSeconds(attackInterval);
            }
        }
    }
}