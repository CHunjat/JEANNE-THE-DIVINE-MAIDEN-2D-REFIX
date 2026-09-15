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

        bool isStuck = Physics2D.OverlapBox(player.transform.position, player.cd.bounds.size * 0.9f, 0f, player.groundLayer | player.stairsLayer) != null;
        if (player.isSprinting)
        {
            player.animator.Play(player.anim_SprintLand, 0, 0);
        }
        else
        {
            player.rb.linearVelocity = Vector2.zero;
            player.animator.CrossFade(animHash, 0.1f);
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

        if (!player.lastGroundedWasSlope && player.rb.linearVelocity.y > 0.05f)
        {
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0f);
        }

        if (player.isSprinting)
        {
            float dir = player.isFacingRight ? 1f : -1f;
            float currentSpeed = player.sprintSpeed;

            bool rideSlope = player.OnSlope() || player.lastGroundedWasSlope;
            if (!rideSlope)
            {
                Vector2 origin = new Vector2(player.cd.bounds.center.x, player.cd.bounds.min.y + 0.1f);
                RaycastHit2D stairUnder = Physics2D.BoxCast(
                    origin, player.groundCheckSize, 0f, Vector2.down, 0.45f, player.stairsLayer);
                if (stairUnder.collider != null && stairUnder.collider != player.ignoredDropCollider)
                {
                    player.slopeHit = stairUnder;
                    player.lastGroundedWasSlope = true;
                    rideSlope = true;
                }
            }

            if (rideSlope)
            {
                player.rb.gravityScale = 0f;
                Vector2 moveDir = new Vector2(dir, 0f);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);
                float rollSpeed = currentSpeed;
                if (slopeMoveDir.y > 0)
                    rollSpeed = 8.0f;
                else
                    rollSpeed = player.sprintSpeed;

                player.rb.linearVelocity = slopeMoveDir * rollSpeed;
                player.rb.AddForce(Vector2.down * 50f, ForceMode2D.Force);
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