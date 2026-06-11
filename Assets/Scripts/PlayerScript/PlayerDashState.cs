using UnityEngine;

public class PlayerDashState : PlayerState
{
    private float dashTime;
    private float dashDirection;
    private bool startedGrounded; // 🔥 대쉬 시작 시점의 접지 상태 저장

    public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();

        // 🔥 대쉬 시작할 때 땅이었는지 기록 (공중 대쉬는 경사면 보정을 받지 않게 하기 위함)
        startedGrounded = player.IsGrounded() || player.OnSlope();

        player.rb.gravityScale = 0f; // 2D gravityScale 사용
        dashTime = player.dashDuration;

        float xInput = player.inputReader.MoveValue.x;
        dashDirection = xInput != 0 ? Mathf.Sign(xInput) : (player.isFacingRight ? 1f : -1f);

        if (player.IsTouchingWall(dashDirection))
        {
            stateMachine.ChangeState(player.IdleState); // 즉시 종료
            return;
        }

        player.FlipController(dashDirection);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 1. 대시 방향 설정
        Vector2 dashVec = new Vector2(dashDirection, 0f);
        float finalDashSpeed = player.dashSpeed;

        // 2. 비탈길 판정 시 (중요: 내려가는 중이든 올라가는 중이든 비탈길이면 꺾어야 함)
        if (player.OnSlope())
        {
            // 3. 경사면의 기울기(Tangent)를 가져와서 대시 벡터를 그 각도로 바꿉니다.
            // 이렇게 하면 대시 벡터가 수평이 아니라 '비탈길 각도'가 되어, 바닥을 타고 내려갑니다.
            dashVec = player.GetSlopeMoveDirection(dashVec);

            // 내리막(y < 0)이라면 감속하지 않고 가속도를 실어줘야 '스으윽-' 미끄러집니다.
            // 만약 너무 빠르면 여기만 살짝 조정하세요.

            finalDashSpeed = player.dashSpeed * (dashVec.y < 0 ? 1.0f : 0.6f);
        }

        // 4. 적용
        player.SetVelocity(dashVec.x * finalDashSpeed, dashVec.y * finalDashSpeed);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return; // 그래플로 넘어갔다면 아래 로직 스킵

        player.HandleGuardInput();
        if (stateMachine.CurrentState == player.GuardState) return;


        player.HandleAttackInput();
        dashTime -= Time.deltaTime;

        // 벽에 닿았을 때 예외 처리
        if (player.IsTouchingWall(dashDirection))
        {
            FinishDash();
            return;
        }

        if (player.inputReader.JumpPressed && player.IsGrounded())
        {
            player.inputReader.JumpPressed = false;
            player.ResetDashCooldown();
            player.SetDashJumpVelocity(player.isFacingRight ? 1f : -1f);
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (dashTime <= 0)
        {
            FinishDash();
        }
    }

    // 🔥 대쉬 종료 로직 공통화
    private void FinishDash()
    {
        player.ResetDashCooldown();
        if (player.OnSlope())
        {
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, 0f);
        }
        if (player.IsGrounded() || player.OnSlope())
        {
            if (Mathf.Abs(player.inputReader.MoveValue.x) > 0.1f)
            {
                player.isSprinting = true;
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                player.isSprinting = false;
                stateMachine.ChangeState(player.IdleState);
            }
        }
        else
        {
            player.isSprinting = false;
            // 공중 종료 시 x축 관성 제거 (원하면 유지 가능)
            player.SetVelocity(0f, player.rb.linearVelocity.y);
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 원복
    }
}