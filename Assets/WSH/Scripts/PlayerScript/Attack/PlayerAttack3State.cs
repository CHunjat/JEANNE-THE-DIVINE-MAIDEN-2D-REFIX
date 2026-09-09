using UnityEngine;

public class PlayerAttack3State : PlayerAttackState
{
    private float stepSpeed = 4f;

    public PlayerAttack3State(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        // 막타의 묵직함을 위해 제자리 고정
        player.SetVelocity(0f, player.rb.linearVelocity.y);
        comboInputRegistered = false;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        float facingDir = player.isFacingRight ? 1f : -1f;

        // [미끄러짐 해결] 공격 중에는 확실하게 중력을 끄고 켭니다
        if (player.OnSlope()) player.rb.gravityScale = 0f;
        else player.rb.gravityScale = 1f;

        if (stateTimer < 0.15f)
        {
            if (player.OnSlope())
            {
                // 비탈길 전용 이동
                Vector2 moveVec = new Vector2(facingDir, 0f);
                Vector2 slopeVec = player.GetSlopeMoveDirection(moveVec);
                float downwardStickiness = slopeVec.y < 0 ? 2.0f : 1.0f;
                float targetSpeedX = facingDir * Mathf.Abs(slopeVec.x) * stepSpeed;
                float targetSpeedY = (slopeVec.y * stepSpeed * downwardStickiness) - (slopeVec.y < 0 ? 2f : 0f);
                player.SetVelocity(targetSpeedX, targetSpeedY);
            }
            else
            {
                // [위로 튀는 현상 해결] 평지일 때는 무조건 Y축을 0으로 강제!
                player.SetVelocity(facingDir * stepSpeed, 0f);
            }
        }
        else
        {
            // 전진 종료 후 완벽 정지
            if (player.OnSlope()) player.SetVelocity(0f, 0f);
            else player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 중력 복구
    }
}