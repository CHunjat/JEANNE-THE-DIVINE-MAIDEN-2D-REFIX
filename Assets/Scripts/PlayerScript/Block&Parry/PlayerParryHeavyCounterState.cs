using UnityEngine;

public class PlayerParryHeavyCounterState : PlayerState
{
    public PlayerParryHeavyCounterState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.rb.linearVelocity = Vector2.zero;

        // 카운터 중 무적!

        // 애니메이션 0프레임부터 재생
        player.animator.Play(animHash, 0, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        float normalizedTime = player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (normalizedTime >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        player.playerStats.isInvincible = false;
    }
}