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
    public GameObject bossUiScreen;       // Canvas의 Boss UI 오브젝트 (보스 체력바 등)
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

    public static bool isRespawnFade = false;

    private bool isGameOverTriggered = false;

    private void Start()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

        // 부활 위치 적용
        if (lastRespawnPosition.HasValue && playerStats != null)
        {
            playerStats.transform.position = lastRespawnPosition.Value;
        }

        // ====================================================
        // 게임오버 후 부활일 때만 FadeIn 실행
        // ====================================================
        if (shouldFadeIn && isRespawnFade && dimmerSprite != null)
        {
            shouldFadeIn = false;
            isRespawnFade = false;

            StartCoroutine(DelayedFadeIn(0.1f));
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

        // 일반 인게임 UI 비활성화
        if (inGameScreen != null)
        {
            inGameScreen.SetActive(false);
        }

        // 화면이 어두워지기 전에 보스 UI도 즉시 꺼줍니다.
        if (bossUiScreen != null)
        {
            bossUiScreen.SetActive(false);
        }

        if (dimmerSprite == null)
            yield break;

        // 카메라 위치 추적 암전 (보스방 위치 고려)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 camPos = mainCamera.transform.position;
            dimmerSprite.transform.position =
                new Vector3(camPos.x, camPos.y, dimmerSprite.transform.position.z);
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
        {
            gameOverScreen.SetActive(true);
        }

        // 3초 대기
        yield return new WaitForSeconds(respawnDelay);

        // 체크포인트 탐색
        Transform targetRespawnTransform = GetNearestUnlockedCheckpointSpawnPoint();

        if (targetRespawnTransform != null)
        {
            lastRespawnPosition = targetRespawnTransform.position;
            Debug.Log($"[GameOverManager] 활성화된 체크포인트 발견! 부활 위치: {lastRespawnPosition}");
        }
        else if (respawnPoint != null)
        {
            lastRespawnPosition = respawnPoint.position;
            Debug.Log($"[GameOverManager] 기본 시작 지점으로 부활합니다. 부활 위치: {lastRespawnPosition}");
        }

        // =====================================================
        // ★ 게임오버로 인한 재로드임을 표시
        // =====================================================
        skipMainMenu = true;
        shouldFadeIn = true;
        isRespawnFade = true;

        // 씬 재로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator FadeInEffectRoutine()
    {
        dimmerSprite.gameObject.SetActive(true);
        dimmerSprite.sortingOrder = 10;

        Color c = dimmerSprite.color;
        c.a = 1f;
        dimmerSprite.color = c;

        float elapsed = 0f;

        // Checkpoint.cs의 private 변수 'isRestingProcess'를 루프 밖에서 미리 한 번만 캐싱합니다 (매 프레임 호출 방지 최적화)
        var field = typeof(Checkpoint).GetField("isRestingProcess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        while (elapsed < fadeDuration)
        {
            // 🔥 [추가] 페이드인 도중 플레이어가 체크포인트와 상호작용(메뉴 열기 혹은 휴식)을 시도했다면,
            // 게임오버 매니저의 코루틴을 즉각 종료(yield break)하여 겹침 현상을 원천 차단합니다.
            if (IsPlayerInteractingWithCheckpoint(field))
            {
                Debug.Log("[GameOverManager] 페이드인 도중 체크포인트 상호작용 감지! 코루틴을 양보하고 종료합니다.");
                yield break;
            }

            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1.0f, 0f, elapsed / fadeDuration);
            dimmerSprite.color = c;
            yield return null;
        }

        c.a = 0f;
        dimmerSprite.color = c;

        // 페이드 아웃이 완전히 끝난 시점에도 체크포인트 조작 중이 아닐 때만 디머를 비활성화합니다.
        if (!IsPlayerInteractingWithCheckpoint(field))
        {
            dimmerSprite.gameObject.SetActive(false);
        }
    }

    // 🔥 [새로 추가된 도우미 함수] 플레이어가 현재 체크포인트와 상호작용 중인지 실시간 검사
    private bool IsPlayerInteractingWithCheckpoint(System.Reflection.FieldInfo field)
    {
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (var cp in checkpoints)
        {
            if (cp == null) continue;

            // 1. 체크포인트 메뉴 UI 혹은 텔레포트 메뉴 UI가 켜져 있는지 확인
            if ((cp.menuUI != null && cp.menuUI.activeSelf) ||
                (cp.teleportMenuUI != null && cp.teleportMenuUI.activeSelf))
            {
                return true;
            }

            // 2. 체크포인트에서 휴식 혹은 텔레포트 동작이 실행 중인지 확인 (isRestingProcess 변수 값 확인)
            if (field != null)
            {
                bool isResting = (bool)field.GetValue(cp);
                if (isResting)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // ========================================================
    // ★ [새로 추가된 도우미 함수] 체크포인트 자동 검색
    // ========================================================
    private Transform GetNearestUnlockedCheckpointSpawnPoint()
    {
        if (playerStats == null) return null;

        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        Checkpoint nearestCheckpoint = null;
        float minDistance = float.MaxValue;

        var field = typeof(Checkpoint).GetField("isUnlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (var cp in checkpoints)
        {
            bool unlocked = false;
            if (field != null)
            {
                unlocked = (bool)field.GetValue(cp);
            }

            if (unlocked)
            {
                float dist = Vector3.Distance(playerStats.transform.position, cp.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestCheckpoint = cp;
                }
            }
        }

        if (nearestCheckpoint != null)
        {
            return nearestCheckpoint.spawnPoint != null ? nearestCheckpoint.spawnPoint : nearestCheckpoint.transform;
        }

        return null;
    }

    private IEnumerator DelayedFadeIn(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(FadeInEffectRoutine());
    }
}