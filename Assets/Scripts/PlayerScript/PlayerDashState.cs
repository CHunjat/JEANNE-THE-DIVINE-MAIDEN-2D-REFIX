using TheBlackCat.TrailEffect2D;
using UnityEngine;

public class PlayerDashState : PlayerState
{
    private float dashTime;
    private float dashDirection;
    public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        base.Enter();
        player.playerStats.isInvincible = true;
        //대쉬 잔상 패리대쉬시 사용
        if (player.playerModelForTrail != null)
        {
            TrailManager.Instance.StartTrail(player.playerModelForTrail);
        }


        // 대쉬 시작할 때 땅이었는지 기록 (공중 대쉬는 경사면 보정을 받지 않게 하기 위함)
        if (!player.IsGrounded() && !player.OnSlope())
        {
            player.hasUsedAirDash = true;
        }
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

        // 🔥 [핵심 수정: 순수 에어 대쉬 판독기]
        // 현재 땅에 있지도 않고(IsGrounded == false), 비탈길을 타는 대쉬(lastGroundedWasSlope)도 아니라면?
        // 이건 밑점프 대쉬거나 평지 점프 대쉬인 '100% 순수 에어 대쉬'입니다.
        bool isPureAirDash = !player.IsGrounded() && !player.lastGroundedWasSlope;

        Vector2 dashVec = new Vector2(dashDirection, 0f);
        float finalDashSpeed = player.dashSpeed;

        // 1. 순수 공중 대쉬일 경우 -> 바닥 밀착 로직 다 무시하고 완벽한 수평 직진!
        if (isPureAirDash)
        {
            player.SetVelocity(dashVec.x * finalDashSpeed, 0f);
            return; // 여기서 함수를 끝내서, 아래의 바닥 찍어누르기 로직이 절대 실행되지 못하게 막습니다!
        }

        // =========================================================================
        // 아래는 오직 [지상 대쉬] 거나 [비탈길 위 점프 대쉬(스르륵 타기)] 일 때만 실행됩니다.
        // =========================================================================

        // 발 밑을 넓게 스캔해서 '평지(각도 0)'가 있는지 찾습니다.
        bool detectedFlatGround = false;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(player.cd.bounds.center, player.cd.bounds.size * 0.9f, 0f, Vector2.down, 0.3f, player.GetCurrentGroundMask());

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider != player.ignoredDropCollider)
            {
                if (Vector2.Angle(Vector2.up, hit.normal) <= 0.1f) // 완전 평지 발견!
                {
                    detectedFlatGround = true;
                    break;
                }
            }
        }

        // 비탈길 로직 처리 (lastGroundedWasSlope 권한이 있을 때만 각도 꺾임)
        if (player.lastGroundedWasSlope && player.OnSlope(isDashing: true))
        {
            Vector2 slopeDir = player.GetSlopeMoveDirection(dashVec);
            bool isDownhill = slopeDir.y < -0.01f;

            if (detectedFlatGround && !isDownhill)
            {
                dashVec.y = -0.1f;
                finalDashSpeed = player.dashSpeed;
            }
            else
            {
                dashVec = slopeDir;
                if (dashVec.y > 0.05f)
                {
                    finalDashSpeed *= 0.9f;
                }
                else
                {
                    finalDashSpeed = player.dashSpeed;
                }
            }
        }
        // 지상 대쉬일 때 평지 바닥에 밀착시키는 힘
        else if (detectedFlatGround)
        {
            dashVec.y = -0.05f;
        }

        // 최종 속도 적용
        player.SetVelocity(dashVec.x * finalDashSpeed, dashVec.y * finalDashSpeed);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        player.HandleAttackInput();
        dashTime -= Time.deltaTime;

        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return; // 그래플로 넘어갔다면 아래 로직 스킵

        player.HandleGuardInput();
        if (stateMachine.CurrentState == player.GuardState) return;



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

    
    private void FinishDash()
    {
        player.ResetDashCooldown();

        player.StartCoroutine(DashGraceCoroutine());
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

    private System.Collections.IEnumerator DashGraceCoroutine()
    {
        player.isDashGracePeriod = true;
        yield return new WaitForSeconds(0.1f);
        player.isDashGracePeriod = false;
    }

    public override void Exit()
    {
        base.Exit();
        player.playerStats.isInvincible = false;

        if (player.playerModelForTrail != null)
        {
            TrailManager.Instance.StopTrail(player.playerModelForTrail);
        }


        player.rb.gravityScale = 1f; // 2D gravityScale 원복
    }
}