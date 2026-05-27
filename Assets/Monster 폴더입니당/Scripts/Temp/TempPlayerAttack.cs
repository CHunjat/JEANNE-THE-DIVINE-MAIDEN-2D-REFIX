using UnityEngine;

// =====================================================
// TempPlayerAttack.cs
// 임시 플레이어 공격 스크립트임.
//
// [중요] 이 스크립트는 Player 담당자의 코드가 없는 동안만 사용하는 임시 스크립트임.
// 나중에 Player 담당자의 코드와 합칠 때 이 파일을 삭제할 것.
//
// [사용 방법]
// 플레이어 오브젝트에 이 스크립트를 붙이면
// Z키를 누를 때마다 근처 적에게 공격이 들어감.
// Console 창에서 공격 로그를 확인할 수 있음.
// =====================================================
public class TempPlayerAttack : MonoBehaviour
{
    [Header("임시 공격 수치 - 기획 확정 후 수정할 것")]
    [SerializeField] private float attackDamage = 30f;   // 플레이어 공격력 (임시값)
    [SerializeField] private float attackRange = 2f;     // 공격 범위 (임시값)
    [SerializeField] private LayerMask enemyLayer;       // Enemy 레이어 (인스펙터에서 반드시 설정할 것)

    private void Update()
    {
        // Z키를 누르면 공격 실행
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        Debug.Log("[TempPlayerAttack] Z키 입력 - 공격 실행!");

        // 공격 범위 안의 적을 모두 탐색함 (여러 명 동시 공격 가능)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        if (hits.Length == 0)
        {
            Debug.Log("[TempPlayerAttack] 공격 범위 안에 적 없음.");
            return;
        }

        foreach (Collider2D hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                Debug.Log($"[TempPlayerAttack] {hit.gameObject.name}에게 {attackDamage} 데미지 적용함.");
            }
        }
    }

    // 씬 뷰에서 공격 범위를 초록색으로 보여줌
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}