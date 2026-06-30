using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossSequenceController : MonoBehaviour
{
    [Header("--- 1. Trigger & Barriers ---")]
    [Tooltip("보라색 구역에 미리 배치해둔 비활성화된 벽/장벽 오브젝트들")]
    [SerializeField] private GameObject[] sideBarriers;

    // ★ 새로 추가된 타일맵 오브젝트 변수
    [Tooltip("보스전 돌입 시 활성화되어 플레이어를 가둘 타일맵 오브젝트 (예: Tilemap_BossRoom)")]
    [SerializeField] private GameObject bossRoomTilemap;

    [Header("--- 2. Camera Settings ---")]
    [Tooltip("메인 카메라")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("화면 중앙에 보스(임시 큐브)가 등장할 위치")]
    [SerializeField] private Transform bossLookTarget;
    [Tooltip("카메라가 보스에게 이동하는 시간(초)")]
    [SerializeField] private float cameraMoveDuration = 2.0f;

    [Header("--- 3. Boss UI ---")]
    [Tooltip("캔버스 내 비활성화되어 있는 보스 UI 부모 오브젝트 (BossUI)")]
    [SerializeField] private GameObject bossUIPanel;
    [Tooltip("보스 체력바 이미지 (BossHp 오브젝트의 Image 컴포넌트)")]
    [SerializeField] private Image bossHealthImage;
    [Tooltip("체력바가 채워지는 시간(초)")]
    [SerializeField] private float healthFillDuration = 1.5f;

    private bool isSequenceStarted = false;

    private void Start()
    {
        // 시작할 때 UI 감추고, 체력바 초기화
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
        if (bossHealthImage != null) bossHealthImage.fillAmount = 0f;

        // ★ 게임 시작 시 타일맵이 켜져있다면 실수 방지를 위해 꺼둡니다.
        if (bossRoomTilemap != null) bossRoomTilemap.SetActive(false);
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
        // -------------------------------------------------------------
        // Step 1. 양옆 장벽 및 타일맵 활성화 (플레이어 가두기)
        // -------------------------------------------------------------
        foreach (GameObject barrier in sideBarriers)
        {
            if (barrier != null) barrier.SetActive(true);
        }

        // ★ 추가된 타일맵 오브젝트 활성화
        if (bossRoomTilemap != null)
        {
            bossRoomTilemap.SetActive(true);
            Debug.Log($"{bossRoomTilemap.name} 타일맵 활성화! 탈출 불가!");
        }

        // Step 2. 임시 큐브(보스) 위치로 카메라 이동
        if (mainCamera != null && bossLookTarget != null)
        {
            Vector3 startCameraPos = mainCamera.transform.position;
            Vector3 targetCameraPos = new Vector3(bossLookTarget.position.x, bossLookTarget.position.y, startCameraPos.z);
            float elapsedTime = 0f;

            while (elapsedTime < cameraMoveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / cameraMoveDuration);
                mainCamera.transform.position = Vector3.Lerp(startCameraPos, targetCameraPos, t);
                yield return null;
            }
            mainCamera.transform.position = targetCameraPos;
        }

        yield return new WaitForSeconds(0.3f);

        // Step 3. 보스 UI 활성화
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(true);
        }

        // Step 4. fillAmount를 0에서 1까지 채우는 연출
        if (bossHealthImage != null)
        {
            bossHealthImage.fillAmount = 0f;
            float elapsedTime = 0f;

            while (elapsedTime < healthFillDuration)
            {
                elapsedTime += Time.deltaTime;
                bossHealthImage.fillAmount = Mathf.Clamp01(elapsedTime / healthFillDuration);
                yield return null;
            }
            bossHealthImage.fillAmount = 1f;
        }

        // Step 5. 연출 종료 및 전투 시작
        Debug.Log("존시나큰거미 등장 완료! 전투를 시작합니다.");
    }
}