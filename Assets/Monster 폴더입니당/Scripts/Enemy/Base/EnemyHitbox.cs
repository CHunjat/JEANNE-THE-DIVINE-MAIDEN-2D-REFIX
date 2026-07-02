using UnityEngine;
// =====================================================
// EnemyHitbox.cs
// =====================================================
public class EnemyHitbox : MonoBehaviour
{
    [Header("데미지 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float ownerDamage = 20f;
    [SerializeField] private float damageRatio = 1.0f;
    [Header("히트 설정")]
    [SerializeField] private bool destroyOnHit = false;
    [SerializeField] private float hitCooldown = 0.5f;

    private float lastHitTime = -999f;

    private void OnEnable()
    {
        lastHitTime = -999f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (Time.time < lastHitTime + hitCooldown) return;

        // PlayerHealth → PlayerStats로 교체
        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
        if (playerStats != null)
        {
            float damage = ownerDamage * damageRatio;
            playerStats.TakeDamage(damage);
            lastHitTime = Time.time;
            Debug.Log($"[{gameObject.name}] 히트박스 발동! 플레이어에게 {damage} 데미지 적용 완료!");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 플레이어와 충돌했으나 PlayerStats 컴포넌트를 찾지 못함!");
        }

        if (destroyOnHit)
            Destroy(gameObject);
    }
}