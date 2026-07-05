using UnityEngine;

// =====================================================
// EnemyHitbox.cs (로컬 테스트 및 파트너 협업 준비 완벽 버전)
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

        // 메인 프로젝트와 병합 후 주석 해제할 것들
        // 파트너의 메인 프로젝트와 합쳤을 때는 아래 주석을 풀어서 
        // 가드/패링 판정용 EvaluateAttack으로 배달하게 함.
        /*
        PlayerController playerCtrl = other.GetComponentInParent<PlayerController>();
        if (playerCtrl != null)
        {
            Vector2 attackerPos = ownerTransform != null ? (Vector2)ownerTransform.position : (Vector2)transform.position;
            playerCtrl.EvaluateAttack(finalDamage, attackerPos);
            
            lastHitTime = Time.time;
            Debug.Log($"[{gameObject.name}] 직접 타격 -> PlayerController.EvaluateAttack 호출 완료!");
            if (destroyOnHit) Destroy(gameObject);
            return;
        }
        */

        // 현재 로컬 테스트용 백업 파트
        // 파트너 코드가 합쳐지기 전까지는 현재 씬의 PlayerHealth를 찾아 때림.
        // 파트너가 말한 선택적 매개변수 규칙을 존중해서 TakeDamage(finalDamage)만 깔끔하게 호출
        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();

        if (playerStats != null)
        {
            playerStats.TakeDamage(finalDamage); // 뒤에 bool 안 적어도 에러 안 남
            lastHitTime = Time.time;
            Debug.Log($"[{gameObject.name}] 히트박스 발동! 플레이어에게 {finalDamage} 데미지 적용 완료!");
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] 플레이어와 충돌했으나 PlayerHealth 컴포넌트를 찾지 못함!");
        }

        if (destroyOnHit)
            Destroy(gameObject);
    }
}