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

    protected void FlipTowardsPlayer()
    {
        if (player == null) FindPlayer();
        if (player == null) return;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = player.position.x < transform.position.x;
        }
    }

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