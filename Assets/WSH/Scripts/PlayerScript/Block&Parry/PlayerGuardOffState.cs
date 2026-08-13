using UnityEngine;

public class PlayerGuardOffState : PlayerState
{
    public PlayerGuardOffState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.animator.Play(player.anim_GuardOff);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 애니메이션 재생 시간이 90% 이상 지나면 Idle로 복귀
        // GetNormalizedTime() 함수가 있다고 가정하거나, animator 정보를 직접 체크
        var stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName(player.anim_GuardOff) && stateInfo.normalizedTime >= 0.9f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 1. 방향키를 누르고 있어도 이동하지 못하게 속도를 0으로 강제 고정
        player.SetVelocity(0f, 0f);

        // 2. 비탈길 위에 있다면 중력을 0으로 만들어서 유니티 물리 엔진이 밑으로 끌어내리지 못하게 차단
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f;
        }
    }
}