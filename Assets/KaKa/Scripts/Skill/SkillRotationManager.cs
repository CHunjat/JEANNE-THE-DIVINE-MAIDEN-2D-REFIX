using DG.Tweening;
using TMPro;
using UnityEngine;

public class SkillRotationManager : MonoBehaviour
{
    [Header("Skill Data")]
    public SkillData[] skills = new SkillData[3];

    [Header("Skill UI")]
    [Tooltip("Skill_1, Skill_2, Skill_3 순서대로 연결")]
    public RectTransform[] skillSlots = new RectTransform[3];

    [Header("Connection")]
    public PlayerController playerController;

    [Header("Carousel Position")]
    [Tooltip("현재 선택된 스킬 위치")]
    [SerializeField] private Vector2 centerPosition = Vector2.zero;

    [Tooltip("왼쪽 슬롯 위치")]
    [SerializeField] private Vector2 leftPosition = new Vector2(-65f, 8f);

    [Tooltip("오른쪽 슬롯 위치")]
    [SerializeField] private Vector2 rightPosition = new Vector2(65f, 8f);

    [Header("Carousel Scale")]
    [SerializeField] private float centerScale = 1f;
    [SerializeField] private float sideScale = 0.7f;

    [Header("Carousel Alpha")]
    [Range(0f, 1f)]
    [SerializeField] private float centerAlpha = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float sideAlpha = 0.3f;

    [Header("Animation")]
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private Ease moveEase = Ease.OutCubic;

    [Header("Fixed UI")]
    public TextMeshProUGUI fixedNeedCountText;

    private SkillSlot[] skillSlotScripts;
    private CanvasGroup[] canvasGroups;

    private PlayerController.SkillSlot lastKnownSkillSlot;

