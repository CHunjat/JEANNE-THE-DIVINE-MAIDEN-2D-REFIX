using UnityEngine;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        // 1. 스프린트 점프 판별
        bool isFirstSprintJump = player.isSprinting && player.IsGrounded();

        if (isFirstSprintJump)
        {
            if (!player.CanSprintJump)
            {
                player.inputReader.JumpPressed = false;
                player.SetVelocity(player.rb.linearVelocity.x, 0f);
                player.isJumpCut = true;
                stateMachine.ChangeState(player.MoveState);
                return;
            }
            player.ResetSprintJumpCooldown();
        }

        stateTimer = 0f;
        player.wallGrabTimer = player.wallGrabCooldown;
        float finalJumpForce = player.jumpForce;
        float xInput = player.inputReader.MoveValue.x;

        // 2. 비탈길 점프 로직
        if (player.OnSlope())
        {
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0f);

            //  방향키를 떼고 있을 때(xInput == 0)는 비탈길 추가 부스트(3.5f) X
            // 이 조건문 하나로 중립 점프 시 하늘로 치솟는 폭주 버그가 완벽히 차단됩니다.
            if (Mathf.Abs(xInput) > 0.1f)
            {
                float moveDirX = Mathf.Sign(xInput);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(new Vector2(moveDirX, 0));

                if (slopeMoveDir.y > 0)
                {
                    if (isFirstSprintJump)
                    {
                        
                        finalJumpForce *= 1.1f; // (취향에 따라 0.7f ~ 0.9f 사이로 조절)
                    }
                    else
                    {
                        
                        finalJumpForce += 1.1f;
                    }
                }
            }
            player.transform.position += (Vector3)Vector2.up * 0.05f;
        }

        // 3. 점프 실행 및 애니메이션 처리
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

        // 관성 점프
        if (Mathf.Abs(xInput) > 0.1f)
        {
            // 방향키를 누르고 있다면: 현재 x속도(대시 관성 포함)를 그대로 유지하며 점프!
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x * 0.5f, finalJumpForce);
        }
        else
        {
            // 방향키를 떼고 있다면: 관성을 끊고 제자리 수직 점프로 전환!
            player.rb.linearVelocity = new Vector2(0f, finalJumpForce);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.HandleAttackInput();

        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return;

        // 4. 2단 점프 입력 체크
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
            if (player.rb.linearVelocity.y < 0.1f || player.OnSlope())
            {
                stateMachine.ChangeState(player.AirState);
                return;
            }
        }

        if (player.rb.linearVelocity.y < -0.1f)
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float xInput = player.inputReader.MoveValue.x;
        float currentXVelocity = player.rb.linearVelocity.x;

        // 1. 공중 방향 전환 
        if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentXVelocity))
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
        // 2. 방향키를 뗐을 때 
        else if (Mathf.Abs(xInput) < 0.1f)
        {
            float stoppingSpeed = Mathf.Lerp(currentXVelocity, 0f, Time.fixedDeltaTime * 10f);
            player.SetVelocity(stoppingSpeed, player.rb.linearVelocity.y);
        }
        // 3. 방향키를 계속 누르고 있을 때 
        else if (Mathf.Abs(currentXVelocity) > player.moveSpeed)
        {
            float targetX = xInput * player.moveSpeed;
            float lerpedX = Mathf.Lerp(currentXVelocity, targetX, Time.fixedDeltaTime * player.airDeceleration);
            player.SetVelocity(lerpedX, player.rb.linearVelocity.y);
        }
        // 4. 일반 공중 이동
        else
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }

        if (xInput != 0) player.FlipController(xInput);
    }
}