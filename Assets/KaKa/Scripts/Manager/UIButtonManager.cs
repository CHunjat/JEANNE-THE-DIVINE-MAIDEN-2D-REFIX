using UnityEngine;
using UnityEngine.InputSystem;

public class UIButtonManager : MonoBehaviour
{
    public GameObject InGame;
    public GameObject MainScreen;
    public GameObject Pause;
    public GameObject Option;
    // 내가 새로 만든 UI 전용 인풋 리더 에셋 연결용
    public UIInputReader uiInputReader;

    private void OnEnable()
    {
        if (uiInputReader != null)
        {
            // uiInputReader가 "나 ESC 눌렸어!" 하고 방송하면, 
            // 즉시 내가 만든 HandlePauseInput 함수를 실행하라고 등록(구독)합니다.
            uiInputReader.OnPausePressed += HandlePauseInput;
        }
    }

    private void OnDisable()
    {
        if (uiInputReader != null)
        {
            // 오브젝트가 사라지거나 꺼질 때는 메모리 누수 방지를 위해 구독을 해제합니다.
            uiInputReader.OnPausePressed -= HandlePauseInput;
        }
    }

    // Update() 대신 이 함수가 신호를 받았을 때만 '탁' 실행됩니다.
    private void HandlePauseInput()
    {
        // 인게임 화면이 켜져 있을 때만 일시정지가 작동하게 합니다.
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
    public void NewGameButton()
    {
        Time.timeScale = 1f;
        if (InGame != null)
        {
            InGame.SetActive(true);
        }

        // 2. this.gameObject.SetActive(false) 대신 메인 화면을 직접 꺼줍니다.
        if (MainScreen != null)
        {
            MainScreen.SetActive(false);
        }
    }
    // ====== [새로 추가된 기능 1] Continue 버튼용 함수 ======
    public void ContinueButton()
    {
        if (Pause != null)
        {
            Pause.SetActive(false); // 일시정지 화면 비활성화
        }
        Time.timeScale = 1f; // 게임 시간 다시 정상 진행
    }

    // ====== [새로 추가된 기능 2] Return to Title 버튼용 함수 ======
    public void ReturnToTitleButton()
    {
        if (Pause != null)
        {
            Pause.SetActive(false); // 일시정지 화면 끄기
        }
        if (InGame != null)
        {
            InGame.SetActive(false); // 인게임 화면 끄기
        }
        if (MainScreen != null)
        {
            MainScreen.SetActive(true); // 타이틀(메인) 화면 켜기
        }
        Time.timeScale = 0f;
    }

    // ====== [새로 추가된 기능] 게임 완전히 종료하기 ======
    public void ExitGameButton()
    {
        // 1. 실제로 빌드된 PC/모바일 게임을 완전히 종료합니다.
        Application.Quit();

        // 2. [테스트용] 유니티 에디터 환경에서 누르면 플레이 모드(▶)가 꺼지도록 만듭니다.
        // #if와 #endif는 전처리 지시어로, 유니티 에디터 안에서만 이 코드가 작동하게 해줍니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif

        // 3. 작동 여부를 콘솔창에서 확실히 확인하기 위한 로그
        Debug.Log("게임이 종료되었습니다.");
    }
    public void OptionButton()
    {
        if (Pause != null)
        {
            Pause.SetActive(false); // 일시정지 화면 다시 켜기
        }
        if (Option != null)
        {
            Option.SetActive(true); // 옵션 화면 끄기
        }
    }
    public void OptionBackButton()
    {
        if (Option != null)
        {
            Option.SetActive(false); // 옵션 화면 끄기
        }
        if (Pause != null)
        {
            Pause.SetActive(true); // 일시정지 화면 다시 켜기
        }
    }
}