    private void Awake()
    {
        int count = skillSlots.Length;

        skillSlotScripts = new SkillSlot[count];
        canvasGroups = new CanvasGroup[count];

        for (int i = 0; i < count; i++)
        {
            if (skillSlots[i] == null)
                continue;

            // 기존 SkillSlot 스크립트 찾기
            skillSlotScripts[i] =
                skillSlots[i].GetComponentInChildren<SkillSlot>();

            // CanvasGroup이 없으면 자동 생성
            canvasGroups[i] =
                skillSlots[i].GetComponent<CanvasGroup>();

            if (canvasGroups[i] == null)
            {
                canvasGroups[i] =
                    skillSlots[i].gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        if (playerController == null)
        {
            Debug.LogError(
                "SkillRotationManager : PlayerController가 연결되지 않았습니다."
            );

            return;
        }

        lastKnownSkillSlot =
            playerController.currentSkillSlot;

        UpdateAllSlotsUI();

        // 시작할 때는 애니메이션 없이 바로 위치
        UpdateCarousel(true);

        UpdateCostText();
    }

    private void Update()
    {
        if (playerController == null)
            return;

        // 선택 슬롯이 변경됨
        if (playerController.currentSkillSlot != lastKnownSkillSlot)
        {
            lastKnownSkillSlot =
                playerController.currentSkillSlot;

            UpdateCarousel(false);
            UpdateCostText();
        }
    }

    // =========================================
    // 캐러셀
    // =========================================

    private void UpdateCarousel(bool instant)
    {
        int currentIndex =
            (int)playerController.currentSkillSlot;

        int count = skillSlots.Length;

        for (int i = 0; i < count; i++)
        {
            RectTransform slot = skillSlots[i];

            if (slot == null)
                continue;

            /*
             * relativeIndex
             *
             * 0 = 현재 선택 슬롯
             * 1 = 오른쪽 슬롯
             * 2 = 왼쪽 슬롯
             *
             * 예:
             *
             * 현재 Skill1
             *
             * Skill3   Skill1   Skill2
             *  LEFT    CENTER   RIGHT
             */

            int relativeIndex =
                (i - currentIndex + count) % count;

            Vector2 targetPosition;
            float targetScale;
            float targetAlpha;

            // ================================
            // CENTER
            // ================================

            if (relativeIndex == 0)
            {
                targetPosition = centerPosition;
                targetScale = centerScale;
                targetAlpha = centerAlpha;

                // 선택 슬롯이 항상 가장 앞에 그려짐
                slot.SetAsLastSibling();
            }

            // ================================
            // RIGHT
            // ================================

            else if (relativeIndex == 1)
            {
                targetPosition = rightPosition;
                targetScale = sideScale;
                targetAlpha = sideAlpha;
            }

            // ================================
            // LEFT
            // ================================

            else
            {
                targetPosition = leftPosition;
                targetScale = sideScale;
                targetAlpha = sideAlpha;
            }

            AnimateSlot(
                i,
                targetPosition,
                targetScale,
                targetAlpha,
                instant
            );
        }

        // SetAsLastSibling 때문에 Hierarchy 순서가 바뀌더라도
        // skills 배열 자체는 전혀 변경되지 않음.
    }

    private void AnimateSlot(
        int index,
        Vector2 targetPosition,
        float targetScale,
        float targetAlpha,
        bool instant)
    {
        RectTransform slot = skillSlots[index];
        CanvasGroup canvasGroup = canvasGroups[index];

        if (slot == null)
            return;

        // 기존 Tween 제거
        slot.DOKill();

        if (canvasGroup != null)
            canvasGroup.DOKill();

        if (instant)
        {
            slot.anchoredPosition = targetPosition;
            slot.localScale =
                Vector3.one * targetScale;

            if (canvasGroup != null)
                canvasGroup.alpha = targetAlpha;

            return;
        }

        // 위치 이동
        slot.DOAnchorPos(
                targetPosition,
                moveDuration
            )
            .SetEase(moveEase)
            .SetUpdate(true);

        // 크기 이동
        slot.DOScale(
                targetScale,
                moveDuration
            )
            .SetEase(moveEase)
            .SetUpdate(true);

        // 투명도 이동
        if (canvasGroup != null)
        {
            canvasGroup
                .DOFade(
                    targetAlpha,
                    moveDuration
                )
                .SetEase(moveEase)
                .SetUpdate(true);
        }
    }

    // =========================================
    // 장착 스킬 데이터 동기화
    // =========================================

    public void SyncSkills(SkillData[] newSkills)
    {
        if (newSkills == null)
            return;

        int count =
            Mathf.Min(
                skills.Length,
                newSkills.Length
            );

        for (int i = 0; i < count; i++)
        {
            skills[i] = newSkills[i];
        }

        UpdateAllSlotsUI();
        UpdateCostText();
    }

    public void UpdateAllSlotsUI()
    {
        int count =
            Mathf.Min(
                skillSlots.Length,
                skills.Length,
                skillSlotScripts.Length
            );

        for (int i = 0; i < count; i++)
        {
            if (skillSlotScripts[i] != null)
            {
                skillSlotScripts[i]
                    .UpdateSlot(skills[i]);
            }
        }
    }

    // =========================================
    // NeedCount
    // =========================================

    private void UpdateCostText()
    {
        if (fixedNeedCountText == null)
            return;

        if (playerController == null)
            return;

        int index =
            (int)playerController.currentSkillSlot;

        if (
            index >= 0 &&
            index < skills.Length &&
            skills[index] != null
        )
        {
            fixedNeedCountText.text =
                skills[index].cost;
        }
        else
        {
            fixedNeedCountText.text = "-";
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                skillSlots[i].DOKill();
            }

            if (
                canvasGroups != null &&
                i < canvasGroups.Length &&
                canvasGroups[i] != null
            )
            {
                canvasGroups[i].DOKill();
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 플레이 중에는 기존 Update/DOTween이 담당
        if (Application.isPlaying)
            return;

        PreviewCarouselInEditor();
    }

    private void PreviewCarouselInEditor()
    {
        if (skillSlots == null || skillSlots.Length == 0)
            return;

        int currentIndex = 0;

        // PlayerController가 연결돼 있으면 현재 선택 슬롯 기준
        if (playerController != null)
        {
            currentIndex = (int)playerController.currentSkillSlot;
        }

        int count = skillSlots.Length;

        for (int i = 0; i < count; i++)
        {
            RectTransform slot = skillSlots[i];

            if (slot == null)
                continue;

            int relativeIndex =
                (i - currentIndex + count) % count;

            Vector2 targetPosition;
            float targetScale;
            float targetAlpha;

            // 현재 선택 슬롯
            if (relativeIndex == 0)
            {
                targetPosition = centerPosition;
                targetScale = centerScale;
                targetAlpha = centerAlpha;
            }

            // 오른쪽 슬롯
            else if (relativeIndex == 1)
            {
                targetPosition = rightPosition;
                targetScale = sideScale;
                targetAlpha = sideAlpha;
            }

            // 왼쪽 슬롯
            else
            {
                targetPosition = leftPosition;
                targetScale = sideScale;
                targetAlpha = sideAlpha;
            }

            // 에디터에서는 애니메이션 없이 즉시 적용
            slot.anchoredPosition = targetPosition;
            slot.localScale = Vector3.one * targetScale;

            CanvasGroup group = slot.GetComponent<CanvasGroup>();

            if (group != null)
            {
                group.alpha = targetAlpha;
            }
        }
    }
#endif
}