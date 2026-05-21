using UnityEngine;

public class PlayerThrustReadyState : PlayerState
{
    public PlayerThrustReadyState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        // 3타 진입처럼 아예 (0,0)으로 못 박고 시작
        player.rb.gravityScale = 0f; // 2D gravityScale 사용
        player.SetVelocity(0f, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 🚨 [삭제] player.SetVelocity(0f, player.rb.linearVelocity.y);
        // 업데이트에서 y값을 건드리면 비탈길 쐐기(0,0)가 풀립니다. 삭제하세요.

        if (!player.inputReader.ThrustAttackHeld)
        {
            stateMachine.ChangeState(player.ThrustAttackState);
            return;
        }

        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용
            player.SetVelocity(0f, 0f); // 여기서만 물리적으로 꽉 잡습니다.
        }
        else
        {
            player.rb.gravityScale = 1f; // 2D gravityScale 사용
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용
    }
}