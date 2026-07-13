using UnityEngine;
using UnityEngine.UI; // 일반 Text용
using TMPro;           // TextMeshPro용

public class SkillUIManager : MonoBehaviour
{
    [Header("Skill Slots")]
    public SkillSlotUI[] skillSlots;

    [Header("Global UI (Legacy Text)")]
    public Text skillCostText;
    public Text availableSlotsText;

    [Range(10, 100)]
    public int costFontSize = 35;

    [Header("Skill Info UI (TextMeshPro)")]
    public GameObject skillInfoPanel;
    public Image infoSkillIconImage;
    public TextMeshProUGUI infoSkillNameText;
    public TextMeshProUGUI infoSkillTypeText;
    public TextMeshProUGUI infoSkillDescText;

    [Header("Skill Info Attributes")]
    public TextMeshProUGUI infoRequireFaithText;
    public TextMeshProUGUI infoRequireSPText;
    public TextMeshProUGUI infoSkillCostText;
    public TextMeshProUGUI infoUsedSlotText;

    [Header("Connected Manager")]
    public ActiveSkillManager activeSkillManager;
    public SkillRotationManager skillRotationManager;

    [Header("Confirm Button Actions")]
    public GameObject inGameUIPanel;    // 활성화할 인게임 화면 UI 오브젝트
    public GameObject skillUIPanel;     // 비활성화할 스킬 화면 UI 오브젝트
    public CheckpointSkillHandler checkpointSkillHandler;   // ★ 추가

    private int currentSelectedIndex = -1;

    // 💡 [추가] 이전 프레임의 스킬 장착 상태를 기억해두기 위한 배열
    private SkillData[] lastSyncedSkills;

    private void Start()
    {
        if (skillSlots != null)
        {
            lastSyncedSkills = new SkillData[skillSlots.Length];
        }

        UpdateAvailableSlotsCount();
        ClearSelection();
    }

    private void Update()
    {
        // 💡 [추가] 실시간 스킬 장착/해제 상태 감지 루프
        if (skillSlots != null && lastSyncedSkills != null)
        {
            bool isAnySlotChanged = false;
            for (int i = 0; i < skillSlots.Length; i++)
            {
                if (skillSlots[i] != null && skillSlots[i].skillData != lastSyncedSkills[i])
                {
                    lastSyncedSkills[i] = skillSlots[i].skillData; // 변경 기록 갱신
                    isAnySlotChanged = true;
                }
            }

            // 스킬 데이터 장착 상태가 변했다면 실시간으로 즉시 인게임 연동 실행!
            if (isAnySlotChanged)
            {
                UpdateAvailableSlotsCount();
            }
        }

        // 기존 키보드 스킬 선택 로직
        if (currentSelectedIndex != -1)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int nextIndex = currentSelectedIndex;
                for (int i = 0; i < skillSlots.Length; i++)
                {
                    nextIndex = nextIndex - 1;
                    if (nextIndex < 0) nextIndex = skillSlots.Length - 1;

                    if (skillSlots[nextIndex].skillData != null)
                    {
                        SelectSlotByIndex(nextIndex);
                        break;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int nextIndex = currentSelectedIndex;
                for (int i = 0; i < skillSlots.Length; i++)
                {
                    nextIndex = nextIndex + 1;
                    if (nextIndex >= skillSlots.Length) nextIndex = 0;

                    if (skillSlots[nextIndex].skillData != null)
                    {
                        SelectSlotByIndex(nextIndex);
                        break;
                    }
                }
            }
        }
    }

