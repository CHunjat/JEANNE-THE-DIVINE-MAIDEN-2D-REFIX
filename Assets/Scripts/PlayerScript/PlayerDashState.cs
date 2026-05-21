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

        // 기본 대쉬 방향 (수평) - Vector2로 전환
        Vector2 dashVec = new Vector2(dashDirection, 0f);

        // 🔥 버그 수정 핵심 로직
        // 1. 지상에서 대쉬를 시작했고 2. 현재 비탈길 위라면 경사면 보정 적용
        // 공중에서 대쉬하다가 비탈길에 닿은 경우는 보정을 하지 않아야 위로 튀지 않습니다.
        if (startedGrounded && player.OnSlope())
        {
            dashVec = player.GetSlopeMoveDirection(dashVec);
        }

        // 공중 대쉬 상태에서 비탈길에 충돌하면 자연스럽게 멈추거나 슬라이딩하도록
        // 수직 속도(y)에 중력을 끄고 대쉬 속도만 주입
        player.SetVelocity(dashVec.x * player.dashSpeed, dashVec.y * player.dashSpeed);
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