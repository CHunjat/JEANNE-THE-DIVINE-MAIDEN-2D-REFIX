using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUIManager : MonoBehaviour
{
    [Header("등록 스킬 슬롯")]
    public SkillSlotUI[] skillSlots;

    [Header("Skill Info UI")]
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

    private int currentSelectedIndex = -1;

    // 슬롯 변경 감지용
    private SkillData[] lastSyncedSkills;

    private void Start()
    {
        InitializeSlotCache();

        UpdateAvailableSlotsCount();

        ClearSelection();
    }

    // ========================================
    // 슬롯 캐시 초기화
    // ========================================

    private void InitializeSlotCache()
    {
        if (skillSlots == null)
        {
            lastSyncedSkills = null;
            return;
        }

        lastSyncedSkills = new SkillData[skillSlots.Length];

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                lastSyncedSkills[i] = skillSlots[i].skillData;
            }
        }
    }

    private void Update()
    {
        CheckSlotChanges();
        HandleSlotKeyboardNavigation();
    }

    // ========================================
    // 슬롯 변경 감지
    // ========================================

    private void CheckSlotChanges()
    {
        if (skillSlots == null ||
            lastSyncedSkills == null ||
            lastSyncedSkills.Length != skillSlots.Length)
        {
            InitializeSlotCache();
            return;
        }

        bool changed = false;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            SkillData currentData = null;

            if (skillSlots[i] != null)
            {
                currentData = skillSlots[i].skillData;
            }

            if (lastSyncedSkills[i] != currentData)
            {
                lastSyncedSkills[i] = currentData;
                changed = true;
            }
        }

        if (changed)
        {
            UpdateAvailableSlotsCount();
        }
    }

    // ========================================
    // ← → 키로 등록 스킬 선택
    // ========================================

    private void HandleSlotKeyboardNavigation()
    {
        if (currentSelectedIndex == -1)
            return;

        if (skillSlots == null || skillSlots.Length == 0)
            return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SelectNextOccupiedSlot(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SelectNextOccupiedSlot(1);
        }
    }

    private void SelectNextOccupiedSlot(int direction)
    {
        if (skillSlots == null || skillSlots.Length == 0)
            return;

        int nextIndex = currentSelectedIndex;

        for (int i = 0; i < skillSlots.Length; i++)
        {
            nextIndex += direction;

            if (nextIndex < 0)
                nextIndex = skillSlots.Length - 1;

            if (nextIndex >= skillSlots.Length)
                nextIndex = 0;

            SkillSlotUI slot = skillSlots[nextIndex];

            if (slot != null &&
                !slot.IsLocked &&
                slot.skillData != null)
            {
                SelectSlotByIndex(nextIndex);
                return;
            }
        }
    }

    // ========================================
    // 현재 등록 슬롯 → 실제 게임 스킬 동기화
    // ========================================

    public void UpdateAvailableSlotsCount()
    {
        if (skillSlots == null)
            return;

        SkillData[] currentSlotSkills =
            new SkillData[skillSlots.Length];

        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] != null)
            {
                currentSlotSkills[i] =
                    skillSlots[i].skillData;
            }
        }

        // 실제 전투 스킬 시스템과 동기화
        if (skillRotationManager != null)
        {
            try
            {
                skillRotationManager.SyncSkills(
                    currentSlotSkills
                );
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(
                    "스킬 슬롯 동기화 중 오류 : " +
                    e.Message
                );
            }
        }

        // 변경 감지 캐시도 같이 최신화
        if (lastSyncedSkills == null ||
            lastSyncedSkills.Length != skillSlots.Length)
        {
            lastSyncedSkills =
                new SkillData[skillSlots.Length];
        }

        for (int i = 0; i < skillSlots.Length; i++)
        {
            lastSyncedSkills[i] =
                skillSlots[i] != null
                ? skillSlots[i].skillData
                : null;
        }
    }

    // ========================================
    // 슬롯 선택
    // ========================================

    public void SelectSlot(SkillSlotUI selectedSlot)
    {
        if (selectedSlot == null ||
            selectedSlot.IsLocked ||
            skillSlots == null)
        {
            return;
        }

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
        if (skillSlots == null)
            return;

        if (index < 0 || index >= skillSlots.Length)
            return;

        SkillSlotUI currentSlot = skillSlots[index];

        if (currentSlot == null ||
            currentSlot.IsLocked ||
            currentSlot.skillData == null)
        {
            return;
        }

        // 왼쪽 Active 스킬 목록의 선택 해제
        if (activeSkillManager != null)
        {
            activeSkillManager.ClearSelection();
        }

        // 기존 선택 슬롯 해제
        if (currentSelectedIndex != -1 &&
            currentSelectedIndex < skillSlots.Length &&
            skillSlots[currentSelectedIndex] != null)
        {
            skillSlots[currentSelectedIndex]
                .SetSelectState(false);
        }

        currentSelectedIndex = index;

        currentSlot.SetSelectState(true);

        ShowSkillInfo(currentSlot.skillData);
    }

    // ========================================
    // 상세정보 표시
    // ========================================

    private void ShowSkillInfo(SkillData data)
    {
        if (data == null)
            return;

        if (skillInfoPanel != null)
        {
            skillInfoPanel.SetActive(true);
        }

        if (infoSkillNameText != null)
        {
            infoSkillNameText.text = data.skillName;
        }

        if (infoSkillTypeText != null)
        {
            infoSkillTypeText.text =
                data.skilltype.ToString();
        }

        if (infoSkillDescText != null)
        {
            infoSkillDescText.text =
                data.description;
        }

        if (infoRequireFaithText != null)
        {
            infoRequireFaithText.text =
                "요구 신앙심 수치 : -";
        }

        if (infoRequireSPText != null)
        {
            infoRequireSPText.text =
                "강화 SP : -";
        }

        if (infoSkillCostText != null)
        {
            infoSkillCostText.text =
                "스킬 코스트 : " + data.cost;
        }

        if (infoUsedSlotText != null)
        {
            infoUsedSlotText.text =
                "사용 슬롯 수 : " + data.usedslot;
        }

        if (infoSkillIconImage != null)
        {
            if (data.skillIcon != null)
            {
                infoSkillIconImage.sprite =
                    data.skillIcon;

                infoSkillIconImage.enabled = true;
            }
            else
            {
                infoSkillIconImage.sprite = null;
                infoSkillIconImage.enabled = false;
            }
        }
    }

    // ========================================
    // 선택 해제
    // ========================================

    public void ClearSelection()
    {
        if (skillSlots != null &&
            currentSelectedIndex != -1 &&
            currentSelectedIndex < skillSlots.Length &&
            skillSlots[currentSelectedIndex] != null)
        {
            skillSlots[currentSelectedIndex]
                .SetSelectState(false);
        }

        currentSelectedIndex = -1;

        if (skillInfoPanel != null)
        {
            skillInfoPanel.SetActive(false);
        }

        if (infoSkillNameText != null)
            infoSkillNameText.text = "-";

        if (infoSkillTypeText != null)
            infoSkillTypeText.text = "-";

        if (infoSkillDescText != null)
            infoSkillDescText.text =
                "선택된 스킬이 없습니다.";

        if (infoRequireFaithText != null)
            infoRequireFaithText.text =
                "요구 신앙심 수치 : -";

        if (infoRequireSPText != null)
            infoRequireSPText.text =
                "강화 SP : -";

        if (infoSkillCostText != null)
            infoSkillCostText.text =
                "스킬 코스트 : -";

        if (infoUsedSlotText != null)
            infoUsedSlotText.text =
                "사용 슬롯 수 : -";

        if (infoSkillIconImage != null)
        {
            infoSkillIconImage.sprite = null;
            infoSkillIconImage.enabled = false;
        }
    }
}