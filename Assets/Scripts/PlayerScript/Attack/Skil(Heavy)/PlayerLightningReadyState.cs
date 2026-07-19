using UnityEngine;

public class PlayerLightningReadyState : PlayerState
{
    private float waitTime = 0.15f; // 유저가 꾹 누르는지 판단하는 시간

    public PlayerLightningReadyState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 비탈길 고정 로직 유지
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f;
            player.SetVelocity(0f, 0f);
        }
        else
        {
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 키를 떼는 순간: 무조건 즉발 공격 실행! (Idle로 가지 말고 Attack으로 가세요)
        if (!player.inputReader.HeavyAttackHeld)
        {
            stateMachine.ChangeState(player.LightningAttackState);
            return;
        }

        // 2. 키를 누르고 있는데 0.15초가 지나면: 기 모으기 상태로 진입
        if (stateTimer >= waitTime)
        {
            stateMachine.ChangeState(player.LightningChargeState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f;
    }
}