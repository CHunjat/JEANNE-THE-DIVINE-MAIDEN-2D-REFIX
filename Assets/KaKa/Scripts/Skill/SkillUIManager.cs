using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public GameObject inGameUIPanel;
    public GameObject skillUIPanel;

    // 유니티 인스펙터에서 3개의 체크포인트를 모두 넣어주세요!
    public CheckpointSkillHandler[] checkpointSkillHandlers;

    private int currentSelectedIndex = -1;
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
        if (skillSlots != null && lastSyncedSkills != null)
        {
            bool isAnySlotChanged = false;
            for (int i = 0; i < skillSlots.Length; i++)
            {
                if (skillSlots[i] != null && skillSlots[i].skillData != lastSyncedSkills[i])
                {
                    lastSyncedSkills[i] = skillSlots[i].skillData;
                    isAnySlotChanged = true;
                }
            }

            if (isAnySlotChanged)
            {
                UpdateAvailableSlotsCount();
            }
        }

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
            try { skillRotationManager.SyncSkills(currentSlotSkills); }
            catch { }
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

    public void OnConfirmButtonClick()
    {
        if (inGameUIPanel != null) inGameUIPanel.SetActive(true);
        if (skillUIPanel != null) skillUIPanel.SetActive(false);

        // ⭐ [핵심 아이디어] Checkpoint.cs를 건드리지 않고, 플레이어와 제일 가까운 체크포인트 1개만 찾아서 엽니다!
        if (checkpointSkillHandlers != null && checkpointSkillHandlers.Length > 0)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CheckpointSkillHandler closestHandler = null;
                float minDistance = float.MaxValue;

                // 배열에 있는 모든 체크포인트 핸들러와 플레이어 사이의 거리를 계산
                foreach (var handler in checkpointSkillHandlers)
                {
                    if (handler != null)
                    {
                        float dist = Vector2.Distance(player.transform.position, handler.transform.position);
                        if (dist < minDistance)
                        {
                            minDistance = dist;
                            closestHandler = handler; // 가장 가까운 녀석 갱신
                        }
                    }
                }

                // 현재 플레이어가 서 있는(가장 가까운) 체크포인트 딱 하나만 메뉴를 닫음(메인화면 호출)
                if (closestHandler != null)
                {
                    closestHandler.CloseSkillMenu();
                }
            }
        }

        // 2. 빈 스킬 데이터가 넘어와도 에러 없이 UI가 꺼지도록 방어(Try-Catch)
        if (skillRotationManager != null && skillSlots != null)
        {
            try
            {
                for (int i = 0; i < skillRotationManager.skills.Length; i++)
                {
                    if (i < skillSlots.Length && skillSlots[i] != null)
                    {
                        skillRotationManager.skills[i] = skillSlots[i].skillData;
                    }
                }
                skillRotationManager.UpdateAllSlotsUI();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("빈 스킬 슬롯으로 인한 아이콘 갱신 무시됨 : " + e.Message);
            }
        }
    }
}