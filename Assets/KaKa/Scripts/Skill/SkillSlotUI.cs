using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI Elements")]
    public GameObject tooltipText;  // 스킬 위의 비활성화 되어있는 툴팁 오브젝트
    public GameObject checkmark;    // Background 하위의 Checkmark
    public Image skillIconImage;    // 스킬 아이콘 이미지 컴포넌트
    public Text skillCostText;      // 코스트 텍스트

    [Header("Data")]
    public SkillData skillData;     // 인스펙터에서 각 스킬에 맞는 SkillData 할당

    private SkillUIManager manager;
    private Button button;

    // 현재 이 슬롯이 클릭되어 선택된 상태인지 여부
    private bool isSelected = false;

    private string cachedTooltipText;
    private Sprite cachedIconSprite;

    private void Awake()
    {
        manager = GetComponentInParent<SkillUIManager>();
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnSlotClicked);
        }

        if (tooltipText != null) tooltipText.SetActive(false);
        if (checkmark != null) checkmark.SetActive(false);

        if (skillIconImage == null)
        {
            skillIconImage = GetComponent<Image>();
        }
    }

    // 외부에서 스킬을 드래그 앤 드롭 등으로 등록할 때 호출하는 함수
    public void RegisterSkill(SkillData data, Sprite iconSprite, string tooltipString)
    {
        // 💡 [중복 등록 방지 로직 핵심]
        // 매니저가 관리하는 모든 슬롯을 검사하여, 이미 똑같은 스킬이 등록된 다른 슬롯이 있다면 비워줍니다.
        if (data != null && manager != null && manager.skillSlots != null)
        {
            foreach (SkillSlotUI slot in manager.skillSlots)
            {
                // '나 자신이 아닌 다른 슬롯' 중에서 '동일한 스킬 데이터'를 가진 슬롯이 있다면
                if (slot != this && slot.skillData == data)
                {
                    slot.UnregisterSkill(); // 해당 기존 슬롯을 깨끗하게 해제(초기화)합니다.
                }
            }
        }

        // 새 슬롯에 스킬 정보 등록
        skillData = data;
        cachedIconSprite = iconSprite;
        cachedTooltipText = tooltipString;

        UpdateSlotRawUI();

        // 사용 가능한 슬롯 개수 최신화
        if (manager != null)
        {
            manager.UpdateAvailableSlotsCount();
        }
    }

    // 마우스 클릭을 감지하는 이벤트 함수 (우클릭 해제)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && skillData != null)
        {
            UnregisterSkill();
        }
    }

    // 스킬 등록을 해제하고 초기 상태로 되돌리는 함수
    public void UnregisterSkill()
    {
        if (isSelected && manager != null)
        {
            manager.ClearSelection();
        }

        skillData = null;
        cachedIconSprite = null;
        cachedTooltipText = "";
        isSelected = false;

        UpdateSlotRawUI();

        if (manager != null)
        {
            manager.UpdateAvailableSlotsCount();
        }
    }

    // 마우스 커서를 올렸을 때 (호버 시작)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData != null && tooltipText != null)
        {
            tooltipText.SetActive(true);
        }
    }

    // 마우스 커서가 벗어났을 때 (호버 종료)
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected && tooltipText != null)
        {
            tooltipText.SetActive(false);
        }
    }

    // 버튼이 좌클릭 되었을 때 매니저에게 알림
    private void OnSlotClicked()
    {
        if (skillData != null && manager != null)
        {
            manager.SelectSlot(this);
        }
    }

    // 매니저(SkillUIManager)가 슬롯을 선택/해제할 때 호출하는 함수
    public void SetSelectState(bool select)
    {
        isSelected = select;

        if (checkmark != null)
        {
            checkmark.SetActive(select);
        }

        if (tooltipText != null)
        {
            tooltipText.SetActive(select);
        }
    }

    // UI를 새로고침하는 함수
    public void UpdateSlotRawUI()
    {
        if (skillData != null)
        {
            // 1. 아이콘 이미지 반영
            if (skillIconImage != null)
            {
                Sprite targetSprite = (cachedIconSprite != null) ? cachedIconSprite : skillData.skillIcon;
                if (targetSprite != null)
                {
                    skillIconImage.sprite = targetSprite;
                    skillIconImage.enabled = true;
                    skillIconImage.color = Color.white;
                }
            }

            // 2. 코스트 텍스트 반영
            if (skillCostText != null)
            {
                skillCostText.text = skillData.cost;
                skillCostText.gameObject.SetActive(true);
            }

            // 3. 툴팁 문구 반영
            if (tooltipText != null && !string.IsNullOrEmpty(cachedTooltipText))
            {
                Text tText = tooltipText.GetComponentInChildren<Text>();
                if (tText != null)
                {
                    tText.text = cachedTooltipText;
                }
            }
        }
        else
        {
            // 빈 슬롯 상태일 때의 초기화 처리
            if (skillIconImage != null)
            {
                skillIconImage.sprite = null;
                skillIconImage.enabled = false;
            }
            if (skillCostText != null)
            {
                skillCostText.text = "";
            }

            cachedTooltipText = "";
            cachedIconSprite = null;

            SetSelectState(false);
        }
    }
}