using UnityEngine;
// =====================================================
// MidBossWebProjectile.cs 거미줄(발사체) 관리
// 디버그 로그 추가 : 실제 이동 속도와 방향을 매초 확인하기 위한 임시 로그
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

    private float debugTimer = 0f;
    private float spawnTime = 0f;

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
        ApplyRotation();
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) target = playerObj.transform;

        spawnTime = Time.time;

        // 디버그 로그 : 초기화 시점 상세 정보
        Debug.Log($"<color=orange>[WebDebug] 초기화 완료. speed={speed}, maxRange={maxRange}, currentDir={currentDir}, startPos={startPos}</color>");
        if (playerObj != null)
        {
            Debug.Log($"<color=orange>[WebDebug] 초기화 시점 플레이어 실제 위치(transform.position)={playerObj.transform.position}, playerYOffset={playerYOffset}</color>");
        }
        else
        {
            Debug.Log("<color=red>[WebDebug] 초기화 시점 Player 태그 오브젝트를 못 찾음!</color>");
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

        Vector3 beforeMove = transform.position;
        transform.position += (Vector3)(currentDir * speed * Time.deltaTime);

        // 디버그 로그 : 1초마다 실제 이동 상태 출력
        debugTimer += Time.deltaTime;
        if (debugTimer >= 1f)
        {
            debugTimer = 0f;
            float dist = Vector2.Distance(startPos, transform.position);
            Debug.Log($"<color=orange>[WebDebug] 경과 {Time.time - spawnTime:F1}초 / position={transform.position} / startPos로부터 거리={dist:F2} (destroy 기준 {maxRange}) / timeScale={Time.timeScale} / deltaTime={Time.deltaTime:F4}</color>");
        }

        if (Vector2.Distance(startPos, transform.position) >= maxRange)
        {
            Debug.Log($"<color=orange>[WebDebug] maxRange 도달로 Destroy. 총 경과시간={Time.time - spawnTime:F1}초</color>");
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        // 디버그 로그 : 실제 타격 시점 경과시간
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