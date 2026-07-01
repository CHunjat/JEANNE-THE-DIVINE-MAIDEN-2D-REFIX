using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossSequenceController : MonoBehaviour
{
    [Header("--- 1. Trigger & Barriers ---")]
    [Tooltip("보스전 돌입 시 활성화되어 플레이어를 가둘 타일맵 오브젝트 (Tilemap_BossRoom)")]
    [SerializeField] private GameObject bossRoomTilemap;
    [Tooltip("양옆에 배치해둔 추가 벽 오브젝트들이 있다면 여기에 등록 (없으면 비워두셔도 됩니다)")]
    [SerializeField] private GameObject[] sideBarriers;

    [Header("--- 2. Cinemachine Cameras (순서대로 등록) ---")]
    [Tooltip("1번: 플레이어가 들어온 뒤쪽 문을 비출 가상 카메라")]
    [SerializeField] private GameObject entryDoorCamera;

    [Tooltip("2번: 보스방 앞쪽 출구 문을 비출 가상 카메라")]
    [SerializeField] private GameObject exitDoorCamera;

    [Tooltip("3번: 중앙의 보스(임시 큐브)를 비출 가상 카메라")]
    [SerializeField] private GameObject bossCamera;

    [Header("--- 3. Time Settings ---")]
    [Tooltip("카메라가 한 목표물에서 다음 목표물로 이동하는 시간(초)")]
    [SerializeField] private float cameraBlendDuration = 1.0f;

    [Tooltip("문이 닫히는 모습을 유저에게 보여주며 머무르는 시간(초)")]
    [SerializeField] private float cameraHoldDuration = 1.0f;

    [Header("--- 4. Boss UI ---")]
    [Tooltip("보스 UI 부모 오브젝트 (BossUI)")]
    [SerializeField] private GameObject bossUIPanel;
    [Tooltip("보스 체력바 이미지 (BossHp)")]
    [SerializeField] private Image bossHealthImage;
    [Tooltip("체력바가 채워지는 시간(초)")]
    [SerializeField] private float healthFillDuration = 1.5f;

    private bool isSequenceStarted = false;

    private void Start()
    {
        // 게임 시작 시 초기화
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
        if (bossHealthImage != null) bossHealthImage.fillAmount = 0f;
        if (bossRoomTilemap != null) bossRoomTilemap.SetActive(false);

        // 연출용 가상 카메라는 모두 꺼둡니다.
        if (entryDoorCamera != null) entryDoorCamera.SetActive(false);
        if (exitDoorCamera != null) exitDoorCamera.SetActive(false);
        if (bossCamera != null) bossCamera.SetActive(false);
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
        // Step 1. 문 폐쇄 (타일맵/벽 활성화)
        // -------------------------------------------------------------
        if (bossRoomTilemap != null) bossRoomTilemap.SetActive(true);
        foreach (GameObject barrier in sideBarriers)
        {
            if (barrier != null) barrier.SetActive(true);
        }

        // -------------------------------------------------------------
        // Step 2. 뒤쪽 입구 문 보여주기
        // -------------------------------------------------------------
        if (entryDoorCamera != null)
        {
            entryDoorCamera.SetActive(true); // 1번 카메라 ON (플레이어 -> 입구문 이동)
            yield return new WaitForSeconds(cameraBlendDuration + cameraHoldDuration);
        }

        // -------------------------------------------------------------
        // Step 3. 앞쪽 출구 문 보여주기
        // -------------------------------------------------------------
        if (exitDoorCamera != null)
        {
            exitDoorCamera.SetActive(true);   // 2번 카메라 ON
            if (entryDoorCamera != null) entryDoorCamera.SetActive(false); // 1번 카메라 OFF (입구문 -> 출구문 이동)
            yield return new WaitForSeconds(cameraBlendDuration + cameraHoldDuration);
        }

        // -------------------------------------------------------------
        // Step 4. 보스(임시 큐브) 보여주기
        // -------------------------------------------------------------
        if (bossCamera != null)
        {
            bossCamera.SetActive(true);      // 3번 카메라 ON
            if (exitDoorCamera != null) exitDoorCamera.SetActive(false);   // 2번 카메라 OFF (출구문 -> 보스 이동)
            yield return new WaitForSeconds(cameraBlendDuration);
        }

        // -------------------------------------------------------------
        // Step 5. 보스 UI 켜지고 체력바 차오르기
        // -------------------------------------------------------------
        if (bossUIPanel != null) bossUIPanel.SetActive(true);

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

        yield return new WaitForSeconds(0.5f); // 연출 직후 잠깐의 여운

        // -------------------------------------------------------------
        // Step 6. 모든 연출 카메라 종료 (보스 -> 플레이어로 복귀)
        // -------------------------------------------------------------
        if (bossCamera != null) bossCamera.SetActive(false); // 3번 카메라 OFF

        // 플레이어 카메라로 완전히 돌아올 때까지 대기
        yield return new WaitForSeconds(cameraBlendDuration);

        Debug.Log("모든 연출 종료! 전투 시작!");
    }
}