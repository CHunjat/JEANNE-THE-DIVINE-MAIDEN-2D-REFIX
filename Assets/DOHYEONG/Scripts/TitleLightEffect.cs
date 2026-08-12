using UnityEngine;
using DG.Tweening;

public class TitleLightEffect : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private CanvasGroup lightCanvasGroup;
    [SerializeField] private RectTransform lightRect;

    [Header("밝기")]
    [SerializeField] private float minAlpha = 0.06f;
    [SerializeField] private float maxAlpha = 0.14f;

    [Header("호흡 속도")]
    [SerializeField] private float fadeDuration = 3.5f;

    [Header("미세한 크기 변화")]
    [SerializeField] private float maxScale = 1.015f;
    [SerializeField] private float scaleDuration = 5f;

    private Vector3 originalScale;

    private void Awake()
    {
        if (lightRect != null)
        {
            originalScale = lightRect.localScale;
        }
    }

    private void OnEnable()
    {
        PlayLightEffect();
    }

    private void PlayLightEffect()
    {
        KillTweens();

        if (lightCanvasGroup != null)
        {
            // 처음에는 중간 정도 밝기
            lightCanvasGroup.alpha = minAlpha;

            // 아주 천천히 밝아졌다 어두워짐
            lightCanvasGroup
                .DOFade(maxAlpha, fadeDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        if (lightRect != null)
        {
            lightRect.localScale = originalScale;

            // 거의 느껴지지 않을 정도로 퍼졌다 좁아짐
            lightRect
                .DOScale(
                    originalScale * maxScale,
                    scaleDuration
                )
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }
    }

    private void KillTweens()
    {
        if (lightCanvasGroup != null)
        {
            lightCanvasGroup.DOKill();
        }

        if (lightRect != null)
        {
            lightRect.DOKill();
        }
    }

    private void OnDisable()
    {
        KillTweens();
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}