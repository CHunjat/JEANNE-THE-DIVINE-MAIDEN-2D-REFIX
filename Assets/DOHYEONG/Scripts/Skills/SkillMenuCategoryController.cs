using UnityEngine;
using UnityEngine.UI;

public class SkillMenuCategoryController : MonoBehaviour
{
    public enum MainCategory
    {
        Passive,
        Active
    }

    public enum ActiveSubCategory
    {
        Attack,
        Heal
    }

    [Header("메인 탭 버튼")]
    [SerializeField] private Button passiveButton;
    [SerializeField] private Button activeButton;

    [Header("메인 패널")]
    [SerializeField] private GameObject passivePanel;
    [SerializeField] private GameObject activePanel;

    [Header("Active 내부 탭 버튼")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button healButton;

    [Header("Active 내부 패널")]
    [SerializeField] private GameObject attackPanel;
    [SerializeField] private GameObject healPanel;

    [Header("처음 열었을 때")]
    [SerializeField]
    private MainCategory defaultMainCategory
        = MainCategory.Passive;

    [SerializeField]
    private ActiveSubCategory defaultActiveSubCategory
        = ActiveSubCategory.Attack;

    private MainCategory currentMainCategory;
    private ActiveSubCategory currentActiveSubCategory;

    private void Awake()
    {
        // 버튼 이벤트 자동 등록
        if (passiveButton != null)
            passiveButton.onClick.AddListener(OpenPassive);

        if (activeButton != null)
            activeButton.onClick.AddListener(OpenActive);

        if (attackButton != null)
            attackButton.onClick.AddListener(OpenAttack);

        if (healButton != null)
            healButton.onClick.AddListener(OpenHeal);
    }

    private void OnEnable()
    {
        // Active 내부 기본 탭 기억
        currentActiveSubCategory = defaultActiveSubCategory;

        // 메뉴 최초 상태
        if (defaultMainCategory == MainCategory.Passive)
        {
            OpenPassive();
        }
        else
        {
            OpenActive();
        }
    }

    // =========================
    // PASSIVE
    // =========================
    public void OpenPassive()
    {
        currentMainCategory = MainCategory.Passive;

        if (passivePanel != null)
            passivePanel.SetActive(true);

        if (activePanel != null)
            activePanel.SetActive(false);
    }

    // =========================
    // ACTIVE
    // =========================
    public void OpenActive()
    {
        currentMainCategory = MainCategory.Active;

        if (passivePanel != null)
            passivePanel.SetActive(false);

        if (activePanel != null)
            activePanel.SetActive(true);

        RefreshActiveSubPanel();
    }

    // =========================
    // ATTACK
    // =========================
    public void OpenAttack()
    {
        if (currentMainCategory != MainCategory.Active)
            return;

        currentActiveSubCategory = ActiveSubCategory.Attack;

        RefreshActiveSubPanel();
    }

    // =========================
    // HEAL
    // =========================
    public void OpenHeal()
    {
        if (currentMainCategory != MainCategory.Active)
            return;

        currentActiveSubCategory = ActiveSubCategory.Heal;

        RefreshActiveSubPanel();
    }

    // =========================
    // Active 내부 패널 갱신
    // =========================
    private void RefreshActiveSubPanel()
    {
        bool isAttack =
            currentActiveSubCategory == ActiveSubCategory.Attack;

        if (attackPanel != null)
            attackPanel.SetActive(isAttack);

        if (healPanel != null)
            healPanel.SetActive(!isAttack);
    }

    private void OnDestroy()
    {
        if (passiveButton != null)
            passiveButton.onClick.RemoveListener(OpenPassive);

        if (activeButton != null)
            activeButton.onClick.RemoveListener(OpenActive);

        if (attackButton != null)
            attackButton.onClick.RemoveListener(OpenAttack);

        if (healButton != null)
            healButton.onClick.RemoveListener(OpenHeal);
    }
}