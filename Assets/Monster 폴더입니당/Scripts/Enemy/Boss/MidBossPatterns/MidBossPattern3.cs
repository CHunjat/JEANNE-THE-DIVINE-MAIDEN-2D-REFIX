using UnityEngine;
// =====================================================
// MidBossPattern3.cs 거미줄 뱉기 (그로기 굳힘 현상 방지용 강제 초기화 추가)
// [디버그 로그 추가 버전 - 원인 파악용]
// =====================================================
public class MidBossPattern3 : BossPatternBase
{
    [Header("거미줄 뱉기 설정")]
    [SerializeField] private float webSpeed = 6f;
    [SerializeField] private float webRange = 12f;
    [SerializeField] private float bindDuration = 3f;
    [SerializeField] private float playerYOffset = 1.5f;
    [SerializeField] private GameObject webPrefab;
    [SerializeField] private Transform webSpawnPoint;
    [Header("발사 위치 높이 강제 조절 (이미지 쳐짐 해결용)")]
    [SerializeField] private float manualYOffset = 0f;
    private Transform owner;
    private Animator visualAnimator;
    private bool isSpitting = false;
    private bool hasFiredThisTurn = false;
    public override bool IsBusy => isSpitting;
    private void Awake()
    {
        visualAnimator = GetComponentInChildren<Animator>();
        owner = transform;
        cooldown = 6f;
        priority = 3;
        distanceType = DistanceType.Far;
        canUseInChase = true;
    }
    protected override void OnExecute()
    {
        if (isSpitting) return;
        isSpitting = true;
        hasFiredThisTurn = false;
        if (visualAnimator != null) visualAnimator.SetTrigger("doSpit");
        Invoke(nameof(UnlockSpitting), 2.0f);
    }
    private void UnlockSpitting() { isSpitting = false; }
    public void AnimEvent_SpitWeb()
    {
        if (!isSpitting || hasFiredThisTurn) return;
        hasFiredThisTurn = true;
        if (webPrefab == null) return;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        bool isFacingLeft = (sr != null && sr.flipX);
        Vector3 spawnPos = owner.position;

        // ===== [디버그 로그 1] webSpawnPoint 자체의 상태 확인 =====
        if (webSpawnPoint != null)
        {
            Debug.Log($"<color=yellow>[Pattern3 DEBUG] webSpawnPoint 이름: {webSpawnPoint.name}</color>");
            Debug.Log($"<color=yellow>[Pattern3 DEBUG] webSpawnPoint.position (월드): {webSpawnPoint.position}</color>");
            Debug.Log($"<color=yellow>[Pattern3 DEBUG] webSpawnPoint.localPosition (로컬): {webSpawnPoint.localPosition}</color>");
            Debug.Log($"<color=yellow>[Pattern3 DEBUG] webSpawnPoint.lossyScale (월드 스케일): {webSpawnPoint.lossyScale}</color>");

            if (webSpawnPoint.parent != null)
            {
                Debug.Log($"<color=yellow>[Pattern3 DEBUG] webSpawnPoint의 부모: {webSpawnPoint.parent.name}, 부모 position: {webSpawnPoint.parent.position}, 부모 lossyScale: {webSpawnPoint.parent.lossyScale}</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>[Pattern3 DEBUG] webSpawnPoint의 부모: 없음(루트)</color>");
            }
        }
        else
        {
            Debug.Log($"<color=red>[Pattern3 DEBUG] webSpawnPoint가 None입니다! (연결 안 됨)</color>");
        }

        Debug.Log($"<color=yellow>[Pattern3 DEBUG] owner(스파이더 루트).position: {owner.position}</color>");
        // ===========================================================

        if (webSpawnPoint != null)
        {
            float offsetX = Mathf.Abs(webSpawnPoint.localPosition.x);
            float finalX = isFacingLeft ? (owner.position.x - offsetX) : (owner.position.x + offsetX);
            float finalY = webSpawnPoint.position.y + manualYOffset;
            spawnPos = new Vector3(finalX, finalY, owner.position.z);
        }

        // ===== [디버그 로그 2] 최종 계산된 spawnPos =====
        Debug.Log($"<color=cyan>[Pattern3 DEBUG] 최종 spawnPos (거미줄이 실제로 생성될 위치): {spawnPos}</color>");
        // ==================================================

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector2 dir;
        if (playerObj != null)
        {
            Vector3 targetPos = playerObj.transform.position + new Vector3(0, playerYOffset, 0);
            dir = ((Vector2)(targetPos - spawnPos)).normalized;
        }
        else
        {
            dir = new Vector2(isFacingLeft ? -1f : 1f, 0f);
        }
        GameObject web = Instantiate(webPrefab, spawnPos, Quaternion.identity);

        // ===== [디버그 로그 3] 실제로 생성된 오브젝트의 위치 (Instantiate 직후) =====
        Debug.Log($"<color=magenta>[Pattern3 DEBUG] Instantiate 직후 web.transform.position: {web.transform.position}</color>");
        // ================================================================

        MidBossWebProjectile webScript = web.GetComponent<MidBossWebProjectile>();
        if (webScript != null)
        {
            webScript.Initialize(dir, webSpeed, webRange, bindDuration);
        }
    }
    // 그로기 진입 시 MidBoss 메인 스크립트에서 호출되는 초기화 함수
    public void EndExecution()
    {
        isSpitting = false;
        hasFiredThisTurn = false;
        CancelInvoke(nameof(UnlockSpitting));
    }
}