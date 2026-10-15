using UnityEngine;
using TMPro;
using DG.Tweening;

public class CurrencyUI : MonoBehaviour
{
    [Header("Player Currency")]
    [SerializeField] private PlayerCurrency playerCurrency;

    [Header("Text")]
    [SerializeField] private TMP_Text animaText;
    [SerializeField] private TMP_Text goldText;

    [Header("Icon (����)")]
    [SerializeField] private RectTransform animaIcon;
    [SerializeField] private RectTransform goldIcon;

    [Header("���� ��ȭ ����")]
    [SerializeField] private float countDuration = 0.35f;

    [Header("������ ����")]
    [SerializeField] private float punchScale = 0.12f;
    [SerializeField] private float punchDuration = 0.25f;

    private int lastAnima;
    private int lastGold;

    private int displayedAnima;
    private int displayedGold;

    private Tween animaCountTween;
    private Tween goldCountTween;

    private Vector3 animaIconBaseScale;
    private Vector3 goldIconBaseScale;

    private void Awake()
    {
        if (animaIcon != null)
            animaIconBaseScale = animaIcon.localScale;

        if (goldIcon != null)
            goldIconBaseScale = goldIcon.localScale;
    }

    private void Start()
    {
        // Inspector�� ���� �� ���� ��� �ڵ� Ž��
        if (playerCurrency == null)
        {
            playerCurrency = FindObjectOfType<PlayerCurrency>();
        }       

        if (playerCurrency == null)
        {
            Debug.LogWarning("[CurrencyUI] PlayerCurrency�� ã�� ���߽��ϴ�.");
            return;
        }

        // ���� �� ��� ǥ��
        lastAnima = playerCurrency.anima;
        lastGold = playerCurrency.gold;

        displayedAnima = lastAnima;
        displayedGold = lastGold;

        RefreshText();
    }

    private void Update()
    {
        if (playerCurrency == null)
            return;

        // =========================
        // �ƴϸ� ���� ����
        // =========================
        if (lastAnima != playerCurrency.anima)
        {
            int newValue = playerCurrency.anima;

            AnimateAnima(newValue);

            lastAnima = newValue;
        }

        // =========================
        // ��� ���� ����
        // =========================
        if (lastGold != playerCurrency.gold)
        {
            int newValue = playerCurrency.gold;

            AnimateGold(newValue);

            lastGold = newValue;
        }
    }

    private void AnimateAnima(int targetValue)
    {
        animaCountTween?.Kill();

        int startValue = displayedAnima;

        animaCountTween = DOTween.To(
                () => startValue,
                value =>
                {
                    displayedAnima = value;

                    if (animaText != null)
                        animaText.text = displayedAnima.ToString("N0");
                },
                targetValue,
                countDuration
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                // �� ������ ���� �ݵ�� ���� ��ȭ ������ ����
                displayedAnima = targetValue;

                if (animaText != null)
                    animaText.text = targetValue.ToString("N0");
            });

        PunchIcon(animaIcon, animaIconBaseScale);
    }

    private void AnimateGold(int targetValue)
    {
        goldCountTween?.Kill();

        int startValue = displayedGold;

        goldCountTween = DOTween.To(
                () => startValue,
                value =>
                {
                    displayedGold = value;

                    if (goldText != null)
                        goldText.text = displayedGold.ToString("N0");
                },
                targetValue,
                countDuration
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                // �� ������ �� ���� ����
                displayedGold = targetValue;

                if (goldText != null)
                    goldText.text = targetValue.ToString("N0");
            });

        PunchIcon(goldIcon, goldIconBaseScale);
    }
    private void PunchIcon(
        RectTransform icon,
        Vector3 baseScale)
    {
        if (icon == null)
            return;

        icon.DOKill();

        icon.localScale = baseScale;

        Vector3 punchAmount = new Vector3(
            baseScale.x * punchScale,
            baseScale.y * punchScale,
            0f
        );

        icon
            .DOPunchScale(
                punchAmount,
                punchDuration,
                5,
                0.5f
            )
            .SetUpdate(true);
    }

    private void RefreshText()
    {
        if (animaText != null)
            animaText.text = displayedAnima.ToString("N0");

        if (goldText != null)
            goldText.text = displayedGold.ToString("N0");
    }

    private void OnDestroy()
    {
        animaCountTween?.Kill();
        goldCountTween?.Kill();

        if (animaIcon != null)
            animaIcon.DOKill();

        if (goldIcon != null)
            goldIcon.DOKill();
    }
}