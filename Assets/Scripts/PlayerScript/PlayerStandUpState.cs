using UnityEngine;

public class PlayerStandUpState : PlayerState
{
    public PlayerStandUpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 일어나는 모션(Standing)이 끝나면 자연스럽게 Idle로 복귀
        if (GetNormalizedTime() >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 혹시 비탈길이었을 경우를 대비해 중력 복구
    }
}