using UnityEngine;
using DG.Tweening;

public class SkillRegistrationKeyboardController : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private ActiveSkillManager activeSkillManager;
    [SerializeField] private SkillUIManager skillUIManager;

    [Header("이동")]
    [SerializeField] private float moveDuration = 0.15f;

    private bool isRegisterSelecting = false;

    private int currentSlotIndex = -1;

    private Active_Skill selectedActiveSkill;
    private SkillData selectedSkill;

    private GameObject previewObject;
    private RectTransform previewRect;

    [Header("플레이어 스킬 입력 차단")]
    [SerializeField] private SkillActivationBridge skillActivationBridge;
    [SerializeField] private PlayerController playerController;

    [Header("슬롯 표시 위치")]
    [SerializeField] private RectTransform[] slotTargets;

    private void Update()
    {
        // ============================
        // 일반 스킬 선택 상태
        // ============================

        if (!isRegisterSelecting)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                BeginRegisterSelection();
            }

            return;
        }


        // ============================
        // 등록 슬롯 선택 상태
        // ============================

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveSlot(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            ConfirmRegister();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelRegister();
        }
    }


    // =====================================================
    // E 첫 입력
    // =====================================================

    private void BeginRegisterSelection()
    {
        if (activeSkillManager == null ||
            skillUIManager == null ||
            skillUIManager.skillSlots == null)
        {
            return;
        }


        int skillIndex =
            activeSkillManager.CurrentSelectedIndex;


        // 선택된 스킬 없음
        if (skillIndex < 0 ||
            skillIndex >= activeSkillManager.activeSkills.Length)
        {
            return;
        }


        selectedActiveSkill =
            activeSkillManager.activeSkills[skillIndex];


        if (selectedActiveSkill == null ||
            selectedActiveSkill.skillData == null)
        {
            return;
        }


        selectedSkill =
            selectedActiveSkill.skillData;


        // 첫 번째 빈 슬롯
        currentSlotIndex =
            FindFirstEmptySlot();


        if (currentSlotIndex == -1)
        {
            Debug.Log("등록 가능한 빈 슬롯이 없습니다.");
            return;
        }


        isRegisterSelecting = true;


        // 왼쪽 목록의 방향키 입력 잠시 정지
        activeSkillManager.enabled = false;


        CreatePreview();

        MovePreviewImmediate();
    }


    // =====================================================
    // 첫 번째 빈 슬롯 찾기
    // =====================================================

    private int FindFirstEmptySlot()
    {
        int bestIndex = -1;
        float highestY = float.MinValue;

        for (int i = 0; i < skillUIManager.skillSlots.Length; i++)
        {
            SkillSlotUI slot = skillUIManager.skillSlots[i];

            if (!IsAvailableSlot(slot))
                continue;

            RectTransform rect =
                slot.GetComponent<RectTransform>();

            if (rect == null)
                continue;

            float y = rect.position.y;

            // 화면에서 가장 위에 있는 빈 슬롯 선택
            if (y > highestY)
            {
                highestY = y;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    // =====================================================
    // 등록 가능한 슬롯인지
    // 잠금 X
    // 등록된 스킬 X
    // =====================================================

    private bool IsAvailableSlot(SkillSlotUI slot)
    {
        if (slot == null)
            return false;

        if (slot.IsLocked)
            return false;

        if (slot.skillData != null)
            return false;

        return true;
    }


    // =====================================================
    // ↑ ↓
    // 빈 슬롯만 찾아 이동
    // =====================================================

    private void MoveSlot(int direction)
    {
        SkillSlotUI currentSlot = GetCurrentSlot();

        if (currentSlot == null)
            return;

        RectTransform currentRect =
            currentSlot.GetComponent<RectTransform>();

        if (currentRect == null)
            return;

        float currentY = currentRect.position.y;

        int bestIndex = -1;
        float bestDistance = float.MaxValue;


        for (int i = 0; i < skillUIManager.skillSlots.Length; i++)
        {
            if (i == currentSlotIndex)
                continue;

            SkillSlotUI slot =
                skillUIManager.skillSlots[i];

            // 잠금 / 이미 등록된 슬롯 건너뜀
            if (!IsAvailableSlot(slot))
                continue;


            RectTransform rect =
                slot.GetComponent<RectTransform>();

            if (rect == null)
                continue;


            float targetY = rect.position.y;


            // ↓
            if (direction > 0)
            {
                if (targetY >= currentY)
                    continue;
            }

            // ↑
            else
            {
                if (targetY <= currentY)
                    continue;
            }


            float distance =
                Mathf.Abs(targetY - currentY);


            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }


        if (bestIndex == -1)
            return;


        currentSlotIndex = bestIndex;

        MovePreview();
    }


    // =====================================================
    // 왼쪽 리스트 아이콘 모습 그대로 복제
    // =====================================================

    private void CreatePreview()
    {
        if (selectedActiveSkill == null ||
            selectedActiveSkill.dragVisual == null)
        {
            return;
        }


        Canvas canvas =
            selectedActiveSkill
            .GetComponentInParent<Canvas>();


        if (canvas == null)
            return;


        previewObject =
            Instantiate(
                selectedActiveSkill.dragVisual.gameObject,
                canvas.transform
            );


        previewObject.name =
            "SkillRegisterPreview";


        previewRect =
            previewObject.GetComponent<RectTransform>();

        // ★ 원래 스킬 리스트에서 보이던 크기 그대로 맞춤
        Vector3 originalLossyScale =
            selectedActiveSkill.dragVisual.lossyScale;

        Vector3 canvasLossyScale =
            canvas.transform.lossyScale;

        previewRect.localScale = new Vector3(
            originalLossyScale.x / canvasLossyScale.x,
            originalLossyScale.y / canvasLossyScale.y,
            1f);

        // 최상단에 보이게
        previewObject.transform.SetAsLastSibling();


        // 클릭 방해 방지
        CanvasGroup canvasGroup =
            previewObject.GetComponent<CanvasGroup>();


        if (canvasGroup == null)
        {
            canvasGroup =
                previewObject.AddComponent<CanvasGroup>();
        }


        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.ignoreParentGroups = true;
        canvasGroup.alpha = 1f;
    }


    // =====================================================
    // 처음 슬롯으로 바로 이동
    // =====================================================

    private void MovePreviewImmediate()
    {
        if (previewRect == null)
            return;

        if (slotTargets == null ||
            currentSlotIndex < 0 ||
            currentSlotIndex >= slotTargets.Length ||
            slotTargets[currentSlotIndex] == null)
        {
            return;
        }

        previewRect.DOKill();

        previewRect.position =
            slotTargets[currentSlotIndex].position;
    }


    // =====================================================
    // 위 / 아래 이동
    // =====================================================

    private void MovePreview()
    {
        if (previewRect == null)
            return;

        if (slotTargets == null ||
            currentSlotIndex < 0 ||
            currentSlotIndex >= slotTargets.Length ||
            slotTargets[currentSlotIndex] == null)
        {
            return;
        }

        previewRect.DOKill();

        previewRect
            .DOMove(
                slotTargets[currentSlotIndex].position,
                moveDuration
            )
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }


    // =====================================================
    // E 두 번째 입력
    // =====================================================

    private void ConfirmRegister()
    {
        SkillSlotUI slot =
            GetCurrentSlot();


        if (slot == null ||
            selectedSkill == null)
        {
            return;
        }


        // 혹시 이동 중 상태가 바뀌었다면 안전 체크
        if (!IsAvailableSlot(slot))
        {
            return;
        }


        slot.RegisterSkill(
            selectedSkill,
            selectedSkill.skillIcon
        );


        EndRegisterSelection();
    }


    // =====================================================
    // 취소
    // =====================================================

    private void CancelRegister()
    {
        EndRegisterSelection();
    }


    private void EndRegisterSelection()
    {
        isRegisterSelecting = false;

        currentSlotIndex = -1;
        selectedSkill = null;
        selectedActiveSkill = null;


        if (previewRect != null)
        {
            previewRect.DOKill();
        }


        if (previewObject != null)
        {
            Destroy(previewObject);
        }


        previewObject = null;
        previewRect = null;


        if (activeSkillManager != null)
        {
            activeSkillManager.enabled = true;
        }
    }


    private SkillSlotUI GetCurrentSlot()
    {
        if (skillUIManager == null ||
            skillUIManager.skillSlots == null)
        {
            return null;
        }


        if (currentSlotIndex < 0 ||
            currentSlotIndex >=
            skillUIManager.skillSlots.Length)
        {
            return null;
        }


        return skillUIManager
            .skillSlots[currentSlotIndex];
    }
    private void OnEnable()
    {
        // 플레이어 입력 차단
        if (playerController != null)
        {
            playerController.SetCanControl(false);

            if (playerController.inputReader != null)
            {
                playerController.inputReader.HAttackPressed = false;
            }
        }

        // 별도로 돌아가는 스킬 발동 브릿지도 차단
        if (skillActivationBridge != null)
        {
            skillActivationBridge.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (previewRect != null)
        {
            previewRect.DOKill();
        }

        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        if (activeSkillManager != null)
        {
            activeSkillManager.enabled = true;
        }

        // 메뉴에서 마지막으로 누른 E 제거
        if (playerController != null &&
            playerController.inputReader != null)
        {
            playerController.inputReader.HAttackPressed = false;
        }

        // 전투 스킬 시스템 자체는 다시 켜도 됨
        // PlayerController.canControl이 false라면 실제 발동은 차단됨
        if (skillActivationBridge != null)
        {
            skillActivationBridge.enabled = true;
        }

        isRegisterSelecting = false;
    }
}