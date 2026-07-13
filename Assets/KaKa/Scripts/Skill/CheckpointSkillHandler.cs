using UnityEngine;
using System.Collections;
public class CheckpointSkillHandler : MonoBehaviour
{
    [Header("스킬 UI 오브젝트 (Canvas 내의 꺼져있는 스킬창)")]
    public GameObject skillMenuUI;

    [Header("스킬창이 열릴 때 [비활성화] -> 끌 때 [재활성화]할 오브젝트")]
    public GameObject targetObjectToToggle;

    [Header("플레이어 연결 (RestState 탈출용)")]
    public PlayerController playerController;

    private Checkpoint checkpoint;
    private bool isSkillMenuOpen = false;

    private void Start()
    {
        // 동일한 오브젝트에 붙어 있는 Checkpoint 컴포넌트를 자동으로 가져옵니다.
        checkpoint = GetComponent<Checkpoint>();

        if (skillMenuUI != null)
        {
            skillMenuUI.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (checkpoint == null) return;

        // 1. 스킬창이 닫혀있고, 체크포인트 메뉴가 열려있을 때
        if (!isSkillMenuOpen && checkpoint.menuUI != null && checkpoint.menuUI.activeSelf)
        {
            // 3번째 커서(스킬)가 활성화되어 있는지 확인
            if (checkpoint.menuCursors != null && checkpoint.menuCursors.Length > 2)
            {
                if (checkpoint.menuCursors[2] != null && checkpoint.menuCursors[2].activeSelf)
                {
                    // 그 상태에서 C 키가 눌렸다면 스킬창 오픈
                    if (Input.GetKeyDown(KeyCode.C))
                    {
                        OpenSkillMenu();
                    }
                }
            }
        }
        // 2. 스킬창이 이미 열려있는 상태에서 ESC를 누르면 이전 메뉴로 복귀
        else if (isSkillMenuOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseSkillMenu();
            }
        }
    }

    private void OpenSkillMenu()
    {
        if (skillMenuUI != null)
        {
            skillMenuUI.SetActive(true);
            isSkillMenuOpen = true;

            // 원본 체크포인트 UI를 잠시 꺼서 가려줍니다.
            if (checkpoint.menuUI != null)
            {
                checkpoint.menuUI.SetActive(false);
            }

            // [추가된 기능] 스킬창이 켜질 때 지정된 다른 오브젝트를 비활성화
            if (targetObjectToToggle != null)
            {
                targetObjectToToggle.SetActive(false);
            }
        }
    }

    private void CloseSkillMenu()
    {
        if (skillMenuUI != null)
        {
            skillMenuUI.SetActive(false);
            isSkillMenuOpen = false;

            // 숨겼던 원본 체크포인트 UI를 다시 켜서 원상복구
            if (checkpoint.menuUI != null)
            {
                checkpoint.menuUI.SetActive(true);
            }

            // [추가된 기능] 스킬창이 꺼질 때 지정된 다른 오브젝트를 다시 활성화
            if (targetObjectToToggle != null)
            {
                targetObjectToToggle.SetActive(true);
            }
        }
    }
    // SkillUIManager의 '확인' 버튼에서 호출 — 체크포인트 메뉴로 돌아가지 않고 바로 게임으로 복귀
    public void ConfirmAndExit()
    {
        if (skillMenuUI != null)
        {
            skillMenuUI.SetActive(false);
        }
        isSkillMenuOpen = false;

        // checkpoint.menuUI는 켜지 않음 (ESC 경로와 달리 체크포인트 메뉴로 안 돌아가고 바로 게임 복귀)
        if (targetObjectToToggle != null)
        {
            targetObjectToToggle.SetActive(true);
        }

        if (playerController != null && playerController.StateMachine.CurrentState == playerController.RestState)
        {
            playerController.StateMachine.ChangeState(playerController.StandUpState);
        }
        // ★ 추가: 어둡게 깔린 Dimmer도 원상복구 (0으로 페이드)
        if (checkpoint != null && checkpoint.dimmerSprite != null)
        {
            StartCoroutine(FadeDimmerOut());
        }
    }


    // ★ 추가: Checkpoint.FadeAlpha와 동일한 방식이지만, Checkpoint.cs를 안 건드리기 위해 여기서 자체 구현
    private IEnumerator FadeDimmerOut()
    {
        SpriteRenderer dimmer = checkpoint.dimmerSprite;
        float duration = checkpoint.fadeDuration; // Checkpoint의 fadeDuration 값 그대로 재사용 (public 필드)

        Color c = dimmer.color;
        float startAlpha = c.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            c.a = Mathf.Lerp(startAlpha, 0f, time / duration);
            dimmer.color = c;
            yield return null;
        }

        c.a = 0f;
        dimmer.color = c;
        dimmer.gameObject.SetActive(false); // Checkpoint.FadeAlpha와 동일하게, 0 도달 시 오브젝트 자체도 꺼줌
    }
}