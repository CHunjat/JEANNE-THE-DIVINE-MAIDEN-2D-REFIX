using UnityEngine;

public class PlayerWallSlideState : PlayerState
{
    private float wallDir; // 벽 방향 기억하기


    public PlayerWallSlideState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        stateTimer = 0f;
        player.isSprinting = false;
        player.SetVelocity(0f, player.rb.linearVelocity.y);

        // 1. 벽 방향 실시간 확정
        if (player.IsTouchingWall(1f)) wallDir = 1f;
        else if (player.IsTouchingWall(-1f)) wallDir = -1f;
        else wallDir = player.inputReader.MoveValue.x > 0 ? 1f : -1f;

        // 2. [수정] 몸 돌리기 로직 강화
        // 단순히 -wallDir을 넣는 게 아니라, 
        // 캐릭터가 무조건 벽의 '반대편'을 바라보도록 강제로 회전값을 꽂아넣어야 합니다.
        // wallDir이 -1(왼쪽)이면 FlipController(1) -> 오른쪽 보기
        // wallDir이 1(오른쪽)이면 FlipController(-1) -> 왼쪽 보기
        player.FlipController(-wallDir);

        // ★ [추가] 만약 FlipController가 제대로 안 먹는다면 강제로 rotation을 조절
        // float targetY = (wallDir == -1f) ? 0f : 180f; // 왼쪽 벽이면 오른쪽(0도), 오른쪽 벽이면 왼쪽(180도)
        // player.transform.rotation = Quaternion.Euler(0, targetY, 0);

        player.animator.Play(player.anim_WallSlide, 0, 0f);
    }

    public override void Exit()
    {
        base.Exit();

        // 벽을 빠져나가는 순간, 벽 쪽으로 밀고 있던 수평 속도를 0으로 초기화합니다.
        // 이걸 안 하면 땅에 닿았을 때 미세하게 옆으로 밀립니다.
        player.SetVelocity(0f, player.rb.linearVelocity.y);


    }

    public override void LogicUpdate()
    {
        player.FlipController(-wallDir);
        base.LogicUpdate();

        // 1. 점프 키 누르면 벽 점프!
        if (player.inputReader.JumpPressed)
        {
            player.inputReader.JumpPressed = false;
            stateMachine.ChangeState(player.WallJumpState);
            return;
        }

        // 2. 바닥에 닿으면 착지
        if (player.IsGrounded())
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        float xInput = player.inputReader.MoveValue.x;

        // 3. [해결 완료] 키 떼면 떨어지기
        // xInput이 wallDir과 다르면 무조건 추락합니다.
        // 즉, 방향키에서 손을 떼서 0이 되거나, 반대 방향을 누르면 바로 떨어집니다!

        //==추가==

        //변경된 부분: xInput != wallDir 대신에 곱하기 방식을 씁니다.
        // xInput(0.707) * wallDir(1) = 0.707 (0.1보다 크니까 안 떨어짐)
        // 만약 손을 떼면 0 * 1 = 0 (0.1보다 작으니 추락)
        // 반대 방향 누르면 -1 * 1 = -1 (0.1보다 작으니 추락)
        if (!player.IsTouchingWall(wallDir) || (xInput * wallDir) < 0.1f)
        {
            stateMachine.ChangeState(player.AirState);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
        // X축 속도를 0으로 멈추지 말고, 벽 방향(wallDir)으로 속도를 계속 줍니다! (예: 2f)
        // ZeroFriction 머티리얼 덕분에 끈적이지 않고, 벽에 빈틈없이 100% 딱 붙어서 긁고 내려옵니다.
        player.SetVelocity(wallDir * 2f, -player.wallSlideSpeed);
    }
}