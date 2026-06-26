using DG.Tweening;
using TMPro;
using UnityEngine;

public class SkillRotationManager : MonoBehaviour
{
    [Header("Skill Data (Logic)")]
    public SkillData[] skills = new SkillData[3];

    [Header("Skill Slots (UI)")]
    public SkillSlotUI[] skillSlots = new SkillSlotUI[3];

    [Header("Connection")]
    [Tooltip("씬에 배치된 플레이어(PlayerController)를 연결해 주세요. 원본 코드는 절대 건드리지 않습니다!")]
    public PlayerController playerController;

    [Header("Curve Settings")]
    public float curveOffset = 80f;

    [Header("Fixed UI")]
    public TextMeshProUGUI fixedNeedCountText; // 여기에 고정된 NeedCount 텍스트 오브젝트를 연결하세요.

    private Vector2[] targetAnchorPositions = new Vector2[3];
    private Vector3[] targetScales = new Vector3[3];
    private bool isRotating = false;

    // 🔥 핵심: 이전 프레임의 스킬 슬롯 상태를 기억할 변수
    private PlayerController.SkillSlot lastKnownSkillSlot;

    private void Start()
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                RectTransform rect = skillSlots[i].GetComponent<RectTransform>();
                if (rect != null)
                {
                    targetAnchorPositions[i] = rect.anchoredPosition;
                }
                targetScales[i] = skillSlots[i].transform.localScale;
            }
        }

        // 게임 시작 시점의 플레이어 스킬 슬롯 상태를 최초로 기억해 둡니다.
        if (playerController != null)
        {
            lastKnownSkillSlot = playerController.currentSkillSlot;
        }

        UpdateAllSlotsUI();
    }

    private void Update()
    {
        if (playerController != null && !isRotating)
    {
        if (playerController.currentSkillSlot != lastKnownSkillSlot)
        {
            // 🔥 로그 추가: 플레이어 상태가 바뀌었음을 감지한 순간
            Debug.Log($"[감지] 플레이어 스킬 변경! 이전: {lastKnownSkillSlot} -> 현재: {playerController.currentSkillSlot}");
            
            RotateSkillDataAndSlots();
            
            lastKnownSkillSlot = playerController.currentSkillSlot;
        }
    }
    }

    private void RotateSkillDataAndSlots()
    {
        if (isRotating || skills[0] == null || skills[1] == null || skills[2] == null) return;
        isRotating = true;

        // [A] 데이터 순환
        SkillData tempData = skills[0];
        skills[0] = skills[1];
        skills[1] = skills[2];
        skills[2] = tempData;

        // [B] UI 오브젝트 순환
        SkillSlotUI tempSlot = skillSlots[0];
        skillSlots[0] = skillSlots[1];
        skillSlots[1] = skillSlots[2];
        skillSlots[2] = tempSlot;

        // 1. 데이터를 먼저 갱신
        UpdateAllSlotsUI();

        // 2. 🔥 핵심: UI를 즉시 강제 재계산 (이게 있으면 텍스트 갱신이 밀리지 않습니다)
        Canvas.ForceUpdateCanvases();

        // 3. 연출 시작
        PlayCurveRotationVisual();
    }

    private void PlayCurveRotationVisual()
    {
        float duration = 0.35f;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;

            RectTransform rect = skillSlots[i].GetComponent<RectTransform>();
            if (rect == null) continue;

            skillSlots[i].transform.DOKill();

            Vector2 startPos = rect.anchoredPosition;
            Vector2 endPos = targetAnchorPositions[i];

            DOTween.To(() => 0f, t => {
                Vector2 currentLinearPos = Vector2.Lerp(startPos, endPos, t);
                float sinOffset = Mathf.Sin(t * Mathf.PI) * curveOffset;
                rect.anchoredPosition = new Vector2(currentLinearPos.x - sinOffset, currentLinearPos.y);

            }, 1f, duration).SetEase(Ease.OutQuad);

            skillSlots[i].transform.DOScale(targetScales[i], duration).SetEase(Ease.OutQuad);
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
            if (skillSlots[i] != null && skills[i] != null)
            {
                skillSlots[i].UpdateSlot(skills[i]);
            }
        }

        // 🔥 여기가 핵심: 항상 '현재 스킬(0번)'의 코스트를 고정 텍스트에 출력
        if (fixedNeedCountText != null && skills[0] != null)
        {
            fixedNeedCountText.text = skills[0].cost.ToString();
        }
    }
}