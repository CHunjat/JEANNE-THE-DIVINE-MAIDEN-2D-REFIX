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
        player.HandleGuardInput();
        if (stateMachine.CurrentState == player.GuardState) return;

        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return;

        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
            return;
        }

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
            if (player.isSprinting)
            {
                if (!player.wasSprinting)
                {
                    player.wasSprinting = true;
                    player.animator.Play(player.anim_SprintBreak);
                }

                if (stateInfo.IsName(player.anim_SprintBreak))
                {
                    if (stateInfo.normalizedTime < 0.98f) return;
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

        //비탈길 bool 판정
        bool isSlope = player.OnSlope();

        //비탈길 스프린트 각도가 15도 이하이면 평지로 취급해서 비탈길에서 스프린트 타고 평지로 내려와도 계속 비탈길 재진입되는거 방지
        if (isSlope && player.isSprinting)
        {
            float angle = Vector2.Angle(Vector2.up, player.slopeHit.normal);
            if (angle < 15f) isSlope = false;
        }


        // 이동 방향 뒤집기
        if (xInput != 0) player.FlipController(xInput);

        float currentSpeed = player.isSprinting ? player.sprintSpeed : player.moveSpeed;

        // 1. 비탈길 판정

        if (isSlope)
        {
            // [비탈길 모드 진입]
            player.rb.gravityScale = 0f;

            // [핵심]움직이는 중이라면 경사면 이동, 멈춰있다면(xInput == 0) 속도 0으로 고정해서 슬라이딩 방지!
            if (xInput != 0)
            {
                Vector2 moveDir = new Vector2(xInput, 0f);
                Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);
                player.SetVelocity(slopeMoveDir.x * currentSpeed, slopeMoveDir.y * currentSpeed);
            }
            else
            {
                bool isHovering = player.slopeHit.collider != null && player.slopeHit.distance > 0.05f;
                if (!isHovering)
                {
                    // 완전히 땅에 닿았을 때만 속도 고정
                    player.SetVelocity(0f, 0f);
                }
                else
                {
                    // 허공이면 중력을 켜서 떨어지게 함
                    player.rb.gravityScale = 1f;
                    player.SetVelocity(0f, player.rb.linearVelocity.y);
                }
            }
        }
        else
        {
            // [평지 모드 진입]
            player.rb.gravityScale = 1f;

            // 관성 유지 (내리막에서 얻은 가속도가 있다면 그대로 평지 질주)
            player.SetVelocity(xInput * currentSpeed, player.rb.linearVelocity.y);
            float velY = player.rb.linearVelocity.y;
            if (velY > 0f)
            {
                velY = 0f;
            }

            player.SetVelocity(xInput * currentSpeed, velY);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용
    }
}