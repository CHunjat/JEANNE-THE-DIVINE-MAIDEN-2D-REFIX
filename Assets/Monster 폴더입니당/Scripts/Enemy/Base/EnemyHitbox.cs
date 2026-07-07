using UnityEngine;
// =====================================================
// EnemyHitbox.cs (PlayerController.EvaluateAttack 연동 버전)
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
    private Transform ownerTransform; // 보스 본체 위치 캐싱용

    private void Awake()
    {
        // 최상단 부모(MidBoss_Spider)의 위치를 기억해 둠
        ownerTransform = transform.root;
    }

    private void OnEnable()
    {
        lastHitTime = -999f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (Time.time < lastHitTime + hitCooldown) return;

        float finalDamage = ownerDamage * damageRatio;

        // PlayerStats.TakeDamage 직접 호출 대신
        // PlayerController.EvaluateAttack으로 넘겨서
        // 가드/패링 판정을 플레이어 쪽 로직에서 처리하도록 위임함.
        PlayerController playerCtrl = other.GetComponentInParent<PlayerController>();
        if (playerCtrl != null)
        {
            Vector2 attackerPos = ownerTransform != null
                ? (Vector2)ownerTransform.position
                : (Vector2)transform.position;

            playerCtrl.EvaluateAttack(finalDamage, attackerPos);

            lastHitTime = Time.time;
            Debug.Log($"[{gameObject.name}] 히트박스 발동 -> PlayerController.EvaluateAttack 호출 완료! (데미지: {finalDamage})");

            if (destroyOnHit) Destroy(gameObject);
            return;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 플레이어와 충돌했으나 PlayerController 컴포넌트를 찾지 못함! " +
                              "(병합이 안 됐거나, Player 프리팹에 PlayerController가 없는 구조일 수 있음)");
        }

        if (destroyOnHit)
            Destroy(gameObject);
    }
}