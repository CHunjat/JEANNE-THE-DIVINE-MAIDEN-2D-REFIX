using DG.Tweening;
using UnityEngine;

public class ActivePanelOpenAnimator : MonoBehaviour
{
    [Header("왼쪽 스킬 목록")]
    [SerializeField] private RectTransform[] skillItems;

    [Header("가운데 등록 슬롯")]
    [SerializeField] private RectTransform[] equipSlots;

    [Header("오른쪽 상세 패널")]
    [SerializeField] private RectTransform detailPanel;

    [Header("설정")]
    [SerializeField] private float skillMoveOffset = 18f;
    [SerializeField] private float detailMoveOffset = 22f;

    [SerializeField] private float skillDuration = 0.22f;
    [SerializeField] private float slotDuration = 0.24f;
    [SerializeField] private float detailDuration = 0.28f;

    [SerializeField] private float skillInterval = 0.045f;
    [SerializeField] private float slotInterval = 0.055f;

    private Vector2[] skillOrigins;
    private Vector3[] skillScales;

    private Vector2[] slotOrigins;
    private Vector3[] slotScales;

    private Vector2 detailOrigin;
    private Vector3 detailScale;

    private CanvasGroup[] skillGroups;
    private CanvasGroup[] slotGroups;
    private CanvasGroup detailGroup;

    private Sequence sequence;

    private void Awake()
    {
        // =========================
        // 스킬 목록 원본 저장
        // =========================

        skillOrigins = new Vector2[skillItems.Length];
        skillScales = new Vector3[skillItems.Length];
        skillGroups = new CanvasGroup[skillItems.Length];

        for (int i = 0; i < skillItems.Length; i++)
        {
            if (skillItems[i] == null)
                continue;

            skillOrigins[i] = skillItems[i].anchoredPosition;
            skillScales[i] = skillItems[i].localScale;

            skillGroups[i] = GetOrAddCanvasGroup(skillItems[i]);
        }

        // =========================
        // 등록 슬롯 원본 저장
        // =========================

        slotOrigins = new Vector2[equipSlots.Length];
        slotScales = new Vector3[equipSlots.Length];
        slotGroups = new CanvasGroup[equipSlots.Length];

        for (int i = 0; i < equipSlots.Length; i++)
        {
            if (equipSlots[i] == null)
                continue;

            slotOrigins[i] = equipSlots[i].anchoredPosition;
            slotScales[i] = equipSlots[i].localScale;

            slotGroups[i] = GetOrAddCanvasGroup(equipSlots[i]);
        }

        // =========================
        // DetailPanel
        // =========================

        if (detailPanel != null)
        {
            detailOrigin = detailPanel.anchoredPosition;
            detailScale = detailPanel.localScale;

            detailGroup = GetOrAddCanvasGroup(detailPanel);
        }
    }

    private void OnEnable()
    {
        Play();
    }

    public void Play()
    {
        sequence?.Kill();

        sequence = DOTween.Sequence()
            .SetUpdate(true);

        // =========================
        // 스킬 목록
        // =========================

        for (int i = 0; i < skillItems.Length; i++)
        {
            if (skillItems[i] == null)
                continue;

            RectTransform item = skillItems[i];
            CanvasGroup group = skillGroups[i];

            item.anchoredPosition =
                skillOrigins[i] + Vector2.left * skillMoveOffset;

            item.localScale = skillScales[i];

            group.alpha = 0f;

            float startTime = i * skillInterval;

            sequence.Insert(
                startTime,
                item.DOAnchorPos(
                        skillOrigins[i],
                        skillDuration
                    )
                    .SetEase(Ease.OutCubic)
            );

            sequence.Insert(
                startTime,
                group.DOFade(1f, skillDuration)
            );
        }

        // =========================
        // 등록 슬롯
        // =========================

        float slotStart =
            skillItems.Length * skillInterval + 0.05f;

        for (int i = 0; i < equipSlots.Length; i++)
        {
            if (equipSlots[i] == null)
                continue;

            RectTransform slot = equipSlots[i];
            CanvasGroup group = slotGroups[i];

            slot.anchoredPosition = slotOrigins[i];

            slot.localScale =
                slotScales[i] * 0.82f;

            group.alpha = 0f;

            float startTime =
                slotStart + i * slotInterval;

            sequence.Insert(
                startTime,
                slot.DOScale(
                        slotScales[i],
                        slotDuration
                    )
                    .SetEase(Ease.OutBack, 1.15f)
            );

            sequence.Insert(
                startTime,
                group.DOFade(1f, 0.16f)
            );
        }

        // =========================
        // DetailPanel
        // =========================

        if (detailPanel != null)
        {
            float detailStart =
                slotStart +
                equipSlots.Length * slotInterval +
                0.05f;

            detailPanel.anchoredPosition =
                detailOrigin +
                Vector2.right * detailMoveOffset;

            detailPanel.localScale = detailScale;

            detailGroup.alpha = 0f;

            sequence.Insert(
                detailStart,
                detailPanel.DOAnchorPos(
                        detailOrigin,
                        detailDuration
                    )
                    .SetEase(Ease.OutCubic)
            );

            sequence.Insert(
                detailStart,
                detailGroup.DOFade(
                    1f,
                    detailDuration
                )
            );
        }
    }

    private CanvasGroup GetOrAddCanvasGroup(
        RectTransform target)
    {
        CanvasGroup group =
            target.GetComponent<CanvasGroup>();

        if (group == null)
        {
            group =
                target.gameObject.AddComponent<CanvasGroup>();
        }

        return group;
    }

    private void OnDisable()
    {
        sequence?.Kill();
        sequence = null;
    }
}