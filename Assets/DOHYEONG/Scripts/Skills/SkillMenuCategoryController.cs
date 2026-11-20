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

    [Header("카테고리 버튼")]
    [SerializeField] private Button passiveButton;
    [SerializeField] private Button activeButton;

    [Header("카테고리 패널")]
    [SerializeField] private GameObject passivePanel;
    [SerializeField] private GameObject activePanel;

    [Header("Passive 분할 패널")]
    [Tooltip("패시브 화면의 왼쪽 영역")]
    [SerializeField] private RectTransform passiveLeftPanel;

    [Tooltip("패시브 화면의 오른쪽 영역")]
    [SerializeField] private RectTransform passiveRightPanel;

    [Header("선택 프레임")]
    [SerializeField] private RectTransform selectorFrame;
    [SerializeField] private RectTransform passiveTarget;
    [SerializeField] private RectTransform activeTarget;

    [Header("선택 프레임 이동")]
    [SerializeField] private float selectorDuration = 0.22f;

    [Header("Active 세로 전환")]
    [SerializeField] private float activeSlideDistance = 850f;
    [SerializeField] private float activeOutDuration = 0.30f;
    [SerializeField] private float activeInDuration = 0.40f;

    [Header("Passive 좌우 전환")]
    [SerializeField] private float passiveSplitDistance = 650f;
    [SerializeField] private float passiveOutDuration = 0.35f;
    [SerializeField] private float passiveInDuration = 0.42f;

    [Header("처음 선택할 탭")]
    [SerializeField]
    private MainCategory defaultMainCategory = MainCategory.Active;

    private MainCategory currentMainCategory;

    private RectTransform activeRect;

    private Vector2 activeOrigin;

    private Vector2 passiveLeftOrigin;
    private Vector2 passiveRightOrigin;

    private Tween selectorTween;
    private Sequence transitionSequence;

    private float selectorY;

    private bool isTransitioning;

    private void Awake()
    {
        // 버튼 등록
        if (passiveButton != null)
            passiveButton.onClick.AddListener(OpenPassive);

        if (activeButton != null)
            activeButton.onClick.AddListener(OpenActive);

        // 선택 프레임 Y 위치 저장
        if (selectorFrame != null)
            selectorY = selectorFrame.anchoredPosition.y;

        // Active 원래 위치 저장
        if (activePanel != null)
        {
            activeRect = activePanel.GetComponent<RectTransform>();

            if (activeRect != null)
                activeOrigin = activeRect.anchoredPosition;
        }

        // Passive 왼쪽 원래 위치 저장
        if (passiveLeftPanel != null)
            passiveLeftOrigin = passiveLeftPanel.anchoredPosition;

        // Passive 오른쪽 원래 위치 저장
        if (passiveRightPanel != null)
            passiveRightOrigin = passiveRightPanel.anchoredPosition;
    }

    private void OnEnable()
    {
        transitionSequence?.Kill();

        isTransitioning = false;

        // 위치 초기화
        ResetPanelPositions();

        // 처음 열릴 카테고리 설정
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

    // =========================
    // Passive 열기
    // =========================

    public void OpenPassive()
    {
        if (currentMainCategory == MainCategory.Passive)
            return;

        if (isTransitioning)
            return;

        MoveSelector(passiveTarget);

        PlayActiveToPassive();
    }

    // =========================
    // Active 열기
    // =========================

    public void OpenActive()
    {
        if (currentMainCategory == MainCategory.Active)
            return;

        if (isTransitioning)
            return;

        MoveSelector(activeTarget);

        PlayPassiveToActive();
    }

    // =========================
    // Active → Passive
    // =========================

    private void PlayActiveToPassive()
    {
        if (activeRect == null)
            return;

        isTransitioning = true;

        transitionSequence?.Kill();

        transitionSequence = DOTween.Sequence()
            .SetUpdate(true);

        // -------------------------
        // 1. Active 아래로 퇴장
        // -------------------------

        transitionSequence.Append(
            activeRect
                .DOAnchorPosY(
                    activeOrigin.y - activeSlideDistance,
                    activeOutDuration
                )
                .SetEase(Ease.InOutSine)
        );

        // -------------------------
        // 2. Passive 준비
        // -------------------------

        transitionSequence.AppendCallback(() =>
        {
            if (activePanel != null)
                activePanel.SetActive(false);

            // Active 위치 복구
            activeRect.anchoredPosition = activeOrigin;

            if (passivePanel != null)
                passivePanel.SetActive(true);

            // Passive를 양쪽 바깥에 배치
            if (passiveLeftPanel != null)
            {
                passiveLeftPanel.anchoredPosition =
                    passiveLeftOrigin +
                    Vector2.left * passiveSplitDistance;
            }

            if (passiveRightPanel != null)
            {
                passiveRightPanel.anchoredPosition =
                    passiveRightOrigin +
                    Vector2.right * passiveSplitDistance;
            }

            currentMainCategory = MainCategory.Passive;
        });

        // -------------------------
        // 3. Passive 좌우에서 등장
        // -------------------------

        if (passiveLeftPanel != null)
        {
            transitionSequence.Join(
                passiveLeftPanel
                    .DOAnchorPos(
                        passiveLeftOrigin,
                        passiveInDuration
                    )
                    .SetEase(Ease.OutQuart)
            );
        }

        if (passiveRightPanel != null)
        {
            transitionSequence.Join(
                passiveRightPanel
                    .DOAnchorPos(
                        passiveRightOrigin,
                        passiveInDuration
                    )
                    .SetEase(Ease.OutQuart)
            );
        }

        transitionSequence.OnComplete(() =>
        {
            isTransitioning = false;
        });
    }

    // =========================
    // Passive → Active
    // =========================

    private void PlayPassiveToActive()
    {
        isTransitioning = true;

        transitionSequence?.Kill();

        transitionSequence = DOTween.Sequence()
            .SetUpdate(true);

        // -------------------------
        // 1. Passive 좌우로 퇴장
        // -------------------------

        if (passiveLeftPanel != null)
        {
            transitionSequence.Join(
                passiveLeftPanel
                    .DOAnchorPos(
                        passiveLeftOrigin +
                        Vector2.left * passiveSplitDistance,
                        passiveOutDuration
                    )
                    .SetEase(Ease.InOutSine)
            );
        }

        if (passiveRightPanel != null)
        {
            transitionSequence.Join(
                passiveRightPanel
                    .DOAnchorPos(
                        passiveRightOrigin +
                        Vector2.right * passiveSplitDistance,
                        passiveOutDuration
                    )
                    .SetEase(Ease.InOutSine)
            );
        }

        // -------------------------
        // 2. Active 준비
        // -------------------------

        transitionSequence.AppendCallback(() =>
        {
            if (passivePanel != null)
                passivePanel.SetActive(false);

            // Passive 위치 복구
            if (passiveLeftPanel != null)
                passiveLeftPanel.anchoredPosition =
                    passiveLeftOrigin;

            if (passiveRightPanel != null)
                passiveRightPanel.anchoredPosition =
                    passiveRightOrigin;

            if (activePanel != null)
                activePanel.SetActive(true);

            // Active를 화면 아래에 준비
            if (activeRect != null)
            {
                activeRect.anchoredPosition =
                    activeOrigin +
                    Vector2.down * activeSlideDistance;
            }

            currentMainCategory = MainCategory.Active;
        });

        // -------------------------
        // 3. Active 아래에서 등장
        // -------------------------

        if (activeRect != null)
        {
            transitionSequence.Append(
                activeRect
                    .DOAnchorPos(
                        activeOrigin,
                        activeInDuration
                    )
                    .SetEase(Ease.OutQuart)
            );
        }

        transitionSequence.OnComplete(() =>
        {
            isTransitioning = false;
        });
    }

    // =========================
    // Selector
    // =========================

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

    // =========================
    // 위치 초기화
    // =========================

    private void ResetPanelPositions()
    {
        if (activeRect != null)
            activeRect.anchoredPosition = activeOrigin;

        if (passiveLeftPanel != null)
            passiveLeftPanel.anchoredPosition =
                passiveLeftOrigin;

        if (passiveRightPanel != null)
            passiveRightPanel.anchoredPosition =
                passiveRightOrigin;
    }

    private void OnDisable()
    {
        selectorTween?.Kill();
        transitionSequence?.Kill();

        selectorTween = null;
        transitionSequence = null;

        ResetPanelPositions();

        isTransitioning = false;
    }

    private void OnDestroy()
    {
        selectorTween?.Kill();
        transitionSequence?.Kill();

        if (passiveButton != null)
            passiveButton.onClick.RemoveListener(OpenPassive);

        if (activeButton != null)
            activeButton.onClick.RemoveListener(OpenActive);
    }
}