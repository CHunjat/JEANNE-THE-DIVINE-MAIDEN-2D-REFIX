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


        //점프
        if (player.inputReader.JumpPressed && player.IsGrounded())
        {
            player.inputReader.JumpPressed = false; // 입력 초기화
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (player.inputReader.MoveValue.x != 0)
        {
            stateMachine.ChangeState(player.MoveState);
        }


    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // 🔥 비탈길 미끄러짐 방지 로직
        if (player.OnSlope() || player.IsOnStairs())
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