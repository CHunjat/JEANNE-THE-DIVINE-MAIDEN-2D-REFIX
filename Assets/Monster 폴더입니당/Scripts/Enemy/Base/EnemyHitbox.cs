using UnityEngine;
using System.Collections.Generic; // HashSet을 쓰기 위해 꼭 필요함!
// =====================================================
// EnemyHitbox.cs (중복 타격 완벽 방지 버전)
// =====================================================
public class EnemyHitbox : MonoBehaviour
{
    [Header("데미지 설정 - 기획 확정 후 수정할 것")]
    [SerializeField] private float ownerDamage = 20f;
    [SerializeField] private float damageRatio = 1.0f;

    [Header("히트 설정")]
    [SerializeField] private bool destroyOnHit = false;

    private Transform ownerTransform;

    // 핵심 : 한 번 공격할 때(히트박스가 켜져있는 동안) 때린 놈을 기억하는 살생부
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    private void Awake()
    {
        ownerTransform = transform.root;
    }

    private void OnEnable()
    {
        // 히트박스가 새로 켜질 때마다 백지화! (다음 공격 땐 다시 때릴 수 있게)
        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        PlayerController playerCtrl = other.GetComponentInParent<PlayerController>();
        if (playerCtrl != null)
        {
            // 이미 처리된 놈이면? 쿨하게 무시! (콜라이더가 2개 겹쳐있어도 방어됨)
            if (hitTargets.Contains(playerCtrl.gameObject)) return;

            // 처음 때린 거라면 살생부에 이름 등록!
            hitTargets.Add(playerCtrl.gameObject);

            float finalDamage = ownerDamage * damageRatio;
            Vector2 attackerPos = ownerTransform != null ? (Vector2)ownerTransform.position : (Vector2)transform.position;

            playerCtrl.EvaluateAttack(finalDamage, attackerPos);
            Debug.Log($"[{gameObject.name}] 히트박스 발동 -> 데미지 {finalDamage} 딱 한 번만 적용!");

            if (destroyOnHit) Destroy(gameObject);
        }
    }
}