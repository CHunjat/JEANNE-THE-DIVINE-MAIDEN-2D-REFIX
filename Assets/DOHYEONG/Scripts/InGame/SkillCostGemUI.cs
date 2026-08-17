using UnityEngine;
using UnityEngine.UI;

public class SkillCostGemUI : MonoBehaviour
{
    [Header("플레이어")]
    [SerializeField] private PlayerStats playerStats;

    [Header("차오르는 보석 이미지 - Slot1 ~ Slot5")]
    [SerializeField] private Image[] fillImages = new Image[5];

    [Header("Glow 이미지 - Slot1 ~ Slot5")]
    [SerializeField] private Image[] glowImages = new Image[5];

    [Header("MP 설정")]
    [SerializeField] private float mpPerSlot = 100f;

    [Header("완충 반복 효과")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float maxScale = 1.08f;

    [Range(0f, 1f)]
    [SerializeField] private float glowMinAlpha = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float glowMaxAlpha = 0.65f;

    private bool[] isFull;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        isFull = new bool[fillImages.Length];
    }

    private void Start()
    {
        if (playerStats == null)
        {
            Debug.LogError(
                "SkillCostGemUI : PlayerStats를 찾을 수 없습니다."
            );

            enabled = false;
            return;
        }

        // Glow 초기화
        for (int i = 0; i < glowImages.Length; i++)
        {
            if (glowImages[i] == null)
                continue;

            glowImages[i].gameObject.SetActive(false);
        }

        RefreshSlots();
    }

    private void Update()
    {
        if (playerStats == null)
            return;

        // MP에 따라 각 슬롯 채우기
        RefreshSlots();

        // 완충된 슬롯 전체를 같은 박자로 움직임
        UpdateGlobalPulse();
    }

    private void RefreshSlots()
    {
        float currentMp = playerStats.currentMp;

        for (int i = 0; i < fillImages.Length; i++)
        {
            Image fill = fillImages[i];

            if (fill == null)
                continue;

            // Slot1 : 0 ~ 100
            // Slot2 : 100 ~ 200
            // Slot3 : 200 ~ 300
            // ...
            float slotStartMp = i * mpPerSlot;

            float progress = Mathf.Clamp01(
                (currentMp - slotStartMp) / mpPerSlot
            );

            // 보석 차오르는 정도
            fill.fillAmount = progress;

            bool nowFull = progress >= 1f;

            // 방금 완충됨
            if (nowFull && !isFull[i])
            {
                ActivateGlow(i);
            }

            // MP 사용으로 완충 상태 해제
            else if (!nowFull && isFull[i])
            {
                DeactivateGlow(i);
            }

            isFull[i] = nowFull;
        }
    }

    private void UpdateGlobalPulse()
    {
        // 0 ~ 1을 반복하는 공통 값
        float pulse =
            (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;

        // 모든 완충 슬롯이 똑같은 Scale 사용
        float scale =
            Mathf.Lerp(1f, maxScale, pulse);

        // 모든 완충 슬롯이 똑같은 Alpha 사용
        float glowAlpha =
            Mathf.Lerp(
                glowMinAlpha,
                glowMaxAlpha,
                pulse
            );

        for (int i = 0; i < fillImages.Length; i++)
        {
            if (fillImages[i] == null)
                continue;

            // =========================
            // 완충된 슬롯
            // =========================
            if (isFull[i])
            {
                fillImages[i].rectTransform.localScale =
                    Vector3.one * scale;

                if (
                    i < glowImages.Length &&
                    glowImages[i] != null
                )
                {
                    glowImages[i].rectTransform.localScale =
                        Vector3.one * scale;

                    SetAlpha(
                        glowImages[i],
                        glowAlpha
                    );
                }
            }

            // =========================
            // 아직 완충되지 않은 슬롯
            // =========================
            else
            {
                fillImages[i].rectTransform.localScale =
                    Vector3.one;

                if (
                    i < glowImages.Length &&
                    glowImages[i] != null
                )
                {
                    glowImages[i].rectTransform.localScale =
                        Vector3.one;

                    SetAlpha(
                        glowImages[i],
                        0f
                    );
                }
            }
        }
    }

    private void ActivateGlow(int index)
    {
        if (
            index < 0 ||
            index >= glowImages.Length
        )
            return;

        if (glowImages[index] == null)
            return;

        glowImages[index].gameObject.SetActive(true);
    }

    private void DeactivateGlow(int index)
    {
        if (
            index < 0 ||
            index >= glowImages.Length
        )
            return;

        if (glowImages[index] == null)
            return;

        glowImages[index].gameObject.SetActive(false);

        if (
            index < fillImages.Length &&
            fillImages[index] != null
        )
        {
            fillImages[index].rectTransform.localScale =
                Vector3.one;
        }
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}