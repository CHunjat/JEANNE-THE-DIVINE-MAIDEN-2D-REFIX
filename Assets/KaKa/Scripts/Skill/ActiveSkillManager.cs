using UnityEngine;

public class ActiveSkillManager : MonoBehaviour
{
    [Header("Active Skill List")]
    public Active_Skill[] activeSkills;

    [Header("Connected Manager")]
    [Tooltip("상단의 스킬 슬롯 매니저(SkillUIManager)를 연결해 주세요.")]
    public SkillUIManager skillUIManager;

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
    }

    private void Update()
    {
        // ⭐ 핵심: 하단 영역 버튼이 선택되어 포커스된 상태일 때만 좌우 입력을 허용합니다.
        if (currentSelectedIndex == -1 || activeSkills.Length == 0) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            int nextIndex = currentSelectedIndex - 1;
            if (nextIndex < 0) nextIndex = activeSkills.Length - 1;
            SelectSkill(nextIndex);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            int nextIndex = currentSelectedIndex + 1;
            if (nextIndex >= activeSkills.Length) nextIndex = 0;
            SelectSkill(nextIndex);
        }
    }

    public void SelectSkill(int index)
    {
        if (index < 0 || index >= activeSkills.Length) return;

        // 💡 하단 버튼을 클릭/선택하는 순간 상단 슬롯 영역의 선택을 강제 해제합니다.
        if (skillUIManager != null)
        {
            skillUIManager.ClearSelection();
        }

        if (currentSelectedIndex != -1 && currentSelectedIndex < activeSkills.Length)
        {
            if (activeSkills[currentSelectedIndex] != null) activeSkills[currentSelectedIndex].SetSelectState(false);
        }

        currentSelectedIndex = index;
        if (activeSkills[currentSelectedIndex] != null) activeSkills[currentSelectedIndex].SetSelectState(true);
    }

    // 상단 매니저가 내 영역의 포커스를 꺼버릴 때 호출할 함수
    public void ClearSelection()
    {
        if (currentSelectedIndex != -1 && currentSelectedIndex < activeSkills.Length)
        {
            if (activeSkills[currentSelectedIndex] != null) activeSkills[currentSelectedIndex].SetSelectState(false);
        }
        currentSelectedIndex = -1;
    }
}