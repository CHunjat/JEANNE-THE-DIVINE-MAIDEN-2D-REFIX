using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

public class PlayerSFXController : MonoBehaviour
{
    private bool sprintJumpSoundPlayed = false;

    private EventInstance lightningCutReadyInstance;
    private EventInstance chargingInstance;
    private EventInstance WallingInstance;
    private Coroutine chargingCompleteCoroutine;

    // ── 이동 ────────────────────────────────
    public void OnFootStepRun() => RuntimeManager.PlayOneShot("event:/Player/Player_Foot_Step_Run", transform.position);
    public void OnFootStepSprint() => RuntimeManager.PlayOneShot("event:/Player/Player_Foot_Step_Sprint", transform.position);
    public void OnFootStepJump() => RuntimeManager.PlayOneShot("event:/Player/Player_Foot_Step_Jump", transform.position);

    public void OnSprintJump()
    {
        if (sprintJumpSoundPlayed) return;
        sprintJumpSoundPlayed = true;
        RuntimeManager.PlayOneShot("event:/Player/Player_Foot_Step_Jump", transform.position);
    }

    public void OnSprintJump_Landing()
    {
        sprintJumpSoundPlayed = false;
    }
    public void OnFootStepJumpLand() => RuntimeManager.PlayOneShot("event:/Player/Player_Foot_Step_Jump _Land", transform.position);
    public void OnMovementDash() => RuntimeManager.PlayOneShot("event:/Player/Player_Movement_Dash", transform.position);


    public void OnMovementWallingStartSFX()
    {
        WallingInstance = RuntimeManager.CreateInstance("event:/Player/Player_Movement_Walling");
        WallingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        WallingInstance.start();
    }

    public void OnMovementWallingStopSFX()
    {
        if (WallingInstance.isValid())
        {
            WallingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            WallingInstance.release();
        }
    }





    // ── 공격 ────────────────────────────────
    public void OnNormalAttackPierce() => RuntimeManager.PlayOneShot("event:/Player/Player_Normal_Attack_Swing_Pierce", transform.position);
    public void OnNormalAttackSlash() => RuntimeManager.PlayOneShot("event:/Player/Player_Normal_Attack_Swing_Slash", transform.position);
    public void OnAirAttackHardAttack() => RuntimeManager.PlayOneShot("event:/Player/Player_Air_Attack_Hard_Attack", transform.position);

    public void OnNormalAttackChrarge() => RuntimeManager.PlayOneShot("event:/Player/Player_Normal_Attack_Charging", transform.position);

    // ── 스킬 차징 (시작/종료 분리) ────────────────────────────────
    public void OnSkillChargingStartSFX()
    {
        chargingInstance = RuntimeManager.CreateInstance("event:/Player/Player_Skill_Attack_Charging");
        chargingInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        chargingInstance.start();
        chargingCompleteCoroutine = StartCoroutine(ChargingCompleteAfterDelay(2f));
    }

    public void OnSkillChargingStopSFX()
    {
        if (chargingCompleteCoroutine != null)
        {
            StopCoroutine(chargingCompleteCoroutine);
            chargingCompleteCoroutine = null;
        }
        if (chargingInstance.isValid())
        {
            chargingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            chargingInstance.release();
        }
    }

    public void OnSkillChargingComplete()
    {
        RuntimeManager.PlayOneShot("event:/Player/Player_Skill_Attack_Charging_Complete", transform.position);
    }

    private IEnumerator ChargingCompleteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnSkillChargingComplete();
        chargingCompleteCoroutine = null;
    }
    public void OnSkillChargingSlash() => RuntimeManager.PlayOneShot("event:/Player/Player_Skill_Attack_Charging_Slash", transform.position);

    // ── 스킬 라이트닝컷 레디 (시작/종료 분리) ────────────────────────────────
    public void OnSkillLightningCutReadyStartSFX()
    {
        lightningCutReadyInstance = RuntimeManager.CreateInstance("event:/Player/Player_Skill_Attack_Lightning_Cut_Ready");
        lightningCutReadyInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        lightningCutReadyInstance.start();
    }

    public void OnSkillLightningCutReadyStopSFX()
    {
        if (lightningCutReadyInstance.isValid())
        {
            lightningCutReadyInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            lightningCutReadyInstance.release();
        }
    }

    public void OnSkillLightningCutSlash() => RuntimeManager.PlayOneShot("event:/Player/Player_Skill_Attack_Lightning_Cut_Slash", transform.position);

    // ── 스킬 그래플링 ────────────────────────────────
    public void OnSkillGrapling() => RuntimeManager.PlayOneShot("event:/Player/Interaction/Player_Skill_Grapling", transform.position);

    // ── 방어 ────────────────────────────────
    public void OnShieldReady() => RuntimeManager.PlayOneShot("event:/Player/Player_Shield_Ready", transform.position);

    // ── 상호작용 ────────────────────────────────
    public void OnShieldParry() => RuntimeManager.PlayOneShot("event:/Player/Interaction/Player_Shield_Parry", transform.position);
    public void OnShieldParryCounter() => RuntimeManager.PlayOneShot("event:/Player/Interaction/Player_Shield_Parry_Counter", transform.position);
    public void OnShieldGuard() => RuntimeManager.PlayOneShot("event:/Player/Interaction/Player_Shield_Guard", transform.position);
    public void OnPlayerAttackHit() => RuntimeManager.PlayOneShot("event:/Player/Interaction/Player_Attack_Hit", transform.position);
    public void OnPlayerDamagedHurtVoice() => RuntimeManager.PlayOneShot("event:/Player/Interaction/Player_Damaged_Hurt_Voice", transform.position);
    public void OnFinalBossAttackHit() => RuntimeManager.PlayOneShot("event:/FinalBoss/Interaction/Final_Boss_Attack_Hit", transform.position);

    // ── 회복 ────────────────────────────────
    public void OnHeal() => RuntimeManager.PlayOneShot("event:/Player/Player_Skill_Heal", transform.position);

    private void OnDestroy()
    {
        if (chargingInstance.isValid())
        {
            chargingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            chargingInstance.release();
        }
        if (lightningCutReadyInstance.isValid())
        {
            lightningCutReadyInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            lightningCutReadyInstance.release();
        }
    }
}