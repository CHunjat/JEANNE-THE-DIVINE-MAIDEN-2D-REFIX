using UnityEngine;
using DG.Tweening;

public class PauseUIAnimator : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private CanvasGroup darkOverlay;
    [SerializeField] private CanvasGroup pauseMenu;
    [SerializeField] private RectTransform pauseMenuRect;

    [Header("배경")]
    [Range(0f, 1f)]
    [SerializeField] private float overlayTargetAlpha = 0.7f;
    [SerializeField] private float overlayFadeDuration = 0.15f;

    [Header("메뉴 등장")]
    [SerializeField] private float startOffsetX = -25f;
    [SerializeField] private float menuDuration = 0.22f;

    private Vector2 originalMenuPosition;

    private void Awake()
    {
        if (pauseMenuRect != null)
            originalMenuPosition = pauseMenuRect.anchoredPosition;
    }

    private void OnEnable()
    {
        PlayOpenAnimation();
    }

    private void PlayOpenAnimation()
    {
        // 이전 Tween 제거
        if (darkOverlay != null)
            darkOverlay.DOKill();

        if (pauseMenu != null)
            pauseMenu.DOKill();

        if (pauseMenuRect != null)
            pauseMenuRect.DOKill();

        // -------------------------
        // 초기 상태
        // -------------------------

        if (darkOverlay != null)
            darkOverlay.alpha = 0f;

        if (pauseMenu != null)
            pauseMenu.alpha = 0f;

        if (pauseMenuRect != null)
        {
            pauseMenuRect.anchoredPosition =
                originalMenuPosition + new Vector2(startOffsetX, 0f);
        }

        // -------------------------
        // 어두운 배경 Fade
        // -------------------------

        if (darkOverlay != null)
        {
            darkOverlay
                .DOFade(overlayTargetAlpha, overlayFadeDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        // -------------------------
        // 메뉴 Fade
        // -------------------------

        if (pauseMenu != null)
        {
            pauseMenu
                .DOFade(1f, menuDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        // -------------------------
        // 메뉴 이동
        // -------------------------

        if (pauseMenuRect != null)
        {
            pauseMenuRect
                .DOAnchorPos(originalMenuPosition, menuDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true);
        }
    }

    private void OnDisable()
    {
        if (darkOverlay != null)
            darkOverlay.DOKill();

        if (pauseMenu != null)
            pauseMenu.DOKill();

        if (pauseMenuRect != null)
        {
            pauseMenuRect.DOKill();
            pauseMenuRect.anchoredPosition = originalMenuPosition;
        }
    }
}