using UnityEngine;

public class PlayerLandState : PlayerState
{
    public PlayerLandState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        player.ToggleStairsCollision(true);
        stateTimer = 0f;
        player.ResetLandTimer();

        // [구르기 판정] 안전하게 검사
        bool isStuck = Physics2D.OverlapBox(player.transform.position, player.cd.bounds.size * 0.9f, 0f, player.groundLayer | player.stairsLayer) != null;

        if (player.isSprinting && !isStuck)
        {
            float currentXVel = player.rb.linearVelocity.x;
            if (Mathf.Abs(currentXVel) < 0.1f)
            {
                currentXVel = (player.isFacingRight ? 1f : -1f) * player.sprintSpeed;
            }
            else if (Mathf.Abs(currentXVel) > player.sprintSpeed)
            {
                currentXVel = Mathf.Sign(currentXVel) * player.sprintSpeed;
            }

            player.rb.linearVelocity = new Vector2(currentXVel, 0f);
            player.animator.Play(player.anim_SprintLand, 0, 0);
        }
        else
        {
            player.rb.linearVelocity = Vector2.zero;
            if (player.isSprinting) player.animator.Play(player.anim_SprintLand, 0, 0);
            else player.animator.CrossFade(animHash, 0.1f);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (player.isSprinting) { if (stateTimer < 0.4f) return; }
        else { if (stateTimer < 0.1f) return; }

        if (player.inputReader.MoveValue.x != 0) { stateMachine.ChangeState(player.MoveState); return; }
        if (stateTimer > 0.5f) { stateMachine.ChangeState(player.IdleState); }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        if (player.isSprinting)
        {
            float dir = player.isFacingRight ? 1f : -1f;
            float currentSpeed = player.sprintSpeed;

            if (player.OnSlope())
            {
                player.rb.gravityScale = 0f;
                Vector2 moveDir = new Vector2(dir, 0f);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);
                player.SetVelocity(slopeMoveDir.x * currentSpeed, (slopeMoveDir.y * currentSpeed) - 4f);
            }
            else
            {
                player.rb.gravityScale = 1f;
                player.SetVelocity(dir * currentSpeed, player.rb.linearVelocity.y);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f;
    }
}