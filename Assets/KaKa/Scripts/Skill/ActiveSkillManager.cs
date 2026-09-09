using UnityEngine;

public class ActiveSkillManager : MonoBehaviour
{
    [Header("Active Skill List")]
    public Active_Skill[] activeSkills;

    [Header("Connected Manager")]
    [Tooltip("상단의 스킬 슬롯 매니저(SkillUIManager)를 연결해 주세요.")]
    public SkillUIManager skillUIManager;

    [Header("Detail Panel")]
    [SerializeField] private SkillDetailPanelUI detailPanelUI;

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

    private void Update()
    {
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

        // 등록 슬롯 선택 해제
        if (skillUIManager != null)
        {
            skillUIManager.ClearSelection();
        }

        // 이전 선택 해제
        if (currentSelectedIndex != -1 && currentSelectedIndex < activeSkills.Length)
        {
            if (activeSkills[currentSelectedIndex] != null)
                activeSkills[currentSelectedIndex].SetSelectState(false);
        }

        // 새 선택
        currentSelectedIndex = index;

        if (activeSkills[currentSelectedIndex] != null)
        {
            activeSkills[currentSelectedIndex].SetSelectState(true);

            // ★ 디테일 패널에 스킬명 표시
            if (detailPanelUI != null)
            {
                detailPanelUI.ShowSkill(activeSkills[currentSelectedIndex].skillData);
            }
        }
    }

    public void ClearSelection()
    {
        if (currentSelectedIndex != -1 && currentSelectedIndex < activeSkills.Length)
        {
            if (activeSkills[currentSelectedIndex] != null)
                activeSkills[currentSelectedIndex].SetSelectState(false);
        }

        currentSelectedIndex = -1;

        if (detailPanelUI != null)
        {
            detailPanelUI.Clear();
        }
    }
}