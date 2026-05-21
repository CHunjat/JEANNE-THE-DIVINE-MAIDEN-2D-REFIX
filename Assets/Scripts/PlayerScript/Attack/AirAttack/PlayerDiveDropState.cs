using UnityEngine;

public class PlayerDiveDropState : PlayerState
{
    public PlayerDiveDropState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 1. 중력 무시 (우리가 직접 엄청난 속도로 끌어내릴 것이기 때문)
        player.rb.gravityScale = 0f; // 2D gravityScale 사용

        // 2. X축(앞뒤) 이동을 멈추고, Y축(아래)으로 최대 속도로 꽂아버림!
        // X축을 0으로 하면 수직으로 떨어지고, 관성을 살리고 싶다면 player.rb.linearVelocity.x 를 넣으세요.
        player.SetVelocity(0f, -player.diveDropSpeed);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 바닥에 닿는 순간, 착지 타격 모션으로 
        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.DiveLandState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 제자리에서 안정적으로 때리도록 X축 속도를 0으로 꽉 잡아줍니다.
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (중력 복구)
    }
}