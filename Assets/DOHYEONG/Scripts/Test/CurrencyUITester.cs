using UnityEngine;
using TMPro;
using DG.Tweening;

public class CurrencyUITester : MonoBehaviour
{
    [Header("아니마 UI")]
    [SerializeField] private TextMeshProUGUI animaText;
    [SerializeField] private RectTransform animaIcon;

    [Header("골드 UI")]
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private RectTransform goldIcon;

    [Header("테스트 값")]
    [SerializeField] private int startAnima = 1000;
    [SerializeField] private int startGold = 5000;

    [SerializeField] private int animaTestGain = 12345;
    [SerializeField] private int goldTestGain = 25000;

    [Header("테스트 키")]
    [SerializeField] private KeyCode animaTestKey = KeyCode.F7;
    [SerializeField] private KeyCode goldTestKey = KeyCode.F8;

    [Header("획득 연출")]
    [SerializeField] private float countDuration = 0.4f;
    [SerializeField] private float iconPunchScale = 0.2f;
    [SerializeField] private float textPunchScale = 0.08f;
    [SerializeField] private float punchDuration = 0.25f;

    private int currentAnima;
    private int currentGold;

    private Tween animaCountTween;
    private Tween goldCountTween;

    private Vector3 animaIconBaseScale;
    private Vector3 goldIconBaseScale;

    private Vector3 animaTextBaseScale;
    private Vector3 goldTextBaseScale;

    private void Start()
    {
        currentAnima = startAnima;
        currentGold = startGold;

        if (animaIcon != null)
            animaIconBaseScale = animaIcon.localScale;

        if (goldIcon != null)
            goldIconBaseScale = goldIcon.localScale;

        if (animaText != null)
            animaTextBaseScale = animaText.rectTransform.localScale;

        if (goldText != null)
            goldTextBaseScale = goldText.rectTransform.localScale;

        RefreshTexts();
    }

    private void Update()
    {
        if (Input.GetKeyDown(animaTestKey))
        {
            AddAnima(animaTestGain);
        }

        if (Input.GetKeyDown(goldTestKey))
        {
            AddGold(goldTestGain);
        }
    }

    public void AddAnima(int amount)
    {
        int startValue = currentAnima;
        int targetValue = Mathf.Min(currentAnima + amount, 999999999);

        currentAnima = targetValue;

        animaCountTween?.Kill();

        int displayValue = startValue;

        animaCountTween = DOTween.To(
            () => displayValue,
            value =>
            {
                displayValue = value;

                if (animaText != null)
                    animaText.text = displayValue.ToString("N0");
            },
            targetValue,
            countDuration
        )
        .SetEase(Ease.OutCubic)
        .SetUpdate(true);

        PlayGainEffect(animaIcon, animaIconBaseScale,
                       animaText, animaTextBaseScale);
    }

    public void AddGold(int amount)
    {
        int startValue = currentGold;
        int targetValue = Mathf.Min(currentGold + amount, 999999999);

        currentGold = targetValue;

        goldCountTween?.Kill();

        int displayValue = startValue;

        goldCountTween = DOTween.To(
            () => displayValue,
            value =>
            {
                displayValue = value;

                if (goldText != null)
                    goldText.text = displayValue.ToString("N0");
            },
            targetValue,
            countDuration
        )
        .SetEase(Ease.OutCubic)
        .SetUpdate(true);

        PlayGainEffect(goldIcon, goldIconBaseScale,
                       goldText, goldTextBaseScale);
    }

    private void PlayGainEffect(
        RectTransform icon,
        Vector3 iconBaseScale,
        TextMeshProUGUI text,
        Vector3 textBaseScale)
    {
        if (icon != null)
        {
            icon.DOKill();
            icon.localScale = iconBaseScale;

            icon.DOPunchScale(
                iconBaseScale * iconPunchScale,
                punchDuration,
                5,
                0.5f
            )
            .SetUpdate(true);
        }

        if (text != null)
        {
            RectTransform textRect = text.rectTransform;

            textRect.DOKill();
            textRect.localScale = textBaseScale;

            textRect.DOPunchScale(
                textBaseScale * textPunchScale,
                punchDuration,
                4,
                0.5f
            )
            .SetUpdate(true);
        }
    }

    private void RefreshTexts()
    {
        if (animaText != null)
            animaText.text = currentAnima.ToString("N0");

        if (goldText != null)
            goldText.text = currentGold.ToString("N0");
    }

    private void OnDestroy()
    {
        animaCountTween?.Kill();
        goldCountTween?.Kill();

        if (animaIcon != null)
            animaIcon.DOKill();

        if (goldIcon != null)
            goldIcon.DOKill();

        if (animaText != null)
            animaText.rectTransform.DOKill();

        if (goldText != null)
            goldText.rectTransform.DOKill();
    }
}