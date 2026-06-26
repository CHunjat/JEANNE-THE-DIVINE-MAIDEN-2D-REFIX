using UnityEngine;

// =====================================================
// EnemyBase.cs
// 모든 적(몬스터, 보스)의 공통 기반 클래스
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

        // 자식 오브젝트(Visual)에 있는 Animator를 가져옴
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 자식 오브젝트에서 Animator를 찾을 수 없습니다.");
        }
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning($"[{gameObject.name}] Player 태그를 가진 오브젝트를 찾지 못함.");
    }

    public virtual void TakeDamage(float amount)
    {
        currentHp -= amount;
        Debug.Log($"[{gameObject.name}] 피격! 남은 체력: {currentHp}/{maxHp}");
        if (currentHp <= 0)
            Die();
    }

    protected virtual void Die()
    {
        Debug.Log($"[{gameObject.name}] 사망 처리됨.");
        Destroy(gameObject);
    }

    protected float GetDistanceToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    protected Vector2 GetDirectionToPlayer()
    {
        if (player == null) return Vector2.zero;
        return (player.position - transform.position).normalized;
    }

    // [추가된 핵심 기능] 플레이어 위치에 따라 좌우 반전(Scale X 조절)
    protected void FlipTowardsPlayer()
    {
        if (player == null) return;

        // Scale 대신 Visual의 SpriteRenderer Flip X로 처리
        Transform visual = transform.Find("Visual");
        if (visual == null) return;

        SpriteRenderer sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        sr.flipX = player.position.x < transform.position.x;
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