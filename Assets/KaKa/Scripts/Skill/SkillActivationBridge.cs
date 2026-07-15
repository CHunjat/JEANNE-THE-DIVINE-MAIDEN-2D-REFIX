using UnityEngine;

/// <summary>
/// PlayerController / PlayerHealState / ExecuteAttack / ExecuteChargeAttack 원본 코드를
/// 전혀 수정하지 않고, 장착화면에서 등록한 SkillData를 기준으로 실제 스킬을 발동시키는 브릿지.
///
/// [필수 세팅]
/// Edit > Project Settings > Script Execution Order 에서
/// 이 스크립트(SkillActivationBridge)를 Default Time보다 "위쪽"에 오도록 배치해주세요.
/// → PlayerController.Update()보다 먼저 실행되어야 HAttackPressed 입력을 먼저 소모해서
///    PlayerController.HandleActiveSkillInput()의 중복 발동을 막을 수 있습니다.
///
/// [필수 세팅 2]
/// heavyBaseIndex / lightningIndex 값은 실제 애니메이션 이벤트에
/// ExecuteChargeAttack(baseIndex) / ExecuteAttack(index)로 박제된 숫자와
/// 반드시 동일하게 맞춰주세요. (Window > Animation 에서 해당 클립의 이벤트 확인)
/// </summary>
public class SkillActivationBridge : MonoBehaviour
{
    [Header("연결")]
    public PlayerController playerController;
    public SkillRotationManager skillRotationManager;

    [Header("슬롯 전환 키")]
    public KeyCode rotateKey = KeyCode.Tab;

    [Header("attackLibrary 인덱스 매핑")]
    [Tooltip("HeavyChargeState 애니메이션 이벤트의 ExecuteChargeAttack(baseIndex) 값과 동일하게")]
    public int heavyBaseIndex = 0; // 실제 사용 인덱스 = heavyBaseIndex + (chargeLevel - 1)

    [Tooltip("LightningAttackState 애니메이션 이벤트의 ExecuteAttack(index) 값과 동일하게")]
    public int lightningIndex = 2;

    private void Update()
    {
        if (playerController == null || skillRotationManager == null) return;

        HandleSlotRotation();
        HandleSkillActivation();
    }

    // Tab 키로 currentSkillSlot 순환 (UI 캐러셀은 SkillRotationManager.Update()의 폴링이 자동 반응)
    private void HandleSlotRotation()
    {
        if (Input.GetKeyDown(rotateKey))
        {
            int next = ((int)playerController.currentSkillSlot - 1 + 3) % 3;   // ★ +1 → -1로 변경
            playerController.currentSkillSlot = (PlayerController.SkillSlot)next;
        }
    }

    private void HandleSkillActivation()
    {
        // 원본 HandleActiveSkillInput()과 동일한 입력 플래그를 사용
        if (!playerController.inputReader.HAttackPressed) return;

        SkillData equipped = GetEquippedSkillData();

        bool isBusyWithSkill =
            playerController.StateMachine.CurrentState == playerController.HeavyReadyState ||
            playerController.StateMachine.CurrentState == playerController.HeavyChargeState ||
            playerController.StateMachine.CurrentState == playerController.HeavyAttackState ||
            playerController.StateMachine.CurrentState == playerController.LightningReadyState ||
            playerController.StateMachine.CurrentState == playerController.LightningChargeState ||
            playerController.StateMachine.CurrentState == playerController.LightningAttackState ||
            playerController.StateMachine.CurrentState == playerController.HealState;

        // 원본과 동일한 가드 조건 (땅에 있어야 함 / 스프린트 중 아님 / 이미 스킬 중 아님 / 장착된 스킬 있어야 함)
        if (equipped == null || playerController.isSprinting || !playerController.IsGrounded() || isBusyWithSkill)
        {
            playerController.inputReader.HAttackPressed = false;
            return;
        }

        switch (equipped.skilltype)
        {
            case SkillType.Heavy:
                if (equipped.attackData != null &&
                    playerController.attackLibrary != null &&
                    heavyBaseIndex + 1 < playerController.attackLibrary.Count)
                {
                    playerController.attackLibrary[heavyBaseIndex] = equipped.attackData;
                    playerController.attackLibrary[heavyBaseIndex + 1] =
                        equipped.attackDataCharged != null ? equipped.attackDataCharged : equipped.attackData;

                    playerController.StateMachine.ChangeState(playerController.HeavyReadyState);
                }
                break;

            case SkillType.Lightning:
                if (equipped.attackData != null &&
                    playerController.attackLibrary != null &&
                    lightningIndex < playerController.attackLibrary.Count)
                {
                    playerController.attackLibrary[lightningIndex] = equipped.attackData;

                    playerController.StateMachine.ChangeState(playerController.LightningReadyState);
                }
                break;

            case SkillType.Heal:
                if (equipped.healData != null)
                {
                    if (playerController.playerStats.currentMp >= equipped.healData.healMpCost)
                    {
                        playerController.healAmount = equipped.healData.healAmount;
                        playerController.healMpCost = equipped.healData.healMpCost;

                        playerController.StateMachine.ChangeState(playerController.HealState);
                    }
                    else
                    {
                        Debug.Log("마나가 부족하여 힐 스킬을 사용할 수 없습니다!");
                    }
                }
                break;
        }

        // 핵심: 이 프레임에 PlayerController.HandleActiveSkillInput()이 같은 입력을
        // 다시 읽고 중복 발동하지 않도록 여기서 입력을 소모합니다.
        // (Script Execution Order에서 이 스크립트가 PlayerController보다 먼저 실행돼야 함)
        playerController.inputReader.HAttackPressed = false;
    }

    private SkillData GetEquippedSkillData()
    {
        int idx = (int)playerController.currentSkillSlot;
        if (idx < 0 || idx >= skillRotationManager.skills.Length) return null;
        return skillRotationManager.skills[idx];
    }
}