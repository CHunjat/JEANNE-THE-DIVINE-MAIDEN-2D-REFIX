using TheBlackCat.TrailEffect2D;
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

        ////대쉬 잔상 패리대쉬시 사용
        //if (player.playerModelForTrail != null)
        //{
        //    TrailManager.Instance.StartTrail(player.playerModelForTrail);
        //}


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

        Vector2 dashVec = new Vector2(dashDirection, 0f);
        float finalDashSpeed = player.dashSpeed;

        // 1. [핵심 1] 발 밑을 넓게 스캔해서 '평지(각도 0)'가 있는지 독자적으로 찾습니다.
        bool detectedFlatGround = false;

        // 캐릭터 사이즈만큼 넓게 레이를 쏴서 윗평지가 발밑에 들어왔는지 확인
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

        // 2. 비탈길 로직 처리
        if (player.OnSlope())
        {
            // ★ [핵심 2] 비탈길과 평지가 '동시 감지'되었다면? (윗평지 교차점)
            if (detectedFlatGround)
            {
                // 비탈길의 오르막 각도를 즉시 무시하고 평지 대시 모드로 전환합니다.
                // 비탈길이 더 높더라도 윗평지에 찰싹 달라붙도록 미세한 하강 벡터(-0.1f)를 주입!
                dashVec.y = -0.1f;
                finalDashSpeed = player.dashSpeed; // 평지 진입이므로 속도 100% 쾌속 유지
            }
            else
            {
                // 순수 비탈길일 때 (동시 감지 아님)
                dashVec = player.GetSlopeMoveDirection(dashVec);

                if (dashVec.y > 0.05f)
                {
                    // 오르막 댐핑 (너무 느리다 싶으면 0.6f ~ 1.0f 사이로 올리세요)
                    finalDashSpeed *= 0.6f;
                }
                else
                {
                    // 내리막 쾌속 슬라이딩
                    finalDashSpeed = player.dashSpeed;
                }
            }
        }
        else if (detectedFlatGround)
        {
            // 순수 평지 대시일 때도 바닥에 미세하게 눌러줘서 턱에서 안 뜨게 만듦
            dashVec.y = -0.05f;
        }

        // 3. 최종 속도 적용
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

        //패리대쉬일때 잔상 밑에 추가
        //if (player.playerModelForTrail != null)
        //{
        //    TrailManager.Instance.StopTrail(player.playerModelForTrail);
        //}


        player.rb.gravityScale = 1f; // 2D gravityScale 원복
    }
}