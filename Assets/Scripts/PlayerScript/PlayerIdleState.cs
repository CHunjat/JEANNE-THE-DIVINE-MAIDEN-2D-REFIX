using Unity.VisualScripting;
using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        //base.Enter();
        stateTimer = 0f;
        player.isSprinting = false;
        player.SetVelocity(0f, player.rb.linearVelocity.y);

        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용
            player.SetVelocity(0f, 0f);   // 하강 관성까지 여기서 사살
        }
        else
        {
            player.SetVelocity(0f, player.rb.linearVelocity.y);
        }


        if (player.wasSprinting)
        {
            player.animator.Play(player.anim_SprintBreak);
            player.wasSprinting = false; // 신호 초기화
        }
        else
        {
            player.animator.CrossFade(animHash, 0.1f); // 기본 대기 모션
        }
    }

    public override void LogicUpdate()
    {

        player.HandleGuardInput();
        if (stateMachine.CurrentState == player.GuardState) return;

        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return; // 그래플로 넘어갔다면 아래 로직 스킵

        base.LogicUpdate();
        player.HandleAttackInput();
        float xInput = player.inputReader.MoveValue.x;

        //idle 시 갑자기 공중으로 떨어졌을때 air 스테이트로 넘겨주는 코드
        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
            return;
        }


        if (player.inputReader.DashPressed && player.CanDash) // 쿨타임 확인 추가
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }
        if (xInput != 0)
        {
            stateMachine.ChangeState(player.MoveState);
            return; // ★ MoveState로 넘어가면 여기서 즉시 끊어줘야 안전합니다!
        }

        // ★ [점프 로직 완벽 통합] (과거 점프 코드 삭제됨)
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

            // 조건 미달 시 일반 점프
            player.wasSprinting = false;
            stateMachine.ChangeState(player.JumpState);
            return;
        }


    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        bool isHovering = player.slopeHit.collider != null && player.slopeHit.distance > 0.05f;
        // 🔥 비탈길 미끄러짐 방지 로직
        if (player.OnSlope() || player.IsOnStairs() && isHovering)
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 차단
            player.SetVelocity(0f, 0f);   // 완전 정지
        }
        else
        {
            player.rb.gravityScale = 1f; // 2D gravityScale 복구
        }
    }

    // 🔥 나갈 때 중력 원복!
    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 복구
    }
}