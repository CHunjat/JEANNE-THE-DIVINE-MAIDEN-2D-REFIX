using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 제어용

public class BossSequenceController : MonoBehaviour
{
    [Header("--- 1. Trigger & Barriers ---")]
    [SerializeField] private GameObject bossRoomTilemap;
    [SerializeField] private GameObject[] sideBarriers;

    [Header("--- 2. Cinemachine Cameras ---")]
    [SerializeField] private GameObject entryDoorCamera;
    [SerializeField] private GameObject exitDoorCamera;
    [SerializeField] private GameObject bossCamera;

    [Header("--- 3. Time Settings ---")]
    [SerializeField] private float cameraBlendDuration = 1.0f;
    [SerializeField] private float cameraHoldDuration = 1.0f;

    [Header("--- 4. Boss UI & Health Bars ---")]
    [Tooltip("보스 UI 부모 오브젝트 (BossUI)")]
    [SerializeField] private GameObject bossUIPanel;

    [Tooltip("실제 빨간색 체력바 (Hp_Fill)")]
    [SerializeField] private RectTransform hpFillRect;

    [Tooltip("데미지 연출용 노란색 체력바 (HP_Dameged)")]
    [SerializeField] private RectTransform hpDamagedRect;

    [Tooltip("인트로 때 체력바가 채워지는 시간(초)")]
    [SerializeField] private float healthFillDuration = 1.5f;

    [Header("--- 5. Damage Effect Settings ---")]
    [Tooltip("데미지를 입은 후 노란바가 줄어들기 시작할 때까지 대기 시간(초)")]
    [SerializeField] private float damageDelayDuration = 0.5f;

    [Tooltip("노란바가 빨간바를 추격하는 속도 (높을수록 빠름)")]
    [SerializeField] private float damageFollowSpeed = 5f;

    [Header("--- 6. External References (수정 없이 연동) ---")]
    [Tooltip("참고할 미드보스 스크립트")]
    [SerializeField] private MidBoss targetBoss;

    [Tooltip("참고할 플레이어 스탯 스크립트")]
    [SerializeField] private PlayerStats playerStats;

    [Tooltip("보스가 입은 데미지를 표시할 텍스트 컴포넌트 (BossDamege)")]
    [SerializeField] private TextMeshProUGUI bossDamageText;

    private bool isSequenceStarted = false;
    private bool isBattleStarted = false;
    private float maxBarWidth;
    private float delayTimer = 0f;
    private float lastBossHp;

    // ★ 조작 제한을 해제할 때 기억해둘 플레이어 컴포넌트 저장용 변수
    private PlayerController cachedPlayerController;

    private void Start()
    {
        if (bossUIPanel != null) bossUIPanel.SetActive(false);

        if (hpFillRect != null)
        {
            maxBarWidth = hpFillRect.sizeDelta.x;
            hpFillRect.sizeDelta = new Vector2(0f, hpFillRect.sizeDelta.y);
        }
        if (hpDamagedRect != null)
        {
            hpDamagedRect.sizeDelta = new Vector2(0f, hpDamagedRect.sizeDelta.y);
        }

        if (bossDamageText != null) bossDamageText.text = "0";

        if (bossRoomTilemap != null) bossRoomTilemap.SetActive(false);
        if (entryDoorCamera != null) entryDoorCamera.SetActive(false);
        if (exitDoorCamera != null) exitDoorCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(false);
    }

    private void Update()
    {
        // 1. 노란색 잔상 바 추격 로직
        if (isSequenceStarted && hpFillRect != null && hpDamagedRect != null)
        {
            if (hpDamagedRect.sizeDelta.x > hpFillRect.sizeDelta.x)
            {
                if (delayTimer > 0f)
                {
                    delayTimer -= Time.deltaTime;
                }
                else
                {
                    float newWidth = Mathf.Lerp(hpDamagedRect.sizeDelta.x, hpFillRect.sizeDelta.x, Time.deltaTime * damageFollowSpeed);
                    hpDamagedRect.sizeDelta = new Vector2(newWidth, hpDamagedRect.sizeDelta.y);
                }
            }
        }

        // 2. 실제 전투 중에 보스 스크립트의 체력을 실시간 모니터링
        if (isBattleStarted && targetBoss != null)
        {
            float currentHp = GetBossCurrentHp();
            float maxHp = GetBossMaxHp();

            // 보스의 체력이 0 이하가 되어 죽었을 때 처리
            if (currentHp <= 0)
            {
                isBattleStarted = false;

                if (bossRoomTilemap != null) bossRoomTilemap.SetActive(false);
                if (sideBarriers != null)
                {
                    foreach (GameObject barrier in sideBarriers)
                    {
                        if (barrier != null) barrier.SetActive(false);
                    }
                }

                if (bossUIPanel != null) bossUIPanel.SetActive(false);

                UpdateBossHP(0f, maxHp);
                Debug.Log("보스 사망 확인: 벽 해제 및 보스 UI 비활성화 완료!");
                return;
            }

            // 체력 변화가 감지되었을 때 (살아있을 때만 작동)
            if (currentHp != lastBossHp)
            {
                if (currentHp < lastBossHp)
                {
                    float singleHitDamage = lastBossHp - currentHp;

                    if (bossDamageText != null)
                    {
                        bossDamageText.text = singleHitDamage.ToString("F0");
                    }
                }

                UpdateBossHP(currentHp, maxHp);
                lastBossHp = currentHp;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSequenceStarted && collision.CompareTag("Player"))
        {
            isSequenceStarted = true;

            // -------------------------------------------------------------
            // ★ [추가] 인트로 시작 시 플레이어 및 보스 행동 고정 (Freeze)
            // -------------------------------------------------------------
            // 1. 플레이어 고정
            GameObject playerObj = collision.gameObject;
            cachedPlayerController = playerObj.GetComponent<PlayerController>();
            if (cachedPlayerController != null)
            {
                cachedPlayerController.enabled = false; // 이동 및 입력 스크립트 해제
            }

            Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero; // 물리적 관성 제거
            }

            // 2. 보스 고정
            if (targetBoss != null)
            {
                targetBoss.enabled = false; // 보스 인공지능(FSM) 작동 중지

                Rigidbody2D bossRb = targetBoss.GetComponent<Rigidbody2D>();
                if (bossRb != null)
                {
                    bossRb.linearVelocity = Vector2.zero; // 보스 관성 제거
                }

                // 보스 애니메이션을 움직이지 않는 평온한 상태(Idle)로 고정
                Animator bossAnim = targetBoss.GetComponent<Animator>();
                if (bossAnim != null)
                {
                    bossAnim.SetBool("isMoving", false);
                    bossAnim.SetBool("isAttacking", false);
                }
            }

            StartCoroutine(PlayBossIntroSequence());
        }
    }

