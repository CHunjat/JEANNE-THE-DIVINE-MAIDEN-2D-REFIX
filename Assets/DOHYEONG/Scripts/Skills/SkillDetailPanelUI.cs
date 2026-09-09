using TMPro;
using UnityEngine;

public class SkillDetailPanelUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text skillNameText;

    [Header("기본 문구")]
    [SerializeField] private string defaultText = "스킬을 선택하세요";

    private void Awake()
    {
        Clear();
    }

    public void ShowSkill(SkillData skillData)
    {
        if (skillNameText == null) return;

        if (skillData == null)
        {
            skillNameText.text = defaultText;
            return;
        }

        skillNameText.text = skillData.skillName;
    }

    public void Clear()
    {
        if (skillNameText == null) return;
        skillNameText.text = defaultText;
    }
}