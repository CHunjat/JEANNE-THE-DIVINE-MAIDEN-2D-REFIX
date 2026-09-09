using UnityEngine;

// 1번 패턴 전용 스크립트임.
// 애니메이션 이벤트 호출 시 지정된 범위에 데미지를 적용함.
public class ElitePattern1 : ElitePatternBase
{
    [Header("1번 공격 설정")]
    [SerializeField] private float hitDamage = 30f;
    [SerializeField] private float hitRadius = 1.5f;
    [SerializeField] private Vector2 offset;
    [SerializeField] private LayerMask playerLayer;

    // 타격 판정 생성 함수임.
    public override void EnableHitbox()
    {
        Vector2 hitPosition = (Vector2)transform.position + offset;

        Collider2D hit = Physics2D.OverlapCircle(hitPosition, hitRadius, playerLayer);

        if (hit != null)
        {
            PlayerStats playerStats = hit.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(hitDamage);
                Debug.Log("Pattern1 적중. 플레이어에게 " + hitDamage + " 데미지 적용함.");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 hitPosition = (Vector2)transform.position + offset;
        Gizmos.DrawWireSphere(hitPosition, hitRadius);
    }
}