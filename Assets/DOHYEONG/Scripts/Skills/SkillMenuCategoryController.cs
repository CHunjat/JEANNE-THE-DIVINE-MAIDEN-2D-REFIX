using DG.Tweening;
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

    [Header("선택 프레임")]
    [SerializeField] private RectTransform selectorFrame;

    [SerializeField] private RectTransform passiveTarget;
    [SerializeField] private RectTransform activeTarget;

    [Header("프레임 이동")]
    [SerializeField] private float selectorDuration = 0.22f;

    [Header("처음 열었을 때")]
    [SerializeField]
    private MainCategory defaultMainCategory
        = MainCategory.Passive;

    private MainCategory currentMainCategory;

    private Tween selectorTween;
    private float selectorY;

    private void Awake()
    {
        if (passiveButton != null)
            passiveButton.onClick.AddListener(OpenPassive);

        if (activeButton != null)
            activeButton.onClick.AddListener(OpenActive);

        if (selectorFrame != null)
            selectorY = selectorFrame.anchoredPosition.y;
    }

    private void OnEnable()
    {
        if (defaultMainCategory == MainCategory.Passive)
        {
            currentMainCategory = MainCategory.Passive;

            if (passivePanel != null)
                passivePanel.SetActive(true);

            if (activePanel != null)
                activePanel.SetActive(false);

            SetSelectorImmediate(passiveTarget);
        }
        else
        {
            currentMainCategory = MainCategory.Active;

            if (passivePanel != null)
                passivePanel.SetActive(false);

            if (activePanel != null)
                activePanel.SetActive(true);

            SetSelectorImmediate(activeTarget);
        }
    }

    public void OpenPassive()
    {
        if (currentMainCategory == MainCategory.Passive)
            return;

        currentMainCategory = MainCategory.Passive;

        if (passivePanel != null)
            passivePanel.SetActive(true);

        if (activePanel != null)
            activePanel.SetActive(false);

        MoveSelector(passiveTarget);
    }

    public void OpenActive()
    {
        if (currentMainCategory == MainCategory.Active)
            return;

        currentMainCategory = MainCategory.Active;

        if (passivePanel != null)
            passivePanel.SetActive(false);

        if (activePanel != null)
            activePanel.SetActive(true);

        MoveSelector(activeTarget);
    }

    private void MoveSelector(RectTransform target)
    {
        if (selectorFrame == null || target == null)
            return;

        selectorTween?.Kill();

        Vector2 destination = new Vector2(
            target.anchoredPosition.x,
            selectorY
        );

        selectorTween = selectorFrame
            .DOAnchorPos(destination, selectorDuration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    private void SetSelectorImmediate(RectTransform target)
    {
        if (selectorFrame == null || target == null)
            return;

        selectorFrame.anchoredPosition = new Vector2(
            target.anchoredPosition.x,
            selectorY
        );
    }

    private void OnDestroy()
    {
        selectorTween?.Kill();

        if (passiveButton != null)
            passiveButton.onClick.RemoveListener(OpenPassive);

        if (activeButton != null)
            activeButton.onClick.RemoveListener(OpenActive);
    }
}