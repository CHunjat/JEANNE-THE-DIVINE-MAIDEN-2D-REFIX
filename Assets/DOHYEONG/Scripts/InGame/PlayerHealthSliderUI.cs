using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthSliderUI : MonoBehaviour
{
    [Header("플레이어 스탯")]
    [SerializeField] private PlayerStats playerStats;

    [Header("체력 Slider")]
    [SerializeField] private Slider currentHpSlider;
    [SerializeField] private Slider recoverHpSlider;

    [Header("리커버리 바 연출")]
    [SerializeField] private float recoverChaseSpeed = 0.5f;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }
    }

    private void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError(
                "PlayerHealthSliderUI: PlayerStats를 찾을 수 없습니다."
            );

            enabled = false;
            return;
        }

        InitializeSlider(currentHpSlider);
        InitializeSlider(recoverHpSlider);

        RefreshImmediately();
    }

    private void Update()
    {
        if (playerStats == null)
        {
            return;
        }

        float maxHp = playerStats.GetMaxHp();

        if (maxHp <= 0f)
        {
            return;
        }

        float currentRatio = Mathf.Clamp01(
            playerStats.currentHp / maxHp
        );

        float recoverRatio = Mathf.Clamp01(
            (playerStats.currentHp +
             playerStats.currentRecoverableHp) / maxHp
        );

        // 빨간 현재 체력은 즉시 반영
        if (currentHpSlider != null)
        {
            currentHpSlider.SetValueWithoutNotify(currentRatio);
        }

        if (recoverHpSlider == null)
        {
            return;
        }

        // 리커버리 영역이 늘어날 때는 즉시 표시
        if (recoverRatio > recoverHpSlider.value)
        {
            recoverHpSlider.SetValueWithoutNotify(recoverRatio);
        }
        else
        {
            // 리커버리 영역이 줄어들 때는 천천히 추적
            float nextValue = Mathf.MoveTowards(
                recoverHpSlider.value,
                recoverRatio,
                recoverChaseSpeed * Time.deltaTime
            );

            recoverHpSlider.SetValueWithoutNotify(nextValue);
        }
    }

    private static void InitializeSlider(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;
    }

    private void RefreshImmediately()
    {
        float maxHp = playerStats.GetMaxHp();

        if (maxHp <= 0f)
        {
            return;
        }

        float currentRatio = Mathf.Clamp01(
            playerStats.currentHp / maxHp
        );

        float recoverRatio = Mathf.Clamp01(
            (playerStats.currentHp +
             playerStats.currentRecoverableHp) / maxHp
        );

        if (currentHpSlider != null)
        {
            currentHpSlider.SetValueWithoutNotify(currentRatio);
        }

        if (recoverHpSlider != null)
        {
            recoverHpSlider.SetValueWithoutNotify(recoverRatio);
        }
    }
}