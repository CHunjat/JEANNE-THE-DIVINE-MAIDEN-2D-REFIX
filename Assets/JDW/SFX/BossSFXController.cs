using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class BossSFXController : MonoBehaviour
{
    

    private List<EventInstance> activeInstances = new();

    // ── 내부 재생 ────────────────────────────
    private void Play(string path)
    {
        var instance = RuntimeManager.CreateInstance(path);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        instance.start();
        instance.release();
        activeInstances.Add(instance);
    }

    // ── 이동 ────────────────────────────────
    public void OnBossFootstep() => Play("event:/FinalBoss/Boss_Movement_Foot_Step_Edit");

    // ── 공격 ────────────────────────────────
    public void OnBossAttackHit() => Play("event:/FinalBoss/Interaction/Final_Boss_Attack_Hit");
    public void OnBossAttackClearing() => Play("event:/FinalBoss/Boss_Attack_Clearing");
    public void OnBossAttackHardSlash() => Play("event:/FinalBoss/Boss_Attack_Hard_Slash");
    public void OnBossAttackJump() =>
    RuntimeManager.PlayOneShot("event:/FinalBoss/Boss_Attack_Jump", transform.position);
    public void OnBossAttackJump_JohnCena() => Play("event:/FinalBoss/BGM/Boss_Attack_Jump_JohnCena");
    public void OnBossAttackLanding() => Play("event:/FinalBoss/Boss_Attack_Landing");
    public void OnBossAttackPierce() => Play("event:/FinalBoss/Boss_Attack_Normal_Pierce");
    public void OnBossAttackSlash() => Play("event:/FinalBoss/Boss_Attack_Normal_Slash");
    public void OnBossAttackSpit() => Play("event:/FinalBoss/Boss_Attack_Spit");
    public void OnBossAttackStomp() => Play("event:/FinalBoss/Boss_Attack_Stomp");

    // ── 사망 ────────────────────────────────
    public void OnBossDeath() => Play("event:/FinalBoss/Boss_Death");

    
    public void StopAllBossSFX()
    {
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

    // ── 그로기 (전체 정지) ───────────────────
    public void OnBossGroggy() => StopAllBossSFX();
}