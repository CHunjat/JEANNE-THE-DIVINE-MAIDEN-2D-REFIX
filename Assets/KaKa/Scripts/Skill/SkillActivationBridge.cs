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

    private SkillData activeHeavySkill;   // 지금 차징 중인 Heavy 스킬 (Tab으로 슬롯 바뀌어도 흔들리지 않게 캐싱)
    private bool chargedExtraPaid = false;

    private SkillData pendingHeavySkill;
    private bool heavyMpConsumed;

    private SkillData pendingLightningSkill;
    private bool lightningMpConsumed;

    private SkillData pendingHealSkill;
    private bool healMpConsumed;

    private object lastObservedState;   // 상태 진입 "순간"을 감지하기 위한 이전 프레임 상태 기록
    private void Update()
    {
        if (playerController == null || skillRotationManager == null) return;

        HandleSlotRotation();
        HandleSkillActivation();
        HandleDeferredMpConsumption();
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

        // ==========================================
        // [수정된 부분] 스킬이 없을 때의 강력한 예외 처리
        if (equipped == null)
        {
            // 1. E키 입력 무효화
            playerController.inputReader.HAttackPressed = false;

            // 2. 혹시나 E를 누르면서 스프린트가 켜지는 현상 방지
            playerController.isSprinting = false;

            // 3. 기 모으기 홀드 상태(HeavyAttackHeld)가 켜져 있다면 강제 OFF
            playerController.inputReader.HeavyAttackHeld = false;

            return; // 강제 전이(ChangeState) 없이 여기서 로직 종료
        }
        // ==========================================

        // 스킬이 존재하지만 시전할 수 없는 상태일 때만 입력을 무시합니다.
        if (playerController.isSprinting || !playerController.IsGrounded() || isBusyWithSkill)
        {
            playerController.inputReader.HAttackPressed = false;
            return;
        }
        // ==========================================

        // 스킬이 존재하지만 시전할 수 없는 상태(달리기, 공중, 이미 시전 중)일 때만 입력을 무시합니다.
        if (playerController.isSprinting || !playerController.IsGrounded() || isBusyWithSkill)
        {
            playerController.inputReader.HAttackPressed = false;
            return;
        }


        switch (equipped.skilltype)
        {
            case SkillType.Heavy:
                if (equipped.attackData != null &&
                    playerController.playerStats.currentMp >= equipped.mpCost &&
                    playerController.attackLibrary != null &&
                    heavyBaseIndex + 1 < playerController.attackLibrary.Count)
                {
                    playerController.attackLibrary[heavyBaseIndex] = equipped.attackData;
                    playerController.attackLibrary[heavyBaseIndex + 1] =
                        equipped.attackDataCharged != null ? equipped.attackDataCharged : equipped.attackData;

                    pendingHeavySkill = equipped;
                    heavyMpConsumed = false;

                    playerController.StateMachine.ChangeState(playerController.HeavyReadyState);
                }
                break;

            case SkillType.Lightning:
                if (equipped.attackData != null &&
                    playerController.playerStats.currentMp >= equipped.mpCost &&
                    playerController.attackLibrary != null &&
                    lightningIndex < playerController.attackLibrary.Count)
                {
                    playerController.attackLibrary[lightningIndex] = equipped.attackData;

                    pendingLightningSkill = equipped;
                    lightningMpConsumed = false;

                    playerController.StateMachine.ChangeState(playerController.LightningReadyState);
                }
                break;

            case SkillType.Heal:
                if (equipped.healData != null && playerController.playerStats.currentMp >= equipped.mpCost)
                {
                    //playerController.healAmount = equipped.healData.healAmount;

                    pendingHealSkill = equipped;
                    healMpConsumed = false;

                    playerController.StateMachine.ChangeState(playerController.HealState);
                }
                break;
        }

        playerController.inputReader.HAttackPressed = false;
    }

    private SkillData GetEquippedSkillData()
    {
        int idx = (int)playerController.currentSkillSlot;
        if (idx < 0 || idx >= skillRotationManager.skills.Length) return null;
        return skillRotationManager.skills[idx];
    }

    private void HandleDeferredMpConsumption()
    {
        object current = playerController.StateMachine.CurrentState;

        // Heavy: HeavyAttackState 진입 순간 결제 (기존과 동일)
        if (current == playerController.HeavyAttackState && lastObservedState != playerController.HeavyAttackState)
        {
            if (pendingHeavySkill != null && !heavyMpConsumed)
            {
                heavyMpConsumed = true;
                float finalCost = (playerController.currentChargeLevel == 2)
                    ? pendingHeavySkill.mpCostChargedExtra
                    : pendingHeavySkill.mpCost;

                if (!playerController.playerStats.TryConsumeMp(finalCost))
                {
                    Debug.Log("MP 부족으로 데미지가 1단계 수준으로 하향 적용됩니다.");
                    if (playerController.attackLibrary != null && heavyBaseIndex + 1 < playerController.attackLibrary.Count)
                    {
                        playerController.attackLibrary[heavyBaseIndex + 1] = pendingHeavySkill.attackData;
                    }
                }
            }
        }

        // Lightning: LightningAttackState 진입 순간 결제 (기존과 동일)
        if (current == playerController.LightningAttackState && lastObservedState != playerController.LightningAttackState)
        {
            if (pendingLightningSkill != null && !lightningMpConsumed)
            {
                lightningMpConsumed = true;
                playerController.playerStats.TryConsumeMp(pendingLightningSkill.mpCost);
            }
        }

        // ★ Heal: HealState 안에서, 실제 회복 애니메이션이 50% 지점을 지나는 순간 결제
        if (current == playerController.HealState)
        {
            if (pendingHealSkill != null && !healMpConsumed)
            {
                AnimatorStateInfo info = playerController.animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(playerController.anim_Heal) && info.normalizedTime >= 0.5f)
                {
                    healMpConsumed = true;
                    playerController.playerStats.TryConsumeMp(pendingHealSkill.mpCost);
                    playerController.playerStats.Heal(pendingHealSkill.healData.healAmount);
                }
            }
        }
        else
        {
            // 힐 상태를 벗어났는데(피격 등으로 중간에 끊김) 아직 결제 전이었다면 그냥 보류 취소
            pendingHealSkill = null;
        }

        lastObservedState = current;
    }
}