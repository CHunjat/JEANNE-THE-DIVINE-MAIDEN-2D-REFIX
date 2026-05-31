using UnityEngine;

public class PlayerAirState : PlayerState
{
    public PlayerAirState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        player.ToggleStairsCollision(true);
        // base.Enter(); 절대 사용 금지!
        stateTimer = 0;

        if (player.isSprinting)
        {
            player.animator.Play(player.anim_SprintJump);
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.HandleAttackInput();

        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return;
        bool isGrounded = player.IsGrounded();

        if (isGrounded && player.ignoredDropCollider != null)
        {
            isGrounded = false;
        }



        // [착지 판정 및 모서리 텔레포트]
        if (player.IsGrounded())
        {
            // 1. 땅에 닿았으니, 물리 연산을 잠시 정지 (고스트 상태)
            player.rb.simulated = false;

            // 2. 이제 안전하게 지형 위로 정렬 (텔레포트)
            RaycastHit2D hit = Physics2D.BoxCast(player.cd.bounds.center, player.groundCheckSize * 1.2f, 0f, Vector2.down, 0.5f, player.GetGroundCheckMask());
            if (hit.collider != null && hit.collider != player.ignoredDropCollider)
            {
                float surfaceY = hit.point.y;
                float footY = player.cd.bounds.min.y;

                //점프 애매할때 윗땅에 올라타지는판정
                if (footY < surfaceY && (surfaceY - footY) < 0.7f)
                {
                    player.transform.position += new Vector3(0f, (surfaceY - footY) + 0.02f, 0f);
                }
            }

            // 3. 다시 물리 연산 켜기 (이제 캐릭터는 정렬된 위치에서 정상적으로 존재함)
            player.rb.simulated = true;

            // 4. 착지 상태로 전환
            if (player.OnSlope())
            {
                player.rb.gravityScale = 0f;
                player.SetVelocity(0f, 0f);
            }
            stateMachine.ChangeState(player.LandState);
            return;
        }

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
        else
        {
            player.inputReader.DashPressed = false;
        }

        #region 벽타기 진입스위치 및 코드 
        float xInput = player.inputReader.MoveValue.x;

        if (Mathf.Abs(xInput) > 0.1f)
        {
            float inputDir = Mathf.Sign(xInput);

            if (player.wallGrabTimer <= 0f && player.IsTouchingWall(inputDir))
            {
                stateMachine.ChangeState(player.WallSlideState);
                return;
            }
        }
        #endregion
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float xInput = player.inputReader.MoveValue.x;
        float currentX = player.rb.linearVelocity.x;

        if (player.OnSlope() && player.rb.linearVelocity.y < 0.5f)
        {
            if (xInput != 0)
            {
                Vector2 moveVec = new Vector2(xInput, 0f);
                Vector2 slopeVec = player.GetSlopeMoveDirection(moveVec);

                if (slopeVec.y < 0)
                {
                    player.rb.AddForce(Vector2.down * 10f, ForceMode2D.Force);
                }
            }
        }

        if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentX))
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
        else if (Mathf.Abs(currentX) > player.moveSpeed)
        {
            float targetX = xInput * player.moveSpeed;
            float lerpedX = Mathf.Lerp(currentX, targetX, Time.deltaTime * player.airDeceleration);
            player.SetVelocity(lerpedX, player.rb.linearVelocity.y);
        }
        else
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }

        if (xInput != 0) player.FlipController(xInput);
    }
}