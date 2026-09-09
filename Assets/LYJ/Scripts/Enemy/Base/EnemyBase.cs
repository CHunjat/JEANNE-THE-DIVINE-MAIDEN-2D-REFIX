using UnityEngine;
// =====================================================
// EnemyBase.cs
// =====================================================
public abstract class EnemyBase : MonoBehaviour
{
    [Header("기본 수치")]
    [SerializeField] protected float maxHp = 100f;
    [SerializeField] protected float currentHp;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float attackDamage = 10f;

    [Header("감지 및 공격 범위")]
    [SerializeField] protected float detectRange = 10f;
    [SerializeField] protected float attackRange = 2.5f;

    protected Transform player;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected Collider2D col;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        currentHp = maxHp;
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 자식 오브젝트에서 Animator를 찾을 수 없습니다.");
        }
    }

    protected virtual void Start()
    {
        FindPlayer();
    }

    protected void FindPlayer()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    // [수정됨] 인수 2개(체력 데미지, 그로기 데미지) 받도록 추가
    public virtual void TakeDamage(float amount, float groggyDamage = 0f)
    {
        currentHp -= amount;
        currentHp = Mathf.Max(currentHp, 0f); // 체력 0 아래로 안 내려가게
        Debug.Log($"[{gameObject.name}] 피격! 남은 체력: {currentHp}/{maxHp}");
        if (currentHp <= 0) Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"[{gameObject.name}] 사망 처리됨.");
        Destroy(gameObject);
    }

    protected float GetDistanceToPlayer()
    {
        if (player == null) FindPlayer();
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    protected Vector2 GetDirectionToPlayer()
    {
        if (player == null) FindPlayer();
        if (player == null) return Vector2.zero;
        return (player.position - transform.position).normalized;
    }

    // [수정됨] 방향이 실제로 바뀔 때만 OnFacingChanged 훅을 호출하도록 변경
    protected virtual void FlipTowardsPlayer()
    {
        if (player == null) FindPlayer();
        if (player == null) return;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            bool shouldFaceLeft = player.position.x < transform.position.x;
            if (sr.flipX != shouldFaceLeft)
            {
                sr.flipX = shouldFaceLeft;
                OnFacingChanged(shouldFaceLeft);
            }
        }
    }

    // [추가됨] 방향이 실제로 바뀔 때 호출되는 훅. 자식 클래스에서 히트박스 위치 조정 등에 사용.
    protected virtual void OnFacingChanged(bool facingLeft) { }

    protected void SetCollisionWithPlayer(bool enable)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = gameObject.layer;
        if (playerLayer == -1 || enemyLayer == -1) return;
        Physics2D.IgnoreLayerCollision(enemyLayer, playerLayer, !enable);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}