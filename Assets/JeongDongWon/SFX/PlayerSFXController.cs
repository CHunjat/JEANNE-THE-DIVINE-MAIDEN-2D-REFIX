using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;

public class PlayerSFXController : MonoBehaviour
{
    private bool sprintJumpSoundPlayed = false;

    private List<EventInstance> activeInstances = new();

    private EventInstance lightningCutReadyInstance;
    private EventInstance chargingInstance;
    private EventInstance WallingInstance;
    private Coroutine chargingCompleteCoroutine;

    // ── 내부 재생 ────────────────────────────
    private EventInstance Play(string path)
    {
        var instance = RuntimeManager.CreateInstance(path);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
        instance.release();
        activeInstances.Add(instance);
        return instance;
    }
    // ── 사망 ────────────────────────────────
    public void OnPlayerDead() => Play("event:/Player/Player_Dead");

    // ── 이동 ────────────────────────────────
    public void OnFootStepRun() => Play("event:/Player/Player_Foot_Step_Run");
    public void OnFootStepSprint() => Play("event:/Player/Player_Foot_Step_Sprint");
    public void OnFootStepJump() => Play("event:/Player/Player_Foot_Step_Jump");

    public void OnSprintJump()
    {
        if (sprintJumpSoundPlayed) return;
        sprintJumpSoundPlayed = true;
        Play("event:/Player/Player_Foot_Step_Jump");
    }

    public void OnSprintJump_Landing()
    {
        sprintJumpSoundPlayed = false;
    }
    public void OnFootStepJumpLand() => Play("event:/Player/Player_Foot_Step_Jump _Land");
    public void OnMovementDash() => Play("event:/Player/Player_Movement_Dash");

    public void OnMovementWallingStartSFX()
    {
        WallingInstance = Play("event:/Player/Player_Movement_Walling");
    }

    public void OnMovementWallingStopSFX()
    {
        if (WallingInstance.isValid())
        {
            WallingInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            WallingInstance.release();
            activeInstances.Remove(WallingInstance);
        }
    }

    // ── 공격 ────────────────────────────────
    public void OnNormalAttackPierce() => Play("event:/Player/Player_Normal_Attack_Swing_Pierce");
    public void OnNormalAttackSlash() => Play("event:/Player/Player_Normal_Attack_Swing_Slash");
    public void OnAirAttackHardAttack() => Play("event:/Player/Player_Air_Attack_Hard_Attack");
    public void OnNormalAttackChrarge() => Play("event:/Player/Player_Normal_Attack_Charging");

    // ── 스킬 차징 (시작/종료 분리) ────────────────────────────────
    public void OnSkillChargingStartSFX()
    {
        chargingInstance = Play("event:/Player/Player_Skill_Attack_Charging");
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
            activeInstances.Remove(chargingInstance);
        }
    }

    public void OnSkillChargingComplete()
    {
        Play("event:/Player/Player_Skill_Attack_Charging_Complete");
    }

    private IEnumerator ChargingCompleteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        OnSkillChargingComplete();
        chargingCompleteCoroutine = null;
    }
    public void OnSkillChargingSlash() => Play("event:/Player/Player_Skill_Attack_Charging_Slash");

    // ── 스킬 라이트닝컷 레디 (시작/종료 분리) ────────────────────────────────
    public void OnSkillLightningCutReadyStartSFX()
    {
        lightningCutReadyInstance = Play("event:/Player/Player_Skill_Attack_Lightning_Cut_Ready");
    }

    public void OnSkillLightningCutReadyStopSFX()
    {
        if (lightningCutReadyInstance.isValid())
        {
            lightningCutReadyInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            lightningCutReadyInstance.release();
            activeInstances.Remove(lightningCutReadyInstance);
        }
    }

    public void OnSkillLightningCutSlash() => Play("event:/Player/Player_Skill_Attack_Lightning_Cut_Slash");

    // ── 스킬 그래플링 ────────────────────────────────
    public void OnSkillGrapling() => Play("event:/Player/Interaction/Player_Skill_Grapling");

    // ── 방어 ────────────────────────────────
    public void OnShieldReady() => Play("event:/Player/Player_Shield_Ready");

    // ── 상호작용 ────────────────────────────────
    public void OnShieldParry() => Play("event:/Player/Interaction/Player_Shield_Parry");
    public void OnShieldParryCounter() => Play("event:/Player/Interaction/Player_Shield_Parry_Counter");
    public void OnShieldGuard() => Play("event:/Player/Interaction/Player_Shield_Guard");
    public void OnPlayerAttackHit() => Play("event:/Player/Interaction/Player_Attack_Hit");
    public void OnPlayerDamagedHurtVoice() => Play("event:/Player/Interaction/Player_Damaged_Hurt_Voice");
    public void OnFinalBossAttackHit() => Play("event:/FinalBoss/Interaction/Final_Boss_Attack_Hit");

    // ── 회복 ────────────────────────────────
    public void OnHeal() => Play("event:/Player/Player_Skill_Heal");

    // ── 피격 (전체 정지) ───────────────────
    public void OnPlayerHit()
    {
        if (chargingCompleteCoroutine != null)
        {
            StopCoroutine(chargingCompleteCoroutine);
            chargingCompleteCoroutine = null;
        }

        foreach (var instance in activeInstances)
        {
            if (instance.isValid())
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                instance.release();
            }
        }
        activeInstances.Clear();
    }

    private void OnDestroy()
    {
        foreach (var instance in activeInstances)
        {
            if (instance.isValid())
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                instance.release();
            }
        }
        activeInstances.Clear();
    }

    // ── 유휴 인스턴스 정리 ────────────────────
    public void PruneInactiveInstances()
    {
        activeInstances.RemoveAll(instance => !instance.isValid());
    }
}