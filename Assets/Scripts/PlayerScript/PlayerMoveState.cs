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
            player.ToggleStairsCollision(false);
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
        if (stateMachine.CurrentState == player.GrappleState) return; // 그래플로 넘어갔다면 아래 로직 스킵

        if (!player.IsGrounded())
        {
            stateMachine.ChangeState(player.AirState);
            return;
        }
        // [1] 입력을 미리 받아둠
        float xInput = player.inputReader.MoveValue.x;
        var stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // ★ [핵심 1] 점프와 대쉬를 로직 최상단으로 이동!
        // 이렇게 해야 브레이크 애니메이션이 재생 중(return)이라도 점프가 씹히지 않습니다.
        if (player.inputReader.JumpPressed && player.IsGrounded())
        {
            player.inputReader.JumpPressed = false;
            player.wasSprinting = false; // 플래그 리셋
            // isSprinting을 여기서 끄지 않아야 공중에서도 스프린트 상태가 유지되어 '스프린트 점프'가 됩니다.
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (player.inputReader.DashPressed && player.CanDash)
        {
            player.inputReader.DashPressed = false;
            player.wasSprinting = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }


        // [2] 브레이크 중에는 부모 클래스의 기본 애니메이션 간섭 차단
        if (!(xInput == 0 && player.isSprinting))
        {
            base.LogicUpdate();
        }


        player.HandleAttackInput();


        // [3] 정지 로직 (x == 0)
        if (xInput == 0)
        {
            if (player.isSprinting)
            {
                if (!player.wasSprinting)
                {
                    player.wasSprinting = true;
                    player.animator.Play(player.anim_SprintBreak);
                }

                // 애니메이션이 98% 끝날 때까지 여기서 가둬둠
                if (stateInfo.IsName(player.anim_SprintBreak))
                {
                    if (stateInfo.normalizedTime < 0.98f) return;
                }
                else { return; }

                // 브레이크가 완전히 끝난 후에만 상태 해제
                player.isSprinting = false;
                player.wasSprinting = false;
            }

            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // [4] 이동 및 스프린트 유지 로직 (x != 0)
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
        if (xInput != 0)
        {
            player.FlipController(xInput);
        }

        float currentSpeed = player.isSprinting ? player.sprintSpeed : player.moveSpeed;
        // 비탈길 오르내리기 보정 로직
        if (player.OnSlope())
        {
            player.rb.gravityScale = 0f; // 2D gravityScale 사용 (덜덜거림 방지)

            // xInput을 경사면 방향으로 변환
            Vector2 moveDir = new Vector2(xInput, 0f); // Vector2로 전환
            Vector2 slopeMoveDir = player.GetSlopeMoveDirection(moveDir);

            player.SetVelocity(slopeMoveDir.x * currentSpeed, slopeMoveDir.y * currentSpeed);
        }
        else
        {
            // 평지 걷기
            player.rb.gravityScale = 1f; // 2D gravityScale 사용
            player.SetVelocity(xInput * currentSpeed, player.rb.linearVelocity.y);
        }
    }

    public override void Exit()
    {
        base.Exit();
        player.rb.gravityScale = 1f; // 2D gravityScale 사용
    }
}