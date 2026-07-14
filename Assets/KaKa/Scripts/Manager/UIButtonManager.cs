using UnityEngine;
using UnityEngine.InputSystem;
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

            if (InGame != null) InGame.SetActive(true);
            if (MainScreen != null) MainScreen.SetActive(false);
            Time.timeScale = 1f; // 게임 시간 정상 진행
        }
        else
        {
            // 빌드 후 게임을 처음 실행했을 때의 타이틀 화면 기본 세팅
            if (InGame != null) InGame.SetActive(false);
            if (MainScreen != null) MainScreen.SetActive(true);
            Time.timeScale = 0f; // 타이틀 화면에서는 시간 정지
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
        if (InGame != null && InGame.activeSelf)
        {
            TogglePause();
        }
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

    // ========================================================
    // ★ [새 게임 기능 수정]
    // ========================================================
    public void NewGameButton()
    {
        Time.timeScale = 1f;

        // 새 게임을 시작하는 것이므로 기존 부활 체크포인트 이력을 완전히 날립니다.
        GameOverManager.lastRespawnPosition = null;

        // 타이틀(메인 화면)을 건너뛰고 바로 게임 인게임이 뜨게끔 만듭니다.
        GameOverManager.skipMainMenu = true;
        GameOverManager.shouldFadeIn = true; // 새 게임 진입 시에도 부드럽게 화면이 켜지게 함

        // 씬을 완전히 새로 로드하여 맵, 보스, 플레이어 스탯을 완전 순정으로 새 출발합니다!
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