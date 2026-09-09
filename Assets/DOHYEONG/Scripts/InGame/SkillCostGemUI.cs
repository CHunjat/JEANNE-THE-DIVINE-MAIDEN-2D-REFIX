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

    [Header("완충 반짝임 Material")]
    [Tooltip("완전히 찬 보석에만 적용할 SkillGemShine Material")]
    [SerializeField] private Material fullShineMaterial;

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

    // ★ 원래 Scale 보존
    private Vector3[] fillBaseScales;
    private Vector3[] glowBaseScales;

    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats = FindFirstObjectByType<PlayerStats>();
        }

        isFull = new bool[fillImages.Length];

        fillBaseScales = new Vector3[fillImages.Length];
        glowBaseScales = new Vector3[glowImages.Length];

        // 원래 Fill Scale 저장
        for (int i = 0; i < fillImages.Length; i++)
        {
            if (fillImages[i] != null)
            {
                fillBaseScales[i] =
                    fillImages[i].rectTransform.localScale;
            }
        }

        // 원래 Glow Scale 저장
        for (int i = 0; i < glowImages.Length; i++)
        {
            if (glowImages[i] != null)
            {
                glowBaseScales[i] =
                    glowImages[i].rectTransform.localScale;
            }
        }
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

        // ★ 시작할 때 반짝임 Material 제거
        for (int i = 0; i < fillImages.Length; i++)
        {
            if (fillImages[i] != null)
            {
                fillImages[i].material = null;
            }
        }

        RefreshSlots();
    }

    private void Update()
    {
        if (playerStats == null)
            return;

        RefreshSlots();
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
            // Slot4 : 300 ~ 400
            // Slot5 : 400 ~ 500
            float slotStartMp = i * mpPerSlot;

            float progress = Mathf.Clamp01(
                (currentMp - slotStartMp) / mpPerSlot
            );

            fill.fillAmount = progress;

            // ★ 이 보석이 100% 찼는지
            bool nowFull = progress >= 0.999f;

            // =============================
            // 방금 완충됨
            // =============================
            if (nowFull && !isFull[i])
            {
                ActivateGlow(i);
                ActivateShine(i);
            }

            // =============================
            // MP 사용으로 완충 해제
            // =============================
            else if (!nowFull && isFull[i])
            {
                DeactivateGlow(i);
                DeactivateShine(i);
            }

            isFull[i] = nowFull;
        }
    }

    // =========================================
    // 완충된 보석에만 Shine Material 적용
    // =========================================
    private void ActivateShine(int index)
    {
        if (index < 0 || index >= fillImages.Length)
            return;

        if (fillImages[index] == null)
            return;

        fillImages[index].material = fullShineMaterial;
    }

    private void DeactivateShine(int index)
    {
        if (index < 0 || index >= fillImages.Length)
            return;

        if (fillImages[index] == null)
            return;

        // Unity 기본 UI Material로 복귀
        fillImages[index].material = null;
    }

    private void UpdateGlobalPulse()
    {
        float pulse =
            (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;

        float scale =
            Mathf.Lerp(1f, maxScale, pulse);

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

            if (isFull[i])
            {
                // ★ 원래 Scale 기준으로 확대
                fillImages[i].rectTransform.localScale =
                    fillBaseScales[i] * scale;

                if (
                    i < glowImages.Length &&
                    glowImages[i] != null
                )
                {
                    glowImages[i].rectTransform.localScale =
                        glowBaseScales[i] * scale;

                    SetAlpha(
                        glowImages[i],
                        glowAlpha
                    );
                }
            }
            else
            {
                // ★ Vector3.one이 아니라 원래 Scale 복구
                fillImages[i].rectTransform.localScale =
                    fillBaseScales[i];

                if (
                    i < glowImages.Length &&
                    glowImages[i] != null
                )
                {
                    glowImages[i].rectTransform.localScale =
                        glowBaseScales[i];

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
                fillBaseScales[index];
        }
    }

    private void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }
}