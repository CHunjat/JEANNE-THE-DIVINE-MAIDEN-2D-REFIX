using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossSequenceController : MonoBehaviour
{
    [Header("--- 1. Trigger & Barriers ---")]
    [Tooltip("보라색 구역에 미리 배치해둔 비활성화된 벽/장벽 오브젝트들")]
    [SerializeField] private GameObject[] sideBarriers;

    [Tooltip("보스전 돌입 시 활성화되어 플레이어를 가둘 타일맵 오브젝트 (Tilemap_BossRoom)")]
    [SerializeField] private GameObject bossRoomTilemap;

    [Header("--- 2. Boss UI ---")]
    [Tooltip("캔버스 내 비활성화되어 있는 보스 UI 부모 오브젝트 (BossUI)")]
    [SerializeField] private GameObject bossUIPanel;
    [Tooltip("보스 체력바 이미지 (BossHp 오브젝트의 Image 컴포넌트)")]
    [SerializeField] private Image bossHealthImage;
    [Tooltip("체력바가 채워지는 시간(초)")]
    [SerializeField] private float healthFillDuration = 1.5f;

    private bool isSequenceStarted = false;

    private void Start()
    {
        // 초기화
        if (bossUIPanel != null) bossUIPanel.SetActive(false);
        if (bossHealthImage != null) bossHealthImage.fillAmount = 0f;
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

        if (bossRoomTilemap != null)
        {
            bossRoomTilemap.SetActive(true);
        }

        // 벽이 생기고 UI가 켜지기 전 아주 잠깐의 대기 시간 (0.2초)
        yield return new WaitForSeconds(0.2f);

        // -------------------------------------------------------------
        // Step 2. 보스 UI 활성화
        // -------------------------------------------------------------
        if (bossUIPanel != null)
        {
            bossUIPanel.SetActive(true);
        }

        // -------------------------------------------------------------
        // Step 3. 체력바 차오르는 연출
        // -------------------------------------------------------------
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

        Debug.Log("존시나큰거미 등장 완료! 전투를 시작합니다.");
    }
}