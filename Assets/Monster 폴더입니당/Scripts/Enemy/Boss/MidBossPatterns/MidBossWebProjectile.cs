using UnityEngine;
// =====================================================
// MidBossWebProjectile.cs 거미줄(발사체) 관리
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

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) visual = sr.transform;

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

        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
        PlayerController playerCtrl = other.GetComponentInParent<PlayerController>();

        if (playerStats != null && playerCtrl != null)
        {
            // ★ [TODO] 플레이어 파트에서 ApplyBind 함수 구현이 완료되면 아래 줄의 주석(//)을 해제해 주세요!
            // playerCtrl.ApplyBind(bindDuration); 

            Debug.Log($"<color=cyan>[MidBossWebProjectile] 플레이어 타격! 구속 {bindDuration}초 (현재는 주석 처리됨)</color>");
        }

        Destroy(gameObject);
    }
}