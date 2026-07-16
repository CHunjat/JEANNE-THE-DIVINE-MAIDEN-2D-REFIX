using UnityEngine;
// =====================================================
// MidBossWebProjectile.cs 거미줄(발사체) 관리
// 디버그 로그 추가 : Visual의 실제 로컬/월드 좌표까지 같이 찍어서 확인
// + gap 원인 진단용 심화 로그 추가 (sprite pivot/rect/PPU, 계층 구조 scale)
// =====================================================
public class MidBossWebProjectile : MonoBehaviour
{
    [Header("유도탄 설정 (기획자 조절)")]
    [Tooltip("켜면 예전처럼 날아가는 내내 플레이어를 계속 쫓아감. 회피 불가능해지니 기본적으로 꺼두는 걸 권장.")]
    [SerializeField] private bool enableHoming = false;
    [SerializeField] private float homingSensitivity = 2f;
    [SerializeField] private float playerYOffset = 1.5f;

    [Header("회전 설정")]
    [Tooltip("이동 방향에 맞춰 스프라이트를 회전시킬지 여부")]
    [SerializeField] private bool rotateToDirection = true;
    [Tooltip("스프라이트 원본이 오른쪽(0도)을 기준으로 그려졌다면 0, 다른 기준이면 보정값 입력")]
    [SerializeField] private float rotationAngleOffset = 0f;

    private float speed;
    private float maxRange;
    private float bindDuration;
    private Vector2 startPos;
    private Transform target;
    private Vector2 currentDir;
    private Transform visual;
    private SpriteRenderer visualRenderer;
    private CircleCollider2D myCollider;

    private float debugTimer = 0f;
    private float spawnTime = 0f;

    public void Initialize(Vector2 dir, float spd, float range, float bind)
    {
        speed = spd;
        maxRange = range;
        bindDuration = bind;
        startPos = transform.position;
        currentDir = dir.normalized;
        visualRenderer = GetComponentInChildren<SpriteRenderer>();
        if (visualRenderer != null) visual = visualRenderer.transform;
        myCollider = GetComponent<CircleCollider2D>();
        FlipVisual(dir.x < 0f);
        ApplyRotation();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) target = playerObj.transform;

        spawnTime = Time.time;

