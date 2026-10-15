using UnityEngine;
using TMPro;

public class ActiveSkillManager : MonoBehaviour
{
    [Header("Active Skill List")]
    public Active_Skill[] activeSkills;

    [Header("Connected Manager")]
    [Tooltip("상단의 스킬 슬롯 매니저(SkillUIManager)를 연결해 주세요.")]
    public SkillUIManager skillUIManager;

    [Header("Detail Panel")]
    [SerializeField] private SkillDetailPanelUI detailPanelUI;

    [Header("선택된 스킬 이름")]
    [SerializeField] private TMP_Text selectedSkillNameText;

    [SerializeField] private string defaultSkillNameText = "스킬을 선택하세요.";

    private int currentSelectedIndex = -1;

    public int CurrentSelectedIndex => currentSelectedIndex;


    private void Start()
    {
        for (int i = 0; i < activeSkills.Length; i++)
        {
            if (activeSkills[i] != null)
            {
                activeSkills[i].skillIndex = i;
                activeSkills[i].manager = this;
                activeSkills[i].SetSelectState(false);
            }
        }

        if (detailPanelUI != null)
        {
            detailPanelUI.Clear();
        }
    }


    // =====================================================
    // 키보드 선택
    // 실제 화면에 보이는 위치를 기준으로 이동
    // =====================================================

    private void Update()
    {
        if (activeSkills == null || activeSkills.Length == 0)
            return;

        // 아무것도 선택되어 있지 않은 상태
        if (currentSelectedIndex == -1)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow))
            {
                int firstIndex = FindTopLeftSkill();

                if (firstIndex != -1)
                {
                    SelectSkill(firstIndex);
                }
            }

            return;
        }


        Vector2 direction = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direction = Vector2.up;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direction = Vector2.down;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direction = Vector2.left;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direction = Vector2.right;
        }


        if (direction == Vector2.zero)
            return;


        int nextIndex =
            FindNearestSkillInDirection(
                currentSelectedIndex,
                direction
            );


        if (nextIndex != -1)
        {
            SelectSkill(nextIndex);
        }
    }


    // =====================================================
    // 현재 스킬 기준 해당 방향에 있는 가장 적절한 스킬 찾기
    // =====================================================

    private int FindNearestSkillInDirection(
        int fromIndex,
        Vector2 direction)
    {
        if (fromIndex < 0 ||
            fromIndex >= activeSkills.Length ||
            activeSkills[fromIndex] == null)
        {
            return -1;
        }


        RectTransform currentRect =
            activeSkills[fromIndex].GetComponent<RectTransform>();

        if (currentRect == null)
            return -1;


        Vector2 currentPosition = currentRect.position;

        int bestIndex = -1;
        float bestScore = float.MaxValue;


        for (int i = 0; i < activeSkills.Length; i++)
        {
            if (i == fromIndex)
                continue;

            if (activeSkills[i] == null)
                continue;

            // 화면에 실제로 활성화되어 있는 스킬만
            if (!activeSkills[i].gameObject.activeInHierarchy)
                continue;


            RectTransform candidateRect =
                activeSkills[i].GetComponent<RectTransform>();

            if (candidateRect == null)
                continue;


            Vector2 candidatePosition =
                candidateRect.position;

            Vector2 offset =
                candidatePosition - currentPosition;


            if (offset.sqrMagnitude <= 0.001f)
                continue;


            Vector2 normalizedOffset =
                offset.normalized;


            // 내가 누른 방향과 얼마나 같은 방향인지
            float directionDot =
                Vector2.Dot(
                    normalizedOffset,
                    direction
                );


            // 옆/뒤쪽 후보 제거
            if (directionDot <= 0.5f)
                continue;


            float distance =
                offset.magnitude;


            // 방향이 정확할수록 점수가 낮아짐
            float anglePenalty =
                1f - directionDot;


            float score =
                distance +
                anglePenalty * 1000f;


            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }


        return bestIndex;
    }


    // =====================================================
    // 처음 방향키 입력 시 화면에서 가장 왼쪽 위 스킬 선택
    // =====================================================

    private int FindTopLeftSkill()
    {
        int bestIndex = -1;

        float bestY = float.MinValue;
        float bestX = float.MaxValue;


        for (int i = 0; i < activeSkills.Length; i++)
        {
            if (activeSkills[i] == null)
                continue;

            if (!activeSkills[i].gameObject.activeInHierarchy)
                continue;


            RectTransform rect =
                activeSkills[i].GetComponent<RectTransform>();

            if (rect == null)
                continue;


            Vector2 position = rect.position;


            // 더 위쪽에 있는 스킬
            if (position.y > bestY + 10f)
            {
                bestY = position.y;
                bestX = position.x;
                bestIndex = i;
            }

            // 높이가 거의 같다면 더 왼쪽
            else if (
                Mathf.Abs(position.y - bestY) <= 10f &&
                position.x < bestX)
            {
                bestX = position.x;
                bestIndex = i;
            }
        }


        return bestIndex;
    }


    // =====================================================
    // 실제 스킬 선택
    // =====================================================

    public void SelectSkill(int index)
    {
        if (index < 0 || index >= activeSkills.Length)
            return;


        // 등록 슬롯 선택 해제
        if (skillUIManager != null)
        {
            skillUIManager.ClearSelection();
        }


        // 이전 선택 해제
        if (currentSelectedIndex != -1 &&
            currentSelectedIndex < activeSkills.Length)
        {
            if (activeSkills[currentSelectedIndex] != null)
            {
                activeSkills[currentSelectedIndex]
                    .SetSelectState(false);
            }
        }


        // 새 선택
        currentSelectedIndex = index;


        if (activeSkills[currentSelectedIndex] != null)
        {
            activeSkills[currentSelectedIndex]
                .SetSelectState(true);


            SkillData selectedSkill =
                activeSkills[currentSelectedIndex].skillData;


            // 위쪽 선택 스킬 이름 변경
            if (selectedSkillNameText != null &&
                selectedSkill != null)
            {
                selectedSkillNameText.text =
                    selectedSkill.skillName;
            }


            // 디테일 패널
            if (detailPanelUI != null)
            {
                detailPanelUI.ShowSkill(selectedSkill);
            }
        }
    }


    // =====================================================
    // 선택 해제
    // =====================================================

    public void ClearSelection()
    {
        if (currentSelectedIndex != -1 &&
            currentSelectedIndex < activeSkills.Length)
        {
            if (activeSkills[currentSelectedIndex] != null)
            {
                activeSkills[currentSelectedIndex]
                    .SetSelectState(false);
            }
        }


        currentSelectedIndex = -1;


        if (selectedSkillNameText != null)
        {
            selectedSkillNameText.text =
                defaultSkillNameText;
        }


        if (detailPanelUI != null)
        {
            detailPanelUI.Clear();
        }
    }
}