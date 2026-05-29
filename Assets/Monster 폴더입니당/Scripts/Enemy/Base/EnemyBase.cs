using UnityEngine;

// =====================================================
// EnemyBase.cs
// 모든 적(몬스터, 보스)의 공통 기반 클래스임.
// 체력, 이동속도, 공격력, 감지범위 등 기본 수치를 가지고 있음.
// 이 클래스를 직접 쓰는 게 아니라, NormalMonster나 MidBoss 같은
// 자식 클래스에서 상속받아 사용함.
// =====================================================
public abstract class EnemyBase : MonoBehaviour
{
    [Header("기본 수치 (기획 확정 후 각 몬스터 스크립트에서 수정할 것)")]
    [SerializeField] protected float maxHp;          // 최대 체력
    [SerializeField] protected float currentHp;      // 현재 체력
    [SerializeField] protected float moveSpeed;      // 이동 속도
    [SerializeField] protected float attackDamage;   // 기본 공격력 (패턴별 데미지 = 이 수치 × 패턴 반영 비율)

    [Header("감지 및 공격 범위")]
    [SerializeField] protected float detectRange;    // 플레이어 감지 범위 (이 안에 들어오면 추격 시작)
    [SerializeField] protected float attackRange;    // 공격 범위 (이 안에 들어오면 공격 시작)

    // 내부적으로 자주 쓰는 컴포넌트 참조
    protected Transform player;       // 플레이어 위치 참조
    protected Rigidbody2D rb;         // 물리 이동용
    protected Animator animator;      // 애니메이션 제어용 (추후 연동)
    protected Collider2D col;         // 충돌 판정용

    protected virtual void Awake()
    {
        // 같은 오브젝트에 붙은 컴포넌트를 자동으로 가져옴
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();

        // 시작할 때 현재 체력을 최대 체력으로 초기화
        currentHp = maxHp;
    }

    protected virtual void Start()
    {
        // 씬에서 "Player" 태그를 가진 오브젝트를 자동으로 찾음
        // Player 오브젝트의 Tag가 반드시 "Player"로 설정되어 있어야 함
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"[{gameObject.name}] Player 태그를 가진 오브젝트를 찾지 못함. Player 오브젝트의 Tag를 확인할 것.");
    }

    // 피격 처리 함수 - 외부(플레이어 공격 스크립트 등)에서 호출함
    public virtual void TakeDamage(float amount)
    {
        currentHp -= amount;
        Debug.Log($"[{gameObject.name}] 피격! 남은 체력: {currentHp}/{maxHp}");

        if (currentHp <= 0)
            Die();
    }

    // 사망 처리 - 자식 클래스에서 사망 연출을 추가할 것
    protected virtual void Die()
    {
        Debug.Log($"[{gameObject.name}] 사망 처리됨.");
        Destroy(gameObject);
    }

    // 플레이어까지의 거리를 반환함. 플레이어가 없으면 무한대를 반환함
    protected float GetDistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    // 플레이어 방향의 단위벡터를 반환함 (이동 방향 계산에 사용)
    protected Vector2 GetDirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return (player.position - transform.position).normalized;
    }

    // 플레이어와의 물리 충돌을 켜거나 끔
    // enable = true  → 몬스터와 플레이어가 서로 충돌함 (보스 기본 상태)
    // enable = false → 플레이어가 몬스터를 통과함 (일반/엘리트 몬스터 - 대쉬로 통과 가능)
    protected void SetCollisionWithPlayer(bool enable)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = gameObject.layer;

        if (playerLayer == -1)
            Debug.LogWarning("'Player' 레이어가 존재하지 않음. Layer 설정을 확인할 것.");
        if (enemyLayer == -1)
            Debug.LogWarning("'Enemy' 레이어가 존재하지 않음. Layer 설정을 확인할 것.");

        Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, !enable);
    }

    // 씬 뷰에서 감지 범위(노란색)와 공격 범위(빨간색)를 시각적으로 보여줌
    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}