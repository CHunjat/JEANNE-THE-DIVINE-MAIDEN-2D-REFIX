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

        // 🔥 [1. 순수 에어 대쉬 판독기]
        // 땅도 아니고, 비탈길도 아니면 이건 100% 공중 대쉬 (밑점프 관통 포함)
        bool isPureAirDash = !player.IsGrounded() && !player.lastGroundedWasSlope;
        Vector2 dashVec = new Vector2(dashDirection, 0f);
        float finalDashSpeed = player.dashSpeed;

        if (isPureAirDash)
        {
            // 바닥 계산 다 쌩까고 완벽하게 수평 직진 (밑에서 올라올 때 100% 관통 보장)
            player.SetVelocity(dashVec.x * finalDashSpeed, 0f);
            return; 
        }

        // =========================================================
        // 🔥 [2. 지상 대쉬 & 비탈길 대쉬 로직]
        // =========================================================
        
        bool detectedFlatGround = false;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(player.cd.bounds.center, player.cd.bounds.size * 0.9f, 0f, Vector2.down, 0.3f, player.GetCurrentGroundMask());
        
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider != player.ignoredDropCollider)
            {
                if (Vector2.Angle(Vector2.up, hit.normal) <= 0.1f)
                {
                    detectedFlatGround = true;
                    break;
                }
            }
        }

        // 🚨 [가장 중요한 핵심 마법] 
        // 대쉬 속도(20f) 때문에 틱이 씹혀서 바닥을 잃어버리는 현상 방지!
        // 평지가 안 잡히는 찰나에 밑으로 1.5f 짜리 초장거리 레이더를 쏴서 앞의 내리막을 낚아챕니다.
        if (!detectedFlatGround && !player.lastGroundedWasSlope && player.rb.linearVelocity.y <= 0.1f)
        {
            RaycastHit2D deepSlopeHit = Physics2D.Raycast(player.cd.bounds.center, Vector2.down, 1.5f, player.stairsLayer);
            if (deepSlopeHit.collider != null)
            {
                // 내 발 아래 깊숙한 곳에 비탈길이 감지되었다면 즉시 썰매 스위치 ON & 물리벽 생성!
                player.lastGroundedWasSlope = true;
                player.ToggleStairsCollision(true);
            }
        }

        // 비탈길 썰매 탑승 로직
        if (player.lastGroundedWasSlope)
        {
            // 기존 OnSlope()는 레이더가 짧아 틱이 씹히므로, 대쉬 전용 긴 레이더(1.5f)로 각도를 계산합니다.
            RaycastHit2D slopeHit = Physics2D.Raycast(player.cd.bounds.center, Vector2.down, 1.5f, player.stairsLayer);
            
            if (slopeHit.collider != null)
            {
                Vector2 slopeDir = Vector2.Perpendicular(slopeHit.normal).normalized;
                if (slopeDir.x * dashVec.x < 0) slopeDir = -slopeDir;

                bool isDownhill = slopeDir.y < -0.01f;

                if (detectedFlatGround && !isDownhill)
                {
                    dashVec.y = -0.1f;
                }
                else
                {
                    dashVec = slopeDir;
                    
                    if (isDownhill)
                    {
                        // [핵심] 내리막길에서 대쉬 속도 때문에 허공으로 날아가지 않도록,
                        // 벡터 밑으로 힘을 강하게 찍어누르면서 달립니다! (절대 붕 뜨지 않음)
                        dashVec.y -= 0.15f; 
                    }
                    else if (dashVec.y > 0.05f)
                    {
                        finalDashSpeed *= 0.9f;
                    }
                }
            }
            else
            {
                // 롱 레이더로도 비탈길을 잃어버렸다면 평지 판정으로
                if (detectedFlatGround) dashVec.y = -0.05f;
            }
        }
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