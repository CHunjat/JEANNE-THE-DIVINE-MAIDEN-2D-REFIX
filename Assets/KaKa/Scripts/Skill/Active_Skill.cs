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

    [Header("Drag Visual")]
    public RectTransform dragVisual;

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
        Debug.Log($"[스킬 클릭] {gameObject.name}");

        if (manager != null)
        {
            manager.SelectSkill(skillIndex);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} : ActiveSkillManager가 아직 연결되지 않았습니다.");
        }
    }

    // --- 3. 드래그 앤 드롭 기능 (유지) ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // ★ 드래그 시작 시에도 해당 스킬 선택
        if (manager != null)
        {
            manager.SelectSkill(skillIndex);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = dragAlpha;

        if (dragVisual == null || mainCanvas == null)
            return;

        if (canvasGroup != null)
            canvasGroup.alpha = dragAlpha;

        if (dragVisual == null || mainCanvas == null)
            return;

        // 원형 프레임 + 아이콘 복제
        dragClone = Instantiate(
            dragVisual.gameObject,
            mainCanvas.transform
        );

        dragClone.name = "DragSkillVisual";

        cloneRect = dragClone.GetComponent<RectTransform>();

        Vector3 originalLossyScale = dragVisual.lossyScale;
        Vector3 canvasLossyScale = mainCanvas.transform.lossyScale;

        cloneRect.localScale = new Vector3(
            originalLossyScale.x / canvasLossyScale.x,
            originalLossyScale.y / canvasLossyScale.y,
            1f
        );

        // ★ 복제본 안의 모든 Graphic Raycast 차단
        Graphic[] graphics =
            dragClone.GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = false;
        }

        // ★ CanvasGroup으로도 한 번 더 완전히 차단
        CanvasGroup cloneCanvasGroup =
            dragClone.GetComponent<CanvasGroup>();

        if (cloneCanvasGroup == null)
        {
            cloneCanvasGroup =
                dragClone.AddComponent<CanvasGroup>();
        }

        cloneCanvasGroup.blocksRaycasts = false;
        cloneCanvasGroup.interactable = false;
        cloneCanvasGroup.ignoreParentGroups = true;
        cloneCanvasGroup.alpha = 0.85f;

        UpdateDragIconPosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateDragIconPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

        if (hitObject != null)
        {
            SkillSlotUI targetSlot =
                hitObject.GetComponentInParent<SkillSlotUI>();

            if (targetSlot != null)
            {
                Debug.Log(
                    $"[드롭 슬롯] {targetSlot.name} / " +
                    $"기존 SkillData: {(targetSlot.skillData != null ? targetSlot.skillData.name : "None")} / " +
                    $"드래그 SkillData: {(skillData != null ? skillData.name : "None")}"
                );

                if (targetSlot.skillData == null)
                {
                    Sprite myIconSprite =
                        skillIconImage != null
                            ? skillIconImage.sprite
                            : null;

                    Debug.Log($"[등록 호출] {skillData?.name} → {targetSlot.name}");

                    targetSlot.RegisterSkill(
                        skillData,
                        myIconSprite
                    );

                    Debug.Log(
                        $"[등록 후] {targetSlot.name} SkillData = " +
                        $"{(targetSlot.skillData != null ? targetSlot.skillData.name : "None")}"
                    );
                }
                else
                {
                    Debug.LogWarning(
                        $"[등록 실패] {targetSlot.name}은 이미 스킬이 등록되어 있음"
                    );
                }
            }
            else
            {
                Debug.LogWarning(
                    $"[등록 실패] {hitObject.name}에서 SkillSlotUI를 찾지 못함"
                );
            }
        }

        if (dragClone != null)
            Destroy(dragClone);
    }

    private void UpdateDragIconPosition(PointerEventData eventData)
    {
        if (dragClone == null || cloneRect == null || mainCanvas == null)
            return;

        RectTransform canvasRect = mainCanvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            mainCanvas.worldCamera,
            out Vector2 localPoint))
        {
            cloneRect.anchoredPosition = localPoint;
        }
    }

    private string GetHierarchyPath(Transform target)
    {
        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}