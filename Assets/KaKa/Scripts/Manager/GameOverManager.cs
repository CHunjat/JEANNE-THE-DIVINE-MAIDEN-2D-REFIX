using UnityEngine;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("연결할 컴포넌트")]
    public PlayerStats playerStats;       // 플레이어 스탯 스크립트
    public SpriteRenderer dimmerSprite;   // 암전용 Dimmer_Sprite
    public GameObject gameOverScreen;     // Canvas의 GameOverScreen 오브젝트
    public GameObject inGameScreen;       // ★ 추가: Canvas의 InGameScreen 오브젝트

    [Header("연출 설정")]
    public float fadeDuration = 2.0f;     // 화면이 완전히 어두워지는 데 걸리는 시간
    [Range(0f, 1f)]
    public float uiTriggerAlpha = 0.7f;   // UI가 켜질 어두움 정도 (70% = 0.7)

    private bool isGameOverTriggered = false;

    private void Start()
    {
        // 게임 시작 시 게임오버 UI는 꺼둡니다.
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
    }

    private void Update()
    {
        // 이미 게임오버 연출이 시작되었다면 체크하지 않음
        if (isGameOverTriggered) return;

        // 플레이어 HP가 0 이하가 되면 게임오버 연출 시작
        if (playerStats != null && playerStats.currentHp <= 0)
        {
            StartCoroutine(GameOverRoutine());
        }
    }

    private IEnumerator GameOverRoutine()
    {
        isGameOverTriggered = true;

        // ★ [기능 추가] 체력이 0이 되자마자 인게임 화면(HUD 등)을 비활성화
        if (inGameScreen != null)
        {
            inGameScreen.SetActive(false);
        }

        if (dimmerSprite == null) yield break;

        // Dimmer 활성화 및 레이어 순위 격상
        dimmerSprite.gameObject.SetActive(true);
        dimmerSprite.sortingOrder = 10;

        Color c = dimmerSprite.color;
        float startAlpha = c.a;
        float elapsed = 0f;
        bool isUiActivated = false;

        // 부드럽게 암전 시작 (목표는 완전히 어두워지는 1.0f)
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, 1.0f, elapsed / fadeDuration);

            c.a = currentAlpha;
            dimmerSprite.color = c;

            // 어두기가 설정값(70%) 이상이 되는 순간 게임오버 캔버스 활성화!
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

        // 최종 알파값 고정
        c.a = 1.0f;
        dimmerSprite.color = c;

        // 예외 방어용 코드
        if (!isUiActivated && gameOverScreen != null)
            gameOverScreen.SetActive(true);
    }
}