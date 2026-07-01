using UnityEngine;

// =====================================================
// EnemyHitbox.cs
// 몬스터/보스의 히트박스(공격 판정) 컴포넌트임.
// 이 스크립트를 히트박스 오브젝트에 붙이면,
// 해당 오브젝트의 Collider2D가 Player에 닿았을 때 데미지를 줌.
//
// [사용 방법]
// 1. 히트박스 오브젝트에 Collider2D(Is Trigger 체크)를 붙임.
// 2. 이 스크립트를 붙이고 ownerDamage와 damageRatio를 설정함.
// 3. 평소에는 오브젝트를 꺼두고(SetActive(false)) 공격 타이밍에만 켬(SetActive(true)).
//
// [주의사항]
// Project Settings -> Physics 2D -> Layer Collision Matrix에서 
// Enemy 레이어와 Player 레이어가 충돌하도록 체크되어 있는지 반드시 확인할 것!
// =====================================================
public class EnemyHitbox : MonoBehaviour
{
    [Header("데미지 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float ownerDamage = 20f;   // 소유한 몬스터의 기본 공격력임.
    [SerializeField] private float damageRatio = 1.0f;  // 패턴 반영 비율임 (1.0 = 100%).

    [Header("히트 설정")]
    [SerializeField] private bool destroyOnHit = false; // true면 플레이어에 닿는 순간 이 오브젝트 삭제함 (발사체용).
    [SerializeField] private float hitCooldown = 0.5f;  // 같은 대상에게 연속으로 데미지 주는 것을 막는 쿨타임임.

    private float lastHitTime = -999f;  // 마지막 히트 시각을 기록해 두는 변수임.

    private void OnEnable()
    {
        // 오브젝트가 다시 켜질 때 히트 쿨타임 초기화해서 재사용 가능하게 함.
        lastHitTime = -999f;
    }

    // Collider2D의 Is Trigger가 체크되어 있어야 이 함수가 호출됨.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 레이어 체크 (Player 레이어만 통과시킴)
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        // 히트 쿨타임 체크 (너무 빠르게 연속 데미지 주는 버그 방지함).
        if (Time.time < lastHitTime + hitCooldown) return;

        // 플레이어 본체(부모)에 있을지 모르는 PlayerHealth 스크립트 찾기
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            float damage = ownerDamage * damageRatio;
            playerHealth.TakeDamage(damage);
            lastHitTime = Time.time;

            // 데미지 제대로 들어갔는지 확인하는 로그
            Debug.Log($"[{gameObject.name}] 히트박스 발동! 플레이어에게 {damage} 데미지 적용 완료!");
        }
        else
        {
            // PlayerHealth를 못 찾았을 경우 경고 (이게 뜨면 플레이어 오브젝트 구조를 확인해야 함)
            Debug.LogWarning($"[{gameObject.name}] 플레이어와 충돌했으나 PlayerHealth 컴포넌트를 찾지 못함!");
        }

        // 발사체(거미줄, 검기 등)라면 맞는 순간 오브젝트 삭제함.
        if (destroyOnHit)
            Destroy(gameObject);
    }
}