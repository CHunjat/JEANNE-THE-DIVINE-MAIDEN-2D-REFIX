using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 관리를 위해 필수 추가

public class UIButtonManager : MonoBehaviour
{
    public GameObject InGame;
    public GameObject MainScreen;
    public GameObject Pause;
    public GameObject Option;
    // 내가 새로 만든 UI 전용 인풋 리더 에셋 연결용
    public UIInputReader uiInputReader;

    private void Start()
    {
        // ========================================================
        // ★ [씬 재로드 시 UI 처리]
        // ========================================================
        // 사망 후 부활하거나 새 게임을 눌러 씬이 새로 로드된 상태인 경우
        if (GameOverManager.skipMainMenu)
        {
            GameOverManager.skipMainMenu = false; // 플래그는 사용 후 즉시 리셋

            if (InGame != null)
                InGame.SetActive(true);

            if (MainScreen != null)
                MainScreen.SetActive(false);

            // 혹시 남아있는 UI가 있으면 모두 닫기
            if (Pause != null)
                Pause.SetActive(false);

            if (Option != null)
                Option.SetActive(false);

            Time.timeScale = 1f;
        }
        else
        {
            // 게임 최초 실행
            if (InGame != null)
                InGame.SetActive(false);

            if (MainScreen != null)
                MainScreen.SetActive(true);

            if (Pause != null)
                Pause.SetActive(false);

            if (Option != null)
                Option.SetActive(false);

            Time.timeScale = 0f;
        }
    }

    private void OnEnable()
    {
        if (uiInputReader != null)
        {
            uiInputReader.OnPausePressed += HandlePauseInput;
        }
    }

    private void OnDisable()
    {
        if (uiInputReader != null)
        {
            uiInputReader.OnPausePressed -= HandlePauseInput;
        }
    }

    private void HandlePauseInput()
    {
        // ========================================================
        // 🔥 [추가] 체크포인트 예외 처리
        // 플레이어가 휴식 중이거나 메뉴를 탈출하는 도중이라면 일시정지 감지를 무시합니다.
        // ========================================================
        if (IsPlayerInteractingWithCheckpoint()) return;

        if (InGame != null && InGame.activeSelf)
        {
            TogglePause();
        }
    }

    // ========================================================
    // 🔥 [추가] 플레이어가 체크포인트와 상호작용 중인지 판별하는 헬퍼 함수
    // ========================================================
    private bool IsPlayerInteractingWithCheckpoint()
    {
        // 1. 플레이어 상태를 기반으로 체크
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.StateMachine != null)
        {
            var currentState = player.StateMachine.CurrentState;
            // 휴식 중(RestState)이거나 일어나는 상태(StandUpState)인 경우 일시정지 방지
            if (currentState == player.RestState || currentState == player.StandUpState)
            {
                return true;
            }
        }

        // 2. 씬에 배치된 체크포인트 UI의 활성화 여부를 기반으로 체크 (방어용 서브 시스템)
        Checkpoint[] checkpoints = FindObjectsOfType<Checkpoint>();
        if (checkpoints != null)
        {
            foreach (var cp in checkpoints)
            {
                if (cp != null)
                {
                    // 메인 휴식 메뉴나 텔레포트 메뉴 중 하나라도 켜져 있다면 상호작용 중으로 간주
                    if ((cp.menuUI != null && cp.menuUI.activeSelf) ||
                        (cp.teleportMenuUI != null && cp.teleportMenuUI.activeSelf))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void TogglePause()
    {
        if (Pause != null)
        {
            bool isPauseActive = !Pause.activeSelf;
            Pause.SetActive(isPauseActive);
            Time.timeScale = isPauseActive ? 0f : 1f;
        }
    }

    public void NewGameButton()
    {
        Time.timeScale = 1f;

        // ==========================
        // 게임 상태 초기화
        // ==========================
        GameOverManager.lastRespawnPosition = null;
        GameOverManager.skipMainMenu = true;

        // ★ 게임오버 연출 비활성화
        GameOverManager.shouldFadeIn = false;
        GameOverManager.isRespawnFade = false;

        // UI 초기화
        if (Pause != null)
            Pause.SetActive(false);

        if (Option != null)
            Option.SetActive(false);

        if (InGame != null)
            InGame.SetActive(true);

        if (MainScreen != null)
            MainScreen.SetActive(false);

        // 씬 완전 초기화
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ContinueButton()
    {
        if (Pause != null)
        {
            Pause.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    public void ReturnToTitleButton()
    {
        if (Pause != null)
        {
            Pause.SetActive(false);
        }
        if (InGame != null)
        {
            InGame.SetActive(false);
        }
        if (MainScreen != null)
        {
            MainScreen.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    public void ExitGameButton()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Debug.Log("게임이 종료되었습니다.");
    }

    public void OptionButton()
    {
        if (Pause != null)
        {
            Pause.SetActive(false);
        }
        if (Option != null)
        {
            Option.SetActive(true);
        }
    }

    public void OptionBackButton()
    {
        if (Option != null)
        {
            Option.SetActive(false);
        }
        if (Pause != null)
        {
            Pause.SetActive(true);
        }
    }
}