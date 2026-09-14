using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class IntroTitleScreen : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private PlayerController playerController;

    [Header("다음 화면")]
    [SerializeField] private GameObject mainScreen;

    [Header("인트로 UI")]
    [SerializeField] private CanvasGroup entireScreenCanvasGroup;

    [SerializeField] private CanvasGroup logoCanvasGroup;
    [SerializeField] private RectTransform logoRect;

    [SerializeField] private CanvasGroup logoGlowCanvasGroup;
    [SerializeField] private RectTransform logoGlowRect;

    [SerializeField] private CanvasGroup pressAnyKeyCanvasGroup;

    [Header("로고 등장")]
    [SerializeField] private float logoFadeDuration = 2.2f;
    [SerializeField] private float logoStartScale = 0.98f;

    [Header("로고 후광")]
    [SerializeField] private float glowFadeDuration = 2.8f;
    [SerializeField] private float glowMaxAlpha = 0.22f;
    [SerializeField] private float glowStartScale = 0.94f;
    [SerializeField] private float glowMinAlpha = 0.10f;
    [SerializeField] private float glowBlinkDuration = 1.8f;

    [Header("PRESS ANY KEY")]
    [SerializeField] private float pressDelay = 0.45f;
    [SerializeField] private float pressFadeDuration = 0.7f;

    [Header("PRESS 점멸")]
    [SerializeField] private float pressBlinkMinAlpha = 0.35f;
    [SerializeField] private float pressBlinkDuration = 0.8f;

    [Header("인트로 종료")]
    [SerializeField] private float screenFadeOutDuration = 0.35f;

    private Sequence introSequence;

    private bool canInput;
    private bool isClosing;

    private void Awake()
    {
        // Inspector에서 안 넣었을 경우 자동 탐색
        if (playerController == null)
        {
            playerController =
                FindFirstObjectByType<PlayerController>();
        }
    }

    private void OnEnable()
    {
        // =========================
        // 인트로에서는 플레이어 조작 차단
        // =========================
        if (playerController != null)
        {
            playerController.canControl = false;
        }

        PlayIntro();
    }

    private void Update()
    {
        if (!canInput || isClosing)
            return;

        // =========================
        // 키보드 입력
        // =========================
        bool keyboardInput =
            Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame;

        // =========================
        // 마우스 클릭
        // =========================
        bool mouseInput =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        // =========================
        // 터치 입력
        // =========================
        bool touchInput =
            Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        if (keyboardInput || mouseInput || touchInput)
        {
            CloseIntro();
        }
    }

    private void PlayIntro()
    {
        KillTweens();

        canInput = false;
        isClosing = false;

        // =========================
        // 초기 상태
        // =========================

        if (entireScreenCanvasGroup != null)
        {
            entireScreenCanvasGroup.alpha = 1f;
        }

        // 로고
        if (logoCanvasGroup != null)
        {
            logoCanvasGroup.alpha = 0f;
        }

        if (logoRect != null)
        {
            logoRect.localScale =
                Vector3.one * logoStartScale;
        }

        // 로고 후광
        if (logoGlowCanvasGroup != null)
        {
            logoGlowCanvasGroup.alpha = 0f;
        }

        if (logoGlowRect != null)
        {
            logoGlowRect.localScale =
                Vector3.one * glowStartScale;
        }

        // PRESS ANY KEY
        if (pressAnyKeyCanvasGroup != null)
        {
            pressAnyKeyCanvasGroup.alpha = 0f;
        }

        // =========================
        // 로고 후광
        // =========================

        if (logoGlowCanvasGroup != null)
        {
            logoGlowCanvasGroup
                .DOFade(glowMaxAlpha, glowFadeDuration)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    StartGlowBlink();
                });
        }

        if (logoGlowRect != null)
        {
            logoGlowRect
                .DOScale(
                    Vector3.one,
                    glowFadeDuration
                )
                .SetEase(Ease.OutSine)
                .SetUpdate(true);
        }

        // =========================
        // 메인 인트로 Sequence
        // =========================

        introSequence = DOTween.Sequence();

        // 1. 로고 등장
        if (logoCanvasGroup != null)
        {
            introSequence.Append(
                logoCanvasGroup
                    .DOFade(
                        1f,
                        logoFadeDuration
                    )
                    .SetEase(Ease.InOutSine)
            );
        }

        if (logoRect != null)
        {
            introSequence.Join(
                logoRect
                    .DOScale(
                        Vector3.one,
                        logoFadeDuration
                    )
                    .SetEase(Ease.OutSine)
            );
        }

        // 2. 잠시 대기
        introSequence.AppendInterval(
            pressDelay
        );

        // 3. PRESS ANY KEY 등장
        if (pressAnyKeyCanvasGroup != null)
        {
            introSequence.Append(
                pressAnyKeyCanvasGroup
                    .DOFade(
                        1f,
                        pressFadeDuration
                    )
                    .SetEase(Ease.InOutSine)
            );
        }

        // 4. 입력 허용
        introSequence.AppendCallback(() =>
        {
            canInput = true;

            StartPressBlink();
        });

        introSequence.SetUpdate(true);
    }

    private void StartPressBlink()
    {
        if (pressAnyKeyCanvasGroup == null)
            return;

        pressAnyKeyCanvasGroup.DOKill();

        pressAnyKeyCanvasGroup.alpha = 1f;

        pressAnyKeyCanvasGroup
            .DOFade(
                pressBlinkMinAlpha,
                pressBlinkDuration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(
                -1,
                LoopType.Yoyo
            )
            .SetUpdate(true);
    }

    private void StartGlowBlink()
    {
        if (logoGlowCanvasGroup == null)
            return;

        logoGlowCanvasGroup.DOKill();

        logoGlowCanvasGroup.alpha = glowMaxAlpha;

        logoGlowCanvasGroup
            .DOFade(
                glowMinAlpha,
                glowBlinkDuration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(
                -1,
                LoopType.Yoyo
            )
            .SetUpdate(true);
    }

    private void CloseIntro()
    {
        if (isClosing)
            return;

        isClosing = true;
        canInput = false;

        KillTweens();

        // =========================
        // 메인 화면 활성화
        // =========================

        if (mainScreen != null)
        {
            mainScreen.SetActive(true);
        }

        // 중요:
        // 메인화면도 UI이므로
        // 여기서는 canControl을 true로 돌리지 않는다.

        if (entireScreenCanvasGroup == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // =========================
        // Intro 전체 Fade Out
        // =========================

        entireScreenCanvasGroup
            .DOFade(
                0f,
                screenFadeOutDuration
            )
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }

    private void KillTweens()
    {
        introSequence?.Kill();
        introSequence = null;

        if (entireScreenCanvasGroup != null)
        {
            entireScreenCanvasGroup.DOKill();
        }

        if (logoCanvasGroup != null)
        {
            logoCanvasGroup.DOKill();
        }

        if (logoRect != null)
        {
            logoRect.DOKill();
        }

        if (logoGlowCanvasGroup != null)
        {
            logoGlowCanvasGroup.DOKill();
        }

        if (logoGlowRect != null)
        {
            logoGlowRect.DOKill();
        }

        if (pressAnyKeyCanvasGroup != null)
        {
            pressAnyKeyCanvasGroup.DOKill();
        }
    }

    private void OnDisable()
    {
        KillTweens();
    }
}