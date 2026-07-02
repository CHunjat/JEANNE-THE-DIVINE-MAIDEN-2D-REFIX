using UnityEngine;
// =====================================================
// MidBossWebProjectile.cs
// =====================================================
public class MidBossWebProjectile : MonoBehaviour
{
    [Header("유도탄 설정 (기획자 조절)")]
    [SerializeField] private float homingSensitivity = 2f;
    [SerializeField] private float playerYOffset = 1.5f;

    private float speed;
    private float maxRange;
    private float bindDuration;
    private Vector2 startPos;
    private Transform target;
    private Vector2 currentDir;
    private Transform visual;

    public void Initialize(Vector2 dir, float spd, float range, float bind)
    {
        speed = spd;
        maxRange = range;
        bindDuration = bind;
        startPos = transform.position;
        currentDir = dir.normalized;

        // Visual 트랜스폼 캐싱
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) visual = sr.transform;

        // 방향에 따라 Scale X 반전 (Flip X 대신)
        FlipVisual(dir.x < 0f);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) target = playerObj.transform;
    }

    private void Update()
    {
        if (target != null)
        {
            Vector2 targetPos = (Vector2)target.position + new Vector2(0, playerYOffset);
            Vector2 dirToTarget = (targetPos - (Vector2)transform.position).normalized;
            currentDir = Vector2.Lerp(currentDir, dirToTarget, homingSensitivity * Time.deltaTime).normalized;

            // 방향 바뀌면 Flip도 업데이트
            FlipVisual(currentDir.x < 0f);
        }

        transform.position += (Vector3)(currentDir * speed * Time.deltaTime);

        if (Vector2.Distance(startPos, transform.position) >= maxRange)
            Destroy(gameObject);
    }

    private void FlipVisual(bool facingLeft)
    {
        if (visual == null) return;
        Vector3 scale = visual.localScale;
        scale.x = facingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        visual.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            // playerHealth.ApplyBind(bindDuration); ← 병합 후 주석 해제
            Debug.Log($"<color=cyan>[MidBossWebProjectile] 플레이어 타격! 구속 {bindDuration}초</color>");
        }

        Destroy(gameObject);
    }
}