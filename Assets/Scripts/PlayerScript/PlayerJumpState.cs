using UnityEngine;
using UnityEngine.Windows;

public class PlayerJumpState : PlayerState
{
    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        // 1. 스프린트 점프 판별 (지상에서 스프린트 중일 때 첫 점프만 해당)
        // 이미 공중이거나 점프 카운트를 소모했다면 스프린트 점프 로직을 타지 않게 합니다.
        bool isFirstSprintJump = player.isSprinting && player.IsGrounded();

        if (isFirstSprintJump)
        {
            if (!player.CanSprintJump)
            {
                player.inputReader.JumpPressed = false;
                player.SetVelocity(player.rb.linearVelocity.x, 0f);
                player.isJumpCut = true;
                stateMachine.ChangeState(player.MoveState);
                return;
            }
            player.ResetSprintJumpCooldown();
        }

        stateTimer = 0f;
        player.wallGrabTimer = player.wallGrabCooldown;
        float finalJumpForce = player.jumpForce;
        float xInput = player.inputReader.MoveValue.x;

        // 2. 비탈길 점프 로직
        if (player.OnSlope())
        {
            float moveDirX = xInput != 0 ? Mathf.Sign(xInput) : (player.isFacingRight ? 1f : -1f);
            Vector2 slopeMoveDir = player.GetSlopeMoveDirection(new Vector2(moveDirX, 0)); // Vector2로 변경

            if (slopeMoveDir.y > 0)
            {
                // 첫 점프가 스프린트일 때만 강한 부스트, 2단 점프는 일반 부스트
                finalJumpForce += isFirstSprintJump ? 3.5f : 0.5f;
            }
            player.transform.position += (Vector3)Vector2.up * 0.05f; // Vector2.up 사용
        }

        // 3. 점프 실행 및 애니메이션 처리
        player.UseJump();

        if (isFirstSprintJump)
        {
            player.animator.Play(player.anim_SprintJump);
        }
        else
        {
            // 2단 점프 혹은 일반 점프
            player.animator.CrossFade(animHash, 0.1f);

            // 🔥 중요: 2단 점프를 뛰는 순간 더 이상 스프린트 상태가 아니라고 판단하게 함
            // 그래야 LogicUpdate의 2단 점프 조건이나 애니메이션이 꼬이지 않습니다.
            player.isSprinting = false;
        }
        //관성점프
        if (Mathf.Abs(xInput) > 0.1f)
        {
            // 방향키를 누르고 있다면: 현재 x속도(대시 관성 포함)를 그대로 유지하며 점프!
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, finalJumpForce); // Vector2로 변경
        }
        else
        {
            // 방향키를 떼고 있다면: 관성을 끊고 제자리 수직 점프로 전환!
            player.rb.linearVelocity = new Vector2(0f, finalJumpForce); // Vector2로 변경
        }

        // 최종 속도 적용
        player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, finalJumpForce); // Vector2로 변경
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.HandleAttackInput(); //떨어질때도 공중공격 가능

        player.HandleGrappleInput();
        if (stateMachine.CurrentState == player.GrappleState) return; // 그래플로 넘어갔다면 아래 로직 스킵

        // 4. 2단 점프 입력 체크 (이미 코드가 잘 들어가 있습니다)
        if (player.inputReader.JumpPressed && player.CanJump)
        {
            player.inputReader.JumpPressed = false;
            stateMachine.ChangeState(player.JumpState); // 재진입
            return;
        }

        // 대쉬 및 상태 전환 로직 (기존 유지)
        if (player.inputReader.DashPressed && player.CanDash)
        {
            player.inputReader.DashPressed = false;
            stateMachine.ChangeState(player.DashState);
            return;
        }

        if (stateTimer > 0.05f && player.IsGrounded())
        {
            if (player.rb.linearVelocity.y < 0.1f || player.OnSlope())
            {
                stateMachine.ChangeState(player.AirState);
                return;
            }
        }

        if (player.rb.linearVelocity.y < -0.1f)
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float xInput = player.inputReader.MoveValue.x;
        float currentXVelocity = player.rb.linearVelocity.x;

        // 1. 공중 방향 전환 (역방향 입력 시 즉시 꺾기)
        if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentXVelocity))
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
        // 🔥 [핵심 수정] 2. 방향키를 뗐을 때 (xInput == 0) 관성 죽이기
        else if (Mathf.Abs(xInput) < 0.1f)
        {
            // 방향키를 떼면 현재 속도가 얼마든 moveSpeed보다 낮아질 때까지 빠르게 감속
            // 10f 수치를 높일수록 더 팍! 하고 멈춥니다.
            float stoppingSpeed = Mathf.Lerp(currentXVelocity, 0f, Time.fixedDeltaTime * 10f); // FixedUpdate 기반이므로 fixedDeltaTime
            player.SetVelocity(stoppingSpeed, player.rb.linearVelocity.y);
        }
        // 3. 방향키를 계속 누르고 있을 때 (대시 관성 유지 및 자연스러운 감속)
        else if (Mathf.Abs(currentXVelocity) > player.moveSpeed)
        {
            float targetX = xInput * player.moveSpeed;
            // player.airDeceleration 수치에 따라 서서히 일반 이동 속도로 돌아옵니다.
            float lerpedX = Mathf.Lerp(currentXVelocity, targetX, Time.fixedDeltaTime * player.airDeceleration); // FixedUpdate 기반
            player.SetVelocity(lerpedX, player.rb.linearVelocity.y);
        }
        // 4. 일반 공중 이동
        else
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }

        // 캐릭터 머리 방향 전환
        if (xInput != 0) player.FlipController(xInput);
    }
}