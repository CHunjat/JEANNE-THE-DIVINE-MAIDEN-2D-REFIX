using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // ★ 씬 관리를 위해 필수 추가

public class GameOverManager : MonoBehaviour
{
    [Header("연결할 컴포넌트")]
    public PlayerStats playerStats;       // 플레이어 스탯 스크립트
    public SpriteRenderer dimmerSprite;   // 암전용 Dimmer_Sprite
    public GameObject gameOverScreen;     // Canvas의 GameOverScreen 오브젝트
    public GameObject inGameScreen;       // Canvas의 InGameScreen 오브젝트
    public Transform respawnPoint;        // 플레이어가 부활할 위치 (보이지 않는 큐브 등)

    [Header("연출 설정")]
    public float fadeDuration = 2.0f;     // 화면이 완전히 어두워지는 데 걸리는 시간
    [Range(0f, 1f)]
    public float uiTriggerAlpha = 0.7f;   // UI가 켜질 어두움 정도 (70% = 0.7)
    public float respawnDelay = 3.0f;     // 게임오버 화면이 뜨고 부활까지 대기할 시간 (3초)

    // ★ [정적 변수] 씬이 재로드되어도 파괴되지 않고 유지되는 메모리 영역입니다.
    public static Vector3? lastRespawnPosition = null;
    public static bool skipMainMenu = false;
    public static bool shouldFadeIn = false;

    private bool isGameOverTriggered = false;

    private void Start()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        // ========================================================
        // ★ [씬 재로드 시 처리]
        // ========================================================
        // 1. 만약 이전에 저장해둔 부활 포인트 좌표가 있다면, 플레이어를 그리로 순간이동시킵니다.
        if (lastRespawnPosition.HasValue && playerStats != null)
        {
            playerStats.transform.position = lastRespawnPosition.Value;
        }

        // 2. 부활 또는 새 게임 버튼으로 진입한 경우, 부드럽게 화면을 밝혀줍니다.
        if (shouldFadeIn && dimmerSprite != null)
        {
            shouldFadeIn = false; // 플래그 원상 복구
            StartCoroutine(FadeInEffectRoutine());
        }
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

        // 카메라 위치 추적 암전 (보스방 위치 고려)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 camPos = mainCamera.transform.position;
            dimmerSprite.transform.position = new Vector3(camPos.x, camPos.y, dimmerSprite.transform.position.z);
        }

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
        // ★ [씬 초기화 및 부활 로직] 3초 대기 후 씬 재로드
        // ========================================================

        // 1. 지정된 시간(3초) 동안 암전 상태로 대기
        yield return new WaitForSeconds(respawnDelay);

        // 2. 부활할 위치를 저장하고 씬 전환 시 메인메뉴 스킵 플래그를 켭니다.
        if (respawnPoint != null)
        {
            lastRespawnPosition = respawnPoint.position;
        }
        skipMainMenu = true;
        shouldFadeIn = true; // 재로드 후 어두운 화면에서 밝아지는 연출 작동용

        // 3. 현재 활성화된 씬을 통째로 재로드하여 보스와 맵 전체를 '순정 상태'로 리셋합니다.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 완전히 어두운 상태에서 원래 밝기로 부드럽게 되돌리는 연출용 코루틴
    private IEnumerator FadeInEffectRoutine()
    {
        dimmerSprite.gameObject.SetActive(true);
        dimmerSprite.sortingOrder = 10;

        Color c = dimmerSprite.color;
        c.a = 1f;
        dimmerSprite.color = c;

        float elapsed = 0f;
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
    }
}