        LogBoundsComparison("초기화 직후");
    }

    // 디버그용 : 그림/콜라이더의 실제 범위뿐 아니라 Visual 자체의 로컬/월드 좌표,
    // 그리고 gap의 근본 원인을 좁히기 위한 sprite/계층 구조 정보까지 같이 찍음
    private void LogBoundsComparison(string tag)
    {
        if (visualRenderer == null || myCollider == null) return;

        Bounds rendererBounds = visualRenderer.bounds;
        Bounds colliderBounds = myCollider.bounds;
        Vector3 gap = colliderBounds.center - rendererBounds.center;

        Debug.Log($"<color=lime>[BoundsCheck-{tag}] transform.position(자기 자신)={transform.position}</color>");
        Debug.Log($"<color=lime>[BoundsCheck-{tag}] visual.localPosition={visual.localPosition}, visual.position(월드)={visual.position}</color>");
        Debug.Log($"<color=lime>[BoundsCheck-{tag}] 그림 중심(Renderer.bounds.center)={rendererBounds.center}, 그림 크기={rendererBounds.size}</color>");
        Debug.Log($"<color=lime>[BoundsCheck-{tag}] 콜라이더 중심(Collider.bounds.center)={colliderBounds.center}, 콜라이더 크기={colliderBounds.size}</color>");
        Debug.Log($"<color=lime>[BoundsCheck-{tag}] 두 중심의 차이(gap)={gap}, 차이 거리={gap.magnitude:F2}</color>");

        // --- 여기부터 신규 진단 로그 ---

        // 1) 콜라이더가 진짜로 Visual과 같은 GameObject/계층에 있는지 확인
        Debug.Log($"<color=yellow>[DIAG-{tag}] collider.gameObject={myCollider.gameObject.name}, " +
                  $"collider.transform.position={myCollider.transform.position}, " +
                  $"collider.transform.lossyScale={myCollider.transform.lossyScale}, " +
                  $"collider.offset={myCollider.offset}, collider.radius={myCollider.radius}</color>");
        Debug.Log($"<color=yellow>[DIAG-{tag}] visual.gameObject={visual.gameObject.name}, " +
                  $"visual.transform.lossyScale={visual.lossyScale}</color>");

        // 2) 현재 프레임 스프라이트의 pivot/rect/PPU 확인 (Tight Mesh / pivot 어긋남 가설 검증용)
        Sprite sprite = visualRenderer.sprite;
        if (sprite != null)
        {
            Debug.Log($"<color=yellow>[DIAG-{tag}] sprite.name={sprite.name}, " +
                      $"sprite.pivot(px)={sprite.pivot}, sprite.rect(px)={sprite.rect}, " +
                      $"sprite.textureRect(px)={sprite.textureRect}, " +
                      $"sprite.pixelsPerUnit={sprite.pixelsPerUnit}, " +
                      $"sprite.bounds(local,unit)={sprite.bounds}</color>");

            // rect의 정중앙과 pivot이 실제로 일치하는지 직접 계산해서 비교
            Vector2 rectCenterPx = new Vector2(sprite.rect.width / 2f, sprite.rect.height / 2f);
            Vector2 pivotDeltaPx = sprite.pivot - rectCenterPx;
            Debug.Log($"<color=yellow>[DIAG-{tag}] pivot이 rect 중앙에서 벗어난 정도(px)={pivotDeltaPx}, " +
                      $"이게 0에 가깝지 않으면 '개별 프레임 pivot 어긋남'이 원인일 가능성 높음</color>");
        }

        // 3) 부모 체인을 타고 올라가며 scale/position 전부 찍기 (중간에 이상한 scale 있는지 확인)
        Transform t = transform;
        while (t != null)
        {
            Debug.Log($"<color=yellow>[DIAG-{tag}] hierarchy: {t.name}, localScale={t.localScale}, localPosition={t.localPosition}</color>");
            t = t.parent;
        }
    }

    private void Update()
    {
        if (enableHoming && target != null)
        {
            Vector2 targetPos = (Vector2)target.position + new Vector2(0, playerYOffset);
            Vector2 dirToTarget = (targetPos - (Vector2)transform.position).normalized;
            currentDir = Vector2.Lerp(currentDir, dirToTarget, homingSensitivity * Time.deltaTime).normalized;
            FlipVisual(currentDir.x < 0f);
            ApplyRotation();
        }

        transform.position += (Vector3)(currentDir * speed * Time.deltaTime);

        debugTimer += Time.deltaTime;
        if (debugTimer >= 0.5f)
        {
            debugTimer = 0f;
            LogBoundsComparison($"경과 {Time.time - spawnTime:F1}초");
        }

        if (Vector2.Distance(startPos, transform.position) >= maxRange)
        {
            Destroy(gameObject);
        }
    }

    private void FlipVisual(bool facingLeft)
    {
        if (visual == null) return;
        Vector3 scale = visual.localScale;
        scale.x = facingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        visual.localScale = scale;
    }

    private void ApplyRotation()
    {
        if (!rotateToDirection || visual == null) return;

        float angle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;

        bool isFlipped = visual.localScale.x < 0f;
        if (isFlipped)
        {
            angle = 180f - angle;
        }

        visual.localRotation = Quaternion.Euler(0f, 0f, angle + rotationAngleOffset);
    }

    // 디버그용 : 콜라이더 실제 판정 범위(마젠타)와 그림 실제 렌더링 범위(초록)를 같이 그려줌
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

        LogBoundsComparison("타격 순간");
        Debug.Log($"<color=magenta>[WebDebug] 플레이어 콜라이더와 접촉! 총 경과시간={Time.time - spawnTime:F1}초, 접촉 시점 position={transform.position}</color>");

        PlayerStats playerStats = other.GetComponentInParent<PlayerStats>();
        PlayerController playerCtrl = other.GetComponentInParent<PlayerController>();
        if (playerStats != null && playerCtrl != null)
        {
            // 중요 : 플레이어 파트에서 ApplyBind 함수 구현이 완료되면 아래 줄의 주석 해제하기
            // playerCtrl.ApplyBind(bindDuration); 
            Debug.Log($"<color=cyan>[MidBossWebProjectile] 플레이어 타격! 구속 {bindDuration}초 (현재는 주석 처리됨)</color>");
        }
        Destroy(gameObject);
    }
}