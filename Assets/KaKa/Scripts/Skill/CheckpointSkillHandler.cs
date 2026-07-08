using UnityEngine;

public class CheckpointSkillHandler : MonoBehaviour
{
    [Header("스킬 UI 오브젝트 (Canvas 내의 꺼져있는 스킬창)")]
    public GameObject skillMenuUI;

    [Header("스킬창이 열릴 때 [비활성화] -> 끌 때 [재활성화]할 오브젝트")]
    public GameObject targetObjectToToggle;

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
}