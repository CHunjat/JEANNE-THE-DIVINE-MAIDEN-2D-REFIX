using UnityEngine;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("연결할 컴포넌트")]
    public PlayerStats playerStats;       // 플레이어 스탯 스크립트
    public SpriteRenderer dimmerSprite;   // 암전용 Dimmer_Sprite
    public GameObject gameOverScreen;     // Canvas의 GameOverScreen 오브젝트
    public GameObject inGameScreen;       // Canvas의 InGameScreen 오브젝트
    public Transform respawnPoint;        // ★ 추가: 플레이어가 부활할 위치 (보이지 않는 큐브 등)

    [Header("연출 설정")]
    public float fadeDuration = 2.0f;     // 화면이 완전히 어두워지는 데 걸리는 시간
    [Range(0f, 1f)]
    public float uiTriggerAlpha = 0.7f;   // UI가 켜질 어두움 정도 (70% = 0.7)
    public float respawnDelay = 3.0f;     // ★ 추가: 게임오버 화면이 뜨고 부활까지 대기할 시간 (3초)

    private bool isGameOverTriggered = false;

    private void Start()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
    }

    private void Update()
    {
        if (isGameOverTriggered) return;

        if (playerStats != null && playerStats.currentHp <= 0)
        {
            StartCoroutine(GameOverRoutine());
        }
    }

    private IEnumerator GameOverRoutine()
    {
        isGameOverTriggered = true;

        if (inGameScreen != null)
        {
            inGameScreen.SetActive(false);
        }

        if (dimmerSprite == null) yield break;

        // 보스방 등 체크포인트와 멀어진 곳에서도 암전이 보이도록 위치 이동
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 camPos = mainCamera.transform.position;
            dimmerSprite.transform.position = new Vector3(camPos.x, camPos.y, dimmerSprite.transform.position.z);
        }

        // Dimmer 활성화 및 레이어 순위 격상
        dimmerSprite.gameObject.SetActive(true);
        dimmerSprite.sortingOrder = 10;

        Color c = dimmerSprite.color;
        float startAlpha = c.a;
        float elapsed = 0f;
        bool isUiActivated = false;

        // [1단계] 화면 암전 및 70% 지점에서 게임오버 UI 켜기
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, 1.0f, elapsed / fadeDuration);

            c.a = currentAlpha;
            dimmerSprite.color = c;

            if (!isUiActivated && currentAlpha >= uiTriggerAlpha)
            {
                isUiActivated = true;
                if (gameOverScreen != null)
                {
                    gameOverScreen.SetActive(true);
                }
            }

            yield return null;
        }

        c.a = 1.0f;
        dimmerSprite.color = c;

        if (!isUiActivated && gameOverScreen != null)
            gameOverScreen.SetActive(true);

        // ========================================================
        // ★ [새로 추가된 부활 로직] 게임오버 후 3초 대기 및 재생성
        // ========================================================

        // 1. 지정된 시간(3초) 동안 암전 상태로 대기
        yield return new WaitForSeconds(respawnDelay);

        // 2. 플레이어의 위치를 부활 포인트(큐브) 위치로 순간이동
        if (respawnPoint != null && playerStats != null)
        {
            playerStats.transform.position = respawnPoint.position;
        }

        // 3. 플레이어 상태 및 스탯 초기화
        if (playerStats != null)
        {
            // 체력과 마나를 최대치로 회복
            playerStats.currentHp = playerStats.baseMaxHp;
            playerStats.currentMp = playerStats.baseMaxMp;

            // PlayerStats 스크립트 내부에서 설정된 playerController를 가져옴
            PlayerController playerController = playerStats.GetComponent<PlayerController>();
            if (playerController != null && playerController.StateMachine != null)
            {
                // 사망 상태(DieState)에서 움직일 수 있는 기본 상태(IdleState)로 전환
                playerController.StateMachine.ChangeState(playerController.IdleState);
            }
        }

        // 4. 게임오버 UI를 끄고, 인게임 UI(체력바 등)를 다시 활성화
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
        if (inGameScreen != null) inGameScreen.SetActive(true);

        // 5. 부활했으므로 화면을 다시 부드럽게 밝게 만듬 (Fade Out)
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1.0f, 0f, elapsed / fadeDuration);
            dimmerSprite.color = c;
            yield return null;
        }

        c.a = 0f;
        dimmerSprite.color = c;
        dimmerSprite.gameObject.SetActive(false);

        // 6. 부활 완료 후, 다음 번에 다시 죽었을 때 작동하도록 플래그 리셋
        isGameOverTriggered = false;
    }
}