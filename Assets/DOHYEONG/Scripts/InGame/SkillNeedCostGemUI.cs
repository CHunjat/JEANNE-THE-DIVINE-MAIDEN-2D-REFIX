using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillNeedCostGemUI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SkillRotationManager skillRotationManager;

    [Header("밝게 켜질 Full 젬 오브젝트들 (1~3)")]
    [SerializeField] private GameObject[] fullGemObjects = new GameObject[3];

    [Header("팝 효과")]
    [SerializeField] private bool usePopEffect = true;
    [SerializeField] private float popScale = 0.2f;
    [SerializeField] private float popDuration = 0.2f;

    private int lastDisplayedCost = -1;
    private PlayerController.SkillSlot lastSelectedSlot;
    private Vector3[] baseScales;

    private void Start()
    {
        baseScales = new Vector3[fullGemObjects.Length];

        for (int i = 0; i < fullGemObjects.Length; i++)
        {
            if (fullGemObjects[i] != null)
            {
                baseScales[i] =
                    fullGemObjects[i].GetComponent<RectTransform>().localScale;
            }
        }

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (skillRotationManager == null)
            skillRotationManager = FindFirstObjectByType<SkillRotationManager>();

        if (playerController == null || skillRotationManager == null)
        {
            Debug.LogError("SkillNeedCostGemUI : 필요한 참조를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        lastSelectedSlot = playerController.currentSkillSlot;
        RefreshUI(true);
    }

    private void Update()
    {
        if (playerController == null || skillRotationManager == null)
            return;

        int currentCost = GetDisplayedCost();

        if (playerController.currentSkillSlot != lastSelectedSlot ||
            currentCost != lastDisplayedCost)
        {
            lastSelectedSlot = playerController.currentSkillSlot;
            RefreshUI(false);
        }
    }

    private int GetDisplayedCost()
    {
        int slotIndex = (int)playerController.currentSkillSlot;

        if (slotIndex < 0 || slotIndex >= skillRotationManager.skills.Length)
            return 0;

        SkillData skill = skillRotationManager.skills[slotIndex];

        if (skill == null)
            return 0;

        // Heavy는 기본 1, 풀차지면 3
        if (skill.skilltype == SkillType.Heavy)
        {
            bool isUsingHeavy =
                playerController.StateMachine.CurrentState == playerController.HeavyReadyState ||
                playerController.StateMachine.CurrentState == playerController.HeavyChargeState ||
                playerController.StateMachine.CurrentState == playerController.HeavyAttackState;

            if (isUsingHeavy && playerController.currentChargeLevel == 2)
                return 3;

            return 1;
        }

        // 일반 스킬은 cost 문자열 사용
        if (int.TryParse(skill.cost, out int normalCost))
        {
            return Mathf.Clamp(normalCost, 0, 3);
        }

        // "1~3" 같은 문자열은 Heavy가 아니면 일단 1로 처리
        return 1;
    }

    private void RefreshUI(bool instant)
    {
        int newCost = GetDisplayedCost();

        for (int i = 0; i < fullGemObjects.Length; i++)
        {
            GameObject fullGem = fullGemObjects[i];

            if (fullGem == null)
                continue;

            bool shouldBeOn = i < newCost;
            bool wasOn = i < lastDisplayedCost;

            if (fullGem.activeSelf != shouldBeOn)
            {
                fullGem.SetActive(shouldBeOn);
            }

            RectTransform rect = fullGem.GetComponent<RectTransform>();
            if (rect == null)
                continue;

            rect.DOKill();
            rect.localScale = baseScales[i];

            // 새로 켜지는 젬만 팝 효과
            if (!instant && usePopEffect && shouldBeOn && !wasOn)
            {
                Vector3 punchAmount = new Vector3( baseScales[i].x * popScale, baseScales[i].y * popScale, 0);

                rect.DOPunchScale(
                    punchAmount,
                    popDuration,
                    4,
                    0.5f
                );
            }
        }

        lastDisplayedCost = newCost;
    }

    private void OnDestroy()
    {
        foreach (GameObject gem in fullGemObjects)
        {
            if (gem == null) continue;

            RectTransform rect = gem.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.DOKill();
            }
        }
    }
}