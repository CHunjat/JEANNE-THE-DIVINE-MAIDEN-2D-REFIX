using UnityEngine;

public class PlayerWallJumpState : PlayerState
{
    private float towardWallDir; // 벽 쪽
    private float jumpAwayDir;   // 튕기는 쪽 (벽 반대)
    private bool kickAway;       // 반대키로 강하게 이탈했는지

    public PlayerWallJumpState(PlayerController player, PlayerStateMachine stateMachine, string animName)
        : base(player, stateMachine, animName) { }

    public override void Enter()
    {
        // base.Enter(); 금지!
        stateTimer = 0f;
        player.UseJump();

        player.animator.Play(player.anim_WallJump, 0, 0f);

        // WallSlide와 동일하게 벽 방향 확정
        if (player.IsTouchingWall(1f)) towardWallDir = 1f;
        else if (player.IsTouchingWall(-1f)) towardWallDir = -1f;
        else towardWallDir = player.isFacingRight ? -1f : 1f; // 등지고 있으므로 벽은 뒤

        jumpAwayDir = -towardWallDir;

        float xInput = player.inputReader.MoveValue.x;
        // 반대 방향키 = 벽에서 멀어지는 쪽
        kickAway = Mathf.Abs(xInput) > 0.1f && Mathf.Sign(xInput) == Mathf.Sign(jumpAwayDir);

        player.FlipController(jumpAwayDir);

        if (kickAway)
        {
            // 반대키 → 강하게 이탈
            player.rb.linearVelocity = new Vector2(
                jumpAwayDir * player.wallJumpForce.x,
                player.wallJumpForce.y);
        }
        else
        {
            // 벽 쪽 키 유지 → 약하게만 떨어짐 (다시 붙기 쉬움)
            player.rb.linearVelocity = new Vector2(
                jumpAwayDir * player.wallJumpForce.x * 0.35f,
                player.wallJumpForce.y);
        }
    }

    public override void Exit()
    {
        base.Exit();

        float xInput = player.inputReader.MoveValue.x;
        float currentX = player.rb.linearVelocity.x;

        if (xInput != 0 && Mathf.Sign(xInput) != Mathf.Sign(currentX))
        {
            player.SetVelocity(xInput * player.moveSpeed, player.rb.linearVelocity.y);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (stateTimer < 0.15f)
            return;

        float xInput = player.inputReader.MoveValue.x;

        // 록맨X: 벽 쪽 키 유지 + 다시 벽 접촉 → 바로 슬라이드
        if (!kickAway
            && Mathf.Abs(xInput) > 0.1f
            && Mathf.Sign(xInput) == Mathf.Sign(towardWallDir)
            && player.IsTouchingWall(towardWallDir))
        {
            stateMachine.ChangeState(player.WallSlideState);
            return;
        }

        if (player.rb.linearVelocity.y <= 0f || stateTimer > 0.2f)
        {
            stateMachine.ChangeState(player.AirState);
        }
    }
}