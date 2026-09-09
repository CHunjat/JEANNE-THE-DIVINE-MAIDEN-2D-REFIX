using UnityEngine;

public class PlayerParryLightCounterState : PlayerState
{
    public PlayerParryLightCounterState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.rb.linearVelocity = Vector2.zero;


        // 애니메이션 강제 덮어쓰기 (0프레임부터 시작)
        player.animator.Play(animHash, 0, 0f);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 애니메이션이 끝나면 자동으로 Idle 상태로 복귀
        float normalizedTime = player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (normalizedTime >= 1.0f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        player.SetVelocity(0f, player.rb.linearVelocity.y); // 관성 고정
    }

    public override void Exit()
    {
        base.Exit();
        // 스테이트를 빠져나갈 때 무적 해제
    }
}