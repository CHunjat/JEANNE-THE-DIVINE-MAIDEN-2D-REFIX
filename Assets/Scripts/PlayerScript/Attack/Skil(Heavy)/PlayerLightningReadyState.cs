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

        // ★ [핵심 수정] LightningHeld 대신 통합 유지 변수인 HeavyAttackHeld를 검사합니다!
        if (!player.inputReader.HeavyAttackHeld)
        {
            stateMachine.ChangeState(player.LightningChargeState);
            return;
        }

        // 0.15초 이상 꾹 누르고 있으면 본격적인 차지 상태로 진입
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