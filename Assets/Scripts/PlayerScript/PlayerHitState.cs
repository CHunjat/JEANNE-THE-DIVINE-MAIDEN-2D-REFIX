using UnityEngine;

public class PlayerHitState : PlayerState
{
    private float hitStunDuration = 0.4f; 
    private float knockbackForce = 6f;    // 뒤로 밀려나는 힘

    public PlayerHitState(PlayerController player, PlayerStateMachine stateMachine, string animBoolName) 
        : base(player, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        stateTimer = 0f;

        // 플레이어가 바라보는 반대 방향 계산
        int facingDir = player.isFacingRight ? 1 : -1;

        // 비탈길 방어 코드 적용!
        if (player.OnSlope())
        {
            // 비탈길: 땅에 파묻히지 않도록 Y축으로 아주 살짝(1f) 
            player.SetVelocity(-facingDir * knockbackForce, 1f); 
        }
        else
        {
            // 평지: 깔끔하게 수평으로만 밀려나도록 Y축 속도 0으로 고정
            player.SetVelocity(-facingDir * knockbackForce, 0f); 
        }

       
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 0.4초의 경직 시간이 지나면 복귀
        if (stateTimer >= hitStunDuration)
        {
            if (player.IsGrounded())
                player.StateMachine.ChangeState(player.IdleState);
            else
                player.StateMachine.ChangeState(player.AirState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        
        player.SetVelocity(0f, player.rb.linearVelocity.y);
    }
}