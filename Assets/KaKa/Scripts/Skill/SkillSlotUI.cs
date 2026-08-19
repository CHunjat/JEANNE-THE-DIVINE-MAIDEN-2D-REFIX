using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image skillIconImage;

    [Tooltip("나중에 선택 연출을 넣을 오브젝트. 지금은 비워둬도 됩니다.")]
    [SerializeField] private GameObject selectionEffect;

    [Header("Lock")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private GameObject lockObject;

    [Header("Data")]
    public SkillData skillData;

    private SkillUIManager manager;

    private Sprite cachedIconSprite;
    private bool isSelected = false;

    public bool IsLocked => isLocked;

    private void Awake()
    {
        manager = GetComponentInParent<SkillUIManager>();

        if (selectionEffect != null)
        {
            selectionEffect.SetActive(false);
        }

        UpdateSlotRawUI();
    }

    // ========================================
    // 스킬 등록
    // ========================================

    public void RegisterSkill(SkillData data, Sprite iconSprite)
    {
        // 잠긴 슬롯이면 등록 불가
        if (isLocked)
        {
            Debug.Log("잠긴 스킬 슬롯입니다.");
            return;
        }

        if (data == null)
            return;

        // 같은 스킬이 다른 슬롯에 이미 등록되어 있다면
        // 기존 슬롯에서 제거
        if (manager != null && manager.skillSlots != null)
        {
            foreach (SkillSlotUI slot in manager.skillSlots)
            {
                if (slot == null)
                    continue;

                if (slot != this && slot.skillData == data)
                {
                    slot.UnregisterSkill();
                }
            }
        }

        // 새로운 스킬 등록
        skillData = data;
        cachedIconSprite = iconSprite;

        UpdateSlotRawUI();

        if (manager != null)
        {
            manager.UpdateAvailableSlotsCount();
        }
    }

    // ========================================
    // 기존 Active_Skill 코드와 호환용
    // ========================================
    // 현재 Active_Skill에서
    // RegisterSkill(data, icon, tooltip) 형태로 호출하고 있기 때문에
    // 당장은 이 오버로드를 남겨둡니다.
    //
    // tooltipString은 새 UI에서는 사용하지 않습니다.
    // ========================================

    public void RegisterSkill(
        SkillData data,
        Sprite iconSprite,
        string tooltipString)
    {
        RegisterSkill(data, iconSprite);
    }

    // ========================================
    // 스킬 등록 해제
    // ========================================

    public void UnregisterSkill()
    {
        if (isLocked)
            return;

        if (isSelected && manager != null)
        {
            manager.ClearSelection();
        }

        skillData = null;
        cachedIconSprite = null;
        isSelected = false;

        UpdateSlotRawUI();

        if (manager != null)
        {
            manager.UpdateAvailableSlotsCount();
        }
    }

    // ========================================
    // 슬롯 클릭
    // ========================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLocked)
            return;

        // 좌클릭
        // → 해당 슬롯의 스킬 선택
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (skillData != null && manager != null)
            {
                manager.SelectSlot(this);
            }
        }

        // 우클릭
        // → 등록 해제
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (skillData != null)
            {
                UnregisterSkill();
            }
        }
    }

    // ========================================
    // 선택 상태
    // ========================================

    public void SetSelectState(bool select)
    {
        isSelected = select;

        if (selectionEffect != null)
        {
            selectionEffect.SetActive(select);
        }
    }

    // ========================================
    // 잠금 상태 변경
    // 나중에 슬롯 해금할 때 사용 가능
    // ========================================

    public void SetLocked(bool locked)
    {
        isLocked = locked;

        if (isLocked && skillData != null)
        {
            skillData = null;
            cachedIconSprite = null;
        }

        UpdateSlotRawUI();

        if (manager != null)
        {
            manager.UpdateAvailableSlotsCount();
        }
    }

    // ========================================
    // 슬롯 UI 갱신
    // ========================================

    public void UpdateSlotRawUI()
    {
        // 자물쇠 표시
        if (lockObject != null)
        {
            lockObject.SetActive(isLocked);
        }

        // 잠긴 슬롯
        if (isLocked)
        {
            if (skillIconImage != null)
            {
                skillIconImage.sprite = null;
                skillIconImage.enabled = false;
            }

            SetSelectState(false);
            return;
        }

        // 스킬이 등록된 슬롯
        if (skillData != null)
        {
            if (skillIconImage != null)
            {
                Sprite targetSprite =
                    cachedIconSprite != null
                    ? cachedIconSprite
                    : skillData.skillIcon;

                skillIconImage.sprite = targetSprite;
                skillIconImage.enabled = targetSprite != null;
                skillIconImage.color = Color.white;
            }
        }

        // 빈 슬롯
        else
        {
            if (skillIconImage != null)
            {
                skillIconImage.sprite = null;
                skillIconImage.enabled = false;
            }

            SetSelectState(false);
        }
    }
}