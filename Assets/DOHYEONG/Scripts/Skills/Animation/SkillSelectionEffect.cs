using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class SkillSelectionEffect : MonoBehaviour
{
    [Header("선택 등장 효과")]
    [SerializeField] private float startScale = 0.96f;
    [SerializeField] private float punchScale = 1.04f;

    [SerializeField] private float appearDuration = 0.18f;
    [SerializeField] private float settleDuration = 0.15f;

    [Header("밝기")]
    [Range(0f, 1f)]
    [SerializeField] private float idleAlpha = 0.72f;

    [Range(0f, 1f)]
    [SerializeField] private float flashAlpha = 1f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector3 baseScale;

    private Sequence effectSequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        baseScale = rectTransform.localScale;

        // 처음에는 숨김
        canvasGroup.alpha = 0f;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        KillTweens();

        // 시작 상태
        rectTransform.localScale =
            baseScale * startScale;

        canvasGroup.alpha = 0f;

        effectSequence = DOTween.Sequence();

        // 1. 빠르게 나타나면서 살짝 커짐
        effectSequence.Append(
            rectTransform
                .DOScale(
                    baseScale * punchScale,
                    appearDuration
                )
                .SetEase(Ease.OutCubic)
        );

        effectSequence.Join(
            canvasGroup
                .DOFade(
                    flashAlpha,
                    appearDuration
                )
                .SetEase(Ease.OutCubic)
        );

        // 2. 원래 크기로 안착
        effectSequence.Append(
            rectTransform
                .DOScale(
                    baseScale,
                    settleDuration
                )
                .SetEase(Ease.OutSine)
        );

        // 3. 너무 밝지 않게 살짝 내려옴
        effectSequence.Join(
            canvasGroup
                .DOFade(
                    idleAlpha,
                    settleDuration
                )
                .SetEase(Ease.OutSine)
        );

        effectSequence.SetUpdate(true);
    }

    public void Hide()
    {
        KillTweens();

        canvasGroup.alpha = 0f;
        rectTransform.localScale = baseScale;

        gameObject.SetActive(false);
    }

    private void KillTweens()
    {
        effectSequence?.Kill();
        effectSequence = null;

        rectTransform.DOKill();
        canvasGroup.DOKill();
    }

    private void OnDisable()
    {
        KillTweens();
    }
}