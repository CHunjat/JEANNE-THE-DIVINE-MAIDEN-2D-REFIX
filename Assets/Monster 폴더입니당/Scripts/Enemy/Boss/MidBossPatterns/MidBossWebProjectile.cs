using UnityEngine;

// =====================================================
// MidBossWebProjectile.cs
// [수정] 형이 짠 완벽한 각도 제한 로직 유지 + 분리 현상(Collider Offset) 완벽 동기화
// =====================================================
public class MidBossWebProjectile : MonoBehaviour
{
    [Header("유도탄 설정 (기획자 조절)")]
    [SerializeField] private bool enableHoming = false;
    [SerializeField] private float homingSensitivity = 2f;
    [SerializeField] private float playerYOffset = 0f;

    [Header("회전 설정")]
    [SerializeField] private bool rotateToDirection = true;
    [SerializeField] private float rotationAngleOffset = 0f;

    private float speed;
    private float maxRange;
    private float bindDuration;
    private Vector2 startPos;
    private Transform target;
    private Vector2 currentDir;
    private SpriteRenderer visualRenderer;
    private CircleCollider2D myCollider;

    // ★ 분리 현상 방지용 원본 위치 저장 변수
    private Vector2 originalColliderOffset;
    private Vector3 originalVisualLocalPos;

    public void Initialize(Vector2 dir, float spd, float range, float bind)
    {
        speed = spd;
        maxRange = range;
        bindDuration = bind;
        startPos = transform.position;
        currentDir = dir.normalized;

        visualRenderer = GetComponentInChildren<SpriteRenderer>();
        myCollider = GetComponent<CircleCollider2D>();

        // 1. 발사될 때 맨 처음 맞춰둔 완벽한 원본 위치와 오프셋을 기억해둠!
        if (myCollider != null) originalColliderOffset = myCollider.offset;
        if (visualRenderer != null) originalVisualLocalPos = visualRenderer.transform.localPosition;

        ApplyRotation();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    private void Update()
    {
        if (enableHoming && target != null)
        {
            Vector2 targetPos = (Vector2)target.position + new Vector2(0, playerYOffset);
            Vector2 dirToTarget = (targetPos - (Vector2)transform.position).normalized;
            currentDir = Vector2.Lerp(currentDir, dirToTarget, homingSensitivity * Time.deltaTime).normalized;

            ApplyRotation();
        }

        transform.position += (Vector3)(currentDir * speed * Time.deltaTime);

        if (Vector2.Distance(startPos, transform.position) >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    // 좌우는 flipX(거울 반사)가 전담, 회전은 오직 상하 기울기(-90~90도)만 담당
    private void ApplyRotation()
    {
        if (!rotateToDirection) return;

        bool facingLeft = currentDir.x < 0f;

        // 형이 짠 완벽한 각도 제한 로직 (절대 안 건드림!)
        float tiltAngle = Mathf.Atan2(currentDir.y, Mathf.Abs(currentDir.x)) * Mathf.Rad2Deg;
        float rotationZ = facingLeft ? -tiltAngle : tiltAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ + rotationAngleOffset);

        // ★ 핵심 해결: flipX로 그림이 휙 뒤집힐 때, 물리적인 위치(Collider, Transform)도 멱살 잡고 같이 뒤집어줌!
        if (visualRenderer != null)
        {
            visualRenderer.flipX = facingLeft;

            // 그림의 로컬 좌표 동기화
            Vector3 localPos = originalVisualLocalPos;
            localPos.x = facingLeft ? -localPos.x : localPos.x;
            visualRenderer.transform.localPosition = localPos;
        }

        if (myCollider != null)
        {
            // 콜라이더 타격점 좌표 동기화 (이제 절대 안 찢어짐)
            Vector2 offset = originalColliderOffset;
            offset.x = facingLeft ? -offset.x : offset.x;
            myCollider.offset = offset;
        }
    }

    private void OnDrawGizmos()
    {
        if (myCollider != null)
        {
            Vector3 worldCenter = transform.TransformPoint(myCollider.offset);
            float worldRadius = myCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(worldCenter, worldRadius);
        }
        if (visualRenderer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(visualRenderer.bounds.center, visualRenderer.bounds.size);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
        PlayerController playerCtrl = other.GetComponentInParent<PlayerController>();
        if (playerStats != null && playerCtrl != null)
        {
            // playerCtrl.ApplyBind(bindDuration); 
        }
        Destroy(gameObject);
    }
}