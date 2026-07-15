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

         //=====================================================================
         //🔥 [추가된 핵심 방어막] 에어 대쉬 중 모서리에 박히는 현상(Clipping) 방지
         //물리 엔진이 멘붕해서 바닥을 통과시키기 전에, 강제로 바닥 위로 끄집어 올립니다!
         //=====================================================================
        //if (player.IsGrounded())
        //{
        //    //발밑을 향해 레이를 쏴서 현재 파고든 바닥의 높이를 찾습니다.
        //   RaycastHit2D snapHit = Physics2D.BoxCast(player.cd.bounds.center, player.cd.bounds.size * 0.9f, 0f, Vector2.down, 0.5f, player.GetCurrentGroundMask());

        //    if (snapHit.collider != null && snapHit.collider != player.ignoredDropCollider)
        //    {
        //        float surfaceY = snapHit.point.y;
        //        float footY = player.cd.bounds.min.y;

        //        // 캐릭터의 발(footY)이 실제 바닥 표면(surfaceY)보다 아래에 있다면? = 파고들었다!
        //        if (footY < surfaceY && (surfaceY - footY) < 0.5f)
        //        {
        //            //파고든 만큼(surfaceY -footY) 더하기 약간의 여유(0.05f)를 줘서 위로 텔포시킵니다.
        //            player.transform.position += new Vector3(0f, (surfaceY - footY) + 0.05f, 0f);
        //        }
        //    }
        //}


        Vector2 dashVec = new Vector2(dashDirection, 0f);
        float finalDashSpeed = player.dashSpeed;

        // 1. 발 밑을 넓게 스캔해서 '평지(각도 0)'가 있는지 독자적으로 찾습니다.
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

        // 2. 비탈길 로직 처리 (기존 코드 완벽하게 유지)
        if (player.OnSlope(isDashing: true))
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
        else if (detectedFlatGround)
        {
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