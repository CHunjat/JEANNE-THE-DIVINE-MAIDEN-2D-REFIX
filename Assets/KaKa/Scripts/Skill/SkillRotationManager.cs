using DG.Tweening;
using TMPro;
using UnityEngine;

public class SkillRotationManager : MonoBehaviour
{
    [Header("Skill Data (Logic)")]
    public SkillData[] skills = new SkillData[3];

    [Header("Skill Parent Objects (UI)")]
    [Tooltip("Skill_1, Skill_2, Skill_3 같은 최상위 부모 RectTransform을 순서대로 연결해 주세요.")]
    public RectTransform[] skillSlots = new RectTransform[3];

    [Header("Connection")]
    [Tooltip("씬에 배치된 플레이어(PlayerController)를 연결해 주세요. 원본 코드는 절대 건드리지 않습니다!")]
    public PlayerController playerController;

    [Header("Curve Settings")]
    public float curveOffset = 80f;

    [Header("Fixed UI")]
    public TextMeshProUGUI fixedNeedCountText;

    // 원본 레이아웃의 고정 위치 및 크기를 기억할 배열 (배열을 절대 셔플하지 않고 보존합니다)
    private Vector2[] baseAnchorPositions = new Vector2[3];
    private Vector3[] baseScales = new Vector3[3];
    private SkillSlot[] skillSlotScripts = new SkillSlot[3];

    private PlayerController.SkillSlot lastKnownSkillSlot;
    private object lastPlayerState = null;

    private void Start()
    {
        // 1. 초기 인스펙터에 배치된 순서대로 위치와 크기, 스크립트 원본 고정 캐싱
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                baseAnchorPositions[i] = skillSlots[i].anchoredPosition;
                baseScales[i] = skillSlots[i].localScale;
                skillSlotScripts[i] = skillSlots[i].GetComponentInChildren<SkillSlot>();
            }
        }

        if (playerController != null)
        {
            lastKnownSkillSlot = playerController.currentSkillSlot;
            if (playerController.StateMachine != null)
            {
                lastPlayerState = playerController.StateMachine.CurrentState;
            }
        }

        UpdateAllSlotsUI();
        AnimateSlotsToCurrentSlot(true); // 시작할 때 현재 슬롯 위치 정렬
        UpdateCostText();
    }

    private void Update()
    {
        if (playerController == null) return;

        // 2. 플레이어의 슬롯이 변경된 것이 확인되면 UI 카루셀 회전 애니메이션 실행
        if (playerController.currentSkillSlot != lastKnownSkillSlot)
        {
            AnimateSlotsToCurrentSlot(false);
            lastKnownSkillSlot = playerController.currentSkillSlot;
            UpdateCostText();
        }
    }

    // 스킬 관련 상태 클래스인지 이름을 통해 판별하는 방어 코드
    private bool IsSkillState(object state)
    {
        if (state == null) return false;
        string stateName = state.GetType().Name.ToLower();
        // 플레이어 상태창의 클래스명에 아래 키워드가 들어가면 스킬 시전 중으로 판단
        return stateName.Contains("heavy") || stateName.Contains("lightning") || stateName.Contains("heal") || stateName.Contains("skill");
    }

    // 플레이어의 실제 타겟 스킬 슬롯을 다음 칸으로 교체하는 함수
    private void RotatePlayerSkillSlot()
    {
        int currentSlotInt = (int)playerController.currentSkillSlot;
        int nextSlotInt = (currentSlotInt + 1) % 3; // 3개 슬롯 순환 공식을 적용합니다.
        playerController.currentSkillSlot = (PlayerController.SkillSlot)nextSlotInt;
    }

    // 💡 UI 스킬 장착창과 실시간 동기화 데이터 연동
    public void SyncSkills(SkillData[] newSkills)
    {
        int count = Mathf.Min(skills.Length, newSkills.Length);
        for (int i = 0; i < count; i++)
        {
            skills[i] = newSkills[i];
        }
        UpdateAllSlotsUI();
        UpdateCostText();
    }

    /// <summary>
    /// 수학적 공식에 의거하여 각 UI 슬롯들을 현재 활성화된 슬롯 기준으로 재정렬 및 회전 연출합니다.
    /// </summary>
    private void AnimateSlotsToCurrentSlot(bool isInstant)
    {
        float duration = isInstant ? 0f : 0.35f;
        int currentSlotIndex = (int)playerController.currentSkillSlot;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;

            // ⭐ 고정 맵핑 공식: (내 인덱스 - 현재 활성화된 슬롯 인덱스 + 3) % 3
            // 이 공식을 사용하면 배열을 뒤섞지 않고도 활성화된 UI가 항상 중앙(0번 자리)으로 오게 됩니다.
            int targetPosIndex = (i - currentSlotIndex + 3) % 3;

            Vector2 endPos = baseAnchorPositions[targetPosIndex];
            Vector3 endScale = baseScales[targetPosIndex];

            RectTransform rect = skillSlots[i];
            rect.DOKill();

            if (isInstant)
            {
                rect.anchoredPosition = endPos;
                rect.localScale = endScale;
            }
            else
            {
                Vector2 startPos = rect.anchoredPosition;

                // 유저님의 원본 아름다운 포물선(Sin) 곡선 연출 적용
                DOTween.To(() => 0f, t => {
                    Vector2 currentLinearPos = Vector2.Lerp(startPos, endPos, t);
                    float sinOffset = Mathf.Sin(t * Mathf.PI) * curveOffset;
                    rect.anchoredPosition = new Vector2(currentLinearPos.x - sinOffset, currentLinearPos.y);
                }, 1f, duration).SetEase(Ease.OutQuad);

                rect.DOScale(endScale, duration).SetEase(Ease.OutQuad);
            }
        }
    }

    public void UpdateAllSlotsUI()
    {
        int count = Mathf.Min(skillSlots.Length, skills.Length, skillSlotScripts.Length);
        for (int i = 0; i < count; i++)
        {
            if (skillSlotScripts[i] != null)
            {
                skillSlotScripts[i].UpdateSlot(skills[i]);
            }
        }
    }

    private void UpdateCostText()
    {
        if (fixedNeedCountText == null) return;

        int idx = (int)playerController.currentSkillSlot;
        if (idx >= 0 && idx < skills.Length && skills[idx] != null)
        {
            fixedNeedCountText.text = skills[idx].cost;
        }
        else
        {
            fixedNeedCountText.text = "-";
        }
    }
}