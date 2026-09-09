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

    [Header("현재 처음 보여줄 콘텐츠")]
    [Tooltip("처음 스킬창을 열었을 때 켜져있는 PassivePanel 또는 ActivePanel")]
    [SerializeField] private RectTransform contentRoot;

    [Header("하단 UI")]
    [SerializeField] private RectTransform exitGuide;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.18f;
    [SerializeField] private float moveDuration = 0.32f;

    [SerializeField] private float horizontalOffset = 35f;
    [SerializeField] private float verticalOffset = 20f;

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
        // SkillMenuCategoryController의 OnEnable 처리가
        // 끝난 뒤 연출 시작
        yield return null;

        PlayOpenAnimation();
    }

    private void PlayOpenAnimation()
    {
        openSequence?.Kill();

        // =========================
        // 시작 상태
        // =========================

        rootCanvasGroup.alpha = 0f;
        rootCanvasGroup.interactable = true;
        rootCanvasGroup.blocksRaycasts = true;

        if (title != null)
        {
            title.anchoredPosition =
                titleOrigin + Vector2.left * horizontalOffset;

            titleGroup.alpha = 0f;
        }

        if (currency != null)
        {
            currency.anchoredPosition =
                currencyOrigin + Vector2.right * horizontalOffset;

            currencyGroup.alpha = 0f;
        }

        if (mainTab != null)
        {
            mainTab.anchoredPosition =
                tabOrigin + Vector2.up * verticalOffset;

            tabGroup.alpha = 0f;
        }

        if (contentRoot != null)
        {
            contentRoot.anchoredPosition =
                contentOrigin + Vector2.down * verticalOffset;

            contentGroup.alpha = 0f;
        }

        if (exitGuide != null)
        {
            exitGuide.anchoredPosition = exitOrigin;
            exitGroup.alpha = 0f;
        }

        // =========================
        // 등장 Sequence
        // =========================

        openSequence = DOTween.Sequence()
            .SetUpdate(true);

        // 전체 화면
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
                title.DOAnchorPos(titleOrigin, moveDuration)
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
                currency.DOAnchorPos(currencyOrigin, moveDuration)
                    .SetEase(Ease.OutCubic)
            );

            openSequence.Insert(
                0.09f,
                currencyGroup.DOFade(1f, 0.22f)
            );
        }

        // Passive / Active
        if (mainTab != null)
        {
            openSequence.Insert(
                0.14f,
                mainTab.DOAnchorPos(tabOrigin, moveDuration)
                    .SetEase(Ease.OutCubic)
            );

            openSequence.Insert(
                0.14f,
                tabGroup.DOFade(1f, 0.22f)
            );
        }

        // 메인 콘텐츠
        if (contentRoot != null)
        {
            openSequence.Insert(
                0.22f,
                contentRoot.DOAnchorPos(contentOrigin, 0.38f)
                    .SetEase(Ease.OutCubic)
            );

            openSequence.Insert(
                0.22f,
                contentGroup.DOFade(1f, 0.28f)
            );
        }

        // ESC
        if (exitGuide != null)
        {
            openSequence.Insert(
                0.34f,
                exitGroup.DOFade(1f, 0.22f)
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
    }
}