    public void UpdateAvailableSlotsCount()
    {
        if (availableSlotsText == null) return;

        int emptyCount = 0;
        SkillData[] currentSlotSkills = new SkillData[skillSlots.Length];

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                currentSlotSkills[i] = skillSlots[i].skillData;
                if (skillSlots[i].skillData == null)
                {
                    emptyCount++;
                }
            }
        }

        availableSlotsText.text = "사용 가능한 슬롯 수 <size=" + costFontSize + ">: " + emptyCount + "</size>";

        if (skillRotationManager != null)
        {
            skillRotationManager.SyncSkills(currentSlotSkills);
        }
    }

    public void SelectSlot(SkillSlotUI selectedSlot)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == selectedSlot)
            {
                SelectSlotByIndex(i);
                return;
            }
        }
    }

    private void SelectSlotByIndex(int index)
    {
        if (activeSkillManager != null)
        {
            activeSkillManager.ClearSelection();
        }

        if (currentSelectedIndex != -1 && currentSelectedIndex < skillSlots.Length)
        {
            skillSlots[currentSelectedIndex].SetSelectState(false);
        }

        currentSelectedIndex = index;
        SkillSlotUI currentSlot = skillSlots[currentSelectedIndex];
        currentSlot.SetSelectState(true);

        if (skillCostText != null && currentSlot.skillData != null)
        {
            skillCostText.text = "스킬 코스트 <size=" + costFontSize + ">: " + currentSlot.skillData.cost + "</size>";
        }

        if (currentSlot.skillData != null)
        {
            if (skillInfoPanel != null) skillInfoPanel.SetActive(true);

            if (infoSkillNameText != null) infoSkillNameText.text = currentSlot.skillData.skillName;
            if (infoSkillDescText != null) infoSkillDescText.text = currentSlot.skillData.description;
            if (infoSkillTypeText != null) infoSkillTypeText.text = currentSlot.skillData.skilltype.ToString();

            if (infoRequireFaithText != null) infoRequireFaithText.text = "요구 신앙심 수치 : -";
            if (infoRequireSPText != null) infoRequireSPText.text = "강화 SP : -";

            if (infoSkillCostText != null) infoSkillCostText.text = "스킬 코스트 : " + currentSlot.skillData.cost.ToString();
            if (infoUsedSlotText != null) infoUsedSlotText.text = "사용 슬롯 수 : " + currentSlot.skillData.usedslot.ToString();

            if (infoSkillIconImage != null && currentSlot.skillData.skillIcon != null)
            {
                infoSkillIconImage.sprite = currentSlot.skillData.skillIcon;
                infoSkillIconImage.enabled = true;
            }
        }
    }

    public void ClearSelection()
    {
        if (currentSelectedIndex != -1 && currentSelectedIndex < skillSlots.Length)
        {
            skillSlots[currentSelectedIndex].SetSelectState(false);
        }
        currentSelectedIndex = -1;

        if (skillCostText != null)
        {
            skillCostText.text = "스킬 코스트 <size=" + costFontSize + ">: -</size>";
        }

        if (infoSkillNameText != null) infoSkillNameText.text = "-";
        if (infoSkillTypeText != null) infoSkillTypeText.text = "-";
        if (infoSkillDescText != null) infoSkillDescText.text = "선택된 스킬이 없습니다.";

        if (infoRequireFaithText != null) infoRequireFaithText.text = "요구 신앙심 수치 : -";
        if (infoRequireSPText != null) infoRequireSPText.text = "강화 SP : -";
        if (infoSkillCostText != null) infoSkillCostText.text = "스킬 코스트 : -";
        if (infoUsedSlotText != null) infoUsedSlotText.text = "사용 슬롯 수 : -";

        if (infoSkillIconImage != null)
        {
            infoSkillIconImage.sprite = null;
            infoSkillIconImage.enabled = false;
        }
    }

    // 💡 UI의 '확인' 버튼에 연결할 함수
    public void OnConfirmButtonClick()
    {
        // 1. 인게임 화면 켜고, 스킬 화면 끄기
        if (inGameUIPanel != null) inGameUIPanel.SetActive(true);
        if (skillUIPanel != null) skillUIPanel.SetActive(false);

        if (checkpointSkillHandler != null)
        {
            checkpointSkillHandler.ConfirmAndExit();   // ★ 추가 — RestState 탈출 + 내부 플래그 정리
        }

        // 2. ⭐ [핵심 추가] 스킬창 슬롯의 데이터를 인게임 회전 UI로 전달 및 아이콘 새로고침
        if (skillRotationManager != null && skillSlots != null)
        {
            // 인게임 회전 슬롯 개수(3개)만큼 반복문 실행
            for (int i = 0; i < skillRotationManager.skills.Length; i++)
            {
                // 스킬 장착창 슬롯에 데이터가 존재한다면
                if (i < skillSlots.Length && skillSlots[i] != null)
                {
                    // 장착창의 스킬 데이터를 인게임 스킬 데이터로 복사합니다.
                    skillRotationManager.skills[i] = skillSlots[i].skillData;
                }
            }

            // 복사된 최신 데이터를 기준으로 인게임 스킬 아이콘들을 즉시 새로고침합니다.
            skillRotationManager.UpdateAllSlotsUI();
        }
    }
}