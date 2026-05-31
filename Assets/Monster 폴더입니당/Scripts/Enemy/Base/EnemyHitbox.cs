using UnityEngine;

// =====================================================
// EnemyHitbox.cs
// 몬스터/보스의 히트박스(공격 판정) 컴포넌트임.
// 이 스크립트를 히트박스 오브젝트에 붙이면,
// 해당 오브젝트의 Collider2D가 Player에 닿았을 때 데미지를 줌.
//
// [사용 방법]
// 1. 몬스터 오브젝트 아래 자식 오브젝트로 히트박스 오브젝트를 만듦.
// 2. 히트박스 오브젝트에 Collider2D(Is Trigger 체크)를 붙임.
// 3. 이 스크립트를 붙이고 ownerDamage와 damageRatio를 설정함.
// 4. 평소에는 오브젝트를 꺼두고(SetActive(false))
//    공격 타이밍에만 켬(SetActive(true)).
//
// [이펙트 히트박스]
// 이펙트 오브젝트(거미줄, 검기 등)에도 이 스크립트를 붙여서 사용함.
// 이펙트가 날아가면서 Player에 닿으면 자동으로 데미지가 들어감.
// =====================================================
public class EnemyHitbox : MonoBehaviour
{
    [Header("데미지 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float ownerDamage = 20f;   // 이 히트박스를 소유한 몬스터의 기본 공격력 (임시값)
    [SerializeField] private float damageRatio = 1.0f;  // 패턴 반영 비율 (1.0 = 100%)

    [Header("히트 설정")]
    [SerializeField] private bool destroyOnHit = false;  // true면 플레이어에 닿는 순간 이 오브젝트 삭제 (발사체용)
    [SerializeField] private float hitCooldown = 0.5f;   // 같은 대상에게 연속으로 데미지 주는 것을 막는 쿨타임

    private float lastHitTime = -999f;  // 마지막 히트 시각

    private void OnEnable()
    {
        // 오브젝트가 켜질 때 히트 쿨타임 초기화
        lastHitTime = -999f;
    }

    // Collider2D의 Is Trigger가 체크되어 있어야 이 함수가 호출됨
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player 레이어인지 확인
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        // 히트 쿨타임 체크 (너무 빠르게 연속 데미지 방지)
        if (Time.time < lastHitTime + hitCooldown) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float damage = ownerDamage * damageRatio;
            playerHealth.TakeDamage(damage);
            lastHitTime = Time.time;
            Debug.Log($"[{gameObject.name}] 히트박스 발동! 플레이어에게 {damage} 데미지 적용함.");
        }

        // 발사체(거미줄, 검기 등)는 맞는 순간 삭제
        if (destroyOnHit)
            Destroy(gameObject);
    }
}