using UnityEditor;
using UnityEngine;

public abstract class PlayerAttackState : PlayerState
{
    protected bool comboInputRegistered;

    public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }


    public override void Enter()
    {
        base.Enter();
        player.inputReader.AttackPressed = false;
        comboInputRegistered = false; // 진입 시 예약 초기화
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 애니메이션 재생이 100%(1.0f) 끝났다면?
        if (GetNormalizedTime() >= 1.0f)
        {
            // 기본 대기 상태로 돌려보냄
            stateMachine.ChangeState(player.IdleState);
        }

        if (stateTimer > 0.1f && GetNormalizedTime() >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }

        if (!comboInputRegistered && stateTimer > 0.1f && GetNormalizedTime() >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 🔥 비탈길에서 공격 시 미끄러짐 완벽 방지
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용 (중력 끄기)
            player.SetVelocity(0f, 0f);   // 속도 완전 고정 (앞으로 전진하는 공격이 아니라면)
        }
        else
        {
            // 평지라면 기존 로직 유지
            player.rb.gravityScale = 1f; // 2D gravityScale 사용
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (🚪 나갈 때 중력 원복 필수!)
    }

    // 현재 애니메이션이 얼마나 재생되었는지(0.0 ~ 1.0) 퍼센트로 가져오는 함수
    public new float GetNormalizedTime() //헤비에서 사용하게 public 으로 변경
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash == animHash)
        {
            return stateInfo.normalizedTime;
        }
        return 0;
    }
}