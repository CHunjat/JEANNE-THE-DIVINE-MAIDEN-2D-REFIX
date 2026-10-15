using UnityEngine;

public class PlayerWallSlideState : PlayerState
{
    private float wallDir; // 벽 방향 기억하기

    private float wallDetachTimer;
    private const float WALL_DETACH_BUFFER = 1.0f; // 1초 대기

    // 🔥 [추가] 현재 미끄러지는 중인지 상태를 기억하는 스위치
    private bool isSlidingDown;

    public PlayerWallSlideState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        // base.Enter(); 절대 사용 금지 (기본 애니 실행 방지)
        stateTimer = 0f;
        player.isSprinting = false;

        player.rb.gravityScale = 0f;
        player.SetVelocity(0f, 0f);

        // 1. 벽 방향 실시간 확정
        if (player.IsTouchingWall(1f)) wallDir = 1f;
        else if (player.IsTouchingWall(-1f)) wallDir = -1f;
        else wallDir = player.inputReader.MoveValue.x > 0 ? 1f : -1f;

        // 2. 캐릭터가 무조건 벽의 '반대편'을 바라보도록 강제 회전
        player.FlipController(-wallDir);

        // 타이머 초기화
        wallDetachTimer = WALL_DETACH_BUFFER;

        // 🔥 처음 벽에 붙었을 때는 무조건 정지(매달리기) 상태!
        isSlidingDown = false;
        player.animator.Play(player.anim_ToWallGrab, 0, 0f); // 매달리기 애니 재생
    }

    public override void Exit()
    {
        base.Exit();

        player.rb.gravityScale = player.defaultGravityScale;
    }

    public override void LogicUpdate()
    {
        player.FlipController(-wallDir);
        base.LogicUpdate();

        if (player.inputReader.JumpPressed)
        {
            player.inputReader.JumpPressed = false;

            float jInput = player.inputReader.MoveValue.x;

            // 중립 → 일반 점프
            if (Mathf.Abs(jInput) < 0.1f)
            {
                player.RestJumpCount(); // 벽 중립 -> 2단가능
                player.SetVelocity(-wallDir * player.wallJumpForce.x * 0.2f, player.rb.linearVelocity.y);
                stateMachine.ChangeState(player.JumpState);
                return;
            }

            // 벽 쪽 / 반대 쪽 → WallJump (안에서 힘만 구분)
            stateMachine.ChangeState(player.WallJumpState);
            return;
        }

        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        if (!player.IsTouchingWall(wallDir))
        {
            stateMachine.ChangeState(player.AirState);
            return;
        }

        float xInput = player.inputReader.MoveValue.x;
        float yInput = player.inputReader.MoveValue.y;

       
        // ==========================================
        if (yInput < -0.1f)
        {
            // 미끄러지기 시작할 때 "한 번만" 슬라이드 애니메이션 틀기
            if (!isSlidingDown)
            {
                isSlidingDown = true;
                player.animator.Play(player.anim_WallSlide);
            }
        }
        else
        {
            // 멈출 때 "한 번만" 매달리기 애니메이션 틀기
            if (isSlidingDown)
            {
                isSlidingDown = false;
                player.animator.Play(player.anim_ToWallGrab);
            }
        }

        // ==========================================
        // [요구사항 5-1] 반대 방향키 입력 시 1초 버퍼 적용 후 떨어짐
        // ==========================================
        if (Mathf.Abs(xInput) > 0.1f && Mathf.Sign(xInput) != wallDir)
        {
            wallDetachTimer -= Time.deltaTime;

            if (wallDetachTimer <= 0f)
            {
                stateMachine.ChangeState(player.AirState);
            }
        }
        else
        {
            wallDetachTimer = WALL_DETACH_BUFFER;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        float yInput = player.inputReader.MoveValue.y;
        float targetY = (yInput < -0.1f) ? -player.wallSlideSpeed : 0f;

        // X축 속도를 0으로 멈추지 말고, 벽 방향(wallDir)으로 속도를 계속 줍니다
        player.SetVelocity(wallDir * 2f, targetY);
    }
}