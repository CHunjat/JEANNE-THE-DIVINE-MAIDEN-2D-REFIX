using DG.Tweening;
using TMPro;
using UnityEngine;

public class SkillRotationManager : MonoBehaviour
{
    [Header("Skill Data (Logic)")]
    public SkillData[] skills = new SkillData[3];

    [Header("Skill Parent Objects (UI)")]
    [Tooltip("Skill_1, Skill_2, Skill_3 같은 최상위 부모 RectTransform을 연결해 주세요.")]
    public RectTransform[] skillSlots = new RectTransform[3];

    [Header("Connection")]
    [Tooltip("씬에 배치된 플레이어(PlayerController)를 연결해 주세요. 원본 코드는 절대 건드리지 않습니다!")]
    public PlayerController playerController;

    [Header("Curve Settings")]
    public float curveOffset = 80f;

    [Header("Fixed UI")]
    public TextMeshProUGUI fixedNeedCountText;

    private Vector2[] targetAnchorPositions = new Vector2[3];
    private Vector3[] targetScales = new Vector3[3];
    private SkillSlot[] skillSlotScripts = new SkillSlot[3];
    private bool isRotating = false;

    private PlayerController.SkillSlot lastKnownSkillSlot;

    private void Start()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                targetAnchorPositions[i] = skillSlots[i].anchoredPosition;
                targetScales[i] = skillSlots[i].localScale;
                skillSlotScripts[i] = skillSlots[i].GetComponentInChildren<SkillSlot>();
            }
        }

        UpdateAllSlotsUI();
    }

    private void Update()
    {
        if (playerController == null) return;

        PlayerController.SkillSlot currentSlot = playerController.currentSkillSlot;
        if (currentSlot != lastKnownSkillSlot && !isRotating)
        {
            isRotating = true;
            ShiftSlotsRight();
            lastKnownSkillSlot = currentSlot;
        }
    }

    // 💡 [추가 기능] UI 스킬 슬롯의 변경사항을 실시간으로 동기화받는 함수
    public void SyncSkills(SkillData[] newSkills)
    {
        for (int i = 0; i < skills.Length; i++)
        {
            if (i < newSkills.Length)
            {
                skills[i] = newSkills[i]; // UI 슬롯에 등록된 스킬(혹은 null)을 그대로 복사
            }
        }
        UpdateAllSlotsUI(); // 데이터가 바뀌었으므로 즉시 인게임 회전 UI 갱신!
    }

    private void ShiftSlotsRight()
    {
        float duration = 0.35f;

        // 위치 및 크기 배열 백업
        Vector2 lastPos = targetAnchorPositions[targetAnchorPositions.Length - 1];
        Vector3 lastScale = targetScales[targetScales.Length - 1];

        for (int i = targetAnchorPositions.Length - 1; i > 0; i--)
        {
            targetAnchorPositions[i] = targetAnchorPositions[i - 1];
            targetScales[i] = targetScales[i - 1];
        }
        targetAnchorPositions[0] = lastPos;
        targetScales[0] = lastScale;

        // 스크립트 배열 백업 및 회전
        SkillSlot lastScript = skillSlotScripts[skillSlotScripts.Length - 1];
        for (int i = skillSlotScripts.Length - 1; i > 0; i--)
        {
            skillSlotScripts[i] = skillSlotScripts[i - 1];
        }
        skillSlotScripts[0] = lastScript;

        // DOTween 애니메이션 연출
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;

            RectTransform rect = skillSlots[i];
            rect.DOKill();

            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = targetAnchorPositions[i];

            DOTween.To(() => 0f, t => {
                Vector2 currentLinearPos = Vector2.Lerp(startPos, endPos, t);
                float sinOffset = Mathf.Sin(t * Mathf.PI) * curveOffset;
                rect.anchoredPosition = new Vector2(currentLinearPos.x - sinOffset, currentLinearPos.y);
            }, 1f, duration).SetEase(Ease.OutQuad);

            rect.DOScale(targetScales[i], duration).SetEase(Ease.OutQuad);
        }

        DOVirtual.DelayedCall(duration, () => {
            UpdateAllSlotsUI();
            isRotating = false;
        });
    }

    private void UpdateAllSlotsUI()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlotScripts[i] != null)
            {
                // 캐싱된 스크립트를 통해 스킬 데이터 주입
                skillSlotScripts[i].UpdateSlot(skills[i]);
            }
        }
    }
}