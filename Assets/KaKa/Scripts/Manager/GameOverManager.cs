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

        if (inGameScreen != null)
        {
            inGameScreen.SetActive(false);
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
        isRespawnFade = true;   // ★ 추가

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
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1.0f, 0f, elapsed / fadeDuration);
            dimmerSprite.color = c;
            yield return null;
        }

        c.a = 0f;
        dimmerSprite.color = c;

        // =================================================================
        // 🔥 [수정된 부분] 
        // dimmerSprite를 끄기 전에, 혹시 체크포인트가 휴식 중인지 검사합니다.
        // =================================================================
        bool isAnyCheckpointResting = false;
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);

        // Checkpoint.cs의 private 변수 'isRestingProcess'를 가져올 리플렉션 설정
        var field = typeof(Checkpoint).GetField("isRestingProcess", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (var cp in checkpoints)
        {
            if (field != null)
            {
                if ((bool)field.GetValue(cp)) // 체크포인트가 휴식 중이라면(true)
                {
                    isAnyCheckpointResting = true;
                    break;
                }
            }
        }

        // 아무도 쉬고 있지 않을 때만 비활성화합니다.
        if (!isAnyCheckpointResting)
        {
            dimmerSprite.gameObject.SetActive(false);
        }
        // =================================================================
    }

    // ========================================================
    // ★ [새로 추가된 도우미 함수] 체크포인트 자동 검색
    // ========================================================
    /// <summary>
    /// 씬 안의 모든 체크포인트 중 활성화(isUnlocked)된 것들을 필터링하고,
    /// 플레이어가 사망한 위치와 가장 가까운 체크포인트의 spawnPoint를 반환합니다.
    /// </summary>
    private Transform GetNearestUnlockedCheckpointSpawnPoint()
    {
        if (playerStats == null) return null;

        // 1. 씬 안에 배치된 모든 Checkpoint 컴포넌트를 찾습니다. 
        // (Unity 2023 이후 최적화된 FindObjectsByType 사용)
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        Checkpoint nearestCheckpoint = null;
        float minDistance = float.MaxValue;

        // 2. Checkpoint.cs의 private 필드인 'isUnlocked' 값을 읽기 위해 리플렉션 정보를 가져옵니다.
        var field = typeof(Checkpoint).GetField("isUnlocked", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        foreach (var cp in checkpoints)
        {
            bool unlocked = false;
            if (field != null)
            {
                unlocked = (bool)field.GetValue(cp);
            }

            // 3. 활성화된 체크포인트인 경우에만 거리를 비교합니다.
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

        // 4. 찾은 체크포인트의 spawnPoint를 반환합니다. (없다면 체크포인트 자체의 Transform 반환)
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