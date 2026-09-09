using UnityEngine;
using UnityEngine.Windows;

public class PlayerWallJumpState : PlayerState
{
    private float wallJumpDir;

    public PlayerWallJumpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        // base.Enter(); 금지!
        stateTimer = 0f;
        player.UseJump();
        // 2단 점프가 있다면 여기서 횟수(JumpCount)를 리셋해 주면 좋습니다.
        //player.RestJumpCount();

        // 애니메이션 강제 재생
        player.animator.Play(player.anim_WallJump, 0, 0f);

        // 바라보던 반대 방향 구하기
        float wallJumpDir = player.isFacingRight ? 1f : -1f;

        // 튕겨나가는 방향으로 몸 돌리기
        player.FlipController(wallJumpDir);

        // 대각선으로 점프 (Vector2로 변경)
        player.rb.linearVelocity = new Vector2(wallJumpDir * player.wallJumpForce.x,
        player.wallJumpForce.y);
    }

    public override void Exit()
    {
        base.Exit();

        float xInput = player.inputReader.MoveValue.x;
        float currentX = player.rb.linearVelocity.x;

        // [핵심] 벽 점프 관성과 유저 입력 방향이 반대라면?
        // 관성을 죽이고 유저가 누르는 방향으로 속도를 즉시 전환할 준비를 합니다.
        if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentX))
        {
            // 관성을 절반 이하로 깎거나 일반 이동 속도로 덮어버림
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // ★ 컨트롤 씹힘 장벽! (이게 있어야 쫀득하게 튕깁니다)
        if (stateTimer < 0.15f)
        {
            return;
        }

        // 정점 찍고 떨어지거나 시간 지나면 다시 조작 가능하게 공중으로 넘김
        if (player.rb.linearVelocity.y <= 0f || stateTimer > 0.2f)
        {
            stateMachine.ChangeState(player.AirState);
        }
    }
}