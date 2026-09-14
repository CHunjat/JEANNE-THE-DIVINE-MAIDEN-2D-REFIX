using UnityEngine;
using UnityEngine.SceneManagement; // ★ 씬 관리를 위해 필수 추가

public class UIButtonManager : MonoBehaviour
{
    public GameObject GameplayHUD;
    public GameObject IntroScreen;
    public GameObject InGame;
    public GameObject MainScreen;
    public GameObject Pause;
    public GameObject Option;
    // 내가 새로 만든 UI 전용 인풋 리더 에셋 연결용
    public UIInputReader uiInputReader;

    [Header("플레이어 조작")]
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        if (GameOverManager.skipMainMenu)
        {
            GameOverManager.skipMainMenu = false;

            if (IntroScreen != null)
                IntroScreen.SetActive(false);

            if (InGame != null)
                InGame.SetActive(true);

            if (MainScreen != null)
                MainScreen.SetActive(false);

            if (Pause != null)
                Pause.SetActive(false);

            if (Option != null)
                Option.SetActive(false);

            if (GameplayHUD != null)
                GameplayHUD.SetActive(true);

            Time.timeScale = 1f;

            // ★ 실제 게임 플레이 가능
            SetPlayerControl(true);
        }
        else
        {
            if (InGame != null)
                InGame.SetActive(false);

            if (MainScreen != null)
                MainScreen.SetActive(false);

            if (Pause != null)
                Pause.SetActive(false);

            if (Option != null)
                Option.SetActive(false);

            if (GameplayHUD != null)
                GameplayHUD.SetActive(false);

            if (IntroScreen != null)
                IntroScreen.SetActive(true);

            Time.timeScale = 0f;

            // ★ 인트로에서는 조작 불가
            SetPlayerControl(false);
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
        if (Pause == null) return;

        bool isPauseActive = !Pause.activeSelf;

        Pause.SetActive(isPauseActive);

        if (GameplayHUD != null)
        {
            GameplayHUD.SetActive(!isPauseActive);
        }

        Time.timeScale = isPauseActive ? 0f : 1f;

        // ★ Pause 중 플레이어 조작 차단
        SetPlayerControl(!isPauseActive);
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
       
        if (GameplayHUD != null)
        {
            GameplayHUD.SetActive(true);
        }
        // 씬 완전 초기화
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ContinueButton()
    {
        if (Pause != null)
        {
            Pause.SetActive(false);
        }

        if (GameplayHUD != null)
        {
            GameplayHUD.SetActive(true);
        }

        Time.timeScale = 1f;

        // ★ 게임으로 복귀
        SetPlayerControl(true);
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

        if (GameplayHUD != null)
        {
            GameplayHUD.SetActive(false);
        }

        Time.timeScale = 0f;

        // ★ 메인 화면이므로 조작 금지
        SetPlayerControl(false);
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

        if (GameplayHUD != null)
        {
            GameplayHUD.SetActive(false);
        }

        if (Option != null)
        {
            Option.SetActive(true);
        }

        SetPlayerControl(false);
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

        if (GameplayHUD != null)
        {
            GameplayHUD.SetActive(false);
        }

        // ★ 아직 Pause 화면
        SetPlayerControl(false);
    }

    private void SetPlayerControl(bool canControl)
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }

        if (playerController != null)
        {
            playerController.canControl = canControl;

            // UI가 열리는 순간 기존 이동 관성 제거
            if (!canControl && playerController.rb != null)
            {
                playerController.rb.linearVelocity =
                    new Vector2(0f, playerController.rb.linearVelocity.y);
            }
        }
    }
}