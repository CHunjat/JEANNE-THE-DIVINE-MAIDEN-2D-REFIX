using UnityEngine;
public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }
    public override void Enter()
    {
        bool isFirstSprintJump = player.isSprinting && player.IsGrounded();
        if (isFirstSprintJump)
        {
            if (!player.CanSprintJump)
            {
                if (player.CanJump)
                {
                    isFirstSprintJump = false;
                }
                else
                {
                    player.inputReader.JumpPressed = false;
                    player.SetVelocity(player.rb.linearVelocity.x, 0f);
                    player.isJumpCut = true;
                    stateMachine.ChangeState(player.MoveState);
                    return;
                }
            }
            else
            {
                player.ResetSprintJumpCooldown();
            }
        }
        stateTimer = 0f;
        player.wallGrabTimer = player.wallGrabCooldown;
        float finalJumpForce = player.jumpForce;
        float xInput = player.inputReader.MoveValue.x;
        if (player.OnSlope())
        {
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0f);
            if (Mathf.Abs(xInput) > 0.1f)
            {
                float moveDirX = Mathf.Sign(xInput);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(new Vector2(moveDirX, 0));
                if (slopeMoveDir.y > 0)
                {
                    if (isFirstSprintJump)
                        finalJumpForce *= 1.1f;
                    else
                        finalJumpForce += 1.1f;
                }
            }
            player.transform.position += (Vector3)Vector2.up * 0.05f;
        }
        player.UseJump();
        if (isFirstSprintJump)
        {
            player.animator.Play(player.anim_SprintJump);
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f);
            player.isSprinting = false;
        }
        if (Mathf.Abs(xInput) > 0.1f)
        {
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x * 0.5f, finalJumpForce);
        }
        else
        {
            player.rb.linearVelocity = new Vector2(0f, finalJumpForce);
            Debug.Log($"<color=orange>[Jump Enter]</color> y={player.rb.linearVelocity.y:F2} slope={player.OnSlope()} lastSlope={player.lastGroundedWasSlope}");
        }
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.HandleAttackInput();
        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return;
        if (player.inputReader.JumpPressed && player.CanJump)
        {
            player.inputReader.JumpPressed = false;
            stateMachine.ChangeState(player.JumpState);
            return;
        }
        if (player.inputReader.DashPressed && player.CanDash)
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }
        if (stateTimer > 0.05f && player.IsGrounded())
        {
            if (player.rb.linearVelocity.y < 0.1f)
            {
                var jumpClips = player.animator.GetCurrentAnimatorClipInfo(0);
                string jumpClip = jumpClips.Length > 0 && jumpClips[0].clip != null
                    ? jumpClips[0].clip.name : "none";
                Debug.Log($"<color=orange>[Jump grounded]</color> t={stateTimer:F2} y={player.rb.linearVelocity.y:F2} clip={jumpClip} nTime={player.animator.GetCurrentAnimatorStateInfo(0).normalizedTime:F2}");
                stateMachine.ChangeState(player.AirState);
                return;
            }
        }
        if (player.rb.linearVelocity.y < -0.1f)
        {
            Debug.Log($"<color=orange>[Jump→Air] falling]</color> y={player.rb.linearVelocity.y:F2}");
            stateMachine.ChangeState(player.AirState);
        }
    }
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        float xInput = player.inputReader.MoveValue.x;
        float currentXVelocity = player.rb.linearVelocity.x;
        if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentXVelocity))
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
        else if (Mathf.Abs(xInput) < 0.1f)
        {
            float stoppingSpeed = Mathf.Lerp(currentXVelocity, 0f, Time.fixedDeltaTime * 10f);
            player.SetVelocity(stoppingSpeed, player.rb.linearVelocity.y);
        }
        else if (Mathf.Abs(currentXVelocity) > player.moveSpeed)
        {
            float targetX = xInput * player.moveSpeed;
            float lerpedX = Mathf.Lerp(currentXVelocity, targetX, Time.fixedDeltaTime * player.airDeceleration);
            player.SetVelocity(lerpedX, player.rb.linearVelocity.y);
        }
        else
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
        if (xInput != 0) player.FlipController(xInput);
    }
}