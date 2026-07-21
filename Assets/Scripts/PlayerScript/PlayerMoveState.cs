using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        // ★ 내 발밑이 계단이 아닐 때(평지일 때)만 계단을 통과하도록 물리 충돌 OFF
        if (!player.IsOnStairs())
        {
            Physics2D.SyncTransforms();
        }

        stateTimer = 0f;
        player.wasSprinting = false;

        if (player.isSprinting)
        {
            if (!player.isJumpCut)
            {
                player.animator.Play(player.anim_SprintStart);
            }
            player.isJumpCut = false;
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f);
        }
    }

    public override void LogicUpdate()
    {

        if (player.StateMachine.CurrentState == player.HealState) return;
        player.HandleGuardInput();
        if (stateMachine.CurrentState == player.GuardState) return;

        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return;

        // =========================================================
        // 🔥 [개발자님 특제 로직] 거리에 따른 유예시간(GraceTime) 조절
        // =========================================================
        if (player.IsGrounded())
        {
            // 땅에 닿아있으면 든든하게 유예시간 충전 (0.8초)
            player.groundedTimer = player.groundedGraceTime;
        }
        else
        {
            // 1. 땅에서 벗어났지만 발밑 0.6f 이내에 바닥이 감지된다! (비탈길 스무스 진행 중)
            if (player.IsNearGround())
            {
                // 정상적으로 서서히 타이머를 깎음 (비탈길 물리 유지)
                player.groundedTimer -= Time.deltaTime;
            }
            // 2. 발밑에 아무것도 안 걸린다! (진짜 깊은 낭떠러지)
            else
            {
                // 유예시간 그딴 거 없이 즉시 0으로 압수! (칼같이 추락하게 만듦)
                player.groundedTimer = 0f;
            }
        }

        // groundedTimer가 0보다 크면 아직 땅에 있는 것과 다름없음 (상태 전환 안 함)
        if (player.groundedTimer <= 0)
        {
            if (!player.IsGrounded())
            {
                stateMachine.ChangeState(player.AirState);
                return;
            }
        }
        // =========================================================

        float xInput = player.inputReader.MoveValue.x;
        var stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // 점프
        bool isJumpBtnPressed = player.inputReader.JumpPressed;

        if (isJumpBtnPressed && player.IsGrounded())
        {
            if (player.inputReader.MoveValue.y < -0.5f)
            {
                Collider2D dropCol = player.GetDropThroughCollider();
                if (dropCol != null)
                {
                    player.wasSprinting = false;
                    stateMachine.ChangeState(player.DropState); // 밑점프 실행!
                    return;
                }
            }

            player.wasSprinting = false;
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        // [대쉬]
        if (player.inputReader.DashPressed && player.CanDash)
        {
            player.inputReader.DashPressed = false;
            player.wasSprinting = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }

        // 브레이크 간섭, 스프린트일때
        if (!(xInput == 0 && player.isSprinting))
        {
            base.LogicUpdate();
        }

        player.HandleAttackInput();

        // [정지 로직]
        if (xInput == 0)
        {
            // 삭제됨: if (player.inputReader.HeavyAttackHeld...) return; 
            // 이유: 스킬 발동 불가 시 MoveState에 갇히는 버그 방지

            if (player.isSprinting)
            {
                if (!player.wasSprinting)
                {
                    player.wasSprinting = true;
                    player.animator.Play(player.anim_SprintBreak);
                }

                if (stateInfo.IsName(player.anim_SprintBreak))
                {
                    if (stateInfo.normalizedTime < 0.5f) return;
                }
                else { return; }

                player.isSprinting = false;
                player.wasSprinting = false;
            }

            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // [이동 스프린트 유지 로직]
        player.wasSprinting = false;

        if (player.isSprinting)
        {
            if (!stateInfo.IsName(player.anim_SprintStart) && !stateInfo.IsName(player.anim_SprintIng))
            {
                player.animator.Play(player.anim_SprintIng);
            }

            if (stateInfo.IsName(player.anim_SprintStart) && stateInfo.normalizedTime >= 1.0f)
            {
                player.animator.Play(player.anim_SprintIng);
            }
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        float xInput = player.inputReader.MoveValue.x;
        bool isSlope = player.OnSlope();

        // 스프린트 각도 보정 유지
        if (isSlope && player.isSprinting)
        {
            float angle = Vector2.Angle(Vector2.up, player.slopeHit.normal);
            if (angle < 15f) isSlope = false;
        }

        if (xInput != 0) player.FlipController(xInput);

        float currentSpeed;
        if (player.isSprinting)
        {
            if (isSlope && xInput != 0)
            {
                // 비탈길에서 움직이는 중이라면: 위로 가는지 아래로 가는지 체크
                Vector2 moveDir = new Vector2(xInput, 0f);
                Vector2 dir = player.GetSlopeMoveDirection(moveDir);

                if (dir.y > 0)
                {
                    currentSpeed = 8.4f; // 오르막 제한 속도
                }
                else
                {
                    currentSpeed = player.sprintSpeed;
                }
            }
            else
            {
                currentSpeed = player.sprintSpeed;
            }
        }
        else
        {
            currentSpeed = player.moveSpeed; // 일반 이동 (4.2f)
        }

        if (isSlope)
        {
            player.rb.gravityScale = 0f;

            if (xInput != 0)
            {
                Vector2 moveDir = new Vector2(xInput, 0f);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

                // 모서리 직전 정점(Crest) 감지 및 Y축 벡터 강제 고정
                // 비탈길 콜라이더의 상단 끝(bounds.max.y)까지의 거리를 계산
                float slopeTopY = player.slopeHit.collider.bounds.max.y;
                float playerBottomY = player.cd.bounds.min.y;

                // 발밑이 정점 근처(0.2f 이내)에 도달했다면 위로 미는 힘을 강제로 죽임
                if (playerBottomY >= slopeTopY - 0.2f && slopeMoveDir.y > 0)
                {
                    if (player.isSprinting)
                    {


                        slopeMoveDir.y *= 0.3f;
                    }
                    else
                    {
                        // [걷기 상태]
                        // 기존처럼 Y축 힘을 0으로 만들어 올라타지는 것 방지
                        slopeMoveDir.y = 0;
                    }
                }

                float finalVelX = slopeMoveDir.x * currentSpeed;
                float finalVelY = slopeMoveDir.y * currentSpeed;

                // 내리막길을 타고 내려가는 중(Y가 음수)이고, 전력질주(스프린트) 중일 때
                if (slopeMoveDir.y < 0 && player.isSprinting)
                {
                    float hitY = player.slopeHit.point.y;
                    float gap = playerBottomY - hitY; // 발바닥과 경사면 표면 사이의 미세한 틈새

                    // 빠른 관성 때문에 허공에 미세하게(0.05f 이상) 붕 떠버렸다면?
                    if (gap > 0.05f)
                    {
                        // 바닥에 곧바로 착지하도록 순식간에 내리누르는 하강 속도(-15f) 추가!
                        finalVelY -= 15f;
                    }
                }

                player.SetVelocity(slopeMoveDir.x * currentSpeed, slopeMoveDir.y * currentSpeed);
            }
            else
            {
                // 정지 시
                bool isHovering = player.slopeHit.collider != null && player.slopeHit.distance > 0.05f;
                if (!isHovering) player.SetVelocity(0f, 0f);
                else { player.rb.gravityScale = 1f; player.SetVelocity(0f, player.rb.linearVelocity.y); }
            }
        }
        else
        {
            // [평지 모드]
            player.rb.gravityScale = 1f;

            // 비탈길 이탈 시점의 Y속도 보정
            // 위로 튀려는 속도가 남아있다면 즉시 0으로 깎아서 툭 떨어지는 느낌 제거
            float velY = player.rb.linearVelocity.y;
            if (velY > 0.1f) velY = 0f;

            player.SetVelocity(xInput * currentSpeed, velY);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용
    }
}