using UnityEngine;

public class PlayerRestState : PlayerState
{
    private bool isRestingLoop;

    public PlayerRestState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        isRestingLoop = false;
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. 처음 앉는 모션(ToRest)이 끝났으면
        if (!isRestingLoop && GetNormalizedTime() >= 1.0f)
        {
            isRestingLoop = true;
            // 2. 대기 모션(Resting)으로 강제 전환하여 무한 반복 (루프 애니메이션 설정 필요)
            player.animator.Play(player.anim_Resting);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f;
            player.SetVelocity(0f, 0f);
        }
    }
}