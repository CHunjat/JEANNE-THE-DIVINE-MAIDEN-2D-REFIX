using UnityEngine;

public class PlayerGuardState : PlayerState
{
    public PlayerGuardState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        // 방어 시작 시 미끄러짐 방지 및 정지
        player.rb.linearVelocity = Vector3.zero;
        player.animator.Play(player.anim_GuardNormal);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // [중요] 방어 키를 떼면 바로 해제 상태로 전이
        if (!player.inputReader.GuardHeld)
        {
            stateMachine.ChangeState(player.GuardOffState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        // 방어 중에는 이동하지 못하도록 속도 고정 (필요 시)
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }
}