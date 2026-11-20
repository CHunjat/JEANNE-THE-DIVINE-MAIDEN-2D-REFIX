using System.Collections;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class SkillPanelOpenAnimator : MonoBehaviour
{
    [Header("상단 UI")]
    [SerializeField] private RectTransform title;
    [SerializeField] private RectTransform currency;
    [SerializeField] private RectTransform mainTab;

    [Header("스킬 콘텐츠")]
    [Tooltip("처음 스킬창을 열었을 때 보여지는 PassivePanel 또는 ActivePanel")]
    [SerializeField] private RectTransform contentRoot;

    [Header("하단 UI")]
    [SerializeField] private RectTransform exitGuide;

    [Header("기본 Animation")]
    [SerializeField] private float fadeDuration = 0.18f;
    [SerializeField] private float moveDuration = 0.32f;

    [SerializeField] private float horizontalOffset = 35f;
    [SerializeField] private float verticalOffset = 20f;

    [Header("Content Slide Animation")]
    [Tooltip("스킬 콘텐츠가 아래에서 올라오는 거리")]
    [SerializeField] private float contentSlideDistance = 850f;

    [Tooltip("스킬 콘텐츠가 올라오는 시간")]
    [SerializeField] private float contentMoveDuration = 0.45f;

    [Tooltip("스킬 콘텐츠 페이드 시간")]
    [SerializeField] private float contentFadeDuration = 0.35f;

    private CanvasGroup rootCanvasGroup;

    private CanvasGroup titleGroup;
    private CanvasGroup currencyGroup;
    private CanvasGroup tabGroup;
    private CanvasGroup contentGroup;
    private CanvasGroup exitGroup;

    private Vector2 titleOrigin;
    private Vector2 currencyOrigin;
    private Vector2 tabOrigin;
    private Vector2 contentOrigin;
    private Vector2 exitOrigin;

    private Sequence openSequence;

    private void Awake()
    {
        rootCanvasGroup = GetComponent<CanvasGroup>();

        titleGroup = GetOrAddCanvasGroup(title);
        currencyGroup = GetOrAddCanvasGroup(currency);
        tabGroup = GetOrAddCanvasGroup(mainTab);
        contentGroup = GetOrAddCanvasGroup(contentRoot);
        exitGroup = GetOrAddCanvasGroup(exitGuide);

        if (title != null)
            titleOrigin = title.anchoredPosition;

        if (currency != null)
            currencyOrigin = currency.anchoredPosition;

        if (mainTab != null)
            tabOrigin = mainTab.anchoredPosition;

        if (contentRoot != null)
            contentOrigin = contentRoot.anchoredPosition;

        if (exitGuide != null)
            exitOrigin = exitGuide.anchoredPosition;
    }

    private void OnEnable()
    {
        StartCoroutine(PlayNextFrame());
    }

    private IEnumerator PlayNextFrame()
    {
        // SkillMenuCategoryController의 OnEnable이
        // 먼저 Passive / Active 패널 상태를 결정하도록 한 프레임 대기
        yield return null;

        PlayOpenAnimation();
    }

    private void PlayOpenAnimation()
    {
        openSequence?.Kill();

        // =========================
        // 초기 상태
        // =========================

        rootCanvasGroup.alpha = 0f;
        rootCanvasGroup.interactable = true;
        rootCanvasGroup.blocksRaycasts = true;

        // Skills 제목
        if (title != null)
        {
            title.anchoredPosition =
                titleOrigin + Vector2.left * horizontalOffset;

            titleGroup.alpha = 0f;
        }

        // 재화
        if (currency != null)
        {
            currency.anchoredPosition =
                currencyOrigin + Vector2.right * horizontalOffset;

            currencyGroup.alpha = 0f;
        }

        // Passive / Active 탭
        if (mainTab != null)
        {
            mainTab.anchoredPosition =
                tabOrigin + Vector2.up * verticalOffset;

            tabGroup.alpha = 0f;
        }

        // 스킬 콘텐츠
        // 화면 아래쪽에서 시작
        if (contentRoot != null)
        {
            contentRoot.anchoredPosition =
                contentOrigin + Vector2.down * contentSlideDistance;

            contentGroup.alpha = 0f;
        }

        if (exitGuide != null)
        {
            exitGuide.anchoredPosition = exitOrigin;
            exitGroup.alpha = 1f;
        }

        // =========================
        // 오픈 Sequence
        // =========================

        openSequence = DOTween.Sequence()
            .SetUpdate(true);

        // 전체 화면 Fade In
        openSequence.Append(
            rootCanvasGroup
                .DOFade(1f, fadeDuration)
                .SetEase(Ease.OutQuad)
        );

        // Skills 제목
        if (title != null)
        {
            openSequence.Insert(
                0.06f,
                title
                    .DOAnchorPos(titleOrigin, moveDuration)
                    .SetEase(Ease.OutCubic)
            );

            openSequence.Insert(
                0.06f,
                titleGroup.DOFade(1f, 0.22f)
            );
        }

        // 재화
        if (currency != null)
        {
            openSequence.Insert(
                0.09f,
                currency
                    .DOAnchorPos(currencyOrigin, moveDuration)
                    .SetEase(Ease.OutCubic)
            );

            openSequence.Insert(
                0.09f,
                currencyGroup.DOFade(1f, 0.22f)
            );
        }

        // Passive / Active 탭
        if (mainTab != null)
        {
            openSequence.Insert(
                0.14f,
                mainTab
                    .DOAnchorPos(tabOrigin, moveDuration)
                    .SetEase(Ease.OutCubic)
            );

            openSequence.Insert(
                0.14f,
                tabGroup.DOFade(1f, 0.22f)
            );
        }

        // =========================
        // 스킬 콘텐츠
        // 아래 → 위로 부드럽게 등장
        // =========================

        if (contentRoot != null)
        {
            openSequence.Insert(
                0.20f,
                contentRoot
                    .DOAnchorPos(contentOrigin, contentMoveDuration)
                    .SetEase(Ease.OutQuart)
            );

            openSequence.Insert(
                0.20f,
                contentGroup
                    .DOFade(1f, contentFadeDuration)
                    .SetEase(Ease.OutQuad)
            );
        }

    }

    private CanvasGroup GetOrAddCanvasGroup(RectTransform target)
    {
        if (target == null)
            return null;

        CanvasGroup group = target.GetComponent<CanvasGroup>();

        if (group == null)
            group = target.gameObject.AddComponent<CanvasGroup>();

        return group;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        openSequence?.Kill();
        openSequence = null;

        // 다시 열었을 때 위치가 꼬이지 않도록 원래 위치 복구
        if (title != null)
            title.anchoredPosition = titleOrigin;

        if (currency != null)
            currency.anchoredPosition = currencyOrigin;

        if (mainTab != null)
            mainTab.anchoredPosition = tabOrigin;

        if (contentRoot != null)
            contentRoot.anchoredPosition = contentOrigin;

        if (exitGuide != null)
            exitGuide.anchoredPosition = exitOrigin;
    }
}