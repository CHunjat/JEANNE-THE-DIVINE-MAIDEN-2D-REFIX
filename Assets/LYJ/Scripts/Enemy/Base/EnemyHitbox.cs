using UnityEngine;
using System.Collections.Generic;

// 적 타격 판정을 관리하는 스크립트
// 다단 히트 때 물리 엔진 딜레이 없애려고 명단 리셋 방식 적용함
public class EnemyHitbox : MonoBehaviour
{
    // 기획자가 설정할 데미지 수치
    [SerializeField] private float ownerDamage = 20f;
    // 배율 곱해서 최종 데미지 계산할 때 씀
    [SerializeField] private float damageRatio = 1.0f;

    // 한 번 때리고 히트박스 파괴할 건지 결정
    [SerializeField] private bool destroyOnHit = false;

    // 공격하는 본체 오브젝트 위치 저장용
    private Transform ownerTransform;

    // 한 번 때린 플레이어를 기억해두는 명단
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    private void Awake()
    {
        // 스크립트 켜질 때 최상위 부모 오브젝트를 본체로 저장함
        ownerTransform = transform.root;
    }

    private void OnEnable()
    {
        // 히트박스가 켜질 때마다 명단 백지화해서 새로 때릴 수 있게 함
        hitTargets.Clear();
    }

    // 콜라이더를 끄지 않고 다단 히트를 넣기 위해 명단만 강제로 지우는 함수
    public void ResetHitRecord()
    {
        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어 레이어가 아니면 그냥 무시함
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        // 플레이어 컨트롤러 스크립트 찾아옴
        PlayerController playerCtrl = other.GetComponentInParent<PlayerController>();
        if (playerCtrl != null)
        {
            // 이미 명단에 있는 플레이어면 두 번 안 때리고 무시함
            if (hitTargets.Contains(playerCtrl.gameObject)) return;

            // 처음 때린 거니까 명단에 추가함
            hitTargets.Add(playerCtrl.gameObject);

            // 최종 데미지 계산
            float finalDamage = ownerDamage * damageRatio;
            // 타격 방향 계산하려고 본체 위치 가져옴
            Vector2 attackerPos = ownerTransform != null ? (Vector2)ownerTransform.position : (Vector2)transform.position;

            // 플레이어한테 데미지랑 위치 넘겨서 맞게 함
            playerCtrl.EvaluateAttack(finalDamage, attackerPos);

            // 한 번 때리고 부서지는 설정이면 오브젝트 파괴함
            if (destroyOnHit) Destroy(gameObject);
        }
    }
}