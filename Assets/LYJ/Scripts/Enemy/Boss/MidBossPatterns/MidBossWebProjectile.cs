using UnityEngine;

// =====================================================
// MidBossWebProjectile.cs
// 레이어 방어 로직 복구 (허공에서 증발하는 버그 완벽 해결)
// 데미지는 프리팹의 EnemyHitbox가 알아서 처리하도록 간섭 안 함
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

    private void ApplyRotation()
    {
        if (!rotateToDirection) return;

        bool facingLeft = currentDir.x < 0f;

        float tiltAngle = Mathf.Atan2(currentDir.y, Mathf.Abs(currentDir.x)) * Mathf.Rad2Deg;
        float rotationZ = facingLeft ? -tiltAngle : tiltAngle;
        transform.rotation = Quaternion.Euler(0f, 0f, rotationZ + rotationAngleOffset);

        if (visualRenderer != null)
        {
            visualRenderer.flipX = facingLeft;

            Vector3 localPos = originalVisualLocalPos;
            localPos.x = facingLeft ? -localPos.x : localPos.x;
            visualRenderer.transform.localPosition = localPos;
        }

        if (myCollider != null)
        {
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
        // 방어 코드 복구
        // 플레이어의 무기나 감지기 같은 껍데기가 아니라, 진짜 'Player' 레이어(몸통)에만 반응함
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
        PlayerController playerCtrl = other.GetComponentInParent<PlayerController>();

        if (playerStats != null && playerCtrl != null)
        {
            // playerCtrl.ApplyBind(bindDuration);
        }

        // 진짜 몸통에 닿았을 때만 파괴 (이때 EnemyHitbox가 정상적으로 데미지를 줌)
        Destroy(gameObject);
    }
}