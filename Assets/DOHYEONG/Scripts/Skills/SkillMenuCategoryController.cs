using UnityEngine;
using UnityEngine.UI;

public class SkillMenuCategoryController : MonoBehaviour
{
    public enum MainCategory
    {
        Passive,
        Active
    }

    [Header("메인 탭 버튼")]
    [SerializeField] private Button passiveButton;
    [SerializeField] private Button activeButton;

    [Header("메인 패널")]
    [SerializeField] private GameObject passivePanel;
    [SerializeField] private GameObject activePanel;

    [Header("처음 열었을 때")]
    [SerializeField]
    private MainCategory defaultMainCategory
        = MainCategory.Passive;

    private MainCategory currentMainCategory;

    private void Awake()
    {
        if (passiveButton != null)
            passiveButton.onClick.AddListener(OpenPassive);

        if (activeButton != null)
            activeButton.onClick.AddListener(OpenActive);
    }

    private void OnEnable()
    {
        if (defaultMainCategory == MainCategory.Passive)
            OpenPassive();
        else
            OpenActive();
    }

    public void OpenPassive()
    {
        currentMainCategory = MainCategory.Passive;

        if (passivePanel != null)
            passivePanel.SetActive(true);

        if (activePanel != null)
            activePanel.SetActive(false);
    }

    public void OpenActive()
    {
        currentMainCategory = MainCategory.Active;

        if (passivePanel != null)
            passivePanel.SetActive(false);

        if (activePanel != null)
            activePanel.SetActive(true);
    }

    private void OnDestroy()
    {
        if (passiveButton != null)
            passiveButton.onClick.RemoveListener(OpenPassive);

        if (activeButton != null)
            activeButton.onClick.RemoveListener(OpenActive);
    }
}