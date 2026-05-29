using UnityEngine;

// =====================================================
// MidBossPattern1.cs
// 거미 보스 1페이즈 패턴 1 - 앞발 찍기
//
// [기획 문서 기준]
// - 현재 위치에서 플레이어 위치로 앞다리를 들어올렸다가 내리찍어 공격함
// - 들어올리는 동작과 내리찍는 타이밍 조절이 핵심 (플레이어가 첫타 방어 가능하게)
// - 타이밍 조절은 애니메이션 작업 시 협력 필요
//
// [현재 상태 - 캡슐 테스트용 임시 버전]
// - 애니메이션 없이 공격 범위 안의 플레이어에게 바로 데미지를 줌
// - 애니메이션 연동 후 선딜레이/후딜레이 추가 예정
// =====================================================
public class MidBossPattern1 : BossPatternBase
{
    [Header("앞발 찍기 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float attackRadius = 2f;    // 공격 판정 범위 (반지름)
    [SerializeField] private float damageRatio = 1.0f;   // 보스 기본 공격력 대비 데미지 비율 (1.0 = 100%)
    [SerializeField] private LayerMask playerLayer;      // Player 레이어 (인스펙터에서 반드시 설정할 것)

    private EnemyBase owner;  // 이 패턴을 보유한 보스 (공격력 수치를 가져오기 위해 참조)

    private void Awake()
    {
        cooldown = 3f;  // 임시 쿨타임 (초 단위) - 기획 확정 후 수정할 것
        owner = GetComponentInParent<EnemyBase>();

        if (owner == null)
            Debug.LogWarning("[MidBossPattern1] 부모 오브젝트에서 EnemyBase를 찾지 못함. MidBoss의 자식 오브젝트인지 확인할 것.");
    }

    // 패턴 실행 내용 - BossPatternBase의 Execute()가 호출될 때 실행됨
    protected override void OnExecute()
    {
        Debug.Log("[MidBossPattern1] 앞발 찍기 시전!");

        // 공격 판정 범위 안의 플레이어를 탐색함
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRadius, playerLayer);
        if (hit != null)
        {
            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // 데미지 = 보스 기본 공격력 × 패턴 반영 비율 (문서 기준 계산 방식)
                float baseDamage = 20f;  // 임시값 - owner.attackDamage를 직접 쓰려면 protected → public으로 변경 필요
                float damage = baseDamage * damageRatio;
                playerHealth.TakeDamage(damage);
                Debug.Log($"[MidBossPattern1] 플레이어에게 {damage} 데미지 적용함.");
            }
        }
        else
        {
            Debug.Log("[MidBossPattern1] 공격 범위 안에 플레이어 없음.");
        }
    }

    // 씬 뷰에서 공격 판정 범위를 분홍색으로 보여줌
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}