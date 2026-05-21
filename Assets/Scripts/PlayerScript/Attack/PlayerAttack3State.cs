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

        // 비탈길 처리: 중력 끄기
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f;
        }

        // [핵심] 초반 0.15초 동안 전진
        if (stateTimer < 0.15f)
        {
            Vector2 moveVec = new Vector2(facingDir, 0f);
            Vector2 slopeVec = player.GetSlopeMoveDirection(moveVec);

            float downwardStickiness = slopeVec.y < 0 ? 2.0f : 1.0f;

            // ⚡ 해결책: slopeVec.x 부호를 믿지 않고, facingDir(현재 보는 방향)과 
            // Mathf.Abs(절대값)를 사용하여 무조건 정방향으로만 나아가게 강제함
            float targetSpeedX = facingDir * Mathf.Abs(slopeVec.x) * stepSpeed;
            float targetSpeedY = (slopeVec.y * stepSpeed * downwardStickiness) - (slopeVec.y < 0 ? 2f : 0f);

            player.SetVelocity(targetSpeedX, targetSpeedY);
        }
        else
        {
            // 후딜레이시 정지
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 중력 복구
    }
}