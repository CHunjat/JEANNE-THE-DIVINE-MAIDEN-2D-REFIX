using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Active_Skill : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Elements")]
    public Image skillIconImage;
    public GameObject tooltipText;
    public GameObject checkmark;

    [Header("Data")]
    public SkillData skillData;

    [Header("Drag Settings")]
    [Range(0f, 1f)]
    public float dragAlpha = 0.5f;

    // 💡 중앙 통제실과 소통하기 위한 변수 (인펙터에 노출 안 됨)
    [HideInInspector] public ActiveSkillManager manager;
    [HideInInspector] public int skillIndex;

    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;
    private GameObject dragClone;
    private RectTransform cloneRect;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (skillData != null && skillIconImage != null)
        {
            skillIconImage.sprite = skillData.skillIcon;
            skillIconImage.gameObject.SetActive(true);
        }
    }

    // ⭐ 매니저가 이 버튼의 선택 상태를 강제로 제어할 때 쓸 함수
    public void SetSelectState(bool isSelected)
    {
        if (checkmark != null) checkmark.SetActive(isSelected);
        if (tooltipText != null) tooltipText.SetActive(isSelected);
    }

    // --- 1. 마우스 호버 기능 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 마우스가 올라가면 일단 무조건 툴팁을 띄웁니다.
        if (tooltipText != null) tooltipText.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 💡 단, 내가 지금 '선택된 버튼'이라면 마우스가 나가도 툴팁을 끄지 않고 유지합니다.
        if (manager != null && manager.CurrentSelectedIndex == skillIndex) return;

        if (tooltipText != null) tooltipText.SetActive(false);
    }

    // --- 2. 클릭 기능 ---
    public void OnPointerClick(PointerEventData eventData)
    {
        // 💡 직접 켜지 않고, 매니저에게 "저 클릭됐어요! 다른 애들 끄고 저만 켜주세요"라고 요청합니다.
        if (manager != null)
        {
            manager.SelectSkill(skillIndex);
        }
    }

    // --- 3. 드래그 앤 드롭 기능 (유지) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.alpha = dragAlpha;

        if (mainCanvas != null) dragClone = Instantiate(gameObject, mainCanvas.transform);
        else dragClone = Instantiate(gameObject, transform.root);

        dragClone.transform.localScale = Vector3.one;
        cloneRect = dragClone.GetComponent<RectTransform>();

        CanvasGroup cloneCanvasGroup = dragClone.GetComponent<CanvasGroup>();
        if (cloneCanvasGroup != null)
        {
            cloneCanvasGroup.blocksRaycasts = false;
            cloneCanvasGroup.alpha = 0.8f;
        }

        Active_Skill cloneScript = dragClone.GetComponent<Active_Skill>();
        if (cloneScript != null) Destroy(cloneScript);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragClone != null && cloneRect != null && mainCanvas != null)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mainCanvas.transform as RectTransform, eventData.position, mainCanvas.worldCamera, out Vector2 localPoint))
            {
                cloneRect.anchoredPosition = localPoint;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        if (eventData.pointerCurrentRaycast.gameObject != null)
        {
            SkillSlotUI targetSlot = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<SkillSlotUI>();

            if (targetSlot != null && targetSlot.skillData == null)
            {
                // 1. 툴팁 텍스트 추출
                string myTooltipString = "";
                if (tooltipText != null)
                {
                    Text tText = tooltipText.GetComponentInChildren<Text>();
                    if (tText != null) myTooltipString = tText.text;
                }

                // 2. ⭐ [핵심 추가] 원본 이미지 아이콘 직접 추출
                Sprite myIconSprite = (skillIconImage != null) ? skillIconImage.sprite : null;

                // 데이터, 이미지, 텍스트를 삼위일체로 슬롯에 등록합니다.
                targetSlot.RegisterSkill(this.skillData, myIconSprite, myTooltipString);
            }
        }

        if (dragClone != null) Destroy(dragClone);
    }
}