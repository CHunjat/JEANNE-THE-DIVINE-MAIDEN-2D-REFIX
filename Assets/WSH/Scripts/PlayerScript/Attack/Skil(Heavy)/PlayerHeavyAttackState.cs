using UnityEngine;

public class PlayerHeavyAttackState : PlayerAttackState
{
    public PlayerHeavyAttackState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 강공격은 단타기이므로 일반 콤보 입력을 차단합니다.
        comboInputRegistered = false;

        // [타격감 팁] 

        /* float facingDir = player.isFacingRight ? 1f : -1f;
        player.SetVelocity(facingDir * player.moveSpeed * 1.5f, player.rb.linearVelocity.y);
        */
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();


        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용 (중력 끄기)
            player.SetVelocity(0f, 0f);   // 속도 완전 고정 (이동 공격이 아닐 경우)
        }
        else
        {
            // 평지라면 기존 중력/마찰력 로직 유지
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }


        // 제자리에서 안정적으로 때리도록 X축 속도를 0으로 꽉 잡아줍니다.
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 공격 도중 바닥이 꺼지거나 낭떠러지로 밀려나면 공중 상태로 전환
        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
            return;
        }

        // ※ 부모 클래스인 PlayerAttackState.cs의 LogicUpdate에 
        // if (GetNormalizedTime() >= 1.0f) stateMachine.ChangeState(player.IdleState); 
        // 이 코드가 이미 들어있기 때문에, 애니메이션이 끝나면 알아서 깔끔하게 Idle로 전환
    }
    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용 (상태 나갈 때 중력 원복 필수!)
        player.currentChargeLevel = 1;
    }
}