    private IEnumerator PlayBossIntroSequence()
    {
        if (bossRoomTilemap != null) bossRoomTilemap.SetActive(true);
        foreach (GameObject barrier in sideBarriers) if (barrier != null) barrier.SetActive(true);

        if (entryDoorCamera != null)
        {
            entryDoorCamera.SetActive(true);
            yield return new WaitForSeconds(cameraBlendDuration + cameraHoldDuration);
        }
        if (exitDoorCamera != null)
        {
            exitDoorCamera.SetActive(true);
            if (entryDoorCamera != null) entryDoorCamera.SetActive(false);
            yield return new WaitForSeconds(cameraBlendDuration + cameraHoldDuration);
        }
        if (bossCamera != null)
        {
            bossCamera.SetActive(true);
            if (exitDoorCamera != null) exitDoorCamera.SetActive(false);
            yield return new WaitForSeconds(cameraBlendDuration);
        }

        if (bossUIPanel != null) bossUIPanel.SetActive(true);

        if (hpFillRect != null && hpDamagedRect != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < healthFillDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentWidth = Mathf.Lerp(0f, maxBarWidth, elapsedTime / healthFillDuration);

                hpFillRect.sizeDelta = new Vector2(currentWidth, hpFillRect.sizeDelta.y);
                hpDamagedRect.sizeDelta = new Vector2(currentWidth, hpDamagedRect.sizeDelta.y);
                yield return null;
            }
            hpFillRect.sizeDelta = new Vector2(maxBarWidth, hpFillRect.sizeDelta.y);
            hpDamagedRect.sizeDelta = new Vector2(maxBarWidth, hpDamagedRect.sizeDelta.y);
        }

        yield return new WaitForSeconds(0.5f);

        if (bossCamera != null) bossCamera.SetActive(false);
        yield return new WaitForSeconds(cameraBlendDuration);

        // -------------------------------------------------------------
        // ★ [추가] 인트로 연출이 완전히 끝나면 조작 및 AI 제한 해제 (Unfreeze)
        // -------------------------------------------------------------
        if (cachedPlayerController != null)
        {
            cachedPlayerController.enabled = true; // 플레이어 다시 이동 가능
        }

        if (targetBoss != null)
        {
            targetBoss.enabled = true; // 보스 AI 다시 작동 시작
            lastBossHp = GetBossCurrentHp();
        }

        isBattleStarted = true;
        Debug.Log("모든 연출 종료! 데이터 실시간 동기화 및 전투 시작!");
    }

    public void UpdateBossHP(float currentHp, float maxHp)
    {
        if (hpFillRect == null) return;

        float hpRatio = Mathf.Clamp01(currentHp / maxHp);
        float targetWidth = maxBarWidth * hpRatio;

        hpFillRect.sizeDelta = new Vector2(targetWidth, hpFillRect.sizeDelta.y);
        delayTimer = damageDelayDuration;

        if (hpDamagedRect != null && hpDamagedRect.sizeDelta.x < targetWidth)
        {
            hpDamagedRect.sizeDelta = new Vector2(targetWidth, hpDamagedRect.sizeDelta.y);
        }
    }

    private float GetBossCurrentHp()
    {
        if (targetBoss == null) return 0f;
        var field = typeof(EnemyFSM).GetField("currentHp", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) return (float)field.GetValue(targetBoss);
        return 0f;
    }

    private float GetBossMaxHp()
    {
        if (targetBoss == null) return 100f;
        var field = typeof(EnemyFSM).GetField("maxHp", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) return (float)field.GetValue(targetBoss);
        return 100f;
    }
}