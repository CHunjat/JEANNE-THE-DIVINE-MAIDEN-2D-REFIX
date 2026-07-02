using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    // [변경] 이제 두 체력바의 RectTransform을 각각 제어합니다.
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

    private bool isSequenceStarted = false;
    private float maxBarWidth;      // 체력바의 최대 가로 길이
    private float delayTimer = 0f;  // 데미지 잔상 대기 타이머

    private void Start()
    {
        if (bossUIPanel != null) bossUIPanel.SetActive(false);

        // 게임 시작 시 두 바의 최대 길이를 기억하고, 가로 크기를 0으로 초기화 (인트로용)
        if (hpFillRect != null)
        {
            maxBarWidth = hpFillRect.sizeDelta.x;
            hpFillRect.sizeDelta = new Vector2(0f, hpFillRect.sizeDelta.y);
        }
        if (hpDamagedRect != null)
        {
            hpDamagedRect.sizeDelta = new Vector2(0f, hpDamagedRect.sizeDelta.y);
        }

        if (bossRoomTilemap != null) bossRoomTilemap.SetActive(false);
        if (entryDoorCamera != null) entryDoorCamera.SetActive(false);
        if (exitDoorCamera != null) exitDoorCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(false);
    }

    private void Update()
    {
        // 인트로가 끝나고 게임이 진행 중일 때 잔상 추격 로직 작동
        if (isSequenceStarted && hpFillRect != null && hpDamagedRect != null)
        {
            // 노란색 바가 빨간색 바보다 클 때 (즉, 데미지를 입어 차이가 생겼을 때)
            if (hpDamagedRect.sizeDelta.x > hpFillRect.sizeDelta.x)
            {
                if (delayTimer > 0f)
                {
                    // 대기 시간 감소
                    delayTimer -= Time.deltaTime;
                }
                else
                {
                    // 대기 시간이 끝나면 부드럽게(Lerp) 빨간색 바의 크기를 따라갑니다.
                    float newWidth = Mathf.Lerp(hpDamagedRect.sizeDelta.x, hpFillRect.sizeDelta.x, Time.deltaTime * damageFollowSpeed);
                    hpDamagedRect.sizeDelta = new Vector2(newWidth, hpDamagedRect.sizeDelta.y);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSequenceStarted && collision.CompareTag("Player"))
        {
            isSequenceStarted = true;
            StartCoroutine(PlayBossIntroSequence());
        }
    }

    private IEnumerator PlayBossIntroSequence()
    {
        // (Step 1 ~ 4는 기존 카메라 연출과 동일)
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

        // -------------------------------------------------------------
        // Step 5. 보스 UI 켜지고 체력바 차오르기 (두 바가 동시에 차오름)
        // -------------------------------------------------------------
        if (bossUIPanel != null) bossUIPanel.SetActive(true);

        if (hpFillRect != null && hpDamagedRect != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < healthFillDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentWidth = Mathf.Lerp(0f, maxBarWidth, elapsedTime / healthFillDuration);

                // 인트로 때는 빨간바와 노란바를 동시에 채워줍니다.
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

        Debug.Log("모든 연출 종료! 전투 시작!");
    }

    // -------------------------------------------------------------
    // ★ 실제 보스가 데미지를 입을 때 외부(Boss 스크립트 등)에서 호출할 함수
    // -------------------------------------------------------------
    public void UpdateBossHP(float currentHp, float maxHp)
    {
        if (hpFillRect == null) return;

        float hpRatio = Mathf.Clamp01(currentHp / maxHp);
        float targetWidth = maxBarWidth * hpRatio;

        // 1. 실제 체력인 빨간색 바는 즉시 감소시킵니다.
        hpFillRect.sizeDelta = new Vector2(targetWidth, hpFillRect.sizeDelta.y);

        // 2. 노란색 바가 줄어들기 시작할 때까지의 타이머를 리셋합니다.
        delayTimer = damageDelayDuration;

        // 만약 보스가 회복(힐)을 하는 게임이라면 노란바도 즉시 맞춰줍니다.
        if (hpDamagedRect != null && hpDamagedRect.sizeDelta.x < targetWidth)
        {
            hpDamagedRect.sizeDelta = new Vector2(targetWidth, hpDamagedRect.sizeDelta.y);
        }
    }
}